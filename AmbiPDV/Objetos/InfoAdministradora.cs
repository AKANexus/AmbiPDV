using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PDV_WPF.Objetos
{
    public class InfoAdministradora
    {
        public int IdAdmin { get; set; }
        public string Descricao { get; set; }
        public int IdConta { get; set; }
        public int IdCliente { get; set; }
        public decimal TaxaCredito { get; set; }
        public decimal TaxaDebito { get; set; }
        public int DiasParaVencimento { get; set; }
    }
}
