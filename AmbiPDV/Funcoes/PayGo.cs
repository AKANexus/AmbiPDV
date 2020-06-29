using System;
using System.Collections.Generic;
using System.IO;
using static PDV_WPF.Funcoes.Statics;

namespace PayGo
{
    internal static class Estaticos
    {
        //public enum Parcelamento { Qualquer, AVista, ParceladoEmissor, ParceladoEstabelecimento, PreDatado, PreDatadoForcado };
        public static readonly string path2 = @"C:\PAYGO\Resp\intpos.001";

    }

    internal class General
    {
        public static bool LimpaCom()
        {
            DirectoryInfo resp = new DirectoryInfo(@"C:\PAYGO\Resp");
            DirectoryInfo req = new DirectoryInfo(@"C:\PAYGO\Req");
            try
            {
                foreach (FileInfo file in resp.GetFiles())
                {
                    file.Delete();
                }
                foreach (FileInfo file in req.GetFiles())
                {
                    file.Delete();
                }
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }
        public static Dictionary<string, string> LeResposta(string caminho = @"C:\PAYGO\Resp\intpos.001")
        {
            Dictionary<string, string> resultado = new Dictionary<string, string>();
            resultado.Clear();
            System.Threading.Thread.Sleep(500);
            using (StreamReader sr = new StreamReader(caminho))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    resultado.Add(line.Substring(0, 7), line.Substring(10).Replace("\"", String.Empty));
                    //audit("LE RESPOSTA PAYGO", line);
                }
            }
            return resultado;
        }

    }

    /// <summary>
    /// Verifica se o PayGo está ativo
    /// </summary>
    internal class ATV
    {
        public static bool Exec()
        {
            General.LimpaCom();
            //System.Threading.Thread.Sleep(500);
            string _path1 = @"C:\PAYGO\Req\intpos.tmp";
            string path1 = @"C:\PAYGO\Req\intpos.001";
            string path3 = @"C:\PAYGO\Resp\intpos.sts";
            try
            {
                using (StreamWriter sw = File.CreateText(_path1))
                {
                    sw.WriteLine("000-000 = ATV");
                    sw.WriteLine(String.Format("001-000 = {0}", String.Format("{0:HHmmssff}", DateTime.Now)));
                    sw.WriteLine(String.Format("733-000 = 215"));
                    sw.WriteLine(String.Format("738-000 = REGISTRODECERTIFICACAO"));
                    sw.WriteLine(String.Format("999-999 = 0"));
                }
                File.Move(_path1, path1);
            }
            catch (Exception)
            {
                return false;
            }


            for (int i = 0; i < 28; i++)
            {
                if (File.Exists(path3))
                {
                    return true;
                }
                System.Threading.Thread.Sleep(250);

            }
            return false;
        }

    }

    /// <summary>
    /// Realiza transação de venda
    /// </summary>
    internal class CRT
    {
        public Dictionary<string, string> resultado = new Dictionary<string, string>();
        //public enum moeda { Real, Dolar, Euro }
        public enum cliente { CPF, CNPJ, INTERNO }
        /// <summary>
        /// Número do documento fiscal
        /// </summary>
        public int? _002 { get; set; }
        /// <summary>
        /// Valor Total do pagamento
        /// </summary>
        public decimal _003 { get; set; }
        /// <summary>
        /// Entidade cliente. F para CPF, J para CNPJ, X para outro identificador
        /// </summary>
        public cliente? _006 { get; set; }
        /// <summary>
        /// Identificador do cliente (CPF, CNPJ ou identificador da loja)
        /// </summary>
        public string _007 { get; set; }
        /// <summary>
        /// Tipo de transação. Consulte a página 31 do manual Pay&Go (rev 2.15)
        /// </summary>
        public string _011 { get; set; }
        /// <summary>
        /// Quantidade de parcelas
        /// </summary>
        public int? _018 { get; set; }
        /// <summary>
        /// Data de agendamento da transação
        /// </summary>
        public int? _024 { get; set; }
        /// <summary>
        /// Valor do troco
        /// </summary>
        public decimal _708 { get; set; }
        /// <summary>
        /// Valor do desconto
        /// </summary>
        public decimal _709 { get; set; }
        /// <summary>
        /// Data e hora do cupom fiscal
        /// </summary>
        public DateTime? _717 { get; set; }
        /// <summary>
        /// Número lógico do terminal
        /// </summary>
        public int? _718 { get; set; }
        /// <summary>
        /// Tipo de operação. Consulte a página 35 do manual Pay&Go (rev 2.15)
        /// </summary>
        public string _730 { get; set; }
        /// <summary>
        /// Tipo de cartão. Consulte a página 35 do manual Pay&Go (rev 2.15)
        /// </summary>
        public string _731 { get; set; }
        /// <summary>
        /// Tipo de financiamento. Consulte a página 35 do manual Pay&Go (rev 2.15)
        /// </summary>
        public string _732 { get; set; }

        public Dictionary<string, string> Exec()
        {
            General.LimpaCom();
            //System.Threading.Thread.Sleep(500);
            string _path1 = @"C:\PAYGO\Req\intpos.tmp";
            string path1 = @"C:\PAYGO\Req\intpos.001";
            try
            {
                using (StreamWriter sw = File.CreateText(_path1))
                {
                    sw.WriteLine("000-000 = CRT");
                    sw.WriteLine(String.Format("001-000 = {0}", String.Format("{0:HHMmssff}", DateTime.Now)));
                    if (_002 != null)
                    {
                        sw.WriteLine(String.Format("002-000 = {0}", _002.ToString()));
                    }
                    sw.WriteLine(String.Format("003-000 = {0}", (_003 * 100).ToString("0")));
                    sw.WriteLine(String.Format("004-000 = 0"));
                    switch (_006)
                    {
                        case cliente.CNPJ:
                            sw.WriteLine(String.Format("006-000 = J"));
                            break;
                        case cliente.CPF:
                            sw.WriteLine(String.Format("006-000 = F"));
                            break;
                        case cliente.INTERNO:
                            sw.WriteLine(String.Format("006-000 = X"));
                            break;
                        default:
                            break;
                    }
                    if (_007 != null)
                    {
                        sw.WriteLine(String.Format("007-000 = {0}", _007));
                    }
                    if (_011 != null)
                    {
                        sw.WriteLine(String.Format("011-000 = {0}", _011));
                    }
                    if (_018 != null)
                    {
                        sw.WriteLine(String.Format("018-000 = {0}", _018));
                    }
                    sw.WriteLine(String.Format("706-000 = 140"));
                    sw.WriteLine(String.Format("716-000 = AMBISOF TECNOLOGIAS")); //TODO: Lembrar de tirar antes de publicar... ☺
                    if (_717 != null)
                    {
                        sw.WriteLine(String.Format("717-000 = {0}", String.Format("{0:yyMMddHHmmss}", _717)));
                    }
                    if (_718 != null)
                    {
                        sw.WriteLine(String.Format("718-000 = {0}", _718.ToString()));
                    }
                    sw.WriteLine(String.Format("726-000 = pt"));
                    //sw.WriteLine(String.Format("727-000 = {0}", (_727*100).ToString()));
                    sw.WriteLine(String.Format("730-000 = {0}", _730.ToString()));
                    sw.WriteLine(String.Format("731-000 = {0}", _731.ToString()));
                    sw.WriteLine(String.Format("732-000 = {0}", _732.ToString()));
                    sw.WriteLine(String.Format("733-000 = 215"));
                    sw.WriteLine(String.Format("735-000 = AMBIPDV"));
                    sw.WriteLine(String.Format($"736-000 = {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"));
                    sw.WriteLine(String.Format("738-000 = 5470"));
                    sw.WriteLine(String.Format("999-999 = 0"));
                }
                File.Copy(_path1, path1);
            }
            catch (Exception /*ex*/)
            {
                resultado.Add("XXX-XXX", "Impossível executar a venda.");
                return resultado;
            }
            resultado.Add("XXX-XXX", "Impossível executar a venda.");
            return resultado;
        }
    }

    /// <summary>
    /// Confirma a última transação realizada
    /// </summary>
    internal class CNF
    {
        public enum cliente { CPF, CNPJ, INTERNO }
        public string _002 { get; set; }
        public string _010 { get; set; }
        public string _027 { get; set; }//Código de Controle - ¡¡¡¡Obrigatório ser informado!!!!!
        public DateTime? _717 { get; set; }

        public Dictionary<string, string> Exec()
        {
            General.LimpaCom();
            string _path1 = @"C:\PAYGO\Req\intpos.tmp";
            string path1 = @"C:\PAYGO\Req\intpos.001";
            Dictionary<string, string> resultado = new Dictionary<string, string>();
            try
            {
                using (StreamWriter sw = File.CreateText(_path1))
                {
                    sw.WriteLine("000-000 = CNF");
                    sw.WriteLine(String.Format("001-000 = {0}", String.Format("{0:HHmmssff}", DateTime.Now)));
                    if (_002 != null)
                    {
                        sw.WriteLine(String.Format("002-000 = {0}", _002));
                    }
                    sw.WriteLine(String.Format("010-000 = {0}", _010));
                    sw.WriteLine(String.Format("027-000 = {0}", _027));
                    if (_717 != null)
                    {
                        sw.WriteLine(String.Format("717-000 = {0}", String.Format("{0:yyMMddHHmmss}", _717)));
                    }
                    sw.WriteLine(String.Format("733-000 = 215"));
                    sw.WriteLine(String.Format("735-000 = AMBIPDV"));
                    sw.WriteLine(String.Format($"736-000 = {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"));
                    sw.WriteLine(String.Format("738-000 = REGISTRODECERTIFICACAO"));
                    sw.WriteLine(String.Format("999-999 = 0"));
                }
                File.Move(_path1, path1);
            }
            catch (Exception)
            {
                resultado.Add("XXX-XXX", "Impossível confirmar a venda.");
                return resultado;
            }
            resultado.Add("XXX-XXX", "Impossível confirmar a venda.");
            return resultado;
        }


    }

    /// <summary>
    /// Desfaz a última transação realizada
    /// </summary>
    internal class NCN
    {
        public int? _002 { get; set; }
        public string _010 { get; set; }
        public string _027 { get; set; }//Código de Controle - ¡¡¡Obrigatório ser informado!!!!!
        public DateTime? _717 { get; set; }

        public Dictionary<string, string> Exec()
        {
            General.LimpaCom();
            string _path1 = @"C:\PAYGO\Req\intpos.tmp";
            string path1 = @"C:\PAYGO\Req\intpos.001";
            Dictionary<string, string> resultado = new Dictionary<string, string>();
            try
            {
                using (StreamWriter sw = File.CreateText(_path1))
                {
                    sw.WriteLine("000-000 = NCN");
                    sw.WriteLine(String.Format("001-000 = {0}", String.Format("{0:HHmmssff}", DateTime.Now)));
                    if (_002 != null)
                    {
                        sw.WriteLine(String.Format("002-000 = {0}", _002.ToString()));
                    }
                    sw.WriteLine(String.Format("010-000 = {0}", _010));
                    sw.WriteLine(String.Format("027-000 = {0}", _027));
                    if (_717 != null)
                    {
                        sw.WriteLine(String.Format("717-000 = {0}", String.Format("{0:yyMMddHHmmss}", _717)));
                    }
                    sw.WriteLine(String.Format("733-000 = 215"));
                    sw.WriteLine(String.Format("735-000 = AMBIPDV"));
                    sw.WriteLine(String.Format($"736-000 = {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"));
                    sw.WriteLine(String.Format("738-000 = REGISTRODECERTIFICACAO"));
                    sw.WriteLine(String.Format("999-999 = 0"));
                }
                File.Move(_path1, path1);
            }
            catch (Exception)
            {
                resultado.Add("XXX-XXX", "Impossível confirmar a venda.");
                return resultado;
            }
            resultado.Add("XXX-XXX", "Impossível confirmar a venda.");
            return resultado;
        }


    }

    /// <summary>
    /// Operação de Cancelamento
    /// </summary>
    internal class CNC
    {
        public Dictionary<string, string> resultado = new Dictionary<string, string>();
        //public enum moeda { Real, Dolar, Euro }
        public enum cliente { CPF, CNPJ, INTERNO }
        /// <summary>
        /// Número do documento fiscal ao qual a operação de TEF está vinculada. (opcional)
        /// </summary>
        public string _002 { get; set; }
        /// <summary>
        /// Valor total da operação, em reais da moeda informada, incluindo todas as taxas cobradas do cliente. (obrigatório)
        /// </summary>
        public string _003 { get; set; }
        //public static moeda _004 { get; set; }
        /// <summary>
        /// Entidade cliente (CPF, ou CPNJ) (opcional)
        /// </summary>
        public cliente? _006 { get; set; }
        /// <summary>
        /// Informação, referente ao campo _006 (obrigatório se _006 for preenchido)
        /// </summary>
        public string _007 { get; set; }
        /// <summary>
        /// Codinome da Rede Adquirente (preferível preencher o campo _739 com o índice).
        /// </summary>
        public string _010 { get; set; } // Esse ou 739-000 são obrigatórios.
        /// <summary>
        /// Índice da Rede Adquirente (obrigatório)
        /// </summary>
        public string _739 { get; set; }
        /// <summary>
        /// NSU da operação a ser cancelada (obrigatório, claro)
        /// </summary>
        public string _012 { get; set; } // Identificador da operação (NSU)
        public string _013 { get; set; } // Presente se for retornado pela CRT.
        public int? _018 { get; set; } // Presente se for parcelado.
        //public string _022 { get; set; } // Data no comprovante
        //public string _023 { get; set; } // Hora do comprovante
        public int? _024 { get; set; } // Presente se pré-datado
        /// <summary>
        /// Data e hora fiscal (obrigatório?)
        /// </summary>
        public DateTime? _717 { get; set; }



        public Dictionary<string, string> Exec()
        {
            General.LimpaCom();
            //System.Threading.Thread.Sleep(500);
            string _path1 = @"C:\PAYGO\Req\intpos.tmp";
            string path1 = @"C:\PAYGO\Req\intpos.001";
            try
            {
                using (StreamWriter sw = File.CreateText(_path1))
                {
                    sw.WriteLine("000-000 = CNC");
                    sw.WriteLine(String.Format("001-000 = {0}", String.Format("{0:HHMmssff}", DateTime.Now)));
                    if (!(_002 is null))
                    {
                        sw.WriteLine($"002-000 = {_002}");
                    }
                    //sw.WriteLine(String.Format("003-000 = {0}", (_003 * 100).ToString("G29")));
                    sw.WriteLine(String.Format("003-000 = {0}", _003));
                    sw.WriteLine(String.Format("004-000 = 0"));
                    switch (_006)
                    {
                        case cliente.CNPJ:
                            sw.WriteLine(String.Format("006-000 = J"));
                            break;
                        case cliente.CPF:
                            sw.WriteLine(String.Format("006-000 = F"));
                            break;
                        case cliente.INTERNO:
                            sw.WriteLine(String.Format("006-000 = X"));
                            break;
                        default:
                            break;
                    }
                    if (_007 != null)
                    {
                        sw.WriteLine(String.Format("007-000 = {0}", _007));
                    }
                    sw.WriteLine(String.Format("012-000 = {0}", _012));
                    sw.WriteLine($"022-000 = {DateTime.Now:ddMMyyyy}");
                    sw.WriteLine(String.Format("023-000 = {0}", DateTime.Now.ToString("hhmmss")));
                    sw.WriteLine(String.Format("706-000 = 156"));
                    sw.WriteLine(String.Format("716-000 = A MELHOR EMPRESA DO BAIRRO")); //TODO: Lembrar de tirar antes de publicar... ☺
                    if (_717 != null)
                    {
                        sw.WriteLine(String.Format("717-000 = {0}", String.Format("{0:yyMMddHHmmss}", _717)));
                    }
                    sw.WriteLine(String.Format("726-000 = pt"));
                    //sw.WriteLine(String.Format("727-000 = {0}", (_727*100).ToString()));
                    sw.WriteLine(String.Format("733-000 = 215"));
                    sw.WriteLine(String.Format("735-000 = AMBIPDV"));
                    sw.WriteLine(String.Format($"736-000 = {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"));
                    sw.WriteLine(String.Format("738-000 = REGISTRODECERTIFICACAO"));
                    if (_010 is null)
                    {
                        sw.WriteLine(String.Format("739-000 = {0}", _739));
                    }
                    sw.WriteLine(String.Format("999-999 = 0"));
                }
                File.Copy(_path1, path1);
            }
            catch (Exception)
            {
                resultado.Add("XXX-XXX", "Impossível executar o cancelamento.");
                return resultado;
            }
            resultado.Add("XXX-XXX", "Impossível executar o cancelamento.");
            return resultado;
        }
    }

    /// <summary>
    /// Operação adminstrativa
    /// </summary>
    internal class ADM
    {
        public Dictionary<string, string> resultado = new Dictionary<string, string>();
        //public enum moeda { Real, Dolar, Euro }
        public enum cliente { CPF, CNPJ, INTERNO }
        public int? _002 { get; set; }
        public decimal _003 { get; set; }
        //public static moeda _004 { get; set; }
        public cliente? _006 { get; set; }
        public string _007 { get; set; }
        public string _010 { get; set; } // Esse ou 739-000 são obrigatórios.
        public string _012 { get; set; } // Identificador da operação (NSU)
        public string _013 { get; set; } // Presente se for retornado pela CRT.
        public int? _018 { get; set; } // Presente se for parcelado.
        public string _022 { get; set; } // Data no comprovante
        public string _023 { get; set; } // Hora do comprovante
        public int? _024 { get; set; } // Presente se pré-datado
        public DateTime? _717 { get; set; }



        public Dictionary<string, string> Exec()
        {
            General.LimpaCom();
            //System.Threading.Thread.Sleep(500);
            string _path1 = @"C:\PAYGO\Req\intpos.tmp";
            string path1 = @"C:\PAYGO\Req\intpos.001";
            try
            {
                using (StreamWriter sw = File.CreateText(_path1))
                {
                    sw.WriteLine("000-000 = ADM");
                    sw.WriteLine(String.Format("001-000 = {0}", String.Format("{0:HHMmssff}", DateTime.Now)));
                    if (_002 != null)
                    {
                        sw.WriteLine(String.Format("002-000 = {0}", _002.ToString()));
                    }
                    switch (_006)
                    {
                        case cliente.CNPJ:
                            sw.WriteLine(String.Format("006-000 = J"));
                            break;
                        case cliente.CPF:
                            sw.WriteLine(String.Format("006-000 = F"));
                            break;
                        case cliente.INTERNO:
                            sw.WriteLine(String.Format("006-000 = X"));
                            break;
                        default:
                            break;
                    }
                    if (_007 != null)
                    {
                        sw.WriteLine(String.Format("007-000 = {0}", _007));
                    }
                    //sw.WriteLine(String.Format("022-000 = {0}", _022));
                    //sw.WriteLine(String.Format("023-000 = {0}", _023));
                    sw.WriteLine(String.Format("706-000 = 156"));
                    sw.WriteLine(String.Format("716-000 = A MELHOR EMPRESA DO BAIRRO")); //TODO: Lembrar de tirar antes de publicar... ☺
                    if (_717 != null)
                    {
                        sw.WriteLine(String.Format("717-000 = {0}", String.Format("{0:yyMMddHHmmss}", _717)));
                    }
                    sw.WriteLine(String.Format("726-000 = pt"));
                    //sw.WriteLine(String.Format("727-000 = {0}", (_727*100).ToString()));
                    sw.WriteLine(String.Format("733-000 = 215"));
                    sw.WriteLine(String.Format("735-000 = AMBIPDV"));
                    sw.WriteLine(String.Format($"736-000 = {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"));
                    sw.WriteLine(String.Format("738-000 = REGISTRODECERTIFICACAO"));
                    sw.WriteLine(String.Format("999-999 = 0"));
                }
                File.Move(_path1, path1);
            }
            catch (Exception)
            {
                resultado.Add("XXX-XXX", "Impossível executar a venda.");
                return resultado;
            }
            resultado.Add("XXX-XXX", "Impossível executar a venda.");
            return resultado;
        }
    }

    /// <summary>
    /// Captura dado no PIN-PAD
    /// </summary>
    internal class CDP
    {
        public Dictionary<string, string> resultado = new Dictionary<string, string>();
        public enum cliente { CPF, CNPJ, INTERNO }
        public int? _002 { get; set; }
        public decimal _003 { get; set; }
        public cliente? _006 { get; set; }
        public string _007 { get; set; }
        public string _010 { get; set; } // Esse ou 739-000 são obrigatórios.
        public string _012 { get; set; } // Identificador da operação (NSU)
        public string _013 { get; set; } // Presente se for retornado pela CRT.
        public int? _018 { get; set; } // Presente se for parcelado.
        public string _022 { get; set; } // Data no comprovante
        public string _023 { get; set; } // Hora do comprovante
        public int? _024 { get; set; } // Presente se pré-datado
        public DateTime? _717 { get; set; }



        public string Exec()
        {
            General.LimpaCom();
            //System.Threading.Thread.Sleep(500);
            string _path1 = @"C:\PAYGO\Req\intpos.tmp";
            string path1 = @"C:\PAYGO\Req\intpos.001";
            try
            {
                using (StreamWriter sw = File.CreateText(_path1))
                {
                    sw.WriteLine("000-000 = CDP");
                    sw.WriteLine(String.Format("001-000 = {0}", String.Format("{0:HHMmssff}", DateTime.Now)));
                    _006 = cliente.CPF;
                    switch (_006)
                    {
                        case cliente.CNPJ:
                            sw.WriteLine(String.Format("006-000 = J"));
                            break;
                        case cliente.CPF:
                            sw.WriteLine(String.Format("006-000 = F"));
                            break;
                        case cliente.INTERNO:
                            sw.WriteLine(String.Format("006-000 = X"));
                            break;
                        default:
                            break;
                    }
                    sw.WriteLine(String.Format("706-000 = 156"));
                    sw.WriteLine(String.Format("716-000 = A MELHOR EMPRESA DO BAIRRO")); //TODO: Lembrar de tirar antes de publicar... ☺
                    sw.WriteLine(String.Format("726-000 = pt"));
                    sw.WriteLine(String.Format("733-000 = 215"));
                    sw.WriteLine(String.Format("735-000 = AMBIPDV"));
                    sw.WriteLine(String.Format($"736-000 = {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"));
                    sw.WriteLine(String.Format("738-000 = REGISTRODECERTIFICACAO"));
                    sw.WriteLine(String.Format("999-999 = 0"));
                }
                File.Move(_path1, path1);
            }
            catch (Exception)
            {
                resultado.Add("XXX-XXX", "Impossível obter o dado.");
                return "";
            }
            return "";
        }
    }

}
