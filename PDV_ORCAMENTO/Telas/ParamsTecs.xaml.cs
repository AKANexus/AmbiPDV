//using PDV_ORCAMENTO.Telas;
//using System;
//using System.Windows;
//using System.Windows.Input;
//using System.IO.Ports;
//using PDV_ORCAMENTO.Properties;
//using static PublicFunc.funcoes;

//namespace PDV_ORCAMENTO
//{
//    public partial class ParamsTecs : Window
//    {
//        #region Fields & Properties

//        #endregion Fields & Properties

//        #region (De)Constructor

//        public ParamsTecs(bool initialsetup)
//        {
//            string[] ports = SerialPort.GetPortNames();
//            InitializeComponent();

//            if (!initialsetup)
//            {
//                DataContext = new MainViewModel();

//            }
//            else
//            {
//                gpb_Senhas.IsEnabled = false;
//            }
//            string[] NetDB = Settings.Default.NetworkDB.Split(';');
//            txb_DB.Text = NetDB[0].Substring(12) + "|" + NetDB[1].Substring(16);
//        }

//        #endregion (De)Constructor

//        #region Methods

//        #endregion Methods

//        #region Events

//        private void Window_KeyDown(object sender, KeyEventArgs e)
//        {
//            if (e.Key == Key.Escape)
//            {
//                var db = new DialogBox("Configurações do sistema", "Deseja salvar e aplicar as alterações feitas?", DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.Warn);
//                db.ShowDialog();
//                switch (db.DialogResult)
//                {
//                    case true:
//                        Settings.Default.Save();
//                        Settings.Default.Reload();
//                        MessageBox.Show("Configurações Salvas.");
//                        Close();
//                        break;
//                    case false:
//                        //TODO Nada
//                        Close();
//                        break;
//                    default:
//                        //TODO Nada
//                        break;
//                }
//            }
//        }

//        private void Button_Click(object sender, RoutedEventArgs e)
//        {
//            /*
//            try
//            {
//                PayGo.ADM Administrativo = new ADM();

//                Dictionary<string, string> respCRT = new Dictionary<string, string>();
//                Administrativo.Exec();
//                TEFBox db = new TEFBox("Operação no TEF", "Pressione 'ENTER' e siga as instruções no TEF.", TEFBox.DialogBoxButtons.Yes, TEFBox.DialogBoxIcons.None);
//                db.ShowDialog();
//                if (db.DialogResult == false)
//                {
//                    return;
//                }
//                respCRT = General.LeResposta();
//                using (TimedBox tb = new TimedBox("Operação no TEF", "", respCRT["030-000"], TimedBox.DialogBoxButtons.Yes, TimedBox.DialogBoxIcons.None, 4))
//                { tb.ShowDialog(); }
//                if ((respCRT.ContainsKey("009-000")) && respCRT["009-000"] != "0")
//                {
//                    DialogBox dbTef = new DialogBox("Operação no TEF", "Operação cancelada ou não concluída.", "Tente novamente, ou use outro método de pagamento.", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.Info);
//                    return;
//                }
//                PrintTEFCliente.ReciboTEF = PrintTEFEstabel.ReciboTEF = PrintTEFUnica.ReciboTEF = PrintTEFRedux.ReciboTEF = respCRT;
//                #region printdecision
//                if (((respCRT.ContainsKey("737-000") && (respCRT["737-000"] == "1" || respCRT["737-000"] == "3"))) || !(respCRT.ContainsKey("737-000")))
//                {
//                    if (respCRT.ContainsKey("710-000") && respCRT["710-000"] != "0")
//                    {
//                        PrintTEFRedux.IMPRIME();
//                    }
//                    else
//                    {
//                        if (respCRT.ContainsKey("712-000") && respCRT["712-000"] != "0")
//                        {
//                            PrintTEFCliente.IMPRIME();
//                        }
//                        else
//                        {
//                            PrintTEFUnica.IMPRIME();
//                        }
//                    }
//                }
//                if (((respCRT.ContainsKey("737-000") && (respCRT["737-000"] == "2" || respCRT["737-000"] == "3"))) || !(respCRT.ContainsKey("737-000")))
//                {
//                    if (respCRT.ContainsKey("714-000") && respCRT["714-000"] != "0")
//                    {
//                        PrintTEFEstabel.IMPRIME();
//                    }
//                    else
//                    {
//                        PrintTEFUnica.IMPRIME();
//                    }
//                }
//                #endregion
//                CNF Confirma = new CNF()
//                {
//                    _010 = respCRT["010-000"],
//                    _027 = respCRT["027-000"],
//                    _717 = DateTime.Now
//                };
//                Confirma.Exec();
//            }
//            catch (ArgumentException ex)
//            {
//                DialogBox db1 = new DialogBox("ERRO DE TEF", "Sistema Pay&Go não está instalado neste sistema.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn);
//                Debug.WriteLine("Erro " + ex.Message);
//                db1.ShowDialog();
//                return;
//            }
//            */
//        }

