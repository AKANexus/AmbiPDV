namespace PDV_WPF.Objetos
{
    class SiTEF
    {
        //public int noCupom;
        //public DateTime tsCupom;
        //public byte[] bufferTEF = new byte[22000];
        //public class ParamsDeConfig
        //{
        //    internal string multiplosCupons;
        //    public bool MultiplosCupons
        //    {
        //        set
        //        {
        //            switch (value)
        //            {
        //                case true:
        //                    multiplosCupons = "1";
        //                    break;
        //                default:
        //                    multiplosCupons = "0";
        //                    break;
        //            }
        //        }
        //    }

        //    internal string portaPinPad;
        //    public int PortaPinPad
        //    {
        //        set
        //        {
        //            if (value > 0)
        //            {
        //                portaPinPad = value.ToString();
        //            }
        //            else
        //            {
        //                throw new ErroDeValidacaoTEF("Porta informada era 0 ou negativa.");
        //            }
        //        }
        //    }

        //    internal string lojaECF;
        //    public string LojaECF
        //    {
        //        set
        //        {
        //            if (value.Length > 20) throw new ErroDeValidacaoTEF("Número da LojaECF contém mais que 20 caracteres.");
        //            lojaECF = value;
        //        }
        //    }

        //    internal string caixaECF;
        //    public string CaixaECF
        //    {
        //        set
        //        {
        //            if (value.Length > 20) throw new ErroDeValidacaoTEF("Número do CaixaECF contém mais que 20 caracteres.");
        //            caixaECF = value;
        //        }
        //    }

        //    internal string numeroSerieECF;
        //    public string NumeroSerieECF
        //    {
        //        set
        //        {
        //            if (value.Length > 20) throw new ErroDeValidacaoTEF("Numero de série da ECF contém mais que 20 caracteres.");
        //            numeroSerieECF = value;
        //        }
        //    }

        //    internal string cnpjEstabelecimento;
        //    public string CNPJEstabelecimento
        //    {
        //        set
        //        {
        //            if (value.Length == 14) cnpjEstabelecimento = value;
        //            else if (value.Length == 18) cnpjEstabelecimento = value.Replace(".", "").Replace("-", "").Replace("/", "");
        //        }
        //    }

        //    internal string cpfEstabelecimento;
        //    public string CPFEstabelecimento
        //    {
        //        set
        //        {
        //            if (value.Length == 11) cpfEstabelecimento = value;
        //            else if (value.Length == 14) cpfEstabelecimento = value.Replace(".", "").Replace("-", "");
        //        }
        //    }

        //    internal string cnpjFacilitador;
        //    public string CNPJFacilitador
        //    {
        //        set
        //        {
        //            if (value.Length == 14) cnpjFacilitador = value;
        //            else if (value.Length == 18) cnpjFacilitador = value.Replace(".", "").Replace("-", "").Replace("/", "");
        //        }
        //    }
        //}
        //private Dictionary<int, string> retornoConfSitef = new Dictionary<int, string>()
        //{
        //    {-5, "Sem conexão SiTEF" },
        //    {0, "Operação concluída com sucesso" },
        //    {1, "Endereço IP inválido ou não resolvido" },
        //    {2, "Código da loja inválido" },
        //    {3, "Código de terminal inválido" },
        //    {6, "Erro na inicialização do TCP/IP" },
        //    {7, "OutOfMemory" },
        //    {8, "Não encontrou CliSiTef ou está com problemas" },
        //    {9, "Configuração de servidores SiTef excedida" },
        //    {10, "Erro de acesso na pasta CliSiTef(Permissão de leitura/escrita negada" },
        //    {11, "Dados inválidos passados pelo AC" },
        //    {12, "Modo seguro não ativo (possível falta de config. no servidor SiTef do arquivo .cha" },
        //    {13, "Caminho DLL inválido (o caminho completo das bibliotecas está muito grande" }
        //};
        //public (int intRetorno, string msgRetorno) ConfiguraSitef(string IP, string numLoja, string numTerminal)
        //{
        //    if (IP.Split('.').Count() != 4) return (-100, "IP inválido.");
        //    //if (int.TryParse(numLoja, out int intNumLoja) || intNumLoja <= 0) return (-100, "Identificador da Loja inválido");
        //    //if (int.TryParse(numTerminal.Substring(2), out int intNumTerminal) || intNumTerminal <= 0) return (-100, "Número do Terminal Inválido");
        //    //if (intNumTerminal.IsBetween(900, 999)) return (-100, "Número do Terminal Proibido. Faixa 900 a 999 é reservada.");
        //    int retorno = ConfiguraIntSiTefInterativo(IP, numLoja, numTerminal, "0");
        //    if (retornoConfSitef.ContainsKey(retorno)) return (retorno, retornoConfSitef[retorno]);
        //    else return (-100, "Erro desconhecido");
        //}
        //public (int intRetorno, string msgRetorno) ConfiguraSitef(string IP, string numLoja, string numTerminal, ParamsDeConfig parametros)
        //{
        //    if (IP.Split('.').Count() != 4) return (-100, "IP inválido.");
        //    if (int.TryParse(numLoja, out int intNumLoja) || intNumLoja <= 0) return (-100, "Identificador da Loja inválido");
        //    if (int.TryParse(numTerminal.Substring(2), out int intNumTerminal) || intNumTerminal <= 0) return (-100, "Número do Terminal Inválido");
        //    if (intNumTerminal.IsBetween(900, 999)) return (-100, "Número do Terminal Proibido. Faixa 900 a 999 é reservada.");
        //    StringBuilder strParametros = new StringBuilder();
        //    if (!String.IsNullOrEmpty(parametros.multiplosCupons))
        //    {
        //        strParametros.Append($"MultiplosCupons={parametros.multiplosCupons};");
        //    }
        //    if (!String.IsNullOrEmpty(parametros.portaPinPad))
        //    {
        //        strParametros.Append($"PortaPinPad={parametros.portaPinPad};");
        //    }
        //    /*
        //    if (!String.IsNullOrEmpty(parametros.lojaECF))
        //    {
        //        strParametros.Append($"LojaECF={parametros.lojaECF};");
        //    }
        //    if (!String.IsNullOrEmpty(parametros.caixaECF))
        //    {
        //        strParametros.Append($"CaixaECF={parametros.caixaECF};");
        //    }
        //    if (!String.IsNullOrEmpty(parametros.numeroSerieECF))
        //    {
        //        strParametros.Append($"NumeroSerieECF={parametros.numeroSerieECF};");
        //    }
        //    */
        //    if (!String.IsNullOrEmpty(parametros.cnpjEstabelecimento))
        //    {
        //        strParametros.Append($"ParmsClient=1={parametros.cnpjEstabelecimento};");
        //    }
        //    if (!String.IsNullOrEmpty(parametros.cpfEstabelecimento))
        //    {
        //        strParametros.Append($"ParmsClient=3={parametros.cpfEstabelecimento};");
        //    }
        //    if (!String.IsNullOrEmpty(parametros.cnpjFacilitador))
        //    {
        //        strParametros.Append($"ParmsClient=4={parametros.cnpjFacilitador};");
        //    }
        //    strParametros.Append($"ParmsClient=2=22141365000179;");
        //    int retorno = ConfiguraIntSiTefInterativoEx(IP, numLoja, numTerminal, "0", strParametros.ToString());
        //    if (retornoConfSitef.ContainsKey(retorno)) return (retorno, retornoConfSitef[retorno]);
        //    else return (-100, "Erro desconhecido");
        //}

        //public (int intRetorno, string msgRetorno) IniciaVendaTEF(TipoTEF tipoTEF, decimal vlrTEF)
        //{

        //    int retorno = IniciaFuncaoSiTefInterativo((int)tipoTEF, vlrTEF.ToString("0,00"), noCupom.ToString(), tsCupom.ToString("yyyyMMdd"), tsCupom.ToString("HHmmss"), operador, "");
        //    return (retorno, "SKITTLES");
        //}

        //private (int intRetorno, string msgRetorno) ContinuaVendaTEF()
        //{
        //    int Comando = 0, Continua = 0;
        //    long TipoCampo = 0;
        //    short TamMinimo = 0, TamMaximo = 0;

        //    int retorno = ContinuaFuncaoSiTefInterativo(ref Comando, ref TipoCampo, ref TamMinimo, ref TamMaximo, bufferTEF, bufferTEF.Length, 0);
        //    ProcessaComando(Comando, bufferTEF);
        //    return (retorno, "SHOX");
        //}

        //public void EfetuaPagamentoNoTEF(TipoTEF tipoTEF, decimal vlrTEF)
        //{
        //    if (IniciaVendaTEF(tipoTEF, vlrTEF).intRetorno == 10000)
        //        while (ContinuaVendaTEF().intRetorno == 10000)
        //        {
        //            continue;
        //        }
        //    else
        //        MessageBox.Show("Retorno diferente de 10000");

        //}
        //private void ProcessaComando(int comando, byte[] buffer)
        //{
        //    switch (comando)
        //    {
        //        case 0:
        //            //ArmazenaValor(buffer);
        //            break;
        //        case 1:
        //            ExibeMensagemOperador(buffer);
        //            break;
        //        case 2:
        //            ExibeMensagemCliente(buffer);
        //            break;
        //        case 3:
        //            //ExibeMensagemOperador(buffer);
        //            //ExibeMensagemCliente(buffer);
        //            break;
        //        case 4:
        //            //ArmazenaTituloMenu(buffer);
        //            break;
        //        case 11:
        //            //RemoveMensagemOperador();
        //            break;
        //        case 12:
        //            RemoveMensagemCliente();
        //            break;
        //        case 13:
        //            //RemoveMensagemOperador();
        //            //RemoveMensagemCliente();
        //            break;
        //        case 14:
        //            //LimpaTitulomeno();
        //            break;
        //        case 15:
        //            //SalvaCabecalho(buffer);
        //            break;
        //        case 16:
        //            //RemoveCabecalho();
        //            break;
        //        case 20:
        //            //PerguntaSimOuNao(buffer);
        //            break;
        //        case 21:
        //            //ExibeMenuDeOpcoes(buffer);
        //            break;
        //        case 22:
        //            //ExibeMensagemDeAlerta(buffer);
        //            break;
        //        case 23:
        //            //AguardaInterrupcao();
        //            break;
        //        case 29:
        //            //ColetaCampoInterno();
        //            break;
        //        case 30:
        //            //ColetaCampoUsuario();
        //            break;
        //        case 31:
        //            //LeNumeroCheque();
        //            break;
        //        case 34:
        //            //LeDinheiro();
        //            break;
        //        case 35:
        //            //LeCodigoDeBarras();
        //            break;
        //        case 41:
        //            //ColetaCampoUsuarioMasked();
        //            break;
        //        case 42:
        //            //ExibeMenu(buffer);
        //            break;
        //    }
        //}

        //private void RemoveMensagemCliente()
        //{
        //    return;
        //}

        //private void RemoveMensagemOperador()
        //{
        //    //avisos.Texto = "";
        //}

        //private void ExibeMensagemCliente(byte[] buffer)
        //{
        //    return;
        //}

        //private void ExibeMensagemOperador(byte[] buffer)
        //{
        //    //avisos.Texto = Encoding.UTF8.GetString(buffer);
        //}
    }
}
