using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for PerguntaVendedor.xaml
    /// </summary>
    public partial class PerguntaVendedor : Window
    {
        #region Fields & Properties

        public int id_vendedor { get; set; }
        public string nome_vendedor { get; set; }
        public DateTime? vencimento { get; set; }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public PerguntaVendedor()
        {
            InitializeComponent();
            cbb_Cliente.Focus();

            //System.Data.DataRowCollection metodoRow;

            DataContext = new MainViewModel();
        }

        #endregion (De)Constructor

        #region Events

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            ConfirmaOpcao();
        }

        private void ConfirmaOpcao()
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                try
                {
                    if (cbb_Cliente.SelectedIndex == -1)
                    {
                        DialogResult = false;
                        Close();
                    }
                    else
                    {
                        using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                        using (var Cliente_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_FUNCIONARIOTableAdapter())
                        {
                            Cliente_TA.Connection = LOCAL_FB_CONN;
                            id_vendedor = (int)Cliente_TA.FuncionarioIdByNome(cbb_Cliente.SelectedItem.ToString());
                        }
                        nome_vendedor = cbb_Cliente.SelectedItem.ToString();
                        //vencimento = dtp_Vencimento.SelectedDate;
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show("Erro ao pescar ID_FUNCIONARIO");
                }
            });

        }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void cbb_Cliente_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmaOpcao();
            }
        }

        private void dtp_Vencimento_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            //    {
            //        try
            //        {
            //            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            //            using (var Cliente_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter())
            //            {
            //                Cliente_TA.Connection = LOCAL_FB_CONN;
            //                id_vendedor = (int)Cliente_TA.PegaIDPorCliente(cbb_Cliente.SelectedItem.ToString());
            //            }
            //            nome_vendedor = cbb_Cliente.SelectedItem.ToString();
            //            vencimento = dtp_Vencimento.SelectedDate;
            //            DialogResult = true;
            //            Close();
            //        }
            //        catch (Exception ex)
            //        {
            //            gravarMensagemErro(RetornarMensagemErro(ex, true));
            //            MessageBox.Show("Erro ao pescar ID_CLIENTE");
            //        }
            //    });
            //}
        }

        private void but_Confirmar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmaOpcao();
            }
        }

        #endregion Events

        #region Methods

        #endregion Methods
    }
}