//        private void Button_Click_1(object sender, RoutedEventArgs e)
//        {
//            string[] conf = txb_DB.Text.Split('|');
//            //Settings.Default.NetworkDB = String.Format(@"data source={0};initial catalog={1};user id=SYSDBA;Password=masterkey;charset=WIN1252", conf[0], conf[1]);
//            Settings.Default.NetworkDB = String.Format(@"data source={0};initial catalog={1};user id=SYSDBA;Password=masterkey;charset=WIN1252", conf[0], conf[1]);
//            Settings.Default.Save();
//            Settings.Default.Reload();
//            DialogBox db = new DialogBox("Configurações salvas", "Para a alteração do banco de dados, o programa deverá ser reiniciado", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.Info);
//            db.ShowDialog();
//            DialogResult = null;
//            this.Close();

//            //OpenFileDialog ofd = new OpenFileDialog();
//            //ofd.ShowDialog();
//            //ofd.FileOk += Ofd_FileOk;
//        }

//        private void tgl_usatef_Checked(object sender, RoutedEventArgs e)
//        {
//        }

//        private void tgl_usatef_Unchecked(object sender, RoutedEventArgs e)
//        {
//        }

//        private void Button_Click_2(object sender, RoutedEventArgs e)
//        {
//            DialogBox db1 = new DialogBox("Zerar senha", "Deseja mesmo resetar a senha para o usuário: ", cbb_Usuario.SelectedItem.ToString().ToUpper(), DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.None);
//            db1.ShowDialog();
//            if (db1.DialogResult == false)
//            {
//                return;
//            }
//            using (FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter PDV_USERS = new FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
//            {
//                if (cbb_Usuario.SelectedItem.ToString().ToUpper() == PublicFunc.funcoes.operador.DESCRICAO)
//                { DialogBox db = new DialogBox("Zerar Senha", "Impossível resetar senha do usuário atualmente logado", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn); db.ShowDialog(); return; }
//                try
//                {
//                    PDV_USERS.CancelaSenha(cbb_Usuario.SelectedItem.ToString().ToUpper());
//                    DialogBox db2 = new DialogBox("Zerar Senha", "Senha removida com sucesso.", "Usuário poderá trocar a senha no próximo login", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.None);
//                    db2.ShowDialog();
//                }
//                catch (Exception ex)
//                {
//                    gravarMensagemErro(RetornarMensagemErro(ex, true));
//                    MessageBox.Show(ex.Message);
//                }
//            }
//        }

//        private void cbb_Marca_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
//        {
//        }

//        #endregion Events
//    }
//}

