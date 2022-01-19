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
        public string store_taxpayer_id { get; set; }
        public string nfe_number { get; set; }
        public string nfe_series_number { get; set; }
        public string nfe_access_key { get; set; }
        public string supplier_taxpayer_id { get; set; }
        public decimal gross_total { get; set; }
        public decimal net_total { get; set; }
        public string cancellation_flag { get; set; }
        public List<SellInItem> items { get; set; }
        public decimal icms { get; set; }
        public decimal freight_price { get; set; }
        public decimal insurance_price { get; set; }
        public decimal other_expenses { get; set; }
        public List<Payment> payment { get; set; }
        public string origem_coleta { get; set; }
        public decimal ipi { get; set; }
        public decimal sales_discount { get; set; }
        public decimal sales_addition { get; set; }
    }

    public class SellInItem
    {
        public string code { get; set; }
        public string ean { get; set; }
        public string description { get; set; }
        public decimal quantity { get; set; }
        public decimal unit_value { get; set; }
        public decimal gross_total { get; set; }
        public decimal net_total { get; set; }
        public string measurement_unit { get; set; }
        public decimal icms { get; set; }
        public decimal pis { get; set; }
        public decimal cofins { get; set; }
        public int cfop { get; set; }
        public decimal addition { get; set; }
        public decimal discount { get; set; }
        public decimal ipi { get; set; }
        public decimal other_expenses { get; set; }
        public decimal icms_st { get; set; }
        public decimal fcp_st { get; set; }
        public decimal freight_price { get; set; }
    }
}
