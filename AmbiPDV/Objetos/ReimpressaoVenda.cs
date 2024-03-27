using System;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;

namespace PDV_WPF.Objetos
{
    public class ReimpressaoVenda
    {
        public string Cliente { get; set; }
        public decimal Valor { get; set; }
        public int Num_Cupom { get; set; }
        public DateTime TS_Venda { get; set; }
        public string Status { get; set; }
        public int ID_NFVENDA { get; set; }
        public string NF_SERIE { get; set; }
        public string NF_MODELO { get; set; }
        public ClienteDuePayDTO ClienteDuePay { get; set; }
        public bool ActivateContextMenu => Status is not "C" && NF_SERIE.StartsWith("N") && SAT_USADO;
    }
}
