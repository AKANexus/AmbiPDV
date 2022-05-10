namespace YandehCargaWS.Yandeh
{
    public class Authorization
    {
        public bool enabled { get; set; }
        public DateTime created_at { get; set; }
        public string[] allowedDomains { get; set; }
        public string principalId { get; set; }
        public string creationtimestamp { get; set; }
        public string[] allowedIPs { get; set; }
        public string name { get; set; }
        public string authkey { get; set; }
    }
}