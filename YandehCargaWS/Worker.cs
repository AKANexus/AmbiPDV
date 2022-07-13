using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using FirebirdSql.Data.FirebirdClient;
using YandehCargaWS.Yandeh;

namespace YandehCargaWS
{
    public class Worker : BackgroundService
    {

        private bool _isRunning = false;
        private Progress<string> _progress;
        private FbConnection _fbConnection;
        private FbCommand _fbCommand;
        private DataTable _fbData;
        private bool _tablesChecked = false;
        private bool _firstRun = false;
        private string _cnpj;
        private string _authKey;
        private SemaphoreSlim _farol = new(0, 1);
        private readonly System.Timers.Timer _intervalTimer = new();
        private bool _isDebugEnabled = false;
        private string dbPath;

        private readonly ILogger<Worker> _logger;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }


        private async Task<bool> ObtemAuthKey()
        {
            DataTable emitenteTable = new();
            if (_fbCommand.Connection is null) _fbCommand.Connection = _fbConnection;
            _fbCommand.CommandText = "SELECT * FROM TB_EMITENTE";
            try
            {
                emitenteTable.Load(await _fbCommand.ExecuteReaderAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            if (emitenteTable.Rows.Count < 1 || string.IsNullOrWhiteSpace((string)emitenteTable.Rows[0]["CNPJ"]))
            {
                EventLog.WriteEntry("YandehCarga", "Falha ao obter o CNPJ do Emitente");
                return false;
            }

            _cnpj = ((string)emitenteTable.Rows[0]["CNPJ"]).TiraPont();
            var response = await YandehAPI.CadastraAPIKey(_cnpj);
            if (response.Item1)
            {
                _authKey = response.Item2;
                return true;
            }
            return false;
        }

        private async Task<bool> CriaArquivoPath()
        {
            try
            {
                if (!Directory.Exists(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS")))
                {
                    EventLog.WriteEntry("YandehCarga", $"Criando pasta em {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS")}");

                    Directory.CreateDirectory(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS"));
                }
                if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS", "path.txt")))
                {
                    EventLog.WriteEntry("YandehCarga", $"Criando arquivo em {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS", "path.txt")}");

                    File.Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS", "path.txt")).Dispose();
                    return false;
                }

                //File.WriteAllText("path.txt", @"localhost|C:\Path\Clipp.FDB");
                return true;
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("YandehCarga", $"Falha ao gravar o caminho.\n{e.Message}");
                return false;

            }
        }

        private async Task<bool> CheckTablesAndTriggers()
        {
            _fbCommand.Connection = _fbConnection;
            _fbCommand.CommandText = "SELECT COUNT(1) FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'YANDEH_PRODUTO'";
            _fbCommand.CommandType = CommandType.Text;
            var triggerCount = await _fbCommand.ExecuteScalarAsync();
            if (triggerCount is not int)
            {
                EventLog.WriteEntry("YandehCarga", "Falha ao obter o status das tabelas Yandeh");
                return false;
            }
            else
            {
                if ((int)triggerCount == 0)
                {
                    try
                    {
                        await CreateTriggersAndTables();
                    }
                    catch (Exception e)
                    {
                        EventLog.WriteEntry("YandehCarga", "Falha ao criar as tabelas");
                        EventLog.WriteEntry("YandehCarga", e.Message);
                        
                        return false;
                    }
                }
            }
            _tablesChecked = true;
            return true;
        }

        private async Task CreateTriggersAndTables()
        {
            _firstRun = true;
            _fbCommand.CommandText = "CREATE TABLE YAN_SYNC (TABLE_NAME VARCHAR(100) NOT NULL, RESPECTIVE_ID INTEGER NOT NULL);";
            await _fbCommand.ExecuteNonQueryAsync();
            _fbCommand.CommandText =
                "CREATE TRIGGER YANDEH_PRODUTO FOR TB_ESTOQUE AFTER INSERT OR UPDATE AS BEGIN INSERT INTO YAN_SYNC (TABLE_NAME, RESPECTIVE_ID) VALUES('ESTOQUE', NEW.ID_ESTOQUE); END";
            await _fbCommand.ExecuteNonQueryAsync();
            _fbCommand.CommandText =
                "CREATE TRIGGER YANDEH_SELLIN FOR TB_NFCOMPRA AFTER INSERT OR UPDATE AS BEGIN INSERT INTO YAN_SYNC (TABLE_NAME, RESPECTIVE_ID) VALUES('COMPRA', NEW.ID_NFCOMPRA); END";
            await _fbCommand.ExecuteNonQueryAsync();
            _fbCommand.CommandText =
                "CREATE TRIGGER YANDEH_SELLOUT FOR TB_NFVENDA AFTER INSERT OR UPDATE AS BEGIN INSERT INTO YAN_SYNC (TABLE_NAME, RESPECTIVE_ID) VALUES('VENDA', NEW.ID_NFVENDA); END";
            await _fbCommand.ExecuteNonQueryAsync();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (!await CriaArquivoPath())
            {
                EventLog.WriteEntry("YandehCarga", $"Arquivo Path não existe a e foi criado. Reinicie o serviço.");
                return;
            }

            dbPath = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS", "path.txt"));


            EventLog.WriteEntry("YandehCarga", "Iniciando Carga Yandeh");
            EventLog.WriteEntry("YandehCarga", $"Lendo path.txt em {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YandehServiceWS", "path.txt")}");
            if (string.IsNullOrWhiteSpace(dbPath) || dbPath.Split('|').Length != 2)
            {
                EventLog.WriteEntry("YandehCarga", "Caminho da base de dados inválido. Tente novamente.");
                EventLog.WriteEntry("YandehCarga", $"Caminho fornecido: {dbPath}");
                return;
            }

            var dataSource = dbPath.Split('|')[0];
            var catalog = dbPath.Split('|')[1];
            _fbConnection =
                new(
                    $@"initial catalog={catalog};data source={dataSource};user id=SYSDBA;password=masterke;encoding=WIN1252;charset=utf8");
            _fbCommand = new();
            _fbData = new();
            try
            {
                await _fbConnection.OpenAsync();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("YandehCarga", "Falha ao abrir a conexão com o Clipp.");
                EventLog.WriteEntry("YandehCarga", e.Message);
                return;
            }

            if (!_tablesChecked)
            {
                await CheckTablesAndTriggers();
            }

            if (!await ObtemAuthKey())
            {
                EventLog.WriteEntry("YandehCarga", "Falha ao obter a authKey");

                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                #region COLETA_E_ENVIO
                _fbData.Clear();
                _fbCommand.CommandText = "SELECT * FROM YAN_SYNC";
                try
                {
                    _fbData.Load(await _fbCommand.ExecuteReaderAsync());
                }
                catch (Exception e)
                {
                    EventLog.WriteEntry("YandehCarga", "Falha ao obter os dados a sincronizar");
                    EventLog.WriteEntry("YandehCarga", e.Message);
                    return;
                }



                EventLog.WriteEntry("YandehCarga", $"{_fbData.Rows.Count} entradas para carregar");
                if (_fbData.Rows.Count == 0)
                {
                    EventLog.WriteEntry("YandehCarga", "Não havia entradas para carregar.");
                }
                else
                {
                    try
                    {
                        foreach (DataRow fbDataRow in _fbData.Rows)
                        {
                            EventLog.WriteEntry("YandehCarga", $"Entrada de {fbDataRow["TABLE_NAME"]} sendo processado");
                            switch (fbDataRow["TABLE_NAME"])
                            {
                                #region ESTOQUE

                                case "ESTOQUE":
                                    _fbCommand.CommandText =
                                        $"SELECT * FROM TB_ESTOQUE te JOIN TB_EST_IDENTIFICADOR tei ON te.ID_ESTOQUE = tei.ID_ESTOQUE JOIN TB_EST_PRODUTO tep ON tei.ID_IDENTIFICADOR = tep.ID_IDENTIFICADOR WHERE te.ID_ESTOQUE = {fbDataRow["RESPECTIVE_ID"]};";
                                    var estoqueTable = new DataTable();
                                    estoqueTable.Load(await _fbCommand.ExecuteReaderAsync());
                                    if (estoqueTable.Rows.Count == 0)
                                    {
                                        EventLog.WriteEntry("YandehCarga", "estoqueTable.Count era 0");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        EventLog.WriteEntry("YandehCarga", "Entrada removida.");
                                        continue;
                                    }
                                    if (estoqueTable.Rows[0]["COD_BARRA"] is DBNull)
                                    {
                                        EventLog.WriteEntry("YandehCarga", "COD_BARRAS era nulo");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        EventLog.WriteEntry("YandehCarga", "Entrada removida.");
                                        continue;
                                    }
                                    if (string.IsNullOrWhiteSpace((string)estoqueTable.Rows[0]["COD_BARRA"]))
                                    {
                                        EventLog.WriteEntry("YandehCarga", "COD_BARRA estava em branco");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        EventLog.WriteEntry("YandehCarga", "Entrada removida.");
                                        continue;
                                    }
                                    if (((string)estoqueTable.Rows[0]["COD_BARRA"]).Length < 8)
                                    {
                                        EventLog.WriteEntry("YandehCarga", "COD_BARRA.length < 8");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        EventLog.WriteEntry("YandehCarga", "Entrada removida.");
                                        continue;
                                    }


                                    EventLog.WriteEntry("YandehCarga", $"Carregando Estoque: {(string)estoqueTable.Rows[0]["DESCRICAO"]}");

                                    EstoqueBody body = new()
                                    {
                                        origem_coleta = "API|Ambisoft",
                                        dt_ultima_alt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        code = ((int)estoqueTable.Rows[0]["ID_IDENTIFICADOR"]).ToString(),
                                        product_type = "simple",
                                        sku = (string)estoqueTable.Rows[0]["COD_BARRA"],
                                        name = (string)estoqueTable.Rows[0]["DESCRICAO"],
                                        description = (string)estoqueTable.Rows[0]["DESCRICAO"],
                                        dimension = new() { measurement_unit = (string)estoqueTable.Rows[0]["UNI_MEDIDA"] },
                                        price_info = new Price_Info()
                                        {
                                            price = (decimal)estoqueTable.Rows[0]["PRC_VENDA"]
                                        },
                                        visibility = "T",
                                        status = "A"
                                    };


                                    try
                                    {
                                        var estoqueResponse = await YandehAPI.EnviaEstoque(body);
                                        if (!estoqueResponse.Item1)
                                        {
                                            EventLog.WriteEntry("YandehCarga", $"Erro ao carregar estoque: {estoqueResponse.Item2}");

                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        EventLog.WriteEntry("YandehCarga", "Erro ao carregar estoque:");
                                        EventLog.WriteEntry("YandehCarga", e.Message);
                                        return;
                                    }
                                    break;

                                #endregion ESTOQUE

                                #region VENDA

                                case "VENDA":
                                    try
                                    {
                                        _fbCommand.CommandText =
                                            $"SELECT * FROM V_NFV WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]};";
                                        var vendaTable = new DataTable();
                                        vendaTable.Load(await _fbCommand.ExecuteReaderAsync());
                                        if (vendaTable.Rows.Count == 0)
                                        {
                                            _fbCommand.CommandText =
                                                $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                            await _fbCommand.ExecuteNonQueryAsync();
                                            continue;
                                        }

                                        EventLog.WriteEntry("YandehCarga",
                                            $"Carregando Venda: {vendaTable.Rows[0]["NF_MODELO"]}-{vendaTable.Rows[0]["NF_SERIE"]}-{vendaTable.Rows[0]["NF_NUMERO"]}");

                                        var itensVendaTable = new DataTable();
                                        _fbCommand.CommandText =
                                            $"SELECT * FROM V_NFV_ITEM WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]};";
                                        itensVendaTable.Load(await _fbCommand.ExecuteReaderAsync());

                                        //var pagamentosVendaTable = new DataTable();
                                        //_fbCommand.CommandText = $"SELECT * FROM V_NFVENDA_PAGAMENTOS WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]}";
                                        //pagamentosVendaTable.Load(await _fbCommand.ExecuteReaderAsync());

                                        var satTable = new DataTable();
                                        _fbCommand.CommandText =
                                            $"SELECT * FROM V_SAT WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]}";
                                        satTable.Load(await _fbCommand.ExecuteReaderAsync());

                                        var nfeTable = new DataTable();
                                        _fbCommand.CommandText =
                                            $"SELECT * FROM V_NFE WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]}";
                                        nfeTable.Load(await _fbCommand.ExecuteReaderAsync());


                                        SelloutBody selloutBody = new()
                                        {
                                            origem_coleta = "API|Ambisoft",
                                            id =
                                                $"{vendaTable.Rows[0]["NF_MODELO"]}-{vendaTable.Rows[0]["NF_SERIE"]}-{vendaTable.Rows[0]["NF_NUMERO"]}",
                                            sellout_timestamp =
                                                $"{vendaTable.Rows[0]["DT_SAIDA"]:yyyy-MM-dd}T{(TimeSpan)vendaTable.Rows[0]["HR_SAIDA"]:hh\\:mm\\:ss}",
                                            store_taxpayer_id = _cnpj,
                                            checkout_id = $"{vendaTable.Rows[0]["NF_SERIE"]}",
                                            receipt_number = ((int)vendaTable.Rows[0]["NF_NUMERO"]).ToString(),
                                            receipt_series_number =
                                                Regex.Match((string)vendaTable.Rows[0]["NF_SERIE"], @"\d+").Value,
                                            total = (decimal)vendaTable.Rows[0]["TOT_NF"],
                                            cancellation_flag = "N",
                                            operation = "S",
                                            transaction_type = "V",
                                            ipi = 0m,
                                            sales_discount = 0m,
                                            sales_addition = 0m,
                                            icms = 0m,
                                            frete = 0m,
                                            items = new()
                                        };
                                        if (nfeTable.Rows.Count > 0 && nfeTable.Rows[0]["ID_NFE"] is not DBNull &&
                                            !string.IsNullOrWhiteSpace((string)nfeTable.Rows[0]["ID_NFE"]))
                                        {
                                            selloutBody.nfe_access_key = "NFe" + (string)nfeTable.Rows[0]["ID_NFE"];
                                        }

                                        else if (satTable.Rows.Count > 0 && satTable.Rows[0]["CHAVE"] is not DBNull &&
                                                 !string.IsNullOrWhiteSpace((string)satTable.Rows[0]["CHAVE"]))
                                        {
                                            selloutBody.nfe_access_key = "CFe" + (string)satTable.Rows[0]["CHAVE"];
                                        }

                                        else
                                        {
                                            selloutBody.nfe_access_key = "null";
                                        }

                                        foreach (DataRow dataRow in itensVendaTable.Rows)
                                        {
                                            SelloutItem selloutItem = new SelloutItem
                                            {
                                                code = ((int)dataRow["ID_IDENTIFICADOR"]).ToString(),
                                                sku = dataRow["COD_BARRA"] is DBNull ||
                                                      string.IsNullOrWhiteSpace((string)dataRow["COD_BARRA"])
                                                    ? ((int)dataRow["ID_IDENTIFICADOR"]).ToString()
                                                    : (string)dataRow["COD_BARRA"],
                                                description = (string)dataRow["PRODUTO"],
                                                quantity = (decimal)dataRow["QTD_ITEM"],
                                                unit_value = (decimal)dataRow["VLR_UNIT"],
                                                measurement_unit = (string)dataRow["UNI_MEDIDA"],
                                                cancellation_flag = "N",
                                                cfop = 5102,
                                                item_addition = 0m,
                                                item_discount = (decimal)dataRow["VLR_DESC"],
                                                icms = dataRow["VLR_ICMS"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_ICMS"],
                                                pis = dataRow["VLR_PIS"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_PIS"],
                                                cofins = dataRow["VLR_COFINS"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_COFINS"],
                                                ipi = dataRow["VLR_IPI"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_IPI"],
                                                other_expenses = (decimal)dataRow["VLR_DESPESA"],
                                                icms_st = dataRow["VLR_ST"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_ST"],
                                                fcp_st = 0m,
                                                frete = (decimal)dataRow["VLR_FRETE"]
                                            };
                                            selloutBody.items.Add(selloutItem);
                                        }

                                        if (selloutBody.nfe_access_key.Contains("NFe"))
                                        {
                                            selloutBody.tipo = "nfe";
                                        }
                                        else if (selloutBody.nfe_access_key.Contains("CFe"))
                                        {
                                            selloutBody.tipo = "sat";
                                        }
                                        else
                                        {
                                            selloutBody.tipo = "ger";
                                        }

                                        //selloutBody.payment = new();
                                        //foreach (DataRow pagamentosTableRow in pagamentosVendaTable.Rows)
                                        //{
                                        //    Payment payment = new();
                                        //    payment.method = (string)pagamentosTableRow["DESC_FORMAPAGAMENTO"];
                                        //    payment.condition = "Á vista";
                                        //    payment.installments = new();
                                        //    Installment installment = new();
                                        //    installment.installment_number = 1;
                                        //    installment.amount = (decimal)pagamentosTableRow["VLR_PAGTO"];
                                        //    installment.payment_term = 1;
                                        //    payment.installments.Add(installment);
                                        //    selloutBody.payment.Add(payment);
                                        //}

                                        try
                                        {
                                            await YandehAPI.EnviaSellout(selloutBody);
                                        }
                                        catch (Exception e)
                                        {
                                            EventLog.WriteEntry("YandehCarga", "Erro ao enviar sellout:");
                                            EventLog.WriteEntry("YandehCarga", e.Message);
                                            return;
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        EventLog.WriteEntry("YandehCarga", "Erro ao enviar sellout:");
                                        EventLog.WriteEntry("YandehCarga", e.Message);
                                        return;
                                    }
                                    break;

                                #endregion VENDA

                                case "COMPRA":
                                    try
                                    {
                                        _fbCommand.CommandText =
                                            $"SELECT * FROM V_NFC WHERE ID_NFCOMPRA = {fbDataRow["RESPECTIVE_ID"]}";
                                        var compraTable = new DataTable();
                                        compraTable.Load(await _fbCommand.ExecuteReaderAsync());
                                        if (compraTable.Rows.Count == 0)
                                        {
                                            _fbCommand.CommandText =
                                                $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                            await _fbCommand.ExecuteNonQueryAsync();
                                            EventLog.WriteEntry("YandehCarga", "Entrada removida.");
                                            continue;
                                        }

                                        EventLog.WriteEntry("YandehCarga",
                                            $"Carregando Venda: {compraTable.Rows[0]["NF_MODELO"]}-{compraTable.Rows[0]["NF_SERIE"]}-{compraTable.Rows[0]["NF_NUMERO"]}");


                                        var itensCompraTable = new DataTable();
                                        _fbCommand.CommandText =
                                            $"SELECT * FROM V_NFC_ITEM WHERE ID_NFCOMPRA = {fbDataRow["RESPECTIVE_ID"]};";
                                        itensCompraTable.Load(await _fbCommand.ExecuteReaderAsync());

                                        SellInBody sellinBody = new()
                                        {
                                            id =
                                                $"{compraTable.Rows[0]["NF_MODELO"]}-{compraTable.Rows[0]["NF_SERIE"]}-{compraTable.Rows[0]["NF_NUMERO"]}",
                                            sellin_timestamp =
                                                $"{compraTable.Rows[0]["DT_ENTRADA"]:yyyy-MM-dd}T{(TimeSpan)compraTable.Rows[0]["HR_ENTRADA"]:hh\\:mm\\:ss}",
                                            store_taxpayer_id = _cnpj,
                                            nfe_number = ((int)compraTable.Rows[0]["NF_NUMERO"]),
                                            nfe_series_number = int.Parse((string)compraTable.Rows[0]["NF_SERIE"]),
                                            nfe_access_key = "null",
                                            supplier_taxpayer_id = compraTable.Rows[0]["CNPJ_FORNECEDOR"] is DBNull
                                                ? ""
                                                : ((string)compraTable.Rows[0]["CNPJ_FORNECEDOR"]).TiraPont(),
                                            gross_total = (decimal)compraTable.Rows[0]["TOT_ITEM"],
                                            net_total = (decimal)compraTable.Rows[0]["TOT_NF"],
                                            cancellation_flag = "N",
                                            freight_price = (decimal)compraTable.Rows[0]["TOT_FRETE"],
                                            insurance_price = (decimal)compraTable.Rows[0]["TOT_SEGURO"],
                                            other_expenses = (decimal)compraTable.Rows[0]["TOT_DESPESA"],
                                            origem_coleta = "API|Ambisoft",
                                            ipi = 0m,
                                            sales_discount = 0m,
                                            sales_addition = 0m,
                                            items = new()
                                        };

                                        foreach (DataRow dataRow in itensCompraTable.Rows)
                                        {
                                            SellInItem sellInItem = new()
                                            {
                                                code = ((int)dataRow["ID_IDENTIFICADOR"]).ToString(),
                                                ean = string.IsNullOrWhiteSpace((string)dataRow["COD_BARRA"])
                                                    ? ((int)dataRow["ID_IDENTIFICADOR"]).ToString()
                                                    : (string)dataRow["COD_BARRA"],
                                                description = (string)dataRow["PRODUTO"],
                                                quantity = (decimal)dataRow["QTD_ITEM"],
                                                measurement_unit = (string)dataRow["UNI_MEDIDA"],
                                                unit_value = (decimal)dataRow["VLR_UNIT"],
                                                gross_total = (decimal)dataRow["VLR_TOTAL"],
                                                net_total = (decimal)dataRow["VLR_TOTAL"] -
                                                            (decimal)dataRow["VLR_DESC"],
                                                icms = dataRow["VLR_ICMS"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_ICMS"],
                                                pis = dataRow["VLR_PIS"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_PIS"],
                                                cofins = dataRow["VLR_COFINS"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_COFINS"],
                                                cfop = int.Parse((string)dataRow["CFOP"]),
                                                addition = 0m,
                                                discount = (decimal)dataRow["VLR_DESC"],
                                                ipi = dataRow["VLR_IPI"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_IPI"],
                                                other_expenses = dataRow["VLR_DESPESA"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_DESPESA"],
                                                icms_st = dataRow["VLR_ST"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_ST"],
                                                fcp_st = 0m,
                                                freight_price = dataRow["VLR_FRETE"] is DBNull
                                                    ? 0m
                                                    : (decimal)dataRow["VLR_FRETE"]
                                            };
                                            sellinBody.items.Add(sellInItem);
                                        }

                                        try
                                        {
                                            await YandehAPI.EnviaSellin(sellinBody);

                                        }
                                        catch (Exception e)
                                        {
                                            EventLog.WriteEntry("YandehCarga", "Erro ao enviar sellin:");
                                            EventLog.WriteEntry("YandehCarga", e.Message);
                                            return;
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        EventLog.WriteEntry("YandehCarga", "Erro ao enviar sellin:");
                                        EventLog.WriteEntry("YandehCarga", e.Message);
                                        return;
                                    }

                                    break;
                                default:
                                    EventLog.WriteEntry("YandehCarga", "TABLE_NAME não reconhecido");
                                    break;
                            }

                            EventLog.WriteEntry("YandehCarga", "Carregado com sucesso.");
                            try
                            {
                                _fbCommand.CommandText =
                                    $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                await _fbCommand.ExecuteNonQueryAsync();
                                EventLog.WriteEntry("YandehCarga", "Entrada removida.");
                            }
                            catch (Exception e)
                            {
                                EventLog.WriteEntry("YandehCarga", "Falha ao apagar o registro");
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        EventLog.WriteEntry("YandehCarga", "Falha não identificada...");
                        EventLog.WriteEntry("YandehCarga", e.Message);
                        return;
                    }





                    #endregion COLETA E ENVIO
                }
                EventLog.WriteEntry("YandehCarga", "Carga concluída com sucesso!");
                EventLog.WriteEntry("YandehCarga", $"Próxima carga: {DateTime.Now.AddMinutes(30):HH:mm}");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);

            }
        }
    }
}


/*
 * 
                EventLog.WriteEntry("My source", "MyMessage");
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
 */