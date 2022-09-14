// Por convenção, por favor colocar todos os namespaces 
// usados no documento atual logo no começo, como usings.
using PDV_ORCAMENTO.Properties;
using PDV_ORCAMENTO.Telas;
using PDV_WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO
{
    public partial class Login : Window
    {
        // Por convenção, por favor organizar os documentos nas seguintes categorias:
        // Fields & Properties, Constructor, Events e Methods.
        // Deixá-los aninhados em regions é opcional.

        #region Fields & Properties

        public string caixa;

        //static MD5 md5Hash = MD5.Create();
        bool confirm_exit = false;
        funcoes _funcoes = new funcoes();
        //FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter TRI_PDV_USERSTableAdapter1 = new FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
        //FDBOrcaDataSetTableAdapters.TB_FUNCIONARIOTableAdapter fUNCIONARIOTableAdapter = new FDBOrcaDataSetTableAdapters.TB_FUNCIONARIOTableAdapter();
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region Constructor

        public Login()
        {
            CarregaConfigSensVer();
            var args = new List<string>();
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                args.Add(arg);
                audit("Argumentos: " + args.ToString());
            }
            if (args.Contains("/auditoria") == true)
            {
                if (args.Contains("/verbose") == true)
                {
                    Settings.Default.Auditoria = 2;
                    Settings.Default.Save();
                    Settings.Default.Reload();
                }
                else
                {
                    Settings.Default.Auditoria = 1;
                    Settings.Default.Save();
                    Settings.Default.Reload();
                }
            }
            else
            {
                Settings.Default.Auditoria = 0;
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            string localpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            audit("Pasta local do programa: " + localpath);

            //audit("ChangeConnectionString:");
            _funcoes.ChangeConnectionString(Settings.Default.NetworkDB);

            verbose("Inicializar Componentes");
            InitializeComponent();

            #region Exibir títulos e textos baixados de um website
            verbose("Pescando títulos e textos do servidor");
            var client = new WebClient();
            var titulos = new List<string>();
            var textos = new List<string>();
            string[] textobaixado;
            try
            {
                textobaixado = client.DownloadString("http://www.trilhast.com.br/arq/loginstring").Split('¿');
            }
            catch (WebException)
            {
                textobaixado = "BEM VINDO|Bem vindo ao AMBI Orçamento. Mais um produto de excelência da Trilha Informática à sua disposição. Conecte-se à internet para obter as notícias mais recentes!".Split('¿');
            }
            foreach (string entry in textobaixado)
            {
                titulos.Add(entry.Split('|')[0].Replace("\n", ""));
                textos.Add(entry.Split('|')[1]);
            }
            verbose("Foram baixados " + titulos.Count + " títulos do servidor.");
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
            var fade_out = FindResource("Fade_Out") as Storyboard;
            var fade_in = FindResource("Fade_In") as Storyboard;
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
                fade_out.Begin();
            }, Dispatcher);

            #endregion Exibir títulos e textos baixados de um website

            #region Início da configuração dos bancos de dados
            audit("Configuração dos bancos de dados");
            try
            {
                audit("Executando Contingencia()");
                Contingencia();

                audit("Sistema não está em contingência");
                bool _startupsequence = _funcoes.StartupSequence();
                audit("StartupSequence() retornou " + _startupsequence);
                if (_startupsequence == false)
                {
                    var db2 = new DialogBox("ERRO AO INICIAR O PROGRAMA", "Não foi possível abrir o programa.", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.None);
                    db2.ShowDialog();
                    Environment.Exit(0);
                    return;
                }
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao executar Contingencia() e StartupSequence() \nO programa será fechado";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);

                Application.Current.Shutdown();
                return;
            }
            #endregion Início da configuração dos bancos de dados

            DataContext = new MainViewModel();
            this.Title = Settings.Default.NomeSoftware + " - Login";
            lbl_Versao.Text = " - " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "-beta";

            #region Verificação de licença de uso

            //TODO: NOPE

            #endregion Verificação de licença de uso

        }

        #endregion Constructor

        #region Events

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
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
                            string strErrMess = "Erro ao sincronizar. Verifique LogErro.txt";
                            gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show(strErrMess);
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
                return;
            }
            else if ((e.Key == Key.P) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    e.Handled = true;
                    var senha = new SenhaTecnico();
                    senha.ShowDialog();
                    if (senha.DialogResult == true)
                    {
                        new ParamsTecs(false, true).ShowDialog();
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
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                FazLogin();
            });
        }
        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //var janela = new EULA();
            //janela.ShowDialog();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //TRI_PDV_USERSTableAdapter1?.Dispose();
            //fUNCIONARIOTableAdapter?.Dispose();
        }

        #endregion Events

        #region Methods

        private void FazLogin()
        {
            if (cbb_Usuario.SelectedIndex == -1 || cbb_Usuario.SelectedItem == null) { return; }

            try
            {
                using (var TRI_PDV_USERSTableAdapter1 = new FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
                {
                    if (TRI_PDV_USERSTableAdapter1.PegaHashDaSenha_CERTO(((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString().ToUpper()) == null || (string)TRI_PDV_USERSTableAdapter1.PegaHashDaSenha_CERTO(((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString().ToUpper()) == "")
                    {
                        #region Possível primeiro login do user
                        audit("PegaHashDaSenha retornou NULL");
                        try
                        {
                            var cs = new Telas.CadastraSenha(((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString().ToUpper(), txb_Senha.Password);
                            cs.ShowDialog();
                            var db = new DialogBox("Novo usuário encontrado", "Novo usuário. Por favor crie uma senha a ser utilizada no sistema", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.Info);
                            audit(String.Format("Usuário {0} acessou pela primeira vez. Método: {1}", ((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString(), MethodBase.GetCurrentMethod().Name));
                            txb_Senha.Password = cs.senha;
                            txb_Senha.Focus();
                        }
                        catch (Exception ex)
                        {
                            gravarMensagemErro(RetornarMensagemErro(ex, true));
                            MessageBox.Show(ex.Message);
                            audit("Falha ao fazer login.");
                        }

                        return;
                        #endregion Possível primeiro login do user
                    }
                    if (ChecaHash(txb_Senha.Password, (string)TRI_PDV_USERSTableAdapter1.PegaHashPorUser(((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString().ToUpper())) == true)
                    {
                        #region Senha correta, segue o jogo.
                        audit("Senha correta.");
                        SplashScreen SS = new SplashScreen("/resources/loading_anim.gif");
                        SS.Show(false, false);
                        operador.DESCRICAO = ((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString();
                        operador.ID = ((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).ID;
                        audit("Operador: " + operador);
                        var MainWindow = new Orcamento();
                        MainWindow.Show();
                        SS.Close(TimeSpan.FromMilliseconds(1));
                        this.Hide();

                        // Gravar no banco local a data do último login válido:
                        //(new Funcoes.LicencaDeUsoOffline(90, 15)).SetLastLog(); //TODO: talvez

                        return;
                        #endregion Senha correta, segue o jogo.
                    }
                    else
                    {
                        #region Senha incorreta
                        var db = new DialogBox("Senha incorreta", "Tente novamente.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Info);
                        db.ShowDialog();
                        audit(String.Format("Usuário {0} tentou entrar com a senha incorreta. Método: {1}", cbb_Usuario.SelectedItem.ToString(), MethodBase.GetCurrentMethod().Name));
                        txb_Senha.SelectAll();
                        txb_Senha.Clear();
                        txb_Senha.Focus();
                        return;
                        #endregion Senha incorreta
                    }
                }
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao fazer login. \nPor favor verifique os dados e tente novamente.";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
        }

        private void Contingencia()
        {
            bool? ConexaoServidorOk = null;

            ConexaoServidorOk = _funcoes.TestaConexaoComServidor();
            switch (ConexaoServidorOk)
            {
                case true:
                    // Conexão com o servidor bem sucedida
                    _funcoes.ChangeConnectionString(Settings.Default.NetworkDB);
                    //TRI_PDV_USERSTableAdapter1.Connection.ConnectionString = Settings.Default.NetworkDB;
                    //fUNCIONARIOTableAdapter.Connection.ConnectionString = Settings.Default.NetworkDB;
                    audit("FDBConnString definido para DB na rede:");
                    audit(Settings.Default.FDBOrcaConnString);
                    break;
                case false:

                    // Não há conexão com o banco do servidor.

                    audit("Não há conexão com o banco do servidor.");

                    MessageBox.Show("Base de dados não foi encontrada. \n\nPor favor verifique se o servidor está acessível e configure corretamente.");
                    audit("Exibindo Parâmetros Técnicos");
                    var pt = new ParamsTecs(true, true);
                    pt.ShowDialog();
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                    return;

                    break;
                case null:
                    break;
            }
        }

        private void CarregaConfigSensVer()
        {
            try
            {

                if (Settings.Default.ConfigSensivelVersaoAlterada == true || (string)Settings.Default.GetPreviousVersion("ContingencyDB") is null)
                    return;
                //MontaStringDeConexao(Settings.Default.ServerName, Settings.Default.ServerCatalog) = (string)Settings.Default.GetPreviousVersion("NetworkDB");
                //MontaStringDeConexao("localhost", localpath) = (string)Settings.Default.GetPreviousVersion("ContingencyDB");
                Settings.Default.NetworkDB = (string)Settings.Default.GetPreviousVersion("NetworkDB");
                Settings.Default.Save();
                return;
            }
            catch (System.Configuration.SettingsPropertyNotFoundException)
            {
                return;
            }
            catch (Exception ex)
            {
                (new DialogBox("Erro de inicialização", "Falha ao carregar as configurações", "Arquivo pode estar corrompido", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Error)).ShowDialog();
                gravarMensagemErro(RetornarMensagemErro(ex, true));
                Environment.Exit(1610); //Erro 1610 = The configuration data for this product is corrupt. Contact your support personnel.
                                        //Fonte: https://www.symantec.com/connect/articles/windows-system-error-codes-exit-codes-description
                return;
            }
        }

        #endregion Methods

    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public class ComboBoxBindingDTO
        {
            public int ID { get; set; }
            public string DESCRICAO { get; set; }
        }

        private ObservableCollection<ComboBoxBindingDTO> _funcionarios;
        public ObservableCollection<ComboBoxBindingDTO> Funcionarios
        {
            get { return _funcionarios; }
            set
            {
                _funcionarios = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Funcionarios"));
            }
        }
        private ObservableCollection<ComboBoxBindingDTO> _clientes;
        public ObservableCollection<ComboBoxBindingDTO> Clientes
        {
            get { return _clientes; }
            set
            {
                _clientes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Clientes"));
            }
        }
        private ObservableCollection<ComboBoxBindingDTO> _parcelamentos;
        public ObservableCollection<ComboBoxBindingDTO> Parcelamentos
        {
            get { return _parcelamentos; }
            set
            {
                _parcelamentos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Parcelamentos"));
            }
        }
        private ObservableCollection<ComboBoxBindingDTO> _transportadoras;
        public ObservableCollection<ComboBoxBindingDTO> Transportadoras
        {
            get { return _transportadoras; }
            set
            {
                _transportadoras = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Transportadoras"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            using (var fUNCIONARIOTableAdapter = new FDBOrcaDataSetTableAdapters.TB_FUNCIONARIOTableAdapter())
            using (var pARCELAMENTO_KEYVALUETableAdapter = new FDBOrcaDataSetTableAdapters.SP_TRI_ORCA_PARCELMNT_KEYVALUETableAdapter())
            using (var tRANSPORTADORA_KEYVALUETableAdapter = new FDBOrcaDataSetTableAdapters.SP_TRI_ORCA_FORNEC_KEYVALUETableAdapter())
            using (var cLIENTES_KEYVALUETableAdapter = new FDBOrcaDataSetTableAdapters.SP_TRI_ORCA_CLIENTES_KEYVALUETableAdapter())
            using (var dt_parc_keyvalue = new FDBOrcaDataSet.SP_TRI_ORCA_PARCELMNT_KEYVALUEDataTable())
            using (var dt_func = new FDBOrcaDataSet.TB_FUNCIONARIODataTable())
            using (var dt_tran_keyvalue = new FDBOrcaDataSet.SP_TRI_ORCA_FORNEC_KEYVALUEDataTable())
            using (var dt_cli_keyvalue = new FDBOrcaDataSet.SP_TRI_ORCA_CLIENTES_KEYVALUEDataTable())
            {
                try
                {
                    fUNCIONARIOTableAdapter.FillByVendedores(dt_func);
                    pARCELAMENTO_KEYVALUETableAdapter.Fill(dt_parc_keyvalue);
                    tRANSPORTADORA_KEYVALUETableAdapter.Fill(dt_tran_keyvalue);
                    cLIENTES_KEYVALUETableAdapter.Fill(dt_cli_keyvalue);
                }
                catch (Exception ex)
                {
                    gravarMensagemErro(RetornarMensagemErro(ex, true));

                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.Message.Contains("I/O Error"))
                        {
                            (new DialogBox("Erro de conexão.",
                                           "Arquivo *.FDB não foi encontrado.",
                                           "Favor executar o assistente de configuração, e tente novamente",
                                           DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Error)).ShowDialog();
                            Application.Current.Shutdown();
                            return;
                        }
                    }

                    Application.Current.Shutdown();
                    return;
                }

                Funcionarios = new ObservableCollection<ComboBoxBindingDTO>();
                foreach (DataRow row in dt_func)
                {
                    Funcionarios.Add(new ComboBoxBindingDTO() { ID = Convert.ToInt32(row["ID_FUNCIONARIO"]), DESCRICAO = row["NOME"].ToString() });
                }

                Clientes = new ObservableCollection<ComboBoxBindingDTO>();
                foreach (DataRow row in dt_cli_keyvalue)
                {
                    Clientes.Add(new ComboBoxBindingDTO() { ID = (int)row["ID_CLIENTE"], DESCRICAO = row["NOME"].ToString() });
                }
                Parcelamentos = new ObservableCollection<ComboBoxBindingDTO>();
                foreach (DataRow row in dt_parc_keyvalue)
                {
                    Parcelamentos.Add(new ComboBoxBindingDTO() { ID = Convert.ToInt32(row["ID_PARCELA"]), DESCRICAO = row["DESCRICAO"].ToString() });
                }

                Transportadoras = new ObservableCollection<ComboBoxBindingDTO>();
                foreach (DataRow row in dt_tran_keyvalue)
                {
                    Transportadoras.Add(new ComboBoxBindingDTO() { ID = Convert.ToInt32(row["ID_FORNEC"]), DESCRICAO = row["NOME"].ToString() });
                }
            }
        }
    }
}
