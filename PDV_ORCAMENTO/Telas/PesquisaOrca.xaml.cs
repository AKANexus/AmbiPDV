using PDV_ORCAMENTO.FDBOrcaDataSetTableAdapters;
using PDV_WPF;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Interaction logic for PesquisaOrca.xaml
    /// </summary>
    public partial class PesquisaOrca : Window
    {
        #region Fields & Properties

        private enum EnmPesqTipo { numOrca = 0, nomeCliente = 1 }

        private List<OrcaDTO> lstOrcas = new List<OrcaDTO>();

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        public int IdOrcamentoRetorno { get; set; }

        private EnmPesqTipo _tipoPesquisa;

        #endregion Fields & Properties

        #region (De)Constructor

        public PesquisaOrca()
        {
            InitializeComponent();

            IdOrcamentoRetorno = -1;

            txbGeral.Focus();

            cmbTipoPesquisa.SelectedIndex = 0;
            SetTipoPesquisa(0);
        }

        #endregion (De)Constructor

        #region Events

        private void dgOrcamentos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //ReimprimirFechamentoSelecionado();
                RetornarOrcamentoSelecionado();

                e.Handled = true;
            } // Retorna orçamento selecionado
            else if (e.Key == Key.Tab)
            {
                #region Parte da solução para o TAB pular linhas na grid em vez de pular células

                int currentRowIndex = this.dgOrcamentos.ItemContainerGenerator.IndexFromContainer(
                    this.dgOrcamentos.ItemContainerGenerator.ContainerFromItem(this.dgOrcamentos.CurrentItem));

                if (currentRowIndex < this.dgOrcamentos.Items.Count - 1)
                {
                    this.dgOrcamentos.SelectionMode = DataGridSelectionMode.Single;
                    GetRow(currentRowIndex + 1).IsSelected = true;
                    GetCell(currentRowIndex + 1, 0).Focus();
                    this.dgOrcamentos.SelectionMode = DataGridSelectionMode.Extended;
                    e.Handled = true;
                }
                else if (currentRowIndex >= this.dgOrcamentos.Items.Count - 1)
                {
                    cmbTipoPesquisa.Focus();

                    e.Handled = true;
                }

                #endregion Parte da solução para o TAB pular linhas na grid em vez de pular células
            } // Próxima linha da grid
        }

        private void txbGeral_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && txbGeral.Text != string.Empty)
                {
                    debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                    {
                        ExecutarPesquisa();
                    });
                }
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    this.Close();
                });
            } // Fecha a tela de pesquisa de orçamento
        }

        private void dgOrcamentos_Loaded(object sender, RoutedEventArgs e)
        {
            ((DataGrid)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void cmbTipoPesquisa_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTipoPesquisa(((ComboBox)sender).SelectedIndex);
        }

        private void btnPesquisar_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ExecutarPesquisa();
        }

        private void btnPesquisar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    ExecutarPesquisa();
                });
            }
        }

        private void dgOrcamentos_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (dgOrcamentos.Items is null) { return; }
            if (dgOrcamentos.Items.Count <= 0) { return; }

            try
            {
                if (!(e.OldFocus is DataGridCell))
                {
                    dgOrcamentos.SelectedItem = dgOrcamentos.Items[0];
                }
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao selecionar orçamento! \nPor favor pesquise outra vez o orçamento.";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
        }



        #endregion Events

        #region Methods

        //private void CarregarOrcas()
        //{
        //    #region Por ID

        //    //using (var taOrca = new SP_TRI_ORCA_PESQ_GETTableAdapter())
        //    //using (var tbOrca = new FDBOrcaDataSet.SP_TRI_ORCA_PESQ_GETDataTable())
        //    //{
        //    //    taOrca.Fill(tbOrca, Convert.ToInt32(txbGeral.Text));

        //    //    lstOrcas = (from FDBOrcaDataSet.SP_TRI_ORCA_PESQ_GETRow row in tbOrca.Rows
        //    //                select new OrcaDTO
        //    //                {
        //    //                    ID_ORCAMENTO = Convert.ToInt32(row["ID_ORCAMENTO"]),
        //    //                    CLIENTE_NOME = (row.IsCLIENTE_NOMENull() ? string.Empty : row["CLIENTE_NOME"].ToString()),
        //    //                    DT_EMISSAO = Convert.ToDateTime(row["DT_EMISSAO"]),
        //    //                    DT_VALIDADE = row.IsDT_VALIDADENull() ? null : (DateTime?)row["DT_VALIDADE"],
        //    //                    DT_ENTREGA = row.IsDT_ENTREGANull() ? null : (DateTime?)row["DT_ENTREGA"],
        //    //                    DT_VENCIMENTO = row.IsDT_VENCIMENTONull() ? null : (DateTime?)row["DT_VENCIMENTO"],
        //    //                    VALOR_TOTAL = Convert.ToDecimal(row["VALOR_TOTAL"])
        //    //                }).ToList();

        //    //    dgOrcamentos.ItemsSource = lstOrcas;
        //    //}

        //    #endregion Por ID

        //    #region Todos

        //    using (var taOrca = new SP_TRI_ORCA_PESQ_GETALLTableAdapter())
        //    using (var tbOrca = new FDBOrcaDataSet.SP_TRI_ORCA_PESQ_GETALLDataTable())
        //    {
        //        try
        //        {

        //        }
        //        catch (Exception ex)
        //        {
        //            gravarMensagemErro(RetornarMensagemErro(ex, true));
        //            throw;
        //        }


        //        taOrca.Fill(tbOrca);

        //        lstOrcas = ExtractOrcaListFromDataTable(tbOrca);
        //        dgOrcamentos.ItemsSource = lstOrcas;
        //    }

        //    #endregion Todos
        //}

        private List<OrcaDTO> ExtractOrcaListFromDataTable(FDBOrcaDataSet.SP_TRI_ORCA_PESQ_GETALLDataTable tbOrca)
        {
            return (from FDBOrcaDataSet.SP_TRI_ORCA_PESQ_GETALLRow row in tbOrca.Rows
                    select new OrcaDTO
                    {
                        ID_ORCAMENTO = Convert.ToInt32(row["ID_ORCAMENTO"]),
                        CLIENTE_NOME = (row.IsCLIENTE_NOMENull() ? string.Empty : row["CLIENTE_NOME"].ToString()),
                        DT_EMISSAO = Convert.ToDateTime(row["DT_EMISSAO"]),
                        DT_VALIDADE = row.IsDT_VALIDADENull() ? null : (DateTime?)row["DT_VALIDADE"],
                        DT_ENTREGA = row.IsDT_ENTREGANull() ? null : (DateTime?)row["DT_ENTREGA"],
                        DT_VENCIMENTO = row.IsDT_VENCIMENTONull() ? null : (DateTime?)row["DT_VENCIMENTO"],
                        VALOR_TOTAL = Convert.ToDecimal(row["VALOR_TOTAL"]),
                        STATUS = row.IsSTATUSNull() ? string.Empty : row["STATUS"].ToString()
                    }).ToList();
        }

        private void RetornarOrcamentoSelecionado()
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                if (dgOrcamentos.SelectedItem != null)
                {
                    int _indice = dgOrcamentos.SelectedIndex;

                    //IdOrcamentoRetorno = (int)(((DataRowView)dgOrcamentos.SelectedItem).Row["ID_ORCAMENTO"]);
                    IdOrcamentoRetorno = ((OrcaDTO)dgOrcamentos.SelectedItem).ID_ORCAMENTO;

                    //ReimprimirFechamentoCaixa(intIdCaixa, dtmFechado);
                    this.Close();

                    dgOrcamentos.SelectedIndex = _indice;
                }
            });
        }

        private void dgOrcamentos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RetornarOrcamentoSelecionado();
        }

        private void ExecutarPesquisa()
        {
            try
            {
                using (var tbPesqOrca = new FDBOrcaDataSet.SP_TRI_ORCA_PESQ_GETALLDataTable())
                {
                    switch (_tipoPesquisa)
                    {
                        case EnmPesqTipo.numOrca:

                            #region Pesquisa por número de orçamento

                            #region Intervalo fechado

                            if ((itbNumOrcaIni.Value > 0 && itbNumOrcaFin.Value > 0) && (itbNumOrcaFin.Value >= itbNumOrcaIni.Value))
                            {
                                using (var taPesqOrca = new SP_TRI_ORCA_PESQ_GETALLTableAdapter())
                                {
                                    try
                                    {
                                        taPesqOrca.FillByInterOrca(tbPesqOrca, Convert.ToInt32(itbNumOrcaIni.Value), Convert.ToInt32(itbNumOrcaFin.Value));
                                    }
                                    catch (Exception ex)
                                    {
                                        string strErrMess = "Erro ao consultar orçamentos (intervalo fechado). \nPor favor tente novamente.";
                                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                                        MessageBox.Show(strErrMess);
                                        return;
                                    }
                                }
                            }

                            #endregion Intervalo fechado

                            #region Maior que o inicial

                            else if (itbNumOrcaIni.Value > 0 && (itbNumOrcaFin.Value is null || itbNumOrcaFin.Value == 0))
                            {
                                using (var taPesqOrca = new SP_TRI_ORCA_PESQ_GETALLTableAdapter())
                                {
                                    try
                                    {
                                        taPesqOrca.FillByInterOrca(tbPesqOrca, Convert.ToInt32(itbNumOrcaIni.Value), 0);
                                    }
                                    catch (Exception ex)
                                    {
                                        string strErrMess = "Erro ao consultar orçamentos (intervalo com final aberto). \nPor favor tente novamente.";
                                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                                        MessageBox.Show(strErrMess);
                                        return;
                                    }
                                }
                            }

                            #endregion Maior que o inicial

                            #region Menor que o final

                            else if ((itbNumOrcaIni.Value is null || itbNumOrcaIni.Value <=0) && itbNumOrcaFin.Value > 0)
                            {
                                using (var taPesqOrca = new SP_TRI_ORCA_PESQ_GETALLTableAdapter())
                                {
                                    try
                                    {
                                        taPesqOrca.FillByInterOrca(tbPesqOrca, 0, Convert.ToInt32(itbNumOrcaFin.Value));
                                    }
                                    catch (Exception ex)
                                    {
                                        string strErrMess = "Erro ao consultar orçamentos (intervalo com início aberto). \nPor favor tente novamente.";
                                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                                        MessageBox.Show(strErrMess);
                                        return;
                                    }
                                }
                            }

                            #endregion Menor que o final

                            #endregion Pesquisa por número de orçamento

                            break;
                        case EnmPesqTipo.nomeCliente:

                            #region Pesquisa por nome de cliente

                            if (!string.IsNullOrWhiteSpace(txbGeral.Text))
                            {
                                using (var taPesqOrca = new SP_TRI_ORCA_PESQ_GETALLTableAdapter())
                                {
                                    try
                                    {
                                        taPesqOrca.FillByClienteNome(tbPesqOrca, txbGeral.Text, GetPesqOrcaClienteNomeTipo());
                                    }
                                    catch (Exception ex)
                                    {
                                        string strErrMess = "Erro ao consultar orçamentos (nome de cliente). \nPor favor tente novamente.";
                                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                                        MessageBox.Show(strErrMess);
                                        return;
                                    }
                                }
                            }

                            #endregion Pesquisa por nome de cliente

                            break;
                        default:
                            throw new NotImplementedException("Tipo de pesquisa de orçamento não esperado!" + _tipoPesquisa.ToString());
                    }

                    lstOrcas = ExtractOrcaListFromDataTable(tbPesqOrca);
                    dgOrcamentos.ItemsSource = lstOrcas;
                }
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao executar a pesquisa de orçamentos!";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
        }

        private int? GetPesqOrcaClienteNomeTipo()
        {
            //if (rbtPesqNomeClienteTipoContendo.IsChecked == true) { return 0; }
            //if (rbtPesqNomeClienteTipoIniciando.IsChecked == true) { return 1; }
            //if (rbtPesqNomeClienteTipoIgual.IsChecked == true) { return 2; }
            return cmbPesqNomeClienteTipo.SelectedIndex;
            //return null;
        }

        private void SetTipoPesquisa(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0: // Nº do orçamento
                    stkPesqNomeCliente.Visibility = Visibility.Collapsed;
                    stkPesqOrca.Visibility = Visibility.Visible;
                    _tipoPesquisa = EnmPesqTipo.numOrca;
                    break;
                case 1: // Nome do cliente
                    stkPesqNomeCliente.Visibility = Visibility.Visible;
                    stkPesqOrca.Visibility = Visibility.Collapsed;
                    //if ((rbtPesqNomeClienteTipoContendo.IsChecked == false || rbtPesqNomeClienteTipoContendo.IsChecked is null) &&
                    //    (rbtPesqNomeClienteTipoIgual.IsChecked == false || rbtPesqNomeClienteTipoIgual.IsChecked is null) &&
                    //    (rbtPesqNomeClienteTipoIniciando.IsChecked == false || rbtPesqNomeClienteTipoIniciando.IsChecked is null))
                    if (!(cmbPesqNomeClienteTipo.SelectedIndex > 0 && cmbPesqNomeClienteTipo.SelectedIndex <= 2))
                    {
                        //rbtPesqNomeClienteTipoContendo.IsChecked = true;
                        cmbPesqNomeClienteTipo.SelectedIndex = 0;
                    }
                    _tipoPesquisa = EnmPesqTipo.nomeCliente;
                    break;
                default:
                    break;
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
                    dgOrcamentos.ScrollIntoView(rowContainer, dgOrcamentos.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);

                return cell;
            }
            return null;
        }
        private DataGridRow GetRow(int index)
        {
            DataGridRow row = (DataGridRow)dgOrcamentos.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                dgOrcamentos.UpdateLayout();
                dgOrcamentos.ScrollIntoView(dgOrcamentos.Items[index]);
                row = (DataGridRow)dgOrcamentos.ItemContainerGenerator.ContainerFromIndex(index);
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

        public class OrcaDTO
        {
            public int ID_ORCAMENTO { get; set; }
            public string CLIENTE_NOME { get; set; }
            public DateTime DT_EMISSAO { get; set; }
            public DateTime? DT_VALIDADE { get; set; }
            public DateTime? DT_ENTREGA { get; set; }
            public DateTime? DT_VENCIMENTO { get; set; }
            public decimal VALOR_TOTAL { get; set; }
            public string STATUS { get; set; }
        }
    }
}