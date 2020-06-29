using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.ViewModels;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for FechamentoOrcamento.xaml
    /// </summary>
    public partial class FechamentoOrcamento : Window
    {
        #region Fields & Properties

        private DataSets.FDBDataSetVenda.TB_PARCELAMENTODataTable PARCEL_DT = new DataSets.FDBDataSetVenda.TB_PARCELAMENTODataTable();

        public string Parcelamento { get; set; }
        public int no_parcelas { get; set; }
        public DateTime primeiro_vencimento { get; set; }
        public bool a_vista = false;
        public decimal valor_a_vista = 0;
        public int cupom;
        public int no_orcamento;
        public int cliente;
        public decimal Valor_Orcamento { get; set; }
        public decimal Valor_Parcela { get; set; }
        private DataTable DGV_DT = new DataTable();
        private FbConnection LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
        private FbConnection SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };

        #endregion Fields & Properties

        #region (De)Constructor

        public FechamentoOrcamento(Orcamento orcamento)
        {
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var PARCEL_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter
            {
                Connection = LOCAL_FB_CONN
            };
            a_vista = false;
            InitializeComponent();
            DataContext = new MainViewModel();
            txb_Parcelamento.SelectedItem = Parcelamento = orcamento.pagamento;
            Valor_Orcamento = orcamento.valor_tot;
            DGV_DT.Columns.Add("Vencimento", typeof(DateTime));
            DGV_DT.Columns.Add("Valor", typeof(decimal));
            dgv_Parcelas.DataContext = DGV_DT.DefaultView;
            cliente = orcamento.Cod_Cliente;
            no_orcamento = orcamento.no_orcamento;
            try
            {
                PARCEL_DT = PARCEL_TA.GetDataByDescricaoParcelamento(Parcelamento);
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao buscar parcelamentos do orçamento!");
            }
        }

        ~FechamentoOrcamento()
        {
            PARCEL_DT.Dispose();
            DGV_DT.Dispose();
        }

        #endregion (De)Constructor

        #region Events

        private void but_Confirmar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /* TODO: Se a forma de parcelamento escolhida tiver venda à vista, preencher
             * a_vista com true, e preencher o valor pago à vista em valor_a_vista. Caso
             * haja parcelas, elas deverão ser listadas nas listas Parcelas e Vencimentos,
             * listas públicas, que serão consumidas no fechamento do cupom em Caixa.cs.
            */

            //foreach (DataRow item in DGV_DT.Rows)
            //{
            //    if (Convert.ToDateTime(item["Vencimento"].ToString()) == DateTime.Today)
            //    {
            //        a_vista = true;
            //        valor_a_vista = (decimal)item["Valor"];
            //    }
            //    else
            //    {
            //        using (FDBDataSetTableAdapters.TB_CONTA_RECEBERTableAdapter ContaRec_TA = new FDBDataSetTableAdapters.TB_CONTA_RECEBERTableAdapter())
            //        using (FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter OPER_TA = new FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter())
            //        {
            //            ContaRec_TA.Connection = OPER_TA.Connection = LOCAL_FB_CONN;
            //            ContaRec_TA.SP_TRI_LANCACONTAREC(cupom, cupom.ToString(), Convert.ToDateTime(item["Vencimento"].ToString()), (decimal)item["Valor"], cliente, "Parcela ref. Orcam. " + no_orcamento.ToString());
            //            OPER_TA.SP_TRI_LANCAMOVDIARIO("x", (decimal)item["Valor"], "Parcela ref. Orcam. " + no_orcamento.ToString(), 147, 5);
            //            OPER_TA.SP_TRI_MOV_CTAREC(cupom.ToString());
            //        }
            //    }
            //}
            DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //if ((string)PARCEL_DT.Rows[0]["ENTRADA"] == "S")
            //{
            //    primeiro_vencimento = DateTime.Today;
            //}
            //else
            //{
            //    primeiro_vencimento = DateTime.Today.AddDays(Convert.ToDouble((short)PARCEL_DT.Rows[0]["INTERVALO"]));
            //}
            //if ((short)PARCEL_DT.Rows[0]["N_PARCELAS"] == 0) { no_parcelas = 1; }
            //else { no_parcelas = (short)PARCEL_DT.Rows[0]["N_PARCELAS"]; }
            //Valor_Parcela = Valor_Orcamento / no_parcelas;
            //for (int i = 0; i < no_parcelas; i++)
            //{
            //    if (i == 0)
            //    { DGV_DT.Rows.Add(primeiro_vencimento, Valor_Parcela); }
            //    else { DGV_DT.Rows.Add(((DateTime)DGV_DT.Rows[i - 1]["Vencimento"]).AddDays(Convert.ToDouble((short)PARCEL_DT.Rows[0]["INTERVALO"])), Valor_Parcela); }
            //}
        }

        private void but_Cancelar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void txb_Parcelamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var PARCEL_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter
            {
                Connection = LOCAL_FB_CONN
            };

            DGV_DT.Clear();
            if ((string)PARCEL_DT.Rows[0]["ENTRADA"] == "S")
            {
                primeiro_vencimento = DateTime.Today;
            }
            else
            {
                primeiro_vencimento = DateTime.Today.AddDays(Convert.ToDouble((short)PARCEL_DT.Rows[0]["INTERVALO"]));
            }
            if ((short)PARCEL_DT.Rows[0]["N_PARCELAS"] == 0) { no_parcelas = 1; }
            else
            {
                PARCEL_DT = PARCEL_TA.GetDataByDescricaoParcelamento(txb_Parcelamento.SelectedItem.ToString());
                no_parcelas = (short)PARCEL_DT.Rows[0]["N_PARCELAS"];
            }
            Valor_Parcela = Valor_Orcamento / no_parcelas;
            Valor_Parcela = Valor_Parcela;
            decimal _auxiliar = 0;
            for (int i = 0; i < no_parcelas; i++)
            {
                if (i == 0)
                {
                    DGV_DT.Rows.Add(primeiro_vencimento, Valor_Parcela);
                    _auxiliar += Valor_Parcela;
                }
                else if (i == no_parcelas - 1)
                {
                    DGV_DT.Rows.Add(((DateTime)DGV_DT.Rows[i - 1]["Vencimento"]).AddDays(Convert.ToDouble((short)PARCEL_DT.Rows[0]["INTERVALO"])), Valor_Orcamento - _auxiliar);
                }
                else
                {
                    DGV_DT.Rows.Add(((DateTime)DGV_DT.Rows[i - 1]["Vencimento"]).AddDays(Convert.ToDouble((short)PARCEL_DT.Rows[0]["INTERVALO"])), Valor_Parcela);
                    _auxiliar += Valor_Parcela;
                }

            }
        }

        #endregion Events

        #region Methods

        #endregion Methods
    }
}
