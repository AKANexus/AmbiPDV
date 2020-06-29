using DeclaracoesDllSat;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using static PDV_WPF.staticfunc;

namespace PDV_WPF.NovoACI
{
    /// <summary>
    /// Interaction logic for configurarSAT.xaml
    /// </summary>
    public partial class configurarSAT : Page
    {
        #region Fields & Properties

        PrinterSettings MPS;
        public int numero { get; set; }
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public configurarSAT(PrinterSettings PrevPS)
        {
            MPS = PrevPS;
            using (var Emitente_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable())
            using (var Emitente_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                Emitente_TA.Connection = LOCAL_FB_CONN;
                Emitente_TA.Fill(Emitente_DT);
                InitializeComponent();
                txb_signAC.Text = Properties.Settings.Default.signAC;
                txb_CodAtiv.Text = Properties.Settings.Default.SAT_CodAtiv;
                MPS.sat_cnpj = mtt_CNPJ.Value = Emitente_DT[0]["CNPJ"].ToString().Replace("-", "");
                MPS.sat_ie = txb_IE.Text = Emitente_DT[0]["INSC_ESTAD"].ToString();
            }
            mtt_CNPJ.IsReadOnly = true;
            txb_IE.IsReadOnly = true;
            
        }

        #endregion (De)Constructor

        #region Events

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                try
                {
                    var ns = new NumSessao();
                    Declaracoes_DllSat.sRetorno = Marshal.PtrToStringAnsi(Declaracoes_DllSat.ConsultarSAT(ns.GeraNumero()));
                    string[] retorno = Declaracoes_DllSat.sRetorno.Split('|');
                    if (retorno.Length <2)
                    {
                        MessageBox.Show("Erro ao obter retorno. Retorno.lenght = 0.");
                        return;
                    }
                    else
                    {
                        switch (retorno[1])
                        {
                            case "08000":
                                MessageBox.Show("SAT configurado com sucesso.");
                                but_Next.MouseDown += but_Next_MouseDown;
                                tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                                break;
                            case "08001":
                                MessageBox.Show("Código de Ativação Incorreto.");
                                break;
                            case "08002":
                                MessageBox.Show("SAT ainda não ativado");
                                break;
                            case "08098":
                                MessageBox.Show("SAT em processamento. Tente novamente mais tarde.");
                                break;
                            case "08099":
                                MessageBox.Show("Erro desconhecido.");
                                break;
                            default:
                                throw new Exception("Erro durante Teste Fim a Fim. Nenhum código de retorno recebido. " + retorno[1]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    gravarMensagemErro(RetornarMensagemErro(ex, true));
                    MessageBox.Show(ex.Message);
                    return;
                }
            });
        }

        private void but_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                Properties.Settings.Default.SATSERVIDOR = (bool)chk_SATSERVIDOR.IsChecked;
                Properties.Settings.Default.SAT_CodAtiv = txb_CodAtiv.Text;
                Properties.Settings.Default.signAC = txb_signAC.Text;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
                try
                {
                    //string retorno = SAT.SATFuncoes.LinkSignature(Properties.Settings.Default.SATCodAtiv, Properties.Settings.Default.CNPJ_Emit, Properties.Settings.Default.signAC)[1];
                    //MessageBox.Show(retorno);
                }
                catch (Exception ex)
                {
                    gravarMensagemErro(RetornarMensagemErro(ex, true));
                    MessageBox.Show(ex.Message);
                }
                MPS.sat_signAC = txb_signAC.Text;
                MPS.sat_cod_ativ = txb_CodAtiv.Text;
                MPS.status_sat = setupstatus.SetupDone;
                NavigationService.Navigate(new FimdeConfig(MPS));
                return;
            });
        }

        private void but_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
            return;
        }

        #endregion Events

        #region Methods



        #endregion Methods

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            but_Next.IsEnabled = true;
            tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            but_Next.IsEnabled = false;
            tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#6609CAAA"));
        }
    }
}