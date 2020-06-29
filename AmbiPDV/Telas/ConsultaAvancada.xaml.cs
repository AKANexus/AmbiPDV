using FirebirdSql.Data.FirebirdClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;



namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ConsultaAvancada.xaml
    /// </summary>
    public partial class ConsultaAvancada : Window
    {
        #region Fields & Properties

        //FDBDataSetTableAdapters.SP_TRI_PREENCHECONSULTATableAdapter Estoque_TA = new FDBDataSetTableAdapters.SP_TRI_PREENCHECONSULTATableAdapter();
        //private FbConnection LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
        //private FbConnection SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
        public int codigo;
        private bool _blnContingencia;

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public ConsultaAvancada(bool pContingencia)
        {
            _blnContingencia = pContingencia;

            InitializeComponent();
            txb_Consulta.Focus();

            //_contingencia = pContingencia;
        }

        #endregion (De)Constructor

        #region Events

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    Int32.TryParse(txb_Consulta.Text.ToString(), out int integro);

                    string strConn = _blnContingencia ? MontaStringDeConexao("localhost", localpath) :
                                                        MontaStringDeConexao(SERVERNAME, SERVERCATALOG);

                    using var X_FB_CONN = new FbConnection { ConnectionString = strConn };
                    using var Estoque_TA = new FDBDataSetTableAdapters.SP_TRI_PREENCHECONSULTATableAdapter
                    {
                        Connection = X_FB_CONN
                    };

                    if (txb_Consulta.Text.ToString().IsNumbersOnly() && txb_Consulta.Text.ToString().Length < 8)
                    {
                        dgv_Tabela.ItemsSource = Estoque_TA.Consulta(null, integro);
                    }
                    else
                    {
                        dgv_Tabela.ItemsSource = Estoque_TA.Consulta(txb_Consulta.Text.ToString(), null);
                    }
                });
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                this.Close();
                return;
            }
        }

        private void dgv_Tabela_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                this.Close();
            }
            else if (e.Key == Key.Enter)
            {
                if (dgv_Tabela.SelectedItem == null)
                {
                    if (dgv_Tabela.HasItems)
                    {
                        dgv_Tabela.SelectedIndex = 0;
                    }
                    else
                    {
                        return;
                    }
                }

                var row = ((DataRowView)dgv_Tabela.SelectedItem).Row["ESTOQUE"];
                codigo = row.Safeint();
                DialogResult = true;
                this.Close();
            }
            else if (e.Key == Key.Tab)
            {
                #region Parte da solução para o TAB pular linhas na grid em vez de pular células

                int currentRowIndex = this.dgv_Tabela.ItemContainerGenerator.IndexFromContainer(
                    this.dgv_Tabela.ItemContainerGenerator.ContainerFromItem(this.dgv_Tabela.CurrentItem));

                if (currentRowIndex < this.dgv_Tabela.Items.Count - 1)
                {
                    this.dgv_Tabela.SelectionMode = DataGridSelectionMode.Single;
                    GetRow(currentRowIndex + 1).IsSelected = true;
                    GetCell(currentRowIndex + 1, 0).Focus();
                    this.dgv_Tabela.SelectionMode = DataGridSelectionMode.Extended;
                    e.Handled = true;
                }
                else if (currentRowIndex >= this.dgv_Tabela.Items.Count - 1)
                {
                    txb_Consulta.Focus();

                    e.Handled = true;
                }

                #endregion Parte da solução para o TAB pular linhas na grid em vez de pular células
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Estoque_TA.Dispose();
        }

        #endregion Events

        #region Methods

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
                    dgv_Tabela.ScrollIntoView(rowContainer, dgv_Tabela.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);

                return cell;
            }
            return null;
        }

        private DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)dgv_Tabela.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                dgv_Tabela.UpdateLayout();
                dgv_Tabela.ScrollIntoView(dgv_Tabela.Items[index]);
                row = (DataGridRow)dgv_Tabela.ItemContainerGenerator.ContainerFromIndex(index);
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
    public class Item
    {
        public string ID_ESTOQUE { get; set; }
    }
}