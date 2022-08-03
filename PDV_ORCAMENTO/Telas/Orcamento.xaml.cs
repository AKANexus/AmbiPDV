using PDV_ORCAMENTO.FDBOrcaDataSetTableAdapters;
using PDV_WPF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static PDV_WPF.Extensoes;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Interaction logic for Orcamento.xaml
    /// </summary>
    public partial class Orcamento : Window
    {
        #region Fields & Properties

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IsIconic(IntPtr handle);

        private const int SW_RESTORE = 9;
        private int _id_cliente = -1;
        private bool _orcamento_inclusao = false;
        private bool _orcamento_alteracao = false;
        private int _id_orcamento;
        private string _status_orcamento = string.Empty;
        private decimal _desconto = 0; // pode ser em porcento ou absoluto
        //public decimal _desconto { get; set; }
        private int _num_produto = 0;
        private List<MainViewModel.ComboBoxBindingDTO> _dtCliente = new List<MainViewModel.ComboBoxBindingDTO>();

        private enum tipoDesconto { Nenhum, Absoluto, Percentual }

        private tipoDesconto _descontando;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        private double _desconto_maximo = 0;
        private Regex _rgx = new Regex(@"(\d+\*)");

        //private class OrcamentoItemDTO
        //{
        //    public int IdOrcamento { get; set; }
        //    public decimal Desconto { get; set; }
        //    public int CodProduto { get; set; }
        //    public decimal Quantidade { get; set; }
        //    public decimal Preco { get; set; }
        //    public int NumProduto { get; set; }
        //}
        //private List<OrcamentoItemDTO> _lstOrcamentoItens = new List<OrcamentoItemDTO>();

        #endregion Fields & Properties

        #region (De)Constructor

        public Orcamento()
        {
            InitializeComponent();

            //cbb_Cliente.SelectedValuePath = "ID_CLIENTE"; cbb_Cliente.DisplayMemberPath = "NOME";

            InicializarOrcamento();
        }

        ///// <summary>
        ///// Testa cada cliente e cada produto em gravações de orçamentos.
        ///// </summary>
        //private void TestarMassivamente()
        //{
        //    try
        //    {
        //        #region Variáveis

        //        bool blnTestarTodosProdutos = true;

        //        int toleranciaCadClienteInexistente = 0;
        //        int toleranciaCadProdutoInexistente = 0;

        //        var randomizador = new Random();

        //        int? idProduto = 0;

        //        int intQtdeItensPorOrcamento = 0;

        //        //int maxIdProduto = 0;

        //        decimal dcmPrecoUnitario = 0.1m;
        //        decimal dcmQuantidadeItem = 0.1m;

        //        int contProdutosGC = 0;
        //        int contClientesGC = 0;

        //        #endregion Variáveis

        //        #region TableAdapters e DataTables
        //        using (var taCliente = new SP_TRI_ORCA_CLIENTE_GETBY_IDTableAdapter())
        //        using (var tbCliente = new FDBOrcaDataSet.SP_TRI_ORCA_CLIENTE_GETBY_IDDataTable())
        //        using (var taProduto = new TB_EST_ESTOQUE_KEYVALUETableAdapter())
        //        using (var tbProduto = new FDBOrcaDataSet.TB_EST_ESTOQUE_KEYVALUEDataTable())
        //        #endregion TableAdapters e DataTables
        //        {
        //            #region Pesquisa produtos e retorna se não houver

        //            taProduto.Fill(tbProduto);
        //            if (tbProduto is null) { MessageBox.Show("Sem produto (null)!"); return; }
        //            if (tbProduto.Rows.Count <= 0) { MessageBox.Show("Sem produto!"); return; }

        //            //maxIdProduto = (from tab1 in tbProduto
        //            //                select tab1.ID_IDENTIFICADOR).Max();

        //            #endregion Pesquisa produtos e retorna se não houver

        //            #region Percorre por cada cliente (2000 no total)

        //            //  e verifica se o ID existe, baseado no índice do total.
        //            // Se não existir no banco, tenta mais 100 vezes.
        //            // Se encontrar um, as tentativas são zeradas.
        //            // Se não encontrar até lá, o teste é encerrado.

        //            for (int indexCliente = 0; indexCliente < 2000; indexCliente++)
        //            {
        //                #region Pesquisa clientes e retorna se não houver

        //                tbCliente.Clear();
        //                taCliente.Fill(tbCliente, indexCliente);

        //                if (tbCliente is null) { continue; }
        //                if (tbCliente.Rows.Count <= 0)
        //                {
        //                    if (toleranciaCadClienteInexistente >= 100) { break; }
        //                    toleranciaCadClienteInexistente++;
        //                    continue;
        //                }

        //                _id_cliente = indexCliente;

        //                toleranciaCadClienteInexistente = 0;

        //                #endregion Pesquisa clientes e retorna se não houver

        //                txbValorFrete.Value = Convert.ToDecimal(150 * randomizador.NextDouble());

        //                if (!ReservaNovoOrcamento()) { return; }

        //                #region Define a quantidade de itens no orçamento

        //                //if (indexCliente > 0)
        //                //{
        //                    blnTestarTodosProdutos = false;
        //                    intQtdeItensPorOrcamento = randomizador.Next(1, 50);
        //                //}
        //                //else
        //                //{
        //                //    blnTestarTodosProdutos = true;
        //                //    intQtdeItensPorOrcamento = tbProduto.Rows.Count;
        //                //}

        //                #endregion Define a quantidade de itens no orçamento

        //                for (int indexProduto = 0; indexProduto <= intQtdeItensPorOrcamento - 1; indexProduto++)
        //                {
        //                    #region Define o produto a ser adicionado no orçamento e retorna se necessário

        //                    int posicaoParaEncontrar = indexProduto;

        //                    if (!blnTestarTodosProdutos)
        //                    {
        //                        posicaoParaEncontrar = randomizador.Next(0, intQtdeItensPorOrcamento - 1);
        //                    }

        //                    //idProduto = (from tab1 in tbProduto
        //                    //             select tab1.ID_IDENTIFICADOR).First(t => t.Equals(idParaEncontrar));

        //                    idProduto = (tbProduto.ElementAt(posicaoParaEncontrar)).ID_IDENTIFICADOR;

        //                    if (idProduto is null)
        //                    {
        //                        if (toleranciaCadProdutoInexistente >= 100) { break; }
        //                        toleranciaCadProdutoInexistente++;
        //                        continue;
        //                    }
        //                    toleranciaCadProdutoInexistente = 0;

        //                    #endregion Define o produto a ser adicionado no orçamento e retorna se necessário

        //                    // Randomizar preço unit
        //                    dcmPrecoUnitario = Convert.ToDecimal(randomizador.Next(1, 100) * randomizador.NextDouble());
        //                    // Randomizar quantidade
        //                    dcmQuantidadeItem = Convert.ToDecimal(randomizador.Next(1, 25) * randomizador.NextDouble());

        //                    txbPrecoUnit.Text = dcmPrecoUnitario.ToString("n2");
        //                    txb_Qtde.Text = dcmQuantidadeItem.ToString("n2");

        //                    ProcessaItem(true, Convert.ToInt32(idProduto));

        //                    contProdutosGC++;

        //                    if (contProdutosGC >= 500)
        //                    {
        //                        GC.Collect();
        //                        contProdutosGC = 0;
        //                    }
        //                }

        //                //Definir txbValorFrete

        //                SalvaOrcamento(true);

        //                contClientesGC++;

        //                if (contClientesGC >= 200)
        //                {
        //                    GC.Collect();
        //                    contClientesGC = 0;
        //                }
        //            }

        //            #endregion Percorre por cada cliente (2000 no total)
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string strErrMess = "Erro no teste massivo! ";
        //        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
        //        MessageBox.Show(strErrMess);
        //        Application.Current.Shutdown();
        //        return;
        //    }
        //}

        #endregion

        #region Methods

        private void AplicarAutoScrollNoAutoCompleteBox(object sender)
        {
            var box = (FocusableAutoCompleteBox)sender;
            var innerListBox = (ListBox)box.Template.FindName("Selector", box);
            innerListBox.ScrollIntoView(innerListBox.SelectedItem);
        }

        private void InicializarOrcamento()
        {
            DataContext = new MainViewModel();

            audit("Inicializando orçamento...");

            //CarregarDatasourceClientes();

            //ACBox.PreviewKeyDown += new KeyEventHandler(ACBox_KeyDown);

            CarregarConfiguracoes();

            LimparTelaAtual();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True se não houver problema. Retorna true mesmo se não houver produto.</returns>
        private bool CarregarProdutos()
        {
            bool blnCarregouProdutosComSucesso = false;

            audit("Carregando produtos...");
            using (var dt = new FDBOrcaDataSet.TB_EST_ESTOQUE_KEYVALUEDataTable())
            using (var estoque_TA = new TB_EST_ESTOQUE_KEYVALUETableAdapter())
            {
                try
                {
                    //Ordena por nome, ascendente - 1.0.2.7 (27-11-2018 ~ Artur)
                    estoque_TA.FillOrderByDescricao(dt);

                    blnCarregouProdutosComSucesso = true;
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao carregar produtos. \nPor favor tente novamente. \nSe o erro persistir, entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    //Application.Current.Shutdown();
                    return false;
                }

                if (blnCarregouProdutosComSucesso)
                {
                    var lstKeyPairProdutos = new List<ComboBoxBindingDTO_Produto>();

                    foreach (DataRow row in dt)
                    {
                        lstKeyPairProdutos.Add(new ComboBoxBindingDTO_Produto()
                        {
                            ID_IDENTIFICADOR = (int)row["ID_IDENTIFICADOR"],
                            COD_BARRA = row["COD_BARRA"].ToString(),
                            DESCRICAO = row["DESCRICAO"].ToString(),
                            REFERENCIA = row["REFERENCIA"].ToString()
                        });
                    }

                    ACBox.ItemsSource = lstKeyPairProdutos;
                }
            }

            return blnCarregouProdutosComSucesso;
        }

        private void CarregarDatasourceClientes()
        {
            audit("Carregando clientes...");
            _dtCliente.Clear();
            using (var cLIENTE_KEYVALUETableAdapter = new SP_TRI_ORCA_CLIENTES_KEYVALUETableAdapter())
            using (var dt_cli_keyvalue = new FDBOrcaDataSet.SP_TRI_ORCA_CLIENTES_KEYVALUEDataTable())
            {
                try
                {
                    cLIENTE_KEYVALUETableAdapter.Fill(dt_cli_keyvalue);
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao carregar clientes. \nPor favor entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    Application.Current.Shutdown();
                    return;
                }

                foreach (DataRow row in dt_cli_keyvalue)
                {
                    _dtCliente.Add(new MainViewModel.ComboBoxBindingDTO() { ID = (int)row["ID_CLIENTE"], DESCRICAO = row["NOME"].ToString() });
                }
            }
        }

        /// <summary>
        /// Carrega as configurações da base de dados e as guarda na memória runtime
        /// </summary>
        private void CarregarConfiguracoes()
        {
            audit("Carregando configurações...");
            using (var ta_config = new TRI_ORCA_CONFIGTableAdapter())
            using (var dt_config = new FDBOrcaDataSet.TRI_ORCA_CONFIGDataTable())
            using (var ta_setup = new TRI_PDV_SETUPTableAdapter())
            using (var tb_setup = new FDBOrcaDataSet.TRI_PDV_SETUPDataTable())
            {
                var macAddr = string.Empty;

                try
                {
                    macAddr = GetCurrentMacAddress();
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao verificar configuração de rede. \nPor favor entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    Application.Current.Shutdown();
                    return;
                }

                try
                {
                    ta_config.FillByMacAddress(dt_config, macAddr);
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao carregar configurações (MAC address). \nPor favor entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    Application.Current.Shutdown();
                }

                if (dt_config.Rows.Count == 0)
                {
                    MessageBox.Show("Execute a configuração inicial.");
                    var config = new ParamsTecs(true, true);
                    config.ShowDialog();
                    Environment.Exit(0);
                    return;
                }

                try
                {
                    ta_setup.Fill(tb_setup);
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao carregar configurações (setup). \nPor favor entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    Application.Current.Shutdown();
                    return;
                }

                if (tb_setup.Rows.Count > 0)
                {
                    _desconto_maximo = (double)tb_setup.Rows[0]["DESC_MAX_OP"];
                }

                //Properties.Settings.Default.rodape = dt_config.Rows[0]["MENSAGEM_RODAPE"].Safestring();
                Properties.Settings.Default.no_caixa = Convert.ToInt32(dt_config.Rows[0]["NO_CAIXA"]);

                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();
            }
            //Local_OPER_TA.Connection.ConnectionString = Local_CTAREC_TA.Connection.ConnectionString = Properties.Settings.Default.ContingencyDB;
        }

        /// <summary>
        /// Processa o que for digitado na ACBox
        /// </summary>
        private void ProcessaItem(bool pAdicionarItemNoOrcamento, int pIdProduto = 0)
        {
            #region Valida seleção de produto

            if (pIdProduto == 0)
            {
                if (ACBox.SelectedItem == null)
                {
                    if (int.TryParse(ACBox.Text, out int tentativa_conversao_cod))
                    {
                        ACBox.SelectedItem = ((IEnumerable<ComboBoxBindingDTO_Produto>)ACBox.ItemsSource).First(item => item.COD_BARRA.Equals(ACBox.Text) ||
                                                                                                                item.ID_IDENTIFICADOR.Equals(tentativa_conversao_cod));
                    }
                    else
                    {
                        ACBox.SelectedItem = ((IEnumerable<ComboBoxBindingDTO_Produto>)ACBox.ItemsSource).First(item => item.COD_BARRA.Equals(ACBox.Text));
                    }

                    if (ACBox.SelectedItem == null)
                    {
                        ACBox.Text = string.Empty;
                        audit("Produto não foi encontrado. Método: " + MethodBase.GetCurrentMethod().Name);
                        return;
                    }
                }
            }

            #endregion Valida seleção de produto

            if (_orcamento_inclusao || _orcamento_alteracao)
            {
                int? cod_produto = 0;

                if (pIdProduto == 0)
                {
                    cod_produto = ((ComboBoxBindingDTO_Produto)ACBox.SelectedItem).ID_IDENTIFICADOR;
                }
                else
                {
                    cod_produto = pIdProduto;
                }

                decimal preco = 0m;

                using (var PDV_Oper = new TRI_ORCA_PRODUTOSTableAdapter())
                {
                    if (pAdicionarItemNoOrcamento)
                    {
                        decimal.TryParse(txbPrecoUnit.Text, out preco);

                        if (preco <= 0m)
                        {
                            MessageBox.Show("O preço unitário não deve ser menor ou igual a zero.");
                            return;
                        }

                        decimal quantidade = 1;
                        if (txb_Qtde.Text != string.Empty)
                        {
                            quantidade = Convert.ToDecimal(txb_Qtde.Text);
                        }

                        _num_produto++;

                        decimal dcmProdValorDesconto = 0M;
                        decimal dcmProdValorSubTotal = 0M;
                        decimal dcmProdValorTotal = 0M;

                        dcmProdValorSubTotal = preco * quantidade;

                        switch (_descontando)
                        {
                            case tipoDesconto.Absoluto:
                                dcmProdValorDesconto = _desconto;
                                break;
                            case tipoDesconto.Percentual:
                                dcmProdValorDesconto = dcmProdValorSubTotal * _desconto;
                                break;
                        }

                        if (_descontando != tipoDesconto.Nenhum) { _descontando = tipoDesconto.Nenhum; } // Desativa desconto

                        dcmProdValorTotal = dcmProdValorSubTotal - dcmProdValorDesconto;


                        try
                        {
                            PDV_Oper.SP_TRI_ORCA_LANCAITEM(_id_orcamento,
                                                           dcmProdValorDesconto,
                                                           cod_produto,
                                                           quantidade,
                                                           preco,
                                                           _num_produto,
                                                           dcmProdValorTotal);
                        }
                        catch (Exception ex)
                        {
                            string strErrMess = "Erro ao adicionar item no orçamento. \nPor favor verifique os dados e tente novamente.";
                            gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show(strErrMess);
                            return;
                        }

                        try
                        {
                            CarregarOrcamentoItens(true, _id_orcamento);
                        }
                        catch (Exception ex)
                        {
                            // Pode dar erro de conexão... neste ponto, o app deve ser fechado.
                            gravarMensagemErro("Erro no CarregarOrcamentoItens(...) em ProcessaItem(...). O app deve ser fechado.\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show("Erro ao consultar itens do orçamento. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                            Application.Current.Shutdown(); // deuruim();
                            return;
                        }

                        GravarTotaisOrca();

                        ACBox.Text = string.Empty;
                        ACBox.SelectedItem = null;
                        txb_Qtde.Clear();
                        txbPrecoUnit.Clear();

                        if (pIdProduto <= 0)
                        {
                            iTENS_ORCAMENTODataGrid.UpdateLayout();
                            iTENS_ORCAMENTODataGrid.ScrollIntoView(iTENS_ORCAMENTODataGrid.Items[iTENS_ORCAMENTODataGrid.Items.Count - 1]); //HACK: gambi pra deixar o último item sempre visível, caso contrário, só a primeira página será visível enquanto o usuário for adicionando itens.
                        }

                        ACBox.Focus();
                    }
                    else
                    {
                        try
                        {
                            preco = (decimal)PDV_Oper.SP_TRI_PEGAPRECO(cod_produto, 1); // SP_TRI_PEGAPRECO é executada para buscar o preço de atacado ou preço de venda comum. É usada a quantidade pra buscar um ou outro preço. Mas aqui a quantidade é constante... MAS deve permanecer assim, para pegar o preço mais atual.
                        }
                        catch (Exception ex)
                        {
                            string strErrMess = "Erro ao consultar preço do produto. \nPor favor verifique os dados e tente novamente.";
                            gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show(strErrMess);
                            return;
                        }

                        txbPrecoUnit.Text = preco.ToString("n2");
                    }
                }
            }
        }

        private void CarregarOrcamentoItens(bool pCalcularTotais, int pIdOrcamento)
        {
            using (var FDBOrcaDataSet = ((FDBOrcaDataSet)(this.FindResource("fDBOrcaDataSet"))))
            using (var FDBOrcaDataSetITENS_ORCAMENTO_TA = new SP_TRI_ORCA_ITENS_ORCAMENTOTableAdapter())
            {
                FDBOrcaDataSetITENS_ORCAMENTO_TA.Fill(FDBOrcaDataSet.SP_TRI_ORCA_ITENS_ORCAMENTO, pIdOrcamento);
                //var iTENS_ORCAMENTOViewSource = ((CollectionViewSource)(this.FindResource("iTENS_ORCAMENTOViewSource"))); //TODO: se der ruim, descomentar
                if (pCalcularTotais)
                {
                    CalcularExibirTotais(FDBOrcaDataSet.SP_TRI_ORCA_ITENS_ORCAMENTO);
                }

                _num_produto = FDBOrcaDataSet.SP_TRI_ORCA_ITENS_ORCAMENTO.Rows.Count;
            }
        }

        private void CarregarOrcamento()
        {
            bool blnCarregouOrcamentoComSucesso = false;

            try
            {
                //biOrca.IsBusy = true;

                if (string.IsNullOrWhiteSpace(txb_Orcamento.Text)) { return; }

                _id_orcamento = txb_Orcamento.Text.Safeint();

                using (var taOrcaOrcamento = new TRI_ORCA_ORCAMENTOSTableAdapter())
                using (var tbOrcaOrcamento = new FDBOrcaDataSet.TRI_ORCA_ORCAMENTOSDataTable())
                using (var taOrcaCliente = new SP_TRI_ORCA_CLIENTE_GETBY_IDTableAdapter())
                using (var tbOrcaCliente = new FDBOrcaDataSet.SP_TRI_ORCA_CLIENTE_GETBY_IDDataTable())
                {
                    int intIdCliente = 0;

                    try
                    {
                        taOrcaOrcamento.Fill(tbOrcaOrcamento, _id_orcamento);
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao consultar orçamento. \nPor favor tente novamente.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        return;
                    }

                    if (tbOrcaOrcamento == null) { return; }
                    if (tbOrcaOrcamento.Rows.Count <= 0) { MessageBox.Show("Orçamento não encontrado!"); return; }

                    intIdCliente = tbOrcaOrcamento[0]["ID_CLIENTE"].Safeint();

                    try
                    {
                        taOrcaCliente.Fill(tbOrcaCliente, intIdCliente);
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao consultar cliente do orçamento. \nPor favor tente novamente.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        return;
                    }

                    if (tbOrcaCliente == null) { return; }
                    if (tbOrcaCliente.Rows.Count <= 0) { MessageBox.Show("Cliente não encontrado!"); return; }

                    #region Exibir dados do orçamento

                    #region Cliente
                    if (tbOrcaCliente[0].IsCLIENTE_CNPJNull())
                    {
                        txbClienteCpfCnpj.Text = tbOrcaCliente[0]["CLIENTE_CPF"].Safestring();
                    }
                    else
                    {
                        txbClienteCpfCnpj.Text = tbOrcaCliente[0]["CLIENTE_CNPJ"].Safestring();
                    }

                    //cbb_Cliente.SelectedValue = intIdCliente;
                    acbCliente.SelectedItem = ((IEnumerable<MainViewModel.ComboBoxBindingDTO>)acbCliente.ItemsSource).First(item => item.ID.Equals(intIdCliente));

                    txbNomeSolicitante.Text = tbOrcaOrcamento[0]["NOME_SOLICITANTE"].Safestring();
                    txb_TelefoneFixo.Value = tbOrcaCliente.Rows[0]["DDD_RESID"].Safestring() + tbOrcaCliente.Rows[0]["FONE_RESID"].Safestring();
                    txb_TelefoneCelular.Value = tbOrcaCliente.Rows[0]["DDD_CELUL"].Safestring() + tbOrcaCliente.Rows[0]["FONE_CELUL"].Safestring();
                    //txb_Telefone3.Value = CLIENTE_DT.Rows[0]["DDD_CELUL"].Safestring() + CLIENTE_DT.Rows[0]["FONE_CELUL"].Safestring();
                    #endregion Cliente

                    #region Frete
                    txb_Transportadora.SelectedValue = tbOrcaOrcamento[0]["ID_FORNEC_TRANSP"].Safeint();
                    //TODO: ESTE ABSURDO DEVE-SE ÚNICA E EXCLUSIVAMENTE A ESSE CONTROLE QUE RESOLVE NÃO EXIBIR O VALOR BUSCADO DO ORÇAMENTO DEPOIS DE UM CLEAR() >>>>>>>>>>>
                    txbValorFrete.Text = "0,00";
                    txbValorFrete.Value = 0.00m;
                    // <<<<<<<<<<<<<
                    txbValorFrete.Value = tbOrcaOrcamento[0].IsVALOR_FRETENull() ? 0m : (decimal?)tbOrcaOrcamento[0]["VALOR_FRETE"];
                    #endregion Frete

                    #region Detalhes
                    txb_Abertura.SelectedDate = tbOrcaOrcamento[0].IsDT_EMISSAONull() ? null : (DateTime?)tbOrcaOrcamento[0]["DT_EMISSAO"];
                    txb_Validade.SelectedDate = tbOrcaOrcamento[0].IsDT_VALIDADENull() ? null : (DateTime?)tbOrcaOrcamento[0]["DT_VALIDADE"];
                    txb_Entrega.SelectedDate = tbOrcaOrcamento[0].IsDT_ENTREGANull() ? null : (DateTime?)tbOrcaOrcamento[0]["DT_ENTREGA"];
                    txbSubtotal.Text = "0,00";
                    txbSubtotal.Value = 0.00m;
                    txbSubtotal.Value = tbOrcaOrcamento[0].IsSUBTOTALNull() ? 0m : (decimal?)tbOrcaOrcamento[0]["SUBTOTAL"];
                    txbDesconto.Text = "0,00";
                    txbDesconto.Value = 0.00m;
                    txbDesconto.Value = tbOrcaOrcamento[0].IsDESCONTO_TOTALNull() ? 0m : (decimal?)tbOrcaOrcamento[0]["DESCONTO_TOTAL"];
                    txb_Parcelamento.SelectedValue = tbOrcaOrcamento[0].IsID_PARCELANull() ? 1 : (short)tbOrcaOrcamento[0]["ID_PARCELA"];
                    txbTotal.Text = "0,00";
                    txbTotal.Value = 0.00m;
                    txbTotal.Value = tbOrcaOrcamento[0].IsVALOR_TOTALNull() ? 0m : (decimal?)tbOrcaOrcamento[0]["VALOR_TOTAL"];
                    txb_Vencimento.SelectedDate = tbOrcaOrcamento[0].IsDT_VENCIMENTONull() ? null : (DateTime?)tbOrcaOrcamento[0]["DT_VENCIMENTO"];
                    txbObservacoes.Text = tbOrcaOrcamento[0].IsOBSERVACOESNull() ? string.Empty : tbOrcaOrcamento[0]["OBSERVACOES"].ToString();

                    _status_orcamento = tbOrcaOrcamento[0].IsSTATUSNull() ? string.Empty : tbOrcaOrcamento[0].STATUS.Safestring();


                    #endregion Detalhes

                    #endregion Exibir dados do orçamento

                }

                CarregarOrcamentoItens(false/* o ideal seria carregar os totais com a capa como origem (como está acima), mas se estiver zerado, deve recalcular a partir dos itens do orçamento */, _id_orcamento);

                blnCarregouOrcamentoComSucesso = true;
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao carregar orçamento. \n\nPor favor, tente novamente. \n\nSe o problema persistir, entre em contato com a equipe de suporte.";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
            finally
            {
                _orcamento_alteracao = blnCarregouOrcamentoComSucesso;
                txb_Orcamento.IsEnabled = !blnCarregouOrcamentoComSucesso;

                if (blnCarregouOrcamentoComSucesso)
                {
                    GravarOrcamentoPendente();
                    SetarHabilitacaoFormulario(_status_orcamento);
                }

                //biOrca.IsBusy = false;
            }
        }

        /// <summary>
        /// Deixa os campos do formulário disponíveis ou não, dependendo do status do orçamento carregado.
        /// Se o status for "FECHADO", exibe uma mensagem.
        /// </summary>
        /// <param name="status_orcamento"></param>
        private void SetarHabilitacaoFormulario(string status_orcamento)
        {
            bool blnNaoTravarTela = true;
            bool blnPermitirReimpressao = false;

            switch (status_orcamento)
            {
                case "FECHADO":
                    blnNaoTravarTela = false;
                    blnPermitirReimpressao = true;
                    (new DialogBox("Orçamento finalizado", "\nEsse orçamento já foi usado em venda. \nNão pode ser editado.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Info)).ShowDialog();
                    break;
                case "SALVO":
                    blnNaoTravarTela = true;
                    break;
                case "EDITANDO":
                    blnNaoTravarTela = true;
                    break;
                case "":
                    blnNaoTravarTela = false;
                    break;
                case null:
                    blnNaoTravarTela = false;
                    break;
                default:
                    throw new NotImplementedException("Status de orçamento não esperado em SetarHabilitacaoFormulario(): " + status_orcamento);
            }

            //gbxCliente.IsEnabled = blnNaoTravarTela;
            gbxClienteNome.IsEnabled = blnNaoTravarTela;
            gbxClienteCpfCnpj.IsEnabled = blnNaoTravarTela;
            grdClienteLinha2.IsEnabled = blnNaoTravarTela;

            gbxFrete.IsEnabled = blnNaoTravarTela;
            gbxDetalhes.IsEnabled = blnNaoTravarTela;
            grdOrcaProdFind.IsEnabled = blnNaoTravarTela;
            iTENS_ORCAMENTODataGrid.IsEnabled = blnNaoTravarTela;

            but_Salvar.Visibility = blnNaoTravarTela ? Visibility.Visible : Visibility.Hidden;
            if (blnPermitirReimpressao) { but_Salvar.Visibility = Visibility.Visible; }

        }

        private void CarregaInfoCliente()
        {
            if (acbCliente.SelectedItem == null) { return; }

            //using (var CIDADE_DT = new FDBOrcaDataSet.TB_CIDADE_SISDataTable())
            //using (var CIDADE_TA = new FDBOrcaDataSetTableAdapters.TB_CIDADE_SISTableAdapter())
            using (var CLIENTE_DT = new FDBOrcaDataSet.SP_TRI_ORCA_CLIENTE_GETBY_IDDataTable())
            using (var CLIENTE_TA = new SP_TRI_ORCA_CLIENTE_GETBY_IDTableAdapter())
            {
                try
                {
                    CLIENTE_TA.Fill(CLIENTE_DT, ((MainViewModel.ComboBoxBindingDTO)acbCliente.SelectedItem).ID);
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao consultar informações do cliente. \nPor favor tente novamente.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    return;
                }

                if (CLIENTE_DT[0].IsCLIENTE_CNPJNull())
                {
                    txbClienteCpfCnpj.Text = CLIENTE_DT[0]["CLIENTE_CPF"].Safestring();
                }
                else
                {
                    txbClienteCpfCnpj.Text = CLIENTE_DT[0]["CLIENTE_CNPJ"].Safestring();
                }
                txb_TelefoneFixo.Value = CLIENTE_DT.Rows[0]["DDD_RESID"].Safestring() + CLIENTE_DT.Rows[0]["FONE_RESID"].Safestring();
                txb_TelefoneCelular.Value = CLIENTE_DT.Rows[0]["DDD_CELUL"].Safestring() + CLIENTE_DT.Rows[0]["FONE_CELUL"].Safestring();
            }
        }

        private bool ReservaNovoOrcamento()
        {
            /// SEM CONSIDERAR ORÇAMENTO PENDENTE!
            bool blnReservouOrcamentoComSucesso = false;

            if (!CarregarProdutos()) { return false; }

            if (!CarregarOrcamentoPendente())
            {

                try
                {
                    using (var ORCA_TA = new TRI_ORCA_ORCAMENTOSTableAdapter())
                    {
                        short userid = Convert.ToInt16(operador.ID);


                        try
                        {
                            _id_orcamento = (int)ORCA_TA.SP_TRI_ORCA_RESERV(userid, Properties.Settings.Default.no_caixa);
                        }
                        catch (Exception ex)
                        {
                            string strErrMess = "Erro ao reservar orçamento. \nPor favor entre em contato com a equipe de suporte.";
                            gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show(strErrMess);
                            Application.Current.Shutdown();
                            return false;
                        }


                        txb_Orcamento.Text = _id_orcamento.Safestring();
                        lbl_novo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9D9D9D"));
                        txb_Abertura.SelectedDate = DateTime.Today;
                    }

                    #region Verificar se o orçamento reservado já tem produtos

                    CarregarOrcamentoItens(true, _id_orcamento);

                    #endregion Verificar se o orçamento reservado já tem produtos

                    blnReservouOrcamentoComSucesso = true;
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao reservar orçamento. \n\nPor favor tente novamente. \n\nSe o problema persistir, entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                }
                finally
                {
                    _orcamento_inclusao = blnReservouOrcamentoComSucesso;
                    _orcamento_alteracao = false;
                    txb_Orcamento.IsEnabled = !blnReservouOrcamentoComSucesso;

                    if (blnReservouOrcamentoComSucesso)
                    {
                        GravarOrcamentoPendente();
                        _status_orcamento = "EDITANDO";
                        SetarHabilitacaoFormulario(_status_orcamento);
                    }
                }
            }

            return blnReservouOrcamentoComSucesso;
        }

        private void GravarOrcamentoPendente()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\UltimoOrcamento.txt";
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("Arquivo gerado automaticamente pelo Ambi Orçamento. Não remova." + Environment.NewLine);
                }
            }
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(_id_orcamento.ToString());
            }
        }

        private void SalvaOrcamento(bool pBlnReimpressao = false/*bool pBlnEmTesteMassa = false*/)
        {
            if (!ValidarOrcamento()) { return; }

            #region Orçamento

            if (!pBlnReimpressao)
            {
                try
                {
                    using (var ORCA_TA = new TRI_ORCA_ORCAMENTOSTableAdapter())
                    {
                        //CLIENTE_TA.FillById(CLIENTE_DT, ((MainViewModel.ComboBoxBindingDTO)cbb_Cliente.SelectedItem).ID);
                        //if (!pBlnEmTesteMassa)
                        //{
                        _id_cliente = ((MainViewModel.ComboBoxBindingDTO)acbCliente.SelectedItem).ID;//CLIENTE_DT.Rows[0]["ID_CLIENTE"].Safeint();
                                                                                                     //}

                        int? idTransp = txb_Transportadora.SelectedValue == null ? null : (int?)Convert.ToInt32(txb_Transportadora.SelectedValue);
                        short? idParcel = txb_Parcelamento.SelectedValue == null ? null : (short?)Convert.ToInt16(txb_Parcelamento.SelectedValue);

                        ORCA_TA.SP_TRI_ORCA_SALVAORCA(_id_orcamento,
                                                      _id_cliente,
                                                      idTransp,
                                                      txb_Validade.SelectedDate,
                                                      idParcel,
                                                      txbSubtotal.Value,
                                                      txbDesconto.Value,
                                                      txbTotal.Value,
                                                      txb_Entrega.SelectedDate,
                                                      txb_Vencimento.SelectedDate,
                                                      txbValorFrete.Value,
                                                      txbNomeSolicitante.Text,
                                                      txbObservacoes.Text);
                    }
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao gravar orçamento. \nPor favor verifique os dados e tente novamente.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    return;
                }
            }

            #endregion Orçamento

            #region Impressão

            //if (!pBlnEmTesteMassa)
            //{
            //TODO: Verificar se o cliente quer imprimir em A4 ou em cupom
            switch ((TipoImpressora)Properties.Settings.Default.TipoImpressora)
            {
                case TipoImpressora.officeA4:

                    #region Prepara para A4

                    try
                    {
                        #region Preencher parâmetros

                        var lstParams = new List<string> { _id_orcamento.ToString() };

                        #endregion Preencher parâmetros

                        (new ReportExibicao("PDV_ORCAMENTO.Reports.rptOrcamento.rdlc", lstParams)).ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao imprimir orçamento (A4). \nO orçamento atual foi salvo, mas houve um problema antes de imprimir. \nPor favor entre em contato com a equipe de suporte.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        return;
                    }

                    #endregion Prepara para A4

                    break;
                case TipoImpressora.thermal80:

                    try
                    {

                        #region Prepara para impressora térmica até 80mm

                        var FDBOrcaDataSet = ((FDBOrcaDataSet)(this.FindResource("fDBOrcaDataSet")));
                        {
                            foreach (var item in FDBOrcaDataSet.SP_TRI_ORCA_ITENS_ORCAMENTO)
                            {
                                PrintORCA.RecebeProduto(item.ID_EST_IDENTIFICADOR.ToString(),
                                                        item.DESCRICAO.ToString(),
                                                        item.UNI_MEDIDA.ToString(),
                                                        item.QUANT,
                                                        item.VALOR,
                                                        item.DESCONTO,
                                                        item.VALOR_TOTAL,
                                                        0,
                                                        0);
                            }
                        }

                        using (var EMITENTE_DT = new FDBOrcaDataSet.SP_TRI_REL_ORCA_EMITDataTable())
                        using (var EMITENTE_TA = new SP_TRI_REL_ORCA_EMITTableAdapter())
                        using (var CLIENTE_DT = new DataSetes.FDBDataSetReports.SP_TRI_REL_ORCA_SOLICITANTEDataTable())
                        using (var CLIENTE_TA = new DataSetes.FDBDataSetReportsTableAdapters.SP_TRI_REL_ORCA_SOLICITANTETableAdapter())
                        //using (var CLIENTE_TA = new FDBOrcaDataSetTableAdapters.TB_CLIENTETableAdapter())
                        {
                            try
                            {
                                EMITENTE_TA.Fill(EMITENTE_DT);
                            }
                            catch (Exception ex)
                            {
                                string strErrMess = "Erro ao consultar dados do emitente. \nO orçamento atual foi salvo, mas houve um problema antes de imprimir. \nPor favor entre em contato com a equipe de suporte.";
                                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                                MessageBox.Show(strErrMess);
                                return;
                            }

                            try
                            {
                                CLIENTE_TA.Fill(CLIENTE_DT, _id_orcamento);
                            }
                            catch (Exception ex)
                            {
                                string strErrMess = "Erro ao consultar dados do cliente. \nO orçamento atual foi salvo, mas houve um problema antes de imprimir. \nPor favor entre em contato com a equipe de suporte.";
                                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                                MessageBox.Show(strErrMess);
                                return;
                            }

                            PrintORCA.nomefantasia = EMITENTE_DT[0].NOME_FANTA;
                            PrintORCA.nomedaempresa = EMITENTE_DT[0].NOME;
                            PrintORCA.enderecodaempresa = string.Format("{0} {1}, {2} - {3}, {4}", EMITENTE_DT[0].END_TIPO,
                                                                                                   EMITENTE_DT[0].END_LOGRAD,
                                                                                                   EMITENTE_DT[0].END_NUMERO,
                                                                                                   EMITENTE_DT[0].END_BAIRRO,
                                                                                                   EMITENTE_DT[0].CIDADE_NOME);
                            PrintORCA.cnpjempresa = EMITENTE_DT[0].CNPJ;
                            PrintORCA.numerodoextrato = _id_orcamento;

                            PrintORCA.emissao = txb_Abertura.SelectedDate;
                            PrintORCA.validade = txb_Validade.SelectedDate;
                            PrintORCA.prevEntrega = txb_Entrega.SelectedDate;

                            PrintORCA.nomecliente = ((MainViewModel.ComboBoxBindingDTO)acbCliente.SelectedItem).DESCRICAO.ToString();

                            PrintORCA.transportadora = (txb_Transportadora.SelectedItem is null) ? "" : ((MainViewModel.ComboBoxBindingDTO)txb_Transportadora.SelectedItem).DESCRICAO.ToString();

                            PrintORCA.nomeSolicitante = txbNomeSolicitante.Text; //TODO: validar?

                            PrintORCA.endereco = (CLIENTE_DT[0].IsEND_TIPONull() ? "" : CLIENTE_DT[0].END_TIPO + " ") +
                                                 (CLIENTE_DT[0].IsEND_LOGRADNull() ? "" : CLIENTE_DT[0].END_LOGRAD + ",") +
                                                 (CLIENTE_DT[0].IsEND_NUMERONull() ? "" : CLIENTE_DT[0].END_NUMERO + "-") +
                                                 (CLIENTE_DT[0].IsEND_COMPLENull() ? "" : CLIENTE_DT[0].END_COMPLE);
                            PrintORCA.cep = CLIENTE_DT[0].IsEND_CEPNull() ? "" : CLIENTE_DT[0].END_CEP;
                            PrintORCA.cidadeUf = (CLIENTE_DT[0].IsCIDADE_NOMENull() ? "" : CLIENTE_DT[0].CIDADE_NOME + "/") +
                                                 (CLIENTE_DT[0].IsSIGLA_UFNull() ? "" : CLIENTE_DT[0].SIGLA_UF);
                            PrintORCA.telComer = (CLIENTE_DT[0].IsDDD_COMERNull() ? "" : "(" + CLIENTE_DT[0].DDD_COMER + ")") +
                                                 (CLIENTE_DT[0].IsFONE_COMERNull() ? "" : CLIENTE_DT[0].FONE_COMER);
                            PrintORCA.telCelul = (CLIENTE_DT[0].IsDDD_CELULNull() ? "" : "(" + CLIENTE_DT[0].DDD_CELUL + ")") +
                                                 (CLIENTE_DT[0].IsFONE_CELULNull() ? "" : CLIENTE_DT[0].FONE_CELUL);
                            PrintORCA.telFax = (CLIENTE_DT[0].IsDDD_FAXNull() ? "" : "(" + CLIENTE_DT[0].DDD_FAX + ")") +
                                               (CLIENTE_DT[0].IsFONE_FAXNull() ? "" : CLIENTE_DT[0].FONE_FAX);
                            PrintORCA.telResid = (CLIENTE_DT[0].IsDDD_RESIDNull() ? "" : "(" + CLIENTE_DT[0].DDD_RESID + ")") +
                                                 (CLIENTE_DT[0].IsFONE_RESIDNull() ? "" : CLIENTE_DT[0].FONE_RESID);
                            PrintORCA.email = CLIENTE_DT[0].IsEMAIL_CONTNull() ? "" : CLIENTE_DT[0].EMAIL_CONT;

                            PrintORCA.desconto = Convert.ToDecimal(txbDesconto.Value);
                            PrintORCA.frete = Convert.ToDecimal(txbValorFrete.Value);

                            PrintORCA.valorTotal = Convert.ToDecimal(txbTotal.Value);

                            PrintORCA.observacoes = txbObservacoes.Text; //TODO: validar?

                            if (txb_Parcelamento.SelectedItem != null)
                            {
                                PrintORCA.RecebePagamento(((MainViewModel.ComboBoxBindingDTO)txb_Parcelamento.SelectedItem).DESCRICAO, Convert.ToDecimal(txbTotal.Value));
                            }

                            PrintORCA.IMPRIME(true, 0, 0, 0, 0, 0, TipoImpressora.thermal80);
                        }

                        #endregion Prepara para impressora térmica até 80mm

                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao imprimir orçamento (impressora térmica). \nO orçamento atual foi salvo, mas houve um problema antes de imprimir. \nPor favor entre em contato com a equipe de suporte.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        return;
                    }

                    break;
                case TipoImpressora.nenhuma:
                    break;
                default:
                    string mensagem = "Tipo de impressora não esperado: " + ((TipoImpressora)Properties.Settings.Default.TipoImpressora).ToString();
                    gravarMensagemErro(mensagem);
                    throw new NotImplementedException(mensagem);
            }

            //}

            #endregion Impressão

            LimparUltimoOrcamento();
            LimparTelaAtual();
        }

        private bool ValidarOrcamento()
        {
            if (txbTotal.Value <= 0) { return false; }
            return true;
        }

        private void CancelaOrcamentoNaTela()
        {
            LimparTelaAtual();
            LimparUltimoOrcamento();
        }

        private void LimparTelaAtual()
        {
            try
            {
                #region Cliente
                txbClienteCpfCnpj.Clear();
                //acbCliente.SelectedIndex = -1;
                acbCliente.Text = string.Empty;
                acbCliente.SelectedItem = null;
                txb_Orcamento.Clear();
                txbNomeSolicitante.Clear();
                txb_TelefoneFixo.Clear(); txb_TelefoneCelular.Clear();
                #endregion Cliente

                #region Frete
                txb_Transportadora.SelectedIndex = -1;
                txb_Transportadora.SelectedItem = null;
                txbValorFrete.Value = null;
                txbValorFrete.Clear();
                #endregion Frete

                #region Detalhes
                txb_Abertura.SelectedDate = txb_Validade.SelectedDate = txb_Entrega.SelectedDate = null;
                txbSubtotal.Value = null;
                txbSubtotal.Clear();
                txbDesconto.Clear();
                txb_Parcelamento.SelectedIndex = -1;
                txb_Parcelamento.SelectedItem = null;
                txbTotal.Value = null;
                txbTotal.Clear();
                txb_Vencimento.SelectedDate = null;
                txbObservacoes.Clear();
                #endregion Detalhes

                #region Produtos
                //ITENS_ORCAMENTO.Clear();
                var FDBOrcaDataSet = ((FDBOrcaDataSet)(this.FindResource("fDBOrcaDataSet")));
                //var iTENS_ORCAMENTOViewSource = ((CollectionViewSource)(this.FindResource("iTENS_ORCAMENTOViewSource")));
                FDBOrcaDataSet?.SP_TRI_ORCA_ITENS_ORCAMENTO?.Clear();
                FDBOrcaDataSet?.Dispose();
                iTENS_ORCAMENTODataGrid.UpdateLayout();

                txb_Qtde.Clear();
                ACBox.Text = string.Empty;
                ACBox.SelectedItem = null;
                #endregion Produtos

                #region Globais (orçamento)
                lbl_novo.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5DDCC7"));

                _id_cliente = -1;
                _id_orcamento = 0;
                _num_produto = 0;

                _orcamento_inclusao = false;
                _orcamento_alteracao = false;

                txb_Orcamento.IsEnabled = true;

                //_lstOrcamentoItens.Clear();
                #endregion Globais (orçamento)

                SetarHabilitacaoFormulario("");
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao limpar a tela! Por favor verifique o arquivo de log.";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
        }

        /// <summary>
        /// Faz o cálculo e exibição dos totais dos produtos (subtotal, desconto e total considerando frete)
        /// </summary>
        /// <param name="iTENS_ORCAMENTO"></param>
        private void CalcularExibirTotais(FDBOrcaDataSet.SP_TRI_ORCA_ITENS_ORCAMENTODataTable iTENS_ORCAMENTO)
        {
            try
            {
                //if (iTENS_ORCAMENTO is null) { return; }

                decimal dcmSubtotal = 0.00m;
                decimal dcmDesconto = 0.00m;
                decimal dcmFrete = 0.00m;
                decimal dcmTotal = 0.00m;

                dcmFrete = (decimal)txbValorFrete.Value;

                if (iTENS_ORCAMENTO == null)
                {
                    dcmSubtotal = txbSubtotal.Value == null ? 0.00m : (decimal)txbSubtotal.Value;
                    dcmDesconto = txbDesconto.Value == null ? 0.00m : (decimal)txbDesconto.Value;
                }
                else
                {
                    foreach (FDBOrcaDataSet.SP_TRI_ORCA_ITENS_ORCAMENTORow produto in iTENS_ORCAMENTO)
                    {
                        dcmSubtotal = dcmSubtotal + (produto.VALOR * produto.QUANT);
                        dcmDesconto = dcmDesconto + produto.DESCONTO;
                    }
                }

                dcmTotal = (dcmSubtotal - dcmDesconto) + dcmFrete;
                //TODO: ESTE ABSURDO DEVE-SE ÚNICA E EXCLUSIVAMENTE A ESSE CONTROLE QUE RESOLVE NÃO EXIBIR O VALOR DO CÁLCULO DEPOIS DE UM CLEAR() >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                txbSubtotal.Text = "0,00";
                txbSubtotal.Value = 0.00m;
                txbDesconto.Text = "0,00";
                txbDesconto.Value = 0.00m;
                txbTotal.Text = "0,00";
                txbTotal.Value = 0.00m;
                // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                txbSubtotal.Value = dcmSubtotal;
                txbDesconto.Value = dcmDesconto;
                txbTotal.Value = dcmTotal;
            }
            catch (Exception ex)
            {
                // Pode dar erro de cálculo... neste ponto, o app deve ser fechado para não comprometer o fluxo.
                gravarMensagemErro("Erro no CalcularExibirTotais(...). O app deve ser fechado.\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro no cálculo do orçamento. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                Application.Current.Shutdown(); // deuruim();
                return;
            }
        }

        private void GravarTotaisOrca()
        {
            using (var taOrca = new TRI_ORCA_ORCAMENTOSTableAdapter())
            {
                try
                {
                    taOrca.SP_TRI_ORCA_GRAVATOTAIS(_id_orcamento, Convert.ToInt16(operador.ID), txbSubtotal.Value, txbDesconto.Value, txbTotal.Value, txbValorFrete.Value);
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao gravar totais do orçamento. \nPor favor entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    Application.Current.Shutdown();
                    return;
                }
            }
        }

        /// <summary>
        /// Se houver um orçamento pendente devido a uma falha no sistema, ele é carregado
        /// </summary>
        private bool CarregarOrcamentoPendente()
        {
            //if (!cupom_aberto)
            //{
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\UltimoOrcamento.txt";
            if (File.Exists(path))
            {
                IEnumerable<string> ultimoOrcamento = File.ReadAllLines(path);

                if (ultimoOrcamento.Count() > 1) // File.ReadLines(path).Count()
                {
                    var continuaVenda = new DialogBox("Orçamento pendente", "Foi encontrada um orçamento pendente. Deseja continuá-lo?", DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.None);
                    continuaVenda.ShowDialog();
                    if (continuaVenda.DialogResult == true)
                    {
                        foreach (var line in ultimoOrcamento)
                        {
                            if (int.TryParse(line, out int intOrcaPendente))
                            {
                                _id_orcamento = intOrcaPendente;
                                txb_Orcamento.Text = _id_orcamento.ToString();
                                break;
                            }
                        }
                        EditarOrcamento();
                        return true;
                    }
                }
            }
            else
            {
                File.WriteAllText(path, "Arquivo gerado automaticamente pelo Ambi Orçamento. Não remova." + Environment.NewLine);
            }
            //}
            return false;
        }

        private void EditarOrcamento()
        {
            if (!_orcamento_inclusao && !_orcamento_alteracao)
            {
                CarregarOrcamento();
            }
            //ACBox.ItemsSource = _dt;
        }

        /// <summary>
        /// Limpa o arquivo "UltimoOrcamento.txt"
        /// </summary>
        public void LimparUltimoOrcamento()
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\UltimoOrcamento.txt";
                if (File.Exists(path) && File.ReadLines(path).Count() > 1)
                {
                    File.WriteAllText(path, "Arquivo gerado automaticamente pelo Ambi Orçamento. Não remova." + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
            }
        }

        private void PreEditarOrcamento()
        {
            if (!CarregarProdutos()) { return; }

            if (!CarregarOrcamentoPendente())
            {
                if (string.IsNullOrWhiteSpace(txb_Orcamento.Text))
                {
                    AbrirTelaConsultaOrcamentos();
                }
                else
                {
                    EditarOrcamento();
                }
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
                        _desconto = pd.reais * 1;
                        _descontando = tipoDesconto.Absoluto;
                        //txb_Avisos.Text = "DESCONTO ATIVO";
                        break;
                    case false:
                        _desconto = pd.porcentagem / 100;
                        _descontando = tipoDesconto.Percentual;
                        //txb_Avisos.Text = "DESCONTO ATIVO";
                        break;
                }
            }
            else if (pd.DialogResult == false)
            {
                //TODO but_Aplica_Desconto.Checked = false;
            }

        }

        private void AbrirTelaConsultaOrcamentos()
        {
            if (!_orcamento_inclusao)
            {
                var pesquisaOrca = new PesquisaOrca();
                pesquisaOrca.ShowDialog();

                if (pesquisaOrca.IdOrcamentoRetorno > 0)
                {
                    LimparTelaAtual();

                    txb_Orcamento.Text = pesquisaOrca.IdOrcamentoRetorno.ToString();

                    // Engatilhar a edição de orçamento aqui
                    EditarOrcamento();
                }
            }
        }

        #region Parte da solução para o TAB pular linhas na grid em vez de pular células
        // Fonte: https://social.msdn.microsoft.com/Forums/vstudio/en-US/54481b8b-0af1-4048-b8b8-a149ead7b643/wpf-datagrid-moving-by-row-not-by-cell?forum=wpf
        private DataGridCell GetCell(int row, int column)
        {
            DataGridRow rowContainer = GetRow(row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                if (presenter == null)
                {
                    iTENS_ORCAMENTODataGrid.ScrollIntoView(rowContainer, iTENS_ORCAMENTODataGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);

                return cell;
            }
            return null;
        }
        private DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)iTENS_ORCAMENTODataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                iTENS_ORCAMENTODataGrid.UpdateLayout();
                iTENS_ORCAMENTODataGrid.ScrollIntoView(iTENS_ORCAMENTODataGrid.Items[index]);
                row = (DataGridRow)iTENS_ORCAMENTODataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }
        private static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
        #endregion Parte da solução para o TAB pular linhas na grid em vez de pular células

        #endregion Methods

        #region Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CarregarProdutos();
            CarregarDatasourceClientes();
            acbCliente.ItemsSource = null;
            acbCliente.ItemsSource = _dtCliente;
            //TestarMassivamente();
        }//Preenche a lista de autocomplete com os produtos.
        private void _ACBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_rgx.IsMatch(ACBox.Text))
            {
                txb_Qtde.Text = ACBox.Text.TrimEnd('*');
                ACBox.Text = string.Empty;
                ACBox.SelectedItem = null;
            }
        }//Reconhece "*"(asterisco) como multiplicador de quantidade.
        private void ACBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //AUtoscroll no autocomplete da caixa de pesquisa.
        {
            AplicarAutoScrollNoAutoCompleteBox(sender);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) // Fecha o programa se a janela principal for fechada.
        {
            Application.Current.Shutdown();
        }
        private void ACBox_KeyDown(object sender, KeyEventArgs e)//Processa informação digitada na caixa de texto. Caso modo_consulta seja true, processa informação para consulta.
        {
            try
            {
                if (e.Key == Key.Enter && ACBox.Text != string.Empty)
                {
                    e.Handled = true;
                    ProcessaItem(false);
                    txbPrecoUnit.Focus();
                }
                else if (e.Key == Key.Escape && (ACBox.Text != string.Empty || ACBox.Text != String.Empty))
                {
                    e.Handled = true;
                    ACBox.Text = string.Empty;
                    txbPrecoUnit.Clear();
                    txb_Qtde.Clear();
                }
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                MessageBox.Show(ex.Message);
            }
        }
        private void Editar_MouseDown(object sender, MouseButtonEventArgs e) //Carrega informações do orçamento informado
        {
            PreEditarOrcamento();
        }

        private void cbb_Cliente_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && (_orcamento_inclusao || _orcamento_alteracao))
            {
                CarregaInfoCliente();
            }
        }
        private void cbb_Cliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_orcamento_inclusao || _orcamento_alteracao)
            {
                CarregaInfoCliente();
            }
        }
        private void lbl_novo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_orcamento_inclusao && !_orcamento_alteracao)
            {
                ReservaNovoOrcamento();
            }
        }
        private void but_Cancelar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CancelaOrcamentoNaTela();
        }
        private void but_Cancelar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CancelaOrcamentoNaTela();
            }
        }
        private void but_Salvar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                SalvaOrcamento(_status_orcamento.ToUpper().Equals("FECHADO"));
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao iniciar gravação de orçamento. Se o problema persistir, por favor entre em contato com a equipe de suporte.");
            }
        }
        private void but_Imprimir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SalvaOrcamento();
        }
        private void but_Salvar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    try
                    {
                        SalvaOrcamento(_status_orcamento.ToUpper().Equals("FECHADO"));
                    }
                    catch (Exception ex)
                    {
                        gravarMensagemErro(RetornarMensagemErro(ex, true));
                        MessageBox.Show("Erro ao iniciar gravação de orçamento. Se o problema persistir, por favor entre em contato com a equipe de suporte.");
                    }
                });
            }
        }

        private void txb_Orcamento_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"\d+");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void iTENS_ORCAMENTODataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                foreach (DataRowView row in iTENS_ORCAMENTODataGrid.SelectedItems)
                {
                    int idOrcaProduto = row.Row["ID_PRODUTO"].Safeint();
                    using (var iTENS_ORCAMENTO_TA = new TRI_ORCA_PRODUTOSTableAdapter())
                    {
                        try
                        {
                            iTENS_ORCAMENTO_TA.SP_TRI_ORCA_REMOVE_ITEM(idOrcaProduto);
                        }
                        catch (Exception ex)
                        {
                            string strErrMess = "Erro ao remover item do orçamento. \nPor favor tente novamente.";
                            gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show(strErrMess);
                            return;
                        }
                    }

                    _num_produto--;
                }

                try
                {
                    CarregarOrcamentoItens(true, _id_orcamento);
                }
                catch (Exception ex)
                {
                    // Pode dar erro de conexão... neste ponto, o app deve ser fechado.
                    gravarMensagemErro("Erro no iTENS_ORCAMENTODataGrid_KeyDown(...) ao deletar item de orçamento. O app deve ser fechado.\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show("Erro ao consultar itens do orçamento. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                    Application.Current.Shutdown(); // deuruim();
                    return;
                }

                GravarTotaisOrca();

                iTENS_ORCAMENTODataGrid.UpdateLayout();
                ACBox.Text = string.Empty;
                ACBox.SelectedItem = null;
                txb_Qtde.Clear();
            }
            else if (e.Key == Key.Tab)
            {
                #region Parte da solução para o TAB pular linhas na grid em vez de pular células

                int currentRowIndex = this.iTENS_ORCAMENTODataGrid.ItemContainerGenerator.IndexFromContainer(
                    this.iTENS_ORCAMENTODataGrid.ItemContainerGenerator.ContainerFromItem(this.iTENS_ORCAMENTODataGrid.CurrentItem));

                if (currentRowIndex < this.iTENS_ORCAMENTODataGrid.Items.Count - 1)
                {
                    this.iTENS_ORCAMENTODataGrid.SelectionMode = DataGridSelectionMode.Single;
                    GetRow(currentRowIndex + 1).IsSelected = true;
                    GetCell(currentRowIndex + 1, 0).Focus();
                    this.iTENS_ORCAMENTODataGrid.SelectionMode = DataGridSelectionMode.Extended;
                    e.Handled = true;
                }
                else if (currentRowIndex >= this.iTENS_ORCAMENTODataGrid.Items.Count - 1)
                {
                    but_Cancelar.Focus();

                    e.Handled = true;
                }

                #endregion Parte da solução para o TAB pular linhas na grid em vez de pular células
            } // Próxima linha da grid
        }

        private void txbValorFrete_LostFocus(object sender, RoutedEventArgs e)
        {
            CalcularExibirTotais(null); //deuruim();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_descontando.Equals(tipoDesconto.Nenhum) && (e.Key == Key.F8))
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    var senha = new perguntaSenha();
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
                            (new DialogBox("Senha incorreta", "A senha digitada não é uma senha de gerente.", "Impossível aplicar desconto.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn)).ShowDialog();
                            return;
                        }
                        AplicaDesconto(true);
                    }

                    ACBox.Focus();
                });
            } // Aplica desconto
            else
            if (e.Key == Key.F8 && _descontando != tipoDesconto.Nenhum)
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    string strDescDesconto = string.Empty;
                    switch (_descontando)
                    {
                        case tipoDesconto.Absoluto:
                            strDescDesconto = string.Format("R$ {0}", _desconto);
                            break;
                        case tipoDesconto.Percentual:
                            strDescDesconto = string.Format("{0} %", _desconto);
                            break;
                    }
                    var db = new DialogBox("Removendo desconto", string.Format("Desconto de {0} cancelado para o próximo item", strDescDesconto), DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.None);
                    db.ShowDialog();
                    _descontando = tipoDesconto.Nenhum;
                    _desconto = 0M;
                    //TODO but_Aplica_Desconto.Checked = false;
                });
            }//Cancela o desconto que seria lançado.
            else
            if (e.Key == Key.F6)
            {
                AbrirTelaConsultaOrcamentos();
            } // Abre tela de pesquisa de orçamentos
            else if (e.Key == Key.F5)
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    CarregarDatasourceClientes();
                    acbCliente.ItemsSource = null;
                    acbCliente.ItemsSource = _dtCliente;
                    MessageBox.Show("Clientes atualizados. Confira.");
                });
            }
            //else if (e.Key == Key.F4) // Chama a janela de consulta avançada.
            //{
            //    debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            //    {
            //        e.Handled = true;
            //        var ca = new ConsultaAvancada();
            //        ca.ShowDialog();
            //        if (ca.DialogResult == true && (_orcamento_alteracao || _orcamento_inclusao))
            //        {
            //            ACBox.Text = ca.codigo.ToString();
            //            try
            //            {
            //                ProcessaItem(false);
            //            }
            //            catch (Exception ex)
            //            {
            //                // Pode dar erro de conexão... neste ponto, o app deve ser fechado.
            //                gravarMensagemErro("Erro no Window_KeyDown(...) em ProcessaItem(...). O app deve ser fechado.\n" + RetornarMensagemErro(ex, true));
            //                MessageBox.Show("Erro ao processar itens do orçamento. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
            //                Application.Current.Shutdown(); // deuruim();
            //                return;
            //            }
            //            txbPrecoUnit.Focus();
            //        }
            //    });
            //}
            else if (!_orcamento_inclusao && !_orcamento_alteracao && (e.Key == Key.Escape))
            {
                e.Handled = true;

                var dbPergunta = new DialogBox("Fechar Módulo Orçamento", "\nDeseja mesmo sair?", DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.None);
                dbPergunta.ShowDialog();

                if (dbPergunta.DialogResult == true)
                {
                    //LimparUltimoOrcamento(); //TODO: não é necessário: só pergunta pra fechar se não tiver orçamento pendente.
                    Application.Current.Shutdown();
                    return;
                }
            }//Pergunta se quer fechar o sistema.
            else if (!_orcamento_inclusao && !_orcamento_alteracao && (e.Key == Key.P && e.KeyboardDevice.Modifiers == ModifierKeys.Control))
            {
                verbose("1 - iniciou Ctrl + P");
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    verbose("2 - Ctrl + P debounced");
                    //bool _senha = PedeSenhaGerencial();
                    //if (_senha == true)
                    //{
                    //    verbose("3 - Pediu senha gerencial com sucesso (validou)");
                    var par = new ParamsTecs(false, false);
                    verbose("3 - Vai abrir a tela ParamsTecs");
                    par.ShowDialog();
                    //verbose("4 - Carrega as configurações no sistema.");
                    //CarregaConfiguracoes(true);
                    return;
                    //}
                });
            }//Pergunta senha para abrir a tela de parâmetros.
            else if (e.Key == Key.F9 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                Process[] processo = Process.GetProcessesByName("AmbiPDV");
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

        }

        private void txb_Orcamento_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    if (!_orcamento_inclusao && !_orcamento_alteracao)
                    {
                        //    CarregarOrcamento();
                        PreEditarOrcamento();
                    }
                    //ACBox.ItemsSource = _dt;
                });
            }
        }

        private void iTENS_ORCAMENTODataGrid_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (iTENS_ORCAMENTODataGrid.Items is null) { return; }
            if (iTENS_ORCAMENTODataGrid.Items.Count <= 0) { return; }

            try
            {
                if (!(e.OldFocus is DataGridCell))
                {
                    iTENS_ORCAMENTODataGrid.SelectedItem = iTENS_ORCAMENTODataGrid.Items[0];
                }
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao selecionar orçamento!";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
        }

        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //if (acbCliente.SelectedIndex < 0) { return; }
                if (acbCliente.SelectedItem == null) { return; }

                (new VisualizacaoCadastroCliente(((MainViewModel.ComboBoxBindingDTO)acbCliente.SelectedItem).ID)).ShowDialog();
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao iniciar consulta expandida de cliente!";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
        }

        private void acbCliente_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && (_orcamento_inclusao || _orcamento_alteracao))
            {
                CarregaInfoCliente();
            }
        }

        private void acbCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarAutoScrollNoAutoCompleteBox(sender);
        }

        private void txbPrecoUnit_KeyDown(object sender, KeyEventArgs e)
        {
            #region Validar preço e focar no próximo controle

            try
            {
                if (e.Key == Key.Enter && txbPrecoUnit.Text != string.Empty)
                {
                    decimal.TryParse(txbPrecoUnit.Text, out decimal preco);

                    if (preco <= 0m)
                    {
                        MessageBox.Show("O preço unitário não deve ser menor ou igual a zero.");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txb_Qtde.Text))
                    {
                        txb_Qtde.Text = "1";
                    }

                    txb_Qtde.Focus();
                }
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                MessageBox.Show(ex.Message);
            }

            #endregion Validar preço e focar no próximo controle
        }

        private void txb_Qtde_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && txb_Qtde.Text != string.Empty)
                {
                    decimal.TryParse(txb_Qtde.Text, out decimal qtde);

                    if (qtde <= 0)
                    {
                        MessageBox.Show("A quantidade não deve ser menor ou igual a zero.");
                        return;
                    }

                    ProcessaItem(true);
                }
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                MessageBox.Show(ex.Message);
            }
        }

        private void txbPrecoUnit_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txbPrecoUnit.SelectAll();
        }

        private void txbPrecoUnit_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txbPrecoUnit.SelectAll();
        }

        private void txb_Qtde_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txb_Qtde.SelectAll();
        }

        private void txb_Qtde_GotMouseCapture(object sender, MouseEventArgs e)
        {
            txb_Qtde.SelectAll();
        }

        #endregion Events

        private void Orcamento_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F4) // Chama a janela de consulta avançada.
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    var ca = new ConsultaAvancada();
                    ca.ShowDialog();
                    if (ca.DialogResult == true && (_orcamento_alteracao || _orcamento_inclusao))
                    {
                        ACBox.Text = ca.codigo.ToString();
                        try
                        {
                            ProcessaItem(false);
                        }
                        catch (Exception ex)
                        {
                            // Pode dar erro de conexão... neste ponto, o app deve ser fechado.
                            gravarMensagemErro("Erro no Window_KeyDown(...) em ProcessaItem(...). O app deve ser fechado.\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show("Erro ao processar itens do orçamento. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                            Application.Current.Shutdown(); // deuruim();
                            return;
                        }
                        txbPrecoUnit.Focus();
                    }
                });
            }
        }
    }

    #region Classes auxiliares

    public class FocusableAutoCompleteBox : AutoCompleteBox
    {
        public new void Focus()
        {
            if (Template.FindName("Text", this) is TextBox textbox) textbox.Focus();
        }
        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    if (e.Key == Key.F4)
        //    {
        //        return;
        //    }
        //    else if (e.Key == Key.Escape && (Text != "" || Text != String.Empty))
        //    {
        //        Text = "";
        //        e.Handled = true;
        //    }
        //    else
        //    {
        //        base.OnKeyDown(e);
        //    }
        //}
    }//Controle da caixa autocompletável.

    public class ComboBoxBindingDTO_Produto
    {
        public int ID_IDENTIFICADOR { get; set; }
        public string DESCRICAO { get; set; }
        public string COD_BARRA { get; set; }
        public string REFERENCIA { get; set; }
    }

    #endregion Classes auxiliares
}
