using FirebirdSql.Data.FirebirdClient;
using PDV_WPF;
using System;
using System.Windows;
using System.Windows.Input;
using System.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Interaction logic for ConsultaAvancada.xaml
    /// </summary>
    public partial class ConsultaAvancada : Window
    {
        #region Fields & Properties

        //FDBOrcaDataSetTableAdapters.SP_TRI_PREENCHECONSULTA_TIPDESCTableAdapter Estoque_TA = new FDBOrcaDataSetTableAdapters.SP_TRI_PREENCHECONSULTA_TIPDESCTableAdapter();
        //private FbConnection SERVER_FB_CONN = new FbConnection { ConnectionString = Properties.Settings.Default.NetworkDB };
        public dynamic codigo;

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public ConsultaAvancada()
        {

            //Estoque_TA.Connection = SERVER_FB_CONN;

            InitializeComponent();
            txb_Consulta.Focus();

            cmbPesqNomeClienteTipo.SelectedIndex = 0;

            //_contingencia = pContingencia;
        }

        //~ConsultaAvancada()
        //{
        //    Estoque_TA?.Dispose();
        //    if (SERVER_FB_CONN != null)
        //    {
        //        if (SERVER_FB_CONN.State != ConnectionState.Closed)
        //        {
        //            SERVER_FB_CONN.Close();
        //        }
        //        SERVER_FB_CONN.Dispose();
        //    }
        //}

        #endregion (De)Constructor

        #region Events

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    Int32.TryParse(txb_Consulta.Text.ToString(), out int integro);

                    try
                    {
                        using (var Estoque_TA = new FDBOrcaDataSetTableAdapters.SP_TRI_PREENCHECONSULTA_TIPDESCTableAdapter())
                        {
                            if (txb_Consulta.Text.ToString().IsNumbersOnly() && txb_Consulta.Text.ToString().Length < 8)
                            {
                                dgv_Tabela.ItemsSource = Estoque_TA.Consulta(null, integro, cmbPesqNomeClienteTipo.SelectedIndex);
                            }
                            else
                            {
                                dgv_Tabela.ItemsSource = Estoque_TA.Consulta(txb_Consulta.Text.ToString(), null, cmbPesqNomeClienteTipo.SelectedIndex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao consultar itens no estoque. \nPor favor verifique os dados e tente novamente.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
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
                RetornarProdutoSelecionado();
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

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            RetornarProdutoSelecionado();
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

        private void RetornarProdutoSelecionado()
        {
            if (dgv_Tabela.SelectedItem == null)
            {
                if (dgv_Tabela.HasItems)
                {
                    dgv_Tabela.SelectedIndex = 0;
                }
                else { return; }
            }
            var referencia = ((DataRowView)dgv_Tabela.SelectedItem).Row["REFERENCIA"];
            string refe = referencia.ToString();
            if (refe != "" && refe != null)
            {
                if (MessageBox.Show("Deseja utilizar o campo 'REFERÊNCIA' nesta consulta?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {                    
                    codigo = referencia.Safestring();
                    DialogResult = true;
                    this.Close();
                }
                else
                {
                    var row = ((DataRowView)dgv_Tabela.SelectedItem).Row["ESTOQUE"];
                    codigo = row.Safeint();
                    DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                var row = ((DataRowView)dgv_Tabela.SelectedItem).Row["ESTOQUE"];
                codigo = row.Safeint();
                DialogResult = true;
                this.Close();
            }
        }

        #endregion Methods


    }
    public class Produto
    {
        public string ID_ESTOQUE { get; set; }
    }
}