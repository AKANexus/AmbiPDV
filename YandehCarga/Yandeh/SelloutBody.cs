using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandehCarga.Yandeh
{

    public class SelloutBody
    {
        public float sales_discount { get; set; }
        public float sales_addition { get; set; }
        public string origem_coleta { get; set; }
        public string id { get; set; }
        public string sellout_timestamp { get; set; }
        public string store_taxpayer_id { get; set; }
        public string checkout_id { get; set; }
        public string receipt_number { get; set; }
        public string receipt_series_number { get; set; }
        public string nfe_access_key { get; set; }
        public float total { get; set; }
        public string cancellation_flag { get; set; }
        public string operation { get; set; }
        public string transaction_type { get; set; }
        public List<SelloutItem> items { get; set; }
        public string tipo { get; set; }
        public List<Payment> payment { get; set; }
        public float ipi { get; set; }
        public float icms { get; set; }
        public float frete { get; set; }
    }

    public class SelloutItem
    {
        public string code { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public float quantity { get; set; }
        public string measurement_unit { get; set; }
        public float unit_value { get; set; }
        public string cancellation_flag { get; set; }
        public int cfop { get; set; }
        public float item_addition { get; set; }
        public float item_discount { get; set; }
        public float ipi { get; set; }
        public float other_expenses { get; set; }
        public float icms_st { get; set; }
        public float fcp_st { get; set; }
        public float frete { get; set; }
        public float icms { get; set; }
        public float pis { get; set; }
        public float cofins { get; set; }
    }

    public class Payment
    {
        public string method { get; set; }
        public string condition { get; set; }
        public List<Installment> installments { get; set; }
    }

    public class Installment
    {
        public int installment_number { get; set; }
        public float amount { get; set; }
        public int payment_term { get; set; }
    }

}
