using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandehCarga.Yandeh
{

    public class SelloutBody
    {
        public string id { get; set; }
        public string sellout_timestamp { get; set; }
        public string store_taxpayer_id { get; set; }
        public int checkout_id { get; set; }
        public int receipt_number { get; set; }
        public string receipt_series_number { get; set; }
        public string nfe_access_key { get; set; }
        public float subtotal { get; set; }
        public string cancellation_flag { get; set; }
        public string operation { get; set; }
        public string transaction_type { get; set; }
        public List<Item> items { get; set; }
        public string tipo { get; set; }
        public List<Payment> payment { get; set; }
    }

    public class Item
    {
        public string code { get; set; }
        public string sku { get; set; }
        public string description { get; set; }
        public float quantity { get; set; }
        public string measurement_unit { get; set; }
        public float unit_value { get; set; }
        public string cancellation_flag { get; set; }
        public int cfop { get; set; }
        public string cst { get; set; }
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
