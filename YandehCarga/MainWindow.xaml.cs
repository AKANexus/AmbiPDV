using FirebirdSql.Data.FirebirdClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YandehCarga.Yandeh;

namespace YandehCarga
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        private Logger _logger = new Logger("Yandeh");
        private bool _isDebugEnabled = false;
        public MainWindow()
        {
            InitializeComponent();
            Logger.Start(new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Trilha Informatica", $"Logs\\YandehCarga-{DateTime.Today:ddMMyy}.log")));
            Logger.IgnoreDebug = !_isDebugEnabled;
            _intervalTimer.AutoReset = true;
            _intervalTimer.Interval = 60000 * 10;
            _intervalTimer.Elapsed += _intervalTimer_Elapsed;
            ButParar.IsEnabled = false;
            _progress = new(AtualizaTextBox);
            if (File.Exists("path.txt"))
            {
                TxbDBPath.Text = File.ReadAllText("path.txt");
                _logger.Debug($"Caminho da base: {TxbDBPath.Text}.");
            }

            if (Environment.GetCommandLineArgs().Contains("-autostart") && !string.IsNullOrWhiteSpace(TxbDBPath.Text))
            {
                _logger.Debug($"Autostart definido.");
                Start();
            }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            if (_isRunning)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
                return;
            }
            base.OnClosing(e);
        }

        private void _intervalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _farol.Release();
        }

        private void AtualizaTextBox(string message)
        {
            TxbLogs.AppendText(Environment.NewLine + message);
            TxbLogs.CaretIndex = TxbLogs.Text.Length;
            TxbLogs.ScrollToEnd();
        }

        private void StartStop()
        {
            if (_isRunning)
            {
                Stop();
            }
            else
            {
                Start();
            }
        }

        private void Stop()
        {
            _logger.Debug("Stop requisitado");
            TxbDBPath.IsReadOnly = false;
            ButParar.IsEnabled = false;
            ButIniciar.IsEnabled = true;
            _isRunning = false;
            _farol.Release();
        }

        private void Start()
        {
            _logger.Debug("Start requisitado");
            TxbDBPath.IsReadOnly = true;
            ButParar.IsEnabled = true;
            ButIniciar.IsEnabled = false;
            _isRunning = true;
            RunScripts(_progress);
        }

        private async Task RunScripts(IProgress<string> progress)
        {

            progress.Report("Iniciando Carga Yandeh");
            progress.Report("Verificando Caminho da BD...");
            if (string.IsNullOrWhiteSpace(TxbDBPath.Text) || TxbDBPath.Text.Split('|').Length != 2)
            {
                _logger.Warn("Caminho da base informado era inválido:");
                _logger.Warn(TxbDBPath.Text);
                progress.Report("Caminho da base de dados inválido. Tente novamente.");
                progress.Report($"Caminho fornecido: {TxbDBPath.Text}");
                Stop();
                return;
            }

            progress.Report("Sintaxe do caminho é válida. Verificando conexão...");
            var dataSource = TxbDBPath.Text.Split('|')[0];
            var catalog = TxbDBPath.Text.Split('|')[1];
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
                progress.Report("Falha ao abrir a conexão com o Clipp.");
                progress.Report(e.Message);
                Stop();
                return;
            }

            progress.Report("Conexão é válida.");
            if (!await GravaPathNoTxt())
            {
                _logger.Warn("Falha ao gravar path.txt");
                progress.Report("Falha ao gravar path.txt");
            }

            progress.Report("Verificando tabelas...");
            if (!_tablesChecked)
            {
                _logger.Debug("Criando tabelas e triggers");
                await CheckTablesAndTriggers(progress);
            }

            progress.Report("Obtendo authkey");
            if (!await ObtemAuthKey(progress))
            {
                _logger.Error("Falha ao obter a authkey.");
                progress.Report($"Falha ao obter a authkey");
                Stop();
                return;
            }

            while (_isRunning)
            {
                #region COLETA_E_ENVIO
                _fbData.Clear();
                _fbCommand.CommandText = "SELECT * FROM YAN_SYNC";
                progress.Report("Obtendo itens para carregar");
                try
                {
                    _fbData.Load(await _fbCommand.ExecuteReaderAsync());
                }
                catch (Exception e)
                {
                    _logger.Error("Falha ao obter os dados a sincronizar");
                    progress.Report("Falha ao obter os dados a sincronizar");
                    progress.Report(e.Message);
                    Stop();
                    return;
                }



                progress.Report($"{_fbData.Rows.Count} entradas para carregar");
                if (_fbData.Rows.Count == 0)
                {
                    _logger.Debug("Não havia entradas para carregar.");
                    progress.Report("Não havia entradas para carregar.");
                }
                else
                {
                    try
                    {
                        foreach (DataRow fbDataRow in _fbData.Rows)
                        {
                            progress.Report($"Entrada de {fbDataRow["TABLE_NAME"]} sendo processado");
                            _logger.Debug($"Processando Entrada de {fbDataRow["TABLE_NAME"]}");
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
                                        progress.Report("estoqueTable.Count era 0");
                                        _logger.Debug("estoqueTable.Count era 0");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        progress.Report("Entrada removida.");
                                        continue;
                                    }
                                    if (estoqueTable.Rows[0]["COD_BARRA"] is DBNull)
                                    {
                                        progress.Report("COD_BARRAS era nulo");
                                        _logger.Debug("COD_BARRAS era nulo");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        progress.Report("Entrada removida.");
                                        continue;
                                    }
                                    if (string.IsNullOrWhiteSpace((string)estoqueTable.Rows[0]["COD_BARRA"]))
                                    {
                                        progress.Report("COD_BARRA estava em branco");
                                        _logger.Debug("COD_BARRA estava em branco");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        progress.Report("Entrada removida.");
                                        continue;
                                    }
                                    if (((string)estoqueTable.Rows[0]["COD_BARRA"]).Length < 8)
                                    {
                                        progress.Report("COD_BARRA.length < 8");
                                        _logger.Debug("COD_BARRA.length < 8");
                                        _fbCommand.CommandText =
                                            $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                        await _fbCommand.ExecuteNonQueryAsync();
                                        progress.Report("Entrada removida.");
                                        continue;
                                    }


                                    progress.Report($"Carregando Estoque: {(string)estoqueTable.Rows[0]["DESCRICAO"]}");

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
                                            progress.Report($"Erro ao carregar estoque: {estoqueResponse.Item2}");

                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        progress.Report("Erro ao carregar estoque:");
                                        progress.Report(e.Message);
                                        Stop();
                                    }
                                    _logger.Debug($"Item code:{body.code} enviado");
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

                                        progress.Report(
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
                                            progress.Report("Erro ao enviar sellout:");
                                            progress.Report(e.Message);
                                            Stop();
                                            return;
                                        }

                                        _logger.Debug($"Venda id:{selloutBody.id} enviada");
                                    }
                                    catch (Exception e)
                                    {
                                        progress.Report("Erro ao enviar sellout:");
                                        progress.Report(e.Message);
                                        Stop();
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
                                            progress.Report("Entrada removida.");
                                            continue;
                                        }

                                        progress.Report(
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
                                            progress.Report("Erro ao enviar sellin:");
                                            progress.Report(e.Message);
                                            Stop();
                                            return;
                                        }
                                        _logger.Debug($"Compra id:{sellinBody.id} enviada");

                                    }
                                    catch (Exception e)
                                    {
                                        progress.Report("Erro ao enviar sellin:");
                                        progress.Report(e.Message);
                                        Stop();
                                        return;
                                    }

                                    break;
                                default:
                                    progress.Report("TABLE_NAME não reconhecido");
                                    break;
                            }

                            progress.Report("Carregado com sucesso.");
                            try
                            {
                                _fbCommand.CommandText =
                                    $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                                await _fbCommand.ExecuteNonQueryAsync();
                                progress.Report("Entrada removida.");
                            }
                            catch (Exception e)
                            {
                                progress.Report("Falha ao apagar o registro");
                                _logger.Error("Falha ao apagar o registro", e);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        progress.Report("Falha não identificada...");
                        progress.Report(e.Message);
                        Stop();
                        return;
                    }




                    
                    #endregion COLETA E ENVIO
                }
                _logger.Info("Carga concluída com sucesso!");
                _logger.Info($"Próxima carga: {DateTime.Now.AddMinutes(5):HH:mm}");
                progress.Report("Carga concluída com sucesso");
                progress.Report($"Próxima carga: {DateTime.Now.AddMinutes(5):HH:mm}");
                _intervalTimer.Start();
                await _farol.WaitAsync();
            }
        }

        private async Task<bool> GravaPathNoTxt()
        {
            try
            {
                if (!File.Exists("path.txt"))
                {
                    File.Create("path.txt").Dispose();
                }

                File.WriteAllText("path.txt", TxbDBPath.Text);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Falha ao gravar o caminho.\n{e.Message}");
                return false;

            }
        }

        private async Task<bool> ObtemAuthKey(IProgress<string> progress)
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
                progress.Report("Falha ao obter o CNPJ do Emitente");
                Stop();
                return false;
            }

            _cnpj = ((string)emitenteTable.Rows[0]["CNPJ"]).TiraPont();
            var response = await YandehAPI.CadastraAPIKey(_cnpj);
            if (response.Item1)
            {
                _authKey = response.Item2;
                return true;
            }
            Stop();
            return false;
        }

        private async Task CheckTablesAndTriggers(IProgress<string> progress)
        {
            _fbCommand.Connection = _fbConnection;
            _fbCommand.CommandText = "SELECT COUNT(1) FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'YANDEH_PRODUTO'";
            _fbCommand.CommandType = CommandType.Text;
            var triggerCount = await _fbCommand.ExecuteScalarAsync();
            if (triggerCount is not int)
            {
                progress.Report("Falha ao obter o status das tabelas Yandeh");
                Stop();
                return;
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
                        progress.Report("Falha ao criar as tabelas");
                        progress.Report(e.Message);
                        Stop();
                        return;
                    }
                }
            }
            _tablesChecked = true;
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

        private void ButIniciar_OnClick(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void ButParar_OnClick(object sender, RoutedEventArgs e)
        {
            Stop();
        }
    }
}
