using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirebirdSql.Data.FirebirdClient;
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
        public MainWindow()
        {
            InitializeComponent();
            ButParar.IsEnabled = false;
            _progress = new(AtualizaTextBox);
            if (File.Exists("path.txt"))
            {
                TxbDBPath.Text = File.ReadAllText("path.txt");
            }
        }

        private void AtualizaTextBox(string message)
        {

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
            TxbDBPath.IsReadOnly = false;
            ButParar.IsEnabled = false;
            ButIniciar.IsEnabled = true;
            _isRunning = false;
        }

        private void Start()
        {
            TxbDBPath.IsReadOnly = true;
            ButParar.IsEnabled = true;
            ButIniciar.IsEnabled = false;
            _isRunning = true;
            RunScripts(_progress);
        }

        private async Task<bool> RunScripts(IProgress<string> progress)
        {
            if (string.IsNullOrWhiteSpace(TxbDBPath.Text) || TxbDBPath.Text.Split('|').Length != 2)
            {
                MessageBox.Show("Caminho da base de dados inválido. Tente novamente.");
                return false;
            }
            var dataSource = TxbDBPath.Text.Split('|')[0];
            var catalog = TxbDBPath.Text.Split('|')[1];
            _fbConnection =
                new($@"initial catalog={catalog};data source={dataSource};user id=SYSDBA;password=masterke;encoding=WIN1252;charset=utf8");
            _fbCommand = new();
            _fbData = new();
            try
            {
                await _fbConnection.OpenAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Falha ao abrir a conexão com o Clipp.\n{e.Message}");
                return false;
            }

            if (!await GravaPathNoTxt())
            {
                return false;
            }


            if (!_tablesChecked)
                await CheckTablesAndTriggers();

            _fbCommand.CommandText = "SELECT * FROM YAN_SYNC";
            try
            {
                _fbData.Load(await _fbCommand.ExecuteReaderAsync());
            }
            catch (Exception e)
            {
                MessageBox.Show($"Falha ao obter os dados de YAN_SYNC.\n{e.Message}");
                return false;
            }

            if (!await ObtemAuthKey())
            {
                MessageBox.Show("Falha ao obter a authkey");
                return false;
            }


            if (_fbData.Rows.Count == 0)
            {
                return true;
            }
            else
            {
                foreach (DataRow fbDataRow in _fbData.Rows)
                {
                    switch (fbDataRow["TABLE_NAME"])
                    {
                        case "ESTOQUE":
                            _fbCommand.CommandText =
                                $"SELECT * FROM TB_ESTOQUE te JOIN TB_EST_IDENTIFICADOR tei ON te.ID_ESTOQUE = tei.ID_ESTOQUE JOIN TB_EST_PRODUTO tep ON tei.ID_IDENTIFICADOR = tep.ID_IDENTIFICADOR WHERE te.ID_ESTOQUE = {fbDataRow["RESPECTIVE_ID"]};";
                            var estoqueTable = new DataTable();
                            estoqueTable.Load(await _fbCommand.ExecuteReaderAsync());
                            if (estoqueTable.Rows.Count == 0) continue;
                            if (string.IsNullOrWhiteSpace((string)estoqueTable.Rows[0]["COD_BARRA"])) continue;
                            try
                            {
                                var estoqueResponse = await YandehAPI.EnviaEstoque((string)estoqueTable.Rows[0]["COD_BARRA"], (string)estoqueTable.Rows[0]["DESCRICAO"], (decimal)estoqueTable.Rows[0]["PRC_VENDA"]);
                                if (!estoqueResponse.Item1)
                                {
                                    MessageBox.Show(estoqueResponse.Item2);
                                }
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show($"Falha ao enviar produto.\n{e.Message}");
                                return false;
                            }
                            break;
                        case "VENDA":
                            _fbCommand.CommandText =
                                $"SELECT * FROM V_NFV WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]};";
                            var compraTable = new DataTable();
                            compraTable.Load(await _fbCommand.ExecuteReaderAsync());
                            if (compraTable.Rows.Count == 0) continue;

                            var itensTable = new DataTable();
                            _fbCommand.CommandText = $"SELECT * FROM V_NFV_ITEM WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]};";
                            itensTable.Load(await _fbCommand.ExecuteReaderAsync());

                            var pagamentosTable = new DataTable();
                            _fbCommand.CommandText = $"SELECT * FROM V_NFVENDA_PAGAMENTOS WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]}";
                            pagamentosTable.Load(await _fbCommand.ExecuteReaderAsync());

                            var satTable = new DataTable();
                            _fbCommand.CommandText =
                                $"SELECT * FROM V_SAT WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]}";
                            satTable.Load(await _fbCommand.ExecuteReaderAsync());

                            var nfeTable = new DataTable();
                            _fbCommand.CommandText =
                                $"SELECT * FROM V_NFE WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]}";
                            nfeTable.Load(await _fbCommand.ExecuteReaderAsync());


                            SelloutBody body = new();
                            body.origem_coleta = "API|Ambisoft";
                            body.id =
                                $"{compraTable.Rows[0]["NF_MODELO"]}-{compraTable.Rows[0]["NF_SERIE"]}-{compraTable.Rows[0]["NF_NUMERO"]}";
                            body.sellout_timestamp =
                                $"{compraTable.Rows[0]["DT_SAIDA"]:yyyy-MM-dd}T{(TimeSpan)compraTable.Rows[0]["HR_SAIDA"]:hh\\:mm\\:ss}";
                            body.store_taxpayer_id = _cnpj;
                            body.checkout_id = $"{compraTable.Rows[0]["NF_SERIE"]}";
                            body.receipt_number = ((int)compraTable.Rows[0]["NF_NUMERO"]).ToString();
                            body.receipt_series_number = Regex.Match((string)compraTable.Rows[0]["NF_SERIE"], @"\d+").Value;
                            body.total = (decimal)compraTable.Rows[0]["TOT_PRODUTO"] +
                                            (decimal)compraTable.Rows[0]["TOT_SERVICO"];
                            body.cancellation_flag = "N";
                            body.operation = "S";
                            body.transaction_type = "V";
                            body.ipi = 0m;
                            body.sales_discount = 0m;
                            body.sales_addition = 0m;
                            body.icms = 0m;
                            body.frete = 0m;
                            body.items = new();
                            if (nfeTable.Rows.Count > 0 && !string.IsNullOrWhiteSpace((string)nfeTable.Rows[0]["ID_NFE"]))
                            {
                                body.nfe_access_key = (string)nfeTable.Rows[0]["ID_NFE"];
                            }
                            if (satTable.Rows.Count > 0 && !string.IsNullOrWhiteSpace((string)satTable.Rows[0]["CHAVE"]))
                            {
                                body.nfe_access_key = (string)satTable.Rows[0]["CHAVE"];
                            }
                            foreach (DataRow dataRow in itensTable.Rows)
                            {
                                SelloutItem selloutItem = new SelloutItem();
                                selloutItem.code = ((int)dataRow["ID_IDENTIFICADOR"]).ToString();
                                selloutItem.sku = string.IsNullOrWhiteSpace((string)dataRow["COD_BARRA"])
                                    ? ((int)dataRow["ID_IDENTIFICADOR"]).ToString()
                                    : (string)dataRow["COD_BARRA"];
                                selloutItem.description = (string)dataRow["PRODUTO"];
                                selloutItem.quantity = (decimal)dataRow["QTD_ITEM"];
                                selloutItem.measurement_unit = (string)dataRow["UNI_MEDIDA"];
                                selloutItem.cancellation_flag = "N";
                                selloutItem.cfop = 5102;
                                selloutItem.item_addition = 0m;
                                selloutItem.item_discount = (decimal)dataRow["VLR_DESC"];
                                selloutItem.icms = (decimal)dataRow["VLR_ICMS"];
                                selloutItem.pis = dataRow["VLR_PIS"] is DBNull ? 0m : (decimal)dataRow["VLR_PIS"];
                                selloutItem.cofins = dataRow["VLR_COFINS"] is DBNull ? 0m : (decimal)dataRow["VLR_COFINS"];
                                selloutItem.ipi = dataRow["VLR_IPI"] is DBNull ? 0m : (decimal)dataRow["VLR_IPI"];
                                selloutItem.other_expenses = (decimal)dataRow["VLR_DESPESA"];
                                selloutItem.icms_st = dataRow["VLR_ST"] is DBNull ? 0m : (decimal)dataRow["VL_ST"];
                                selloutItem.fcp_st = 0m;
                                selloutItem.frete = (decimal)dataRow["VLR_FRETE"];
                                body.items.Add(selloutItem);
                            }

                            body.tipo = (string)compraTable.Rows[0]["NF_MODELO"] switch
                            {
                                "55" => "nfe",
                                _ => "sat"
                            };
                            body.payment = new();
                            foreach (DataRow pagamentosTableRow in pagamentosTable.Rows)
                            {
                                Payment payment = new();
                                payment.method = (string)pagamentosTableRow["DESC_FORMAPAGAMENTO"];
                                payment.condition = "Á vista";
                                payment.installments = new();
                                Installment installment = new();
                                installment.installment_number = 1;
                                installment.amount = (decimal)pagamentosTableRow["VLR_PAGTO"];
                                installment.payment_term = 1;
                                payment.installments.Add(installment);
                                body.payment.Add(payment);
                            }

                            try
                            {
                                await YandehAPI.EnviaSellout(body);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show($"Falha ao enviar o Sellout.\n{e.Message}");
                                return false;
                            }
                            break;
                        case "COMPRA":
                            break;
                        default:
                            break;
                    }

                    _fbCommand.CommandText = $"DELETE FROM YAN_SYNC WHERE TABLE_NAME = '{fbDataRow["TABLE_NAME"]}' AND RESPECTIVE_ID = {fbDataRow["RESPECTIVE_ID"]}";
                    await _fbCommand.ExecuteNonQueryAsync();
                }

                return true;
            }
        }

        private async Task<bool> GravaPathNoTxt()
        {
            try
            {
                if (!File.Exists("path.txt"))
                {
                    File.Create("path.txt");
                }

                await File.WriteAllTextAsync("path.txt", TxbDBPath.Text);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Falha ao gravar o caminho.\n{e.Message}");
                return false;

            }
        }

        private async Task<bool> ObtemAuthKey()
        {
            DataTable emitenteTable = new();
            _fbCommand.CommandText = "SELECT * FROM TB_EMITENTE";
            emitenteTable.Load(await _fbCommand.ExecuteReaderAsync());
            if (emitenteTable.Rows.Count < 1 || string.IsNullOrWhiteSpace((string)emitenteTable.Rows[0]["CNPJ"]))
            {
                MessageBox.Show("Falha ao obter o CNPJ");
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

        private async Task CheckTablesAndTriggers()
        {
            _fbCommand.Connection = _fbConnection;
            _fbCommand.CommandText = "SELECT COUNT(1) FROM RDB$TRIGGERS WHERE RDB$TRIGGER_NAME = 'YANDEH_PRODUTO'";
            _fbCommand.CommandType = CommandType.Text;
            var triggerCount = await _fbCommand.ExecuteScalarAsync();
            if (triggerCount is not int)
            {
                MessageBox.Show("Falha ao obter o status das tabelas Yandeh");
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
                        MessageBox.Show($"Falha ao criar as tabelas Yandeh.\n{e.Message}");
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