using PDV_ORCAMENTO.Properties;
using PDV_ORCAMENTO.Telas;
using PDV_WPF;
using System;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Input;
//using static PublicFunc.funcoes;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO
{
    public partial class ParamsTecs : Window
    {
        #region Fields & Properties

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        private bool _blnTecnico = false;

        #endregion Fields & Properties

        #region (De)Constructor

        public ParamsTecs(bool initialsetup, bool pTecnico)
        {
            string[] ports = SerialPort.GetPortNames();
            InitializeComponent();

            if (!initialsetup)
            {
                DataContext = new MainViewModel();

                using (var taOrcaConfigServ = new FDBOrcaDataSetTableAdapters.TRI_ORCA_CONFIGTableAdapter())
                using (var tbOrcaConfigServ = new FDBOrcaDataSet.TRI_ORCA_CONFIGDataTable())
                {
                    try
                    {
                        taOrcaConfigServ.FillByMacAddress(tbOrcaConfigServ, GetCurrentMacAddress());
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao carregar configurações (from MAC address). \nPor favor tente novamente.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        this.Close();
                    }
                    
                    txb_No_Caixa.Text = tbOrcaConfigServ.Rows.Count > 0 ? tbOrcaConfigServ[0]["NO_CAIXA"].Safestring() : "1";
                }
            }
            else
            {
                gpb_Senhas.IsEnabled = false;
            }

            #region Carrega parâmetros para exibição

            if (string.IsNullOrWhiteSpace(Settings.Default.NetworkDB))
            {
                txb_DB.Text = @"localhost|C:\Program Files (x86)\CompuFour\Clipp\Base";
            }
            else
            {
                string[] NetDB = Settings.Default.NetworkDB.Split(';');
                txb_DB.Text = NetDB[0].Substring(12) + "|" + NetDB[1].Substring(16);
            }

            #endregion Carrega parâmetros para exibição

            gpbServidor.IsEnabled = pTecnico ? true : false;
            gpb_Senhas.IsEnabled = pTecnico ? true : false;

            _blnTecnico = pTecnico;
        }

        #endregion (De)Constructor

        #region Events

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var db = new DialogBox("Configurações do sistema", "Deseja salvar e aplicar as alterações feitas?", DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.Warn);
                db.ShowDialog();
                switch (db.DialogResult)
                {
                    case true:

                        SalvarConfigs();

                        Close();
                        break;
                    case false:
                        Close();
                        break;
                    default:
                        break;
                }
            }
        }

        private void but_Testar_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                TestarConnString(true);
            });
        }

        private void btnResetarSenha_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                var db1 = new DialogBox("Zerar senha", "Deseja mesmo resetar a senha para o usuário: ", ((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString().ToUpper(), DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.None);
                db1.ShowDialog();
                if (db1.DialogResult == false) { return; }

                using (var taUsersServ = new FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
                {
                    taUsersServ.Connection.ConnectionString = Settings.Default.NetworkDB.ToString(); //ATENCAO: resetar senha só no servidor.

                    if (((MainViewModel.ComboBoxBindingDTO)cbb_Usuario.SelectedItem).DESCRICAO.ToString().ToUpper() == operador.DESCRICAO)
                    { var db = new DialogBox("Zerar Senha", "Impossível resetar senha do usuário atualmente logado", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn); db.ShowDialog(); return; }
                    try
                    {
                        taUsersServ.CancelaSenha(cbb_Usuario.SelectedItem.ToString().ToUpper());
                        var db2 = new DialogBox("Zerar Senha", "Senha removida com sucesso.", "Usuário poderá trocar a senha no próximo login", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.None);
                        db2.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao zerar senha. \nPor favor tente novamente.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                    }
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                var aCI = new ACI();
                aCI.ShowDialog();
            });
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            SalvarConfigs();
        }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        #endregion Events

        #region Methods

        private void SalvarConfigs()
        {
            if (!TestarConnString(false)) { return; }

            string strMacAddress = string.Empty;
            try
            {
                strMacAddress = GetCurrentMacAddress();
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao verificar configuração de rede. \nPor favor entre em contato com a equipe de suporte.";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
                Application.Current.Shutdown();
                return;
            }
            
            short.TryParse(txb_No_Caixa.Text, out short no_caixa);

            if (!ValidarConfigs(strMacAddress, no_caixa)) { return; }

            using (var taConfigServ = new FDBOrcaDataSetTableAdapters.TRI_ORCA_CONFIGTableAdapter())
            {
                try
                {
                    taConfigServ.SP_TRI_ORCA_CONFIG_UPSERT(strMacAddress, no_caixa);
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao gravar configurações. \nPor favor entre em contato com a equipe de suporte.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    Application.Current.Shutdown();
                    return;
                }
            }

            if (_blnTecnico)
            {
                if (!TestarConnString(false)) { return; }

                string[] conf = txb_DB.Text.Split('|');
                Settings.Default.NetworkDB = String.Format(@"data source={0};initial catalog={1};user id=SYSDBA;Password=masterkey;charset=WIN1252", conf[0], conf[1]);
                Settings.Default.Save();
                Settings.Default.Reload();
                var db = new DialogBox("Configurações salvas", "Para a alteração do banco de dados, o programa deverá ser reiniciado", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.Info);
                db.ShowDialog();
                DialogResult = null;
                Settings.Default.ConfigSensivelVersaoAlterada = true;
                this.Close();
            }
            else
            {
                Settings.Default.Save();
                Settings.Default.Reload();
                MessageBox.Show("Configurações Salvas.");
            }

            //if (_blnTecnico)
            //{
            //Settings.Default.ConfigSensivelVersaoAlterada = true;
            //}
        }

        private bool ValidarConfigs(string strMacAddress, short no_caixa)
        {
            if (no_caixa <= 0 || txb_No_Caixa.Text.Contains(',') || txb_No_Caixa.Text.Contains('.'))
            {
                txb_No_Caixa.Focus();
                (new DialogBox("Falha na configuração", "O número do caixa digitado é inválido. Tente novamente.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn)).ShowDialog();
                return false;
            }
            using (var taConfigServ = new FDBOrcaDataSetTableAdapters.TRI_ORCA_CONFIGTableAdapter())
            {
                try
                {
                    if ((int)taConfigServ.ChecaPorNoCaixa(no_caixa, strMacAddress) > 0)
                    {
                        txb_No_Caixa.Focus();
                        (new DialogBox("Falha na configuração", "O número de caixa informado já está designado a outro terminal. Por favor altere e tente novamente.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn)).ShowDialog();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    string strErrMess = "Erro ao consultar configuração do terminal. \nPor favor tente novamente.";
                    gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                    MessageBox.Show(strErrMess);
                    return false;
                }
            }
            return true;
        }

        private bool TestarConnString(bool pBlnMostrarMensagemSucesso)
        {
            bool blnTesteOk = false;

            string[] conf = txb_DB.Text.Split('|');
            Settings.Default.NetworkDB = String.Format(@"data source={0};initial catalog={1};user id=SYSDBA;Password=masterkey;charset=WIN1252", conf[0], conf[1]);
            Settings.Default.Save();
            Settings.Default.Reload();
            var _funcoes = new PDV_WPF.funcoes();
            _funcoes.ChangeConnectionString(Settings.Default.NetworkDB);
            switch (_funcoes.TestaConexaoComServidor())
            {
                case true:
                    if (pBlnMostrarMensagemSucesso) { MessageBox.Show("Conexão bem sucedida."); }
                    blnTesteOk = true;
                    break;
                case false:
                    MessageBox.Show("Conexão mal sucedida.");
                    break;
                default:
                    MessageBox.Show("Conexão mal sucedida (outro).");
                    break;
            }

            return blnTesteOk;
        }

        #endregion Methods

    }

}
