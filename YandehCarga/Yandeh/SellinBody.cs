using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandehCarga.Yandeh
{

    public class SellInBody

    {
        public string id { get; set; }
        public string sellin_timestamp { get; set; }
        public long store_taxpayer_id { get; set; }
        public int nfe_number { get; set; }
        public int nfe_series_number { get; set; }
        public string nfe_access_key { get; set; }
        public long supplier_taxpayer_id { get; set; }
        public float gross_total { get; set; }
        public float net_total { get; set; }
        public string cancellation_flag { get; set; }
        public List<SellInItem> items { get; set; }
        public float icms { get; set; }
        public int freight_price { get; set; }
        public int insurance_price { get; set; }
        public int other_expenses { get; set; }
        public List<Payment> payment { get; set; }
    }

    public class SellInItem
    {
        public string code { get; set; }
        public string ean { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public float unit_value { get; set; }
        public float gross_total { get; set; }
        public float net_total { get; set; }
        public string measurement_unit { get; set; }
        public float icms { get; set; }
        public float pis { get; set; }
        public float cofins { get; set; }
        public int cfop { get; set; }
        public string cst { get; set; }
    }
}
