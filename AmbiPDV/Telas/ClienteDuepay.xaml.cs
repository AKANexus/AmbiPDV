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
using PDV_WPF.Funcoes;
using PDV_WPF.Objetos;
using System.Windows.Markup;
using System.Linq;
using Clearcove.Logging;
using FluentValidation.Results;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para ClienteDuepay.xaml
    /// </summary>
    public partial class ClienteDuepay : Window
    {
        private Logger log = new Logger("ClienteDuepay");
        private LoadingProccess loadingProccess;        
        private readonly string _identificacaoConsumidor;
        private bool cpfOrCnpjJaInformado;
        private Venda _vendaAtual;
        private int numberLucky;
        public int idCliente;

        public ClienteDuepay(string identificacaoConsumidor, ref Venda vendaAtual)
        {
            _identificacaoConsumidor = identificacaoConsumidor;
            _vendaAtual = vendaAtual;
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

                            (idCliente, numberLucky) = await ChecksIfClientExists(CpfOrCnpj: txb_Cpf.Text.TiraPont());
                            if (idCliente == 0) 
                                (idCliente, numberLucky) = await SaveCustomerOnBase();

                            var errors = _vendaAtual.SetClienteDuepay(new ClienteDuePayDTO(id: idCliente,
                                                                                           nome: txb_Nome.Text,
                                                                                           cpfOrCnpj: txb_Cpf.Text.TiraPont().ParseToCpfOrCnpj(),
                                                                                           telefone: txb_Telefone.Text,
                                                                                           numeroDaSorte: numberLucky));
                            if (errors != null)
                            {
                                foreach(var error in errors)
                                {
                                    log.Error($"Erro ao validar cliente duepay.");
                                    log.Error($"{error}");
                                }                                
                                DialogBox.Show("Erro", DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.ERRO_VENDA_CLIENTE_DUEPAY);
                            }

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
        private async Task<(int customerIdServ, int luckyNumber)> SaveCustomerOnBase()
        {
            var retorno = await SaveCustomerOnConnection(SERVERNAME, SERVERCATALOG);
            return await SaveCustomerOnConnection("localhost", localpath, retorno.customerIdServ, retorno.luckyNumber);
        }        

        private async Task<(int customerIdServ, int luckyNumber)> SaveCustomerOnConnection(string serverName, string serverCatalog, int idComingFromTheServer = 0, int numberComingFromTheServer = 0)
        {
            using (FbConnection connection = new FbConnection(MontaStringDeConexao(serverName, serverCatalog)))
            {
                connection.Open();

                using (FbTransaction transaction = connection.BeginTransaction(new FbTransactionOptions() { TransactionBehavior = FbTransactionBehavior.Wait, WaitTimeout = new TimeSpan(0, 0, 10) }))
                {
                    await Task.Delay(1000);

                    try
                    {
                        string ddd = "11";
                        string telefone = txb_Telefone.Text;

                        if (txb_Telefone.Text.Length > 9)
                        {
                            ddd = txb_Telefone.Text.Substring(0, 2);
                            telefone = txb_Telefone.Text.Substring(2, 9);
                        }

                        using (var taCliente = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter() { Connection = connection, Transaction = transaction })
                        {                            
                            int newCustomerIdServ = idComingFromTheServer == 0 ? Convert.ToInt32(taCliente.GenTbClienteId()) : idComingFromTheServer;
                            int newLuckyNumber = numberComingFromTheServer == 0 ? GenerateLuckyNumber(taCliente) : numberComingFromTheServer;

                            taCliente.InsertDuepayCustomer(ID_CLIENTE: newCustomerIdServ, 
                                                           DT_CADASTRO: DateTime.Now, 
                                                           NOME: txb_Nome.Text, 
                                                           MENSAGEM: newLuckyNumber.ToString(), 
                                                           DDD_CELUL: ddd, 
                                                           FONE_CELUL: telefone);

                            if (txb_Cpf.Text.TiraPont() is string customerCpf && customerCpf.Length == 11)
                            {
                                using (var taClientePf = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PFTableAdapter() { Connection = connection, Transaction = transaction })
                                {                                                                        
                                    taClientePf.InsertCustomerPf(ID_CLIENTE: newCustomerIdServ, CPF: customerCpf.ParseToCpfOrCnpj());
                                }
                            }
                            else if (txb_Cpf.Text.TiraPont() is string customerCnpj && customerCnpj.Length == 14)
                            {
                                using (var taClientePj = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PJTableAdapter() { Connection = connection, Transaction = transaction })
                                {                                                                       
                                    taClientePj.InsertCustomerPj(ID_CLIENTE: newCustomerIdServ, CNPJ: customerCnpj.ParseToCpfOrCnpj());
                                }
                            }
                            transaction.Commit();
                            return (newCustomerIdServ, newLuckyNumber);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Falha:", ex);
                    }
                }
            }           
        }

        private async Task<(int customerIdServ, int luckyNumber)> ChecksIfClientExists(string CpfOrCnpj)
        {
            try
            {
                await Task.Delay(1500);

                if(CpfOrCnpj.Length == 11)
                {
                    using(var taClientePf = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PFTableAdapter() { Connection = new FbConnection(MontaStringDeConexao("localhost", localpath)) })
                    { 
                        var customerFound = taClientePf.CheckRegisteredCustomerPf(CPF: CpfOrCnpj.ParseToCpfOrCnpj());
                        if (customerFound is not null && int.TryParse(customerFound.ToString(), out int id))
                        {
                            using(var dtCliente = new DataSets.FDBDataSetOperSeed.TB_CLIENTEDataTable())
                            using(var taClientePdv = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter() { Connection = new FbConnection(MontaStringDeConexao("localhost", localpath)) })                            
                            {
                                taClientePdv.FillById(dtCliente, id);

                                if (dtCliente.First().IsMENSAGEMNull())
                                {
                                    using (var taClienteServ = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter() { Connection = new FbConnection(MontaStringDeConexao(SERVERNAME, SERVERCATALOG)) })
                                    {
                                        int newNumber = GenerateLuckyNumber(taClientePdv);
                                        taClientePdv.SetMensagemByCliente(MENSAGEM: newNumber.ToString(), ID_CLIENTE: id);
                                        taClienteServ.SetMensagemByCliente(MENSAGEM: newNumber.ToString(), ID_CLIENTE: id);
                                        return (id, newNumber);
                                    }
                                }

                                if (int.TryParse(dtCliente.Select(x => x.MENSAGEM).FirstOrDefault(), out int savedNumber))                                  
                                    return (id, savedNumber);                                                                    
                                else
                                    return (id, 0);
                            }                            
                        }                            
                        return (0, 0);                        
                    }
                }
                else if(CpfOrCnpj.Length == 14)
                {
                    using(var taClientePj = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLI_PJTableAdapter() { Connection = new FbConnection(MontaStringDeConexao("localhost", localpath)) })
                    {
                        var customerFound = taClientePj.CheckRegisteredCustomerPj(CNPJ: CpfOrCnpj.ParseToCpfOrCnpj());
                        if (customerFound is not null && int.TryParse(customerFound.ToString(), out int id))
                        {
                            using (var dtCliente = new DataSets.FDBDataSetOperSeed.TB_CLIENTEDataTable())
                            using (var taClientePdv = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter() { Connection = new FbConnection(MontaStringDeConexao("localhost", localpath)) })
                            {
                                taClientePdv.FillById(dtCliente, id);

                                if (dtCliente.First().IsMENSAGEMNull())
                                {
                                    using (var taClienteServ = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter() { Connection = new FbConnection(MontaStringDeConexao(SERVERNAME, SERVERCATALOG)) })
                                    {
                                        int newNumber = GenerateLuckyNumber(taClientePdv);
                                        taClientePdv.SetMensagemByCliente(MENSAGEM: newNumber.ToString(), ID_CLIENTE: id);
                                        taClienteServ.SetMensagemByCliente(MENSAGEM: newNumber.ToString(), ID_CLIENTE: id);
                                        return (id, newNumber);
                                    }
                                }
                                if (int.TryParse(dtCliente.Select(x => x.MENSAGEM).FirstOrDefault(), out int savedNumber))
                                    return (id, savedNumber);
                                else
                                    return (id, 0); ;
                            }
                        }
                        return (0, 0);                        
                    }
                }
                return (0, 0);
            }
            catch(Exception ex)
            {
                throw new Exception("Falha:", ex);                                    
            }
        }

        Func <DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter, int> GenerateLuckyNumber = (taCliente) =>
        {
            Random random = new Random();
            using (var dtCliente = new DataSets.FDBDataSetOperSeed.TB_CLIENTEDataTable())
            {
                StartNumberGenerator:

                dtCliente.Clear();
                int number = random.Next(100000, 999999);
                taCliente.FillByClienteDuepay(dtCliente, "CLIENTE DUEPAY");

                if (dtCliente != null && dtCliente.Count > 0 && dtCliente.Select($"MENSAGEM = '{number}'").Length > 0)
                    goto StartNumberGenerator;

                return number;
            }            
        };
        #endregion Methods
    }
}
