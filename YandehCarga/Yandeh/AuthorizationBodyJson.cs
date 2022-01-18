using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YandehCarga.Yandeh
{

    public class AuthorizationBodyJson
    {
        public string name { get; set; }
        public List<string> allowedIPs { get; set; }
        public List<string> allowedDomains { get; set; }
    }

}
