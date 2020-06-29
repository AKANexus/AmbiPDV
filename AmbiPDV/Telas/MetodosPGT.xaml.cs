using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for MetodosPGT.xaml
    /// </summary>
    public partial class MetodosPGT : Window
    {
        #region Fields & Properties

        private FDBDataSet fDBDataSet = new FDBDataSet();
        //private FbConnection SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
        //private DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_METODOSTableAdapter taVendaMetodosServ = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_METODOSTableAdapter();
        //private FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter taMetodosServ = new FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter();
        bool editando = false;
        List<string> modos = new List<string> { "C", "F" };
        Dictionary<int, string> Interno_CFE = new Dictionary<int, string>
        {
                        { 0, "01" },
                        { 1, "02" },
                        { 2, "03" },
                        { 3, "04" },
                        { 4, "05" },
                        { 5, "10" },
                        { 6, "11" },
                        { 7, "12" },
                        { 8, "13" },
                        { 9, "99" }
        };
        Dictionary<string, int> CFE_Interno = new Dictionary<string, int>
        {
                        { "01", 0 },
                        { "02", 1 },
                        { "03", 2 },
                        { "04", 3 },
                        { "05", 4 },
                        { "10", 5 },
                        { "11", 6 },
                        { "12", 7 },
                        { "13", 8 },
                        { "99", 9 }
        };
        int _cod;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public MetodosPGT()
        {
            //taVendaMetodosServ.Connection = SERVER_FB_CONN;
            //taMetodosServ.Connection = SERVER_FB_CONN;

            InitializeComponent();
            fDBDataSet = ((FDBDataSet)(this.FindResource("fDBDataSet")));
        }

        ~MetodosPGT()
        {
            //taVendaMetodosServ?.Dispose();
            //taMetodosServ?.Dispose();
            fDBDataSet?.Dispose();

            //if (SERVER_FB_CONN != null)
            //{
            //    if (SERVER_FB_CONN.State != ConnectionState.Closed)
            //    {
            //        SERVER_FB_CONN.Close();
            //    }
            //    SERVER_FB_CONN.Dispose();
            //}
        }

        #endregion (De)Constructor

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PreencherMetodosTela();
        }

        private void but2_click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                if (editando == true)
                {
                    editando = false;
                    but_2.IsEnabled = true;
                    but_1.Content = "Ativar/Desativar";
                    but_2.Content = "Editar";
                    txb_descr.Clear(); txb_cod.Clear(); txb_receb.Clear(); cbb_fiscal.SelectedIndex = 0; cbb_modo.SelectedIndex = 0;
                    return;
                }
                else
                {
                    if (tRI_PDV_METODOSDataGrid is null) { return; }
                    if (tRI_PDV_METODOSDataGrid.SelectedItem is null) { return; }

                    try
                    {
                        int rowindex = (int)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row[0]);
                    }
                    catch (NullReferenceException) //HACK: isso pode ser engatilhado frequentemente? Se for, o melhor seria verificar se o objeto é nulo ou não.
                    {
                        return;
                        throw;
                    }

                    _cod = (int)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["ID_PAGAMENTO"]);


                    txb_cod.Text = (_cod).ToString();
                    txb_descr.Text = (((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["DESCRICAO"]).ToString();
                    txb_receb.Text = (((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["DIAS"]).ToString();



                    switch ((string)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["METODO"]))
                    {
                        case ("C"):
                            cbb_modo.SelectedIndex = 0;
                            break;
                        case ("F"):
                            cbb_modo.SelectedIndex = 1;
                            break;
                        default:
                            break;
                    }
                    cbb_fiscal.SelectedIndex = CFE_Interno[(string)((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["PGTOCFE"]];
                    but_1.Content = "Salvar";
                    but_2.Content = "Cancelar";
                    editando = true;
                }

                PreencherMetodosTela();

            });
        }

        private void cbb_fiscal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void but1_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                if (editando == true)
                {
                    int.TryParse(txb_receb.Text, out int _res1);
                    int.TryParse(txb_cod.Text, out int _res2);

                    //try
                    //{
                    //    using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
                    //    using (var taMetodosServ = new FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter())
                    //    {
                    //        taMetodosServ.Connection = SERVER_FB_CONN;
                    //        taMetodosServ.AtualizaMetodo(txb_descr.Text, _res1, modos[cbb_modo.SelectedIndex], Interno_CFE[cbb_fiscal.SelectedIndex], _res2);
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    gravarMensagemErro(RetornarMensagemErro(ex, true));
                    //    MessageBox.Show("Erro ao atualizar métodos de pagamento. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                    //    Environment.Exit(0); // deuruim();
                    //    return;
                    //}

                    editando = false;
                    but_1.Content = "Ativar/Desativar";
                    but_2.Content = "Editar";
                    txb_descr.Clear(); txb_cod.Clear(); txb_receb.Clear(); cbb_fiscal.SelectedIndex = 0; cbb_modo.SelectedIndex = 0;
                    int rowindex = _res2; // (int)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row[0]);
                                          //int firstrowindex = tRI_PDV_METODOSDataGrid.FirstDisplayedCell.RowIndex;

                    //tRI_PDV_METODOSDataGrid.FirstDisplayedScrollingRowIndex = firstrowindex;
                    //tRI_PDV_METODOSDataGrid.CurrentCell = tRI_PDV_METODOSDataGrid.Rows[rowindex].Cells[0];

                    PreencherMetodosTela();

                    if (USATEF == true && ((cbb_fiscal.Text.StartsWith("03")) || (cbb_fiscal.Text.StartsWith("04"))))
                    {
                        DialogBox.Show("Utilização de TEF", DialogBoxButtons.Yes, DialogBoxIcons.None, false, "Códigos 04 e 05 irão acionar o TEF no momento da venda.");
                    }
                }
                else
                {
                    try
                    {
                        int rowindex = (int)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row[0]);
                    }
                    catch (NullReferenceException) //HACK: isso pode ser engatilhado frequentemente? Se for, o melhor seria verificar se o objeto é nulo ou não.
                    {

                        return;
                    }

                    //int firstrowindex = tRI_PDV_METODOSDataGrid.FirstDisplayedCell.RowIndex;
                    //int.TryParse((string)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["ID_PAGAMENTO"]), out int _cod)
                    int _indice = tRI_PDV_METODOSDataGrid.SelectedIndex;
                    _cod = (int)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["ID_PAGAMENTO"]);

                    MetodoPgtoToggleAtivo(_cod);

                    PreencherMetodosTela();

                    tRI_PDV_METODOSDataGrid.SelectedIndex = _indice;
                    //tRI_PDV_METODOSDataGrid.FirstDisplayedScrollingRowIndex = firstrowindex;
                    //tRI_PDV_METODOSDataGrid.CurrentCell = tRI_PDV_METODOSDataGrid.Rows[rowindex].Cells[0];
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Row_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int _indice = tRI_PDV_METODOSDataGrid.SelectedIndex;
            _cod = (int)(((DataRowView)tRI_PDV_METODOSDataGrid.SelectedItem).Row["ID_PAGAMENTO"]);

            MetodoPgtoToggleAtivo(_cod);

            PreencherMetodosTela();

            tRI_PDV_METODOSDataGrid.SelectedIndex = _indice;

        }

        #endregion Events

        #region Methods

        private void MetodoPgtoToggleAtivo(int cod)
        {
            //try
            //{
            //    using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
            //    using (var taVendaMetodosServ = new FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter())
            //    {
            //        taVendaMetodosServ.Connection = SERVER_FB_CONN;
            //        taVendaMetodosServ.SP_TRI_TOGGLEMETODO(_cod);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    gravarMensagemErro(RetornarMensagemErro(ex, true));
            //    MessageBox.Show("Erro ao ativar/desativar método de pagamento. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
            //    Environment.Exit(0); // deuruim();
            //    return;
            //}
        }

        private void PreencherMetodosTela()
        {
            //try
            //{
            //    using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
            //    using (var taMetodosServ = new FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter())
            //    {
            //        taMetodosServ.Connection = SERVER_FB_CONN;
            //        taMetodosServ.ClearBeforeFill = true;
            //        taMetodosServ.Fill(fDBDataSet.TRI_PDV_METODOS);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    gravarMensagemErro(RetornarMensagemErro(ex, true));
            //    MessageBox.Show("Erro ao consultar métodos de pagamento. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
            //    Environment.Exit(0); // deuruim();
            //    return;
            //}
        }

        #endregion Methods

    }
}
