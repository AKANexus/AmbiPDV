using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para SangSupr.xaml
    /// </summary>
    public partial class SangSupr : Window
    {
        #region Fields & Properties

        private enum modo { suprimento, sangria };
        //private modo _modo = modo.sangria; //TODO: unused
        private readonly DebounceDispatcher debounceTimer = new DebounceDispatcher();
        readonly funcoesClass funcoes = new funcoesClass();

        #endregion Fields & Properties

        #region (De)Constructor

        public SangSupr()
        {
            InitializeComponent();
            txb_Operador.Text = operador;
            lbl_SANG.FontWeight = FontWeights.Bold;
            lbl_SANG.FontSize = 20;
            txb_Caixa.Text = NO_CAIXA.ToString("000");
            txb_Valor.Text = "0,00";
            txb_Valor.Focus();
            txb_Valor.SelectAll();
        }

        #endregion (De)Constructor

        #region Events

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F2:
                    //_modo = modo.suprimento;
                    lbl_Titulo.Content = "Suprimento";
                    lbl_SANG.FontSize = 14;
                    lbl_SANG.FontWeight = FontWeights.Normal;
                    lbl_SUPR.FontWeight = FontWeights.Bold;
                    lbl_SUPR.FontSize = 20;
                    txb_Valor.Focus();
                    break;
                case Key.F3:
                    //_modo = modo.sangria;
                    lbl_Titulo.Content = "Sangria";
                    lbl_SUPR.FontSize = 14;
                    lbl_SUPR.FontWeight = FontWeights.Normal;
                    lbl_SANG.FontWeight = FontWeights.Bold;
                    lbl_SANG.FontSize = 20;
                    txb_Valor.Focus();
                    break;
                case Key.Escape:
                    this.Close();
                    break;
                default:
                    break;
            }
        }
        private void button1_Click(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                ExecutaOperacao();
            });
        }
        private void txb_Valor_KeyDown(object sender, KeyEventArgs e)
        {
            Decimal.TryParse(txb_Valor.Text, out decimal _v1);
            if (e.Key == Key.Enter && _v1 > 0)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    ExecutaOperacao();
                });
            }
        }
        private void button2_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
        private void txb_Hora_Loaded(object sender, RoutedEventArgs e)
        {
            //DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            //{
            //    this.txb_Hora.Text = DateTime.Now.ToShortTimeString();
            //}, this.Dispatcher);
        }

        #endregion Events

        #region Methods

        private void ExecutaOperacao()
        {
            Logger log = new Logger("Sangria/Sup");
            string a = "";
            string b = "";
            var c = DialogBoxIcons.None;
            switch (lbl_Titulo.Content.ToString().ToUpper())
            {
                case "SANGRIA":
                    a = "SANGRIA";
                    b = "Deseja efetuar uma SANGRIA?";
                    c = DialogBoxIcons.Sangria;
                    break;
                case "SUPRIMENTO":
                    a = "SUPRIMENTO";
                    b = "Deseja efetuar um SUPRIMENTO?";
                    c = DialogBoxIcons.Suprimento;
                    break;
                default:
                    break;
            }
            switch (DialogBox.Show(a, DialogBoxButtons.YesNo, c, false, b))
            {
                case true:
                    using (var PDV_OperTA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                    using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                    {
                        PDV_OperTA.Connection = LOCAL_FB_CONN;

                        decimal.TryParse(txb_Valor.Text, out decimal _v1);
                        var SANSUP = new PrintSANSUP
                        {
                            operacao = lbl_Titulo.Content.ToString().ToUpper(),
                            valor = _v1,
                            numcaixa = NO_CAIXA.ToString()
                        };
                        try
                        {
                            using FbCommand fbComm = new FbCommand() { CommandType = System.Data.CommandType.Text };
                            DateTime abertura = (DateTime)PDV_OperTA.GetByCaixaAberto(NO_CAIXA)[0]["CURRENTTIME"];
                            fbComm.Parameters.AddWithValue("@pID_CAIXA", NO_CAIXA);
                            fbComm.Parameters.AddWithValue("@pTS_ABERTURA", abertura);
                            fbComm.Parameters.AddWithValue("@pVALOR", _v1);
                            fbComm.Connection = LOCAL_FB_CONN;
                            if (lbl_Titulo.Content.ToString().Equals("SUPRIMENTO", StringComparison.InvariantCultureIgnoreCase))
                            {
                                fbComm.CommandText = "INSERT INTO TRI_PDV_SANSUP " +
                                                     "(ID_SANSUP, ID_CAIXA, TS_ABERTURA, OPERACAO, VALOR, TS_OPERACAO) " +
                                                     "VALUES(0, @pID_CAIXA, @pTS_ABERTURA, 'U', @pVALOR, CURRENT_TIMESTAMP);";
                                if (LOCAL_FB_CONN.State != System.Data.ConnectionState.Open)
                                    LOCAL_FB_CONN.Open();
                                fbComm.ExecuteNonQuery();
                                LOCAL_FB_CONN.Close();
                                log.Debug($"Registrado um suprimento de {_v1}");
                            }
                            else if (lbl_Titulo.Content.ToString().Equals("SANGRIA", StringComparison.InvariantCultureIgnoreCase))
                            {
                                //DateTime abertura = (DateTime)PDV_OperTA.GetByCaixaAberto(NO_CAIXA)[0]["CURRENTTIME"];
                                decimal valorEmCaixa = funcoes.CalculaValorEmCaixa(NO_CAIXA);
                                if (valorEmCaixa - _v1 < 0)
                                {
                                    log.Debug($"Tentou-se fazer uma sangria de R${_v1} com R${valorEmCaixa} em caixa.");
                                    DialogBox.Show("Valor Inválido", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Sangria requisitada é maior do que o dinheiro disponível em caixa.");
                                    return;
                                }
                                fbComm.CommandText = "INSERT INTO TRI_PDV_SANSUP " +
                                                     "(ID_SANSUP, ID_CAIXA, TS_ABERTURA, OPERACAO, VALOR, TS_OPERACAO) " +
                                                     "VALUES(0, @pID_CAIXA, @pTS_ABERTURA, 'A', @pVALOR, CURRENT_TIMESTAMP);";
                                if (LOCAL_FB_CONN.State != System.Data.ConnectionState.Open)
                                    LOCAL_FB_CONN.Open();
                                fbComm.ExecuteNonQuery();
                                LOCAL_FB_CONN.Close();
                                log.Debug($"Registrado uma sangria de {_v1}");
                            }
                            //Lógica pra efetuar o procedimento e salvar no banco de dados.
                        }
                        catch (Exception ex)
                        {
                            log.Error("Erro ao gravar operação de sangria/suprimento", ex);
                            MessageBox.Show(ex.Message);
                            return;
                        }
                        var via = SANSUP.IMPRIME();//Imprime a primeira via,
                        var viadois = SANSUP.IMPRIME();//e a segunda.

                        if (!via || !viadois)
                        {
                            DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.Yes, DialogBoxIcons.Info, false, "A impressão falhou, porém o suprimento FOI contabilizado.", "Sr(a). Operador(a), favor anotar o valor do suprimento efetuado.", $"Suprimento: {SANSUP.valor:C2}.");
                        }
                        this.Close();
                        break;
                    }
                case false:
                    txb_Valor.Text = "0,00";
                    txb_Valor.Focus();
                    break;
            }
        }

        #endregion Methods
    }
}
