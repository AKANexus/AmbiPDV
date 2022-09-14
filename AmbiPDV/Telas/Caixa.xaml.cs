using Balancas;
using CfeRecepcao_0007;
using Clearcove.Logging;
using DeclaracoesDllSat;
using FirebirdSql.Data.FirebirdClient;
using LocalDarumaFrameworkDLL;
using PayGo;
using PDV_WPF.Controls;
using PDV_WPF.DataSets.FDBDataSetOperSeedTableAdapters;
using PDV_WPF.FDBDataSetTableAdapters;
using PDV_WPF.Funcoes;
using PDV_WPF.Objetos;
using PDV_WPF.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using PDV_WPF.REMENDOOOOO;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.SiTEFDLL;
using static PDV_WPF.Funcoes.Statics;
using ECF = PDV_WPF.FuncoesECF;
using WinForms = System.Windows.Forms;

namespace PDV_WPF.Telas
{
    public partial class Caixa : Window
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IsIconic(IntPtr handle);

        private const int SW_RESTORE = 9;

        #region Block System Keys
        private static bool allowkeys;
        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _hookID = InterceptKeys.SetHook(_proc);
        }
        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool alt = (WinForms.Control.ModifierKeys & WinForms.Keys.Alt) != 0;
                bool control = (WinForms.Control.ModifierKeys & WinForms.Keys.Control) != 0;

                int vkCode = Marshal.ReadInt32(lParam);
                WinForms.Keys key = (WinForms.Keys)vkCode;

                if (alt && key == WinForms.Keys.F4 && !allowkeys)
                {
                    //DESCOMENTAR PARA BLOQUEAR Alt+F4
                    return (IntPtr)1; // Handled.
                }

                if (!AllowKeyboardInput(alt, control, key) && !allowkeys)
                {
                    //DESCOMENTAR PARA BLOQUEAR ALT+TAB
                    return (IntPtr)1; // Handled.
                }
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        public static bool AllowKeyboardInput(bool alt, bool control, WinForms.Keys key)
        {
            // Disallow various special keys.
            if (key < WinForms.Keys.Back || key == WinForms.Keys.None || key == WinForms.Keys.Menu || key == WinForms.Keys.Pause || key == WinForms.Keys.Help)
            {
                return false;
            }
            // Disallow ranges of special keys.
            // Currently leaves volume controls enabled; consider if this makes sense.
            // Disables non-existing Keys up to 65534, to err on the side of caution for future keyboard expansion.
            if (key >= WinForms.Keys.LWin && key <= WinForms.Keys.Sleep ||
                key >= WinForms.Keys.KanaMode && key <= WinForms.Keys.HanjaMode ||
                key >= WinForms.Keys.IMEConvert && key <= WinForms.Keys.IMEModeChange ||
                key >= WinForms.Keys.BrowserBack && key <= WinForms.Keys.BrowserHome ||
                key >= WinForms.Keys.MediaNextTrack && key <= WinForms.Keys.LaunchApplication2 ||
                key >= WinForms.Keys.ProcessKey && key <= (WinForms.Keys)65534)
            {
                return false;
            }

            // Disallow specific key combinations. (These component keys would be OK on their own.)
            if (alt && key == WinForms.Keys.Space ||
                control && key == WinForms.Keys.Escape)
            {
                return false;
            }
            if (alt && key == WinForms.Keys.Tab)
            {
                return false;
            }

            // Allow anything else (like letters, numbers, spacebar, braces, and so on).
            return true;
        }
        #endregion Block System Keys

        #region Fields & Properties
        private readonly List<Key> konami = new();
        private readonly List<Key> nghtmd = new() { Key.Up, Key.Up, Key.Down, Key.Down, Key.Left, Key.Right, Key.Left, Key.Right, Key.B, Key.B, Key.A, Key.A };
        private readonly Regex rgx = new(@"(\d+\*)");
        public string numeroWhats;//HACK
        private enum tipoDesconto { Nenhum, Absoluto, Percentual }
        private enum statusSangria { Normal, Folga, Excesso }

        private readonly DebounceDispatcher debounceTimer = new();
        private Dictionary<string, string> oldCRT = new();
        private bool _prepesado,
            _usouOS,
            _usouOrcamento,
            _nightmode = false,
            _usouPedido,
            erroVenda,            
            _modoDevolucao,
            _emTransacao,
            _modo_consulta,
            _painelFechado = true,
            _modoTeste,
            _interromperModoTeste;
        public static bool _contingencia;

        //private PerguntaWhatsEnum _modoWhats;

        private bool turno_aberto;

        //private bool atalho_whats = false;

        private readonly funcoesClass funcoes = new();

        private readonly Orcamento orcamentoAtual = new();
        private CLIPP_OS ordemDeServico = new();

        private readonly Pedido pedidoAtual = new();

        private int maitNrPedido, noCupom, timeKeepAliveSAT, numProximoItem = 1;

        private readonly int indentdaMargem = 0;

        private int? vendedorId;

        private decimal desconto, subtotal;

        private string infoStr, vendedorString, total1, total2;

        private (string sequencial, string datafiscal, string horafiscal) infoAdminTEF;

        private readonly NumSessao ns = new();

        private readonly StringBuilder cupomVirtual = new();

        private Logger log = new("Caixa");



        public static Venda vendaAtual;
        private Devolucao devolAtual;
        //private OperTEF tefAtual;

        private PrintDocument ultimaImpressao;
        //private envCFeCFeInfCFeIde ide;

        public Configs configuracoes;

        private DateTime ultimaContingencia = DateTime.Now;
        private udx_pdv_oper_class udx_pdv_oper = new();

        private tipoDesconto tipoDeDesconto;
        private ItemChoiceType _tipo = ItemChoiceType.FECHADO;

        private List<ComboBoxBindingDTO_Produto_Sync> _lstProdutosAlteradosSync;

        #endregion Fields & Properties

        #region (De)Constructor

        public Caixa(bool _contingencia)
        {           
            DataContext = mvm;
            var args = new List<string>();
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                args.Add(arg.ToLower());
            }
            if (args.Contains("/developer") == true)
            {
                allowkeys = true;
            }
            try
            {
                InicializarCaixa(_contingencia);
                combobox.MultiplyAdded += Combobox_MultiplyAdded;                
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void Combobox_MultiplyAdded(object sender, EventArgs e)
        {
            AplicarSelecaoDeQuantidade();
        }

        #endregion (De)Constructor

        #region Events

        /// <summary>
        /// Processa informação digitada na caixa de texto.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ACBox_KeyDown(object sender, KeyEventArgs e)
        {
            //konami.Add(e.Key);
            //if (konami.Count > 12)
            //{
            //    konami.RemoveAt(0);
            //}

            //if (konami.SequenceEqual(nghtmd))
            //{
            //    AlternarModoEscuro();
            //}
            try
            {
                if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(combobox.Text))
                {
                    ProcessarTextoNoACBox();
                }
            }
            catch (Exception ex)
            {
                DialogBox.Show(strings.LANCAMENTO_DE_PRODUTO, DialogBoxButtons.No, DialogBoxIcons.Error, true, RetornarMensagemErro(ex, false));
                log.Error("Lançamento de Produto", ex);
                return;
            }
            //if (e.Key == Key.Down && !ACBox.IsDropDownOpen)
            //{
            //    e.Handled = true;
            //    return;
            //}
        }

        #region Teclas de Atalho

        void ExecutaTeste()
        {
            var pararTeste = new DialogBox(strings.AUTOTESTE, strings.CLIQUE_PARA_INTERROMPER_AUTOTESTE, DialogBoxButtons.Yes, DialogBoxIcons.None, false);
            pararTeste.Closed += PararTeste_Closed;
            pararTeste.ShowDialog();
            DialogBox.Show(strings.AUTOTESTE, DialogBoxButtons.Yes, DialogBoxIcons.None, true, strings.AUTOTESTE_CONCLUIDO_COM_SUCESSO);
        }

        /// <summary>
        /// Cancela a venda e limpa todos os textbox para o modo normal
        /// </summary>
        private void CancelarVendaAtual()
        {
            if (_modo_consulta) AlternarModoDeConsulta();
            log.Debug($"Cancelando a venda atual, sem informar pagamentos.");
            //LIMPAR OBJETO DE VENDA
            vendaAtual?.Clear();
            devolAtual?.Clear();


            //LIMPAR A TELA
            LimparUltimaVenda();
            LimparTela();
            _usouOrcamento = _usouPedido = _usouOS = false; //Caso o cliente "desista" da venda do orçamento limpando os produtos do cupom e posteriormente pressionando F2 ou F3
            orcamentoAtual.Clear();
            pedidoAtual?.Clear();
            LimparObjetoDeVendaNovo();


            //CANCELAR TEFs EFETUADOS
            //if (tefAtual.emVenda)
            //{
            //    tefAtual.DesfazVendaAtual();
            //}
        }
        /// <summary>
        /// Interrompe os testes
        /// </summary>
        private void PararTeste_Closed(object sender, EventArgs e)
        {
            _interromperModoTeste = true;
        }

        #endregion

        #region Botões Desativados

        private void but_F3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PrepararFinalizacaoDeCupomFiscal();
            return;
        }
        private void but_F4_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RemoverItemDaVendaNovo();
            return;
        }
        private void but_F5_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AlternarModoDeConsulta();
            return;
        }
        private void but_F6_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CancelarUltimoCupom();
            return;
        }
        private void but_F7_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AlternarModoDevolucao();
            return;
        }
        private void but_F8_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AlternarDescontoNoItem();
            return;
        }
        private void but_F11_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AbrirJanelaSangriaSupr();
            return;
        }
        private void but_F12_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (turno_aberto)
            {
                case true:
                    if (ExecFechamentoCaixa())
                    {
                        //MessageBox.Show("DEBUG - CAIXA FECHADO!");
                        DialogBox.Show(strings.FECHAMENTO_DE_TURNO, DialogBoxButtons.Yes, DialogBoxIcons.Info, false, strings.TURNO_FECHADO_COM_SUCESSO);
                    }
                    break;
                case false:
                    if (ExecAberturaCaixa())
                    {
                        DialogBox.Show(strings.ABERTURA_DE_TURNO, DialogBoxButtons.Yes, DialogBoxIcons.Info, false, strings.TURNO_ABERTO_COM_SUCESSO);                        //MessageBox.Show("DEBUG - CAIXA ABERTO!");
                    }
                    break;
            }
            return;
        }

        #endregion

        private void ACBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FocusableAutoCompleteBox box = (FocusableAutoCompleteBox)sender;
            ListBox innerListBox = (ListBox)box.Template.FindName("Selector", box);
            innerListBox.ScrollIntoView(innerListBox.SelectedItem);
        }//AUtoscroll no autocomplete da caixa de pesquisa.

        /// <summary>
        /// Evento de inicialização da tela, também tem a função de ajustar a tela
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var tela = WinForms.Screen.PrimaryScreen.Bounds;
            if (tela.Width == 1024 && tela.Height == 768)
            {
                log.Debug("Tela muito pequena detectada. Ajustando interface");
                txb_TotGer.FontSize = 43;
                Canvas_Menu.Visibility = Visibility.Collapsed;
                lbl_Operador.Margin = new Thickness(-70, 5, -70, 5);
                richTextBox1.Margin = new Thickness(0, 10, 40, 10);
                richTextBox1.FontSize = 22.6;
            }



            combobox.Focus();
        }

        #region Função Desativada
        [Obsolete("Esse metodo será removido na próxima Major", true)]
        private void métodosDePagamentoToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        #endregion
        /// <summary>
        /// Fecha o programa com gentileza
        /// </summary>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Application.Current.Shutdown();
        }

        private void Lbl_Operador_MouseEnter(object sender, MouseEventArgs e)
        {
            //lbl_Operador.Content = string.Format(strings.VOCE_ESTA_SENDO_ATENDIDO_POR, operador.Split(' ')[0]);
        }

        private void Lbl_Operador_MouseLeave(object sender, MouseEventArgs e)
        {
            //var rnd = new Random();
            //lbl_Operador.Content = string.Format(strings.VOCE_ESTA_SENDO_ATENDIDO_POR, funcoes.eegg[rnd.Next(0, funcoes.eegg.Count)]);
        }
        private void Tef_StatusChanged(object sender, TEFEventArgs e)
        {
            var printTEFAdmin = new ComprovanteSiTEF();
            if (!(e.viaCliente is null) && e.viaCliente.Count > 0)
            {
                //CupomTef = e.viaCliente;
                printTEFAdmin.IMPRIME(e.viaCliente);
            }
            if (!(e.viaLoja is null) && e.viaLoja.Count > 0)
            {
                printTEFAdmin.IMPRIME(e.viaLoja);
            }
            FinalizaFuncaoSiTefInterativo(1, infoAdminTEF.sequencial, infoAdminTEF.datafiscal, infoAdminTEF.horafiscal, $"NumeroPagamentoCupom={e.idMetodo}");
            _emTransacao = false;
        }

        #endregion Events

        #region Methods

        #region TESTES

        //private void IniciarTestes()
        //{
        //    TESTE_Safedecimal(double.NaN);
        //    //TESTE_Safedecimal(2.5);

        //    //using (var taEstoquePdv = new TB_ESTOQUETableAdapter())
        //    //using (var taEstProdutoPdv = new TB_EST_PRODUTOTableAdapter())
        //    //{
        //    //    taEstoquePdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();
        //    //    taEstProdutoPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();
        //    //    TESTE_CEST_processaItem(134, 1, 1, 1, taEstoquePdv, taEstProdutoPdv); // com CEST
        //    //    TESTE_CEST_processaItem(2, 1, 1, 1, taEstoquePdv, taEstProdutoPdv); // sem CEST
        //    //}
        //}

        //private decimal TESTE_Safedecimal(object vDado)
        //{
        //    decimal vResult = 0;

        //    if (vDado == null) return vResult;

        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(vDado.ToString())) return vResult;
        //        //if (double.IsNaN(vDado)) return vResult; //TODO: Não é necessário, pois a conversão a seguir já pode estourar e resultar em zero.

        //        vResult = Convert.ToDecimal(vDado);
        //    }
        //    catch
        //    {
        //        vResult = 0;
        //    }

        //    return vResult;
        //}

        ///// <summary>
        ///// TESTE CEST - Processa o item que o sistema identificou através do código interno e o adiciona tanto ao cupom virtual quanto ao cupom fiscal
        ///// </summary>
        ///// <param name="codigo_item">Código interno do item</param>
        ///// <param name="p_unit">Preço unitário do item</param>
        ///// <param name="qntd">Quantidade a ser inserida nos cupons</param>
        ///// <param name="p_venda">Preço cadastrado no sistema</param>
        //private void TESTE_CEST_processaItem(int codigo_item, decimal p_unit, decimal qntd, decimal p_venda, TB_ESTOQUETableAdapter _ESTOQUE_TA, TB_EST_PRODUTOTableAdapter _EST_PRODUTO_TA)
        //{
        //    //TODO: organizar os objetos na ordem que estiver determinado no documento de especificação do SAT.

        //    var det = new envCFeCFeInfCFeDet();
        //    var produto = new envCFeCFeInfCFeDetProd();
        //    var imposto = new envCFeCFeInfCFeDetImposto();
        //    var icms = new envCFeCFeInfCFeDetImpostoICMS();
        //    var icmssn102 = new envCFeCFeInfCFeDetImpostoICMSICMSSN102();
        //    var ofd = new envCFeCFeInfCFeDetProdObsFiscoDet(); //TODO: CEST preenchido aqui até 30.06.18. Após isso, deverá ser preenchido em envCFeCFeInfCFeDetProd.CEST
        //    var PIS = new envCFeCFeInfCFeDetImpostoPIS();
        //    var PISOutr = new envCFeCFeInfCFeDetImpostoPISPISOutr();
        //    var COFINS = new envCFeCFeInfCFeDetImpostoCOFINS();
        //    var COFINSOutr = new envCFeCFeInfCFeDetImpostoCOFINSCOFINSOutr();

        //    //det.nItem = itematual.ToString();
        //    produto.cProd = codigo_item.ToString();
        //    produto.xProd = _ESTOQUE_TA.DescricaoPorID(codigo_item); //TODO: em vez de usar 1 método para cada campo, tentar buscar um DataRow já contendo todas as infos necessárias.
        //    produto.NCM = _EST_PRODUTO_TA.NCMPorCOD(codigo_item);
        //    produto.CFOP = _ESTOQUE_TA.CFOPPorCod(codigo_item);
        //    produto.uCom = _ESTOQUE_TA.TipoDeItem(codigo_item).ToString();

        //    #region Pegar peso
        //    //if (prepesado == false && (produto.uCom == "KG" || produto.uCom == "KU"))
        //    //{
        //    //    try
        //    //    {
        //            //balanca();
        //            qntd = Convert.ToDecimal(1.0000);
        //    //    }
        //    //    catch (SemBalanca)
        //    //    {

        //    //    }
        //    //    catch (Exception erro)
        //    //    {
        //    //        DialogBox err = DialogBox.Show("Erro ao obter peso", RetornarMensagemErro(erro, false), DialogBoxButtons.No, DialogBoxIcons.Error);
        //    //        err.ShowDialog();
        //    //        gravarMensagemErro(RetornarMensagemErro(erro, true));
        //    //        return;
        //    //    }
        //    //}
        //    #endregion Pegar peso

        //    produto.qCom = qntd.ToString("0.0000");//"1.0000";
        //    produto.vUnCom = p_venda.ToString("0.000");
        //    produto.vDesc = (p_venda - p_unit).ToString("0.00");

        //    #region CEST
        //    // Verificar se o item tem CEST. Preenche se tiver.
        //    string strCest = (string)_EST_PRODUTO_TA.SP_TRI_ESTPROD_GETCEST_IDENTIF(codigo_item);
        //    if (!string.IsNullOrWhiteSpace(strCest))
        //    {
        //        ofd.xCampoDet = "Cod. CEST"; //TODO: testar vendas SAT de itens com e sem CEST na mesma venda
        //        ofd.xTextoDet = strCest.ToString();

        //        // Instancia o vetor:
        //        envCFeCFeInfCFeDetProdObsFiscoDet[] obj = new envCFeCFeInfCFeDetProdObsFiscoDet[1];
        //        obj[0] = ofd;
        //        produto.obsFiscoDet = obj;
        //    }
        //    #endregion CEST

        //    produto.indRegra = "A";
        //    icmssn102.Orig = "0";
        //    icmssn102.CSOSN = "500";
        //    PISOutr.CST = "99";
        //    PISOutr.ItemsElementName = new CfeRecepcao_0007.ItemsChoiceType[] { ItemsChoiceType.vBC, ItemsChoiceType.pPIS };
        //    PISOutr.Items = new string[] { "0.00", "0.0000" };
        //    COFINSOutr.CST = "99";
        //    COFINSOutr.ItemsElementName = new ItemsChoiceType2[] { ItemsChoiceType2.vBC, ItemsChoiceType2.pCOFINS };
        //    COFINSOutr.Items = new string[] { "0.00", "0.0000" };
        //    icms.Item = icmssn102;
        //    PIS.Item = PISOutr;
        //    imposto.PIS = PIS;
        //    COFINS.Item = COFINSOutr;
        //    imposto.COFINS = COFINS;
        //    det.imposto = imposto;
        //    imposto.Item = icms;
        //    det.prod = produto;
        //    //lbl_Marquee.Visibility = Visibility.Hidden;
        //    //lbl_Cortesia.Content = _ESTOQUE_TA.DescricaoPorID(codigo_item);
        //    //txb_ValorUnit.Text = p_unit.ToString("C2");
        //    //txb_TotProd.Text = (p_unit * qntd).ToString("C2");
        //    subtotal += p_unit * qntd;
        //    //txb_TotGer.Text = subtotal.ToString("C2");
        //    lista_dets.Add(det);
        //    //ACBox.Text = "";
        //    //txb_Qtde.Clear();
        //    string barcode = null;
        //    //if (prepesado == false)
        //    {
        //        try
        //        {
        //            barcode = _EST_PRODUTO_TA.CodBarras(codigo_item);
        //        }
        //        catch (Exception)
        //        {
        //            barcode = codigo_item.ToString();
        //        }
        //    }
        //    //else { barcode = codigo_item.ToString(); }

        //    prepesado = false;
        //    //if (barcode == null) { ImprimeCV(string.Format(@"{0} {1} {2} ", det.nItem.PadLeft(3, '0'), produto.cProd.PadLeft(13, '0'), produto.xProd.Trunca(27))); }
        //    //else { ImprimeCV(string.Format(@"{0} {1} {2} ", det.nItem.PadLeft(3, '0'), barcode.PadLeft(13, '0'), produto.xProd.Trunca(27))); }
        //    //ImprimeCV(string.Format(@"{0} {1} {2} {3}", produto.qCom.Trunca(5).PadLeft(8, ' '), produto.uCom, p_venda.ToString("0.00").PadLeft(10, ' '), (p_unit * qntd).ToString("0.00").PadLeft(20, ' ')));

        //    string path = AppDomain.CurrentDomain.BaseDirectory + "\\Ultimavenda.txt";
        //    if (!File.Exists(path))
        //    {
        //        using (StreamWriter sw = File.CreateText(path))
        //        {
        //            sw.WriteLine("Arquivo gerado automaticamente pelo AmbiPDV. Não remova.\n\r");
        //        }
        //    }
        //    using (StreamWriter sw = File.AppendText(path))
        //    {
        //        sw.WriteLine(qntd.ToString() + "|" + codigo_item);
        //    }

        //}//Lança o item no objeto de venda.

        #endregion TESTES

        /// <summary>
        /// Abre a janela de consulta avançada.
        /// </summary>
        private void AbrirConsultaAvancada()
        {
            var ca = new ConsultaAvancada(_contingencia);
            ca.ShowDialog();
            if (ca.DialogResult == true)
            {
                combobox.Text = ca.codigo.ToString();
            }
            if (_emTransacao == false) { lbl_Marquee.Visibility = Visibility.Visible; }
            lbl_Cortesia.Content = null;
            txb_ValorUnit.Clear();
            //txb_Qtde.Clear(); //TODO: a quantidade deve ser apagada mesmo?
            txb_TotGer.Text = total1;
            txb_TotProd.Text = total2;
            _modo_consulta = false;
            if (_nightmode)
            {
                txb_Avisos.Foreground = txb_TotGer.Foreground = txb_TotProd.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                txb_Avisos.Foreground = txb_TotGer.Foreground = txb_TotProd.Foreground = (Brush)new BrushConverter().ConvertFrom("#FF333333");
            }
            if (!turno_aberto)
            {
                txb_Avisos.Text = "CAIXA FECHADO";
            }
            else
            {
                if (_emTransacao)
                {
                    txb_Avisos.Text = "CUPOM ABERTO";
                }
                else
                {
                    txb_Avisos.Text = "CAIXA LIVRE";
                }
            }
            txb_Qtde.Foreground = txb_ValorUnit.Foreground = combobox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
        }

        /// <summary>
        /// Abre a janela de sangria e suprimento
        /// </summary>
        private void AbrirJanelaSangriaSupr()
        {
            if (!turno_aberto)
            {
                DialogBox.Show(strings.SANGRIA_SUPRIMENTO, DialogBoxButtons.No, DialogBoxIcons.Warn, false, strings.NAO_HA_TURNO_ABERTO);
                return;
            }
            if (PedeSenhaGerencial("Fazendo Sangria ou Suprimento"))
            {
                if (!ChecagemPreVenda(true))
                {
                    return;
                }

                var ss = new SangSupr();
                ss.ShowDialog();
                using var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter();
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                OPER_TA.Connection = LOCAL_FB_CONN;
                if (DeterminarStatusDeSangria(false) != statusSangria.Normal)
                {
                    txb_Avisos.Text = strings.FAZER_SANGRIA;
                }
                else
                {
                    txb_Avisos.Text = strings.CAIXA_LIVRE;
                }

            }
        }

        /// <summary>
        /// Alterna entre desconto e sem desconto no próximo item a ser lançado
        /// </summary>
        private void AlternarDescontoNoItem()
        {
            if (tipoDeDesconto == tipoDesconto.Nenhum)
            {
                var senha = new perguntaSenha("Aplicando desconto no item");
                senha.ShowDialog();
                if (senha.DialogResult == false)
                {
                    return;
                }
                else if (senha.DialogResult == true && senha.NivelAcesso == perguntaSenha.nivelDeAcesso.Gerente)
                { AplicarDesconto(false); }
                else if (senha.DialogResult == true && senha.NivelAcesso == perguntaSenha.nivelDeAcesso.Funcionario)
                {
                    if (DESCONTO_MAXIMO == 0)
                    {
                        DialogBox.Show(strings.APLICAR_DESCONTO, DialogBoxButtons.No, DialogBoxIcons.Warn, false, strings.SENHA_DIGITADA_NAO_E_VALIDA);
                        return;
                    }
                    AplicarDesconto(true);
                }
            }
            else if (tipoDeDesconto != tipoDesconto.Nenhum)
            {
                DialogBox.Show(strings.APLICAR_DESCONTO, DialogBoxButtons.Yes, DialogBoxIcons.None, false, strings.DESCONTO_CANCELADO_PARA_PROXIMO_ITEM);
                tipoDeDesconto = tipoDesconto.Nenhum;
                if (_emTransacao)
                {
                    txb_Avisos.Text = strings.CUPOM_ABERTO;
                }
                else if (_modoDevolucao)
                {
                    txb_Avisos.Text = strings.MODO_DE_DEVOLUCAO;
                }
                else
                {
                    txb_Avisos.Text = strings.CAIXA_LIVRE;
                }
            }
        }

        /// <summary>
        /// Alterna entre modo de consulta e modo normal de venda.
        /// </summary>
        private void AlternarModoDeConsulta()
        {
            if (!_modo_consulta)
            {
                txb_ValorUnit.Clear();
                total1 = txb_TotGer.Text;
                total2 = txb_TotProd.Text;
                txb_TotGer.Clear();
                txb_TotProd.Clear();
                _modo_consulta = true;
                lbl_Marquee.Visibility = Visibility.Hidden;
                lbl_Cortesia.Content = "";
                txb_Avisos.Text = "MODO DE CONSULTA";
                txb_Qtde.Foreground = txb_ValorUnit.Foreground = combobox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF09CAAA"));
            }
            else if (_modo_consulta)
            {
                if (!_emTransacao)
                {
                    lbl_Marquee.Visibility = Visibility.Visible;
                    txb_Avisos.Text = turno_aberto ? "CAIXA LIVRE" : "CAIXA FECHADO";
                }
                else if (_modoDevolucao) { txb_Avisos.Text = "MODO DE DEVOLUÇÃO"; }
                else
                {
                    txb_Avisos.Text = "CUPOM ABERTO";
                }

                lbl_Cortesia.Content = null;
                txb_ValorUnit.Clear();
                txb_Qtde.Clear();
                txb_TotGer.Text = total1;
                txb_TotProd.Text = total2;
                _modo_consulta = false;
                txb_Qtde.Foreground = txb_ValorUnit.Foreground = combobox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF333333"));
                ChecarPorSangria();
            }
        }

        /// <summary>
        /// Alterna entre modo de devolução e modo normal de venda.
        /// </summary>
        private void AlternarModoDevolucao()
        {
            if (_modo_consulta) return;
            switch (_modoDevolucao)
            {
                case true:
                    if (_emTransacao)
                    {
                        FecharDevolucaoNovo();
                    }
                    txb_Avisos.Text = "CAIXA LIVRE";
                    _modoDevolucao = false;
                    break;
                case false:
                    if (!_emTransacao && turno_aberto)
                    {
                        if (PedeSenhaGerencial("Iniciando Devolução"))
                        {
                            txb_Avisos.Text = "MODO DE DEVOLUÇÃO";
                            _modoDevolucao = true;
                        }
                    }
                    break;
            }
        }

        private void NovoModoDeDevolucao()
        {
            if (PedeSenhaGerencial("Iniciando devolução"))
            {
                ListaDevolucao ld = new ListaDevolucao();
                ld.ShowDialog();
            }
        }

        /// <summary>
        /// Alterna entre modo escuro e normal
        /// </summary>
        private void AlternarModoEscuro()
        {
            //var bc = new BrushConverter();
            //if (!_nightmode)
            //{
            //    MainWindow.Background = new SolidColorBrush(Colors.Black);
            //    richTextBox1.Background = ACBox.Background = txb_Qtde.Background = txb_Avisos.Background = txb_TotGer.Background = txb_TotProd.Background = txb_ValorUnit.Background = (Brush)bc.ConvertFrom("#FF454545");
            //    MainWindow.Foreground = ACBox.Foreground = richTextBox1.Foreground = txb_Avisos.Foreground = txb_Qtde.Foreground = txb_TotGer.Foreground = txb_TotProd.Foreground = txb_ValorUnit.Foreground = new SolidColorBrush(Colors.White);
            //    rec_Logo.Fill = (Brush)bc.ConvertFrom("#7F292929");
            //    ACBox.Text = string.Empty;
            //    _nightmode = true;
            //}
            //else
            //{
            //    MainWindow.Background = Brushes.White;

            //    richTextBox1.Background = ACBox.Background = txb_Qtde.Background = txb_Avisos.Background = txb_TotGer.Background = txb_TotProd.Background = txb_ValorUnit.Background = (Brush)bc.ConvertFrom("#E4FFFFFF");
            //    MainWindow.Foreground = (Brush)bc.ConvertFrom("#FF4D4D4D");
            //    richTextBox1.Foreground = ACBox.Foreground = txb_Avisos.Foreground = txb_Qtde.Foreground = txb_TotGer.Foreground = txb_TotProd.Foreground = txb_ValorUnit.Foreground = (Brush)bc.ConvertFrom("#FF333333");
            //    rec_Logo.Fill = (Brush)bc.ConvertFrom("#7FFFFFFF");
            //    ACBox.Text = string.Empty;
            //    _nightmode = false;
            //}
        }

        /// <summary>
        /// Abre/fecha o painel de ajuda
        /// </summary>
        private void AlternarPainelDeAjuda()
        {
            switch (_painelFechado)
            {
                case true:
                    Storyboard abre = FindResource("Canvas_Open") as Storyboard;
                    abre.Begin();
                    _painelFechado = false;
                    break;
                case false:
                    Storyboard fecha = FindResource("Canvas_Close") as Storyboard;
                    fecha.Begin();
                    _painelFechado = true;
                    break;
            }
        }

        /// <summary>
        /// Pergunta o desconto para o usuário e aplica nas variáveis desconto e descontando
        /// </summary>
        /// <param name="pRestrito"></param>
        private void AplicarDesconto(bool pRestrito)
        {
            var pd = new Desconto(pRestrito, DESCONTO_MAXIMO);
            pd.ShowDialog();

            if (pd.DialogResult == true)
            {
                switch (pd.absoluto)
                {
                    case true:
                        desconto = pd.reais;
                        tipoDeDesconto = tipoDesconto.Absoluto;
                        txb_Avisos.Text = "DESCONTO ATIVO";
                        break;
                    case false:
                        desconto = pd.porcentagem;
                        tipoDeDesconto = tipoDesconto.Percentual;
                        txb_Avisos.Text = "DESCONTO ATIVO";
                        break;
                }
            }
            else if (pd.DialogResult == false)
            {
                //TODO but_Aplica_Desconto.Checked = false;
            }

        }

        /// <summary>
        /// Converte asterisco em multiplicador de quantidade
        /// </summary>
        private void AplicarSelecaoDeQuantidade()
        {
            if (!string.IsNullOrWhiteSpace(combobox.Text) && rgx.IsMatch(combobox.Text) && decimal.TryParse(combobox.Text.TrimEnd('*'), out decimal quantidade))
            {
                txb_Qtde.Text = quantidade.ToString();
                combobox.Text = "";
                // Atualizar o valor total:
                if (!string.IsNullOrWhiteSpace(txb_ValorUnit.Text))
                {
                    txb_TotProd.Text = (txb_ValorUnit.Text.ExtrairValorFromCurrencyString(ptBR) * txb_Qtde.Text.ExtrairValorFromCurrencyString(ptBR)).ToString();
                }
            }
        }


        /// <summary>
        /// Atualiza as variáveis de dobra retroativamente caso os hiperlinks autômatos não sejam designados pela interface artificial.
        /// </summary>
        private void AtualizarRetroTabelas()
        {
            //if ((DateTime.Today.Month == 4) && ((DateTime.Today.Day == 1) || (DateTime.Today.Day == 2)))
            //{
            //    var rnd = new Random();
            //    lbl_Operador.Content = string.Format(strings.VOCE_ESTA_SENDO_ATENDIDO_POR, funcoes.eegg[rnd.Next(0, funcoes.eegg.Count)]);
            //}
        }

        /// <summary>
        /// Método de entrada para o cancelamento do último cupom.
        /// </summary>
        private void CancelarUltimoCupom()
        {
            if (_emTransacao || !PedeSenhaGerencial("Cancelando Último Cupom", _modoTeste)) { return; }
            new List().ShowDialog();
            IniciarSincronizacaoDB(EnmTipoSync.tudo, Settings.Default.SegToleranciaUltSync/*, EnmTipoSync.vendas*/);
            return;

        }


        /// <summary>
        /// Carrega os produtos na lista de autocomplete do ACBox
        /// </summary>
        /// <param name="pContingencia">A contingência estava ativada previamente?</param>
        private void AtualizarProdutosNoACBox()
        {
            // Vê se o ACBox tem ItemsSource.
            // Se tiver, continuar o procedimento novo.
            // Se não, continuar com a rotina antiga.
            // Ver a lista de produtos alterados (_lstProdutosAlteradosSync).
            // Se estiver vazia, fazer nada.
            // Se tiver item, ver se tem pelo menos 1 item para deletar.
            //      - Se tiver, continuar com a rotina antiga.
            //      - Não tem, percorrer por cada item da lista e atualizar seu correspondente no ItemsSource.
            if (mvm.LstProdutos is null) mvm.LstProdutos = new ObservableCollection<ComboBoxBindingDTO_Produto>();
            if (mvm.LstProdutos.Count == 0)
            {
                CarregarProdutosNoAcbox();
                combobox.ItemsSource = mvm.LstProdutos;
            }
            else
            {
                if (_lstProdutosAlteradosSync == null || _lstProdutosAlteradosSync.Count == 0) { return; }

                else
                {
                    try
                    {
                        foreach (var itemAlterado in _lstProdutosAlteradosSync)
                        {
                            // Encontrar o item equivalente no ItemsSource
                            var itemEncontradoAcbox = mvm.LstProdutos.FirstOrDefault(item => item.ID_IDENTIFICADOR.Equals(itemAlterado.ID_IDENTIFICADOR));

                            // Tá, e se o itemEncontrado não for encontrado?
                            // Será nulo?
                            // Será um ComboBoxBindingDTO_Produto vazio?
                            // Será só imaginação?

                            if (itemEncontradoAcbox == null || itemEncontradoAcbox.ID_IDENTIFICADOR.Equals(0))
                            {
                                mvm.LstProdutos.Add(
                                    new ComboBoxBindingDTO_Produto
                                    {
                                        COD_BARRA = string.IsNullOrWhiteSpace(itemAlterado.COD_BARRA) ? string.Empty : itemAlterado.COD_BARRA,
                                        DESCRICAO = string.IsNullOrWhiteSpace(itemAlterado.DESCRICAO) ? string.Empty : itemAlterado.DESCRICAO,
                                        ID_IDENTIFICADOR = itemAlterado.ID_IDENTIFICADOR,
                                        REFERENCIA = string.IsNullOrWhiteSpace(itemAlterado.REFERENCIA) ? string.Empty : itemAlterado.REFERENCIA
                                    });
                            }
                            else
                            {
                                //TODO -- DONE --: testar. Ver se a alteração no itemEncontrado é ByRef ou ByVal.
                                // O esperado aqui é ByRef.
                                // OoOok, é ByRef.

                                //TODO: -- TESTAR -- ver a origem da alteração de cada property.
                                // Dependendo da tabela/origem, não permitir anular valores.

                                itemEncontradoAcbox.COD_BARRA = itemAlterado.ORIGEM_TB.Equals("TB_EST_PRODUTO") ? itemAlterado.COD_BARRA.Safestring() : itemEncontradoAcbox.COD_BARRA;
                                itemEncontradoAcbox.DESCRICAO = itemAlterado.ORIGEM_TB.Equals("TB_ESTOQUE") ? itemAlterado.DESCRICAO.Safestring() : itemEncontradoAcbox.DESCRICAO;
                                itemEncontradoAcbox.REFERENCIA = itemAlterado.ORIGEM_TB.Equals("TB_EST_PRODUTO") ? itemAlterado.REFERENCIA.Safestring() : itemEncontradoAcbox.REFERENCIA;
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        log.Error($"CarregarProdutos(...)", ex);
                        throw ex;
                    }
                }
                _lstProdutosAlteradosSync = null;
            }
        }
        /// <summary>
        /// Carrega os produtos no ACBox para a utilização do Auto complete
        /// </summary>
        /// <param name="pContingencia"></param>
        private void CarregarProdutosNoAcbox()
        {
            using var dt = new DataSets.FDBDataSetOperSeed.TB_EST_ESTOQUE_KEYVALUEDataTable();
            using var tB_ESTOQUETableAdapter = new TB_EST_ESTOQUE_KEYVALUETableAdapter();
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            tB_ESTOQUETableAdapter.Connection = LOCAL_FB_CONN;
            try
            {
                log.Debug("Preenchendo datatable para o ACBox");
                tB_ESTOQUETableAdapter.FillByAtivos(dt);
                log.Debug("Preenchido");
            }
            catch (Exception ex)
            {
                string mensagemErro = string.Empty;
                if (dt.Rows.Count > 0)
                {
                    try
                    {
                        mensagemErro = "Erro ao preencher dt (TB_ESTOQUE):\nRegistro " +
                                       dt.GetErrors()[0][0] +
                                       " retornou um erro: " +
                                       dt.GetErrors()[0].RowError + "\n";
                    }
                    catch (Exception ex2)
                    {
                        log.Error("CarregarProdutos no ACBox", ex2);
                    }
                }
                log.Error($"Carregar Produtos no ACBOx - {mensagemErro}", ex);

                Application.Current.Shutdown();
                return;
            }

            foreach (DataSets.FDBDataSetOperSeed.TB_EST_ESTOQUE_KEYVALUERow row in dt)
            {
                mvm.LstProdutos.Add(new ComboBoxBindingDTO_Produto()
                {
                    ID_IDENTIFICADOR = row.ID_IDENTIFICADOR,
                    COD_BARRA = row.IsCOD_BARRANull() ? "" : row.COD_BARRA,
                    DESCRICAO = row.DESCRICAO,
                    REFERENCIA = row.IsREFERENCIANull() ? "" : row.REFERENCIA,
                    STATUS = row.STATUS
                    //ID_IDENTIFICADOR = (int)row["ID_IDENTIFICADOR"],
                    //COD_BARRA = row["COD_BARRA"].ToString(),
                    //DESCRICAO = row["DESCRICAO"].ToString(),
                    //REFERENCIA = row["REFERENCIA"].ToString(),

                    //,
                    //QTD_ATACADO = row["QTD_ATACADO"] == System.DBNull.Value ? null : (decimal?)row["QTD_ATACADO"],
                    //PRC_VENDA = (decimal)row["PRC_VENDA"]
                });
            }
        }
        /// <summary>
        /// Detecta se uma comanda foi inserida, e carrega os produtos nela lançados.
        /// </summary>
        private bool CarregarProdutosDaComandaNovo()
        {
            if (!combobox.Text.StartsWith("$") || !USA_COMANDA) { return false; }
            Infopad pad = new Infopad();
            int.TryParse(combobox.Text.TrimStart('$'), out int comanda);

            if (comanda <= 0) { return false; }

            try
            {
                log.Debug($"Comanda detectada: {comanda}");

                if (!pad.LeComandas(comanda))
                {
                    log.Warn("Erro ao ler comanda");
                    return true;
                }

                List<int> cods = pad.Codigos;
                List<decimal> qtds = pad.Quantidades;
                if (cods.Count == 0)
                {
                    combobox.Text = "";
                    MessageBox.Show("Comanda vazia.");
                }
                else
                {
                    if (_tipo == ItemChoiceType.FECHADO && _modo_consulta == false)
                    {

                        _tipo = ItemChoiceType.ABERTO;

                        if (!ChecagemPreVenda())
                        {
                            return false;
                        }

                        if (!PrepararCabecalhoDoCupom())
                        {
                            return false;
                        }
                    }
                    if (cods.Count == 0)
                    {
                        combobox.Text = "";
                        DialogBox.Show(strings.COMANDA, DialogBoxButtons.No, DialogBoxIcons.None, false, strings.NAO_HA_ITEMS_LANCADOS_NA_COMANDA);
                        return true;
                    }
                    using (var ESTOQUE_TA = new TB_ESTOQUETableAdapter())
                    //using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
                    using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                    {
                        //if (contingencia)
                        //{
                        ESTOQUE_TA.Connection = LOCAL_FB_CONN;
                        //}
                        //else
                        //{
                        //    ESTOQUE_TA.Connection = SERVER_FB_CONN;
                        //}

                        for (var i = 0; i < cods.Count; i++)
                        {
                            if (qtds[i] <= 0)
                            {
                                DialogBox.Show(strings.COMANDA,
                                               DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                               strings.A_QUANT_DO_ITEM_ERA_ZERO_OU_NEGATIVA,
                                               strings.IMPOSSIVEL_PROSSEGUIR_COM_A_VENDA);
                                FinalizarVendaNovo();
                                return true;
                            }

                            decimal precoUnitVenda = (decimal?)ESTOQUE_TA.SP_TRI_PEGAPRECO(cods[i], qtds[i]) ?? 0M;
                            if (precoUnitVenda <= 0) continue;
                            ProcessarItemNovo(cods[i],
                                         precoUnitVenda,
                                         qtds[i],
                                         0);

                            numProximoItem += 1;
                            combobox.Text = "";

                        }
                    }
                    pad.FechaComanda(comanda);
                    cods.Clear();
                    qtds.Clear();
                    return true;
                }
            }
            catch (Exception ex)
            {
                DialogBox.Show(strings.COMANDA,
                               DialogBoxButtons.No, DialogBoxIcons.Error, true,
                               RetornarMensagemErro(ex, false));
                log.Error("Erro ao ler Comanda", ex);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Detecta se um orçamento foi inserido e carrega os produtos nele lançados
        /// </summary>
        private bool CarregarProdutosDoOrcamentoNovo()
        {
            if (!combobox.Text.StartsWith("+")) { return false; }

            try
            {
                //Contingencia();
                orcamentoAtual.Clear();
                int.TryParse(combobox.Text.TrimStart('+'), out int orcamento);
                log.Debug($"Orçamento detectado: {orcamento}");
                //if (orcamento_atual.LeOrcaProdutos(orcamento) && orcamento_atual.LeOrcamento(orcamento))

                if (!orcamentoAtual.LeOrcamento(orcamento))
                {
                    log.Debug("Erro ao ler orçamento, ou orçamento está FECHADO");
                    MessageBox.Show("Orçamento indisponível.");
                    combobox.Text = "";
                    return true;
                }

                if (!orcamentoAtual.LeOrcaProdutos(orcamento))
                {
                    log.Debug("Orçamento sem item");
                    return true;
                }

                if (orcamentoAtual.produtos.Count == 0)
                {
                    combobox.Text = "";
                    MessageBox.Show("Orçamento vazio.");
                    return true;
                }

                if (_tipo == ItemChoiceType.FECHADO && _modo_consulta == false)
                {
                    _tipo = ItemChoiceType.ABERTO;

                    if (!ChecagemPreVenda())
                    {
                        return false;
                    }

                    if (!PrepararCabecalhoDoCupom())
                    {
                        return false;
                    }
                }

                foreach (var item in orcamentoAtual.produtos)
                {
                    if (item.QUANT <= 0)
                    {
                        DialogBox.Show(strings.ORCAMENTO,
                                       DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                       strings.A_QUANT_DO_ITEM_ERA_ZERO_OU_NEGATIVA,
                                       strings.IMPOSSIVEL_PROSSEGUIR_COM_A_VENDA);
                        FinalizarVendaNovo();
                        return true;
                    }

                    //processaItem(cods[i], (decimal)ESTOQUE_TA.SP_TRI_PEGAPRECO(cods[i], qtds[i]), qtds[i], (decimal)ESTOQUE_TA.SP_TRI_PEGAPRECO(cods[i], qtds[i]), ESTOQUE_TA, EST_PRODUTO_TA);
                    ProcessarItemNovo(item.ID_ESTOQUE,
                                 item.VALOR,
                                 item.QUANT,
                                 item.DESCONTO);

                    numProximoItem += 1;

                    combobox.Text = "";
                }
                _usouOrcamento = true;
                return true;
            }
            catch (Exception ex)
            {
                DialogBox.Show(strings.ORCAMENTO, DialogBoxButtons.No, DialogBoxIcons.Error, true, RetornarMensagemErro(ex, false));
                log.Error("Carregar produtos do orçamento", ex);
            }
            return false;
        }


        /// <summary>
        /// Detecta se um orçamento foi inserido e carrega os produtos nele lançados
        /// </summary>
        private bool CarregarProdutosDaOS()
        {
            if (!combobox.Text.StartsWith("%")) { return false; }

            try
            {
                //Contingencia();
                orcamentoAtual.Clear();
                int.TryParse(combobox.Text.TrimStart('%'), out int ordemServico);
                log.Debug($"Ordem de Seriço detectado: {ordemServico}");

                FbConnection connection = new FbConnection(MontaStringDeConexao(SERVERNAME, SERVERCATALOG));
                ordemDeServico = _funcoes.GetClippOsByID(connection, ordemServico);

                if (_tipo == ItemChoiceType.FECHADO && _modo_consulta == false)
                {
                    _tipo = ItemChoiceType.ABERTO;

                    if (!ChecagemPreVenda())
                    {
                        return false;
                    }

                    if (!PrepararCabecalhoDoCupom())
                    {
                        return false;
                    }
                }

                if (ordemDeServico is null)
                {
                    log.Debug("Erro ao ler orçamento, ou orçamento está FECHADO");
                    MessageBox.Show("Orçamento indisponível.");
                    combobox.Text = "";
                    return true;
                }

                if (ordemDeServico.ClippOsItems is null || ordemDeServico.ClippOsItems.Count == 0)
                {
                    combobox.Text = "";
                    MessageBox.Show("Orçamento vazio.");
                    return true;
                }

                foreach (var item in ordemDeServico.ClippOsItems)
                {
                    if (item.QTD_ITEM <= 0)
                    {
                        DialogBox.Show(strings.ORCAMENTO,
                                       DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                       strings.A_QUANT_DO_ITEM_ERA_ZERO_OU_NEGATIVA,
                                       strings.IMPOSSIVEL_PROSSEGUIR_COM_A_VENDA);
                        FinalizarVendaNovo();
                        return true;
                    }

                    ProcessarItemNovo(item.ID_IDENTIFICADOR,
                                 item.VLR_UNIT,
                                 item.QTD_ITEM ?? 1,
                                 item.VLR_DESC ?? 0);

                    numProximoItem += 1;

                    combobox.Text = "";
                }
                _usouOS = true;
                return true;
            }
            catch (Exception ex)
            {
                DialogBox.Show(strings.ORCAMENTO, DialogBoxButtons.No, DialogBoxIcons.Error, true, RetornarMensagemErro(ex, false));
                log.Error("Carregar produtos do orçamento", ex);
            }
            return false;
        }

        /// <summary>
        /// Se houver uma venda pendente devido a uma falha no sistema, ela é carregada
        /// </summary>
        private bool CarregarVendaPendenteNovo()
        {
            if (!_emTransacao)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Ultimavenda.txt";
                if (File.Exists(path) && File.ReadAllText(path).Contains("|"))
                {
                    if (DialogBox.Show(strings.VENDA, DialogBoxButtons.YesNo, DialogBoxIcons.None, false, strings.FOI_ENCONTRADA_VENDA_PENDENTE) == true)
                    {
                        IEnumerable<string> ultimavenda = File.ReadAllLines(path);
                        File.WriteAllText(path, "Arquivo gerado automaticamente pelo AmbiPDV. Não remova.\n\r");
                        if (!ChecagemPreVenda())
                        {
                            return false;
                        }

                        if (!PrepararCabecalhoDoCupom())
                        {
                            return false;
                        }

                        using (var ESTOQUE_TA = new TB_ESTOQUETableAdapter())
                        //using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
                        using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                        {
                            //if (contingencia)
                            //{
                            ESTOQUE_TA.Connection = LOCAL_FB_CONN;
                            //}
                            //else
                            //{
                            //    ESTOQUE_TA.Connection = SERVER_FB_CONN;
                            //}

                            foreach (var line in ultimavenda)
                            {
                                if (line.Contains("|"))
                                {
                                    _prepesado = true;

                                    ProcessarItemNovo(line.Split('|')[1].Safeint(),
                                                 (decimal)ESTOQUE_TA.SP_TRI_PEGAPRECO(line.Split('|')[1].Safeint(), line.Split('|')[0].Safedecimal()),
                                                 line.Split('|')[0].Safedecimal(),
                                                 0);
                                }
                            }
                        }
                        return true;
                    }

                }
                File.WriteAllText(path, "Arquivo gerado automaticamente pelo AmbiPDV. Não remova.\n\r");
            }
            return false;
        }

        /// <summary>
        /// Executa o procedimento de alteração da interface do banco entre banco em rede e banco de contingência. Caso o sistema esteja voltando da contingência, executa a sincronização personalizada.
        /// </summary>
        /// <param name="pContingencia">A contingência estava ativada previamente?</param>
        /// <param name="pTipo">Qual tipo de sincronização deverá ser efetuada, caso o sistema esteja voltando de uma contingência.</param>
        private void ChecarPorContingencia(bool pContingencia, int pSegundosTolerancia, EnmTipoSync pTipo = EnmTipoSync.tudo)
        {
            var funcoes = new funcoesClass();
            bool conectividade;
            log.Debug("Checando conexão com o servidor.");
            //var task = Task.Run(() => TestaConexaoComServidor());
            //conectividade = task.Wait(TimeSpan.FromSeconds(3));
            conectividade = funcoes.TestaConexaoComServidor(SERVERNAME, SERVERCATALOG, FBTIMEOUT);
            if (conectividade == true && pContingencia == false && !_emTransacao)
            {
                //Se a contingência não estava ativada e a conectividade ainda persiste, não há nada a se fazer.

                IniciarSincronizacaoDB(pTipo, pSegundosTolerancia);
                ultimaContingencia = DateTime.Now;
                return;
            }
            else if (conectividade == true && pContingencia == true && !_emTransacao)
            {
                //Houve o retorno da conectividade ao servidor.
                funcoes.ChangeConnectionString(MontaStringDeConexao(SERVERNAME, SERVERCATALOG));
                _contingencia = false;
                log.Debug($"FDBConnString definido para DB na rede: {Settings.Default.FDBConnString}");
                bar_Contingencia.Visibility = Visibility.Hidden;
                IniciarSincronizacaoDB(pTipo, pSegundosTolerancia);
                ultimaContingencia = DateTime.Now;
                //vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                // O sistema saiu de contingência, deve sincronizar
                // tudo.
                //IniciarSincronizacaoDB(EnmTipoSync.tudo);
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                return;
            }
            else if (conectividade == false && pContingencia == false)
            {              
                //Aqui houve a queda de conexão com o servidor.                
                DialogBox.Show("CONEXÃO COM O SERVIDOR", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Não foi possivel se conectar com o Servidor.\n O sistema entrara em modo de contigencia " +
                    "e para testar novamente a comunicação com o servidor utilize as teclas\n'CTRL+S'."); ;
                funcoes.ChangeConnectionString(MontaStringDeConexao("localhost", localpath));
                _contingencia = true;
                log.Debug($"FDBConnString definido para DB de contingência: {Settings.Default.FDBConnString}");
                lbl_Carga.Content = ultimaContingencia.ToShortTimeString();
                bar_Contingencia.Visibility = Visibility.Visible;
                return;
            }
            else if (conectividade == false && pContingencia == true)
            {
                //Por fim, o sistema pode ainda estar em contingência.
                _contingencia = true;
                return;
            }
            //task.Dispose();
        }


        /// <summary>
        /// Checa se o sistema precisa de sangria ou não
        /// </summary>
        /// <returns>RETURNS</returns>
        private bool ChecarPorSangria()
        {
            if (_modoTeste)
            {
                return false;
            }

            using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                OPER_TA.Connection = LOCAL_FB_CONN;
                if (turno_aberto && DeterminarStatusDeSangria(false) != statusSangria.Normal)
                {
                    txb_Avisos.Text = "FAZER SANGRIA";
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checa se há um turno aberto ou não.
        /// </summary>
        private void ChecarStatusTurno()
        {
            udx_pdv_oper = funcoes.VerificaPDVOper(operador);
            if (udx_pdv_oper != null)
            {
                log.Debug($"Pré VerificaPDVOper() {udx_pdv_oper.timestamp}");
                log.Debug($"Caixa foi encontrado no status LIVRE");
                turno_aberto = true;
                if (!ChecarPorSangria())
                {
                    txb_Avisos.Text = "CAIXA LIVRE";
                }
            }
            else
            {
                log.Debug($"Pré VerificaPDVOper() NULO");
                log.Debug("Caixa foi encontrado no status FECHADO");
                turno_aberto = false;
                txb_Avisos.Text = "CAIXA FECHADO";
            }
            log.Debug("LOADING COMPLETED!");
        }


        /// <summary>
        /// Tenta converter o que foi digitado em um identificador interno de produto. Retorna -1 caso não encontre um item.
        /// 
        /// Método importante para a seleção de produto: deve deixar o item selecionado sempre válido (ACBox.SelectedItem).
        /// </summary>
        /// <param name="pInput">Informação a ser convertida</param>
        /// <returns></returns>
        private ComboBoxBindingDTO_Produto ConverterInformacaoEmProduto(string pInput)
        {
            #region Valida seleção de produto

            if (true)
            {
                try
                {
                    log.Debug("Procurando por referência");
                    object objItemEncontrado_with_ref = mvm.LstProdutos.First(item => item.REFERENCIA.Equals(pInput, StringComparison.InvariantCultureIgnoreCase) && item.STATUS == "A");
                    if (objItemEncontrado_with_ref != null)
                    {
                        log.Debug("Encontrou um item pela referência");
                        combobox.SelectedItem = objItemEncontrado_with_ref;
                    }
                    log.Debug($"cbb.SelectedItem {(combobox.SelectedItem == null ? "" : "não")} era nulo");
                    if (combobox.SelectedItem == null)
                    {
                        log.Debug($"Tentando converter {combobox.Text} em int");
                        if (int.TryParse(combobox.Text, out int tentativa_conversao_cod))
                        {
                            log.Debug($"Convertido em int: {tentativa_conversao_cod}");

                            object objItemEncontrado = mvm.LstProdutos.FirstOrDefault(item => item.COD_BARRA.Equals(pInput) ||
                                item.ID_IDENTIFICADOR.Equals(tentativa_conversao_cod));
                            if (objItemEncontrado != null)
                            {
                                log.Debug($"Achei");
                                combobox.SelectedItem = objItemEncontrado;
                            }
                        }
                        else
                        {
                            log.Debug("Não converti em int");
                            object objItemEncontrado = mvm.LstProdutos.FirstOrDefault(item => item.COD_BARRA.Equals(pInput) && item.STATUS == "A");
                            if (objItemEncontrado != null)
                            {
                                log.Debug("Achei");
                                combobox.SelectedItem = objItemEncontrado;
                            }
                        }
                    }
                    log.Debug($"cbb.SelectedItem {(combobox.SelectedItem == null ? "" : "não")} era nulo");

                }
                catch (Exception ex)
                {
                    log.Error("Erro ao buscar produto", ex);
                    //return MostrarProdutoNaoEncontrado(pInput);
                    int prodNaoEncontrado = MostrarProdutoNaoEncontrado(pInput);
                    return prodNaoEncontrado == -1 ? null : new ComboBoxBindingDTO_Produto() { ID_IDENTIFICADOR = prodNaoEncontrado, COD_BARRA = "", DESCRICAO = "", REFERENCIA = "" };

                    //return -1;
                }

                if (combobox.SelectedItem == null)
                {
                    //return MostrarProdutoNaoEncontrado(pInput);
                    ////return -1;
                    int prodNaoEncontrado = MostrarProdutoNaoEncontrado(pInput);
                    return prodNaoEncontrado == -1 ? null : new ComboBoxBindingDTO_Produto() { ID_IDENTIFICADOR = prodNaoEncontrado, COD_BARRA = "", DESCRICAO = "", REFERENCIA = "" };
                }
            }

            #endregion Valida seleção de produto

            return (ComboBoxBindingDTO_Produto)combobox.SelectedItem;//.ID_IDENTIFICADOR;
        }

        /// <summary>
        /// Determina o status da sangria do caixa
        /// </summary>
        /// <param name="aviso"></param>
        /// <returns></returns>
        private statusSangria DeterminarStatusDeSangria(bool aviso = true)
        {
            if (!EXIGE_SANGRIA)
            {
                return statusSangria.Normal;
            }

            var funcoes = new funcoesClass();
            decimal valoremgaveta = funcoes.CalculaValorEmCaixa(NO_CAIXA);
            //using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            //using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            //{
            //    OPER_TA.Connection = LOCAL_FB_CONN;
            //    valoremgaveta = (decimal)OPER_TA.SP_TRI_VALOREMGAVETA(NO_CAIXA);
            //}

            if (BLOQUEIA_NO_LIMITE)
            {
                if (PERMITE_FOLGA_SANGRIA && valoremgaveta < VALOR_DE_FOLGA + VALOR_MAX_CAIXA && valoremgaveta > VALOR_MAX_CAIXA) //Valor em gaveta está entre caixamaximo e (caixamaximo e valordefolga)
                {
                    if (aviso)
                    {
                        DialogBox.Show(strings.SANGRIA,
                                       DialogBoxButtons.Yes, DialogBoxIcons.None,
                                       false,
                                       strings.CAIXA_ESTA_ACIMA_DO_LIMITE,
                                       strings.PRESSIONE_ENTER_PARA_CONTINUAR);
                    }
                    txb_Avisos.Text = "FAZER SANGRIA";
                    return statusSangria.Folga;
                }
                else if (valoremgaveta > VALOR_MAX_CAIXA)
                {
                    if (aviso)
                    {
                        DialogBox.Show(strings.SANGRIA,
                                       DialogBoxButtons.No, DialogBoxIcons.Info,
                                       false,
                                       strings.CAIXA_ESTA_ACIMA_DO_LIMITE);
                    }
                    log.Debug("Venda negada por limite de caixa excedido.");
                    _tipo = ItemChoiceType.FECHADO;
                    combobox.Text = "";
                    txb_Avisos.Text = "FAZER SANGRIA";
                    return statusSangria.Excesso;
                }
            }
            else if (!BLOQUEIA_NO_LIMITE)
            {
                if (valoremgaveta > VALOR_MAX_CAIXA)
                {
                    if (aviso)
                    {
                        DialogBox.Show(strings.SANGRIA,
                                       DialogBoxButtons.Yes, DialogBoxIcons.None,
                                       false,
                                       strings.CAIXA_ESTA_ACIMA_DO_LIMITE,
                                       strings.PRESSIONE_ENTER_PARA_CONTINUAR);
                    }
                    txb_Avisos.Text = strings.FAZER_SANGRIA;
                    return statusSangria.Folga;
                }
            }
            return statusSangria.Normal;
        }

        /// <summary>
        /// Executa a abertura do caixa
        /// </summary>
        /// <returns></returns>
        private bool ExecAberturaCaixa()
        {
            using (var TERMARIO_TA = new TRI_PDV_TERMINAL_USUARIOTableAdapter())//declara uma variável do tipo var e cria uma instância da tabela TRI_PDV_TERMINAL_USUARIO em um table adapter que irá preencher o data set(Conjuntos de dados) com os dados dessa tabela 
            using (var USERS_TA = new TRI_PDV_USERSTableAdapter())// Idem 
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })//Monta uma string de conexão 
            {
                TERMARIO_TA.Connection = LOCAL_FB_CONN;// conecta a instancia TERMARIO_TA ao banco de dados através da string de conexão LOCAL_FB_CONN
                USERS_TA.Connection = LOCAL_FB_CONN;// idem só que com USERS_TA
                log.Debug($"ExecAberturaCaixa() - operador = {operador}");//Registra em um  .TXT as informações inseridas no método
                int iduser = USERS_TA.PegaIdPorUser(operador).Safeint();// pesquisa o numero de ID atraves do nome(Lembrando que o clipp não deixa cadastrar dois nomes iguais)
                log.Debug($"ExecAberturaCaixa() - iduser = {iduser}");//  *       
                log.Debug($"ExecAberturaCaixa() - NO_CAIXA = {NO_CAIXA}");//  *
                int _terminaisabertos = (int)TERMARIO_TA.ContaCaixaAberto(NO_CAIXA, iduser);//para saber quais caixas estão abertos, passo como parâmetros o numero ID do caixa e o ID do usuário 
                log.Debug($"ExecAberturaCaixa() - _terminaisabertos = {_terminaisabertos}");
                if (_terminaisabertos == 1)//se existirem terminais abertos com o mesmo número de ID do usuário e do caixa, vai cair nesse condicional onde mostra uma mensagem de erro que já existe um caixa aberto. 
                {
                    log.Warn($"ExecAberturaCaixa() - vai retornar FALSE, pois não deveria chegar aqui. Checar se a tabela TRI_PDV_TERMINAL_USUARIO está corretamente preenchida.");
                    return false;
                }
                else if (_terminaisabertos == 0)// se voltar zerado, o caixa está livre pra ser iniciado o turno
                {
                    string _usuario = (string)TERMARIO_TA.SP_TRI_CHECASTATUSTERMINAL(NO_CAIXA);
                    log.Debug($"ExecAberturaCaixa() - _usuario = {_usuario.Safestring()}");
                    if (_usuario == "")
                    {
                        var AbreCaixa = new AberturaCaixa(iduser, NO_CAIXA);
                        AbreCaixa.ShowDialog();

                        if (AbreCaixa.DialogResult == true)
                        {
                            udx_pdv_oper = AbreCaixa._udx_pdv_oper;
                            if (txb_Avisos.Text != "MODO DE CONSULTA") { txb_Avisos.Text = "CAIXA LIVRE"; }

                            //TODO: Abriu o caixa em modo de consulta? E como fica logo após a saída desse modo, o aviso muda pra "CAIXA LIVRE"?

                            turno_aberto = true;
                            return true;
                        }
                    }
                    else
                    {
                        log.Debug($"ExecAberturaCaixa() - Terminal já em uso por {_usuario.Safestring()}");
                        switch (iduser == 0)
                        {
                            case true:
                                if (DialogBox.Show(strings.ABERTURA_DE_TURNO, DialogBoxButtons.YesNo, DialogBoxIcons.Info, false, $"Este terminal está em uso por {_usuario}", strings.DESEJA_FECHAR_O_TURNO_DO_OUTRO_FUNCIONARIO) == true)
                                {
                                    turno_aberto = true;
                                    udx_pdv_oper = funcoes.VerificaPDVOper(_usuario);
                                    ExecFechamentoCaixa();
                                    if (turno_aberto)
                                    {
                                        turno_aberto = false;
                                    }

                                    if (!(udx_pdv_oper is null))
                                    {
                                        udx_pdv_oper = null;
                                    }

                                    return false;
                                }
                                return false;
                            default:
                                DialogBox.Show(strings.ABERTURA_DE_TURNO,
                                   DialogBoxButtons.No,
                                   DialogBoxIcons.Warn, false,
                                   $"Este terminal está em uso por {_usuario}",
                                   strings.FECHE_O_TURNO_ANTES);
                                break;
                        }
                    }
                }
                else if (_terminaisabertos > 1)
                {
                    //using (var Conn = new FbConnection(LOCAL_FB_CONN.ConnectionString))
                    //using (var Comm = new FbCommand())
                    //{
                    //    Comm.Connection = Conn;
                    //    Comm.CommandType = CommandType.Text;
                    //    Comm.CommandText = "UPDATE TRI_PDV_TERMINAL_USUARIO SET STATUS = 'F' WHERE ID_USER = @Param1 ORDER BY ID_OPER ROWS 1;";
                    //    Comm.Parameters.Add("@Param1", iduser);
                    //    Conn.Open();
                    //    Comm.ExecuteNonQuery();
                    //    Conn.Close();
                    //    ExecAberturaCaixa();
                    //}
                }
            }
            log.Debug("ExecAberturaCaixa() - return false");
            return false;
        }

        /// <summary>
        /// Checa se o caixa está apto a fazer uma venda
        /// </summary>
        /// <param name="pSkipBlock">Pula o travamento do sistema em caso de sangria obrigatória?</param>
        private bool ChecagemPreVenda(bool pOperacao = false)
        {
            if (_modoTeste)
            {
                return true;
            }

            #region Redução Z (ECF)

            if (ECF_ATIVA)
            {
                bool? retorno = ECF.ChecaStatusReducaoZ();
                if (retorno == false)
                {
                    DialogBox.Show(strings.ABERTURA_DE_TURNO,
                                   DialogBoxButtons.No, DialogBoxIcons.Info, false,
                                   strings.NECESSARIO_FAZER_REDUCAO_Z,
                                   strings.EFETUE_REDUCAO_Z_COM_CTRL_O);
                    return false;
                }
                else if (retorno == null)
                {
                    return false;
                }
            }
            #endregion Redução Z (ECF)


            Settings.Default.Reload();
            richTextBox1.Document.Blocks.Clear();


            if (!turno_aberto)
            {
                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.NAO_HA_TURNO_ABERTO, strings.PRESSIONE_F12_PARA_ABRIR_TURNO);
                return false;
            }


            if (PEDE_CPF == 1 && !pOperacao)
            {
                PedirIdentificacao();
            }

            if (SYSCOMISSAO == 1 && !pOperacao)
            {
                PedirVendedor();
            }

            _modo_consulta = false;
            statusSangria _status = DeterminarStatusDeSangria(!pOperacao);
            if (_status != statusSangria.Normal)
            {
                txb_Avisos.Text = "FAZER SANGRIA";
                if (_status == statusSangria.Excesso && !pOperacao)
                {
                    return false;//***************Não pode retornar falso, pois impede o fechamento e/ou sangria************
                }
            }
            //PrintFunc.LimpaRePrint();
            return true;
        }

        /// <summary>
        /// Checa se o caixa está apto a fazer uma venda
        /// </summary>
        private bool ExecChecagemPreDevolucao()
        {
            if (_contingencia)
            {
            }
            Settings.Default.Reload();
            richTextBox1.Document.Blocks.Clear();

            if (!turno_aberto)
            {
                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.NAO_HA_TURNO_ABERTO, strings.PRESSIONE_F12_PARA_ABRIR_TURNO);
                return false;
            }

            _modo_consulta = false;

            //PrintFunc.LimpaRePrint();
            return true;
        }


        /// <summary>
        /// Executa o fechamento do caixa
        /// </summary>
        /// <returns></returns>
        private bool ExecFechamentoCaixa()
        {
            log.Debug($"ExecFechamentoCaixa() - contingencia = {_contingencia}");
            //if (_contingencia)
            //{
            //    DialogBox.Show(strings.FECHAMENTO_DE_TURNO, "O caixa não pode ser fechado em contingência.", "Verifique a conexão com o servidor e tente novamente.", DialogBoxButtons.No, DialogBoxIcons.Error);
            //    return false;
            //}
            log.Debug($"ExecFechamentoCaixa() {udx_pdv_oper.timestamp}");
            //using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            //using (var USERS_TA = new TRI_PDV_USERSTableAdapter())
            //{
            //OPER_TA.Connection = USERS_TA.Connection = LOCAL_FB_CONN;
            if (!turno_aberto)
            {
                log.Debug("ExecFechamentoCaixa() NÃO HÁ CAIXA ABERTO - turno_aberto = FALSE ");
                DialogBox.Show(strings.FECHAMENTO_DE_TURNO, DialogBoxButtons.No, DialogBoxIcons.None, false, strings.NAO_HA_TURNO_ABERTO);
                return false;
            }
            //}
            if (PedeSenhaGerencial(strings.FECHAMENTO_DE_TURNO))
            {
                try
                {
                    if (IMPRESSORA_USB != "Nenhuma")
                    {
                        PrintFunc.RecebePrint(" ", PrintFunc.negrito, PrintFunc.centro, 1);
                        PrintFunc.PrintaSpooler();
                    }
                    log.Debug($"Abrindo novo FechamentoCaixa" + udx_pdv_oper.timestamp.ToString());
                    var fc = new FechamentoCaixa(udx_pdv_oper.timestamp);
                    fc.ShowDialog();
                    if (fc.DialogResult == false)
                    {
                        return false;
                    }

                    ChecarPorContingencia(_contingencia, 0, EnmTipoSync.tudo);
                    txb_Avisos.Text = "CAIXA FECHADO";
                }
                catch (Exception ex)
                {
                    DialogBox.Show(strings.FECHAMENTO_DE_TURNO, DialogBoxButtons.No, DialogBoxIcons.Error, false, RetornarMensagemErro(ex, false));
                    log.Error("Erro ao executar o fechamento de turno", ex);
                    return false;

                }
                turno_aberto = false;
                if (txb_Avisos.Text == "FAZER SANGRIA")
                {
                    txb_Avisos.Text = "CAIXA LIVRE";
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Executa uma função administrativa no TEF
        /// </summary>
        private void ExecFuncaoAdminTEF()
        {
            if (PedeSenhaGerencial("Executando Função Administraiva no TEF"))
            {
                int tef_cliente = 0;
                int tef_estab = 0;
                int tef_redux = 0;
                int tef_unica = 0;
                try
                {
                    var Administrativo = new ADM();

                    var respCRT = new Dictionary<string, string>();
                    Administrativo.Exec();
                    var db = new TEFBox(strings.VENDA_NO_TEF, strings.SIGA_AS_INSTRUCOES_NO_TEF, TEFBox.DialogBoxButtons.Yes, TEFBox.DialogBoxIcons.None);
                    db.ShowDialog();
                    if (db.DialogResult == false)
                    {
                        return;
                    }
                    respCRT = General.LeResposta();
                    using (var tb = new TimedBox(strings.VENDA_NO_TEF, "", respCRT["030-000"], TimedBox.DialogBoxButtons.Yes, TimedBox.DialogBoxIcons.None, 4))
                    { tb.ShowDialog(); }
                    if (respCRT.ContainsKey("009-000") && respCRT["009-000"] != "0")
                    {
                        var dbTef = DialogBox.Show(strings.VENDA_NO_TEF, DialogBoxButtons.Yes, DialogBoxIcons.Info, false, strings.OPERACAO_CANCELADA_OU_NAO_CONCLUIDA, strings.TENTE_NOVAMENTE_OU_OUTRO_METODO);
                        return;
                    }
                    VendaImpressa.ReciboTEF = respCRT;
                    #region printdecision
                    if (respCRT.ContainsKey("737-000") && (respCRT["737-000"] == "1" || respCRT["737-000"] == "3") || !respCRT.ContainsKey("737-000"))
                    {
                        if (respCRT.ContainsKey("710-000") && respCRT["710-000"] != "0")
                        {
                            tef_redux += 1;
                        }
                        else
                        {
                            if (respCRT.ContainsKey("712-000") && respCRT["712-000"] != "0")
                            {
                                tef_cliente += 1;
                            }
                            else
                            {
                                tef_unica += 1;
                            }
                        }
                    }
                    if (respCRT.ContainsKey("737-000") && (respCRT["737-000"] == "2" || respCRT["737-000"] == "3") || !respCRT.ContainsKey("737-000"))
                    {
                        if (respCRT.ContainsKey("714-000") && respCRT["714-000"] != "0")
                        {
                            tef_estab += 1;
                        }
                        else
                        {
                            tef_unica += 1;
                        }
                    }
                    ultimaImpressao = VendaImpressa.IMPRIME(0);

                    if (tef_estab > 0)
                    {
                        ultimaImpressao = VendaImpressa.IMPRIME(0);
                    }
                    #endregion
                    var Confirma = new CNF()
                    {
                        _010 = respCRT["010-000"],
                        _027 = respCRT["027-000"],
                        _717 = DateTime.Now
                    };
                    Confirma.Exec();
                }
                catch (ArgumentException)
                {
                    DialogBox.Show(strings.VENDA_NO_TEF, DialogBoxButtons.No, DialogBoxIcons.Warn, false, strings.PAYGO_NAO_ESTA_INSTALADO);
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao executar função admin no TEF", ex);
                    DialogBox.Show(strings.VENDA_NO_TEF, DialogBoxButtons.No, DialogBoxIcons.Warn, false, strings.ERRO_INESPERADO, RetornarMensagemErro(ex, false));
                    return;
                }
            }
        }

        /// <summary>
        /// Executa bateria de testes de vendas.
        /// </summary>
        private void ExecTesteMassivo()
        {
            //    var rand = new Random();
            //    int contagemDeVendas = 0, contagemDeCancelamentos = 0, randomCancelamento = rand.Next(5, 6), roundDevendas = 0;


            //    #region Pegar o ID do último produto cadastrado - PEGA O ÚLTIMO NO MOMENTO
            //    int iDIdentificadorMax;
            //    using (var fbCommRemoveRegistroAuxSync = new FbCommand())
            //    using (var fbConnPdv = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            //    {
            //        fbConnPdv.Open();
            //        fbCommRemoveRegistroAuxSync.CommandText = $"SELECT MAX(ID_IDENTIFICADOR) FROM TB_EST_PRODUTO";
            //        fbCommRemoveRegistroAuxSync.CommandType = CommandType.Text;
            //        fbCommRemoveRegistroAuxSync.Connection = fbConnPdv;

            //        iDIdentificadorMax = fbCommRemoveRegistroAuxSync.ExecuteScalar().Safeint();
            //        fbConnPdv.Close();
            //    }
            //    #endregion Pegar o ID do último produto cadastrado - PEGA O ÚLTIMO NO MOMENTO


            //    while (_modoTeste)
            //    {
            //        if (!turno_aberto || _interromperModoTeste) { _modoTeste = false; return; }
            //        _modoTeste = true;

            //        #region Função Desativada
            //        //#region Pegar o ID do último produto cadastrado - SEMPRE PEGA O ÚLTIMO
            //        //int iDIdentificadorMax;
            //        //using (var fbCommRemoveRegistroAuxSync = new FbCommand())
            //        //using (var fbConnPdv = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            //        //{
            //        //    fbConnPdv.Open();
            //        //    fbCommRemoveRegistroAuxSync.CommandText = $"SELECT MAX(ID_IDENTIFICADOR) FROM TB_EST_PRODUTO";
            //        //    fbCommRemoveRegistroAuxSync.CommandType = CommandType.Text;
            //        //    fbCommRemoveRegistroAuxSync.Connection = fbConnPdv;

            //        //    iDIdentificadorMax = fbCommRemoveRegistroAuxSync.ExecuteScalar().Safeint();
            //        //    fbConnPdv.Close();
            //        //}
            //        //#endregion Pegar o ID do último produto cadastrado - SEMPRE PEGA O ÚLTIMO
            //        #endregion

            //        //Começar nova venda
            //        //int numDeProdutosAPassarNaVenda = rand.Next(1, 25);

            //        combobox.Text = $"{rand.NextDouble() + rand.Next(10)}*";
            //        combobox.Text = iDIdentificadorMax.ToString();
            //        try
            //        {
            //            ProcessarTextoNoACBox();
            //            INTERROMPE_NAO_ENCONTRADO = false;
            //            //if (lbl_Cortesia.Content.ToString().Equals("Produto não encontrado", StringComparison.InvariantCultureIgnoreCase)) { }
            //        }
            //        catch (Exception ex)
            //        {
            //            log.Error("Erro durante o teste massivo", ex);
            //            throw ex;
            //        }

            //        //TODO: se o caixa for não-fiscal, a venda não finaliza. Ver se a condição abaixo se repete ao longo da rotina de testes.
            //        if ((!SAT_USADO && !ECF_ATIVA) || _tipo == ItemChoiceType.FECHADO)
            //        {
            //            PrepararFinalizacaoDeCupomDemo();
            //        }
            //        else
            //        {
            //            PrepararFinalizacaoDeCupomFiscal();
            //        }

            //        LimparCupomVirtual(0);
            //        contagemDeVendas++;
            //        roundDevendas++;

            //        if (roundDevendas == randomCancelamento)
            //        {
            //            CancelarUltimoCupom();
            //            contagemDeCancelamentos++;
            //            roundDevendas = 0;
            //            randomCancelamento = rand.Next(20, 50);
            //        }
            //        Thread.Sleep(5000);
            //        log.Debug($"Venda número {contagemDeVendas}");
            //        log.Debug($"Cancelamento número {contagemDeCancelamentos}");
            //        log.Debug($"Round de Vendas {roundDevendas}");
            //        log.Debug($"Random Cancelamento {randomCancelamento}");
            //    }
            //    return;
        }

        /// <summary>
        /// Determina o como fechar o cupom fiscal
        /// </summary>
        private bool FecharCupomFiscalNovo(FechamentoCupom pFechamento)
        {
            //if (ECF_ATIVA || _modoTeste)
            if (ECF_ATIVA)
            {
                if (ProcessarVendaNoECF(pFechamento))
                {
                    LimparUltimaVenda();
                    log.Debug("Verificando se o caixa já está em modo de contigencia na finalização da venda Fiscal(...)");
                    if (_contingencia == false)
                    {
                        log.Debug("Foi verificado que o caixa não está em modo de contigencia, será checado novamente se a conexão persiste.");
                        ChecarPorContingencia(bar_Contingencia.IsVisible, Settings.Default.SegToleranciaUltSync, EnmTipoSync.tudo);
                    }
                    else
                    {
                        log.Debug("Foi verificado que o caixa já está em contigencia, assim pulando a checagem automatica!\n" +
                      "Caso deseje reestabelecer conexão com o servidor utilize as teclas 'CTRL+S'.");
                    }
                    return true;
                }
                else
                    return false;
            }
            if (SAT_USADO)
            {
                if (EnviaParaSAT(pFechamento))
                {
                    LimparUltimaVenda();
                    log.Debug("Verificando se o caixa já está em modo de contigencia na finaização da venda Fiscal(...)");
                    if(_contingencia == false)
                    {
                        log.Debug("Foi verificado que o caixa não está em modo de contigencia, será checado novamente se a conexão persiste.");
                        ChecarPorContingencia(bar_Contingencia.IsVisible, Settings.Default.SegToleranciaUltSync, EnmTipoSync.tudo);
                    }
                    else
                    {
                        log.Debug("Foi verificado que o caixa já está em contigencia, assim pulando a checagem automatica!\n" +
                        "Caso deseje reestabelecer conexão com o servidor utilize as teclas 'CTRL+S'.");
                    }
                }
                else
                {
                    erroVenda = true;
                    //MessageBox.Show("Erro ao enviar a venda para o SAT Servidor");
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determina se o cupom não fiscal poderá ser fechado
        /// </summary>
        /// <param name="pFechamento"></param>
        private void FecharCupomNaoFiscalNovo(FechamentoCupom pFechamento)
        {
            if (IMPRESSORA_USB == "Nenhuma")
            {
                log.Debug("Nenhuma impressora instalada");
                if (DialogBox.Show(strings.VENDA, DialogBoxButtons.YesNo, DialogBoxIcons.Info, false, strings.IMPRESSORA_NAO_CONFIGURADA) != false)
                {
                    if (!ImprimeESalvaCupomNaoFiscal(pFechamento))
                    {
                        erroVenda = true;
                        return;
                    }
                    VendaDEMO.produtos.Clear();
                    VendaDEMO.pagamentos.Clear();
                }
                return;
            }//Caso tente lançar um cupom NF sem configurar uma impressora.
            else if (IMPRESSORA_USB != "Nenhuma")
            {
                if (!ImprimeESalvaCupomNaoFiscal(pFechamento))
                {
                    erroVenda = true;
                    return;
                }
            }
            LimparUltimaVenda();
            log.Debug("Verificando se o caixa já está em modo de contigencia na finalização da venda Não Fiscal(...)");
            if (_contingencia == false)
            {
                log.Debug("Foi verificado que o caixa não está em modo de contigencia, será checado novamente se a conexão persiste.");
                ChecarPorContingencia(bar_Contingencia.IsVisible, 0, EnmTipoSync.tudo);
            }
            else
            {
                log.Debug("Foi verificado que o caixa já está em contigencia, assim pulando a checagem automatica!\n" +
                        "Caso deseje reestabelecer conexão com o servidor utilize as teclas 'CTRL+S'.");
            }
        }

        /// <summary>
        /// Fecha o cupom de devolução
        /// </summary>
        /// <param name="pFechamento"></param>
        private void FecharDevolucaoNovo()
        {
            decimal _qCom;
            decimal _vUnCom;
            decimal _vDesc;
            //using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
            using var NFCOMPRA_TA = new DataSets.FDBDataSetDevolucaoTableAdapters.TB_NFCOMPRATableAdapter();
            using var NFC_ITEM_TA = new DataSets.FDBDataSetDevolucaoTableAdapters.TB_NFC_ITEMTableAdapter();
            using var EST_PRODUTO = new DataSets.FDBDataSetDevolucaoTableAdapters.TB_EST_PRODUTOTableAdapter();
            using var ESTOQUE_TA = new DataSets.FDBDataSetDevolucaoTableAdapters.TB_ESTOQUETableAdapter();

            //OPER_TA.Connection = LOCAL_FB_CONN;
            //int intItemCup;
            int NFCOMPRA_ID = Convert.ToInt32(NFCOMPRA_TA.NextIdNFCompra());
            int NF_NUMERO = Convert.ToInt32(NFCOMPRA_TA.PegaProxNFNum(0));
            NFCOMPRA_TA.Insert(NFCOMPRA_ID, null, 0, NF_NUMERO, "2", "55", DateTime.Today, DateTime.Today, DateTime.Today, null, null, null, null, "0", 0, 0, "E", 9, null, devolAtual.RetornaListaDets().Count, 26, 3, null, null, null, null, null, null, null, null);
            int sequencial = 1;

            foreach (envCFeCFeInfCFeDet item in devolAtual.RetornaListaDets())
            {
                _qCom = decimal.Parse(item.prod.qCom, ptBR);
                _vUnCom = decimal.Parse(item.prod.vUnCom, ptBR);
                _vDesc = decimal.Parse(item.prod.vDesc, ptBR);

                try
                {
                    //using var FBComm = new FbCommand
                    //{
                    //    Connection = SERVER_FB_CONN,

                    //    CommandText = "SP_TRI_PRODUTO_RETIRAESTOQUE",
                    //    CommandType = CommandType.StoredProcedure
                    //};

                    //FBComm.Parameters.Add("@pQTD_ITEM", -_qCom);
                    //FBComm.Parameters.Add("@pID_IDENTIF", int.Parse(item.prod.cProd, ptBR));
                    //FBComm.Parameters.Add("@pID_COMPPRO", 0);
                    //FBComm.Parameters.Add("@pID_COMPOSICAO", 0);



                    int ID_IDENTIFICADOR = int.Parse(item.prod.cProd, ptBR);
                    int NFCITEM_ID = Convert.ToInt32(NFC_ITEM_TA.NextNFCItemID());
                    NFC_ITEM_TA.Insert(NFCITEM_ID, ID_IDENTIFICADOR, NFCOMPRA_ID, (short)sequencial, _qCom, item.prod.uCom, _vUnCom, _vDesc, null, null, 0, 0, 0, 0, null, null, "1202", null, null, null, "S", 0);
                    EST_PRODUTO.UpdateQuery(_qCom, DateTime.Now, ID_IDENTIFICADOR);
                    sequencial++;

                }
                catch (Exception ex)
                {
                    log.Error("Erro ao fechar a devolução (novo)", ex);
                    DialogBox.Show(strings.VENDA,
                                   DialogBoxButtons.No, DialogBoxIcons.Error, true,
                                   strings.ERRO_INESPERADO, RetornarMensagemErro(ex, false));
                    return;
                }

            }
            try
            {
                //PrintDEVOLOld.IMPRIME();
            }
            catch (Exception ex)
            {
                log.Error("Erro na devolução", ex);
                DialogBox.Show(@"Erro na Devolução/Troca", DialogBoxButtons.No, DialogBoxIcons.Error, true, "Verifique os logs");
            }
            finally
            {
                ImprimirCupomVirtual(@"TOTAL:" + ("R$ " + subtotal.ToString("0.00")).PadLeft(39, ' ') + @" ");
                txb_Avisos.Text = string.Format("ITEM(NS) RETORNADOS");
                _emTransacao = false;
                subtotal = 0;
                txb_Qtde.Clear();
                txb_TotGer.Clear();
                txb_TotProd.Clear();
                txb_ValorUnit.Clear();
                LimparCupomVirtual(5000);
                cupomVirtual.Clear();
                cupomVirtual.Append(@"{\rtf1\pc ");
                _tipo = ItemChoiceType.FECHADO;
            }

            LimparTela();
            LimparUltimaVenda();
            ChecarPorContingencia(bar_Contingencia.IsVisible, 0, EnmTipoSync.tudo);
            LimparObjetoDeVendaNovo();
        }


        /// <summary>
        /// "Consome" o orçamento. Não será possível editar ou reutilizá-lo.
        /// Seta "Status" para "FECHADO" e vincula o orçamento com o cupom.
        /// </summary>
        private void FecharOrcamento(int pID_NFVENDA)
        {
            //bool blnFechadoComSucesso = false;

            try
            {
                using var taOrcaServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_ORCAMENTOSTableAdapter();
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                //taOrcaServ.Connection = LOCAL_FB_CONN;

                taOrcaServ.SP_TRI_ORCA_FECHAORCA(orcamentoAtual.no_orcamento,
                                                 pID_NFVENDA,
                                                 Convert.ToInt16(NO_CAIXA));
                //blnFechadoComSucesso = true;
            }
            catch (Exception ex)
            {
                // TODO: um erro possível pode ser a utilização de um orçamento em mais de um caixa ao mesmo tempo.
                // Pode engatilhar violação de chave primária em TRI_PDV_ORCA_CUPOM_REL (ID_ORCAMENTO e ID_CUPOM).
                // Se acontecer, o primeiro consumo deve fechar o orçamento e os seguintes apresentar erro, mas deverão
                // fechar a venda normalmente.

                log.Error("Erro ao fechar orçamento", ex);
                DialogBox.Show("Orçamento", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao fechar orçamento! Verifique o Log de erro!");
            }
            finally
            {
                //if (blnFechadoComSucesso) { usou_orcamento = false; }
                _usouOrcamento = _usouPedido = _usouOS = false; // Independentemente do resultado deste método, deve indicar o término do uso do orçamento na venda, para não comprometer o funcionamento subsequente.
            }
        }

        /// <summary>
        /// Determina se o cupom é fiscal ou não ou se é uma devolução
        /// </summary>
        private void FinalizarVendaNovo()
        {
            //ChecaPorContingencia(contingencia, EnmTipoSync.cadastros);
            if (subtotal > 0)
            {                
                #region AmbiMAITRE

                if (IMPRESSORA_USB_PED != "Nenhuma")
                {
                    using var MAIT_PED_TA = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PEDIDOTableAdapter();
                    using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                    MAIT_PED_TA.Connection = LOCAL_FB_CONN;
                    if (_contingencia)
                    {
                        DialogBox.Show(strings.PEDIDO,
                                       DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                       strings.SISTEMA_ESTA_EM_CONTINGENCIA,
                                       strings.PEDIDO_NAO_FOI_ENVIADO_PARA_IMPRESSORA);
                    }
                    try
                    {
                        maitNrPedido = (int)MAIT_PED_TA.SP_TRI_MAITRE_PEDIDO_NR_GET(Settings.Default.PedidoMax);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Erro ao gerar o número do pedido", ex);
                        MessageBox.Show("Erro ao gerar número do pedido. \nPor favor tente novamente. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                        return;
                    }
                }

                #endregion AmbiMAITRE
                //Pega valor do textBox Total Geral fiquei com preguiça de procurar a variavel que armazena esse valor então peguei do textbox.
                string tot = txb_TotGer.Text;
                string[] totConvert = tot.Split(' ');
                decimal.TryParse(totConvert[1], out decimal totConvertido);
                FechamentoCupom.vlrTotalVenda = totConvertido;
                var fechamento = new FechamentoCupom(DESCONTO_MAXIMO, ref vendaAtual, _modoTeste)
                {
                    //valor_venda = subtotal,
                    _info_int = infoStr,
                    _tipo_int = _tipo
                };

                log.Debug("Cupom sem ser devolução a ser finalizado");
                try
                {                                      
                    fechamento.ShowDialog();
                    //vendaAtual = fechamento._vendaAtual;
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao finalizar a venda", ex);
                    throw;
                }

                if (fechamento.DialogResult == false)
                {
                    log.Debug("Finaliza cupom. Fechamento cancelado");
                    _tipo = ItemChoiceType.ABERTO;
                    return;
                }
                else if (fechamento.DialogResult == true) //Caso o fechamento tenha sido bem sucedido ou é um processo de devolução:
                {
                    //oldCRT = fechamento.respCRT;
                    foreach ((string strCfePgto, decimal vlrPgto) metodo in fechamento.metodosnew)
                    {
                        if (metodo.strCfePgto == "05")
                            vendaAtual.RecebePagamento(metodo.strCfePgto.PadLeft(2, '0'), metodo.vlrPgto, fechamento.vencimento, fechamento.id_cliente);
                        else if (metodo.strCfePgto == "01")
                            vendaAtual.RecebePagamento(metodo.strCfePgto.PadLeft(2, '0'), metodo.vlrPgto, fechamento.troco);
                        else if ((metodo.strCfePgto == "04" || metodo.strCfePgto == "03") && USATEF)
                        {
                            vendaAtual.RecebePagamento(metodo.strCfePgto.PadLeft(2, '0'), metodo.vlrPgto);
                        }
                        else
                            vendaAtual.RecebePagamento(metodo.strCfePgto.PadLeft(2, '0'), metodo.vlrPgto);

                    }

                    if (!vendaAtual.TotalizaCupom())
                    {
                        _tipo = ItemChoiceType.ABERTO;
                        return;
                    }
                    if (_tipo == ItemChoiceType.CPF || _tipo == ItemChoiceType.CNPJ || _tipo == ItemChoiceType.NENHUM)
                    {
                        log.Debug("Fechamento FISCAL");
                        try
                        {
                            FecharCupomFiscalNovo(fechamento);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Erro ao fechar o cupom fiscal", ex);
                            MessageBox.Show("LogERR.");
                            return;
                        }
                    }//Fechamento fiscal.
                    else if (_tipo == ItemChoiceType.DEMONSTRACAO)
                    {
                        log.Debug("Fechamento não fiscal");

                        try
                        {
                            FecharCupomNaoFiscalNovo(fechamento);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Erro ao fechar cupom não fiscal", ex);
                            MessageBox.Show("LogERR");
                            return;
                            //deuruim();
                        }
                    }//Fechamento não fiscal.
                }
                else if (fechamento.DialogResult == null)
                {

                    log.Debug("Finaliza cupom. Fechamento nulo. Tentou novamente");

                    FinalizarVendaNovo();
                    return;
                } //Caso o fechamento tenha sido cancelado, ou não tenha sido bem sucedido.

                //if (_usouOrcamento) { FecharOrcamento(intNumeroCupomParaFechamentoDeOrcamento); }

                //if (usou_pedido) { FecharPedido(intNumeroCupomParaFechamentoDePedido); }

                #region AmbiMAITRE

                if (IMPRESSORA_USB_PED != "Nenhuma")
                {
                    PrintMaitrePEDIDO.no_pedido = maitNrPedido;
                    PrintMaitrePEDIDO.IMPRIME(_contingencia);
                    //TODO: Escreve informação na tabela de pedidos de fabricação.
                }

                #endregion AmbiMAITRE

                var printTEF = new ComprovanteSiTEF();
                foreach (SiTEFBox tef in fechamento.tefUsados)
                {
                    if (tef.Status == StatusTEF.Confirmado && vendaAtual.imprimeViaCliente && !erroVenda)
                        try
                        {
                            if (!(numeroWhats is null))
                            {
                                new PerguntaNumWhats(tef._viaCliente, numeroWhats);
                                numeroWhats = null;
                            }//HACK
                            else
                                printTEF.IMPRIME(tef._viaCliente);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                }
                foreach (SiTEFBox tef in fechamento.tefUsados)
                {
                    if (tef.Status == StatusTEF.Confirmado && !erroVenda)
                        try
                        {
                            printTEF.IMPRIME(tef._viaLoja);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    tef.FinalizaOperacaoTEF(tef.numPagamentoTEF, erroVenda);
                }
            }


            //Caso tenha sido um cupom inutilizado ou zerado:
            else if (subtotal == 0)
            {
                LimparUltimaVenda();
                LimparTela();
                _usouOrcamento = _usouPedido = _usouOS = false; //Caso o cliente "desista" da venda do orçamento limpando os produtos do cupom e posteriormente pressionando F2 ou F3
                orcamentoAtual.Clear();
                pedidoAtual?.Clear();
                LimparObjetoDeVendaNovo();
            }
            if (!erroVenda)
            {
                LimparObjetoDeVendaNovo();
                AtualizarRetroTabelas();
                DeterminarStatusDeSangria(false);
            }
        }


        /// <summary>
        /// Lança texto no cupom virtual (na tela)
        /// </summary>
        /// <param name="pText"></param>
        private void ImprimirCupomVirtual(string pText)
        {
            var pg = new Paragraph
            {
                Margin = new Thickness(0),
                TextAlignment = TextAlignment.Left,
                TextIndent = indentdaMargem
            };
            pg.Inlines.Add(new Run(pText));
            richTextBox1.Document.Blocks.Add(pg);
            richTextBox1.Focus();
            richTextBox1.ScrollToEnd();
            combobox.Focus();
        }

        /// <summary>
        /// Finaliza a venda informada usando a impressora não-fiscal, e preenche o objeto de impressão.
        /// </summary>
        /// <param name="pFechamento">Fechamento a ser processado</param>
        private bool ImprimeESalvaCupomNaoFiscal(FechamentoCupom pFechamento)
        {
            var _metodos_de_pagamento = new Dictionary<string, string>
                        {
                            { "01", "Dinheiro" },
                            { "02", "Cheque" },
                            { "03", "Cartão Crédito" },
                            { "04", "Cartão Débito" },
                            { "05", "Crédito Loja" },
                            { "10", "Vale Aliment." },
                            { "11", "Vale Refeição" },
                            { "12", "PIX" },
                            { "13", "Vale Combustível" },
                            { "20", "PIX" },
                            { "99", "Outros" }
                        };//Dicionário de métodos de pagamento.
            var cFeDeRetorno = vendaAtual.RetornaCFe();
            int venda_prazo = 0;
            //bool usouTEF = false;

            VendaDEMO.operadorStr = operador;
            VendaDEMO.num_caixa = NO_CAIXA;

            //TODO: Permitir vencimento/id_cliente POR MÉTODO DE PAGAMENTO
            //for (int i = 0; i < pFechamento.pagamentos.Count; i++)
            //{
            //    if (cFeDeRetorno.Pgto.MP[i].cMP == "05")
            //    {
            //        cFeDeRetorno.Pgto.MP[i].idCliente = pFechamento.
            //    }
            //}
            //TODOEND

            foreach (var item in cFeDeRetorno.infCFe.pgto.MP)
            {
                if (item.cMP == "05") item.idCliente = pFechamento.id_cliente; item.vencimento = pFechamento.vencimento;
            }


            string valorcfe = cFeDeRetorno.infCFe.total.vCFe;

            VendaDEMO.vendedor = vendedorString;
            log.Debug(" Produtos computado: cProd, xProd, uCom, qCom, vUnCom, vDesc, ICMS, TAXAIBPT");


            var Funcoes = new funcoesClass();
            decimal _qCom, _vUnCom, _vDesc;
            foreach (envCFeCFeInfCFeDet item in cFeDeRetorno.infCFe.det)
            {
                _qCom = decimal.Parse(item.prod.qCom, ptBR);
                _vUnCom = decimal.Parse(item.prod.vUnCom, ptBR);
                _vDesc = decimal.Parse(string.IsNullOrWhiteSpace(item.prod.vDesc) ? "0" : item.prod.vDesc, ptBR)/* + item.descAtacado*/;

                if (string.IsNullOrWhiteSpace(item.prod.NCM) || !Funcoes.ConsultarTaxasPorNCM(item.prod.NCM, out decimal taxa_fed, out decimal taxa_est, out decimal taxa_mun))
                {
                    VendaDEMO.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, 0, 0, 0, item.prod.vUnComOri.Safedecimal());
                    log.Debug($"{item.prod.cProd}, {item.prod.xProd}, {item.prod.uCom}, {_qCom}, {_vUnCom}, {_vDesc}, {0}, {0}, {0}");

                }
                else
                {
                    VendaDEMO.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, taxa_est, taxa_fed, taxa_mun, item.prod.vUnComOri.Safedecimal());
                    log.Debug($"{item.prod.cProd}, {item.prod.xProd}, {item.prod.uCom}, {_qCom}, {_vUnCom}, {_vDesc}, {taxa_est}, {taxa_fed}, {taxa_mun}");
                    // item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, taxa_est, taxa_fed, taxa_mun));
                }
            }

            var (nFNumero, ID_NFVENDA) = vendaAtual.GravaNaoFiscalBase(pFechamento.troco, NO_CAIXA, (short?)vendedorId ?? 0, false);
            VendaDEMO.numerodocupom = nFNumero;

            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                string strMensagemLogLancaContaRec = string.Empty;
                string strMensagemLogLancaMovDiario = string.Empty;
                var _vMP = 0m;
                decimal valor_prazo = 0;

                using (var CONTAREC_TA = new TB_CONTA_RECEBERTableAdapter())
                using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                {
                    OPER_TA.Connection = LOCAL_FB_CONN;
                    CONTAREC_TA.Connection = LOCAL_FB_CONN;

                    foreach (envCFeCFeInfCFePgtoMP item in pFechamento.metodos)
                    {
                        VendaDEMO.RecebePagamento(_metodos_de_pagamento[item.cMP.ToString()], item.dec_vMP);
                        log.Debug($"FISCAL>> Pagamento efetuado: {_metodos_de_pagamento[item.cMP.ToString()]}, Valor {item.dec_vMP}");
                        if (USATEF == true && (item.cMP == "03" || item.cMP == "04"))
                        {
                            //usouTEF = true;
                            log.Debug("TEF UTILIZADO!!!!!");
                        }
                        if (item.cMP == "05")
                        {
                            valor_prazo = item.dec_vMP;
                        }
                    }
                }
                VendaDEMO.desconto = pFechamento.desconto;
                log.Debug($"Desconto: {pFechamento.desconto}");
                if (pFechamento.troco > 0.01m)
                {
                    VendaDEMO.troco = pFechamento.troco.ToString("0.00");
                }
                else
                {
                    VendaDEMO.troco = "0,00";
                }
                if (!(cFeDeRetorno.infCFe.infAdic is null) && !(cFeDeRetorno.infCFe.infAdic.obsFisco is null))
                {
                    VendaDEMO.observacaoFisco = (cFeDeRetorno.infCFe.infAdic.obsFisco[0].xCampo, cFeDeRetorno.infCFe.infAdic.obsFisco[0].xTexto);
                }
                log.Debug($"Troco: {pFechamento.troco}");
                try
                {
                    if (vendaAtual.imprimeViaAssinar)
                    {
                        VendaDEMO.cliente = pFechamento.nome_cliente;
                        VendaDEMO.vencimento = pFechamento.vencimento;
                        VendaDEMO.valor_prazo = valor_prazo;
                        venda_prazo += 1;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao imprimir e salvar cupom não fiscal", ex);
                    DialogBox.Show(strings.IMPRESSAO,
                                   DialogBoxButtons.No, DialogBoxIcons.Warn, true,
                                   strings.ERRO_AO_ENVIAR_IMPRESSAO_DO_TEF_PARA_SPOOLER,
                                   RetornarMensagemErro(ex, false));
                    return false;
                }

                if (_usouOrcamento) FecharOrcamento(ID_NFVENDA);
                if (_usouOS) _funcoes.FechaOrdemDeServico(new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) }, ordemDeServico.ID_OS);
                #region IMPRESSÃO DE CUPOM VIRTUAL

                ImprimirCupomVirtual($"TOTAL: (R$ {subtotal,39:N2}");
                if (pFechamento.desconto != 0)
                {
                    ImprimirCupomVirtual($"DESCONTO: R$ {pFechamento.desconto,36}");
                }

                foreach (envCFeCFeInfCFePgtoMP item in cFeDeRetorno.infCFe.pgto.MP)
                {
                    _vMP = decimal.Parse(item.vMP, ptBR);
                    ImprimirCupomVirtual(_metodos_de_pagamento[item.cMP] + "R$ " + _vMP.ToString().PadLeft(42 - _metodos_de_pagamento[item.cMP.ToString()].Length));
                }
                if (pFechamento.troco > 0)
                {
                    ImprimirCupomVirtual(@"TROCO" + pFechamento.troco.ToString().PadLeft(45 - "Troco".Length) + @"  ");
                }

                #endregion IMPRESSÃO DE CUPOM VIRTUAL

                txb_Avisos.Text = $"TROCO: {pFechamento.troco.ToString("C2")}";
                _emTransacao = false;
                numProximoItem = 1;
                subtotal = 0;
                txb_Qtde.Clear();
                txb_TotGer.Clear();
                txb_TotProd.Clear();
                txb_ValorUnit.Clear();
                LimparCupomVirtual(5000);
                cupomVirtual.Clear();
                cupomVirtual.Append(@"{\rtf1\pc ");
                _tipo = ItemChoiceType.FECHADO;


                foreach (var item in pFechamento.pagamentos)
                {
                    if (item.Value < 0)
                    {
                        MessageBox.Show("Erro ao lançar cupom. Um dos valores informados era negativo.");
                        log.Debug("Erro ao lançar cupom. Um dos valores informados era negativo.");
                        return false;
                    }
                }
                if (_usouOS) _funcoes.FechaOrdemDeServico(new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) }, ordemDeServico.ID_OS);

                if (_usouOrcamento) FecharOrcamento(ID_NFVENDA);

                try
                {

                    subtotal = 0;
                    log.Debug($"Caixa {NO_CAIXA}, Cupom: {noCupom}, MODO NÃO FISCAL");

                    if (pFechamento.devolucoes_usadas.Count > 0)
                    {
                        using var DEVOL_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_DEVOLTableAdapter()
                        {
                            Connection = new FbConnection() { ConnectionString = MontaStringDeConexao("localhost", localpath) }
                        };
                        foreach ((int, decimal) item in pFechamento.devolucoes_usadas)
                        {
                            DEVOL_TA.RedeemaDevolucao(item.Item1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao imprimir e salvar cupom não fiscal", ex);
                    DialogBox.Show(strings.VENDA,
                                   DialogBoxButtons.No,
                                   DialogBoxIcons.Error, true,
                                   strings.ERRO_DURANTE_VENDA,
                                   RetornarMensagemErro(ex, false));
                    return false;
                }
            }

            DecisaoWhats resultado;
            switch (SYSEMITECOMPROVANTE)
            {
                case 0:
                    resultado = DecisaoWhats.NaoImprime;
                    break;
                case 1:
                    switch (SYSUSAWHATS.ToBool())
                    {
                        case false:
                            resultado = DecisaoWhats.ImpressaoNormal;
                            break;
                        default:
                            resultado = DecisaoComprovante(true);
                            break;
                    }
                    break;
                default:
                    resultado = DecisaoComprovante(true);
                    break;
            }

            switch (resultado)
            {
                case DecisaoWhats.Whats:
                    PerguntaNumWhats whats = new PerguntaNumWhats("NF", vendaAtual);
                    //Pergunta o número do Uatizápi
                    whats.ShowDialog();
                    numeroWhats = whats.number.ToString();
                    log.Debug("PASSOU PELO PERGUNTA WHATS NFISCAL");
                    break;
                case DecisaoWhats.NaoImprime:
                    vendaAtual.imprimeViaCliente = false;
                    if (FORÇA_GAVETA) AbreGaveta();
                    break;

                case DecisaoWhats.ImpressaoNormal:
                    try
                    {
                        ultimaImpressao = VendaDEMO.IMPRIME(venda_prazo, cFeDeRetorno);
                        if (vendaAtual.imprimeViaAssinar)
                        {
                            VendaDEMO.IMPRIME(1);
                        }

                        if (PERMITE_ESTOQUE_NEGATIVO == null && RelNegativ.produtos.Count > 0)
                        {
                            log.Debug("Relatório de Itens negativos impresso.");
                            RelNegativ.IMPRIME();
                        }

                        noCupom = 0;
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha na impressão", ex);
                        MessageBox.Show("FinalizaNoSATLocal - Falha na impressão");
                        return false;
                    }
                    break;

            }
            #region PERGUNTAWHATS
            //if (PERGUNTA_WHATS == PerguntaWhatsEnum.Sempre)
            //{
            //    var vendaPrazo = vendaAtual.RetornaCFe().infCFe.pgto;
            //    if (vendaPrazo.MP[0].cMP.Equals("05"))
            //    {
            //        //Se houve venda a prazo, não é permitido Uatizápi
            //    }
            //    else
            //    {
            //        string CfeNaoFiscal = "NF";
            //        PerguntaNumWhats whats = new PerguntaNumWhats(CfeNaoFiscal, vendaAtual);
            //        //Pergunta o número do Uatizápi
            //        whats.ShowDialog();
            //    }
            //}
            //if (PERGUNTA_WHATS == PerguntaWhatsEnum.TeclaAtalho)//Menu de opção
            //{
            //    bool veredito = false;
            //    while (!veredito)
            //    {

            //        var vendaPrazo = vendaAtual.RetornaCFe().infCFe.pgto;
            //        OpcoesDeImpressao nova = new OpcoesDeImpressao(vendaPrazo.MP[0].cMP);
            //        string CfeNaoFiscal = "NF";
            //        PerguntaNumWhats whats = new PerguntaNumWhats(CfeNaoFiscal, vendaAtual);

            //        while (nova.ShowDialog() != true) { };

            //        if (nova.veredito == DecisaoWhats.Whats)
            //        {
            //            switch (whats.ShowDialog())
            //            {
            //                case true:
            //                    veredito = true;
            //                    nova.Close();
            //                    break;
            //                case false:
            //                    break;
            //                default:
            //                    return false;

            //            }

            //        }

            //        if (nova.ImprimeWhats() == DecisaoWhats.ImpressaoNormal)
            //        {
            //            try
            //            {
            //                if ((Settings.Default.SYSConfirmaImpressao &&
            //                                       DialogBox.Show("Impressão de Cupom", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja imprimir o cupom?") == true
            //                                       ) || !Settings.Default.SYSConfirmaImpressao)
            //                {
            //                    ultimaImpressao = VendaDEMO.IMPRIME(venda_prazo, cFeDeRetorno);
            //                    if (vendaAtual.imprimeViaAssinar)
            //                    {
            //                        VendaDEMO.IMPRIME(1);
            //                    }
            //                }
            //                if (PERMITE_ESTOQUE_NEGATIVO == null && RelNegativ.produtos.Count > 0)
            //                {
            //                    log.Debug("Relatório de Itens negativos impresso.");
            //                    RelNegativ.IMPRIME();
            //                }

            //                noCupom = 0;
            //                //usouTEF = false;
            //            }
            //            catch (Exception ex)
            //            {
            //                log.Error("Falha na impressão", ex);
            //                MessageBox.Show("FinalizaNoSATLocal - Falha na impressão");
            //                return false;
            //            }
            //            veredito = true;
            //            nova.Close();
            //        }
            //        if (nova.ImprimeWhats() == DecisaoWhats.NaoImprime)
            //        {
            //            if (vendaPrazo.MP[0].cMP.Equals("05"))
            //            { MessageBox.Show("Venda a prazo tem impressão obrigatória.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
            //            else
            //            { veredito = true; }
            //        }

            //    }

            //}
            //if (!_modoTeste && PERGUNTA_WHATS == PerguntaWhatsEnum.Nunca) //Nunca
            //{
            //    try
            //    {
            //        if ((Settings.Default.SYSConfirmaImpressao &&
            //                               DialogBox.Show("Impressão de Cupom", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja imprimir o cupom?") == true
            //                               ) || !Settings.Default.SYSConfirmaImpressao)
            //        {
            //            ultimaImpressao = VendaDEMO.IMPRIME(venda_prazo, cFeDeRetorno);
            //            if (vendaAtual.imprimeViaAssinar)
            //            {
            //                VendaDEMO.IMPRIME(1);
            //            }
            //        }

            //        if (PERMITE_ESTOQUE_NEGATIVO == null && RelNegativ.produtos.Count > 0)
            //        {
            //            log.Debug("Relatório de Itens negativos impresso.");
            //            RelNegativ.IMPRIME();
            //        }

            //        noCupom = 0;
            //        //usouTEF = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error("Erro ao imprimir e salvar cupom não fiscal", ex);
            //        MessageBox.Show("FinalizaNoSATLocal - Falha na impressão");
            //        return false;
            //    }
            //}
            #endregion PERGUNTAWHATS
            VendaDEMO.Clear();
            vendedorId = null;
            return true;
        }

        private DecisaoWhats DecisaoComprovante(bool permiteNenhuma)
        {
            var vendaPrazo = vendaAtual.RetornaCFe().infCFe.pgto;
            OpcoesDeImpressao nova = new OpcoesDeImpressao(vendaPrazo.MP[0].cMP, permiteNenhuma);
            while (nova.ShowDialog() != true) { };
            return nova.veredito;
        }

        /// <summary>
        /// Finaliza a venda informada usando um ECF.
        /// </summary>
        /// <param name="pFechamento">Fechamento a ser processado</param>
        private bool ProcessarVendaNoECF(FechamentoCupom pFechamento)
        {
            //int tef_cliente = 0;
            int tef_estabel = 0;
            //int tef_unica = 0;
            //int tef_redux = 0;
            decimal valor_prazo = 0;
            //bool usouTEF = false;

            //TODO: rRetornarInformacao_ECF_Daruma(57, out vStatus)
            // Retorna em vStatus o estado do CF/CNF: diferente de 0, cupom aberto.


            log.Debug("Venda no ECF");
            if (!_modoTeste)
            {
                #region ImprimirNoECF
                ECF.AbreGaveta();
                int resposta;
                switch (_tipo)
                {
                    case ItemChoiceType.CNPJ:
                    case ItemChoiceType.CPF:
                        resposta = UnsafeNativeMethods.iCFAbrir_ECF_Daruma(infoStr, "", "");
                        log.Debug($"_info = {infoStr}, iCFAbrirECFDaruma = {resposta} ");
                        break;
                    case ItemChoiceType.NENHUM:
                        resposta = UnsafeNativeMethods.iCFAbrirPadrao_ECF_Daruma();
                        log.Debug($"iCFAbrirPadraoECFDaruma = {resposta}");
                        break;
                    default:
                        resposta = UnsafeNativeMethods.iCFAbrirPadrao_ECF_Daruma();
                        log.Debug($"iCFAbrirPadraoECFDaruma = {resposta}");
                        break;
                }

                switch (resposta)
                {
                    case 1:
                        log.Debug($"Resposta FECHA_ECF: {resposta}");
                        int erro = UnsafeNativeMethods.eRetornarErro_ECF_Daruma();
                        switch (erro)
                        {
                            case 0:
                                break;
                            case 88:
                                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.NECESSARIO_FAZER_REDUCAO_Z);
                                log.Debug("ECF RETORNOU ERRO 88 - Redução Z pendente");
                                return false;
                            case 89:
                                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.REDUCAO_Z_JA_FOI_FEITA, strings.NAO_E_POSSIVEL_VENDER_ATE_AMANHA, strings.NAO_HA_SUPORTE_DISPONIVEL);
                                log.Debug("ECF RETORNOU ERRO 89 - Redução Z já feita");
                                return false;
                            case 144:
                                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.ECF_DESCONECTADA);
                                log.Debug("ECF RETORNOU ERRO 144 - ECF Desconectada");
                                return false;
                            case 72:
                                _tipo = ItemChoiceType.ABERTO;
                                DialogBox.Show(strings.VENDA,
                                               DialogBoxButtons.No, DialogBoxIcons.Info, false,
                                               strings.ECF_SEM_PAPEL,
                                               strings.TENTE_NOVAMENTE);
                                return false;
                            case 78:
                                UnsafeNativeMethods.iCFCancelar_ECF_Daruma();
                                _tipo = ItemChoiceType.ABERTO;
                                DialogBox.Show(strings.VENDA,
                                               DialogBoxButtons.No, DialogBoxIcons.Info, false,
                                               strings.ECF_COM_CUPOM_ABERTO,
                                               strings.TENTE_NOVAMENTE);
                                erroVenda = true;
                                return false;

                            default:
                                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.ERRO_INESPERADO, erro.ToString());
                                log.Debug($"ECF RETORNOU ERRO {erro}");
                                return false;
                        }
                        break;
                    case -6:
                        {
                            DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.ECF_NAO_ENCONTRADA);
                            log.Debug("ECF RETORNOU ERRO -6 - ECF Desconectada");
                            erroVenda = true;
                            return false;
                        }
                    default:
                        break;
                }

                string _NCM = string.Empty;
                //decimal _qCom = 0m;
                //decimal _vUnCom = 0m;
                //decimal _vDesc = 0m;
                foreach (envCFeCFeInfCFeDet item in vendaAtual.RetornaCFe().infCFe.det)
                {
                    //_NCM = (item.prod.NCM is null) ? "" : item.prod.NCM;
                    //  Declaracoes.confCFNCM_ECF_Daruma(_NCM, "0");
                    // Declaracoes.iCFVender_ECF_Daruma("T1800", item.prod.qCom.Substring(0, item.prod.qCom.Length - 1), item.prod.vUnCom.Substring(0, item.prod.vUnCom.Length - 1), "D$", item.prod.vDesc, item.prod.cProd, item.prod.uCom, item.prod.xProd);
                    ECF.VendeProdutoECF(item);
                    #region AmbiMAITRE

                    //if (Settings.Default.PRTUSBPedido != "Nenhuma")
                    //{
                    //    _qCom = decimal.Parse(item.prod.qCom.Replace(',', '.'), CultureInfo.InvariantCulture);
                    //    _vUnCom = decimal.Parse(item.prod.vUnCom.Replace(',', '.'), CultureInfo.InvariantCulture);
                    //    _vDesc = decimal.Parse(item.prod.vDesc.Replace(',', '.'), CultureInfo.InvariantCulture);

                    //    PrintMaitrePEDIDO.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, 0, 0, 0);
                    //}

                    #endregion AmbiMAITRE
                }

                if (!(vendaAtual.RetornaCFe().infCFe.total?.DescAcrEntr is null))
                {
                    decimal.TryParse(vendaAtual.RetornaCFe().infCFe.total.DescAcrEntr.Item, out decimal _desconto_ecf);
                    if (_desconto_ecf > 0)
                    { UnsafeNativeMethods.iCFTotalizarCupom_ECF_Daruma("D$", vendaAtual.RetornaCFe().infCFe.total.DescAcrEntr.Item); }
                    else
                    {
                        UnsafeNativeMethods.iCFTotalizarCupomPadrao_ECF_Daruma();
                    }
                }
                else
                {
                    UnsafeNativeMethods.iCFTotalizarCupomPadrao_ECF_Daruma();
                }

                log.Debug("FinalizarCupomPadrao");
                //using (var Metodos_DT = new FDBDataSet.TRI_PDV_METODOSDataTable())
                //using (var METODOS_TA = new TRI_PDV_METODOSTableAdapter())
                using (var METODOS_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                using (var METODOS_DT = new DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable())
                using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                {
                    //if (contingencia)
                    //{
                    METODOS_TA.Connection = LOCAL_FB_CONN;
                    METODOS_TA.FillByAtivos(METODOS_DT);
                    //}
                    //else
                    //{
                    //    METODOS_TA.Connection = SERVER_FB_CONN;
                    //}

                    //TODO: ou seria:
                    //for (int i = 0; i <= fechamento.nomes_pgtos.Count -1; i++)
                    string DESCRICAO_PGTO = "Dinheiro";
                    string strRetornoPegaPgCfeDesc = string.Empty;
                    for (int i = 0; i < pFechamento.nomes_pgtos.Count; i++)
                    {
                        //VendaDEMO.RecebePagamento(fechamento.nomes_pgtos[i], (decimal)fechamento.valores_pgtos[i]);
                        DESCRICAO_PGTO = "Dinheiro";
                        strRetornoPegaPgCfeDesc = string.Empty;
                        try
                        {
                            strRetornoPegaPgCfeDesc = (from linha in METODOS_DT
                                                       where linha.DESCRICAO == pFechamento.nomes_pgtos[i]
                                                       select linha.ID_NFCE).FirstOrDefault();

                            //strRetornoPegaPgCfeDesc = METODOS_TA.PegaPGCFE_DESC(pFechamento.nomes_pgtos[i]);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Erro ao processar venda no ECF", ex);
                            MessageBox.Show("Erro ao verificar método de pagamento. \nPor favor tente novamente. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                            int intRetornoCancelarEcfDaruma = UnsafeNativeMethods.iCFCancelar_ECF_Daruma();
                            if (intRetornoCancelarEcfDaruma != 1)
                            {
                                string strMensagemErro = "Erro ao cancelar cupom fiscal (ECF): " + intRetornoCancelarEcfDaruma.ToString();
                                log.Debug(strMensagemErro);
                                MessageBox.Show(strMensagemErro + "\n\nPor favor entre em contato com a equipe de suporte.");
                            }
                            return false; //deuruim ^ 2();
                        }

                        switch (strRetornoPegaPgCfeDesc)
                        {
                            case "01":
                                DESCRICAO_PGTO = "Dinheiro";
                                break;
                            case "02":
                                DESCRICAO_PGTO = "Cheque";
                                break;
                            case "03":
                            case "04":
                                DESCRICAO_PGTO = "Cartão";
                                break;
                            case "05":
                                DESCRICAO_PGTO = "Prazo";
                                break;
                            case "10":
                            case "11":
                            case "12":
                            case "13":
                                DESCRICAO_PGTO = "Ticket";
                                break;
                            case "99":
                                DESCRICAO_PGTO = "Dinheiro";
                                break;
                            default:
                                break;
                        }
                        log.Debug($"iCFEfetuarPagamentoFormatado_ECF_Daruma({DESCRICAO_PGTO}, {pFechamento.valores_pgtos[i]:0.00})");
                        UnsafeNativeMethods.iCFEfetuarPagamentoFormatado_ECF_Daruma(DESCRICAO_PGTO, pFechamento.valores_pgtos[i].ToString("0.00"));
                        log.Debug("Pagamento Processado pela ECF");

                        if (USATEF == true &&
                            (strRetornoPegaPgCfeDesc == "03" ||
                             strRetornoPegaPgCfeDesc == "04"))
                        {
                            //usouTEF = true;
                            log.Debug("TEF UTILIZADO!!!!!");
                        }
                    }
                }
                log.Debug("iCFEncerrarPadrao_ECF_Daruma");
                UnsafeNativeMethods.iCFEncerrarPadrao_ECF_Daruma();//Método leva aprox. 3 segundos pra ser executado. Fora de nosso controle...
                log.Debug("CFe Encerrado");
                #endregion ImprimirNoECF
            }
            #region IMPRESSÃO DE CUPOM VIRTUAL

            try
            {
                ImprimirCupomVirtual(@"TOTAL:" + subtotal.RoundABNT().ToString("0.00").PadLeft(39, ' ') + @" ");
                log.Debug($"TOTAL:{subtotal,39}");
                if (pFechamento.desconto != 0)
                {
                    ImprimirCupomVirtual(@"DESCONTO:" + pFechamento.desconto.RoundABNT().ToString("0.00").PadLeft(36, ' ') + @" ");
                    log.Debug($"DESCONTO: {pFechamento.desconto,36}");
                }
                for (int i = 0; i < pFechamento.nomes_pgtos.Count; i++)
                {
                    ImprimirCupomVirtual(pFechamento.nomes_pgtos[i] + pFechamento.valores_pgtos[i].ToString("0.00").PadLeft(45 - pFechamento.nomes_pgtos[i].Length) + @" ");
                    log.Debug($"{pFechamento.nomes_pgtos[i]} {pFechamento.valores_pgtos[i].ToString().PadLeft(45 - pFechamento.nomes_pgtos[i].Length)}");
                }
                if (pFechamento.troco >= 0.01M)
                {
                    ImprimirCupomVirtual(@"TROCO" + pFechamento.troco.RoundABNT().ToString("0.00").PadLeft(45 - "Troco".Length) + @"  ");
                    log.Debug($"Troco {pFechamento.troco.ToString().PadLeft(45 - "Troco".Length)}");
                }
            }
            catch (Exception ex)
            {
                log.Error("Erro ao processar venda no ECF", ex);
                //throw; deuruim();
            }

            #endregion IMPRESSÃO DE CUPOM VIRTUAL


            if (Settings.Default.ECFAuditoria)
            {
                string identificacao = !String.IsNullOrWhiteSpace(infoStr) ? infoStr : "Cliente não ident.";
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"AuditoriaECF\");
                string pathFile = AppDomain.CurrentDomain.BaseDirectory + @$"\AuditoriaECF\ECF_{DateTime.Today:ddMMyyyy}.txt";
                using StreamWriter sw = File.AppendText(pathFile);
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" + identificacao + "\t" + pFechamento.valores_pgtos.Sum().ToString());
                sw.Close();
            }

            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })

            {
                #region Imprime ECF

                try
                {
                    if (vendaAtual.imprimeViaAssinar)
                    {
                        foreach (envCFeCFeInfCFePgtoMP item in pFechamento.metodos)
                        {
                            if (item.cMP == "05")
                            {
                                valor_prazo = item.dec_vMP;
                            }
                        }

                        PrintRELATORIOECF rELATORIOECF = new PrintRELATORIOECF();
                        rELATORIOECF.AbrirNovoRG("VENDA A PRAZO");
                        rELATORIOECF.CentraECF("<b>COMPROVANTE DE VENDA A PRAZO</b>");
                        rELATORIOECF.TextoECF($"<b>Cliente: {pFechamento.nome_cliente}</b>");
                        rELATORIOECF.PulaLinhaECF();
                        rELATORIOECF.PulaLinhaECF();
                        rELATORIOECF.TextoECF($"<b>Venda a prazo no valor: {valor_prazo:C2}</b>");
                        rELATORIOECF.TextoECF($"<b>Vencimento: {pFechamento.vencimento.ToShortDateString()}</b>");
                        rELATORIOECF.PulaLinhaECF();
                        rELATORIOECF.TextoECF("Assinatura:".PadRight(45, '_'));
                        rELATORIOECF.TextoECF($"Terminal: {NO_CAIXA:D3}  Op.: {operador}");
                        rELATORIOECF.ImprimeTextoGuardado();
                    }

                    for (int i = 0; i < tef_estabel; i++)
                    {
                        UnsafeNativeMethods.confCadastrar_ECF_Daruma("RG", "GERENCIAL", "");
                        //TODO Checar retorno;
                        UnsafeNativeMethods.iRGAbrir_ECF_Daruma("GERENCIAL");
                        //TODO Checar retorno;

                        UnsafeNativeMethods.iRGFechar_ECF_Daruma();
                        //TODO Checar retorno;
                        //usouTEF = false;
                    }
                }

                #endregion Imprime ECF
                catch (Exception ex)
                {
                    log.Error("Erro ao processar venda no ECF", ex);
                    DialogBox.Show(strings.IMPRESSAO,
                                   DialogBoxButtons.No, DialogBoxIcons.Warn, true,
                                   strings.ERRO_AO_ENVIAR_IMPRESSAO_DO_TEF_PARA_SPOOLER,
                                   RetornarMensagemErro(ex, false));
                }


                log.Debug("Encerrar Padrão");
                vendaAtual.GravaNaoFiscalBase(pFechamento.troco, NO_CAIXA, (short?)vendedorId ?? 0, true);


                txb_Avisos.Text = string.Format("TROCO: {0}", pFechamento.troco.ToString("C2"));
                _emTransacao = false;
                numProximoItem = 1;
                subtotal = 0;
                txb_Qtde.Clear();
                txb_TotGer.Clear();
                txb_TotProd.Clear();
                txb_ValorUnit.Clear();
                LimparCupomVirtual(5000);
                cupomVirtual.Clear();
                cupomVirtual.Append(@"{\rtf1\pc ");
                _tipo = ItemChoiceType.FECHADO;

                foreach (var item in pFechamento.pagamentos)
                {
                    if (item.Value < 0)
                    {
                        MessageBox.Show("FinalizaNoECF - Erro ao lançar cupom. Um dos valores informados era negativo.");
                        log.Debug("Erro ao lançar cupom. Um dos valores informados era negativo.");
                        return false;
                    }
                }
                using var MAITRE_LANCA_ITEM_PED_TA = new DataSets.FDBDataSetMaitreTableAdapters.SP_TRI_MAITRE_LANCAITEMPEDIDOTableAdapter();
                using var taEstCompProd = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_PRODUCAOTableAdapter();
                using var tbLancaItemPedido = new DataSets.FDBDataSetMaitre.SP_TRI_MAITRE_LANCAITEMPEDIDODataTable();
                using var taMaitPedItemCupomItem = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PED_ITEM_CUPOM_ITEMTableAdapter();
                MAITRE_LANCA_ITEM_PED_TA.Connection = LOCAL_FB_CONN;
                taEstCompProd.Connection = LOCAL_FB_CONN;
                taMaitPedItemCupomItem.Connection = LOCAL_FB_CONN;
                #region AmbiMAITRE

                //int intIdMaitrePedido = 0;
                //try
                //{
                //    if (Settings.Default.PRTUSBPedido != "Nenhuma")
                //    {
                //        using (var MAITRE_TA = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PEDIDOTableAdapter())
                //        using (var USERS_TA = new TRI_PDV_USERSTableAdapter())
                //        {
                //            MAITRE_TA.Connection = LOCAL_FB_CONN;
                //            USERS_TA.Connection = LOCAL_FB_CONN;
                //            intIdMaitrePedido = (int)MAITRE_TA.SP_TRI_MAITRE_LANCAPEDIDO(maitNrPedido,
                //                                                                         USERS_TA.PegaIdPorUser(operador).Safeint(),
                //                                                                         string.Empty, //TODO: campo ainda não utilizado
                //                                                                         NO_CAIXA,
                //                                                                         noCupom); // Insert_TRI_MAIT_PEDIDO_CUPOM();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    gravarMensagemErro(RetornarMensagemErro(ex, true));
                //    MessageBox.Show("Erro ao gravar pedido. \nPor favor tente novamente. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                //    return false; //deuruim();
                //}

                #endregion AmbiMAITRE

                string strMensagemLogLancaContaRec = string.Empty;
                string strMensagemLogLancaMovDiario = string.Empty;
                using var CONTAREC_TA = new TB_CONTA_RECEBERTableAdapter
                {
                    Connection = LOCAL_FB_CONN
                };
                foreach (envCFeCFeInfCFePgtoMP item in pFechamento.metodos)
                {
                    if (item.cMP == "03" || item.cMP == "04")
                    {
                        break; //Pára de gerar contas a receber ao lançar vendas nos cartões
                        #region DESATIVADO
                        /*
                        string descricao = "TEF/POS ";
                        int result_contarec = 0;
                        short result_movdiario = 0;

                        if (item.cMP == "03")
                        {
                            descricao = "TEF/POS - Crédito ";
                        }
                        else if (item.cMP == "04")
                        {
                            descricao = "TEF/POS - Débito ";
                        }
                        audit("FECHAECF", "" + descricao);

                        try
                        {
                            strMensagemLogLancaContaRec = String.Format("FECHAECF", "SP_TRI_LANCACONTAREC(Cupom: {0}, Cupom: {1}, Vencimento: {2}, vMP: {3}, {4}, Descrição: {5}",
                                                                        no_cupom,
                                                                        no_cupom.ToString(),
                                                                        fechamento.vencimento,
                                                                        Convert.ToDecimal(item.vMP, ptBR),
                                                                        0, (descricao + DateTime.Now.ToShortTimeString()).ToUpper());
                            audit(strMensagemLogLancaContaRec);
                            result_contarec = (int)CONTAREC_TA.SP_TRI_LANCACONTAREC(no_cupom,
                                                                                    no_cupom.ToString(),
                                                                                    fechamento.vencimento,
                                                                                    Convert.ToDecimal(item.vMP, ptBR),
                                                                                    0, (descricao + DateTime.Now.ToShortTimeString()).ToUpper());
                        }
                        catch (Exception ex)
                        {
                            gravarMensagemErro(RetornarMensagemErro(ex, true));
                            MessageBox.Show("Erro ao gravar conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                            CancelarECF();
                            return; //deuruim();
                        }

                        if (result_contarec != 1)
                        {
                            //TODO: erro grave que detona com o fluxo da venda.
                            // Pelo menos deixar algumas dicas...
                            gravarMensagemErro(string.Format("Erro no {0} \n\nRetorno: {1}", strMensagemLogLancaContaRec, result_contarec));
                            MessageBox.Show("Erro ao gravar conta a receber (FinalizaNoECF - Erro ao gerar ContaRec). \n\nPor favor entre em contato com a equipe de suporte.");
                            CancelarECF();
                            return; //deuruim();
                        }

                        try
                        {
                            strMensagemLogLancaMovDiario = String.Format("FECHAECF", "SP_TRI_LANCAMOVDIARIO({0}, vMP: {1}, Descrição: {2}, {3}, {4}", "x",
                                                                         Convert.ToDecimal(item.vMP, ptBR),
                                                                         (descricao + " Cupom " + NO_CAIXA.ToString() + "-" + coo.ToString() + " " + DateTime.Now.ToShortTimeString()).ToUpper(),
                                                                         147, 5);
                            audit(strMensagemLogLancaMovDiario);
                            using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                            {
                                OPER_TA.Connection = LOCAL_FB_CONN;
                                result_movdiario = (short)OPER_TA.SP_TRI_LANCAMOVDIARIO("x",
                                                                                    Convert.ToDecimal(item.vMP, ptBR),
                                                                                    (descricao + " Cupom " + NO_CAIXA.ToString() + "-" + coo.ToString() + " " + DateTime.Now.ToShortTimeString()).ToUpper(),
                                                                                    147, 5);
                            }
                        }
                        catch (Exception ex)
                        {
                            gravarMensagemErro(RetornarMensagemErro(ex, true));
                            MessageBox.Show("Erro ao gravar movimentação referente à conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                            CancelarECF();
                            return; //deuruim();
                        }

                        if (result_movdiario != 1)
                        {
                            //TODO: erro grave que detona com o fluxo da venda.
                            // Pelo menos deixar algumas dicas...
                            gravarMensagemErro(string.Format("Erro no {0} \n\nRetorno: {1}", strMensagemLogLancaMovDiario, result_movdiario));
                            MessageBox.Show("Erro ao gravar movimentação financeira (FinalizaNoECF - Erro ao gerar MovDiario). \n\nPor favor entre em contato com a equipe de suporte.");
                            CancelarECF();
                            return; //deuruim();
                        }

                        try
                        {
                            audit(String.Format("FECHAECF", "SP_TRI_CTAREC_MOVTO (Caixa: {0}, coo: {1}", (short)NO_CAIXA, coo));
                            using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                            {
                                OPER_TA.Connection = LOCAL_FB_CONN;
                                OPER_TA.SP_TRI_CTAREC_MOVTO((short)NO_CAIXA, coo);
                            }
                        }
                        catch (Exception ex)
                        {
                            gravarMensagemErro(RetornarMensagemErro(ex, true));
                            MessageBox.Show("Erro ao gravar movimentação/conta a receber. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                            CancelarECF();
                            return; //deuruim();
                        }
                        */
                        #endregion
                    }
                }
                //tsTransacaoFinalVenda.Complete();
            }
            vendedorId = null;
            return true;
        }


        private bool ImprimeESalvaCupomFiscal(FechamentoCupom pFechamento, string _xmlret, CFe cFeDeRetorno)
        {
            var _metodos_de_pagamento = new Dictionary<string, string>
                        {
                            { "01", "Dinheiro" },
                            { "02", "Cheque" },
                            { "03", "Cartão Crédito" },
                            { "04", "Cartão Débito" },
                            { "05", "Crédito Loja" },
                            { "10", "Vale Aliment." },
                            { "11", "Vale Refeição" },
                            { "12", "PIX" },
                            { "13", "Vale Combustível" },
                            { "20", "PIX" },
                            { "99", "Outros" }
                        };//Dicionário de métodos de pagamento.
            int venda_prazo = 0;
            //bool usouTEF = false;
            string chavecfe = cFeDeRetorno.infCFe.Id.Replace("CFe", string.Empty);

            string ChaveCFEWhats = chavecfe;



            //string chavecfe = cFeDeRetorno.infCFe.ide.cUF +
            //                  cFeDeRetorno.infCFe.ide.dEmi.Substring(2, 4) +
            //                  cFeDeRetorno.infCFe.emit.CNPJ +
            //                  cFeDeRetorno.infCFe.ide.mod +
            //                  cFeDeRetorno.infCFe.ide.nserieSAT +
            //                  cFeDeRetorno.infCFe.ide.nCFe +
            //                  cFeDeRetorno.infCFe.ide.cNF +
            //                  cFeDeRetorno.infCFe.ide.cDV;
            Directory.CreateDirectory(@"SAT\Vendas");
            File.WriteAllText($@"SAT\Vendas\AD{chavecfe}.xml", _xmlret);
            string id_dest = "NÃO DECLARADO";
            int.TryParse(cFeDeRetorno.infCFe.ide.nCFe, out int nCFe);
            VendaImpressa.numerodocupom = nCFe;


            //TODO: Permitir vencimento/id_cliente POR MÉTODO DE PAGAMENTO
            //for (int i = 0; i < pFechamento.pagamentos.Count; i++)
            //{
            //    if (cFeDeRetorno.Pgto.MP[i].cMP == "05")
            //    {
            //        cFeDeRetorno.Pgto.MP[i].idCliente = pFechamento.
            //    }
            //}
            //TODOEND
            foreach (var item in cFeDeRetorno.infCFe.pgto.MP)
            {
                if (item.cMP == "05") item.idCliente = pFechamento.id_cliente; item.vencimento = pFechamento.vencimento;
            }

            log.Debug($" ID_DEST: {id_dest}");
            string valorcfe = cFeDeRetorno.infCFe.total.vCFe;


            PrintMaitrePEDIDO.nomedaempresa = cFeDeRetorno.infCFe.emit.xNome;
            PrintMaitrePEDIDO.nomefantasia = cFeDeRetorno.infCFe.emit.xFant;
            PrintMaitrePEDIDO.cnpjempresa = cFeDeRetorno.infCFe.emit.CNPJ;
            PrintMaitrePEDIDO.enderecodaempresa = string.Format("{0}, {1} - {2}, {3}", cFeDeRetorno.infCFe.emit.enderEmit.xLgr,
                                                                           cFeDeRetorno.infCFe.emit.enderEmit.nro,
                                                                           cFeDeRetorno.infCFe.emit.enderEmit.xBairro,
                                                                           cFeDeRetorno.infCFe.emit.enderEmit.xMun);
            PrintMaitrePEDIDO.ieempresa = cFeDeRetorno.infCFe.emit.IE;
            PrintMaitrePEDIDO.imempresa = cFeDeRetorno.infCFe.emit.IM;
            VendaImpressa.vendedor = vendedorString;
            log.Debug("Produtos computados: cProd, xProd, uCom, qCom, vUnCom, vDesc, ICMS, TAXAIBPT");


            var Funcoes = new funcoesClass();
            decimal _qCom, _vUnCom, _vDesc;
            foreach (envCFeCFeInfCFeDet item in cFeDeRetorno.infCFe.det)
            {
                _qCom = decimal.Parse(item.prod.qCom, CultureInfo.InvariantCulture);
                _vUnCom = decimal.Parse(item.prod.vUnCom, CultureInfo.InvariantCulture);
                _vDesc = decimal.Parse(string.IsNullOrWhiteSpace(item.prod.vDesc) ? "0" : item.prod.vDesc, CultureInfo.InvariantCulture) + item.descAtacado;


                if (string.IsNullOrWhiteSpace(item.prod.NCM) || !Funcoes.ConsultarTaxasPorNCM(item.prod.NCM, out decimal taxa_fed, out decimal taxa_est, out decimal taxa_mun))
                {
                    VendaImpressa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, 0, 0, 0, item.prod.vUnComOri.Safedecimal());
                    log.Debug($"{item.prod.cProd}, {item.prod.xProd}, {item.prod.uCom}, {_qCom}, {_vUnCom}, {_vDesc}, {0}, {0}, {0}");
                }
                else
                {
                    VendaImpressa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, taxa_est, taxa_fed, taxa_mun, item.prod.vUnComOri.Safedecimal());
                    log.Debug($"{item.prod.cProd}, {item.prod.xProd}, {item.prod.uCom}, {_qCom}, {_vUnCom}, {_vDesc}, {taxa_est}, {taxa_fed}, {taxa_mun}");
                }


                //if (!string.IsNullOrWhiteSpace(item.prod.NCM))
                //{
                //    if (!Funcoes.ConsultarTaxasPorNCM(item.prod.NCM, out decimal taxa_fed, out decimal taxa_est, out decimal taxa_mun))
                //    {
                //        DialogBox.Show("VENDA", DialogBoxButtons.No, DialogBoxIcons.Error, false, "NCM não foi encontrado na tabela IBPT", $"Verifique o NCM do produto {item.prod.vDesc}");
                //        return false;
                //    }

                //    VendaImpressa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, taxa_est, taxa_fed, taxa_mun);
                //    audit("FECHASATLOCAL>> ", String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}",
                //                                             item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, taxa_est, taxa_fed, taxa_mun));
                //}
                //else
                //{
                //    VendaImpressa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, 0, 0, 0);
                //    audit("FECHASATLOCAL>> ", String.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}",
                //                                             item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, 0, 0, 0));

                //}

                #region AmbiMAITRE
                //if (Settings.Default.PRTUSBPedido != "Nenhuma")
                //{
                //    PrintMaitrePEDIDO.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, _qCom, _vUnCom, _vDesc, 0, 0, 0);
                //}
                #endregion AmbiMAITRE
            }

            int ID_NFVENDA = vendaAtual.GravaVendaNaBase(NO_CAIXA, (short?)vendedorId ?? 0, nCFe);

            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                string strMensagemLogLancaContaRec = string.Empty;
                string strMensagemLogLancaMovDiario = string.Empty;
                var _vMP = 0m;
                decimal valor_prazo = 0;

                using (var CONTAREC_TA = new TB_CONTA_RECEBERTableAdapter())
                using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
                {
                    OPER_TA.Connection = LOCAL_FB_CONN;
                    CONTAREC_TA.Connection = LOCAL_FB_CONN;

                    foreach (envCFeCFeInfCFePgtoMP item in pFechamento.metodos)
                    {
                        VendaImpressa.RecebePagamento(_metodos_de_pagamento[item.cMP.ToString()], item.dec_vMP);
                        log.Debug($"FISCAL>> Pagamento efetuado: {_metodos_de_pagamento[item.cMP.ToString()]}, Valor {item.dec_vMP}");
                        if (USATEF == true && (item.cMP == "03" || item.cMP == "04"))
                        {
                            //usouTEF = true;
                            log.Debug("TEF UTILIZADO!!!!!");
                        }
                        if (item.cMP == "05")
                        {
                            valor_prazo = item.dec_vMP;
                        }
                    }
                }
                VendaImpressa.chavenfe = chavecfe;
                log.Debug($"Chave NFE: {chavecfe}");
                VendaImpressa.assinaturaQRCODE = chavecfe + "|" +
                                         DateTime.Now.ToString("yyyyMMddhhmmss") + "|" +
                                         valorcfe + "|" +
                                         id_dest + "|" +
                                         cFeDeRetorno.infCFe.ide.assinaturaQRCODE;
                VendaImpressa.desconto = pFechamento.desconto;
                log.Debug($"Desconto: {pFechamento.desconto}");
                if (pFechamento.troco > 0.01m)
                {
                    cFeDeRetorno.infCFe.pgto.vTroco = pFechamento.troco.ToString("0.00");
                    VendaImpressa.troco = cFeDeRetorno.infCFe.pgto.vTroco;
                }
                else
                {
                    VendaImpressa.troco = "0,00";
                }
                if (!(cFeDeRetorno.infCFe.infAdic is null) && !(cFeDeRetorno.infCFe.infAdic.obsFisco is null))
                {
                    VendaImpressa.observacaoFisco = (cFeDeRetorno.infCFe.infAdic.obsFisco[0].xCampo, cFeDeRetorno.infCFe.infAdic.obsFisco[0].xTexto);
                }
                log.Debug($"Troco: {pFechamento.troco}");
                try
                {
                    if (vendaAtual.imprimeViaAssinar)
                    {
                        VendaImpressa.cliente = pFechamento.nome_cliente;
                        VendaImpressa.vencimento = pFechamento.vencimento;
                        VendaImpressa.valor_prazo = valor_prazo;
                        venda_prazo += 1;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao imprimir e salvar cupom fiscal", ex);
                    DialogBox.Show(strings.IMPRESSAO,
                                   DialogBoxButtons.No, DialogBoxIcons.Warn, true,
                                   strings.ERRO_AO_ENVIAR_IMPRESSAO_DO_TEF_PARA_SPOOLER,
                                   RetornarMensagemErro(ex, false));
                    return false;
                }
                if (_usouOS) _funcoes.FechaOrdemDeServico(new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) }, ordemDeServico.ID_OS);

                if (_usouOrcamento) FecharOrcamento(ID_NFVENDA);
                #region IMPRESSÃO DE CUPOM VIRTUAL

                ImprimirCupomVirtual(@"TOTAL:" + ("R$ " + subtotal.ToString("N2")).PadLeft(39, ' ') + @" ");
                if (pFechamento.desconto != 0)
                {
                    ImprimirCupomVirtual(@"DESCONTO:" + ("R$ " + pFechamento.desconto.ToString()).PadLeft(36, ' ') + @" ");
                }

                foreach (envCFeCFeInfCFePgtoMP item in cFeDeRetorno.infCFe.pgto.MP)
                {
                    _vMP = decimal.Parse(item.vMP, CultureInfo.InvariantCulture);
                    ImprimirCupomVirtual(_metodos_de_pagamento[item.cMP] + "R$ " + _vMP.ToString().PadLeft(42 - _metodos_de_pagamento[item.cMP.ToString()].Length));
                }
                if (pFechamento.troco > 0)
                {
                    ImprimirCupomVirtual(@"TROCO" + pFechamento.troco.ToString().PadLeft(45 - "Troco".Length) + @"  ");
                }

                #endregion IMPRESSÃO DE CUPOM VIRTUAL

                txb_Avisos.Text = string.Format("TROCO: {0}", pFechamento.troco.ToString("C2"));
                _emTransacao = false;
                numProximoItem = 1;
                subtotal = 0;
                txb_Qtde.Clear();
                txb_TotGer.Clear();
                txb_TotProd.Clear();
                txb_ValorUnit.Clear();
                LimparCupomVirtual(5000);
                cupomVirtual.Clear();
                cupomVirtual.Append(@"{\rtf1\pc ");
                _tipo = ItemChoiceType.FECHADO;


                foreach (var item in pFechamento.pagamentos)
                {
                    if (item.Value < 0)
                    {
                        MessageBox.Show("Erro ao lançar cupom. Um dos valores informados era negativo.");
                        log.Debug("Erro ao lançar cupom. Um dos valores informados era negativo.");
                        return false;
                    }
                }
                if (_usouOS) _funcoes.FechaOrdemDeServico(new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) }, ordemDeServico.ID_OS);

                if (_usouOrcamento) FecharOrcamento(ID_NFVENDA);

                #region AmbiMAITRE

                //int intIdMaitrePedido = 0;
                //try
                //{
                //    if (Settings.Default.PRTUSBPedido != "Nenhuma")
                //    {
                //        using (var MAITRE_TA = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PEDIDOTableAdapter())
                //        using (var USERS_TA = new TRI_PDV_USERSTableAdapter())
                //        {
                //            MAITRE_TA.Connection = LOCAL_FB_CONN;
                //            USERS_TA.Connection = LOCAL_FB_CONN;
                //            intIdMaitrePedido = (int)MAITRE_TA.SP_TRI_MAITRE_LANCAPEDIDO(maitNrPedido,
                //                                                                         USERS_TA.PegaIdPorUser(operador).Safeint(),
                //                                                                         string.Empty, //TODO: campo ainda não utilizado
                //                                                                         NO_CAIXA,
                //                                                                         noCupom); // Insert_TRI_MAIT_PEDIDO_CUPOM();
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    gravarMensagemErro(RetornarMensagemErro(ex, true));
                //    MessageBox.Show("Erro ao gravar pedido. \nPor favor tente novamente. \nSe o problema persistir, entre em contato com a equipe de suporte.");
                //    return false;
                //}



                //using (var MAITRE_LANCA_ITEM_PED_TA = new DataSets.FDBDataSetMaitreTableAdapters.SP_TRI_MAITRE_LANCAITEMPEDIDOTableAdapter())
                //using (var taEstCompProd = new DataSets.FDBDataSetMaitreTableAdapters.TB_EST_COMP_PRODUCAOTableAdapter())
                //using (var tbLancaItemPedido = new DataSets.FDBDataSetMaitre.SP_TRI_MAITRE_LANCAITEMPEDIDODataTable())
                //using (var taMaitPedItemCupomItem = new DataSets.FDBDataSetMaitreTableAdapters.TRI_MAIT_PED_ITEM_CUPOM_ITEMTableAdapter())
                //{
                //    MAITRE_LANCA_ITEM_PED_TA.Connection = LOCAL_FB_CONN;
                //    taEstCompProd.Connection = LOCAL_FB_CONN;
                //    taMaitPedItemCupomItem.Connection = LOCAL_FB_CONN;

                //    foreach (envCFeCFeInfCFeDet detalhamento in cFeDeRetorno.infCFe.det)
                //    {
                //        int intItemCup = int.Parse(detalhamento.nItem);


                //        try
                //        {
                //            if (Settings.Default.PRTUSBPedido != "Nenhuma")
                //            {
                //                try
                //                {
                //                    tbLancaItemPedido.Clear();
                //                    MAITRE_LANCA_ITEM_PED_TA.LancarItemPedido(tbLancaItemPedido,
                //                                                              intIdMaitrePedido,
                //                                                              Convert.ToInt32(detalhamento.prod.cProd),
                //                                                              Convert.ToDecimal(detalhamento.prod.qCom.Replace(".", ",")),
                //                                                              string.Empty);
                //                }
                //                catch (Exception ex)
                //                {
                //                    gravarMensagemErro(RetornarMensagemErro(ex, true));
                //                    MessageBox.Show("Erro ao lançar item de pedido. Por favor, entre em contato com a equipe de suporte.");
                //                    return false;
                //                }

                //                #region Verifica o lançamento de item de pedido

                //                if (tbLancaItemPedido is null |
                //                    tbLancaItemPedido.Rows.Count <= 0 | tbLancaItemPedido.Rows.Count > 1)
                //                {
                //                    gravarMensagemErro(string.Format("tbLancaItemPedido é ausente, nulo ou retornou mais de um registro: \nIDPEDIDO={0} \nIDPRODUTO={1} \nQUANT={2} \nCount={2}",
                //                                                     intIdMaitrePedido,
                //                                                     detalhamento.prod.cProd,
                //                                                     detalhamento.prod.qCom,
                //                                                     tbLancaItemPedido is null ? "null" : tbLancaItemPedido.Rows.Count.ToString()));
                //                    MessageBox.Show("Erro ao lançar item de pedido. Por favor, entre em contato com a equipe de suporte.");
                //                    return false;
                //                }

                //                if (tbLancaItemPedido[0].IsIDPEDIDOITEMNull() || tbLancaItemPedido[0].IDPEDIDOITEM <= 0)
                //                {
                //                    throw new Exception("Valor de retorno não esperado em FECHANOECF>> LancarItemPedido(...): ");
                //                }

                //                #endregion Verifica o lançamento de item de pedido

                //                if (tbLancaItemPedido[0].COMPOSICAO > 0)
                //                {
                //                    string strRetorno = (string)taEstCompProd.SP_TRI_MAITRE_PROCESSA_COMP(tbLancaItemPedido[0].IDPEDIDOITEM,
                //                                                                                      Convert.ToInt32(detalhamento.prod.cProd),
                //                                                                                      Convert.ToDecimal(detalhamento.prod.qCom.Replace(".", ",")));
                //                    if (!strRetorno.Equals("deu certo"))
                //                    {
                //                        string strMensagemParcial = "Erro no FECHANOECF>> " +
                //                                            String.Format("SP_TRI_MAITRE_PROCESSA_COMP(idpedidoitem: {0}, ididentif: {1}, quant: {2}, retorno: {3}",
                //                                                          tbLancaItemPedido[0].IDPEDIDOITEM,
                //                                                          Convert.ToInt32(detalhamento.prod.cProd),
                //                                                          Convert.ToDecimal(detalhamento.prod.qCom.Replace(".", ",")),
                //                                                          strRetorno);
                //                        throw new Exception(strMensagemParcial);
                //                    }
                //                }

                //                if (tbLancaItemPedido[0].COMPOSICAO > 1)
                //                {
                //                    audit("COMPOSICAO", "Mais de uma receita foi encontrada para esse produto. Utilizando a primeira.");
                //                }

                //                try
                //                {
                //                    audit("FECHANOSAT", $"SP_TRI_MTPDITM_CPITM_SYNCINSERT(): {taMaitPedItemCupomItem.SP_TRI_MTPDITM_CPITM_SYNCINSERT(tbLancaItemPedido[0].IDPEDIDOITEM, intItemCup)}");
                //                }
                //                catch (Exception ex)
                //                {
                //                    gravarMensagemErro(string.Format("Erro no FECHAECF", "SP_TRI_MTPDITM_CPITM_SYNCINSERT(ID_MAIT_PEDIDO_ITEM: {0}, ID_ITEMCUP: {1})",
                //                                                     tbLancaItemPedido[0].IDPEDIDOITEM,
                //                                                     intItemCup));
                //                    throw ex;
                //                }
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            gravarMensagemErro(RetornarMensagemErro(ex, true));
                //            MessageBox.Show("Erro durante o pedido de venda.");
                //            return false;
                //        }


                //    }

                //}
                #endregion AmbiMAITRE

                try
                {

                    subtotal = 0;
                    log.Debug($"FINALIZACUPOM>> Caixa {NO_CAIXA}, Cupom: {noCupom}, MODO FISCAL");

                    if (pFechamento.devolucoes_usadas.Count > 0)
                    {
                        using var DEVOL_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_DEVOLTableAdapter()
                        {
                            Connection = new FbConnection() { ConnectionString = MontaStringDeConexao("localhost", localpath) }
                        };
                        foreach ((int, decimal) item in pFechamento.devolucoes_usadas)
                        {
                            DEVOL_TA.RedeemaDevolucao(item.Item1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao imprimir e salvar cupom fiscal", ex);
                    DialogBox.Show(strings.VENDA,
                                   DialogBoxButtons.No,
                                   DialogBoxIcons.Error, true,
                                   strings.ERRO_DURANTE_VENDA,
                                   RetornarMensagemErro(ex, false));
                    return false;
                }
            }

            //if (!_modoTeste && PERGUNTA_WHATS == 0)
            //{
            //    try
            //    {
            //        ultimaImpressao = VendaImpressa.IMPRIME(venda_prazo, cFeDeRetorno);
            //        if (vendaAtual.imprimeViaAssinar)
            //        {
            //            VendaImpressa.IMPRIME(1);
            //        }
            //        #region Cod antigo
            //        //using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            //        //{
            //        //    if (usouTEF)
            //        //    {
            //        //        tefAtual.ConfirmarUltimaTransacao();
            //        //        foreach (var file in new DirectoryInfo(@"C:\PAYGO\OPER").GetFiles())
            //        //        {
            //        //            Dictionary<string, string> respostaCRT = General.LeResposta(file.FullName);
            //        //            using var taNsuPdv = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_NSUTableAdapter();
            //        //            int a = tiposDeTransacao.IndexOf(respostaCRT["011-000"]);
            //        //            if ((a == -1) || (respostaCRT["009-000"] != "0")) continue;
            //        //            taNsuPdv.Connection = LOCAL_FB_CONN;
            //        //            audit("NFISCAL", "TEF>> Executou a confirmação");

            //        //            //try
            //        //            //{
            //        //            //    string strRespostaCRT_013_000 = string.IsNullOrWhiteSpace(respostaCRT["013-000"]) ? "0" : respostaCRT["013-000"];
            //        //            //    Decimal.TryParse(respostaCRT["003-000"], out decimal vlr_oper);
            //        //            //    taNsuPdv.SP_TRI_SALVA_NSU(noCupom,
            //        //            //                              respostaCRT["012-000"],
            //        //            //                              respostaCRT["739-000"],
            //        //            //                              strRespostaCRT_013_000,
            //        //            //                              vlr_oper / 100);
            //        //            //    audit("ImprimirNaoFiscal", $"TEF>> NSU Salvo: Cupom: {noCupom}, NSU: {respostaCRT["012-000"]}, REDE: {respostaCRT["739-000"]}, CÓD AUTOR: {strRespostaCRT_013_000}");

            //        //            //}
            //        //            //catch (Exception ex)
            //        //            //{
            //        //            //    gravarMensagemErro(RetornarMensagemErro(ex, true));
            //        //            //    MessageBox.Show("Erro ao gravar NSU. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
            //        //            //    //blnDeuRuimNsu = true; //deuruim();
            //        //            //}

            //        //            if (true)
            //        //            {
            //        //                ComprovanteTEF.ReciboTEF = respostaCRT;

            //        //                #region printdecision
            //        //                if (respostaCRT.ContainsKey("737-000") && (respostaCRT["737-000"] == "1" || respostaCRT["737-000"] == "3") || !respostaCRT.ContainsKey("737-000"))
            //        //                {
            //        //                    if (respostaCRT.ContainsKey("710-000") && respostaCRT["710-000"] != "0")
            //        //                    {
            //        //                        ComprovanteTEF.IMPRIME(0, 0, 0, 1);
            //        //                    }
            //        //                    else
            //        //                    {
            //        //                        if (respostaCRT.ContainsKey("712-000") && respostaCRT["712-000"] != "0")
            //        //                        {
            //        //                            ComprovanteTEF.IMPRIME(1, 0, 0, 0);
            //        //                        }
            //        //                        else
            //        //                        {
            //        //                            ComprovanteTEF.IMPRIME(0, 0, 1, 0);
            //        //                        }
            //        //                    }
            //        //                }
            //        //                if (respostaCRT.ContainsKey("737-000") && (respostaCRT["737-000"] == "2" || respostaCRT["737-000"] == "3") || !respostaCRT.ContainsKey("737-000"))
            //        //                {
            //        //                    if (respostaCRT.ContainsKey("714-000") && respostaCRT["714-000"] != "0")
            //        //                    {
            //        //                        ComprovanteTEF.IMPRIME(0, 1, 0, 0);
            //        //                    }
            //        //                    else
            //        //                    {
            //        //                        ComprovanteTEF.IMPRIME(0, 0, 1, 0);
            //        //                    }
            //        //                }
            //        //                #endregion
            //        //            }
            //        //        }
            //        //    }
            //        //}
            //        #endregion

            //        if (PERMITE_ESTOQUE_NEGATIVO == null && RelNegativ.produtos.Count > 0)
            //        {
            //            log.Debug("Relatório de Itens negativos impresso.");
            //            RelNegativ.IMPRIME();
            //        }

            //        noCupom = 0;
            //        //usouTEF = false;
            //    }
            //    catch (Exception ex)
            //    {
            //        log.Error("Erro ao imprimir e salvar cupom fiscal", ex);
            //        MessageBox.Show("FinalizaNoSATLocal - Falha na impressão");
            //        return false;
            //    }
            //}
            DecisaoWhats resultado;
            switch (SYSEMITECOMPROVANTE)
            {
                case 0:
                    resultado = DecisaoWhats.NaoImprime;
                    break;
                case 1:
                    switch (SYSUSAWHATS.ToBool())
                    {
                        case false:
                            resultado = DecisaoWhats.ImpressaoNormal;
                            break;
                        default:
                            resultado = DecisaoComprovante(true);
                            break;
                    }
                    break;
                default:
                    resultado = DecisaoComprovante(true);
                    break;
            }

            switch (resultado)

            {
                case DecisaoWhats.Whats:
                    PerguntaNumWhats whats = new PerguntaNumWhats(ChaveCFEWhats, vendaAtual);
                    //Pergunta o número do Uatizápi
                    numeroWhats = whats.number.ToString();
                    whats.ShowDialog();
                    log.Debug("PASSOU PELO PERGUNTA WHATS FISCAL");
                    break;
                case DecisaoWhats.NaoImprime:
                    vendaAtual.imprimeViaCliente = false;
                    if (FORÇA_GAVETA) AbreGaveta();
                    break;

                case DecisaoWhats.ImpressaoNormal:
                    try
                    {

                        ultimaImpressao = VendaImpressa.IMPRIME(venda_prazo, cFeDeRetorno);
                        if (vendaAtual.imprimeViaAssinar)
                        {
                            VendaImpressa.IMPRIME(1);
                        }

                        if (PERMITE_ESTOQUE_NEGATIVO == null && RelNegativ.produtos.Count > 0)
                        {
                            log.Debug("Relatório de Itens negativos impresso.");
                            RelNegativ.IMPRIME();
                        }

                        noCupom = 0;
                    }
                    catch (Exception ex)
                    {
                        log.Error("Falha na impressão", ex);
                        MessageBox.Show("FinalizaNoSATLocal - Falha na impressão");
                        return false;
                    }
                    break;

            }

            #region PERGUNTAWHATS
            //if ((int)PERGUNTA_WHATS == 1)
            //{
            //    PerguntaNumWhats whats = new PerguntaNumWhats(ChaveCFEWhats, vendaAtual);
            //    whats.ShowDialog();
            //}
            //if ((int)PERGUNTA_WHATS == 2)
            //{
            //    bool veredito = false;
            //    while (!veredito)
            //    {

            //        var vendaPrazo = vendaAtual.RetornaCFe().infCFe.pgto;
            //        OpcoesDeImpressao nova = new OpcoesDeImpressao(vendaPrazo.MP[0].cMP);


            //        PerguntaNumWhats whats = new PerguntaNumWhats(ChaveCFEWhats, vendaAtual);
            //        nova.ShowDialog();
            //        if (nova.ImprimeWhats() == DecisaoWhats.Whats)
            //        {
            //            switch (whats.ShowDialog())
            //            {
            //                case true:
            //                    veredito = true;
            //                    nova.Close();
            //                    break;
            //                case false:
            //                    break;
            //                default:
            //                    return false;

            //            }

            //        }

            //        if (nova.ImprimeWhats() == DecisaoWhats.ImpressaoNormal)
            //        {
            //            try
            //            {
            //                ultimaImpressao = VendaImpressa.IMPRIME(venda_prazo, cFeDeRetorno);
            //                if (vendaAtual.imprimeViaAssinar)
            //                {
            //                    VendaImpressa.IMPRIME(1);
            //                }


            //                if (PERMITE_ESTOQUE_NEGATIVO == null && RelNegativ.produtos.Count > 0)
            //                {
            //                    log.Debug("Relatório de Itens negativos impresso.");
            //                    RelNegativ.IMPRIME();
            //                }

            //                noCupom = 0;
            //                //usouTEF = false;
            //            }
            //            catch (Exception ex)
            //            {
            //                log.Error("Erro ao imprimir e salvar cupom fiscal", ex);
            //                MessageBox.Show("FinalizaNoSATLocal - Falha na impressão");
            //                return false;
            //            }
            //            veredito = true;
            //            nova.Close();
            //        }
            //        if (nova.ImprimeWhats() == DecisaoWhats.NaoImprime)
            //        {
            //            if (vendaPrazo.MP[0].cMP.Equals("05"))
            //            { MessageBox.Show("Venda a prazo tem impressão obrigatória.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Exclamation); }
            //            else
            //            { veredito = true; }
            //        }

            //    }
            //}
            #endregion PERGUNTAWHATS


            vendedorId = null;
            if (!File.Exists($@"SAT\Vendas\AD{chavecfe}.xml"))
            {
                DialogBox.Show(strings.GRAVACAO_DE_XML, DialogBoxButtons.No, DialogBoxIcons.Warn, false, strings.ARQUIVO_XML_MOVIDO, strings.ENTRE_EM_CONTATO_COM_CONTADOR, strings.NAO_ENTRE_EM_CONTATO_SEM_VERIFICAR_COM_CONTADOR);
            }
            VendaImpressa.Clear();

            return true;
        }

        /// <summary>
        /// Inicializa a tela principal do caixa
        /// </summary>
        /// <param name="pContingencia">A contingência estava ativada previamente?</param>
        private void InicializarCaixa(bool pContingencia)
        {
            var Funcoes = new funcoesClass();
            Funcoes.CarregarIBPT();
            InitializeComponent();
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Ultimavenda.txt"))
            {
                using StreamWriter sw = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\Ultimavenda.txt");
                sw.WriteLine("Arquivo gerado automaticamente pelo AmbiPDV. Não remova.\n\r");
            }

            CarregaConfigs(pContingencia);
            //ACBox.MinimumPrefixLength = ACFILLPREFIX;
            //ACBox.MinimumPopulateDelay = ACFILLDELAY;
            switch (ACFILLMODE)
            {
                case 0:
                    log.Debug($"ACFILLMODE: StartsWith");
                    //ACBox.FilterMode = AutoCompleteFilterMode.StartsWith;
                    break;
                case 1:
                default:
                    log.Debug($"ACFILLMODE: Contains");
                    //ACBox.FilterMode = AutoCompleteFilterMode.Contains;
                    break;
            }
            #region Inicializar Interface Extra                                        
            lbl_Marquee.MarqueeContent = MENSAGEM_CORTESIA;
            switch (pContingencia)
            {
                case true:
                    lbl_Carga.Content = "--:--:--";
                    bar_Contingencia.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

            var timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 1000), DispatcherPriority.Normal, (p, s) =>
            {
                lbl_Hora.Content = DateTime.Now.ToString();
                if (SATLIFESIGNINTERVAL == 0)
                {
                    return;
                }

                if (!_emTransacao)
                {
                    timeKeepAliveSAT++;
                }

                if (!_emTransacao && timeKeepAliveSAT >= SATLIFESIGNINTERVAL * 60)
                {
                    IniciarSincronizacaoDB(EnmTipoSync.cadastros, Settings.Default.SegToleranciaUltSync);
                    try
                    {
                        ChecaStatusSATLocal(_contingencia);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Erro ao checar status do SAT Local", ex);
                        return;
                    }
                    timeKeepAliveSAT = 0;
                    log.Debug("SATLifeSign Checked!");
                }
                return;
            }, Dispatcher);

            lbl_Operador.Content = "VOCÊ ESTÁ SENDO ATENDIDO POR: " + operador.Split(' ')[0];
            if (DateTime.Today.Month == 4 && (DateTime.Today.Day == 1 || DateTime.Today.Day == 2))
            {
                //var rnd = new Random();
                //lbl_Operador.Content = "VOCÊ ESTÁ SENDO ATENDIDO POR: " + funcoes.eegg[rnd.Next(0, funcoes.eegg.Count)];
                //lbl_Operador.MouseEnter += Lbl_Operador_MouseEnter;
                //lbl_Operador.MouseLeave += Lbl_Operador_MouseLeave;
            }
            cupomVirtual.Append(@"{\rtf1\pc ");
            Title = NOMESOFTWARE;

            //combobox.PreviewKeyDown += new KeyEventHandler(ACBox_KeyDown);


            AtualizarProdutosNoACBox();


            lbl_no_Caixa.Content = "CAIXA: " + NO_CAIXA.ToString("D3");
            //_i = 0;
            #endregion
            _emTransacao = false;

            // Inicia a sync de cadastros para evitar aquele problema de senha cadastrada no servidor mas ainda não no PDV:
            IniciarSincronizacaoDB(EnmTipoSync.cadastros, 0);

            //IniciarTestes();
            ChecarStatusTurno();
            try
            {
                CarregarClientesOC();
                log.Debug("Clientes carregados");
            }
            catch (Exception ex)
            {
                log.Error("Falha ao carregar os clientes", ex);
                throw ex;
            }
            try
            {
                VerificaUtilizaSAT(pContingencia);
                log.Debug("UtilizaSAT Verificado");

            }
            catch (Exception ex)
            {
                log.Error("Falha ao testar o SAT", ex);
                throw ex;
            }

            try
            {
                if (!VerificaDadosDoEmitente())
                {
                    DialogBox.Show("DADOS DO EMITENTE", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Ocorreu um erro ao obter os dados do emitente.");
                };

            }
            catch (Exception ex)
            {
                DialogBox.Show("DADOS DO EMITENTE", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Ocorreu um erro ao obter os dados do emitente. Um log de erro foi gerado.", "O programa será fechado.");
                log.Error("Falha ao obter os dados do emitente", ex);
                Application.Current.Shutdown();

            }
            try
            {
                VerificaTEF();
                log.Debug("Status do TEF verificado, e emitente registrado");
            }
            catch (Exception ex)
            {
                log.Error("Falha ao verificar o TEF", ex);
                throw ex;
            }

            //if (!ObtemDadosDoEmitente())
            //{
            //    DialogBox.Show("Carregando Dados do Emitente", DialogBoxButtons.No, DialogBoxIcons.Error, true, "Falha ao obter os dados do emitente. Verifique os dados na retaguarda, e tente novamente.");
            //}

            _contingencia = pContingencia;
        }

        private void VerificaTEF()
        {
            if (USATEF
#if HOMOLOGATEF || true
#endif
                )
            {
                int intRetorno;
                string strRetorno;
                var tef = new SiTEFBox();
#if HOMOLOGATEF
                log.Debug($"TEFIP: {TEFIP}, TEFNUMLOJA: {TEFNUMLOJA}, TEFNUMTERMINAL: {TEFNUMTERMINAL}");
                (intRetorno, strRetorno) = tef.ConfiguraSitef("127.0.0.1", "00000000", "AA000001", new ParamsDeConfig());
                log.Debug($"ConfiguraSiTEF: intRetorno => {intRetorno}, strRetorno => {strRetorno}");
                tef.SetaMensagemPinpad($"{DateTime.Now.ToShortDateString()}|{DateTime.Now.ToShortTimeString()}");
#else
                log.Debug($"TEFIP: {TEFIP}, TEFNUMLOJA: {TEFNUMLOJA}, TEFNUMTERMINAL: {TEFNUMTERMINAL}");
                (intRetorno, strRetorno) = tef.ConfiguraSitef(TEFIP, TEFNUMLOJA, TEFNUMTERMINAL, new ParamsDeConfig());
                log.Debug($"ConfiguraSiTEF: intRetorno => {intRetorno}, strRetorno => {strRetorno}");
                tef.SetaMensagemPinpad($"{DateTime.Now.ToShortDateString()}|{DateTime.Now.ToShortTimeString()}");
#endif
                List<Pendencia> pendencias = tef.ListaPendenciasDoTEF();
                foreach (Pendencia pendencia in pendencias)
                {
                    FinalizaFuncaoSiTefInterativo(0, pendencia.NoCupom, pendencia.DataFiscal, pendencia.HoraFiscal, $"NumeroPagamentoCupom={pendencia.IdPag}");
                }
                if (pendencias.Count > 0)
                {
                    DialogBox.Show("Última venda", DialogBoxButtons.No, DialogBoxIcons.Warn, true, "A última operação no TEF não foi concluída com sucesso. Favor reter o cupom.");
                }
            }
        }

        private bool VerificaDadosDoEmitente()
        {
            if (Emitente.CarregaInfo() != true)

                return false;

            switch (Emitente.BoolSimples)
            {
                case false:
                    tipoDeEmpresa = TipoDeEmpresa.RPA;
                    break;
                case true:
                    tipoDeEmpresa = TipoDeEmpresa.SN;
                    break;
            }
            return true;
            //switch (TB_EMITENTE.GetData()[0]["SIMPLES"])
            //{
            //    case "N":
            //        tipoDeEmpresa = TipoDeEmpresa.RPA;
            //        break;
            //    case "S":
            //    default:
            //        break;
            //}
        }

        private void VerificaUtilizaSAT(bool pContingencia)
        {
            if (SAT_USADO)
            {
                if (SATSERVIDOR)
                {
                    try
                    {
                        if (!ChecaStatusSATServidor())
                        {
                            Login.stateGif = false;
                            MessageBox.Show("SAT não está respondendo. Não será possível fazer vendas no SAT.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Erro ao checar status do SAT Servidor", ex);
                        MessageBox.Show(ex.Message);
                    }


                }
                else
                {
                    try
                    {
                        if (!ChecaStatusSATLocal(pContingencia))
                        {
                            Login.stateGif = false;
                            MessageBox.Show("SAT não está respondendo. Não será possível fazer vendas no SAT.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error("Erro ao checar status do SAT Local", ex);
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Sincroniza a base local com a base do servidor
        /// </summary>
        /// <param name="pTipoSync">Parâmetro que determina quais tabelas devem ser sincronizadas.</param>
        /// <param name="pSegsTolerancia">Segundos até que o método entenda que o servidor parou de responder.</param>
        /// <param name="pTipoSyncForcado">Não usado</param>
        private void IniciarSincronizacaoDB(EnmTipoSync pTipoSync, int pSegsTolerancia/*, EnmTipoSync pTipoSyncForcado = EnmTipoSync.dummy*/)
        {
            log.Debug($"Iniciando sincronização do tipo {pTipoSync}");

            //string strTipoSync = string.Empty;

            //switch (pTipoSync)
            //{
            //    case EnmTipoSync.cadastros:
            //        strTipoSync = "cadastros";
            //        break;
            //    case EnmTipoSync.vendas:
            //        strTipoSync = "vendas";
            //        break;
            //    case EnmTipoSync.fechamentos:
            //        strTipoSync = "fechamentos";
            //        break;
            //    case EnmTipoSync.tudo:
            //        strTipoSync = "cadastros, vendas e fechamentos";
            //        break;
            //    default:
            //        throw new NotImplementedException("Tipo de sincronização informado não esperado (" + pTipoSync.ToString() + ")!");
            //}

            //TODO -- DONE --: pode existir um gap entre a sync e a atualização dos itens no ACBox.
            // Se o sync rodar e retornar registros para atualizar e rodar mais uma vez,
            // o segundo retorno pode anular o primeiro se o databind do ACBox não foi executado.
            //
            // Verifica se a nova lista tem registros. Faz nada se estiver nula ou vazia.
            // Verifica se a lista acumulada tem registros.
            // Se não, atribui toda a nova lista para a lista acumulada.
            // Se tiver, adiciona todos os itens da nova lista na lista acumulada.

            var lstProdutosAlteradosSync = (List<ComboBoxBindingDTO_Produto_Sync>)new SincronizadorDB().SincronizarContingencyNetworkDbs(pTipoSync, pSegsTolerancia);
            if (lstProdutosAlteradosSync == null || lstProdutosAlteradosSync.Count == 0) { return; }

            if (_lstProdutosAlteradosSync == null || _lstProdutosAlteradosSync.Count == 0)
            {
                // atribuir lista
                _lstProdutosAlteradosSync = lstProdutosAlteradosSync;
            }
            else
            {
                // adicionar à lista
                foreach (var item in lstProdutosAlteradosSync)
                {
                    _lstProdutosAlteradosSync.Add(item);
                }
            }
        }

        /// <summary>
        /// Limpa o cupom virtual, ou seja, a lista de produtos ao lado direito da tela principal do caixa
        /// </summary>
        /// <param name="pDelay"></param>
        private async void LimparCupomVirtual(int pDelay)
        {
            await Task.Delay(pDelay);
            if (!_emTransacao) { richTextBox1.Document.Blocks.Clear(); }
            //mostratroco = false;
            if (!ChecarPorSangria())
            {
                txb_Avisos.Text = "CAIXA LIVRE";
            }

            if (_modo_consulta == false)
            {
                lbl_Cortesia.Content = null;
                lbl_Marquee.Visibility = Visibility.Visible;
            }
            //TODO Checar se há a necessidade de fazer sangria.
        }



        /// <summary>
        /// Limpa o objeto de venda, lista de produtos, e se pNovaVenda == true, limpa o desconto aplicado no produto
        /// </summary>
        /// <param name="pNovaVenda">True se for o início de uma nova venda</param>
        private void LimparObjetoDeVendaNovo(bool pNovaVenda = false)
        {
            vendaAtual?.Clear();
            devolAtual?.Clear();
            if (!pNovaVenda)
            {
                tipoDeDesconto = tipoDesconto.Nenhum;
                desconto = 0;
            }
            PendenciasDoTEF pendTef = new PendenciasDoTEF();
            pendTef.LimpaTodasPendencias();

        }

        /// <summary>
        /// Limpa o cupom virtual, os campos e retorna a interface ao modo standby.
        /// </summary>
        private void LimparTela()
        {
            logoplaceholder.Visibility = Visibility.Visible;

            fotoProd.Source = null;
            fotoProd.UpdateLayout();
            _emTransacao = false;
            _tipo = ItemChoiceType.FECHADO;
            LimparCupomVirtual(0);
            txb_Qtde.Clear();
            txb_TotGer.Clear();
            txb_TotProd.Clear();
            txb_ValorUnit.Clear();
            //ChecaPorContingencia(bar_Contingencia.IsVisible, EnmTipoSync.cadastros);
        }

        /// <summary>
        /// Limpa o .TXT e insere uma mensagem padrão no "UltimaVenda.txt"
        /// </summary>
        private void LimparUltimaVenda()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Ultimavenda.txt";
            if (File.Exists(path) && File.ReadLines(path).Count() > 1)
            {
                File.WriteAllText(path, "Arquivo gerado automaticamente pelo AmbiPDV. Não remova.\n\r");
            }
        }

        /// <summary>
        /// Busca um produto que contenha como código ou descrição 
        /// </summary>
        /// <param name="pInput"></param>
        /// <returns></returns>
        private int MostrarProdutoNaoEncontrado(string pInput)
        {
            // Antes de afirmar que o produto não foi encontrado em memória,
            // pesquisar no banco local:

            int? a;
            int? b;
            using (var OPER_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                OPER_TA.Connection = LOCAL_FB_CONN;

                a = (int?)OPER_TA.SP_TRI_PESQUISAITEM(pInput);
                b = null;
                b = (int?)OPER_TA.SP_TRI_PESQUISACOD(pInput.Safeint());
            }
            if (a < 0 || b < 0)
            {
                lbl_Marquee.Visibility = Visibility.Hidden;
                lbl_Cortesia.Content = "Descrição duplicada! Pesquisar pelo identificador";
                combobox.Text = "";
                log.Debug($"Produto com desc. duplicada. - ConverteInformacaoEmProduto({pInput})");
                return -1;
            }
            else if (a != null || b != null)
            {
                if (a != null)
                {
                    return (int)a;
                }
                else
                {
                    return (int)b;
                }
            }
            else if (a == null && b == null)
            {
                lbl_Marquee.Visibility = Visibility.Hidden;
                lbl_Cortesia.Content = "Produto/Código não encontrado!";
                combobox.Text = "";
                log.Debug($"Produto não foi encontrado. - ConverteInformacaoEmProduto({pInput})");
                if (INTERROMPE_NAO_ENCONTRADO == true)
                {
                    perguntaSenha pg = new perguntaSenha("Produto não encontrado")
                    {
                        permiteescape = false
                    };
                    pg.ShowDialog();
                }
                return -1;
            }

            lbl_Marquee.Visibility = Visibility.Hidden;
            lbl_Cortesia.Content = "Produto/Código não encontrado!";
            combobox.Text = string.Empty;
            log.Debug($"Produto não foi encontrado. - ConverteInformacaoEmProduto({pInput})");
            if (INTERROMPE_NAO_ENCONTRADO == true)
            {
                var pg = new perguntaSenha("Produto não encontrado") { permiteescape = false };
                pg.ShowDialog();
            }

            return -1;
        }

        /// <summary>
        /// Pergunta o CPF/CNPJ do cliente
        /// </summary>
        private void PedirIdentificacao()
        {
            //if (USATEF && TEFPEDECPFPELOPINPAD)
            //{
            //    CDP Captura = new CDP()
            //    {

            //    };
            //    var db = new TEFBox(strings.VENDA_NO_TEF, strings.SIGA_AS_INSTRUCOES_NO_TEF, TEFBox.DialogBoxButtons.Yes, TEFBox.DialogBoxIcons.None);
            //    Captura.Exec();
            //    db.ShowDialog();
            //    if (db.DialogResult == false)
            //    {
            //        return;
            //    }
            //    string identif = null;
            //    if (General.LeResposta().Keys.Contains("007-000"))
            //    { identif = General.LeResposta()["007-000"]; }
            //    if (!(identif is null))
            //    {
            //        infoStr = identif;
            //        _tipo = ItemChoiceType.CPF;
            //    }
            //    else
            //    {
            //        var pegaID = new IdentificaConsumidor();
            //        pegaID.ShowDialog();
            //        infoStr = pegaID.identificacao;
            //        _tipo = pegaID.tipo;
            //    }
            //}
            //else
            {
                var pegaID = new IdentificaConsumidor();
                pegaID.ShowDialog();
                infoStr = pegaID.identificacao;
                _tipo = pegaID.tipo;
            }

        }

        /// <summary>
        /// Abre a janela pedindo o vendedor
        /// </summary>
        private void PedirVendedor()
        {
            var pegaVEND = new PerguntaVendedor();
            pegaVEND.ShowDialog();
            vendedorId = pegaVEND.id_vendedor;
            vendedorString = pegaVEND.nome_vendedor;
        }

        /// <summary>
        /// Obtém o peso da balança se ela estiver configurada
        /// </summary>
        private void PegarPesoDaBalanca()
        {
            if (_modoTeste) { return; }
            {
                decimal peso = Balanca.RetornaPeso();
                if (peso < 0)
                {
                    BalancaBox bb = new BalancaBox();
                    if (BALMODELO == 0)
                    {
                        bb.run_Linha1.Text = "Não há balança instalada no sistema, porém o produto";
                        bb.run_Linha2.Text = "informado está cadastrado como \"KG\".";
                        bb.run_Linha3.Text = "Informe a quantidade/peso e pressione [ENTER],";
                        bb.run_Linha4.Text = "ou pressione [ESC] para cancelar.";
                        bb.run_Linha5.Text = "";
                        bb.lbl_Title.Text = "Balança não configurada";
                        bb.txb_Peso.Text = "1,000";

                    }
                    bb.ShowDialog();
                    switch (bb.DialogResult)
                    {
                        case true:
                            peso = bb.novopeso;
                            break;
                        case false:
                            switch (bb.retry)
                            {
                                case true:
                                    PegarPesoDaBalanca();
                                    return;
                                case false:
                                    txb_Qtde.Text = "";
                                    return;
                            }
                    }
                    txb_Qtde.Text = peso.ToString();
                    return;
                }
                txb_Qtde.Text = peso.ToString();
            }
        }

        /// <summary>
        /// Pergunta o número do orçamento a ser importado
        /// </summary>
        private void PerguntarNumeroDoOrcamento()
        {
            if (_modo_consulta)
            {
                AlternarModoDeConsulta();
            }
            if (!turno_aberto)
            {
                DialogBox.Show("ORÇAMENTO", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Abra o caixa antes de importar um orçamento.");
                return;
            }
            if (_modoDevolucao || _emTransacao)
            {
                return;
            }
            var po = new PerguntaOrcamento(PerguntaOrcamento.EnmTipo.orcamento);
            switch (po.ShowDialog())
            {
                case true:

                    #region Zerar o cupom para iniciar um novo
                    LimparObjetoDeVendaNovo();
                    LimparTela();
                    #endregion Zerar o cupom para iniciar um novo

                    combobox.Text = "+" + po.numeroInformado.ToString();

                    CarregarProdutosDoOrcamentoNovo();
                    combobox.Text = "";
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// Pergunta o número do orçamento a ser importado
        /// </summary>
        private void PerguntarNumeroDaOS()
        {
            if (_modo_consulta)
            {
                AlternarModoDeConsulta();
            }
            if (!turno_aberto)
            {
                DialogBox.Show("Ordem de Serviço", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Abra o caixa antes de importar um orçamento.");
                return;
            }
            if (_modoDevolucao || _emTransacao)
            {
                return;
            }
            var po = new PerguntaOrcamento(PerguntaOrcamento.EnmTipo.ordemServico);
            switch (po.ShowDialog())
            {
                case true:

                    #region Zerar o cupom para iniciar um novo
                    LimparObjetoDeVendaNovo();
                    LimparTela();
                    #endregion Zerar o cupom para iniciar um novo

                    combobox.Text = "%" + po.numeroInformado.ToString();

                    CarregarProdutosDaOS();
                    combobox.Text = "";
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// Pesquisa o preço do item identificado pelo código interno, sem adicionar a um cupom
        /// </summary>
        /// <param name="codigo_item">Código interno do item</param>
        /// <param name="pPrecoUnitario">Preço unitário do item</param>
        private void PesquisarItem(int codigo_item, decimal pPrecoUnitario, decimal pQuant, bool pPrePesado = false)
        {
            using (var obtemdadosdoitem = new SP_TRI_OBTEMDADOSDOITEMTableAdapter())
            using (var dadosDoItem = new DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMDataTable())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                obtemdadosdoitem.Connection = LOCAL_FB_CONN;
                obtemdadosdoitem.Fill(dadosDoItem, codigo_item);
                lbl_Cortesia.Content = dadosDoItem.Rows[0][dadosDoItem.DESCRICAOColumn].ToString(); // Usa ID_IDENTIFICADOR
                txb_ValorUnit.Text = pPrecoUnitario.RoundABNT(2).ToString("C2");
                var strTipoDeItem = dadosDoItem.Rows[0][dadosDoItem.UNI_MEDIDAColumn].ToString(); // Usa ID_IDENTIFICADOR
                if (strTipoDeItem.Safestring() == "KG" || strTipoDeItem.Safestring() == "KU")
                {
                    if (!pPrePesado)
                    {
                        PegarPesoDaBalanca();
                        if (String.IsNullOrWhiteSpace(txb_Qtde.Text))
                        {
                            pQuant = 0;
                        }
                        else
                        {
                            pQuant = Convert.ToDecimal(txb_Qtde.Text);
                        }
                    }
                    else
                    {
                        txb_Qtde.Text = pQuant.RoundABNT(3).ToString("0.000");
                    }
                }
            }
            if (pQuant == 0) { pQuant = 1; }
            txb_TotProd.Text = (pPrecoUnitario * pQuant).RoundABNT(2).ToString("C2");
            combobox.Text = "";

        }

        /// <summary>
        /// Preenche o objeto de venda com as informações iniciais de venda
        /// </summary>
        private bool PrepararCabecalhoDoCupom()
        {
            using (var taCupomPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_CUPOMTableAdapter())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                /*Cupons são reservados localmente no início da venda, para
                 *posteriormente serem sincronizados.*/
                taCupomPdv.Connection = LOCAL_FB_CONN;
            }
            LimparObjetoDeVendaNovo(true);
            vendaAtual = new Venda();
            //tefAtual = new OperTEF();
            subtotal = 0;
            string CNPJdaVenda = Emitente.CNPJ;
            string IEdaVenda = Emitente.IE;
            if (SIGN_AC.Contains("RETAGUARDA"))
            {
                switch (MODELO_SAT)
                {
                    case ModeloSAT.NENHUM:
                        break;
                    case ModeloSAT.DARUMA:
                        break;
                    case ModeloSAT.DIMEP:
                        CNPJSH = "16716114000172";
                        CNPJdaVenda = "61099008000141";
                        IEdaVenda = "111111111111";
                        break;
                    case ModeloSAT.BEMATECH:
                        break;
                    case ModeloSAT.ELGIN:
                        break;
                    case ModeloSAT.SWEDA:
                        break;
                    case ModeloSAT.CONTROLID:
                        CNPJSH = "16716114000172";
                        CNPJdaVenda = "08238299000129";
                        IEdaVenda = "149392863111";
                        break;
                    case ModeloSAT.TANCA:
                        break;
                    case ModeloSAT.EMULADOR:
                        break;
                    default:
                        break;
                }
            }

            if (File.Exists("emitente.ini"))
            {
                var emitente = File.ReadAllLines("emitente.ini");
                foreach (string s in emitente)
                {
                    var linha = s.Split('=');
                    if (linha[0].ToUpper() == "CNPJ")
                    {
                        CNPJdaVenda = linha[1].TiraPont();
                    }

                    if (linha[0].ToUpper() == "IE")
                    {
                        IEdaVenda = linha[1].TiraPont();
                    }
                }
            }
            if (CNPJdaVenda is null || IEdaVenda is null)
            {

                throw new Exception($"Falha ao obter os dados do emitente. Dado não encontrado: {Emitente.ListaErro[0]}");
            }
            try
            {
                vendaAtual.AbrirNovaVenda(CNPJSH, SIGN_AC, NO_CAIXA.ToString(), CNPJdaVenda.TiraPont(), IEdaVenda.TiraPont(), Emitente.IM);
                //tefAtual.PreparaNovaVenda();
            }
            catch (Exception ex)
            {
                DialogBox.Show(strings.ABERTURA_DE_CUPOM, DialogBoxButtons.No, DialogBoxIcons.None, false, ex.Message);
                log.Error("Erro ao abrir nova venda", ex);
                return false;
            }

            _emTransacao = true;
            Paragraph pg = new Paragraph
            {
                Margin = new Thickness(0),
                TextIndent = indentdaMargem,
                TextAlignment = TextAlignment.Center
            };
            pg.Inlines.Add(new Run(@"CUPOM FISCAL "));
            richTextBox1.Document.Blocks.Add(pg);
            richTextBox1.Focus();
            richTextBox1.ScrollToEnd();
            combobox.Focus();
            ImprimirCupomVirtual(@"ITEM  CÓDIGO        DESCRIÇÃO ");
            ImprimirCupomVirtual(@"    QTD. UN.  VL. UNIT R$         VL. ITEM R$");
            ImprimirCupomVirtual(new string('=', 45) + @" ");

            txb_Avisos.Text = "CUPOM ABERTO";

            return true;
        }

        /// <summary>
        /// Preenche o objeto de venda com as informações iniciais de venda
        /// </summary>
        private bool PrepararCabecalhoDaDevolucao()
        {
            LimparObjetoDeVendaNovo(true);
            devolAtual = new Devolucao();
            subtotal = 0;
            string CNPJdaVenda = Emitente.CNPJ;
            string IEdaVenda = Emitente.IE;
            if (SIGN_AC.Contains("RETAGUARDA"))
            {
                switch (MODELO_SAT)
                {
                    case ModeloSAT.NENHUM:
                        break;
                    case ModeloSAT.DARUMA:
                        break;
                    case ModeloSAT.DIMEP:
                        CNPJdaVenda = "61099008000141";
                        IEdaVenda = "111111111111";
                        break;
                    case ModeloSAT.BEMATECH:
                        break;
                    case ModeloSAT.ELGIN:
                        break;
                    case ModeloSAT.SWEDA:
                        break;
                    case ModeloSAT.CONTROLID:
                        CNPJdaVenda = "08238299000129";
                        IEdaVenda = "149392863111";
                        break;
                    case ModeloSAT.TANCA:
                        break;
                    case ModeloSAT.EMULADOR:
                        break;
                    default:
                        break;
                }
            }
            if (String.IsNullOrEmpty(CNPJdaVenda) || String.IsNullOrEmpty(IEdaVenda)) throw new Exception("Falha ao obter os dados do emitente.");
            try
            {
                devolAtual.AbrirNovaDevolucao();
            }
            catch (Exception ex)
            {
                log.Error("Erro ao abrir nova devolução", ex);
                return false;
            }
            _emTransacao = true;
            Paragraph pg = new Paragraph
            {
                Margin = new Thickness(0),
                TextIndent = indentdaMargem,
                TextAlignment = TextAlignment.Center
            };
            pg.Inlines.Add(new Run("CUPOM DE DEVOLUÇÃO"));
            richTextBox1.Document.Blocks.Add(pg);
            richTextBox1.Focus();
            richTextBox1.ScrollToEnd();
            combobox.Focus();
            ImprimirCupomVirtual(@"ITEM  CÓDIGO        DESCRIÇÃO ");
            ImprimirCupomVirtual(@"    QTD. UN.  VL. UNIT R$         VL. ITEM R$");
            ImprimirCupomVirtual(new string('=', 45) + @" ");

            txb_Avisos.Text = "CUPOM ABERTO";

            return true;
        }


        //private /*(string CNPJ, string IE, string IM)? */ bool ObtemDadosDoEmitente()
        //{
        //    //string CNPJ_emitente, IE_emitente, IM_emitente = null;
        //    using (var EMITENTE_TA = new TB_EMITENTETableAdapter())
        //    using (var EMITENTE_DT = new DataSets.FDBDataSetOperSeed.TB_EMITENTEDataTable())
        //    using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
        //    {
        //        EMITENTE_TA.Connection = LOCAL_FB_CONN;
        //        int _emitenteCount = 0;
        //        try
        //        {
        //            _emitenteCount = EMITENTE_TA.Fill(EMITENTE_DT);
        //        }
        //        catch (Exception ex)
        //        {
        //            audit("CABECALHO", "Não foi possível obter as informações do emitente - " + ex.Message);
        //            return false;

        //        }
        //        audit("CABECALHO", "emitenteCount: " + _emitenteCount, 2);
        //        try
        //        {
        //            CNPJ = EMITENTE_DT[0].CNPJ.ToString().Replace(".", string.Empty).Replace("-", string.Empty).Replace("/", string.Empty);
        //        }
        //        catch (Exception ex)
        //        {
        //            audit("CABECALHO", "Não foi possível obter o CNPJ do emitente - " + ex.Message);
        //            return false;
        //        }
        //        try
        //        {
        //            IE = EMITENTE_DT[0].INSC_ESTAD.Replace(".", "");
        //        }
        //        catch (Exception ex)
        //        {
        //            audit("CABECALHO", "Não foi possível obter o IE do emitente - " + ex.Message);
        //            return false;
        //        }
        //        try
        //        {
        //            IM = EMITENTE_DT[0].INSC_MUNIC;

        //        }
        //        catch (Exception ex)
        //        {
        //            audit("CABECALHO", "Não foi possível obter o IM do emitente - " + ex.Message);
        //        }
        //    }
        //    return true;
        //}


        /// <summary>
        /// Prepara a finalização do cupom não fiscal
        /// </summary>
        private void PrepararFinalizacaoDeCupomDemo()
        {
            erroVenda = false;
            if (_modoDevolucao)
            {
                DialogBox.Show(strings.DEVOLUCAO_DE_MERCADORIA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.DEVOLUCAO_DEVE_SER_POR_F7);
                return;
            }
            if (_tipo != ItemChoiceType.FECHADO)
            {
                _tipo = ItemChoiceType.DEMONSTRACAO;
            }
            try
            {
                if (SYSCOMISSAO == 2)// Properties.Settings.Default.SYSComissao são variáveis definidas por XML, uma espécie de variável global e para acessa-las, é necessário utilizar o comando  
                {
                    PedirVendedor();
                }
                vendaAtual.AplicaPrecoAtacado();
                FinalizarVendaNovo();
            }
            catch (Exception ex)
            {
                DialogBox.Show(strings.VENDA,
                               DialogBoxButtons.No, DialogBoxIcons.Error, false,
                               RetornarMensagemErro(ex, false));
                log.Error("Erro ao preparar finalização de cupom demo", ex);
            }
        }

        /// <summary>
        /// Prepara a finalização do cupom fiscal
        /// </summary>
        private void PrepararFinalizacaoDeCupomFiscal()
        {
            erroVenda = false;
            if (_modoDevolucao)
            {
                DialogBox.Show(strings.DEVOLUCAO_DE_MERCADORIA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.DEVOLUCAO_DEVE_SER_POR_F7);
                return;
            }
            if (!SAT_USADO && !ECF_ATIVA || _tipo == ItemChoiceType.FECHADO)
            {
                return;
            }

            if (PEDE_CPF == 2 && !_modoTeste)
            {
                PedirIdentificacao();
            }

            if (_tipo == ItemChoiceType.ABERTO)
            {
                _tipo = ItemChoiceType.NENHUM;
            }

            if (SYSCOMISSAO == 2)
            {
                PedirVendedor();
            }
            try
            {
                vendaAtual.AplicaPrecoAtacado();
                FinalizarVendaNovo();
            }
            catch (Exception ex)
            {
                log.Error("Erro ao finalizar venda fiscal", ex);
                throw;
            }
            return;
        }
        private FuncoesFirebird _funcoes = new();

        private void ProcessarItemNovo(int pCodigoItem, decimal pPrecoUnitario, decimal pQuant, decimal pDesconto)
        {
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };


            var dadosDoItem = _funcoes.ObtemDadosDoItem(pCodigoItem, LOCAL_FB_CONN);
            if (dadosDoItem is null)
            {
                throw new Exception("dadosDoItem era vazio");
            }

            //using var dadosDoItem = new DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMDataTable();
            //log.Debug("Executando obtemDadosdoItem.Fill");
            //using (var obtemdadosdoitem = new SP_TRI_OBTEMDADOSDOITEMTableAdapter())
            //{
            //    obtemdadosdoitem.Connection = LOCAL_FB_CONN;
            //    obtemdadosdoitem.Fill(dadosDoItem, pCodigoItem);
            //}
            //log.Debug("obtemDadosdoItem.Fill done");
            //log.Debug("isolando itemRow");
            //DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMRow itemRow = (DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMRow)dadosDoItem.Rows[0];
            //log.Debug("itemRow isolado");

            #region Pegar peso
            if (_prepesado == false && (dadosDoItem.UNI_MEDIDA == "KG" || dadosDoItem.UNI_MEDIDA == "KU") && BALMODELO != 0)
            {
                try
                {
                    combobox.IsEnabled = false;
                    txb_Qtde.Text = "Pesando...";
                    PegarPesoDaBalanca();
                    combobox.IsEnabled = true;
                    if (txb_Qtde.Text == "")
                    {
                        return;
                    }

                    pQuant = Convert.ToDecimal(txb_Qtde.Text);
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao pegar peso da balança", ex);
                    DialogBox.Show(strings.OBTER_PESO, DialogBoxButtons.No, DialogBoxIcons.Error, true, RetornarMensagemErro(ex, false));
                    return;
                }
            }
            #endregion Pegar peso

            log.Debug($" Código: {pCodigoItem}, Preço: {pPrecoUnitario}, Quant {pQuant}");
            log.Debug("obtendo csosnCfe");
            string csosnCfe = dadosDoItem.RCSOSN_CFE.Safestring().Trim();
            log.Debug("csosnCfe obtido");
            lbl_Marquee.Visibility = Visibility.Hidden;
            log.Debug("obtendo descrição");
            lbl_Cortesia.Content = dadosDoItem.DESCRICAO;
            log.Debug("descrição obtida");
            combobox.Text = "";
            txb_Qtde.Clear();
            string barcode = null;
            if (_prepesado == false)
            {
                barcode = dadosDoItem.COD_BARRA;
            }
            else { barcode = pCodigoItem.ToString(); }

            _prepesado = false;
            string famiglia = null;
            string[] observacaoLines =
                dadosDoItem.OBSERVACAO.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            if (observacaoLines.Length > 0)
            {
                foreach (string observacaoLine in observacaoLines)
                {
                    if (observacaoLine.Split(':')[0].ToLower() == "classe")
                    {
                        famiglia = observacaoLine.Split(':')[1].ToLower();
                    }
                }
            }
            if (_modoDevolucao)
            {
                devolAtual.RecebeNovoProduto(
                                        pCodigoItem,
                                        dadosDoItem.DESCRICAO,
                                        dadosDoItem.COD_NCM,
                                        pPrecoUnitario,
                                        0, pDesconto, dadosDoItem.UNI_MEDIDA, pQuant, dadosDoItem.COD_BARRA
                                        );
                devolAtual.AdicionaProduto();
            }
            else
            {
                log.Debug("Recebendo Novo Produto em vendaAtual");

                vendaAtual.RecebeNovoProduto(
                                        pCodigoItem,
                                        dadosDoItem.DESCRICAO,
                                        dadosDoItem.COD_NCM,
                                        dadosDoItem.CFOP,
                                        pPrecoUnitario,
                                        dadosDoItem.RSTR_CEST,
                                        0,
                                        pDesconto,
                                        dadosDoItem.UNI_MEDIDA,
                                        pQuant,
                                        dadosDoItem.COD_BARRA,
                                        famiglia
                                        );
                log.Debug("vendaAtual.RecebeNovoProduto concluído");
                switch (dadosDoItem.RID_TIPOITEM == "9")
                {
                    case true:
                        {
                            log.Debug("Recebendo ISSQN");
                            vendaAtual.RecebeInfoISSQN(dadosDoItem.RALIQ_ISS);
                            log.Debug("ISSQN Recebido");
                            break;
                        }
                    case false:
                        {
                            log.Debug("Recebendo ICMS");
                            vendaAtual.RecebeInfoICMS(
                                tipoDeEmpresa,
                                dadosDoItem.RCSOSN_CFE.Trim(),
                                dadosDoItem.RCST_CFE,
                                dadosDoItem.RUF_SP.ToString(),
                                dadosDoItem.RBASE_ICMS.ToString()
                                );
                            log.Debug("ICMS Recebido");
                            break;
                        }
                }
                log.Debug("Recebendo PIS");
                vendaAtual.RecebePIS(
                    dadosDoItem.RCST_PIS,
                    pPrecoUnitario,
                    dadosDoItem.RPIS,
                    pQuant
                    );
                log.Debug("PIS Recebido. Recebendo COFINS");
                vendaAtual.RecebeCOFINS(
                    dadosDoItem.RCST_COFINS,
                    pPrecoUnitario,
                    dadosDoItem.RCOFINS,
                    pQuant
                    );
                log.Debug("COFINS Recebido. Adicionando produto");
                vendaAtual.AdicionaProduto(dadosDoItem.RCST_CFE);
                log.Debug("Produto adicionado");
                using (StreamWriter sw = File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "\\Ultimavenda.txt"))
                {
                    sw.WriteLine(pQuant.ToString() + "|" + pCodigoItem);
                }
            }
            if (EXIBEFOTO)
            {
                logoplaceholder.Visibility = Visibility.Collapsed;
                using TB_EST_PRODUTOTableAdapter EST_PRODUTO_TA = new TB_EST_PRODUTOTableAdapter();
                byte[] fotoByte = EST_PRODUTO_TA.GetFotoColumn(pCodigoItem);
                if (!(fotoByte is null))
                {
                    System.Drawing.ImageConverter _imageConverter = new System.Drawing.ImageConverter();

                    System.Drawing.Bitmap bm = (System.Drawing.Bitmap)_imageConverter.ConvertFrom(fotoByte);

                    if (bm != null && (bm.HorizontalResolution != (int)bm.HorizontalResolution ||
                                       bm.VerticalResolution != (int)bm.VerticalResolution))
                    {
                        // Correct a strange glitch that has been observed in the test program when converting 
                        //  from a PNG file image created by CopyImageToByteArray() - the dpi value "drifts" 
                        //  slightly away from the nominal integer value
                        bm.SetResolution((int)(bm.HorizontalResolution + 0.5f),
                                         (int)(bm.VerticalResolution + 0.5f));
                    }

                    fotoProd.Source = bm.ToImageSource();
                    fotoProd.UpdateLayout();
                }


            }

            txb_ValorUnit.Text = pPrecoUnitario != 0 ? pPrecoUnitario.RoundABNT().ToString("C2") : "R$ -,--";
            txb_TotProd.Text = pPrecoUnitario != 0 ? (pPrecoUnitario * pQuant).RoundABNT().ToString("C2") : "R$ -,--";
            subtotal += (pPrecoUnitario * pQuant - pDesconto).RoundABNT();
            txb_TotGer.Text = subtotal.RoundABNT().ToString("C2");
            string numAtual = String.Empty;
            switch (_modoDevolucao)
            {
                case true:
                    numAtual = (devolAtual.nItemCupom - 1).ToString().PadLeft(3, '0');
                    break;
                case false:
                    numAtual = (vendaAtual.nItemCupom - 1).ToString().PadLeft(3, '0');
                    break;
            }
            if (barcode == null) { ImprimirCupomVirtual($@"{numAtual} {pCodigoItem.ToString().PadLeft(13, '0')} {dadosDoItem.DESCRICAO.Trunca(27)}"); }

            else { ImprimirCupomVirtual($"{numAtual} {barcode.PadLeft(13, '0')} {dadosDoItem.DESCRICAO.Trunca(27)}"); }
            ImprimirCupomVirtual($"{pQuant.RoundABNT(3).ToString("0.000").Trunca(5),8} {dadosDoItem.UNI_MEDIDA} {pPrecoUnitario.RoundABNT(),10:0.00} {(pPrecoUnitario * pQuant).RoundABNT(),20:0.00}");
            if (pDesconto > 0) ImprimirCupomVirtual($"---Desconto no item: {pDesconto:C2}");
        }


        private bool EnviaParaSAT(FechamentoCupom pFechamento)
        {
            vendaAtual.InformaCliente(_tipo, infoStr);

            log.Debug(SATSERVIDOR ? "Fechamento no SAT Servidor" : "Fechamento no SAT Local");
            string m_erro_venda = "", xmlret = "", codigoDeRetorno;

            var settings = new XmlWriterSettings() { Encoding = new UTF8Encoding(true), OmitXmlDeclaration = false, Indent = false };
            var XmlFinal = new StringBuilder();
            var serializer = new XmlSerializer(typeof(CFe));
            using (var xwriter2 = XmlWriter.Create(XmlFinal, settings))
            {
                var xns = new XmlSerializerNamespaces();
                xns.Add(string.Empty, string.Empty);
                Directory.CreateDirectory(@"SAT_LOG");
                serializer.Serialize(xwriter2, vendaAtual.RetornaCFe(), xns); //Popula o stringbuilder para ser enviado para o SAT.
            }
            string _XML_ = XmlFinal.ToString().Replace(',', '.').Replace("utf-16", "utf-8");
            File.WriteAllText(@"SAT_LOG\UltimaVenda.xml", _XML_);
            string[] retorno;
            //HACK: Trecho pra garantir que o encoding da string (???) seja em UTF-8.
            // Se não executar essa conversão string -> bytes -> string com encoding, pode acontecer o erro de validação 6010|1999|Erro não identificado, com erro de conversão UTF-8.
            // O bug é deflagrado quando a descrição de algum produto contém pelo menos um caracter diacrítico.
            // ---------------------------------------------->>>
            byte[] bytes = Encoding.Default.GetBytes(_XML_);
            _XML_ = Encoding.UTF8.GetString(bytes);
            // ----------------------------------------------<<<

            if (!SATSERVIDOR)
            {
                try
                {
                    Declaracoes_DllSat.sRetorno = Marshal.PtrToStringAnsi(Declaracoes_DllSat.EnviarDadosVenda(ns.GeraNumero(), SAT_CODATIV, _XML_, MODELO_SAT));
                    var arraydebytes = Encoding.Default.GetBytes(Declaracoes_DllSat.sRetorno);
                    string sRetorno = Encoding.UTF8.GetString(arraydebytes);
                    retorno = sRetorno.Split('|');
                    codigoDeRetorno = retorno.Length > 1 ? retorno[1] : "06099";
                }

                catch (Exception ex)
                {
                    log.Error("Erro ao enviar dados para a venda", ex);
                    DialogBox.Show(strings.VENDA,
                                   DialogBoxButtons.No, DialogBoxIcons.Error, false,
                                   strings.VERIFIQUE_AS_LUZES_DO_SAT);
                    erroVenda = true;
                    return false;
                }
            }
            else
            {
                try
                {
                    using (var SAT_ENV_TA = new TRI_PDV_SAT_ENVTableAdapter())
                    {
                        SAT_ENV_TA.SP_TRI_ENVIA_SAT_SERVIDOR(NO_CAIXA, bytes);
                    }

                    var sb = new SATBox("Operação no SAT", "Aguarde a resposta do SAT.");
                    sb.ShowDialog();
                    if (sb.DialogResult == false)
                    {
                        erroVenda = true;
                        return false;
                    }
                    else
                    {
                        retorno = sb.retorno;
                        codigoDeRetorno = sb.cod_retorno;
                    }

                }
                catch (Exception ex)
                {
                    log.Error("Erro ao enviar XML para o Sat Servidor", ex);
                    DialogBox.Show(strings.VENDA,
                                   DialogBoxButtons.No, DialogBoxIcons.Error, false,
                                   "Erro ao enviar a venda para o servidor SAT");
                    log.Debug("ERRO - Verifique LOGERRO.TXT");
                    erroVenda = true;
                    return false;
                }
            }

            if (retorno.Length < 2)
            {
                erroVenda = true;
                return false;
            }

            switch (codigoDeRetorno)
            {
                case "06000":
                    xmlret = Encoding.UTF8.GetString(Convert.FromBase64String(retorno[6].ToString()));
                    break;
                case "06001":
                    m_erro_venda = "Código de ativação inválido.";
                    erroVenda = true;
                    break;
                case "06002":
                    m_erro_venda = "SAT ainda não ativado.";
                    erroVenda = true;
                    break;
                case "06003":
                    m_erro_venda = "SAT ainda não vinculado ao Aplicativo Comercial.";
                    erroVenda = true;
                    break;
                case "06004":
                    m_erro_venda = "Vinculação do AC não confere.";
                    erroVenda = true;
                    break;
                case "06005":
                    m_erro_venda = "Tamanho do CF-e-SAT superior a 1500 KB.";
                    erroVenda = true;
                    break;
                case "06006":
                    m_erro_venda = "SAT bloqueado pelo contribuinte.";
                    erroVenda = true;
                    break;
                case "06007":
                    m_erro_venda = "SAT bloqueado pela SEFAZ.";
                    erroVenda = true;
                    break;
                case "06008":
                    m_erro_venda = "SAT bloqueado por falta de comunicação.";
                    erroVenda = true;
                    break;
                case "06009":
                    m_erro_venda = "SAT temporariamente bloqueado. Número de tentativas ultrapassado.";
                    erroVenda = true;
                    break;
                case "06010":
                    log.Debug("Logs extraídos");
                    m_erro_venda = "Erro de validação de conteúdo: \n" + retorno[3];
                    erroVenda = true;
                    break;
                case "06098":
                    m_erro_venda = "SAT ocupado, aguarde para tentar novamente.";
                    erroVenda = true;
                    break;
                default:
                    m_erro_venda = "ERRO DESCONHECIDO. Ligue para (11) 4304-7778 e informe erro " + retorno[1];
                    erroVenda = true;
                    break;
            }
            if (erroVenda == true)
            {
                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Error, false, m_erro_venda);
                log.Debug($"ERRO DE SAT: {m_erro_venda}");
                return false;
            }
            CFe cFeDeRetorno;
            using (var XmlRetorno = new StringReader(xmlret))
            using (var xreader = XmlReader.Create(XmlRetorno))
            {
                cFeDeRetorno = (CFe)serializer.Deserialize(xreader);
            }

            for (int i = 0; i < cFeDeRetorno.infCFe.det.Length; i++)
            {
                cFeDeRetorno.infCFe.det[i].prod.vUnComOri = vendaAtual.RetornaCFe().infCFe.det[i].prod.vUnComOri;
            }
            vendaAtual.RecebeCFeDoSAT(cFeDeRetorno);
            return ImprimeESalvaCupomFiscal(pFechamento, xmlret, cFeDeRetorno);
        }

        private void ProcessarTextoNoACBox()
        {
            string input = combobox.Text;
            log.Debug("ProcessarTextoNoACBox chamado");
            if (!_modoDevolucao && !_modo_consulta)
            {
                if (CarregarVendaPendenteNovo())
                {
                    log.Debug("Carregou venda pendente");
                    input = "";
                }
                USA_COMANDA = true;
                if (USA_COMANDA && CarregarProdutosDaComandaNovo()) { log.Debug("Carregou produtos da comanda"); return; }
                if (USA_ORÇAMENTO && CarregarProdutosDoOrcamentoNovo()) { log.Debug("Carregou produtos do orçamento (ou não, ou seja, tentou carregar orçamento)"); return; }
                //if (CarregaProdutosDoPedido()) { verbose("Carregou produtos do pedido (ou não, ou seja, tentou carregar pedido)"); return; }
                //TODO: a atualização de preços só fará efeito se o turno estiver aberto!
            }
            if (turno_aberto && !_emTransacao)
            {
                log.Debug("Verificando se o caixa já está em modo de contigencia na abertura da venda(...)");
                if (_contingencia == false)
                {
                    log.Debug("Foi verificado que o caixa não está em modo de contigencia, será checado novamente se a conexão persiste.");
                    ChecarPorContingencia(_contingencia, Settings.Default.SegToleranciaUltSync, EnmTipoSync.cadastros);
                }
                else
                {
                    log.Debug("Foi verificado que o caixa já está em contigencia, assim pulando a checagem automatica!\n" +
                        "Caso deseje reestabelecer conexão com o servidor utilize as teclas 'CTRL+S'.");
                }
                log.Debug("Iniciando CarregarProdutos(...)");
                AtualizarProdutosNoACBox();
            }

            #region Define a quantidade do item (user input)

            log.Debug("Buscando a quantidade informada...");

            decimal quant = 1;
            if (txb_Qtde.Text != "") { quant = Convert.ToDecimal(txb_Qtde.Text); }

            if (quant <= 0) { quant = 1; }

            log.Debug($"Quantidade obtida: {quant}");

            #endregion Define a quantidade do item (user input)

            ComboBoxBindingDTO_Produto produtoEncontrado/* = null*/;

            #region Determina o estado do programa
            if (_tipo == ItemChoiceType.FECHADO && _modo_consulta == false && !_modoDevolucao)
            {

                log.Debug("Abriu um cupom");
                _tipo = ItemChoiceType.ABERTO;

                if (_emTransacao == false)
                {
                    if (!ChecagemPreVenda())
                    {
                        log.Debug("Checagem pré venda falhou");
                        _tipo = ItemChoiceType.FECHADO;
                        return;
                    }
                    if (!PrepararCabecalhoDoCupom())
                    {
                        log.Debug("Cabecalho do cupom falhou");
                        _tipo = ItemChoiceType.FECHADO;
                        return;
                    }
                }
            }
            else if (_modoDevolucao & !_emTransacao)
            {
                if (!ExecChecagemPreDevolucao())
                {
                    log.Debug("Checagem pré venda falhou");
                    return;
                }
                if (!PrepararCabecalhoDaDevolucao())
                {
                    log.Debug("Cabecalho do cupom falhou");
                    _tipo = ItemChoiceType.FECHADO;
                    //AlternarModoDevolucao();
                    return;
                }
            }
            #endregion

            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };

            #region Detecta se foi escaneado um código pré-pesado
            if (input.StartsWith("2") && input.Length == 13)
            {
                log.Debug("Código pré-pesado foi escaneado");
                Decimal.TryParse(input.Substring(7, 5), out quant);
                var produtoEncontradoPrePesado = ConverterInformacaoEmProduto(input.Substring(0, 7));
                if (produtoEncontradoPrePesado == null) { return; }

                input = produtoEncontradoPrePesado.ID_IDENTIFICADOR.ToString();
                string _tipo;
                using (var obtemdadosdoitem = new SP_TRI_OBTEMDADOSDOITEMTableAdapter())
                using (var dadosDoItem = new DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMDataTable())
                {
                    obtemdadosdoitem.Connection = LOCAL_FB_CONN;
                    obtemdadosdoitem.Fill(dadosDoItem, produtoEncontradoPrePesado.ID_IDENTIFICADOR);
                    _tipo = dadosDoItem.Rows[0][dadosDoItem.UNI_MEDIDAColumn].ToString();
                }
                if (_tipo == "KG")
                {
                    quant /= 1000;
                }
                _prepesado = true;

            }
            #endregion

            if (input == "") { return; }

            produtoEncontrado = ConverterInformacaoEmProduto(input.Safestring()); //Retorna um ID_IDENTIFICADOR de acordo com o que foi digitado
            log.Debug($"ConverterInformacaoEmProduto({input.Safestring()}): {(produtoEncontrado == null ? "null" : produtoEncontrado.ID_IDENTIFICADOR.ToString())}");

            if (produtoEncontrado == null) { return; }

            #region Lançamento de produto no cupom

            decimal vUnCom;
            //decimal comdesc;
            decimal vDescAplic = 0;
            using var ESTOQUE_TA = new TB_ESTOQUETableAdapter();

            if (decimal.TryParse(ESTOQUE_TA.SP_TRI_PEGAPRECO(/*cod_produto*/produtoEncontrado.ID_IDENTIFICADOR, quant).Safestring(),
                             out vUnCom) == false)
            {
                throw new Exceptions.DataNotLoadedException("Não foi possível \"parsear\" o preço do produto.");
            }
            log.Debug($"SP_TRI_PEGAPRECO({produtoEncontrado.ID_IDENTIFICADOR}, {quant}): {vUnCom}");

            if (tipoDeDesconto == tipoDesconto.Percentual)
            {
                log.Debug($"Aplicado desconto: {desconto}%");
                //comdesc = vUnCom - (vUnCom * desconto);
                vDescAplic = vUnCom * desconto * quant;
                tipoDeDesconto = tipoDesconto.Nenhum;
                txb_Avisos.Text = "CUPOM ABERTO";
            }
            else if (tipoDeDesconto == tipoDesconto.Absoluto)
            {
                log.Debug($"Aplicado desconto: R${desconto}");
                //comdesc = vUnCom - desconto;
                //vDescAplic = (vUnCom - desconto) * quant;
                vDescAplic = desconto;
                tipoDeDesconto = tipoDesconto.Nenhum;
                txb_Avisos.Text = "CUPOM ABERTO";
            }
            //else { comdesc = vUnCom; }

            //Checa se é possível fazer a venda do produto se o estoque for negativo, e lança o produto.
            if (_modo_consulta == false && _emTransacao == true)
            {
                using (var EST_PRODUTO_TA = new TB_EST_PRODUTOTableAdapter())
                {
                    EST_PRODUTO_TA.Connection = LOCAL_FB_CONN;

                    //TODO: se o caixa estiver configurado para permitir venda de produto com estoque negativo 
                    // e para não notificar, nem precisa pesquisar a quantidade em estoque.

                    if (PERMITE_ESTOQUE_NEGATIVO == false || PERMITE_ESTOQUE_NEGATIVO == null)
                    {
                        decimal? qtdeEstoque = EST_PRODUTO_TA.ConsultaQtde(produtoEncontrado.ID_IDENTIFICADOR);
                        log.Debug($"EST_PRODUTO_TA.ConsultaQtde({produtoEncontrado.ID_IDENTIFICADOR}): {qtdeEstoque}");

                        if (qtdeEstoque <= 0 && !_modoDevolucao)
                        {
                            if (PERMITE_ESTOQUE_NEGATIVO == false)
                            {
                                DialogBox.Show(strings.ESTOQUE_VAZIO,
                                               DialogBoxButtons.No, DialogBoxIcons.Info, false,
                                               strings.NAO_HA_ESTOQUE_DISPONIVEL,
                                               strings.IMPOSSIVEL_PROSSEGUIR_COM_A_VENDA);
                                combobox.Text = "";
                                return;
                            }
                            else if (PERMITE_ESTOQUE_NEGATIVO == null)
                            {
                                switch (DialogBox.Show(strings.ESTOQUE_VAZIO,
                                                            DialogBoxButtons.Yes, DialogBoxIcons.Info, false,
                                                            strings.NAO_HA_ESTOQUE_DISPONIVEL,
                                                            strings.SERA_GERADO_UM_RELATORIO))
                                {
                                    case true:

                                        #region Buscar a descrição do produto informado

                                        // Detalhe importante: se o produto encontrado estiver apenas no servidor 
                                        // (novo cadastro, nem deu tempo de sincronizar), o retorno do objeto "produtoEncontrado"
                                        // deverá conter apenas a property "ID_IDENTIFICADOR".
                                        // Neste caso, deve consultar a descrição direto do banco.

                                        string descricaoProduto = produtoEncontrado.DESCRICAO;

                                        if (string.IsNullOrWhiteSpace(descricaoProduto))
                                        {
                                            ESTOQUE_TA.Connection = LOCAL_FB_CONN;
                                            descricaoProduto = ESTOQUE_TA.DescricaoPorID(/*(int)cod_produto*/produtoEncontrado.ID_IDENTIFICADOR).ToString();
                                        }

                                        #endregion Buscar a descrição do produto informado

                                        RelNegativ.RecebeProduto(produtoEncontrado.ID_IDENTIFICADOR.ToString(),
                                                             descricaoProduto, //TODO: não precisa buscar a descrição por id. Basta pegar a descrição direto no ACBox. OOuuuuu, se o objeto "produtoEncontrado" não tiver valor na property "DESCRICAO", o valor deve ser recuperado no banco na etapa anterior.
                                                             vUnCom);
                                        break;
                                    case false:
                                        return;
                                    default:
                                        return;
                                }
                            }
                        }
                    }
                    ProcessarItemNovo(produtoEncontrado.ID_IDENTIFICADOR,
                         vUnCom,
                         quant.RoundABNT(3),
                         vDescAplic);
                }

                numProximoItem += 1;
            }
            #endregion

            //Faz a consulta do produto, apenas.
            else if (_modo_consulta == true)
            {
                PesquisarItem(produtoEncontrado.ID_IDENTIFICADOR, vUnCom, quant, _prepesado);
            }
            //});
        }

        /// <summary>
        /// Tenta remover um item do objeto de venda atual
        /// </summary>
        private void RemoverItemDaVendaNovo()
        {
            bool _permissao;
            if (PEDESENHACANCEL)
            {
                _permissao = PedeSenhaGerencial("Removendo item da venda");
            }
            else
            {
                _permissao = true;
            }
            if (_permissao == true)
            {
                var remitem = new RemoverItem(numProximoItem - 1);
                if (remitem.ShowDialog() == true)
                {
                    envCFeCFeInfCFeDetProd produtoARemover = vendaAtual.RemoveProduto(remitem._int);
                    if (produtoARemover is null)
                    {
                        DialogBox.Show(strings.ESTORNO_DE_ITEM, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.ITEM_INVALIDO_VERIFIQUE);
                        return;
                    }
                    try
                    {
                        decimal.TryParse(produtoARemover.vUnCom.Replace('.', ','), out decimal precounit);
                        decimal.TryParse(produtoARemover.qCom.Replace('.', ','), out decimal quant);
                        decimal.TryParse(produtoARemover.vDesc.Replace('.', ','), out decimal desc);

                        ImprimirCupomVirtual(string.Format("CANCELADO - " + produtoARemover.xProd.Trunca(29)));
                        ImprimirCupomVirtual(string.Format(@"{0} {1} {2} {3}", quant.RoundABNT(3).ToString().Trunca(4).PadLeft(8, ' '), produtoARemover.uCom, precounit.RoundABNT().ToString().PadLeft(10, ' '), (quant * (precounit - desc)).ToString("0.00").PadLeft(20)));

                        subtotal -= ((precounit - desc) * quant).RoundABNT();
                        txb_TotGer.Text = subtotal.RoundABNT().ToString("C2");
                        numProximoItem += 1;

                    }
                    catch (Exception ex)
                    {
                        log.Error("Erro ao remover item da venda", ex);
                        DialogBox.Show(strings.ESTORNO_DE_ITEM, DialogBoxButtons.No, DialogBoxIcons.Error, false, strings.FALHA_AO_ESTORNAR, RetornarMensagemErro(ex, false));
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Se o AmbiOrcamento estiver aberto, traz a janela para frente
        /// </summary>
        private void TrazerJanOrcamentoPraFrente()
        {
            Process[] processo = Process.GetProcessesByName("AmbiORCAMENTO");
            if (processo.Length != 0)
            {
                IntPtr handle = processo[0].MainWindowHandle;
                if (IsIconic(handle))
                {
                    ShowWindow(handle, SW_RESTORE);
                }
                SetForegroundWindow(handle);
            }
        }

        ///// <summary>
        ///// Detecta se um pedido foi inserido e carrega os produtos nele lançados
        ///// </summary>
        //private bool CarregaProdutosDoPedido()
        //{
        //    //TODO: testar função
        //    if (!ACBox.Text.StartsWith(":")) { return false; }

        //    try
        //    {
        //        //Contingencia();
        //        pedido_atual.Clear();
        //        int.TryParse(ACBox.Text.TrimStart('+'), out int pedido);
        //        audit("LEPEDIDO>> Orçamento detectado: " + pedido);
        //        //if (pedido_atual.LePedidoProdutos(pedido) && pedido_atual.LePedido(pedido))

        //        if (!pedido_atual.LePedido(pedido))
        //        {
        //            audit("LEPEDIDO>> Erro ao ler pedido, ou pedido está FECHADO");
        //            MessageBox.Show("Orçamento indisponível.");
        //            ACBox.Text = "";
        //            return true;
        //        }

        //        if (!pedido_atual.LePedidoProdutos(pedido))
        //        {
        //            audit("LEPEDIDO>> Orçamento sem item");
        //            return true;
        //        }

        //        //var cods = new List<int>();
        //        //var qtds = new List<decimal>();
        //        //foreach (var item in pedido_atual.produtos)
        //        //{
        //        //    cods.Add(item.codigo);
        //        //    qtds.Add(item.quantidade);
        //        //}

        //        //if (cods.Count == 0)
        //        if (pedido_atual.produtos.Count == 0)
        //        {
        //            ACBox.Text = "";
        //            MessageBox.Show("Pedido vazio.");
        //            return true;
        //        }

        //        if ((_tipo == CfeRecepcao_0007.ItemChoiceType.FECHADO || _tipo == CfeRecepcao_0007.ItemChoiceType.DEVOLUCAO) && modo_consulta == false)
        //        {
        //            if (_tipo != CfeRecepcao_0007.ItemChoiceType.DEVOLUCAO)
        //            {
        //                _tipo = CfeRecepcao_0007.ItemChoiceType.ABERTO;
        //            }
        //            if (!ChecagemPreVenda())
        //            {
        //                return false;
        //            }

        //            if (!CabecalhoDoCupom())
        //            {
        //                return false;
        //            }
        //        }

        //        //if (cods.Count == 0)
        //        //{
        //        //    ACBox.Text = "";
        //        //    var db = DialogBox.Show("Pedido vazio", "Não há serviços/produtos lançados nesse pedido.", DialogBoxButtons.No, DialogBoxIcons.None);
        //        //    db.ShowDialog();
        //        //    return true;
        //        //}
        //        //using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
        //        //using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
        //        //{
        //        foreach (var item in pedido_atual.produtos)
        //        {
        //            if (item.QTD_ITEM <= 0)
        //            {
        //                (DialogBox.Show("Erro no pedido",
        //                               "A quantidade de um ou mais itens era negativa ou zero.",
        //                               "Impossível prosseguir com a venda.",
        //                               DialogBoxButtons.No, DialogBoxIcons.Warn)).ShowDialog();
        //                FinalizacaoDaVenda();
        //                return true;
        //            }

        //            //processaItem(cods[i], (decimal)ESTOQUE_TA.SP_TRI_PEGAPRECO(cods[i], qtds[i]), qtds[i], (decimal)ESTOQUE_TA.SP_TRI_PEGAPRECO(cods[i], qtds[i]), ESTOQUE_TA, EST_PRODUTO_TA);
        //            processaItem(item.ID_IDENTIFICADOR,
        //                         //(item.VALOR - item.DESCONTO), // não tem desconto
        //                         item.VALOR,
        //                         item.QTD_ITEM,
        //                         item.VALOR);

        //            itematual += 1;

        //            ACBox.Text = "";
        //        }
        //        //}

        //        //FechaPedido();
        //        usou_pedido = true;
        //        return true;
        //    }
        //    catch (Exception erro)
        //    {
        //        (DialogBox.Show("Erro ao obter pedido", RetornarMensagemErro(erro, false), DialogBoxButtons.No, DialogBoxIcons.Error)).ShowDialog();
        //        gravarMensagemErro(RetornarMensagemErro(erro, true));
        //    }
        //    return false;
        //}




        ///// <summary>
        ///// "Consome" o pedido. Não será possível editar ou reutilizá-lo.
        ///// Seta "Status" para "FECHADO" e vincula o orçamento com o cupom.
        ///// </summary>
        //private void FecharPedido(int pNoCupom)
        //{
        //    //TODO: 

        //    //bool blnFechadoComSucesso = false;

        //    try
        //    {
        //        using (var taPedidoServ = new DataSets.FDBDataSetOrcamTableAdapters.TRI_ORCA_ORCAMENTOSTableAdapter())
        //        using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
        //        {
        //            taPedidoServ.Connection = SERVER_FB_CONN;

        //            taPedidoServ.SP_TRI_PEDIDO_FECHAPEDIDO(pedido_atual.no_pedido,
        //                                             pNoCupom,
        //                                             Convert.ToInt16(NO_CAIXA));
        //        }
        //        //blnFechadoComSucesso = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // TODO: um erro possível pode ser a utilização de um orçamento em mais de um caixa ao mesmo tempo.
        //        // Pode engatilhar violação de chave primária em TRI_PDV_PEDIDO_CUPOM_REL (ID_PEDIDO e ID_CUPOM).
        //        // Se acontecer, o primeiro consumo deve fechar o orçamento e os seguintes apresentar erro, mas deverão
        //        // fechar a venda normalmente.

        //        gravarMensagemErro(RetornarMensagemErro(ex, true));
        //        MessageBox.Show("Erro ao fechar orçamento!");
        //    }
        //    finally
        //    {
        //        //if (blnFechadoComSucesso) { usou_pedido = false; }
        //        usou_pedido = false; // Independentemente do resultado deste método, deve indicar o término do uso do pedido na venda, para não comprometer o funcionamento subsequente.
        //    }
        //}

        #endregion Methods

        private void Combobox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessarTextoNoACBox();
            //else MainWindow_KeyDown(sender, e);
        }

        private void Caixa_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            /*Os métodos a seguir dependem da configuração do sistema:
             * Caso o sistema esteja configurado para funcionar apenas em modo não fiscal,
             * Apertar F2 ou F3 finaliza o cupom não fiscal, sem perguntar o CPF ao
             * cliente.
             * 
             * Caso o sistema esteja configurado para funcionar apenas em modo fiscal,
             * Apertar F2 não faz nada, enquanto F3 fecha o cupom normalmente.
             * 
             * Por, fim, caso o sistema funcione em ambos os modos, F2 finaliza o
             * cupom não fiscal e F3 finaliza no fiscal.              
             */
            if (e.Key == Key.F1 && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                //combobox.SelectedIndex = 13732;
                e.Handled = true;
                AlternarPainelDeAjuda();
            } // Abre um painel de ajuda (Tecla F1)
            if (e.Key == Key.F1 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (_tipo == ItemChoiceType.ABERTO)
                {
                    string cliente;
                    var PC = new PerguntaCliente(1);
                    PC.ShowDialog();
                    switch (PC.DialogResult)
                    {
                        case true:
                            cliente = PC.nome_cliente;
                            break;
                        default:
                            MessageBox.Show("É necessário informar um cliente.");
                            return;
                    }
                    vendaAtual.TotalizaCupom();
                    (int id, int nf) info = vendaAtual.GravaNaoFiscalBase(0, 99, 0);

                    foreach (var item in vendaAtual.RetornaCFe().infCFe.det)
                    {
                        Remessa.RecebeProduto(item.prod.cProd, item.prod.xProd, item.prod.uCom, item.prod.qCom.Safedecimal(), item.prod.vUnComOri.Safedecimal());

                    }
                    Remessa.numerodocupom = info.nf;
                    Remessa.cliente = cliente;
                    Remessa.IMPRIME();
                    CancelarVendaAtual();
                }
            }
            /* ---------------*/
            else if (e.Key == Key.F2 && _emTransacao == true && IMPRESSORA_USB != "Nenhuma")
            {
                debounceTimer.Debounce(125, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    PrepararFinalizacaoDeCupomDemo();
                });

                return;
            }//Inicia o fechamento de de uma venda sem CPF (Tecla F2)
            /* ---------------*/
            else if (e.Key == Key.F3 && _emTransacao == true)
            {
                debounceTimer.Debounce(125, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    PrepararFinalizacaoDeCupomFiscal();
                });
            } //Inicia o fechamento de de uma venda com CPF(Tecla F3)
            /* ---------------*/
            else if (e.Key == Key.F4 && _modo_consulta)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    AbrirConsultaAvancada();
                });
            } // Ativa o modo de consulta (Tecla F4)
            /* ---------------*/
            else if (e.Key == Key.F4 && _emTransacao && !(vendaAtual is null) && !_modo_consulta)//Remove um item do cupom com a venda aberta. (Chama método)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    RemoverItemDaVendaNovo();
                });
            } //Ativa modo de cancelamento de item (Tecla F4 com venda ativa)
            /* ---------------*/
            else if (e.Key == Key.F5)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    AlternarModoDeConsulta();
                });
            } // Ativa modo de consulta avançado (Tecla F5)
            /* ---------------*/
            else if (e.Key == Key.F6 && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                //cancelamentoAtual.CancelaCupomFiscal(new CupomSAT());
                //return;
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    if (!_emTransacao)
                    {
                        CancelarUltimoCupom();
                    }
                    else
                    {
                        if (!PERMITE_CANCELAR_VENDA_EM_CURSO || !PedeSenhaGerencial("Cancelamento da Venda Atual", false))
                        {
                            return;
                        }
                        CancelarVendaAtual();
                    }
                });
            } //Ativa o cancelamento de compras (Tecla F6)
            /* ---------------*/
            else if (e.Key == Key.F7)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    //#if HOMOLOGADEVOL
                    NovoModoDeDevolucao();
                    //#else

                    //AlternarModoDevolucao();
                    //#endif
                });
            } // Ativa o modo de devolução (Tecla F7)
            /* ---------------*/
            else if (e.Key == Key.F8)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    AlternarDescontoNoItem();
                });
            }// Ativa modo de desconto (Tecla F8)
            /* ---------------*/
            else if (e.Key == Key.F9 && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    PerguntarNumeroDoOrcamento();
                });
            } // Ativa modo de orçamento
            /* ---------------*/
            else if (e.Key == Key.F9 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                TrazerJanOrcamentoPraFrente();
            } //Ativa modo de orçamento por período
            /* ---------------*/
            else if (e.SystemKey == Key.F10 && USATEF)
            {
                e.Handled = true;
                SiTEFBox tefAdmin = new SiTEFBox();
                DateTime timestampFiscal = DateTime.Now;
                infoAdminTEF = (timestampFiscal.ToString("ddhhmmss"), timestampFiscal.ToString("yyyyMMdd"), timestampFiscal.ToString("hhmmss"));
                tefAdmin.StatusChanged += Tef_StatusChanged;
                tefAdmin.ShowTEF(TipoTEF.Administrativo, 0, infoAdminTEF.sequencial, timestampFiscal, -1);
                _emTransacao = true;
                //debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                //{

                //});
            }
            /* ---------------*/
            else if (e.Key == Key.F11 && e.KeyboardDevice.Modifiers == ModifierKeys.None && !_emTransacao && !_modo_consulta)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    AbrirJanelaSangriaSupr();
                });
            } // Ativa modo sangria/suprimento
            else if (e.Key == Key.F11 && e.KeyboardDevice.Modifiers == ModifierKeys.Control && !_emTransacao && !_modo_consulta)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    if (!PedeSenhaGerencial("LISTANDO REIMPRESSÃO DE SANGRIAS"))
                    {
                        return;
                    }
                    new ListaSanSup().ShowDialog();

                });
            } // Ativa modo sangria/suprimento
            /* ---------------*/
            else if (e.Key == Key.F12 && e.KeyboardDevice.Modifiers == ModifierKeys.None && !_emTransacao) // condicional pra saber se o botão f12 foi pressionado e se algum cupom de venda não estiver aberto
            {
                e.Handled = true;//Encerra um processo de Keypress, ou seja, caso eu digite a tecla acima, ela não será preenchida em nenhum text box e nem terá efeitos em nenhum campo.  
                log.Debug($"turno_aberto = {turno_aberto}");
                switch (turno_aberto)// condicional para descobrir o status do turno
                {
                    case true://Caso o turno esteja true, quer dizer que está aberto e você deseja fechar, então o método abaixo irá fechar o caixa 
                        if (ExecFechamentoCaixa()) // já faz sync (tudo)
                        {
                            DialogBox.Show(strings.FECHAMENTO_DE_TURNO, DialogBoxButtons.Yes, DialogBoxIcons.Info, false, strings.TURNO_FECHADO_COM_SUCESSO);
                        }
                        break;
                    case false:// Caso seja false, quer dizer que está fechado e você deseja abrir, o método abaixo irá abrir o caixa
                        if (ExecAberturaCaixa())
                        {
                            DialogBox.Show(strings.ABERTURA_DE_TURNO, DialogBoxButtons.Yes, DialogBoxIcons.Info, false, strings.TURNO_ABERTO_COM_SUCESSO);
                            //TODO: fazer sync (vendas)
                            IniciarSincronizacaoDB(EnmTipoSync.vendas, Settings.Default.SegToleranciaUltSync);
                        }
                        break;
                }
            }//Faz o fechamento do caixa.
            /* ---------------*/
            else if (e.Key == Key.F12 && e.KeyboardDevice.Modifiers == ModifierKeys.Control && !_emTransacao)// Ao apertar F12 + CRTL essa condição será aceita
            {
                if (!PedeSenhaGerencial(@strings.LISTANDO_REIMPRESSAO_DE_FECHAMENTOS))
                {
                    return;
                }

                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    new Fechamentos().ShowDialog();
                });
            }//Reimpressão de caixa fechado
            /* ---------------*/
            else if (e.Key == Key.Escape && e.KeyboardDevice.Modifiers == ModifierKeys.Shift && !_emTransacao)
            {
                e.Handled = true;
                bool _senha = PedeSenhaGerencial("Fechando o programa");
                if (_senha == true)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    combobox.Text = null;
                }
            }//Pergunta senha para fechar o sistema.
            /* ---------------*/
            else if (e.Key == Key.Escape && combobox.Text != "")
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    combobox.Text = "";
                });
            }//Limpa o campo de pesquisa.
            /* ---------------*/
            else if (e.Key == Key.Escape && _usouOrcamento || _usouOS)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    if (DialogBox.Show(strings.ORCAMENTO, DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Pausar esse(a) orçamento/OS agora?", "Você poderá usá-lo mais tarde.") == true)
                    {
                        #region Zerar o cupom para iniciar um novo
                        LimparObjetoDeVendaNovo();
                        LimparTela();
                        LimparUltimaVenda();
                        _usouOrcamento = _usouOS = false;
                        #endregion Zerar o cupom para iniciar um novo
                    }
                });
            }//Limpa o cupom virtual e deixa o caixa livre.

            else if (e.Key == Key.Escape && _usouOS)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    if (DialogBox.Show("OS", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Pausar essa OS agora?", "Você poderá usá-la mais tarde.") == true)
                    {
                        #region Zerar o cupom para iniciar um novo
                        LimparObjetoDeVendaNovo();
                        LimparTela();
                        LimparUltimaVenda();
                        _usouOS = false;
                        #endregion Zerar o cupom para iniciar um novo
                    }
                });
            }//Limpa o cupo

            /* ---------------*/
            //TODO: terminar e testar snippet
            else if (e.Key == Key.Escape && _usouPedido)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    if (DialogBox.Show(strings.PEDIDO, DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Não usar esse pedido agora?", "Você poderá usá-lo mais tarde.") == true)
                    {
                        #region Zerar o cupom para iniciar um novo
                        LimparObjetoDeVendaNovo();
                        LimparTela();
                        #endregion Zerar o cupom para iniciar um novo
                    }
                });
            }//Limpa o cupom virtual e deixa o caixa livre.
            /* ---------------*/
            else if (e.Key == Key.B && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    try
                    {
                        log.Debug("Pega peso da balança - TESTE");
                        PegarPesoDaBalanca();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Testar balança", ex);
                        DialogBox.Show(strings.OBTER_PESO, DialogBoxButtons.No, DialogBoxIcons.Error, true, RetornarMensagemErro(ex, false));
                        return;
                    }
                });
            }//Tenta pegar o peso da balança.
            /* ---------------*/
            else if (e.Key == Key.G && e.KeyboardDevice.Modifiers == ModifierKeys.Control && !_emTransacao)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    if (PedeSenhaGerencial("Abrindo Gaveta"))
                    {
                        if (IMPRESSORA_USB != "Nenhuma")
                        {
                            AbreGaveta();
                        }
                        else if (ECF_ATIVA)
                        {
                            ECF.AbreGaveta();
                        }
                    }
                });
            }//Pergunta senha para abrir a gaveta.
            /* ---------------*/
            else if (e.Key == Key.M && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                bool _senha = PedeSenhaGerencial("Minimizando o Programa");
                if (_senha == true)
                {
                    WindowState = WindowState.Minimized;
                }
                else
                {
                    combobox.Text = null;
                }
            }//Minimiza o programa
            /* ---------------*/
            else if (e.Key == Key.O && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    PerguntarNumeroDaOS();
                    //if (!ECF.EfetuaReducaoZ()) { return; }
                });
            } //Extração de relatório de impressora fiscal
            /* ---------------*/
            else if (e.Key == Key.W && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (File.Exists(@".\logo.png"))
                {
                    log.Debug("Logo encontrado. Carregando novo logo...");

                    logoplaceholder.Visibility = Visibility.Collapsed;

                    // Create Image and set its width and height  
                    Image dynamicImage = new Image();
                    //dynamicImage.Width = 500;
                    //dynamicImage.Height = 500;

                    // Create a BitmapSource  
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(@"logo.png", UriKind.RelativeOrAbsolute);
                    bitmap.EndInit();

                    // Set Image.Source  
                    dynamicImage.Source = bitmap;

                    grd_Grid1.Children.Add(dynamicImage);
                    dynamicImage.SetValue(Grid.ColumnProperty, 1);
                    dynamicImage.UpdateLayout();
                }
            } //Dá um refresh no logo da aplicação
            /* ---------------*/
            else if (e.Key == Key.P && e.KeyboardDevice.Modifiers == ModifierKeys.Control && !_emTransacao && !_contingencia)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    bool _senha = PedeSenhaGerencial("Abrindo Configurações");
                    if (_senha == true)
                    {
                        Parametros par = new Parametros(turno_aberto, _contingencia) { conf_inicial = false };
                        par.ShowDialog();

                        IniciarSincronizacaoDB(EnmTipoSync.cadastros, 0);

                        CarregaConfigs();
                        return;
                    }
                    else
                    {
                        combobox.Text = null;
                    }
                });
            }//Pergunta senha para abrir a tela de parâmetros.
            /* ---------------*/
            else if (e.Key == Key.R && e.KeyboardDevice.Modifiers == ModifierKeys.Control && IMPRESSORA_USB != "Nenhuma")
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    new ReimprimeCupons().ShowDialog();
                });
            }//Tenta pegar o peso da balança.
            /* ---------------*/
            else if (e.Key == Key.S && e.KeyboardDevice.Modifiers == ModifierKeys.Control && !_emTransacao && !_modo_consulta)// Sincronização manual

            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    // Ctrl+S para sync
                    e.Handled = true;

                    /// ToDo:
                    /// Perguntar pro usuário se ele tem certeza se quer continuar;
                    /// Exibir uma tela de aguardar.
                    if (DialogBox.Show(strings.SINCRONIZACAO, DialogBoxButtons.YesNo, DialogBoxIcons.Warn, false, strings.PDV_INICIARA_ATUALIZACAO_DE_REGISTROS) == true)
                    {
                        ChecarPorContingencia(_contingencia, Settings.Default.SegToleranciaUltSync, EnmTipoSync.tudo);
                        IniciarSincronizacaoDB(EnmTipoSync.tudo, Settings.Default.SegToleranciaUltSync/*, EnmTipoSync.CtrlS*/);
                    }

                    //IniciarTestes();
                });
            } // Sincronização manual
            /* ---------------*/
            else if (e.Key == Key.T && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                funcoesClass Func = new funcoesClass();
                string IDAnyDesk = Func.ObtemIDAnyDesk();
                DialogBox.Show(strings.ACESSO_REMOTO, DialogBoxButtons.Yes, DialogBoxIcons.None, true, strings.INFORME_SUA_ID, IDAnyDesk);
            } // Informa o ID para acesso remoto pelo AnyDesk
            /* ---------------*/
            else if (e.Key == Key.T && e.KeyboardDevice.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift))
            {
                e.Handled = true;
                if (new SenhaTecnico().ShowDialog() != true)
                {
                    return;
                }

                if (DialogBox.Show(strings.AUTOTESTE, DialogBoxButtons.YesNo, DialogBoxIcons.Warn, true, strings.DESEJA_EXECUTAR_AUTOTESTE) == false)
                {
                    return;
                }

                _interromperModoTeste = false;
                Thread meuThread = new Thread(ExecutaTeste);
                meuThread.SetApartmentState(ApartmentState.STA);
                meuThread.Start();
                _modoTeste = true;
                ExecTesteMassivo();
                return;
            } //Executa uma bateria de teste na base de dados, injetando dados validos para verificar possíveis erros ou conflitos.

            else
            {
                if (!(FocusManager.GetFocusedElement(MainWindow).GetType() == typeof(TextBox))) combobox.Focus();
            }//Volta o foco para a caixa de pesquisa.
        }
    }

    #region Classes Auxiliares

    public class ComboBoxBindingDTO_Produto
    {
        /// <summary>
        /// TB_EST_PRODUTO
        /// </summary>
        public int ID_IDENTIFICADOR { get; set; }
        /// <summary>
        /// TB_ESTOQUE
        /// </summary>
        public string DESCRICAO { get; set; }
        /// <summary>
        /// TB_EST_PRODUTO
        /// </summary>
        public string COD_BARRA { get; set; }
        /// <summary>
        /// TB_EST_PRODUTO
        /// </summary>
        public string REFERENCIA { get; set; }
        public string STATUS { get; set; }
        //public decimal? QTD_ATACADO { get; set; }
        //public decimal PRC_VENDA { get; set; }
    }


    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return new UTF8Encoding(false); }
        }
    }

    internal class InterceptKeys
    {
        #region Delegates

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        private const int WH_KEYBOARD_LL = 13;
        //private const int WM_KEYDOWN = 0x0100;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    #endregion Classes Auxiliares

}