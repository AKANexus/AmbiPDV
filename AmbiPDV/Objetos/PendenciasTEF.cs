using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Objetos
{
    public class PendenciasDoTEF
    {
        private XmlSerializer serializer = new XmlSerializer(typeof(PendTEFXml));
        public PendTEFXml PendTEFOrig;

        public bool AdicionaPendenciaNoXML(string noCupom, string idPag, string data, string hora, string codFuncao, string vlrOrig, string nsu, TipoTEF tipo)
        {

            // Deserialization
            PendTEFOrig = Deserializa();

            // Add element
            PendTEFOrig.Pendencias.Insert(0, new Pendencia(noCupom, idPag, data, hora, codFuncao, vlrOrig, nsu, tipo));

            PendTEFOrig.Pendencias = PendTEFOrig.Pendencias.Distinct().ToList();
            //Serialization
            Serializa();
            return true;
        }

        public void LimpaPendencias(string noCupom)
        {
            PendTEFOrig = Deserializa();
            PendTEFOrig.Pendencias.RemoveAll(p => p.NoCupom == noCupom);
            Serializa();
        }

        public void LimpaTodasPendencias()
        {
            PendTEFOrig = Deserializa();
            PendTEFOrig.Pendencias.Clear();
            Serializa();
        }
        private PendTEFXml Deserializa()
        {
            if (!File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}Logs\TEFPendentes.txt"))
            {
                using StreamWriter sw = new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}Logs\TEFPendentes.txt");
                sw.WriteLine("<pendTEFs></pendTEFs>");
            }
            //DeSerialization
            string xmlReadStream = File.ReadAllText($@"{AppDomain.CurrentDomain.BaseDirectory}Logs\TEFPendentes.txt");
            using var XmlRetorno = new StringReader(xmlReadStream);
            using var xreader = XmlReader.Create(XmlRetorno);
            PendTEFXml retorno;
            try
            {
                retorno = (PendTEFXml)serializer.Deserialize(xreader);
            }
            catch (Exception)
            {
                File.Delete($@"{AppDomain.CurrentDomain.BaseDirectory}Logs\TEFPendentes.txt");
                using StreamWriter sw = new StreamWriter($@"{AppDomain.CurrentDomain.BaseDirectory}Logs\TEFPendentes.txt");
                sw.WriteLine("<pendTEFs></pendTEFs>");
                retorno = new PendTEFXml();
            }
            return retorno;
        }
        private void Serializa()
        {
            var settings = new XmlWriterSettings() { Encoding = new UTF8Encoding(true), OmitXmlDeclaration = true, Indent = true };
            var XMLPendFinal = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(XMLPendFinal, settings))
            {
                var xns = new XmlSerializerNamespaces();
                xns.Add(string.Empty, string.Empty);
                serializer.Serialize(writer, PendTEFOrig, xns);
            }
            File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}Logs\TEFPendentes.txt", XMLPendFinal.ToString());
        }
        public List<Pendencia> ObtemListaDePendencias(string noCupom)
        {
            return Deserializa().Pendencias.FindAll(p => p.NoCupom == noCupom);
        }
    }


    [XmlRoot(ElementName = "pendTEFs", DataType = "string", IsNullable = true)]
    public class PendTEFXml
    {
        [XmlArray("pendencias")]
        [XmlArrayItem("pendencia")]
        public List<Pendencia> Pendencias { get; set; }
        public PendTEFXml()
        {
            Pendencias = new List<Pendencia>();
        }
    }

    public class Pendencia
    {
        [XmlElement("noCupom")]
        public string NoCupom { get; set; }
        [XmlElement("idPagto")]
        public string IdPag { get; set; }
        [XmlElement("dataFiscal")]
        public string DataFiscal { get; set; }
        [XmlElement("horaFiscal")]
        public string HoraFiscal { get; set; }
        [XmlElement("codDaFuncao")]
        public string CodDaFuncao { get; set; }
        [XmlElement("valOriginal")]
        public string ValOriginal { get; set; }
        [XmlElement("NSU")]
        public string NSU { get; set; }
        [XmlElement("tipo")]
        public string Tipo { get; set; }

        public Pendencia() { }

        public Pendencia(string nocupom, string idpag, string data, string hora, string codfunc, string valOrig, string nsu, TipoTEF tipo)
        {
            NoCupom = nocupom;
            IdPag = idpag;
            DataFiscal = data;
            HoraFiscal = hora;
            CodDaFuncao = codfunc;
            ValOriginal = valOrig;
            Tipo = tipo switch
            {
                TipoTEF.Credito => "credito",
                TipoTEF.Debito => "debito",
                _ => "undefined"
            };
            NSU = nsu;
        }
    }

}
