using FirebirdSql.Data.FirebirdClient;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using PDV_WPF.Properties;
using System.Text.RegularExpressions;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para ClienteDuepay.xaml
    /// </summary>
    public partial class ClienteDuepay : Window
    {
        private LoadingProccess loadingProccess;
        private readonly string _identificacaoConsumidor;
        private bool cpfOrCnpjJaInformado;

        public ClienteDuepay(string identificacaoConsumidor)
        {
            _identificacaoConsumidor = identificacaoConsumidor;
            InitializeComponent();
        }

        #region Events
        private void Window_loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_identificacaoConsumidor))
            {
                txb_Cpf.Text = _identificacaoConsumidor;
                txb_Cpf.IsEnabled = false;
                cpfOrCnpjJaInformado = true;
            }
            txb_Nome.Focus();
        }

        private void Window_keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogBox.Show("Atenção", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "É obrigatório informar os dados do cliente.");
            }
        }

        private async void txb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                textBox.Text = textBox.Text.Trim();                                    
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    MessageBox.Show($"Preencha o {textBox.Name switch { "txb_Nome" => "NOME", "txb_Cpf" => "CPF", "txb_Telefone" => "TELEFONE", _ => "CAMPO" }} do cliente corretamente.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                switch (textBox.Name)
                {
                    case "txb_Nome":
                        if (cpfOrCnpjJaInformado)
                            txb_Telefone.Focus();
                        else txb_Cpf.Focus();
                        break;
                    case "txb_Cpf":
                        if(!Funcoes.ValidaCNPJ.IsCnpj(textBox.Text.ToString()) && !Funcoes.ValidaCPF.IsCpf(textBox.Text.ToString()))
                        {
                            DialogBox.Show(strings.IDENTIFICACAO_DO_CONSUMIDOR, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.CPF_CNPJ_INVALIDO);
                            return;
                        }
                        txb_Telefone.Focus();                        
                        break;
                    case "txb_Telefone":
                        if (string.IsNullOrEmpty(txb_Nome.Text) || string.IsNullOrEmpty(txb_Cpf.Text) || string.IsNullOrEmpty(txb_Telefone.Text))
                        {
                            MessageBox.Show("Campos obrigatórios não estão preenchidos.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        try
                        {
                            loadingProccess = new();
                            loadingProccess.Show();
                            loadingProccess.progress.Report("Gravando dados...");
                            this.IsEnabled = false;
                            if(!await ChecksIfClientExists(CpfOrCnpj: txb_Cpf.Text)) await SaveCustomerOnBase();
                            loadingProccess.Close();
                            this.IsEnabled = true;
                        }
                        catch (Exception ex)
                        {
                            loadingProccess.Close();
                            DialogBox.Show("Erro", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Ocorreu erros ao salvar/verificar cliente:", $"{ex.InnerException?.Message ?? ex.Message}");
                            this.IsEnabled = true;
                            txb_Telefone.Focus();
                            return;
                        }

                        DialogResult = true;
                        this.Close();
                        break;
                }
            }
        }
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion Events

        #region Methods
        private async Task SaveCustomerOnBase()
        {
            using (var connectionServ = new FbConnection(MontaStringDeConexao(SERVERNAME, SERVERCATALOG)))
            {
                connectionServ.Open();
                using (var transactionServ = connectionServ.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, 10) }))
                {
                    try
                    {                        
                        await Task.Delay(2000);                         

                        string ddd = "11";
                        string telefone = txb_Telefone.Text;

                        if (txb_Telefone.Text.Length > 9)
                        {
                            ddd = txb_Telefone.Text.Substring(0, 2);
                            telefone = txb_Telefone.Text.Substring(2, 9);
                        }

                        using (var taCliente = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter())
                        {
                            taCliente.Connection = connectionServ;
                            taCliente.Transaction = transactionServ;

                            taCliente.InsertDuepayCustomer(DT_CADASTRO: DateTime.Now, NOME: txb_Nome.Text, DDD_CELUL: ddd, FONE_CELUL: telefone);
                            int idCliente = (int)taCliente.SelectLastId();                                                        

                            if (txb_Cpf.Text.Length == 11)
                            {
                                using (var taClientePf = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PFTableAdapter())
                                {
                                    taClientePf.Connection = connectionServ;
                                    taClientePf.Transaction = transactionServ;
                                    taClientePf.InsertCustomerPf(ID_CLIENTE: idCliente, CPF: txb_Cpf.Text);
                                }
                            }
                            else if (txb_Cpf.Text.Length == 14)
                            {
                                using (var taClientePj = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PJTableAdapter())
                                {
                                    taClientePj.Connection = connectionServ;
                                    taClientePj.Transaction = transactionServ;
                                    taClientePj.InsertCustomerPj(ID_CLIENTE: idCliente, CNPJ: txb_Cpf.Text);
                                }
                            }                            
                        }
                        transactionServ.Commit();
                    }
                    catch (Exception ex)
                    {
                        transactionServ.Rollback();
                        throw new Exception("Falha:", ex);
                    }
                }
            }
        }

        private async Task<bool> ChecksIfClientExists(string CpfOrCnpj)
        {
            try
            {
                await Task.Delay(2000);

                if(CpfOrCnpj.Length == 11)
                {
                    using(var taClientePf = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PFTableAdapter())
                    {
                        int CustomerFound = (int)taClientePf.CheckRegisteredCustomerPf(CPF: CpfOrCnpj);
                        if (CustomerFound > 0) return true;
                        else return false;
                    }
                }
                else if(CpfOrCnpj.Length == 14)
                {
                    using(var taClientePj = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PJTableAdapter())
                    {
                        int CustomerFound = (int)taClientePj.CheckRegisteredCustomerPj(CNPJ: CpfOrCnpj);
                        if (CustomerFound > 0) return true;
                        else return false;
                    }
                }

                return false;
            }
            catch(Exception ex)
            {
                throw new Exception("Falha:", ex);                                    
            }
        }
        #endregion Methods
    }
}
