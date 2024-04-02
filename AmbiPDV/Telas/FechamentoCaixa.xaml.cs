using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using System.Threading;
using System.Threading.Tasks;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for FechamentoCaixa.xaml
    /// </summary>
    public partial class FechamentoCaixa : Window
    {
        #region Fields & Properties

        Logger log = new Logger("Fechamento de Caixa");
        private Dictionary<string, decimal> valores = new Dictionary<string, decimal>();
        public decimal total { get; set; }
        public decimal _dinheiro { get; set; }
        public decimal _cheque { get; set; }
        public decimal _credito { get; set; }
        public decimal _debito { get; set; }
        public decimal _valeloja { get; set; }
        public decimal _alimentacao { get; set; }
        public decimal _refeicao { get; set; }
        public decimal _presente { get; set; }
        public decimal _combustivel { get; set; }
        public decimal _pix { get; set; }
        public decimal _outros { get; set; }
        public decimal _SANG { get; set; }
        public decimal _SUP { get; set; }
        public decimal _TROCA { get; set; }

        DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable METODOS_DT = new DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable();
        private DateTime _abertura;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        public bool ImpRelX = false;
        #endregion Fields & Properties

        #region (De)Constructor

        public FechamentoCaixa(DateTime abertura)
        {
            log.Debug($"FechamentoCaixa(abertura:) {abertura}");
            _abertura = abertura;
            InitializeComponent();
            using (var METODOS_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                METODOS_TA.Connection = LOCAL_FB_CONN;
                METODOS_TA.FillByAtivos(METODOS_DT);
                //var statuses = (from linha in METODOS_DT
                //                               select linha.ID_NFCE, linha.STATUS);
                var statuses = METODOS_DT.Select(x => new { COD_CFE = x.ID_NFCE, x.STATUS, x.DESCRICAO });
                foreach (var item in statuses)
                {
                    switch (item.COD_CFE)
                    {
                        case "01":
                            txb_Dinheiro.Visibility = lbl_dinheiro.Visibility = Visibility.Visible;
                            lbl_dinheiro.Content = item.DESCRICAO;
                            break;
                        case "02":
                            txb_Cheque.Visibility = lbl_cheque.Visibility = Visibility.Visible;
                            lbl_cheque.Content = item.DESCRICAO;
                            break;
                        case "03":
                            if (!USATEF) txb_Credito.Visibility = lbl_credito.Visibility = Visibility.Visible;
                            lbl_credito.Content = item.DESCRICAO;
                            break;
                        case "04":
                            if (!USATEF) txb_Debito.Visibility = lbl_debito.Visibility = Visibility.Visible;
                            lbl_debito.Content = item.DESCRICAO;
                            break;
                        case "05":
                            txb_Vale.Visibility = lbl_vale.Visibility = Visibility.Visible;
                            lbl_vale.Content = item.DESCRICAO;
                            break;
                        case "10":
                            if (!USATEF) txb_Alimentacao.Visibility = lbl_alimentacao.Visibility = Visibility.Visible;
                            lbl_alimentacao.Content = item.DESCRICAO;
                            break;
                        case "11":
                            if (!USATEF) txb_Refeicao.Visibility = lbl_refeicao.Visibility = Visibility.Visible;
                            lbl_refeicao.Content = item.DESCRICAO;
                            break;
                        case "12":
                            txb_Presente.Visibility = lbl_presente.Visibility = Visibility.Visible;
                            lbl_presente.Content = item.DESCRICAO;
                            break;
                        case "13":
                            txb_Combustivel.Visibility = lbl_combustivel.Visibility = Visibility.Visible;
                            lbl_combustivel.Content = item.DESCRICAO;
                            break;
                        case "17":
                            if(!USATEF) txb_Pix.Visibility = lbl_pix.Visibility = Visibility.Visible;
                            lbl_pix.Content = item.DESCRICAO;
                            break;
                        case "99":
                            txb_Outros.Visibility = lbl_outros.Visibility = Visibility.Visible;
                            lbl_outros.Content = item.DESCRICAO;
                            break;
                        default:
                            break;
                    }
                }
            }
            //frm_FechamentoCaixa.Height = SystemParameters.PrimaryScreenHeight - 100;
            frm_FechamentoCaixa.MaxHeight = SystemParameters.PrimaryScreenHeight - 100;
            switch (USARECARGAS)
            {
                case true:
                    txb_San.EnterToMoveNext = true;
                    txb_San.KeyDown -= txb_Total_KeyDown;
                    txb_Recargas.Visibility = Visibility.Visible;
                    lbl_recargas.Visibility = Visibility.Visible;
                    break;
                case false:
                default:

                    break;
            }
            lbl_Terminal.Content = NO_CAIXA.ToString("000");
            lbl_Date.Content = DateTime.Now.ToShortDateString();
            txb_Dinheiro.Focus();

        }

        #endregion (De)Constructor

        #region Events
        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                fecha_o_caixa();
            });
        }
        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            if (DialogBox.Show("Fechamento de Caixa", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja cancelar o fechamento? Será necessário digitar a senha novamente.") == true)
            {
                DialogResult = false;
                this.Close();
            }
        }
        private void relatorioX_Click(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                relatorio_x();
            });
        }             
        private void txb_Total_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {                    
                    fecha_o_caixa(); // deuruim();
                });
            }
        }

        private void txb_Troca_LostFocus(object sender, RoutedEventArgs e)
        {
            atualiza_valores();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Atualiza as caixas de textos com os valores das variáveis
        /// </summary>       
        private void atualiza_valores()
        {
            _dinheiro = txb_Dinheiro.Value;
            _cheque = txb_Cheque.Value;
            _credito = txb_Credito.Value;
            _debito = txb_Debito.Value;
            _valeloja = txb_Vale.Value;
            _alimentacao = txb_Alimentacao.Value;
            _refeicao = txb_Refeicao.Value;
            _presente = txb_Presente.Value;
            _combustivel = txb_Combustivel.Value;
            _pix = txb_Pix.Value;
            _outros = txb_Outros.Value;
            _pix = txb_Pix.Value;
            _SANG = txb_San.Value;
            _SUP = txb_Sup.Value;
            _TROCA = txb_Troca.Value;
            total = _dinheiro + _cheque + _credito + _debito + _valeloja + _alimentacao + _refeicao + _presente + _combustivel + _outros + _pix;
            txb_Total.Value = total;
        }
        private bool fecha_o_caixa()
        {
            using (var Impressao = new PrintFECHA())
            {
                atualiza_valores();
                switch (DialogBox.Show("Fechamento do caixa", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja executar o fechamento do caixa?"))
                {
                    case true:
                        TimedBox dialog = new TimedBox("Fechamento", "Realizando a contagem dos valores em caixa, aguarde ...", TimedBox.DialogBoxButtons.No, TimedBox.DialogBoxIcons.None, 100);
                        dialog.Show();
                        DataRow metodo_pgto_col;
                        metodo_pgto_col = Impressao.fecha_infor_dt.NewRow();
                        metodo_pgto_col[0] = -1;
                        metodo_pgto_col["DIN"] = _dinheiro;
                        metodo_pgto_col["CHEQUE"] = _cheque;
                        metodo_pgto_col["CREDITO"] = _credito;
                        metodo_pgto_col["DEBITO"] = _debito;
                        metodo_pgto_col["LOJA"] = _valeloja;
                        metodo_pgto_col["ALIMENTACAO"] = _alimentacao;
                        metodo_pgto_col["REFEICAO"] = _refeicao;
                        metodo_pgto_col["PRESENTE"] = _presente;
                        metodo_pgto_col["COMBUSTIVEL"] = _combustivel;
                        metodo_pgto_col["OUTROS"] = _outros;
                        metodo_pgto_col["EXTRA_1"] = _pix;
                        metodo_pgto_col["EXTRA_2"] = 0;
                        metodo_pgto_col["EXTRA_3"] = 0;
                        metodo_pgto_col["EXTRA_4"] = 0;
                        metodo_pgto_col["EXTRA_5"] = 0;
                        metodo_pgto_col["EXTRA_6"] = 0;
                        metodo_pgto_col["EXTRA_7"] = 0;
                        metodo_pgto_col["EXTRA_8"] = 0;
                        metodo_pgto_col["EXTRA_9"] = 0;
                        metodo_pgto_col["EXTRA_10"] = 0;
                        metodo_pgto_col["CURRENTTIME"] = DateTime.Now;
                        metodo_pgto_col["ABERTO"] = "X";
                        metodo_pgto_col["HASH"] = "X";
                        metodo_pgto_col["SANGRIAS"] = _SANG;
                        metodo_pgto_col["SUPRIMENTOS"] = _SUP;
                        metodo_pgto_col["TROCAS"] = _TROCA;
                        metodo_pgto_col["ID_OPER"] = 0;
                        metodo_pgto_col["ID_USER"] = 0;
                        Impressao.fecha_infor_dt.Rows.Add(metodo_pgto_col);
                        using (var EMIT_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter())
                        using (var EMIT_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable())
                        using (var USERS_TA = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
                        using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                        using (var Oper = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                        {
                            EMIT_TA.Connection = LOCAL_FB_CONN;
                            Oper.Connection = LOCAL_FB_CONN;
                            USERS_TA.Connection = LOCAL_FB_CONN;
                            EMIT_TA.Fill(EMIT_DT);
                            Impressao.cnpjempresa = EMIT_DT[0].CNPJ.TiraPont();
                            Impressao.nomefantasia = EMIT_DT[0].NOME_FANTA.Safestring();
                            Impressao.enderecodaempresa = string.Format("{0} {1}, {2} - {3}, {4}",
                                                                        EMIT_DT[0].END_TIPO,
                                                                        EMIT_DT[0].END_LOGRAD,
                                                                        EMIT_DT[0].END_NUMERO,
                                                                        EMIT_DT[0].END_BAIRRO,
                                                                        "São Paulo"); //TODO: CIDADE ESTÁ CONSTANTE
                            int userid = USERS_TA.PegaIdPorUser(operador).Safeint();
                            switch (USARECARGAS)
                            {
                                case true:
                                    Impressao.val_recargas = txb_Recargas.Value;
                                    break;
                                case false:
                                default:
                                    break;
                            }
                            log.Debug($"ImprimeFechamento() {_abertura}");
                            if (!Impressao.IMPRIME(DateTime.MinValue, METODOS_DT, NO_CAIXA)) // deuruim()
                            {
                                dialog.Close();
                                DialogResult = false;
                                this.Close();
                                return false;
                            }
                            log.Debug($"SP_TRI_FECHACAIXA() {_abertura}");
                            Oper.SP_TRI_FECHACAIXA(NO_CAIXA,
                                                   _dinheiro, _cheque, _credito, _debito, _valeloja, _alimentacao, _refeicao, _presente, _combustivel, _outros, _pix, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                                   _TROCA, _SUP, _SANG, userid, _abertura);
                            log.Debug($"Foi registrado o fechamento do caixa {NO_CAIXA} pelo operador {operador}");
                        }
                        dialog.Close();
                        DialogResult = true;                        
                        this.Close();
                        return true;
                    case false:
                        txb_Dinheiro.Focus();
                        break;
                    default:
                        break;
                }
            }
            return false;
        }
        private void relatorio_x()
        {
            if ((DialogBox.Show("Relatório X", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja realizar a impressão do Relatório X?\n Obs:. o turno não será fechado.") == true))
            {
                TimedBox dialog = new TimedBox("Relatório X", "Realizando a contagem dos valores em caixa, aguarde ...", TimedBox.DialogBoxButtons.No, TimedBox.DialogBoxIcons.None, 100);
                dialog.Show();

                using (var EMIT_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter())
                using (var EMIT_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable())
                using (var Impressao = new PrintFECHA())
                {
                    EMIT_TA.Fill(EMIT_DT);
                    Impressao.cnpjempresa = EMIT_DT[0].CNPJ.TiraPont();
                    Impressao.nomefantasia = EMIT_DT[0].NOME_FANTA.Safestring();
                    Impressao.enderecodaempresa = string.Format("{0} {1}, {2} - {3}, {4}",
                                                                EMIT_DT[0].END_TIPO,
                                                                EMIT_DT[0].END_LOGRAD,
                                                                EMIT_DT[0].END_NUMERO,
                                                                EMIT_DT[0].END_BAIRRO,
                                                                "São Paulo");
                    if (!Impressao.IMPRIME(DateTime.MinValue, METODOS_DT, NO_CAIXA, false, true))
                    {
                        DialogBox.Show("Relatório X", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao calcular e/ou imprimir relatório X\n" +
                                       "Se o problema persistir entre em contato com o suporte técnico.");
                    }
                    dialog.Close();
                    ImpRelX = true;
                    DialogResult = true;                    
                    this.Close();
                }
            }
        }

        #endregion Methods
        private void Frm_FechamentoCaixa_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && (DialogBox.Show("Fechamento de Caixa", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja cancelar o fechamento? Será necessário digitar a senha novamente.") == true))
            {
                DialogResult = false;
                this.Close();
            }
            else if(e.Key == Key.R && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                but_RelatorioX.Visibility = Visibility.Visible;
            }
        }       
    }

    public class CurrencyTextBox : CurrencyTextBoxControl.CurrencyTextBox
    {
        public bool EnterToMoveNext { get; set; } = true;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            var element = this as UIElement;
            if (e.Key == Key.Enter && EnterToMoveNext)
            {
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            base.OnKeyDown(e);
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Char.IsDigit(GetCharFromKey(e.Key)))
            {
                var Element = this as CurrencyTextBox;
                if (Element.SelectionLength == Element.Text.Length)
                {
                    Element.Number = 0;
                }
            }

            base.OnPreviewKeyDown(e);
        }
        public decimal Value
        {
            get
            {
                return Number;
            }
            set
            {
                Number = value;
            }
        }
    }
}
