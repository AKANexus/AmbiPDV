using CfeRecepcao_0008;
using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Objetos;
using PDV_WPF.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    public partial class FechamentoCupom : Window
    {
        #region Fields & Properties

        Logger log = new Logger("Fechamento de cupom");
        public Dictionary<string, string> respCRT { get; set; }
        //public static Dictionary<string, string> respCRT = new Dictionary<string, string>() { get; set;}
        public decimal desconto { get; set; }
        public List<string> nomes_pgtos = new List<string>();
        public List<decimal> valores_pgtos = new List<decimal>();
        public List<(int, decimal)> devolucoes_usadas = new List<(int, decimal)>();
        public ItemChoiceType _tipo_int = ItemChoiceType.NENHUM;
        public string _info_int;
        private int metodo;
        private bool taxaAdicionada = false;
        //int id_promo;
        private decimal _desconto_maximo;
        public int id_cliente;
        public string nome_cliente;
        public DateTime vencimento;
        public decimal valor_a_ser_pago;
        private decimal valor_pago;
        private bool _modoTeste;
        private bool _painelFechado = true;
        public envCFeCFeInfCFePgto pgto = new envCFeCFeInfCFePgto();
        public List<envCFeCFeInfCFePgtoMP> metodos = new List<envCFeCFeInfCFePgtoMP>();
        public List<(string, decimal, int)> metodosnew = new List<(string, decimal, int)>();
        public List<SiTEFBox> tefUsados = new List<SiTEFBox>();
        //private OperTEF tefAtual;
        public Venda _vendaAtual;
        private SiTEFBox tefAtual;
        //private FDBDataSet.TRI_PDV_METODOSDataTable Metodos_DT = new FDBDataSet.TRI_PDV_METODOSDataTable();
        private DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable Metodos_DT = new DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable();
        public decimal troco { get; set; }
        public Dictionary<int, decimal> pagamentos = new Dictionary<int, decimal>()
        {
            {1, 0},{2, 0},{3,0},{4,0 },{5,0 },{6,0 },{7,0},{8,0},{9,0},{10,0},{11,0},{12,0},{13,0},{14,0},{15,0},{16,0},{17,0},{18,0},{19,0},{20,0},{100,0}
            //Estes são ID_PAGAMENTO
        };
        public string numCupomTEF;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        public readonly decimal _vlrTotalVenda; public decimal descNaVenda = 0;
        private readonly bool _scannTech;
        private bool fechouManualmente;

        #endregion Fields & Properties

        #region (De)Constructor

        public FechamentoCupom(decimal desconto_maximo, decimal vlrTotalVenda, ref Venda vendaAtual, bool modoTeste = false, bool scannTech = false)
        {
            _vlrTotalVenda = vlrTotalVenda;
            _desconto_maximo = desconto_maximo;
            InitializeComponent();
            this.Title = NOMESOFTWARE + " - Fechamento de cupom.";
            txb_Metodo.Focus();
            _modoTeste = modoTeste;
            //tefAtual = tEFAtual;
            _vendaAtual = vendaAtual;
            _scannTech = scannTech;
        }

        /// <summary>
        /// Destrutor do objeto FechamentoCupom
        /// </summary>
        ~FechamentoCupom()
        {
            Metodos_DT?.Dispose();
        }

        #endregion (De)Constructor

        #region Events

        private void but_F1_MouseEnter(object sender, EventArgs e)
        {
            but_F1.FontSize = 33;
        }
        private void but_F1_MouseLeave(object sender, EventArgs e)
        {
            but_F1.FontSize = 25;
        }
        private void but_F4_MouseEnter(object sender, EventArgs e)
        {
            but_F4.FontSize = 33;
        }
        private void but_F4_MouseLeave(object sender, EventArgs e)
        {
            but_F4.FontSize = 25;
        }
        private void but_F7_MouseEnter(object sender, EventArgs e)
        {
            but_F7.FontSize = 37;
        }
        private void but_F7_MouseLeave(object sender, EventArgs e)
        {
            but_F7.FontSize = 29;
        }
        private void but_F8_MouseEnter(object sender, EventArgs e)
        {
            but_F8.FontSize = 35;
        }
        private void but_F8_MouseLeave(object sender, EventArgs e)
        {
            but_F8.FontSize = 27;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            using (var FbComm = new FbCommand())
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
            {
                FbComm.Connection = LOCAL_FB_CONN;
                FbComm.CommandType = CommandType.Text;
                switch (_tipo_int)
                {
                    case ItemChoiceType.CNPJ:
                    case ItemChoiceType.CPF:
                    case ItemChoiceType.NENHUM:
                        FbComm.CommandText = @$"SELECT MAX(NF_NUMERO)+1 FROM TB_NFVENDA WHERE NF_MODELO = '59' AND NF_SERIE = '{NO_CAIXA}'";
                        break;
                    default:
                        FbComm.CommandText = @$"SELECT MAX(NF_NUMERO)+1 FROM TB_NFVENDA WHERE NF_MODELO = '59' AND NF_SERIE = 'N{NO_CAIXA}'";
                        break;
                }
                if (LOCAL_FB_CONN.State != ConnectionState.Open) LOCAL_FB_CONN.Open();
                numCupomTEF = FbComm.ExecuteScalar().ToString();
            }

            try
            {
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                //using (var taMetodosPdv = new FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter())
                using var taMetodosPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter
                {
                    Connection = LOCAL_FB_CONN
                };
                taMetodosPdv.FillByAtivos(Metodos_DT);
            }
            catch (Exception ex)
            {
                log.Error("Erro ao preencger por métodos de pgto. ativos", ex);
                MessageBox.Show("Erro ao consultar métodos de pagamento ativos. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                Environment.Exit(0); // deuruim();
                return;
            }
            dgv_Metodos.ItemsSource = Metodos_DT;

            valor_a_ser_pago = _vendaAtual.ValorDaVenda().RoundABNT();
            txb_Valor.Value = 0;
            //txb_Valor.Text = valor_a_ser_pago.ToString("C2", ptBR);
            txb_Valor.Value = valor_a_ser_pago;
            txb_SaldoRest.Value = valor_a_ser_pago;
            txb_Pago.Value = 0;
            VerificaVlrTotal(); //CHAMA METODO QUE VERIFICA SE VALOR TOTAL DA VENDA É DIFERENTE DO SALDO RESTANTE
            if (_vendaAtual.DescontoAplicado() > 0)
            {
                desconto = _vendaAtual.DescontoAplicado();
                valor_a_ser_pago = (_vendaAtual.ValorDaVenda() - desconto);
                stp_Desconto.Visibility = Visibility.Visible;
                txb_Desconto.Value = desconto;
                txb_SaldoRest.Value = valor_a_ser_pago;
                txb_Valor.Value = 0;
                //txb_Valor.Text = valor_a_ser_pago.ToString("C2", ptBR);
                txb_Valor.Value = valor_a_ser_pago;
                txb_Metodo.Focus();
            }
            if (USATEF)
                RecuperarTEFsEfetuados();
            txb_Metodo.Focus();


            if (_modoTeste)
            {
                var rand = new Random();
                //using (var METODOS_TA = new FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter())
                //using (var METODOS_DT = new FDBDataSet.TRI_PDV_METODOSDataTable())
                using (var METODOS_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter())
                using (var METODOS_DT = new DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable())
                {
                    METODOS_TA.FillByAtivos(Metodos_DT);
                }
                bool pagamentoIncompleto = true;
                for (int i = 0; i < 1; i++)
                {
                    if (!pagamentoIncompleto) return;
                    int forma = Metodos_DT[rand.Next(Metodos_DT.Rows.Count - 1)].ID_FMANFCE;
                    if (forma == 1)
                    {
                        txb_Metodo.Text = forma.ToString();
                        int valorEmCentavos = (int)(_vendaAtual.ValorDaVenda() * 100M);
                        int valorASerPagoEmCentavos = rand.Next(valorEmCentavos, valorEmCentavos + 10000);
                        txb_Valor.Value = (((decimal)valorASerPagoEmCentavos) / 100);
                        pagamentoIncompleto = false;
                        ProcessarMetodoDePagamento();
                    }
                    else
                    {
                        txb_Metodo.Text = forma.ToString();
                        txb_Valor.Value = _vendaAtual.ValorDaVenda();
                        pagamentoIncompleto = false;
                        ProcessarMetodoDePagamento();
                    }
                }
            }

        }

        private void RecuperarTEFsEfetuados()
        {
            PendenciasDoTEF pendTef = new PendenciasDoTEF();
            var pendencias = pendTef.ObtemListaDePendencias(numCupomTEF);
            for (int i = 0; i < pendencias.Count; i++)
            {
                if (pendencias[i].Tipo == "credito")
                    AcrescentaMetodoPagamento(3, Decimal.Parse(pendencias[i].ValOriginal, CultureInfo.InvariantCulture), "3");
                if (pendencias[i].Tipo == "debito")
                    AcrescentaMetodoPagamento(4, Decimal.Parse(pendencias[i].ValOriginal, CultureInfo.InvariantCulture), "4");
            }
        }

        private void RemovePendenciasTef()
        {

        }

        private bool ValidarMetodo(int idmetodo)
        {
            foreach (var DataRow in Metodos_DT)
            {
                if (DataRow.ID_FMANFCE == idmetodo)
                {
                    return true;
                }
            }
            return false;
        }

        private void txb_Metodo_KeyDown(object sender, KeyEventArgs e)        
        {
            if (e.Key == Key.Enter)            
            {
                int.TryParse(txb_Metodo.Text, out int _result);
                switch (ValidarMetodo(_result))
                {
                    case true:
                        if (_result == 3 && SYSPARCELA.ToBool())
                        {
                            stp_Parcelas.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            stp_Parcelas.Visibility = Visibility.Hidden;
                        }                                                                      
                        metodo = _result;
                        txb_Valor.Focus();
                        txb_Valor.SelectAll();
                        break;
                    case false:
                        MessageBox.Show("Método não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }
            }
        }

        private void txb_Parcelas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessarMetodoDePagamento();
            }
        }

        private void ProcessarMetodoDePagamento()
        {
            int.TryParse(txb_parcelas.Text, out int parcelas);
            if (parcelas <= 0) parcelas = 1;

            int.TryParse(txb_Metodo.Text, out int idMetodo);
            decimal _valor = txb_Valor.Value;
            if (_valor == 0) _valor = valor_a_ser_pago;
            else if (_valor < 0 || _valor > 99999.99M)
            { DialogBox.Show("FECHAMENTO DE CUPOM", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Valor digitado é inválido."); return; }
            string strPgCfe = string.Empty;
            strPgCfe = (from linha in Metodos_DT
                        where linha.ID_FMANFCE == idMetodo
                        select linha.ID_NFCE).FirstOrDefault();
            if (idMetodo == 1 && _valor < valor_a_ser_pago)
            {
                DialogBox.Show(strings.VENDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, strings.VALOR_A_VISTA_NAO_PODE_SER_MENOR, strings.A_VISTA_DEVE_SER_O_ULTIMO_METODO);
                return;
            }

            if (strPgCfe is "03" or "04" or "10" or "11")
            {
                int intPagamentoDias = 10;
                if (DateTime.Today.Day < intPagamentoDias)
                {
                    string data = String.Format("{0}/{1}/{2}",
                        intPagamentoDias.Safestring(),
                        DateTime.Today.Month.ToString(),
                        DateTime.Today.Year.ToString()
                        );
                    vencimento = data.Safedate();
                }
                else
                {
                    string data = String.Format("{0}/{1}/{2}",
                        intPagamentoDias.Safestring(),
                        DateTime.Today.AddMonths(1).Month.ToString(),
                        DateTime.Today.AddMonths(1).Year.ToString()
                        );
                    vencimento = data.Safedate();
                }
                if (USATEF)
                {
                    tefAtual = new SiTEFBox();
                    switch (strPgCfe)
                    {
                        case "03":
                            tefAtual.ShowTEF(TipoTEF.Credito, _valor, $"{numCupomTEF}", DateTime.Now, idMetodo);
                            break;
                        case "04":
                            tefAtual.ShowTEF(TipoTEF.Debito, _valor, $"{numCupomTEF}", DateTime.Now, idMetodo);
                            break;
                        case "10":
                            tefAtual.ShowTEF(TipoTEF.Debito, _valor, $"{numCupomTEF}", DateTime.Now, idMetodo);
                            break;
                        case "11":
                            tefAtual.ShowTEF(TipoTEF.Debito, _valor, $"{numCupomTEF}", DateTime.Now, idMetodo);
                            break;
                    }
                    tefAtual.StatusChanged += Tef_StatusChanged;
                    this.IsEnabled = false;
                    return;
                }
            }

            if (strPgCfe == "05")
            {
                if (SENHA_PRAZO && !PedeSenhaGerencial("Necessária autorização de gerente")) return;
                if (/*intPagamentoDiasByIdPag > 0*/ true)
                {
                    var PC = new PerguntaCliente(idMetodo, false, _valor);
                    PC.ShowDialog();
                    switch (PC.DialogResult)
                    {
                        case true:
                            nome_cliente = PC.nome_cliente;
                            id_cliente = PC.id_cliente;
                            if (PC.vencimento != null)
                            {
                                vencimento = (DateTime)PC.vencimento;
                            }
                            else { vencimento = DateTime.Today.AddDays(30); }
                            log.Debug($"ID_CLIENTE: {id_cliente}, vencimento: {vencimento}");
                            break;
                        default:
                            MessageBox.Show("É necessário informar um cliente.");
                            return;
                    }
                }
            }

            if (strPgCfe == "17")
            {
                if (USATEF)
                {
                    tefAtual = new SiTEFBox();
                    tefAtual.ShowTEF(TipoTEF.Pix, _valor, $"{numCupomTEF}", DateTime.Now, idMetodo);
                    tefAtual.StatusChanged += Tef_StatusChanged;
                    this.IsEnabled = false;
                    return;
                }
            }
            AcrescentaMetodoPagamento(idMetodo, _valor, strPgCfe);
        }

        private void Tef_StatusChanged(object sender, TEFEventArgs e)
        {
            this.Dispatcher.Invoke(() => { this.IsEnabled = true; this.Focus(); });
            if (e.status == StatusTEF.Confirmado)
                try
                {
	                AcrescentaMetodoPagamento(idMetodo: e.idMetodo, _valor: e.Valor, strPgCfe: Metodos_DT.First(x => x.ID_FMANFCE == e.idMetodo).ID_NFCE, sitefbox: tefAtual);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
        }

        private void AcrescentaMetodoPagamento(int idMetodo, decimal _valor, string strPgCfe, SiTEFBox sitefbox = null)
        {
            valor_a_ser_pago = valor_a_ser_pago.Truncate(2);
            if (idMetodo != 1 && _valor > valor_a_ser_pago)
            {
                _valor = valor_a_ser_pago;
            }
            envCFeCFeInfCFePgtoMP pgto = new envCFeCFeInfCFePgtoMP()
            {
                cMP = strPgCfe.PadLeft(2, '0'),
                dec_vMP = (_valor),                
                vMP = (_valor).ToString("0.00"),
                desconto = false               
            };
            if (strPgCfe == "05")
            {
                pgto.idCliente = id_cliente;
                pgto.vencimento = vencimento;
            }
            if (sitefbox != null)
            {
                tefUsados.Add(sitefbox);
            }
            string strDescricaoMetodo = string.Empty;
            strDescricaoMetodo = (from linha in Metodos_DT
                                  where linha.ID_FMANFCE == idMetodo
                                  select linha.DESCRICAO).FirstOrDefault();

            nomes_pgtos.Add(strDescricaoMetodo);

            pagamentos[idMetodo] += (_valor - troco);
            metodosnew.Add((strPgCfe, _valor, Administradora.idAdm));
            metodos.Add(pgto);
            valores_pgtos.Add(_valor);

            if (_valor >= valor_a_ser_pago)
            {
                if (_valor == valor_a_ser_pago)
                {
                    troco = 0;
                }
                else if (_valor > valor_a_ser_pago)
                {
                    troco = _valor - valor_a_ser_pago;
                }
                PendenciasDoTEF pendTef = new PendenciasDoTEF();
                pendTef.LimpaPendencias(numCupomTEF);
                this.Dispatcher.Invoke(() => { DialogResult = true; Close(); return; });
            }
            else //Caso seja efetuado um pagamento parcial
            {
                valor_a_ser_pago -= _valor;
                valor_pago += _valor;
                this.Dispatcher.Invoke(() =>
                {
                    txb_SaldoRest.Value = valor_a_ser_pago;
                    txb_Pago.Value = valor_pago;
                    txb_Metodo.Clear();
                    txb_Valor.Clear();
                    txb_Metodo.Focus();
                    txb_Valor.Value = valor_a_ser_pago;
                });
            }
        }

        private void NovoFechamento_KeyDown(object sender, KeyEventArgs e)
        {
            #region Troca desativada
            if (e.Key == Key.F7)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    var pc = new PerguntaVale();
                    pc.ShowDialog();
                    if (pc.DialogResult == true)
                    {
                        using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                        using DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_DEVOLTableAdapter DEVOL_TA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_DEVOLTableAdapter();// { Connection = LOCAL_FB_CONN };
                        var devolucoesDisponiveis = DEVOL_TA.GetDataByID_DEVOLUCAO(pc.valeDigitado);
                        if (devolucoesDisponiveis.Count == 0)
                        {
                            DialogBox.Show("Finalização de Cupom", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Devolução informada não encontrada", "Ela pode já ter sido usada ou não existe");
                        }
                        decimal _valor = 0;
                        foreach (DataSets.FDBDataSetVenda.TRI_PDV_DEVOLRow item in devolucoesDisponiveis.Rows)
                        {
                            devolucoes_usadas.Add((pc.valeDigitado, item.VALOR));
                            _valor += item.VALOR;
                        }
                        if (valor_a_ser_pago == 0)
                        {
                            troco = 0;
                            DialogResult = true;
                            Close();
                        }//Finalizando a venda, caso não haja troco
                        else if (valor_pago > valor_a_ser_pago)
                        {
                            troco = valor_pago - valor_a_ser_pago;
                            DialogResult = true;
                            Close();
                        }//Finalizando a venda, caso haja troco
                        else
                        {
                            if (_valor > valor_a_ser_pago)
                            {
                                //_valor = valor_a_ser_pago;
                            }
                            var pgto = new envCFeCFeInfCFePgtoMP()
                            {
                                cMP = "99",
                                dec_vMP = _valor,
                                vMP = _valor.ToString("0.00"),
                                desconto = true
                            };
                            nomes_pgtos.Add("DEVOLUÇÃO");
                            valores_pgtos.Add(_valor);
                            pagamentos[100] += _valor;
                            metodos.Add(pgto);
                            metodosnew.Add(("99", _valor, Administradora.idAdm));
                            if (_valor >= valor_a_ser_pago)
                            {
                                if (_valor == valor_a_ser_pago)
                                {
                                    troco = 0;
                                    DialogResult = true;
                                    Close();
                                }
                                else if (_valor > valor_a_ser_pago)
                                {
                                    troco = valor_pago + _valor - valor_a_ser_pago;
                                    DialogResult = true;
                                    Close();
                                }
                            }
                            else //Caso seja efetuado um pagamento parcial
                            {
                                valor_a_ser_pago -= _valor;
                                valor_pago += _valor;
                                txb_SaldoRest.Value = valor_a_ser_pago;
                                txb_Pago.Value = valor_pago;
                                txb_Metodo.Clear();
                                txb_Valor.Clear();
                                txb_Metodo.Focus();
                                txb_Valor.Value = valor_a_ser_pago;
                            }
                        }//Recebe o pagamento
                    }
                });
            }
            #endregion
            if (e.Key == Key.F8)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    //if (valor_a_ser_pago != valor_venda)
                    //{
                    //    DialogResult refazer = System.Windows.Forms.MessageBox.Show("Só é possível aplicar desconto ao cupom antes de inserir um método de pagamento.\nDeseja cancelar o pagamento para tentar novamente?", "Erro", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    //    switch (refazer)
                    //    {
                    //        case System.Windows.Forms.DialogResult.Yes:
                    //            DialogResult = null;
                    //            this.Close();
                    //            return;
                    //        case System.Windows.Forms.DialogResult.No:
                    //            return;
                    //        default:
                    //            throw new Exception("Erro ao aplicar desconto.");
                    //    }
                    //}
                    if (_scannTech) { DialogBox.Show("Desconto", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Não é possivel aplicar descontos pois a venda possui produtos de promoção ScannTech."); return; }
                    if (_vendaAtual.DescontoAplicado() > 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Desconto total da venda removido com sucesso.", "Removendo desconto", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        _vendaAtual.LimpaAjuste();
                        desconto = 0;
                        valor_a_ser_pago = (_vendaAtual.ValorDaVenda().RoundABNT() - valor_pago);
                        stp_Desconto.Visibility = Visibility.Hidden;
                        txb_Desconto.Value = 0;
                        txb_SaldoRest.Value = valor_a_ser_pago;
                        txb_Valor.Value = valor_a_ser_pago;
                        txb_Metodo.Focus();
                        return;
                    }                    
                    var senha = new perguntaSenha("Aplicando Desconto na Venda");
                    senha.ShowDialog();
                    if (senha.DialogResult == false)
                    {
                        return;
                    }
                    else if (senha.DialogResult == true && senha.NivelAcesso == perguntaSenha.nivelDeAcesso.Gerente)
                    { AplicaDesconto(false); }
                    else if (senha.DialogResult == true && senha.NivelAcesso == perguntaSenha.nivelDeAcesso.Funcionario)
                    {
                        if (_desconto_maximo == 0)
                        {
                            DialogBox.Show("Senha incorreta",
                                           DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                           "A senha digitada não é uma senha de gerente.",
                                           "Impossível aplicar desconto.");
                            return;
                        }
                        AplicaDesconto(true);
                    }
                });
            }
            if (e.Key == Key.F4)
            {
                if (taxaAdicionada)
                {
                    _vendaAtual.RemoveProduto(_vendaAtual.nItemCupom - 1);
                    taxaAdicionada = false;
                    DialogBox.Show("Taxa de Serviço", DialogBoxButtons.Yes, DialogBoxIcons.None, false, "Taxa de serviço removida.");
                }
                else
                {
                    AcrTaxaServico taxaServico = new AcrTaxaServico();
                    if (taxaServico.ShowDialog() == true)
                    {
                        using (var obtemdadosdoitem = new DataSets.FDBDataSetOperSeedTableAdapters.SP_TRI_OBTEMDADOSDOITEMTableAdapter())
                        using (var dadosDoItem = new DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMDataTable())
                        using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                        {
                            if (COD10PORCENTO is -1 || COD10PORCENTO is 0)
                            {
                                MessageBox.Show("Código da Taxa de Serviço não configurada.\n\n     Configure corretamente e tente novamente.", "Informação", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            obtemdadosdoitem.Connection = LOCAL_FB_CONN;
                            obtemdadosdoitem.Fill(dadosDoItem, COD10PORCENTO);
                            DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMRow itemRow = (DataSets.FDBDataSetOperSeed.SP_TRI_OBTEMDADOSDOITEMRow)dadosDoItem.Rows[0];

                            _vendaAtual.RecebeNovoProduto(
                                COD10PORCENTO,
                                itemRow.DESCRICAO,
                                itemRow.IsCOD_NCMNull() ? "" : itemRow.COD_NCM,
                                itemRow.CFOP,
                                _vendaAtual.ValorDaVenda() * taxaServico.taxa,
                                itemRow.IsRSTR_CESTNull() ? "" : itemRow.RSTR_CEST,
                                0, 0, "UN", 1, itemRow.IsCOD_BARRANull() ? "" : itemRow.COD_BARRA
                                );
                            _vendaAtual.RecebePIS(
                                itemRow.IsRCST_PISNull() ? "" : itemRow.RCST_PIS,
                                _vendaAtual.ValorDaVenda() * taxaServico.taxa,
                                (itemRow.IsRPISNull()) ? 0M : itemRow.RPIS,
                                1
                                );
                            _vendaAtual.RecebeCOFINS(
                                itemRow.IsRCST_COFINSNull() ? "" : itemRow.RCST_COFINS,
                                _vendaAtual.ValorDaVenda() * taxaServico.taxa,
                                (itemRow.IsRCOFINSNull()) ? 0M : itemRow.RCOFINS,
                                1
                                ); _vendaAtual.RecebeInfoISSQN();
                            _vendaAtual.AdicionaProduto("101");
                        }
                        taxaAdicionada = true;
                    }
                }
                valor_a_ser_pago = _vendaAtual.ValorDaVenda().RoundABNT();
                txb_Valor.Value = 0;
                txb_Valor.Value = valor_a_ser_pago;
                txb_SaldoRest.Value = valor_a_ser_pago;
                txb_Pago.Value = 0;
            }
            if (e.Key == Key.F1 && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                //combobox.SelectedIndex = 13732;
                e.Handled = true;
                AlternarPainelDeAjuda();
            } // Abre um painel de ajuda (Tecla F1)
        }
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

        private void AplicaDesconto(bool restrito)
        {
            var pd = new Desconto(restrito, _desconto_maximo);
            pd.ShowDialog();

            if (pd.DialogResult == true)
            {
                switch (pd.absoluto)
                {
                    case true:
                        desconto = pd.reais;
                        break;
                    case false:
                        desconto = (pd.porcentagem) * _vendaAtual.ValorDaVenda().RoundABNT();
                        break;
                }               
                valor_a_ser_pago = (_vendaAtual.ValorDaVenda().RoundABNT() - desconto);
                txb_Desconto.Value = desconto;
                stp_Desconto.Visibility = Visibility.Visible;
                txb_SaldoRest.Value = valor_a_ser_pago;
                txb_Valor.Value = valor_a_ser_pago;
                txb_Metodo.Focus();
                _vendaAtual.RecebeAjuste(0, -desconto);
            }
            else if (pd.DialogResult == false)
            {
                //Em branco????
            }

        }

        #endregion Events

        #region Methods



        #endregion Methods

        private void Txb_Metodo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int.TryParse(txb_Metodo.Text, out int _result);
            if (_result == 3 && SYSPARCELA.ToBool())
            {
                stp_Parcelas.Visibility = Visibility.Visible;
            }
            else
            {
                stp_Parcelas.Visibility = Visibility.Hidden;
            }

        }

        private void Txb_Valor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            log.Debug($"txb_Valor.Value: {txb_Valor.Value}");
            log.Debug($"valor_a_ser_pago: {valor_a_ser_pago}");
            log.Debug("Bora arrendondar?");
            if (txb_Valor.Value.RoundABNT() > valor_a_ser_pago.RoundABNT())
            {
                stp_Troco.Visibility = Visibility.Visible;
                txb_Troco.Value = (-1 * (valor_a_ser_pago - txb_Valor.Value));
            }
            else if (txb_Valor.Value.RoundABNT() <= valor_a_ser_pago.RoundABNT())
            {
                stp_Troco.Visibility = Visibility.Hidden;
            }
        }

        private void txb_Valor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int.TryParse(txb_Metodo.Text, out int _result);
                switch (_result)
                {
                    case 3:
                    case 4:                    
                        if (INFORMA_MAQUININHA == true)
                        {
                            Administradora adm = new Administradora();
                            adm.ShowDialog();                            
                        }
                        break;
                }
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    //int.TryParse(txb_Metodo.Text, out int _result);
                if (_result == 3 && SYSPARCELA.ToBool())
                    {
                        txb_parcelas.Focus();
                        return;
                    }
                    ProcessarMetodoDePagamento();
                });
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                fechouManualmente = true;
                DialogResult = false;
                this.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {            
            if (taxaAdicionada && DialogResult != true) _vendaAtual.RemoveProduto(_vendaAtual.nItemCupom - 1);            

            if (fechouManualmente)
            {
                foreach(var itemComDesc in _vendaAtual._listaDets.Where(l => l.atacado is true || l.scannTech is true))
                {
                    itemComDesc.prod.vUnCom = itemComDesc.prod.vUnComOri ?? itemComDesc.prod.vUnCom;                    
                    itemComDesc.prod.vDesc = "0,00";
                    itemComDesc.atacado = false;                    
                }
            }
        }
        public void VerificaVlrTotal()
        {            
            if (_vlrTotalVenda > valor_a_ser_pago)
            {
                descNaVenda = _vlrTotalVenda - valor_a_ser_pago;                          
                //txt_Vlr.Text = descNaVenda.ToString("C");
                //txbDesc.Visibility = Visibility.Visible;
            }
        }
    }

}