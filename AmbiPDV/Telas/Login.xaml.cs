using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using NetFwTypeLib;
using PDV_WPF.Configuracoes;
using PDV_WPF.Funcoes;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using PDV_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Properties.strings;
using ECF = PDV_WPF.FuncoesECF;
using System.Threading;

namespace PDV_WPF
{
    public partial class Login : Window
    {
        #region Fields & Properties
        public string caixa;
        private bool _contingencia;
        private bool confirm_exit = false;
        private readonly funcoesClass funcoes = new funcoesClass();
        private readonly DebounceDispatcher debounceTimer = new DebounceDispatcher();
        //private SiTEFBox vendaTEF;
        Logger log = new Logger("Login");
        LoadingScreen ls = new LoadingScreen();
        public static volatile bool stateGif;
        public static Thread t1;

        #endregion Fields & Properties

        #region Constructor

        public Login()
        {
            Logger.Start(new FileInfo($"./Logs/Audit-{DateTime.Today:dd-MM-yy}.log"));
            //BombaLogica(true, true);
            ChecaModoDeSeguranca();
            ChecaTamanhoDaTela();
            //SplashScreen ss = new SplashScreen("Resources/loading_anim.gif");
            //ss.Show(false, false);
            try
            {
                //progress.Report("Carregando XML");
                CarregaXML();
                //progress.Report("Processando argumentos");
                ProcessaRuntimeArgs();
                //progress.Report("Alterando connection string");
                funcoes.ChangeConnectionString(MontaStringDeConexao(SERVERNAME, SERVERCATALOG));
                //progress.Report("Iniciando Componentes");
                InitializeComponent();
                log.Debug("InitializeComponent successful.");
                //progress.Report("Verificando firewallk");
                VerificaFirewall();
                log.Debug("VerificaFirewall successful.");
                //CarregaConfigsAnteriores();
                //log.Debug("CarregaConfigsAnteriores successful.");
                //progress.Report("Configurando banco de dados");
                ConfigurarBancosDeDados();
                log.Debug("ConfigurarBancosDeDados successful.");
                //progress.Report("Carregando configs");
                bool configsCarregadas = false;
                try
                {
                    configsCarregadas = CarregaConfigs(_contingencia);

                }
                catch (Exception)
                {
                    configsCarregadas = CarregaConfigs(true);
                }
                log.Debug($"CarregaConfigs successful. Result: {configsCarregadas}");
                switch (configsCarregadas)
                {
                    case true:
                        break;
                    case false:
                    default:
                        MessageBox.Show(EXECUTE_A_CONFIGURACAO_INICIAL);
                        Parametros config = new Parametros(false, _contingencia) { conf_inicial = true };
                        if (config.ShowDialog() == true)
                        {
                            break;
                        }
                        else
                        {
                            MessageBox.Show(E_NECESSARIO_CONFIGURAR_O_CAIXA);
                            Application.Current.Shutdown();
                            return;
                        }
                }
                //progress.Report("Carregando mensagens de login");
                CarregaTitulosDaTelaDeLogin();
                log.Debug($"CarregaTitulosDaTelaDeLogin successful");
                //progress.Report("Verificando XML Daruma");
                RecriaXMLDaruma();
                log.Debug($"RecriaXMLDaruma successful");
                try
                {
                    (new SincronizadorDB()).SincronizarContingencyNetworkDbs(EnmTipoSync.cadastros, 0);
                }
                catch (Exception)
                {
                    _contingencia = true;
                }
                log.Debug($"SincronizarContingencyNetworkDbs successful");
                DataContext = new MainViewModel();
                this.Title = NOMESOFTWARE + " - Login";
                lbl_Versao.Text = " - " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO;
                //progress.Report("Aplicando controle de licenca");
                AplicarControleLicenca();
                log.Debug($"AplicarControleLicenca successful");
                //ss.Close(TimeSpan.FromMilliseconds(1));
                ls.Close();
                TimedBox.stateDialog = false;
            }
            catch (Exception ex)
            {
                stateGif = false;
                TimedBox.stateDialog = false;
                log.Error("Erro ao abrir o caixa", ex);
                MessageBox.Show("Falha ao iniciar o caixa. Verifique Logerro.txt", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
        }

        private void But_Confirmar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion Constructor

        #region Events

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    confirm_exit = false;
                    if (cbb_Usuario.IsFocused)
                    {                        
                        txb_Senha.Focus();
                        txb_Senha.SelectAll();
                    }
                    else if (txb_Senha.IsFocused)
                    {
                        try
                        {
                            FazLogin();
                        }
                        catch (Exception ex)
                        {
                            stateGif = false;
                            log.Error("Erro na abertura do caixa: ", ex);
                            DialogBox.Show("ERRO AO ABRIR O SISTEMA", DialogBoxButtons.No, DialogBoxIcons.Error, false, "\n", ex.Message, "Entre em contato com o suporte");                            
                            return;
                        }
                    }
                });
            }
            else if ((e.Key == Key.Escape) && (confirm_exit == false))
            {
                confirm_exit = true;
            }
            else if ((e.Key == Key.Escape) && (confirm_exit == true))
            {
                Application.Current.Shutdown();
            }
            //else if (homologaTEF && (e.Key == Key.G) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            //{
            //    SiTEFBox TEF = new SiTEFBox();
            //    int intRetorno;
            //    string strRetorno;
            //    (intRetorno, strRetorno) = TEF.ConfiguraSitef("127.0.0.1", "00000000", "AA000001");
            //    e.Handled = true;
            //}
            else if ((e.Key == Key.P) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    var senha = new SenhaTecnico();
                    senha.ShowDialog();
                    if (senha.DialogResult == true)
                    {
                        new ParamsTecs(false, _contingencia).ShowDialog();
                        return;
                    }
                });
            }
            else { confirm_exit = false; }
        }
        private void Login_Shown(object sender, EventArgs e)
        {
            cbb_Usuario.Focus();
        }

        //private void Bla()
        //{
        //    vendaTEF = new SiTEFBox();
        //    vendaTEF.StatusChanged += VendaTEF_PropertyChanged;
        //    vendaTEF.ShowTEF(TipoTEF.Debito, 100, 123, DateTime.Now, 4);
        //}

        private void VendaTEF_PropertyChanged(object sender, EventArgs e)
        {
            Blu();
        }

        public void Blu()
        {
            // melhor jogador de x1 contra o Artur
            //antionte fez frii 
            //antionte feez friiiiiiii
        }
        private void but_Confirmar_MouseEnter(object sender, EventArgs e)
        {
            lbl_Confirmar.FontSize = 15;
        }
        private void but_Confirmar_MouseLeave(object sender, EventArgs e)
        {
            lbl_Confirmar.FontSize = 12;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                confirm_exit = false;
                if (cbb_Usuario.IsFocused)
                {
                    txb_Senha.Focus();
                    txb_Senha.SelectAll();
                }
                else if (txb_Senha.IsFocused)
                {
                    try
                    {
                        FazLogin();
                    }
                    catch (Exception ex)
                    {
                        stateGif = false;
                        log.Error("Erro na abertura do caixa ao tentar sincronizar", ex);
                        DialogBox.Show("ERRO AO ABRIR O SISTEMA", DialogBoxButtons.No, DialogBoxIcons.Error, false, "\n", ex.Message, "Entre em contato com o suporte");
                        return;
                    }
                }
            });
        }
        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var janela = new EULA();
            janela.ShowDialog();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TRI_PDV_USERSTableAdapter1?.Dispose();
            //fUNCIONARIOTableAdapter?.Dispose();
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Carrega as informações publicitarias a partir do URL informado.
        /// </summary>
        private void CarregaTitulosDaTelaDeLogin()
        {
            var titulos = new List<string>();
            var textos = new List<string>();
            string[] textobaixado;
            try
            {
                using var client = new WebClient();
                textobaixado = client.DownloadString("http://www.ambisoft.com.br/AmbiPDV/loginstring").Split('¿');
            }
            catch (WebException)
            {
                textobaixado = "BEM VINDO|Bem vindo ao AMBIPDV. Mais um produto de excelência da Trilha Informática à sua disposição. Conecte-se à internet para obter as notícias mais recentes!".Split('¿');
            }
            foreach (string entry in textobaixado)
            {
                titulos.Add(entry.Split('|')[0].Replace("\n", ""));
                textos.Add(entry.Split('|')[1]);
            }
            lbl_Titulo.Content = titulos[titulos.Count - 1];
            txbl_Descricao.Text = textos[titulos.Count - 1];
            int i = titulos.Count - 1;
            for (int j = 0; j < titulos.Count; j++)
            {
                var MyLabel = new Label()
                {
                    Content = "l",
                    FontFamily = new FontFamily("Wingdings"),
                    FontSize = 15,
                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#3FFFFFFF"),
                    Name = "dot_" + j.ToString(),
                    Padding = new Thickness(0, 5, 5, 5)
                };
                spn_Dots.Children.Add(MyLabel);
            };
            ((Label)spn_Dots.Children[0]).Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFFFF");
            Storyboard fade_out = FindResource("Fade_Out") as Storyboard;
            Storyboard fade_in = FindResource("Fade_In") as Storyboard;
            fade_out.Completed += new EventHandler(fade_out_completed);
            void fade_out_completed(object sender, EventArgs e)
            {
                i += 1;
                if (i > titulos.Count - 1) { i = 0; }
                if (i == 0) { ((Label)spn_Dots.Children[titulos.Count - 1]).Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#3FFFFFFF"); }
                else { ((Label)spn_Dots.Children[i - 1]).Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#3FFFFFFF"); }
                ((Label)spn_Dots.Children[i]).Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFFFF");
                lbl_Titulo.Content = titulos[i];
                txbl_Descricao.Text = textos[i];
                fade_in.Begin();
            }
            var timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 7000), DispatcherPriority.Normal, delegate
            {
                if (this.Visibility == Visibility.Visible)
                    fade_out.Begin();
            }, Dispatcher);
        }

        /// <summary>
        /// Verifica se o sistema deve abrir em modo de segurança (F8 ou SWAT Code)
        /// </summary>
        private void ChecaModoDeSeguranca()
        {
            switch (Settings.Default.SWATCode)
            {
                case "10-92": //Deleção da base de contingência.
                    (new SWATMain()).ShowDialog();
                    return;
                default:
                    break;
            }
            if (Keyboard.IsKeyDown(Key.F8))
            {
                (new SWATMain()).ShowDialog();
                Environment.Exit(0);
                //Application.Current.Shutdown();
                return;
            }
        }

        /// <summary>
        /// Verifica se o banco precisa ser apontado, configurado ou atualizado.
        /// </summary>
        private void ConfigurarBancosDeDados()
        {
            //BombaLogica(true, true);
            log.Debug("Configuração dos bancos de dados");
            try
            {
                log.Debug("Executando Contingencia()");
                Contingencia();
                log.Debug($"_contingencia definido para {_contingencia}");
                if (_contingencia)
                {
                    log.Debug("Sistema está em contingência");
                }
                else
                {
                    log.Debug("Sistema não está em contingência");
                    bool _startupsequence = funcoes.StartupSequence();
                    log.Debug($"StartupSequence() retornou {_startupsequence}");
                    if (_startupsequence == false)
                    {
                        DialogBox.Show(AMBIPDV, DialogBoxButtons.Yes, DialogBoxIcons.None, true, FALHA_AO_ABRIR_O_PROGRAMA);
                        Environment.Exit(0);
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                stateGif = false;
                log.Error("Erro ao executar Contingencia() e StartupSequence()", ex);
                MessageBox.Show("Erro ao inicar o caixa. O caixa será fechado.");
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Verifica as regras de firewall para permitir o programa se comunicar com outros computadores em rede
        /// </summary>
        private void VerificaFirewall()
        {
            var caminho = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);

            INetFwRule firewallRuleOut = (INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));
            firewallRuleOut.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRuleOut.Description = "A comunicação do AmbiPDV.";
            firewallRuleOut.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            firewallRuleOut.Enabled = true;
            firewallRuleOut.ApplicationName = (caminho);
            firewallRuleOut.InterfaceTypes = "All";
            firewallRuleOut.Name = "AmbiPDV";

            INetFwRule firewallRuleIn = (INetFwRule)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FWRule"));
            firewallRuleIn.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRuleIn.Description = "A comunicação do AmbiPDV.";
            firewallRuleIn.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            firewallRuleIn.Enabled = true;
            firewallRuleIn.ApplicationName = (caminho);
            firewallRuleIn.InterfaceTypes = "All";
            firewallRuleIn.Name = "AmbiPDV";


            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            //var currentprofiles = firewallPolicy.CurrentProfileTypes;
            //List<INetFwRule> Rules = new List<INetFwRule>();
            bool inward_rule_exists = false, outward_rule_exists = false;
            foreach (INetFwRule rule in firewallPolicy.Rules)
            {
                if (rule.Name == "AmbiPDV")
                {
                    switch (rule.Direction)
                    {
                        case NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN:
                            inward_rule_exists = true;
                            break;
                        case NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT:
                            outward_rule_exists = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            if (!outward_rule_exists)
            {
                firewallPolicy.Rules.Add(firewallRuleOut);
            }

            if (!inward_rule_exists)
            {
                firewallPolicy.Rules.Add(firewallRuleIn);
            }
        }

        /// <summary>
        /// Recria o XML da DarumaFramework, caso uma impressora fiscal esteja configurada
        /// </summary>
        private void RecriaXMLDaruma()
        {
            if ((!string.IsNullOrWhiteSpace(ECF_PORTA)) && ECF_ATIVA == true)
            {
                List<bool?> validação = new List<bool?>
                {
                    ECF.AlteraValorXML($@"START\Produto", "ECF"),
                    ECF.AlteraValorXML($@"DUAL\EncontrarDUAL", "0"),
                    ECF.AlteraValorXML($@"ECF\PortaSerial", ECF_PORTA),
                    ECF.AlteraValorXML($@"ECF\EncontrarECF", "0"),
                    ECF.AlteraValorXML($@"ECF\Velocidade", "115200"),
                    ECF.AlteraValorXML($@"ECF\ArredondarTruncar", "A")
                };
                if (validação.Contains(false) || validação.Contains(null))
                {
                    log.Warn("Houve um erro ao configurar a DarumaFramework.dll. Verifique.");
                }
            }
        }


        /// <summary>
        /// Processa os argumentos passados ao iniciar o programa
        /// </summary>
        private void ProcessaRuntimeArgs()
        {
            var args = new List<string>();
            var homologacoes = new List<string>();
            foreach (string arg in Environment.GetCommandLineArgs().Skip(1))// Para cada argumento com informações necessárias no getcommandline, substitui as letras por minúsculas e adiciona em args
            {
                //if (arg.StartsWith("/"))
                args.Add(arg.ToLower());
                log.Debug($"Argumentos: {arg}");
            }
            for (int i = 0; i < args.Count; i++)
            {
                if (args[i] == "/homologa")
                {
                    for (int j = i + 1; j < args.Count; j++)
                    {
                        if (args[j].StartsWith("/")) break;
                        else homologacoes.Add(args[j]);
                    }
                }
            }
            //homologaTEF = homologacoes.Contains("tef");
            #if HOMOLOGASAT
            homologaSAT = true;
            #endif
            homologaDEVOL = homologacoes.Contains("devol");
            if (args.Contains("/auditoria") == true)// se os argumentos contiverem /auditoria
            {
                Logger.IgnoreDebug = false;
                //    if (args.Contains("/verbose") == true)// se os argumentos contiverem /verbose
                //    {
                //        Settings.Default.Auditoria = 2;// aciona o  modo auditoria 2
                //        Settings.Default.Save();
                //        Settings.Default.Reload();
                //    }
                //    else
                //    {
                //        Settings.Default.Auditoria = 1;
                //        Settings.Default.Save();
                //        Settings.Default.Reload();
                //    }
                //}
                //else
                //{
                //    Settings.Default.Auditoria = 0;
                //    Settings.Default.Save();
                //    Settings.Default.Reload();
            }
            if (args.Contains("/stdcall") && args.Contains("/cdecl")) { MessageBox.Show("Argumentos incorretos"); Application.Current.Shutdown(); }
            eLGINStdCall = args.Contains("/stdcall");
            eLGINStdCall = !args.Contains("/cdecl");

        }
        /// <summary>
        /// Faz a verificação do user e senha no banco e retorna se é possível logar
        /// </summary>
        private void FazLogin()
        {
            if (cbb_Usuario.SelectedIndex == -1) { return; }
            if (cbb_Usuario.SelectedItem is null) { return; }

            //var fUNCIONARIOTableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.TB_FUNCIONARIOTableAdapter();

            string strHashDaSenha = string.Empty;
            try
            {
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB") };
                using var TRI_PDV_USERSTableAdapter1 = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter
                {
                    Connection = LOCAL_FB_CONN
                };
                strHashDaSenha = TRI_PDV_USERSTableAdapter1.PegaHashPorUser(cbb_Usuario.SelectedItem.ToString().ToUpper()).Safestring();
            }
            catch (Exception ex)
            {
                log.Error("Erro ao consultar hash da senha do usuário", ex);
                MessageBox.Show("Erro ao consultar hash da senha do usuário. Verifique os logs.");
                Environment.Exit(0); // deuruim();
                return;
            }

            if (string.IsNullOrWhiteSpace(strHashDaSenha))
            {
                #region Possível primeiro login do user
                log.Debug("PegaHashDaSenha retornou NULL");
                try
                {
                    var cs = new CadastraSenha(cbb_Usuario.SelectedItem.ToString().ToUpper(), txb_Senha.Password);
                    cs.ShowDialog();
                    //var db = new DialogBox("Novo usuário encontrado",
                    //                       "Novo usuário. Por favor crie uma senha a ser utilizada no sistema",
                    //                       DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.Info);
                    log.Debug($"Usuário {cbb_Usuario.SelectedItem} acessou pela primeira vez.");
                    txb_Senha.Password = cs.senha;
                    txb_Senha.Focus();
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao fazer login", ex);
                    MessageBox.Show("Erro ao fazer login");
                }

                return;
                #endregion Possível primeiro login do user
            }

            string strHashDoUser = string.Empty;
            try
            {
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB") };
                using var TRI_PDV_USERSTableAdapter1 = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter
                {
                    Connection = LOCAL_FB_CONN
                };
                strHashDoUser = TRI_PDV_USERSTableAdapter1.PegaHashPorUser(cbb_Usuario.SelectedItem.ToString().ToUpper()).Safestring();
            }
            catch (Exception ex)
            {
                log.Error("Erro ao consultar hash da senha do usuário", ex);
                MessageBox.Show("Erro ao consultar hash da senha do usuário. Verifique os logs.");
                Environment.Exit(0); // deuruim();
                return;
            }

            if (ChecaHash(txb_Senha.Password, strHashDoUser) == true)
            {
                #region Senha correta, segue o jogo.
                t1 = new Thread(() => { ExibirGif gif = new ExibirGif(); gif.ShowDialog(); });
                t1.SetApartmentState(ApartmentState.STA);
                t1.Start();
                log.Debug("Senha correta.");
                operador = cbb_Usuario.SelectedItem.ToString();
                log.Debug($"Operador: {operador}");
                //SplashScreen ss = new SplashScreen("Resources/loading_anim.gif");
                //ss.Show(false, false);
                var MainWindow = new Caixa(_contingencia);
                MainWindow.Show();                
                //ss.Close(TimeSpan.FromMilliseconds(1));
                this.Hide();
                stateGif = false;
                // Gravar no banco local a data do último login válido:
                (new LicencaDeUsoOffline(90, 15)).SetLastLog();                
                return;
                #endregion Senha correta, segue o jogo.                
            }
            else
            {
                #region Senha incorreta
                DialogBox.Show(SENHA_DIGITADA_NAO_E_VALIDA, DialogBoxButtons.No, DialogBoxIcons.Info, false, TENTE_NOVAMENTE);
                log.Debug($"Usuário {cbb_Usuario.SelectedItem} tentou entrar com a senha incorreta.");
                txb_Senha.SelectAll();
                txb_Senha.Clear();
                txb_Senha.Focus();
                return;
                #endregion Senha incorreta
            }

        }

        private void Contingencia()
        {
            bool? ConexaoServidorOk;
            //TODO: Quando o app é inicializado, ele não responde por alguns segundos.
            // Não seria interessante deixar um splash ou mostrar que o app está executando?
            // Do jeito como está, deixa a impressão que o app não iniciou.
            //var task = Task.Run(() => TestaConexaoComServidor()); //ATENCAO: deu a louca na task: enquanto o TestaConexaoComServidor() era executado em outra thread, a thread principal continuou normalmente, caindo no else da condição abaixo e setando ConexaoServidorOk = false...
            //if (task.Wait(TimeSpan.FromSeconds(5)))
            //{
            //    ConexaoServidorOk = task.Result;
            //}
            //else
            //{
            //    ConexaoServidorOk = false;
            //}
            ConexaoServidorOk = funcoes.TestaConexaoComServidor(SERVERNAME, SERVERCATALOG, FBTIMEOUT);
            switch (ConexaoServidorOk)
            {
                case true:
                    // Conexão com o servidor bem sucedida
                    funcoes.ChangeConnectionString(MontaStringDeConexao(SERVERNAME, SERVERCATALOG));
                    log.Debug($"FDBConnString definido para DB na rede: {Settings.Default.FDBConnString}");
                    _contingencia = false;
                    break;
                case false:

                    // Entra em contingência ou chama a tela de parâmetros técnicos
                    // NÃO HAVERÁ CONTINGÊNCIA SE O BANCO LOCAL NÃO EXISTIR

                    bool blnExigirParamsTecs = false;
                    log.Debug("blnExigirParamsTecs = false");
                    bool arquivoExiste = File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB");
                    log.Debug($"Checagem do arquivo local: {arquivoExiste}");
                    if (!arquivoExiste)
                    {

                        blnExigirParamsTecs = true;
                        log.Debug("blnExigirParamsTecs = true");
                    }
                    else
                    {
                        funcoes.ChangeConnectionString(MontaStringDeConexao("localhost", localpath));
                        log.Debug($"FDBConnString definido para DB de contingência: {Settings.Default.FDBConnString}");
                        //TODO: E SE NÃO EXISTIR BANCO LOCAL???
                        // Verificar e encaminhar a tela de configuração de banco inicial.
                        switch (DialogBox.Show(strings.AMBIPDV,
                                               DialogBoxButtons.YesNo,
                                               DialogBoxIcons.None, true,
                                               strings.SERVIDOR_INDISPONIVEL, // contingência
                                               strings.EXPLICACAO_MODO_DE_CONTINGENCIA))
                        {
                            case true:
                                _contingencia = true;
                                break;
                            case false:
                                var senhaTecnico = new SenhaTecnico();
                                senhaTecnico.ShowDialog();
                                if (senhaTecnico.DialogResult == true)
                                {
                                    blnExigirParamsTecs = true;
                                }
                                else
                                {
                                    DialogBox.Show(AMBIPDV, DialogBoxButtons.No, DialogBoxIcons.Warn, true, SERVIDOR_INDISPONIVEL, PARA_ABRIR_EM_CONTINGENCIA_REINICIE);
                                    Environment.Exit(0);
                                    return;
                                }
                                break;
                            default:
                                return;
                        }
                    }

                    if (blnExigirParamsTecs)
                    {
                        MessageBox.Show("Base de dados não foi encontrada. Configure corretamente");
                        log.Debug("Exibindo Parâmetros Técnicos");
                        (new ParamsTecs(true, _contingencia)).ShowDialog();
                        Application.Current.Shutdown();
                        Environment.Exit(0);
                    }

                    break;
                case null:
                    break;
            }
        }

        private void VerificarLicencaOffline()
        {
            (new LicencaDeUsoOffline(90, 15)).VerificarLicencaOffline();
        }

        private void VerificarLicencaOnline(string serial)
        {
            (new LicencaDeUsoOnline()).VerificarLicencaOnline(serial);
        }

        /// <summary>
        /// Verifica o licenciamento do software, seja online ou offline
        /// </summary>
        private void AplicarControleLicenca()
        {
            // (1) Ver se o terminal tem serial:
            //      (1.1) Se não, exibir tela pedindo serial válido. Não prosseguir enquanto o serial for inválido; (ADICIONAR TRI_PDV_SETUP.SERIAL)
            //      (1.2) Definir se a licença é "online" ou "offline" (ADICIONAR TRI_PDV_SETUP.TIPO_LICENCA);
            //          (1.2.1) Online: todo login tentará conectar com o servidor de licenças (deve considerar o campo TOLERANCIA);
            //          (1.2.2) Offline: não conectará com o servidor de licença, mas considera x dias de validade de licença (padrão 90 dias, assim como o PDV antigo, mas sem usar o registro do Windows);
            // (2) Ver se o status da licença é válido:
            //      (2.1) Online;
            //          (2.1.1) Comparar a data/hora do terminal com o servidor de licenças;
            //              (2.1.1.1) Se a diferença for maior que x horas, não continuar enquanto o relógio do terminal for inválido. Idealmente, esse valor x deve estar no banco do servidor de licenças. MAS será 2 horas, por enquanto;
            //          (2.1.2) Se licença inválida (diferente de "A"), ver qual é esse status e agir de acordo:
            //              (2.1.2.1) I = Inativo - RESERVADO - por enquanto, tratado como Bloqueado;
            //              (2.1.2.2) B = Bloqueado - Exibir MOTIV_BLOQUEIO e não prosseguir;
            //              (2.1.2.3) P = Pendente - RESERVADO - por enquanto, tratado como Bloqueado;
            //      (2.2) Offline;
            //          (2.2.1) 

            try
            {
                //using (var SETUP_TA = new FDBDataSetTableAdapters.TRI_PDV_SETUPTableAdapter())
                //{
                //    SETUP_TA.Connection.ConnectionString = MontaStringDeConexao("localhost", localpath);
                //    //if ((int)SETUP_TA.GetData().Rows[0]["VALID_ONLINE"] == 1) licencaoffline = false;
                //    // E se o TipoLicenca não existir em Settings? Isso pode acontecer quanto atualizar em clientes que ainda não tem esse setting e ainda não há uma inicialização.
                //    //if (!(Settings.Default.GetPreviousVersion("TipoLicenca") is null))
                //    //{
                //    //    Settings.Default.TipoLicenca = 0;
                //    //}
                if (true)
                {
                    string serial = (new LicencaDeUsoOnline()).ChecarSerialExiste();
                    int tentativas = 0;
                    while (serial == "NaSK" && tentativas < 4)
                    {
                        tentativas += 1;
                        //MessageBox.Show("Serial = NaSK");
                        (new LicencaDeUsoOnline()).PedeSerial();
                        serial = (new LicencaDeUsoOnline()).ChecarSerialExiste();
                    }
                    if (tentativas == 4)
                    {
                        MessageBox.Show("Número de tentativas excedido.");
                        Application.Current.Shutdown();
                    }
                    VerificarLicencaOnline(serial);
                    VerificarSQLRemoto(serial);
                }
                else
                {
                    VerificarLicencaOffline();
                }
                //}
            }
            catch (Exception ex)
            {
                log.Error("Erro ao aplicar controle de licenca", ex);
                MessageBox.Show("Erro ao aplicar controle de clicenca. O programa será fechado.");
                Application.Current.Shutdown();
            }
        }

        private void VerificarSQLRemoto(string serial)
        {
            //using var md5Hash = new HMACMD5(Encoding.UTF8.GetBytes("Mah"));
            //string hash =             

            using var CerealConn = new MySqlConnection("server=turkeyshit.mysql.dbaas.com.br;user id=turkeyshit;password=Pfes2018;persistsecurityinfo=True;database=turkeyshit;ConvertZeroDateTime=True");
            using var CerealComm = new MySqlCommand();
            CerealComm.Connection = CerealConn;
            CerealComm.CommandType = CommandType.Text;
            CerealComm.Parameters.AddWithValue("@SERIAL", serial);
            CerealComm.CommandText = "SELECT * FROM REMOTESQL WHERE PK_SERIAL = @SERIAL";
            try
            {
                CerealConn.Open();
            }
            catch (Exception)
            {
                return;
            }
            var SQL_DT = new DataTable();
            SQL_DT.Load(CerealComm.ExecuteReader());

        }

        ///// <summary>
        ///// Verifica se existem configurações em %LocalAppData% que podem ser reutilizadas em caso de atualização.
        ///// </summary>
        //private void CarregaConfigsAnteriores()
        //{

        //    //1. Detectar se é instalação nova ou já havia instalação anterior. Se for uma instalação
        //    //nova, não deveria existir um arquivo de configurações anterior, então não há o que
        //    //carregar da versão anterior.
        //    try
        //    {
        //        if ((string)Settings.Default.GetPreviousVersion("ServerCatalog") is null)
        //        {
        //            //Não existe configuração anterior. Segue reto.
        //            return;
        //        }
        //        else
        //        {
        //            SERVERCATALOG = (string)Settings.Default.GetPreviousVersion("ServerCatalog");
        //            SERVERNAME = (string)Settings.Default.GetPreviousVersion("ServerName");
        //            try
        //            {
        //                Settings.Default.BALBaud = (long)Settings.Default.GetPreviousVersion("Bal_BAUD");
        //                Settings.Default.BALBits = (long)Settings.Default.GetPreviousVersion("Bal_BITS");
        //                Settings.Default.BALModelo = (string)Settings.Default.GetPreviousVersion("Bal_MODELO");
        //                Settings.Default.BALParity = (int)Settings.Default.GetPreviousVersion("Bal_PARITY");
        //                Settings.Default.BALPorta = (string)Settings.Default.GetPreviousVersion("Bal_PORTA");
        //            }
        //            catch (SettingsPropertyNotFoundException)
        //            {

        //            }
        //            try
        //            {
        //                Settings.Default.FBTimeout = (int)Settings.Default.GetPreviousVersion("FBTimeout");
        //            }
        //            catch (SettingsPropertyNotFoundException)
        //            {

        //            }
        //            Settings.Default.Save();
        //            Settings.Default.Reload();

        //            string FullfilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

        //            foreach (string folder in Directory.GetDirectories(Path.GetFullPath(Path.Combine(FullfilePath, @"..\..\"))))
        //            {
        //                if (!folder.EndsWith(Assembly.GetEntryAssembly().GetName().Version.ToString()))
        //                {
        //                    Directory.Delete(folder, true);
        //                }
        //            }
        //            return;
        //        }
        //    }
        //    catch (SettingsPropertyNotFoundException)
        //    {
        //        //return;
        //    }
        //    catch (Exception ex)
        //    {
        //        DialogBox.Show(strings.AMBIPDV, DialogBoxButtons.No, DialogBoxIcons.Error, true, strings.FALHA_AO_CARREGAR_CONFIGS, strings.ARQUIVO_PODE_ESTAR_CORROMPIDO);
        //        log.Error("Erro ao carregar configurações anteriores", ex);
        //        Environment.Exit(1610); //Erro 1610 = The configuration data for this product is corrupt. Contact your support personnel.
        //                                //Fonte: https://www.symantec.com/connect/articles/windows-system-error-codes-exit-codes-description
        //        return;
        //    }
        //}

        #endregion Methods
    }

}
