using DeclaracoesDllSat;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Telas;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using SAT = DeclaracoesDllSat.Declaracoes_DllSat;


namespace PDV_WPF.PCA_Telas
{
    /// <summary>
    /// Interaction logic for PCA_SAT.xaml
    /// </summary>
    public partial class PCA_SAT : Page
    {
        private bool _configurado = false;
        private bool _semsat = false;
        public PCA_SAT()
        {
            InitializeComponent();
            using var Emitente_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable();
            using var Emitente_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            Emitente_TA.Connection = LOCAL_FB_CONN;
            Emitente_TA.Fill(Emitente_DT);
            InitializeComponent();
            txb_CNPJ.Text = Emitente_DT[0]["CNPJ"].ToString();
            txb_IE.Text = Emitente_DT[0]["INSC_ESTAD"].ToString();
            txb_signAC.Text = SIGN_AC;
            txb_CodAtiv.Text = SAT_CODATIV;
            if (MODELO_SAT != ModeloSAT.NENHUM) { cbb_ModSat.SelectedIndex = MODELO_SAT.Safeint(); }
            chk_SATSERVIDOR.IsChecked = SATSERVIDOR;
        }

        private void But_Next_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed || e.MiddleButton == MouseButtonState.Pressed) return;

            if (!_configurado)
            {
                return;
            }
            switch (_semsat)
            {
                case true:
                    MODELO_SAT = ModeloSAT.NENHUM;
                    SATSERVIDOR = false;
                    SAT_CODATIV = txb_CodAtiv.Text;
                    SIGN_AC = txb_signAC.Text;
                    SAT_USADO = false;
                    ECF_ATIVA = false;
                    break;
                default:
                case false:
                    MODELO_SAT = (ModeloSAT)cbb_ModSat.SelectedIndex;
                    SATSERVIDOR = (bool)chk_SATSERVIDOR.IsChecked;
                    SAT_CODATIV = txb_CodAtiv.Text;
                    SIGN_AC = txb_signAC.Text;
                    SAT_USADO = true;
                    ECF_ATIVA = false;
                    break;
            }
            SalvaConfigsNaBase();
            Window.GetWindow(this).Close();
        }

        private void But_Prev_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void But_Action_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((txb_signAC.Text.ToUpper() == "HOMOLOGACAO" || txb_signAC.Text.ToUpper() == "SDK") && homologaSAT)
            {
                txb_signAC.Text = "SGR-SAT SISTEMA DE GESTAO E RETAGUARDA DO SAT";
            }

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                _semsat = true;
                txb_signAC.Text = "O representante dessa empresa se responsabiliza por não utilizar um equipamento emissor de CFe. Pressione \"Finalizar\" para confirmar.";
                _configurado = true;
                tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                return;

            }
            _semsat = false;
            if (txb_CodAtiv.Text.Length == 0 || txb_signAC.Text.Length == 0)
            {
                return;
            }

            MODELO_SAT = (ModeloSAT)cbb_ModSat.SelectedIndex;
            switch (chk_SATSERVIDOR.IsChecked)
            {
                case true:
                    TestaSATServidor();
                    break;
                case false:
                    TestaSATLocal();
                    break;
                default:
                    break;
            }

        }

        private void TestaSATLocal()
        {
            var ns = new NumSessao();
            SAT.sRetorno = Marshal.PtrToStringAnsi(SAT.ConsultarSAT(ns.GeraNumero(), MODELO_SAT));
            string[] retorno = SAT.sRetorno.Split('|');
            if (retorno.Length < 2)
            {
                MessageBox.Show("Erro ao obter retorno." + retorno[0]);
            }
            else
            {
                switch (retorno[1])
                {
                    case "08000":
                        MessageBox.Show("SAT configurado com sucesso.");
                        _configurado = true;
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
                        MessageBox.Show("Erro durante Teste Fim a Fim. Nenhum código de retorno recebido. " + retorno[1]);
                        break;
                }
            }
        }

        private void TestaSATServidor()
        {
            try
            {
                if (!ChecaStatusSATServidor())
                {
                    MessageBox.Show("Erro ao Testar SAT Servidor");
                    return;
                }
                else
                {
                    MessageBox.Show("SAT configurado com sucesso.");
                    _configurado = true;
                    tbl_Continuar.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF09CAAA"));
                    return;
                }

            }
            catch (System.Exception ex)
            {
                DialogBox.Show("Sat Servidor", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Erro ao testar SAT Servidor", ex.Message);
                return;
            }
        }

        private void Chk_SATSERVIDOR_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Chk_SATSERVIDOR_Click(object sender, RoutedEventArgs e)
        {

        }
        private void but_Prev_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_Prev.FontSize = 24;
        }
        private void but_Prev_MouseLeave(object sender, MouseEventArgs e)
        {
            lbl_Prev.FontSize = 20;
        }
        private void but_Action_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_Action.FontSize = 24;
        }
        private void but_Action_MouseLeave(object sender, MouseEventArgs e)
        {
            lbl_Action.FontSize = 20;
        }
        private void but_Next_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_Next.FontSize = 24;
        }
        private void but_Next_MouseLeave(object sender, MouseEventArgs e)
        {
            lbl_Next.FontSize = 20;
        }
    }
}
