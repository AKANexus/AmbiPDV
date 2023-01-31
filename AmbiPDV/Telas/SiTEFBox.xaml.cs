using Clearcove.Logging;
using PDV_WPF.Funcoes;
using PDV_WPF.Objetos;
using PDV_WPF.Objetos.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.SiTEFDLL;
using static PDV_WPF.Funcoes.Statics;

#pragma warning disable CS4014
namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for SiTEFBox.xaml
    /// </summary>
    public partial class SiTEFBox : Window, INotifyPropertyChanged
    {
        enum DllState { Listening, Speaking }
        DllState estadoDaDll = DllState.Listening;
        private StatusTEF statusAtual = StatusTEF.Aberto;
        public StatusTEF Status
        {
            get
            {
                return statusAtual;
            }
            set
            {
                statusAtual = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("status"));
                StatusChanged?.Invoke(this, new TEFEventArgs());
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private StateTEF estadoTEF = StateTEF.OperacaoPadrao;
        private string campoDigitado = "";
        private decimal valorDigitado;
        private string tituloMenu = "", tituloJanela = "OPERAÇÃO NO TEF", mensagemJanela = "";
        private int Comando = 0, Continua = 0, _idMetodo;
        private string numCupom;
        private long TipoCampo = 0;
        private short TamMinimo = 0, TamMaximo = 0;
        public DateTime tsCupom;
        public byte[] bufferTEF;
        private DateTime tsFiscal;
        private bool IsSilent;
        private bool PermiteCancelar = false;
        public SiTEFBox()
        {
            InitializeComponent();
            DataContext = this;
        }
        public List<Pendencia> pendenciasList;
        public List<string> _viaCliente = new List<string>();
        public List<string> _viaLoja = new List<string>();
        public string numPagamentoTEF;
        public string _nsu;
        private decimal valor;
        private TipoTEF _tipoTEF;

        private Logger log = new Logger("SITEF");


        private void Window_Activated(object sender, EventArgs e)
        {
            if (this.Owner != null)
            {
                //this.Owner.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.Owner != null)
            {
                //this.Owner.IsEnabled = true;
            }
        }

        public List<Pendencia> ListaPendenciasDoTEF()
        {
            pendenciasList = new List<Pendencia>();
            InitializeComponent();
            bufferTEF = new byte[22000];
            _tipoTEF = TipoTEF.PendenciasTerminal;
            IniciaFuncaoSiTefInterativo(130, "0,00", "0", DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("HHmmss"), operador, "");
            int retorno = 10000;
            while (retorno == 10000)
            {

                //log.Debug("estadoTEF == OperacaoPadrao");
                retorno = ContinuaVendaTEF();

            }
            string msgErro = retorno switch
            {
                -1 => "Módulo não inicializado. O PDV tentou chamar alguma rotina sem antes executar a função configura.",
                -3 => "O parâmetro função/modalidade é inexistente/inválido.",
                -4 => "Falta de memória no PDV",
                -5 => "Sem conexão SiTEF (-5)",
                -8 => "CliSiTef não possui a implementação da função necessária. Verifique por atualizações da CliSiTef",
                -9 => "ContinuaFuncaoInterativo foi chamado antes de IniciaFuncaoInterativo",
                -10 => "Parâmetro obrigatório não foi passado",
                -12 => "Processo Interativo anterior não foi concluído até o talo",
                -20 => "Parâmetro inválido passado para a função",
                //-40 => "Transação negada pelo servidor SiTef",
                _ => "NAE" //Not an Error
            };
            if (msgErro != "NAE") this.Dispatcher.Invoke(() => DialogBox.Show("ERRO DE TEF", DialogBoxButtons.No, DialogBoxIcons.Error, true, $"{msgErro}"));
            //FinalizaOperacaoTEF();
            else statusAtual = StatusTEF.Confirmado;


            this.Dispatcher.Invoke(() => this.Close());
            StatusChanged?.Invoke(this, new TEFEventArgs() { TipoDoTEF = _tipoTEF, Valor = valor, idMetodo = _idMetodo, status = statusAtual, viaCliente = _viaCliente, viaLoja = _viaLoja, pendenciasXML = pendenciasList });
            return pendenciasList;


        }
        public void ShowTEF(TipoTEF tipoTEF, decimal vlrTEF, string noCupom, DateTime tsCupom, int idMetodo, bool silent = false)
        {
            IsSilent = silent;
            _idMetodo = idMetodo;
            valor = vlrTEF;
            _tipoTEF = tipoTEF;
            tsFiscal = tsCupom;
            numCupom = noCupom;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Left = 0;
            Top = 0;
            Topmost = true;
            InitializeComponent();
            if (!silent) Show();
            //audit("SITEFBOX", $"Iniciando uma transação do tipo {(int)tipoTEF} - {tipoTEF}");
            //log.Debug($"Iniciando nova transação do tipo {(int)tipoTEF} - {tipoTEF}");
            IniciaFuncaoSiTefInterativo((int)tipoTEF, $"{vlrTEF:F2}", numCupom, tsFiscal.ToString("yyyyMMdd"), tsFiscal.ToString("HHmmss"), operador, "");
            statusAtual = StatusTEF.EmAndamento;
            progressIndicator = new Progress<(string body, string title)>(AtualizaUI);
            bufferTEF = new byte[22000];
            GCHandle gCBuffer = GCHandle.Alloc(bufferTEF, GCHandleType.Pinned);
            ComunicaComTEFAsync(progressIndicator);
        }
        Progress<(string, string)> progressIndicator;
        public void FinalizaOperacaoTEF(string numPag, bool estorno = false)
        {
            //estadoTEF = StateTEF.OperacaoPadrao;
            FinalizaFuncaoSiTefInterativo(estorno ? (short)0 : (short)1, numCupom.ToString(), tsFiscal.ToString("yyyyMMdd"), tsFiscal.ToString("HHmmss"), $"NumeroPagamentoCupom={numPag}");
            //ComunicaComTEF(progressIndicator);
        }
        int chamada = 0;
        private async Task<int> ComunicaComTEFAsync(IProgress<(string body, string title)> progress)
        {
            chamada++;
            Console.WriteLine($"Chamada {chamada} << Novo ciclo de comunicação");
            log.Debug($"Chamada {chamada} << Novo ciclo de comunicação");
            //log.Debug("Iniciando ciclo de comunicação com o TEF");
            int retorno = 10000;
            return await Task.Run<int>(() =>
            {
                while (retorno == 10000)
                {
                    if (progress != null)
                        progress.Report((mensagemJanela, tituloJanela));
                    if (estadoTEF != StateTEF.CancelamentoRequisitado)
                    {
                        if (estadoTEF != StateTEF.OperacaoPadrao && estadoTEF != StateTEF.RetornaMenuAnterior)
                        {
                            Console.WriteLine($"Chamada {chamada} >> Aguardando Interação de Usuário");
                            log.Debug($"Chamada {chamada} >> Aguardando Interação de Usuário");
                            return 0;
                        }
                        else
                        {
                            Console.WriteLine($"Chamada {chamada} == Continuando Operação");
                            log.Debug($"Chamada {chamada} == Continuando Operação");
                            retorno = ContinuaVendaTEF();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Chamada {chamada} == Cancelando Operação");
                        log.Debug($"Chamada {chamada} == Cancelando Operação");
                        statusAtual = StatusTEF.Cancelado;
                        retorno = CancelaOperacaoAtual();
                    }
                }
                string msgErro = retorno switch
                {
                    -1 => "Módulo não inicializado. O PDV tentou chamar alguma rotina sem antes executar a função configura.",
                    -3 => "O parâmetro função/modalidade é inexistente/inválido.",
                    -4 => "Falta de memória no PDV",
                    -5 => "Sem conexão SiTEF (-5)",
                    -8 => "CliSiTef não possui a implementação da função necessária. Verifique por atualizações da CliSiTef",
                    -9 => "ContinuaFuncaoInterativo foi chamado antes de IniciaFuncaoInterativo",
                    -10 => "Parâmetro obrigatório não foi passado",
                    -12 => "Processo Interativo anterior não foi concluído até o talo",
                    -20 => "Parâmetro inválido passado para a função",
                    -40 => "Transação negada pelo servidor SiTef",
                    _ => ""
                };
                if (retorno.IsBetween(0, 10000, false))
                {
                    statusAtual = StatusTEF.NaoAutorizado;
                }
                if (retorno < 0) this.Dispatcher.Invoke(() =>
                {
                    //DialogBox.Show("ERRO DE TEF", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Error, true, $"{msgErro}");
                    statusAtual = StatusTEF.Erro;
                });
                PendenciasDoTEF pendTefObj = new PendenciasDoTEF();
                //FinalizaOperacaoTEF();
                if (!(new[] { TipoTEF.Administrativo, TipoTEF.PendenciasTerminal }.Contains(_tipoTEF)) && !(numPagamentoTEF is null))
                {
                    pendTefObj.AdicionaPendenciaNoXML(numCupom.ToString(), numPagamentoTEF.ToString(), tsFiscal.ToString("yyyyMMdd"), tsFiscal.ToString("HHmmss"), ((int)_tipoTEF).ToString("00"), (valor).ToString("0.00", CultureInfo.InvariantCulture), _nsu, _tipoTEF);
                }
                else if (statusAtual != StatusTEF.Cancelado && statusAtual != StatusTEF.Erro && statusAtual != StatusTEF.NaoAutorizado)
                {
                    statusAtual = StatusTEF.Confirmado;
                }
                this.Dispatcher.Invoke(() => this.Close());
                StatusChanged?.Invoke(this, new TEFEventArgs() { TipoDoTEF = _tipoTEF, Valor = valor, idMetodo = _idMetodo, status = statusAtual, viaCliente = _viaCliente, viaLoja = _viaLoja, NoCupom = numCupom });
                Console.WriteLine($"Chamada {chamada} >> Fim do ciclo de comunicação ");
                log.Debug($"Chamada {chamada} >> Fim do ciclo de comunicação ");
                return 0;
            });
        }

        private readonly Dictionary<int, string> retornoConfSitef = new Dictionary<int, string>()
        {
            {-5, "Sem conexão SiTEF" },
            {0, "Operação concluída com sucesso" },
            {1, "Endereço IP inválido ou não resolvido" },
            {2, "Código da loja inválido" },
            {3, "Código de terminal inválido" },
            {6, "Erro na inicialização do TCP/IP" },
            {7, "OutOfMemory" },
            {8, "Não encontrou CliSiTef ou está com problemas" },
            {9, "Configuração de servidores SiTef excedida" },
            {10, "Erro de acesso na pasta CliSiTef(Permissão de leitura/escrita negada" },
            {11, "Dados inválidos passados pelo AC" },
            {12, "Modo seguro não ativo (possível falta de config. no servidor SiTef do arquivo .cha" },
            {13, "Caminho DLL inválido (o caminho completo das bibliotecas está muito grande" }
        };



        public event EventHandler<TEFEventArgs> StatusChanged;

        public (int intRetorno, string msgRetorno) ConfiguraSitef(string IP, string numLoja, string numTerminal)
        {
            if (IP.Split('.').Count() != 4) return (-100, "IP inválido.");
            //if (int.TryParse(numLoja, out int intNumLoja) || intNumLoja <= 0) return (-100, "Identificador da Loja inválido");
            //if (int.TryParse(numTerminal.Substring(2), out int intNumTerminal) || intNumTerminal <= 0) return (-100, "Número do Terminal Inválido");
            //if (intNumTerminal.IsBetween(900, 999)) return (-100, "Número do Terminal Proibido. Faixa 900 a 999 é reservada.");
            int retorno = ConfiguraIntSiTefInterativo(IP, numLoja, numTerminal, "0");
            if (retornoConfSitef.ContainsKey(retorno)) return (retorno, retornoConfSitef[retorno]);
            else return (-100, "Erro desconhecido");
        }
        public (int intRetorno, string msgRetorno) ConfiguraSitef(string IP, string numLoja, string numTerminal, ParamsDeConfig parametros)
        {
            if (IP.Split('.').Count() != 4) return (-100, "IP inválido.");
            //if (int.TryParse(numLoja, out int intNumLoja) || intNumLoja <= 0) return (-100, "Identificador da Loja inválido");
            //if (int.TryParse(numTerminal.Substring(2), out int intNumTerminal) || intNumTerminal <= 0) return (-100, "Número do Terminal Inválido");
            //if (intNumTerminal.IsBetween(900, 999)) return (-100, "Número do Terminal Proibido. Faixa 900 a 999 é reservada.");
            StringBuilder strParametros = new StringBuilder();
            if (!String.IsNullOrEmpty(parametros.multiplosCupons))
            {
                strParametros.Append($"MultiplosCupons={parametros.multiplosCupons};");
            }
            if (!String.IsNullOrEmpty(parametros.portaPinPad))
            {
                strParametros.Append($"PortaPinPad={parametros.portaPinPad};");
            }
            //if (homologaTEF)
            //{
            //    parametros.cnpjEstabelecimento = "31406434895111";
            //    cNPJSH = "12523654185985";
            //}
            //else
            //{
            if (File.Exists(@$"{AppDomain.CurrentDomain.BaseDirectory}\TEFCONFIG.ini"))
            {
                var TEFCONFIG = File.ReadAllLines($@"{AppDomain.CurrentDomain.BaseDirectory}\TEFCONFIG.ini");
                Dictionary<string, string> TEFCONFIGdict = new Dictionary<string, string>();
                foreach (var linha in TEFCONFIG)
                {
                    TEFCONFIGdict.Add(linha.Split('=')[0], linha.Split('=')[1]);
                }
                parametros.cnpjEstabelecimento = TEFCONFIGdict["CNPJ_ESTABELECIMENTO"];
            }
            else parametros.cnpjEstabelecimento = Emitente.CNPJ.TiraPont();

            //}
            if (!String.IsNullOrEmpty(parametros.cnpjEstabelecimento))
            {
                strParametros.Append($"ParmsClient=1={parametros.cnpjEstabelecimento};");
            }
            if (!String.IsNullOrEmpty(parametros.cpfEstabelecimento))
            {
                strParametros.Append($"3={parametros.cpfEstabelecimento};");
            }
            if (!String.IsNullOrEmpty(parametros.cnpjFacilitador))
            {
                strParametros.Append($"4={parametros.cnpjFacilitador};");
            }

            strParametros.Append($"2={CNPJSH}");

            //int retorno = ConfiguraIntSiTefInterativo(IP, numLoja, numTerminal, "0");
            //audit("ConfiguraTefEx", $"ConfiguraIntSiTefInterativoEx({IP}, {"00000930"}, {numTerminal}, {"0"}, {$"[{strParametros}]"})");
#if HOMOLOGATEF
            strParametros.Clear();
            strParametros.Append("ParmsClient=1=31406434895111;2=12523654185985");
#endif
            int retorno = ConfiguraIntSiTefInterativoEx(IP, numLoja, numTerminal, "0", $"[{strParametros}]");
            if (retornoConfSitef.ContainsKey(retorno)) return (retorno, retornoConfSitef[retorno]);
            else return (-100, "Erro desconhecido");
        }

        private int ContinuaVendaTEF()
        {
            //log.Debug(Encoding.ASCII.GetString(bufferTEF).Split('\0')[0], 0);
            if (estadoTEF == StateTEF.RetornaMenuAnterior)
            {
                Continua = 1;
                estadoTEF = StateTEF.OperacaoPadrao;
            }
            else Continua = 0;
            estadoDaDll = DllState.Speaking;
            var retorno = ContinuaFuncaoSiTefInterativo(ref Comando, ref TipoCampo, ref TamMinimo, ref TamMaximo, bufferTEF, bufferTEF.Length, Continua);
            estadoDaDll = DllState.Listening;
            ProcessaComando(Comando, bufferTEF);
            LimpaBuffer();
            return retorno;
        }

        private int CancelaOperacaoAtual()
        {
            statusAtual = StatusTEF.Cancelado;
            estadoTEF = StateTEF.OperacaoPadrao;
            while (estadoDaDll == DllState.Speaking)
            {

            }
            var retorno = ContinuaFuncaoSiTefInterativo(ref Comando, ref TipoCampo, ref TamMinimo, ref TamMaximo, bufferTEF, bufferTEF.Length, -1);
            ProcessaComando(Comando, bufferTEF);
            LimpaBuffer();
            return retorno;
        }

        private void LimpaBuffer()
        {
            //Array.Clear(bufferTEF, 0, 22000);
        }

        private void ProcessaComando(int comando, byte[] buffer)
        {
            //log.Debug($"Processing command: {comando}");
            PermiteCancelar = comando switch
            {
                23 => true,
                _ => false
            };
            switch (comando)
            {
                case 0:
                    //log.Debug($"\r\n\tTipoCampo:  {TipoCampo}\tValor: {Encoding.ASCII.GetString(buffer)}\r\n");
                    ArmazenaValor(buffer);
                    break;
                case 1:
                    ExibeMensagemOperador(buffer);
                    break;
                case 2:
                    ExibeMensagemCliente(buffer);
                    break;
                case 3:
                    ExibeMensagemOperador(buffer);
                    ExibeMensagemCliente(buffer);
                    break;
                case 4:
                    ArmazenaTituloMenu(buffer);
                    break;
                case 11:
                    RemoveMensagemOperador();
                    break;
                case 12:
                    RemoveMensagemCliente();
                    break;
                case 13:
                    RemoveMensagemOperador();
                    RemoveMensagemCliente();
                    break;
                case 14:
                    LimpaTituloMenu();
                    break;
                case 15:
                    //log.Debug("Lê número de cheque");
                    //SalvaCabecalho(buffer);
                    break;
                case 16:
                    //log.Debug("Lê número de cheque");
                    //RemoveCabecalho();
                    break;
                case 20:
                    PerguntaSimOuNao(buffer);
                    break;
                case 21:
                    ExibeMenuDeOpcoes(buffer);
                    break;
                case 22:
                    //log.Debug("Exibe mensagem de alerta");
                    ExibeMensagemDeAlerta(buffer);
                    break;
                case 23:
                    //Estado = Permitido Cancelar
                    break;
                case 29:
                    //log.Debug("Coleta Campo Interno");
                    //ColetaCampoInterno();
                    break;
                case 30:
                    //log.Debug("Coleta Campo Usuário");
                    if (TipoCampo == 500)
                    {
                        ColetaCampoUsuarioMasked(buffer);
                    }
                    else ColetaCampoUsuario(buffer);
                    break;
                case 31:
                    //log.Debug("Lê número de cheque");
                    //LeNumeroCheque();
                    break;
                case 34:
                    //log.Debug("Lê dinheiro");
                    LeDinheiro(buffer);
                    break;
                case 35:
                    //log.Debug("Lê código de barras");
                    //LeCodigoDeBarras();
                    break;
                case 41:
                    //log.Debug("Coleta Campo de Usuário Mascarado");
                    ColetaCampoUsuarioMasked(buffer);
                    break;
                case 42:
                    //log.Debug("Exibe Menu");
                    //ExibeMenu(buffer);
                    break;
            }
        }

        private void ColetaCampoUsuarioMasked(byte[] pergunta)
        {
            campoDigitado = "";
            estadoTEF = StateTEF.AguardaSenha;
            mensagemJanela = "";
            tituloJanela = Encoding.ASCII.GetString(pergunta).Split('\0')[0];
        }

        private void LeDinheiro(byte[] pergunta)
        {
            campoDigitado = "R$ ";
            estadoTEF = StateTEF.AguardaValor;
            mensagemJanela = "R$ ";
            tituloJanela = Encoding.ASCII.GetString(pergunta).Split('\0')[0];
        }

        private void ExibeMensagemDeAlerta(byte[] buffer)
        {
            tituloJanela = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            mensagemJanela = "Pressione (ENTER) para continuar";
            if (!IsSilent) estadoTEF = StateTEF.AguardaEnter;
        }

        private Pendencia pendenciaTEFAtual;
        private void ArmazenaValor(byte[] buffer)
        {
            Console.WriteLine($"tipoCampo = {TipoCampo}");
            if (TipoCampo == 100)
            {
                statusAtual = StatusTEF.Confirmado;
            }
            if (TipoCampo == 121)
            {
                Console.WriteLine("Via do Cliente");
                foreach (var linha in Encoding.ASCII.GetString(buffer).Split('\0')[0].Split('\n'))
                {
                    Console.WriteLine(linha);
                    _viaCliente.Add(linha);
                }
            }
            if (TipoCampo == 122)
            {
                Console.WriteLine("Via da Loja");
                foreach (var linha in Encoding.ASCII.GetString(buffer).Split('\0')[0].Split('\n'))
                {
                    Console.WriteLine(linha);
                    _viaLoja.Add(linha);
                }
            }
            if (TipoCampo == 123)
            {
                statusAtual = StatusTEF.Confirmado;
            }
            if (TipoCampo == 161)
            {
                numPagamentoTEF = Encoding.ASCII.GetString(buffer).Split('\0')[0];
                Console.WriteLine($"NumPagamentoTEF = {numPagamentoTEF}");
            }
            if (TipoCampo == 210 && Encoding.ASCII.GetString(buffer).Split('\0')[0] != "0")
            {
                //Status = StatusTEF.Confirmado;
            }
            if (TipoCampo == 160)
            {
                Console.WriteLine("Nova pendencia");
                pendenciaTEFAtual = new Pendencia() { NoCupom = Encoding.ASCII.GetString(buffer).Split('\0')[0] };
                //teflog($"Cupom fiscal: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
            }
            if (TipoCampo == 161 && _tipoTEF == TipoTEF.PendenciasTerminal)
            {
                Console.WriteLine("Pendencia idPag");
                //teflog($"ID do pag. : {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
                pendenciaTEFAtual.IdPag = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            }
            if (TipoCampo == 163 && _tipoTEF == TipoTEF.PendenciasTerminal)
            {
                Console.WriteLine("Pendencia DataFiscal");
                //teflog($"Data Fiscal: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
                pendenciaTEFAtual.DataFiscal = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            }
            if (TipoCampo == 134)
            {
                //teflog($"Data Fiscal: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
                if (_tipoTEF == TipoTEF.PendenciasTerminal) pendenciaTEFAtual.DataFiscal = Encoding.ASCII.GetString(buffer).Split('\0')[0];
                else _nsu = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            }
            if (TipoCampo == 164 && _tipoTEF == TipoTEF.PendenciasTerminal)
            {
                Console.WriteLine("Pendencia HoraFiscal");
                //teflog($"Hora Fiscal: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
                pendenciaTEFAtual.HoraFiscal = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            }
            if (TipoCampo == 211 && _tipoTEF == TipoTEF.PendenciasTerminal)
            {
                Console.WriteLine("Pendencia CodDaFuncao");
                //teflog($"Cód da função: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
                pendenciaTEFAtual.CodDaFuncao = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            }
            if (TipoCampo == 1319 && _tipoTEF == TipoTEF.PendenciasTerminal)
            {
                Console.WriteLine("Pendencia ValOriginal");
                //teflog($"Vlr orig.: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}\n");
                pendenciaTEFAtual.ValOriginal = Encoding.ASCII.GetString(buffer).Split('\0')[0];
                pendenciasList.Add(pendenciaTEFAtual);
            }
        }

        private void LimpaTituloMenu()
        {
            Console.WriteLine("Limpando título");
            tituloMenu = "";
            tituloJanela = "OPERAÇÃO NO TEF";
        }

        string[] ListaDeOpcoes;
        int pagina = -1, paginas;
        StringBuilder sbOpcoes = new StringBuilder();
        private void ExibeMenuDeOpcoes(byte[] buffer)
        {
            Console.WriteLine("Exibindo menu");
            sbOpcoes.Clear();
            if (pagina == -1)
            {
                ListaDeOpcoes = Encoding.ASCII.GetString(buffer).Split(';');
                for (int i = 0; i < ListaDeOpcoes.Length; i++)
                {
                    ListaDeOpcoes[i] = ListaDeOpcoes[i].Substring(ListaDeOpcoes[i].IndexOf(':') + 1);
                }
                pagina = 1;
                paginas = (ListaDeOpcoes.Length / 9) + 1;
            }
            if (pagina == paginas)
            {
                for (int i = 9 * (pagina - 1); i < ((ListaDeOpcoes.Length % 9) + 10 * (pagina - 1)) - 1; i++)
                {
                    if (ListaDeOpcoes[i].Contains('\0')) continue;
                    sbOpcoes.Append($"{(i + 1) - (9 * (pagina - 1))} - {ListaDeOpcoes[i]}\n");
                }
                if (paginas > 1) sbOpcoes.Append("0 - Primeira Página");
            }
            else
            {
                for (int j = 9 * (pagina - 1); j < ((9 * (pagina - 1)) + 9); j++)
                {
                    sbOpcoes.Append($"{(j + 1) - (9 * (pagina - 1))} - {ListaDeOpcoes[j]}\n");
                }
                sbOpcoes.Append("0 - Próxima Página");
            }
            estadoTEF = StateTEF.AguardaMenu;
            mensagemJanela = sbOpcoes.ToString().TrimEnd('\n');
            tituloJanela = tituloMenu;
        }

        private void ArmazenaTituloMenu(byte[] buffer)
        {
            tituloMenu = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            tituloJanela = tituloMenu;
            Console.WriteLine($"ArmazenaTituloMenu: {tituloMenu}");

        }

        private void RemoveMensagemCliente()
        {
            Console.WriteLine("RemoveMensagemCliente");
        }

        private void RemoveMensagemOperador()
        {
            Console.WriteLine("RemoveMensagemOperador");
        }

        private void ExibeQRCode(byte[] buffer)
        {

        }

        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (estadoTEF == StateTEF.AguardaMenu && e.Text.IsNumbersOnly())
            {
                if (e.Text == "0" && ListaDeOpcoes.Count() > 9)
                {
                    if (pagina == paginas) pagina = 1;
                    else pagina++;
                    ExibeMenuDeOpcoes(bufferTEF);
                    ComunicaComTEFAsync(progressIndicator);
                }
                else
                {
                    int opcaoEscolhida = int.Parse(e.Text) + (9 * (pagina - 1));
                    pagina = -1;
                    //LimpaBuffer();
                    Array.Copy(Encoding.ASCII.GetBytes(opcaoEscolhida.ToString().PadRight(22000, '\0')), bufferTEF, 22000);
                    //bufferTEF = Encoding.ASCII.GetBytes(opcaoEscolhida.ToString());
                    estadoTEF = StateTEF.OperacaoPadrao;
                    ComunicaComTEFAsync(progressIndicator);
                }
            }
            else if (estadoTEF == StateTEF.AguardaSimNao && (e.Text.ToUpper() == "S" || e.Text.ToUpper() == "N"))
            {
                //LimpaBuffer();
                if (e.Text.ToUpper() == "S")
                {
                    Array.Copy(Encoding.ASCII.GetBytes("0".PadRight(22000, '\0')), bufferTEF, 22000);
                    estadoTEF = StateTEF.OperacaoPadrao;
                    ComunicaComTEFAsync(progressIndicator);
                }
                else if (e.Text.ToUpper() == "N")
                {
                    Array.Copy(Encoding.ASCII.GetBytes("1".PadRight(22000, '\0')), bufferTEF, 22000);
                    estadoTEF = StateTEF.OperacaoPadrao;
                    ComunicaComTEFAsync(progressIndicator);
                }
            }
            else if (estadoTEF == StateTEF.AguardaCampo && e.Text != "\b" && e.Text != "\r" && e.Text != "\n" && e.Text != "\t")
            {
                if (campoDigitado.Length == TamMaximo) return;
                campoDigitado += e.Text;
                tbl_Body.Text = campoDigitado;
            }
            else if (estadoTEF == StateTEF.AguardaValor && e.Text != "\b" && e.Text != "\r" && e.Text != "\n" && e.Text != "\t")
            {
                if (campoDigitado.Length == TamMaximo + 3) return;
                campoDigitado += e.Text;
                tbl_Body.Text = campoDigitado;
            }
            else if (estadoTEF == StateTEF.AguardaSenha && e.Text != "\b" && e.Text != "\r" && e.Text != "\n" && e.Text != "\t")
            {
                if (campoDigitado.Length == TamMaximo) return;
                campoDigitado += e.Text;
                tbl_Body.Text += "*";
            }
        }

        private void ExibeMensagemCliente(byte[] buffer)
        {
            Console.WriteLine($"ExibeMensagemCliente: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
            return;
        }

        private void ExibeMensagemOperador(byte[] buffer)
        {
            tituloJanela = "OPERAÇÃO NO TEF";
            mensagemJanela = Encoding.ASCII.GetString(buffer).Split('\0')[0];
            Console.WriteLine($"ExibeMensagemOperador: {Encoding.ASCII.GetString(buffer).Split('\0')[0]}");
        }

        private void AtualizaUI((string body, string titulo) item)
        {
            //log.Debug("AtualizaUI called");
            tbl_Body.Text = item.body;
            lbl_Title.Text = item.titulo;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (new[] { StateTEF.AguardaCampo, StateTEF.AguardaEnter, StateTEF.AguardaMenu, StateTEF.AguardaSenha, StateTEF.AguardaValor }.Contains(estadoTEF))
                {
                    log.Debug("Cancelamento e prosseguimento solicitados");
                    estadoTEF = StateTEF.CancelamentoRequisitado;
                    ComunicaComTEFAsync(progressIndicator);
                }
                else if (PermiteCancelar)
                {
                    log.Debug("Cancelamento solicitado");
                    estadoTEF = StateTEF.CancelamentoRequisitado;
                }
                return;
            }
            if ((new[] { StateTEF.AguardaEnter }.Contains(estadoTEF)) && e.Key == Key.Enter)
            {
                estadoTEF = StateTEF.OperacaoPadrao;
                ComunicaComTEFAsync(progressIndicator);
                return;
            }
            if ((new[] { StateTEF.AguardaCampo }.Contains(estadoTEF)) && e.Key == Key.Enter)
            {
                Array.Copy(Encoding.ASCII.GetBytes(campoDigitado.ToString().PadRight(22000, '\0')), bufferTEF, 22000);
                estadoTEF = StateTEF.OperacaoPadrao;
                ComunicaComTEFAsync(progressIndicator);
                return;
            }
            if ((new[] { StateTEF.AguardaSenha }.Contains(estadoTEF)) && e.Key == Key.Enter)
            {
                Array.Copy(Encoding.ASCII.GetBytes(campoDigitado.ToString().PadRight(22000, '\0')), bufferTEF, 22000);
                estadoTEF = StateTEF.OperacaoPadrao;
                ComunicaComTEFAsync(progressIndicator);
                return;
            }
            if ((new[] { StateTEF.AguardaValor }.Contains(estadoTEF)) && e.Key == Key.Enter)
            {
                if (!decimal.TryParse(campoDigitado, (NumberStyles.AllowCurrencySymbol | NumberStyles.AllowLeadingWhite | NumberStyles.AllowDecimalPoint), ptBR, out valorDigitado))
                {
                    DialogBox.Show("TEF", DialogBoxButtons.No, DialogBoxIcons.None, false, "O valor digitado não é válido");
                    return;
                }
                Array.Copy(Encoding.ASCII.GetBytes(valorDigitado.ToString().PadRight(22000, '\0')), bufferTEF, 22000);
                estadoTEF = StateTEF.OperacaoPadrao;
                ComunicaComTEFAsync(progressIndicator);
                return;
            }
            if (estadoTEF == StateTEF.AguardaCampo && e.Key == Key.Back)
            {
                if (campoDigitado.Length == 0) return;
                campoDigitado = campoDigitado.Substring(0, campoDigitado.Length - 1);
                tbl_Body.Text = campoDigitado;
            }
            if (estadoTEF == StateTEF.AguardaSenha && e.Key == Key.Back)
            {
                if (campoDigitado.Length == 0) return;
                campoDigitado = campoDigitado.Substring(0, campoDigitado.Length - 1);
                tbl_Body.Text = new string('*', campoDigitado.Length);
            }
            if (estadoTEF == StateTEF.AguardaValor && e.Key == Key.Back)
            {
                if (campoDigitado.Length == 3) return;
                campoDigitado = campoDigitado.Substring(0, campoDigitado.Length - 1);
                tbl_Body.Text = campoDigitado;
            }
            if ((new[] { StateTEF.AguardaCampo, StateTEF.AguardaSenha, StateTEF.AguardaValor, StateTEF.AguardaMenu }.Contains(estadoTEF)) && e.Key == Key.F5)
            {
                pagina = -1;
                estadoTEF = StateTEF.RetornaMenuAnterior;
                ComunicaComTEFAsync(progressIndicator);
                return;
            }
        }

        public void SetaMensagemPinpad(string mensagem)
        {
            EscreveMensagemPermanentePinPad(mensagem);
        }

        public void ColetaCampoUsuario(byte[] pergunta)
        {
            campoDigitado = "";
            estadoTEF = StateTEF.AguardaCampo;
            mensagemJanela = "";
            tituloJanela = Encoding.ASCII.GetString(pergunta).Split('\0')[0];
        }

        public void PerguntaSimOuNao(byte[] pergunta)
        {
            estadoTEF = StateTEF.AguardaSimNao;
            mensagemJanela = "(S)im / (N)ão";
            tituloJanela = Encoding.ASCII.GetString(pergunta).Split('\0')[0];
        }
    }
#pragma warning restore CS4014

}
