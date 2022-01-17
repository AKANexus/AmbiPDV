using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        public MainWindow()
        {
            InitializeComponent();
            ButParar.IsEnabled = false;
            _progress = new(AtualizaTextBox);
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

            try
            {
                await _fbConnection.OpenAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show($"Falha ao abrir a conexão com o Clipp.\n{e.Message}");
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
                            if (string.IsNullOrWhiteSpace((string) estoqueTable.Rows[0]["COD_BARRA"])) continue;
                            try
                            {
                                await YandehAPI.EnviaEstoque((string)estoqueTable.Rows[0]["COD_BARRAS"], (string)estoqueTable.Rows[0]["DESCRICAO"], decimal.Parse((string)estoqueTable.Rows[0]["PRC_VENDA"]));
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
                            _fbCommand.CommandText = $"SELECT * FROM TB_NFV_ITEM WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]};";
                            itensTable.Load(await _fbCommand.ExecuteReaderAsync());

                            var pagamentosTable = new DataTable();
                            _fbCommand.CommandText = $"SELECT * FROM V_NFVENDA_PAGAMENTOS WHERE ID_NFVENDA = {fbDataRow["RESPECTIVE_ID"]}";
                            pagamentosTable.Load(await _fbCommand.ExecuteReaderAsync());

                            SelloutBody body = new();
                            body.id =
                                $"{compraTable.Rows[0]["NF_MODELO"]}-{compraTable.Rows[0]["NF_SERIE"]}-{compraTable.Rows[0]["NF_NUMERO"]}";
                            body.sellout_timestamp =
                                $"{compraTable.Rows[0]["DT_SAIDA"]} {compraTable.Rows[0]["HR_SAIDA"]}";
                            body.store_taxpayer_id = _cnpj;
                            body.receipt_number = int.Parse((string)compraTable.Rows[0]["NF_NUMERO"]);
                            body.receipt_series_number = (string)compraTable.Rows[0]["NF_NUMERO"];
                            body.subtotal = float.Parse((string) compraTable.Rows[0]["TOT_PRODUTO"]) +
                                            float.Parse((string) compraTable.Rows[0]["TOT_SERVICO"]);
                            body.cancellation_flag = "N";
                            body.operation = "S";
                            body.transaction_type = "V";
                            foreach (DataRow dataRow in itensTable.Rows)
                            {
                                Item item = new Item();
                                item.code = (string) dataRow["ID_IDENTIFICADOR"];
                                item.sku = string.IsNullOrWhiteSpace((string) dataRow["7898223580217"])
                                    ? (string) dataRow["ID_IDENTIFICADOR"]
                                    : (string) dataRow["7898223580217"];
                                item.description = (string) dataRow["PRODUTO"];
                                item.quantity = float.Parse((string) dataRow["QTD_ITEM"]);
                                item.measurement_unit = (string) dataRow["UNI_MEDIDA"];
                                item.cancellation_flag = "N";
                                item.cfop = 5102;
                                item.cst = "00";
                                body.items.Add(item);
                            }

                            body.tipo = (string) compraTable.Rows[0]["NF_MODELO"] switch
                            {
                                "55" => "nfe",
                                _ => "sat"
                            };
                            foreach (DataRow pagamentosTableRow in pagamentosTable.Rows)
                            {
                                Payment payment = new();
                                payment.method = (string) pagamentosTableRow["DESC_FORMAPAGAMENTO"];
                                payment.condition = "Á vista";
                                Installment installment = new();
                                installment.installment_number = 1;
                                installment.amount = float.Parse((string) pagamentosTableRow["VLR_PAGTO"]);
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
                }
            }
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
