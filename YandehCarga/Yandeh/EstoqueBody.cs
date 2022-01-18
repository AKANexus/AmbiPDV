using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandehCarga.Yandeh
{

    public class EstoqueBody
    {
        public string product_type { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Price_Info price_info { get; set; }
        public string visibility { get; set; }
        public string status { get; set; }
    }

    public class Price_Info
    {
        public float price { get; set; }
        public float special_price { get; set; }
        public string special_price_from { get; set; }
        public string special_price_to { get; set; }
    }

}
