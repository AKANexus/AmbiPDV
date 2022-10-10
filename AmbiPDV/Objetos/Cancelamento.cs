using CfeCancelamento_0008;
using DeclaracoesDllSat;
using PDV_WPF.Exceptions;
using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;

namespace PDV_WPF.Objetos
{
    class Cancelamento : IDisposable
    {
        private cancCFeCFeCanc _cFeCanc;
        private cancCFeCFeCancInfCFe _cancinfCFe;
        private cancCFeCFeCancInfCFeDest _cancinfCFeDest;
        private cancCFeCFeCancInfCFeEmit _cancinfCFeEmit;
        private cancCFeCFeCancInfCFeIde _cancinfCfeIde;
        //private infCFeObsFisco _caninfCFeObsFisco;
        private cancCFeCFeCancInfCFeTotal _cancinfCFeTotal;



        public void Dispose()
        {

        }

        public string GeraXMLdeCancelamentoFiscal(string chaveAnterior, string assinatura, string numeroCaixa, string CNPJSH)
        {
            if (!(_cFeCanc is null)) throw new ErroDeValidacaoDeConteudo("XML só pode conter um grupo de informações de cabeçalho");
            if (chaveAnterior.Length != 44) throw new ErroDeValidacaoDeConteudo("Chave de acesso informada é inválida");
            _cFeCanc = new();
            _cancinfCFe = new();
            _cancinfCfeIde = new();
            _cancinfCFeEmit = new();
            _cancinfCFeDest = new();
            _cancinfCFeTotal = new();
            _cFeCanc.infCFe.chCanc = "CFe" + chaveAnterior;
            _cancinfCfeIde.signAC = assinatura;
            _cancinfCfeIde.numeroCaixa = numeroCaixa;
            _cancinfCfeIde.CNPJ = CNPJSH;
            if (assinatura.Contains("RETAGUARDA"))
            {
                switch (MODELO_SAT)
                {
                    case ModeloSAT.NENHUM:
                        break;
                    case ModeloSAT.DARUMA:
                        break;
                    case ModeloSAT.DIMEP:
                        _cancinfCfeIde.CNPJ = "16716114000172";
                        //_cancinfCFeEmit.CNPJ = "61099008000141";
                        //_cancinfCFeEmit.IE = "111111111111";
                        break;
                    case ModeloSAT.BEMATECH:
                        break;
                    case ModeloSAT.ELGIN:
                        break;
                    case ModeloSAT.SWEDA:
                        break;
                    case ModeloSAT.CONTROLID:
                        _cancinfCfeIde.CNPJ = "16716114000172";
                        //_cancinfCFeEmit.CNPJ = "08238299000129";
                        //_cancinfCFeEmit.IE = "149392863111";
                        break;
                    case ModeloSAT.TANCA:
                        break;
                    case ModeloSAT.EMULADOR:
                        break;
                    default:
                        break;
                }
            }
            _cancinfCFe.dest = _cancinfCFeDest;
            _cancinfCFe.ide = _cancinfCfeIde;
            _cancinfCFe.emit = _cancinfCFeEmit;
            _cancinfCFe.total = _cancinfCFeTotal;
            _cFeCanc.infCFe = _cancinfCFe;
            var _settings = new XmlWriterSettings() { Encoding = new UTF8Encoding(false) };
            var _XmlFinal = new StringBuilder();
            var _xwriter2 = XmlWriter.Create(_XmlFinal, _settings);
            var _serializer = new XmlSerializer(_cFeCanc.GetType());
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);
            _serializer.Serialize(_xwriter2, _cFeCanc, xns); //Popula o stringbuilder XmlFinal.
            string XML = _XmlFinal.ToString().Replace(',', '.').Replace("utf-16", "utf-8");
            return XML;
        }
    }
}
