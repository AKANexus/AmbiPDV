using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Objetos;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para DetalhesVenda.xaml
    /// </summary>
    public partial class DetalhesVenda : Window
    {
        public int IdNfVenda { get; set; }
        public int NfNumero { get; set; }
        public string NfSerie { get; set; }
        public string NfModelo { get; set; }
        public string NomeCliente { get; set; }
        public string DtEmissao { get; set; }
        public string HrEmissao { get; set; }
        public string ValorTotal { get; set; }
        public StringBuilder SbPagamentos { get; set; } = new();
        public string Pagamentos => SbPagamentos.ToString();
        public string StatusVenda { get; set; }
        public string TipoVenda => NfSerie.Contains(value: "N") ? "Não fiscal" : "Fiscal";

        public DetalhesVenda(ReimpressaoVenda venda)
        {
            InitializeComponent();
            DataContext = this;

            IdNfVenda = venda.ID_NFVENDA;
            NfNumero = venda.Num_Cupom;
            NfSerie = venda.NF_SERIE;
            NfModelo = venda.NF_MODELO;
            NomeCliente = venda.Cliente;
            DtEmissao = venda.TS_Venda.ToShortDateString();
            HrEmissao = venda.TS_Venda.ToShortTimeString();
            ValorTotal = venda.Valor.ToString("C2");
            StatusVenda = venda.Status == "C" ? "Cancelada" : "Efetivada";

            using (var detalhesPagtosDt = new DataSets.FDBDataSetVenda.DatalhesVendaTableDataTable())
            using (var detalhesPagtosTa = new DataSets.FDBDataSetVendaTableAdapters.DatalhesVendaTableTableAdapter())
            {
                //detalhesPagtosTa.Connection = new FbConnection(MontaStringDeConexao(datasource: "localhost", initialcatalog: localpath));
                detalhesPagtosTa.FillByIdNfVenda(dataTable: detalhesPagtosDt, ID_NFVENDA: venda.ID_NFVENDA);

                foreach (var pagamento in detalhesPagtosDt)
                {
                    if (SbPagamentos.Length > 0)
                        SbPagamentos.Append(value: $", {pagamento.DESCRICAO}");
                    else
                        SbPagamentos.Append(value: $"{pagamento.DESCRICAO}");                    
                }
            }
        }

        private void DetalhesVenda_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
