using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF
{
    /// <summary>
    /// Lógica interna para Parametros.xaml
    /// </summary>
    public partial class Parametros : Window
    {
        #region Fields & Properties

        //private FbConnection LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
        //private FbConnection SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) }; //ATENCAO: Ctrl + P em contingência vai dar pau
        //DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter taConfigServ = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter();
        //FDBDataSetTableAdapters.TRI_PDV_SETUPTableAdapter taSetup = new FDBDataSetTableAdapters.TRI_PDV_SETUPTableAdapter();

        public bool conf_inicial { get; set; }
        private bool _caixaaberto;
        private bool _contingencia;
        private string _macAddr;
        private string _versao;

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public Parametros(bool pCaixaAberto, bool pContingencia)
        {
            _contingencia = pContingencia;
            _caixaaberto = pCaixaAberto;
            _macAddr = (new Funcoes.LicencaDeUsoOffline(90, 15)).GetSerialHexNumberFromExecDisk();
            _versao = "Versão do PDV: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + strings.VERSAO_ADENDO;


            InitializeComponent();

            lbl_Versao.Content = _versao;

        }

        #endregion (De)Constructor

        #region Events

        private void but_Confirmar_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_Da.FontSize = 15;
        }
        private void but_Confirmar_MouseLeave(object sender, MouseEventArgs e)
        {
            lbl_Da.FontSize = 12;
        }
        private void but_Cancelar_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_Nyet.FontSize = 15;
        }
        private void but_Cancelar_MouseLeave(object sender, MouseEventArgs e)
        {
            lbl_Nyet.FontSize = 12;
        }
        private void PCA_but_click(object sender, RoutedEventArgs e)
        {
            //if (txb_No_Caixa.Text.Safeint() <= 0)
            //    MessageBox.Show("Preencha o numero do caixa antes.");
            GravaConfiguracoes();
            (new PCA()).ShowDialog();
        }

        private void ConfigsTecs_click(object sender, RoutedEventArgs e)
        {
            var senhaTecnico = new SenhaTecnico();
            senhaTecnico.ShowDialog();
            if (senhaTecnico.DialogResult == true)
            {
                var paramsTecs = new ParamsTecs(false, _contingencia);
                paramsTecs.ShowDialog();
            }
            else { }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void MetodosPgto_click(object sender, RoutedEventArgs e)
        {
            if (!_caixaaberto && !_contingencia)
            {
                var metodos = new MetodosPGT();
                metodos.ShowDialog();
            }
            else
            {
                DialogBox.Show("Configurações do sistema", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "O caixa está aberto e/ou em contingência. Não será possível alterar métodos de pagamento agora.");
            }
        }
        private void AbreGaveta_spooler(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Confira nas configurações da impressora se a opção\n 'Abre gaveta' está devidamente habilitada!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void chk_Bloqueia_Limite_Checked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Folga.IsEnabled = true;
        }

        private void chk_Bloqueia_Limite_Unchecked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Folga.IsEnabled = false;
            txb_Valor_Folga.Value = 0;
        }

        private void chk_Exige_Sangria_Checked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Max.IsEnabled = true;

        }

        private void chk_Exige_Sangria_Unchecked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Max.IsEnabled = false;
            txb_Valor_Max.Value = 0;
        }
        private void chk_Usatef_Checked(object sender, RoutedEventArgs e)
        {
            chk_maquininha.IsChecked = false;
            chk_maquininha.IsEnabled = false;
        }
        private void chk_Usatef_Unchecked(object sender, RoutedEventArgs e)
        {
            chk_maquininha.IsEnabled = true;
        }
        private void chk_Maquininha_Checked(object sender, RoutedEventArgs e)
        {
            chk_usatef.IsChecked = false;
            chk_usatef.IsEnabled = false;
        }
        private void chk_Maquininha_Unchecked(object sender, RoutedEventArgs e)
        {
            chk_usatef.IsEnabled = true;
        }
        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                if (VerificaConfigs())
                {
                    GravaConfiguracoes();
                    DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Falha na config.");
                }
                return;
            });
        }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
            return;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CarregaConfigs())
            {
                //TODO: ver se não está faltando algo:
                chk_Exige_Sangria.IsChecked = EXIGE_SANGRIA;
                txb_Valor_Max.Value = VALOR_MAX_CAIXA;
                chk_Bloqueia_Limite.IsChecked = BLOQUEIA_NO_LIMITE;
                txb_Valor_Folga.Value = VALOR_DE_FOLGA;
                chk_Permite_Folga.IsChecked = PERMITE_FOLGA_SANGRIA;
                txb_No_Caixa.Text = NO_CAIXA.ToString("D3");
                cbb_Pede_CPF.SelectedIndex = PEDE_CPF;
                cbb_Pede_Vend.SelectedIndex = SYSCOMISSAO;
                chk_maquininha.IsChecked = INFORMA_MAQUININHA;
                chk_Interrompe_Nao_Encontrado.IsChecked = INTERROMPE_NAO_ENCONTRADO;
                chk_Permite_Venda_Negativa.IsChecked = PERMITE_ESTOQUE_NEGATIVO;
                cbb_Mod_CUP.SelectedIndex = (int)MODELO_CUPOM;
                txb_Desc_Max.Number = DESCONTO_MAXIMO;
                chk_Recargas.IsChecked = USARECARGAS;
                txb_Cortesia.Text = MENSAGEM_CORTESIA;
                txb_Rodape.Text = MENSAGEM_RODAPE;

                if (!(bool)chk_Exige_Sangria.IsChecked)
                {
                    txb_Valor_Max.IsEnabled = false;
                }
                if (!(bool)chk_Permite_Folga.IsChecked)
                {
                    txb_Valor_Folga.IsEnabled = false;
                }
                cbb_Pede_WHATS.SelectedIndex = (int)PERGUNTA_WHATS;
                chk_usatef.IsChecked = USATEF;
                chk_pedesenha.IsChecked = PEDESENHACANCEL;
                chk_comanda.IsChecked = USA_COMANDA;
                chk_detalhadesconto.IsChecked = DETALHADESCONTO;
                chk_modobar.IsChecked = MODOBAR;
                cbb_PerguntaImpressao.SelectedIndex = SYSEMITECOMPROVANTE;
                return;
            }
            else
            {
                chk_Exige_Sangria.IsChecked = false;
                txb_Valor_Max.Value = 0;
                chk_Bloqueia_Limite.IsChecked = false;
                txb_Valor_Folga.Value = 0;
                chk_Permite_Folga.IsChecked = false;
                txb_No_Caixa.Text = "";
                cbb_Pede_CPF.SelectedIndex = 0;
                cbb_Pede_Vend.SelectedIndex = 0;
                chk_maquininha.IsChecked = false;
                chk_Interrompe_Nao_Encontrado.IsChecked = false;
                chk_Permite_Venda_Negativa.IsChecked = false;
                cbb_Mod_CUP.SelectedIndex = 0;
                txb_Desc_Max.Number = 0;
                chk_Recargas.IsChecked = false;
                txb_Cortesia.Text = "Bem vindo!";
                txb_Rodape.Text = "Obrigado e volte sempre!";
                txb_Valor_Max.IsEnabled = false;
                txb_Valor_Folga.IsEnabled = false;
                cbb_Pede_WHATS.SelectedIndex = 0;
                chk_usatef.IsChecked = false;
                cbb_PerguntaImpressao.SelectedIndex = SYSEMITECOMPROVANTE;
                return;
            }
        }

        #endregion Events

        #region Methods
        private bool VerificaConfigs()
        {
            var funcoes = new funcoesClass();
            if (txb_No_Caixa.Text.Safeint() > 99 || txb_No_Caixa.Text.Safeint() < 1)
            {
                DialogBox.Show("Falha na configuração", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Por favor digite um número entre 1 e 99.");
                return false;
            }
            int intChecagemNoCaixa = 0;
            try
            {
                using var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
                using var taConfigServ = new DataSets.FDBDataSetConfigTableAdapters.TRI_PDV_CONFIGTableAdapter
                {
                    Connection = SERVER_FB_CONN
                };
                intChecagemNoCaixa = (int)taConfigServ.ChecaPorNoCaixa(Convert.ToInt16(txb_No_Caixa.Text.Safeint()), funcoes.GetSerialHexNumberFromExecDisk());
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao verificar numeração do caixa. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                Environment.Exit(0); // deuruim();
                return false;
            }
            if (intChecagemNoCaixa > 0)
            {
                txb_No_Caixa.Focus();
                DialogBox.Show("Falha na configuração", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "O número de caixa informado já está designado a outro terminal. Por favor altere e tente novamente.");
                return false;
            }

            if ((bool)chk_Exige_Sangria.IsChecked && txb_Valor_Max.Value <= 0)
            {
                MessageBox.Show("Valor Máximo em Caixa não pode ser negativo, ou zero");
                return false;
            }
            if ((bool)chk_Permite_Folga.IsChecked && txb_Valor_Folga.Value <= 0)
            {
                MessageBox.Show("Margem de sangria deve ser maior que 0");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txb_No_Caixa.Text))
            {
                MessageBox.Show("Número de caixa não pode ser em branco");
                return false;
            }
            if (txb_Desc_Max.Number > 1 || txb_Desc_Max.Number < 0)
            {
                MessageBox.Show("Desconto deve ser entre 0% e 100%");
                return false;
            }
            return true;
        }

        private void GravaConfiguracoes()
        {
            EXIGE_SANGRIA = chk_Exige_Sangria.IsChecked ?? false;
            VALOR_MAX_CAIXA = txb_Valor_Max.Value;
            BLOQUEIA_NO_LIMITE = chk_Bloqueia_Limite.IsChecked ?? false;
            VALOR_DE_FOLGA = txb_Valor_Folga.Value;
            PERMITE_FOLGA_SANGRIA = chk_Permite_Folga.IsChecked ?? false;
            NO_CAIXA = txb_No_Caixa.Text.Safeint();
            PEDE_CPF = cbb_Pede_CPF.SelectedIndex;
            INFORMA_MAQUININHA = chk_maquininha.IsChecked ?? false;
            INTERROMPE_NAO_ENCONTRADO = chk_Interrompe_Nao_Encontrado.IsChecked ?? false;
            PERMITE_ESTOQUE_NEGATIVO = chk_Permite_Venda_Negativa.IsChecked;
            MODELO_CUPOM = ModeloCupom.Simples;
            MENSAGEM_CORTESIA = txb_Cortesia.Text;
            MENSAGEM_RODAPE = txb_Rodape.Text;
            USARECARGAS = chk_Recargas.IsChecked ?? false;
            ICMS_CONT = 0;
            CSOSN_CONT = 0;
            USATEF = chk_usatef.IsChecked ?? false;
            USA_COMANDA = chk_comanda.IsChecked ?? false;
            DETALHADESCONTO = chk_detalhadesconto.IsChecked ?? false;
            MODOBAR = chk_modobar.IsChecked ?? false;
            PEDESENHACANCEL = chk_pedesenha.IsChecked ?? false;
            SYSEMITECOMPROVANTE = cbb_PerguntaImpressao.SelectedIndex.Safeshort();
            SYSUSAWHATS = cbb_Pede_WHATS.SelectedIndex.Safeshort();
            SYSCOMISSAO = cbb_Pede_Vend.SelectedIndex.Safeshort();
            PERGUNTA_WHATS = (PerguntaWhatsEnum)cbb_Pede_WHATS.SelectedIndex;

            CONFIGURADO = true;

            if (!SalvaConfigsNaBase()) MessageBox.Show("Erro ao gravar dados.");
        }
        #endregion Methods       
    }
}
