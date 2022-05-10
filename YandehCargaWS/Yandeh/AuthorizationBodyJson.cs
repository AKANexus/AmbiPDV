namespace YandehCargaWS.Yandeh
{
    public class AuthorizationBodyJson
    {
        public string name { get; set; }
        public List<string> allowedIPs { get; set; }
        public List<string> allowedDomains { get; set; }
    }
}