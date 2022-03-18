using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relatórios2
{
    public class ReportObjectVenda
    {
        public List<Venda> Vendas { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        private List<Pagamento> TodosOsPagamentos => Vendas.SelectMany(x => x.Pagamentos).ToList();
        private List<string> FormasPagamentos => TodosOsPagamentos.Select(x => x.FormaPagamento).Distinct().ToList();

        public List<Pagamento> SomaPagamentos
        {
            get
            {
                List<Pagamento> retorno = new();
                foreach (string formaPagto in FormasPagamentos)
                {
                    decimal sum = TodosOsPagamentos.Where(x => x.FormaPagamento == formaPagto)
                        .Sum(x => x.ValorPagamento);
                    retorno.Add(new(formaPagto, sum));
                }

                return retorno;
            }
        }
    }

    

    public class Pagamento
    {

        public Pagamento(string formaPagamento, decimal valorPagamento)
        {
            FormaPagamento = formaPagamento;
            ValorPagamento = valorPagamento;
        }

        public string FormaPagamento { get; set; }
        public decimal ValorPagamento { get; set; }

    }

    public class Venda
    {
        public Venda(string numCupom, DateTime tsVenda, decimal valorTotal, List<Pagamento> pagamentos)
        {
            NumCupom = numCupom;
            TsVenda = tsVenda;
            ValorTotal = valorTotal;
            Pagamentos = pagamentos;
        }
        public String NumCupom { get; set; }
        public DateTime TsVenda { get; set; }
        public decimal ValorTotal { get; set; }
        public List<Pagamento> Pagamentos { get; set; }
    }

}
