namespace YandehCargaWS.Yandeh
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
        public string code { get; set; }
        public string dt_ultima_alt { get; set; }
        public string origem_coleta { get; set; }
        public Dimension dimension { get; set; }
    }

    public class Dimension
    {
        public string measurement_unit { get; set; }
    }

    public class Price_Info
    {
        public decimal price { get; set; }
        //public float special_price { get; set; }
        //public string special_price_from { get; set; }
        //public string special_price_to { get; set; }
    }
}