using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for Fechamentos.xaml
    /// </summary>
    public partial class Fechamentos : Window
    {
        #region Fields & Properties

        private List<decimal> valores = new List<decimal>();
        public decimal total { get; set; }
        public decimal _01 { get; set; }
        public decimal _02 { get; set; }
        public decimal _03 { get; set; }
        public decimal _04 { get; set; }
        public decimal _05 { get; set; }
        public decimal _06 { get; set; }
        public decimal _07 { get; set; }
        public decimal _08 { get; set; }
        public decimal _09 { get; set; }
        public decimal _10 { get; set; }
        public decimal _11 { get; set; }
        public decimal _12 { get; set; }
        public decimal _13 { get; set; }
        public decimal _14 { get; set; }
        public decimal _15 { get; set; }
        public decimal _16 { get; set; }
        public decimal _17 { get; set; }
        public decimal _18 { get; set; }
        public decimal _19 { get; set; }
        public decimal _20 { get; set; }
        public decimal _SANG { get; set; }
        public decimal _SUP { get; set; }
        public decimal _TROCA { get; set; }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public Fechamentos()
        {
            InitializeComponent();

            dtp_DataInicial.Focus();
        }

        #endregion (De)Constructor

        #region Events

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ReimprimirFechamentoSelecionado(); // deuruim();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                //e.Handled = true;

                this.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PrepararDatabindGrid();
        }

        private void sP_TRI_LISTAFECHAMENTOSDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReimprimirFechamentoSelecionado(); // deuruim();

                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                #region Parte da solução para o TAB pular linhas na grid em vez de pular células

                int currentRowIndex = this.sP_TRI_LISTAFECHAMENTOSDataGrid.ItemContainerGenerator.IndexFromContainer(
                    this.sP_TRI_LISTAFECHAMENTOSDataGrid.ItemContainerGenerator.ContainerFromItem(this.sP_TRI_LISTAFECHAMENTOSDataGrid.CurrentItem));

                if (currentRowIndex < this.sP_TRI_LISTAFECHAMENTOSDataGrid.Items.Count - 1)
                {
                    this.sP_TRI_LISTAFECHAMENTOSDataGrid.SelectionMode = DataGridSelectionMode.Single;
                    GetRow(currentRowIndex + 1).IsSelected = true;
                    GetCell(currentRowIndex + 1, 0).Focus();
                    this.sP_TRI_LISTAFECHAMENTOSDataGrid.SelectionMode = DataGridSelectionMode.Extended;
                    e.Handled = true;
                }
                else if (currentRowIndex >= this.sP_TRI_LISTAFECHAMENTOSDataGrid.Items.Count - 1)
                {
                    dtp_DataInicial.Focus();

                    e.Handled = true;
                }

                #endregion Parte da solução para o TAB pular linhas na grid em vez de pular células
            }
        }

        #endregion Events

        #region Methods

        private void ReimprimirFechamentoSelecionado()
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                if (sP_TRI_LISTAFECHAMENTOSDataGrid.SelectedItem != null)
                {
                    int _indice = sP_TRI_LISTAFECHAMENTOSDataGrid.SelectedIndex;

                    var intIdCaixa = (int)(((DataRowView)sP_TRI_LISTAFECHAMENTOSDataGrid.SelectedItem).Row["ID_CAIXA"]);
                    var dtmFechado = (DateTime)(((DataRowView)sP_TRI_LISTAFECHAMENTOSDataGrid.SelectedItem).Row["FECHADO"]);

                    ReimprimirFechamentoCaixa(intIdCaixa, dtmFechado); // deuruim();

                    sP_TRI_LISTAFECHAMENTOSDataGrid.SelectedIndex = _indice;
                }
            });
        }

        private void PrepararDatabindGrid()
        {
            if (dtp_DataFinal.SelectedDate != null && dtp_DataInicial.SelectedDate != null)
            {
                try
                {
                    var fDBDataSet = ((FDBDataSet)(this.FindResource("fDBDataSet")));

                    using (var fDBDataSetSP_TRI_LISTAFECHAMENTOSTableAdapter = new FDBDataSetTableAdapters.SP_TRI_LISTAFECHAMENTOSTableAdapter())
                    using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                    {
                        fDBDataSetSP_TRI_LISTAFECHAMENTOSTableAdapter.Connection = LOCAL_FB_CONN;
                        fDBDataSetSP_TRI_LISTAFECHAMENTOSTableAdapter.Fill(fDBDataSet.SP_TRI_LISTAFECHAMENTOS,
                                                                           (DateTime)dtp_DataInicial.SelectedDate,
                                                                           ((DateTime)dtp_DataFinal.SelectedDate).AddHours(23).AddMinutes(59).AddSeconds(59));
                    }

                    var sP_TRI_LISTAFECHAMENTOSViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("sP_TRI_LISTAFECHAMENTOSViewSource")));
                    sP_TRI_LISTAFECHAMENTOSViewSource.View.MoveCurrentToFirst();
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show("Erro ao consultar fechamentos. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                    Environment.Exit(0); // deuruim();
                    return;
                }
            }
        }

        private async void ReimprimirFechamentoCaixa(int intIdCaixa, DateTime dtmFechado)
        {
            using var Impressao = new PrintFECHA();
            PegarValoresFechamentoUsuario(intIdCaixa, dtmFechado);
            switch (DialogBox.Show("Reimpressão de Fechamento", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja reimprimir o fechamento do caixa?"))
            {
                case true:
                    LoadingProccess loadingProccess = new LoadingProccess();
                    loadingProccess.Show();
                    this.IsEnabled = false;

                    await Task.Run(() =>
                    {
                        DataRow metodo_pgto_col;
                        metodo_pgto_col = Impressao.fecha_infor_dt.NewRow();
                        metodo_pgto_col[0] = -1;
                        metodo_pgto_col["DIN"] = _01;
                        metodo_pgto_col["CHEQUE"] = _02;
                        metodo_pgto_col["CREDITO"] = _03;
                        metodo_pgto_col["DEBITO"] = _04;
                        metodo_pgto_col["LOJA"] = _05;
                        metodo_pgto_col["ALIMENTACAO"] = _06;
                        metodo_pgto_col["REFEICAO"] = _07;
                        metodo_pgto_col["PRESENTE"] = _08;
                        metodo_pgto_col["COMBUSTIVEL"] = _09;
                        metodo_pgto_col["OUTROS"] = _10;
                        metodo_pgto_col["EXTRA_1"] = _11;
                        metodo_pgto_col["EXTRA_2"] = _12;
                        metodo_pgto_col["EXTRA_3"] = _13;
                        metodo_pgto_col["EXTRA_4"] = _14;
                        metodo_pgto_col["EXTRA_5"] = _15;
                        metodo_pgto_col["EXTRA_6"] = _16;
                        metodo_pgto_col["EXTRA_7"] = _17;
                        metodo_pgto_col["EXTRA_8"] = _18;
                        metodo_pgto_col["EXTRA_9"] = _19;
                        metodo_pgto_col["EXTRA_10"] = _20;
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
                        using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                        {
                            EMIT_TA.Connection = LOCAL_FB_CONN;

                            try
                            {
                                EMIT_TA.Fill(EMIT_DT);
                            }
                            catch (Exception ex)
                            {
                                logErroAntigo(RetornarMensagemErro(ex, true));
                                MessageBox.Show("Erro ao consultar dados do emitente. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                                Environment.Exit(0); // deuruim();
                                return;
                            }

                            Impressao.cnpjempresa = EMIT_DT[0].CNPJ.TiraPont();
                            //Impressao.nomefantasia = Properties.Settings.Default.nomefantasia;//TODO essa informação deve vir da base de dados do Clipp.
                            Impressao.nomefantasia = EMIT_DT[0].NOME_FANTA.Safestring();
                            Impressao.enderecodaempresa = string.Format("{0} {1}, {2} - {3}, {4}",
                                                                        EMIT_DT[0].END_TIPO,
                                                                        EMIT_DT[0].END_LOGRAD,
                                                                        EMIT_DT[0].END_NUMERO,
                                                                        EMIT_DT[0].END_BAIRRO,
                                                                        "São Paulo");
                            using var METODOS_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter();
                            using var METODOS_DT = new DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable();
                            METODOS_TA.Connection = LOCAL_FB_CONN;
                            METODOS_TA.FillByAtivos(METODOS_DT);
                            Impressao.IMPRIME(dtmFechado, METODOS_DT, intIdCaixa, false); // deuruim();
                                                                                          //Oper.SP_TRI_FECHACAIXA(Properties.Settings.Default.no_caixa, _01, _02, _03, _04, _05, _06, _07, _08, _09, _10, _11, _12, _13, _14, _15, _16, _17, _18, _19, _20, _TROCA, _SUP, _SANG, userid);
                        }
                        //this.Close();
                    });
                    this.IsEnabled = true; this.Focus(); 
                    loadingProccess.Close();                    
                    return;
                case false:
                    //txb_01.Focus();
                    break;
                default:
                    break;
            }
        }

        private void PegarValoresFechamentoUsuario(int intIdCaixa, DateTime dtmFechado)
        {
            using var taFechamentoPdv = new FDBDataSetTableAdapters.TRI_PDV_FECHAMENTOSTableAdapter();
            using var tblFechamentoPdv = new FDBDataSet.TRI_PDV_FECHAMENTOSDataTable();
            taFechamentoPdv.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath).ToString();

            try
            {
                taFechamentoPdv.FillByCaixaFech(tblFechamentoPdv, intIdCaixa, dtmFechado);
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao consultar fechamentos. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                Environment.Exit(0); // deuruim();
                return;
            }

            if (tblFechamentoPdv != null)
            {
                if (tblFechamentoPdv.Rows.Count > 0)
                {
                    _01 = (decimal)tblFechamentoPdv.Rows[0]["DIN"];
                    _02 = (decimal)tblFechamentoPdv.Rows[0]["CHEQUE"];
                    _03 = (decimal)tblFechamentoPdv.Rows[0]["CREDITO"];
                    _04 = (decimal)tblFechamentoPdv.Rows[0]["DEBITO"];
                    _05 = (decimal)tblFechamentoPdv.Rows[0]["LOJA"];
                    _06 = (decimal)tblFechamentoPdv.Rows[0]["ALIMENTACAO"];
                    _07 = (decimal)tblFechamentoPdv.Rows[0]["REFEICAO"];
                    _08 = (decimal)tblFechamentoPdv.Rows[0]["PRESENTE"];
                    _09 = (decimal)tblFechamentoPdv.Rows[0]["COMBUSTIVEL"];
                    _10 = (decimal)tblFechamentoPdv.Rows[0]["OUTROS"];
                    _11 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_1"];
                    _12 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_2"];
                    _13 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_3"];
                    _14 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_4"];
                    _15 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_5"];
                    _16 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_6"];
                    _17 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_7"];
                    _18 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_8"];
                    _19 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_9"];
                    _20 = (decimal)tblFechamentoPdv.Rows[0]["EXTRA_10"];
                    _SANG = (decimal)tblFechamentoPdv.Rows[0]["SANGRIAS"];
                    _SUP = (decimal)tblFechamentoPdv.Rows[0]["SUPRIMENTOS"];
                    _TROCA = (decimal)tblFechamentoPdv.Rows[0]["TROCAS"];
                    total = _01 + _02 + _03 + _04 + _05 + _06 + _07 + _08 + _09 + _10 + _11 + _12 + _13 + _14 + _15 + _16 + _17 + _18 + _19 + _20;
                    valores.Clear();
                    valores.AddRange(new List<decimal> { _01, _02, _03, _04, _05, _06, _07, _08, _09, _10, _11, _12, _13, _14, _15, _16, _17, _18, _19, _20, _SANG, _SUP, _TROCA });
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
                    sP_TRI_LISTAFECHAMENTOSDataGrid.ScrollIntoView(rowContainer, sP_TRI_LISTAFECHAMENTOSDataGrid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);

                return cell;
            }
            return null;
        }

        private DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)sP_TRI_LISTAFECHAMENTOSDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                sP_TRI_LISTAFECHAMENTOSDataGrid.UpdateLayout();
                sP_TRI_LISTAFECHAMENTOSDataGrid.ScrollIntoView(sP_TRI_LISTAFECHAMENTOSDataGrid.Items[index]);
                row = (DataGridRow)sP_TRI_LISTAFECHAMENTOSDataGrid.ItemContainerGenerator.ContainerFromIndex(index);
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

    }
}
