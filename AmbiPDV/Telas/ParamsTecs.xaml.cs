using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Funcoes;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using PDV_WPF.ViewModels;
using System.Linq;
using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;
using PDV_WPF.Configuracoes;

namespace PDV_WPF
{
    public partial class ParamsTecs : Window
    {
        #region Fields & Properties

        Logger log = new Logger("Parâmetros Extras");
        private bool _contingencia;
        private bool swat = false;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public ParamsTecs(bool initialsetup, bool pContingencia)
        {
            _contingencia = pContingencia;

            var ports = SerialPort.GetPortNames().ToList();
            InitializeComponent();
            cbb_printers.Items.Add("Nenhuma");
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                cbb_printers.Items.Add(printer);
            }

            if (!initialsetup)
            {
                DataContext = new MainViewModel(clientes:false);

            }
            else
            {
                gpb_Senhas.IsEnabled = false;
                gpb_balanca.IsEnabled = false;
                Porcentagemtxb.Text = "-1";
                Porcentagemtxb.IsEnabled = false;
                but_Alterar.Visibility = Visibility.Visible;
                but_Confirmar.Visibility = Visibility.Collapsed;
            }
            foreach (string port in ports)
            {
                cbb_Ports.Items.Add(port);
            }
            string[] NetDB = MontaStringDeConexao(SERVERNAME, SERVERCATALOG).Split(';');
            txb_DB.Text = NetDB[1].Substring(12) + "|" + NetDB[0].Substring(16);
            cbb_Baud.Text = BALBAUD.ToString();
            cbb_Parity.SelectedIndex = BALPARITY;
            Porcentagemtxb.Text = COD10PORCENTO.ToString();
            #region AmbiMAITRE
            cbb_printers.Text = IMPRESSORA_USB_PED;
            #endregion AmbiMAITRE
            cbb_Marca.SelectedIndex = BALMODELO;
            cbb_Ports.SelectedIndex = ports.FindIndex(a => a == "COM" + BALPORTA);
            PreencherTipoLicenca();
        }

        #endregion (De)Constructor

        #region Events

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                swat = true;
            }
        }

        private void but_Testar_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                string[] conf = txb_DB.Text.Replace("\"", "").Split('|');
                if (conf.Length != 2) { DialogBox.Show("Configuração", DialogBoxButtons.No, DialogBoxIcons.None, false, "Caminho da Base de dados incorretamente preenchido.", "Verifique e tente novamente"); return; }
                SERVERCATALOG = conf[1];
                SERVERNAME = conf[0];
                var _funcoes = new funcoesClass();
                _funcoes.ChangeConnectionString(MontaStringDeConexao("localhost", localpath));
                switch (_funcoes.TestaConexaoComServidor(SERVERNAME, SERVERCATALOG, FBTIMEOUT))
                {
                    case true:
                        MessageBox.Show("Conexão bem sucedida.");
                        break;
                    case false:
                        MessageBox.Show("Conexão mal sucedida.");
                        break;
                }
            });
        }

        private void but_Alterar_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                string[] conf = txb_DB.Text.Replace("\"", "").Split('|');
                if (conf.Length != 2) { DialogBox.Show("Configuração", DialogBoxButtons.No, DialogBoxIcons.None, false, "Caminho da Base de dados incorretamente preenchido.", "Verifique e tente novamente"); return; }
                SERVERCATALOG = conf[1];
                SERVERNAME = conf[0];
                ConfiguracoesXML configuracoesXML = new ConfiguracoesXML();
                configuracoesXML.Serializa();
                DialogBox.Show("Configurações salvas", DialogBoxButtons.Yes, DialogBoxIcons.Info, false, "Para a alteração do banco de dados, o programa deverá ser reiniciado");
                DialogResult = null;
                this.Close();
            });
        }

        private void btnResetarSenha_Click(object sender, RoutedEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                //Verificar se não está em contingência:
                if (_contingencia)
                {
                    DialogBox.Show("Zerar senha", DialogBoxButtons.No, DialogBoxIcons.None, false, "Esse operação só poderá ser executada quando o caixa não estiver em contingência.", cbb_Usuario.SelectedItem.ToString().ToUpper());
                    return;
                }

                if (
                DialogBox.Show("Zerar senha", DialogBoxButtons.YesNo, DialogBoxIcons.None, false, "Deseja mesmo resetar a senha para o usuário:", cbb_Usuario.SelectedItem.ToString().ToUpper()) == false)
                {
                    return;
                }
                using var taUsersServ = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
                taUsersServ.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG); //ATENCAO: resetar senha só no servidor.

                if (cbb_Usuario.SelectedItem.ToString().ToUpper() == operador)
                {
                    DialogBox.Show("Zerar Senha",
                                   DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                   "Impossível resetar senha do usuário atualmente logado");
                    return;
                }
                try
                {
                    taUsersServ.CancelaSenha(cbb_Usuario.SelectedItem.ToString().ToUpper());
                    DialogBox.Show("Zerar Senha",
                                   DialogBoxButtons.Yes, DialogBoxIcons.None, false,
                                   "Senha removida com sucesso.",
                                   "Usuário poderá trocar a senha no próximo login");
                }
                catch (Exception ex)
                {
                    log.Error("Erro ao zerar a senha de usuário", ex);
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void cbb_Marca_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbb_Marca.SelectedIndex == 0)
            {
                cbb_Baud.IsEnabled = cbb_Parity.IsEnabled = cbb_Ports.IsEnabled = false;
            }
            else
            {
                cbb_Baud.IsEnabled = cbb_Parity.IsEnabled = cbb_Ports.IsEnabled = true;
            }
        }

        private void cbb_TipoLicenca_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GravarTipoLicenca();
        }

        private void cbb_printers_DropDownClosed(object sender, EventArgs e)
        {
            if (cbb_printers.SelectedIndex.ToString() != "-1")
            {
                //TODO: cara, essa é a impressora de pedidos... não deveria ficar na setting ImpressoraUSB, deveria?
                IMPRESSORA_USB_PED = cbb_printers.SelectedItem.ToString();
                //MessageBox.Show("cbb_printers.SelectedItem.ToString(): "+ cbb_printers.SelectedItem.ToString());
                PrintFunc.RecebePrint("Teste de impressão.", PrintFunc.titulo, PrintFunc.centro, 1);
                PrintFunc.RecebePrint("Se você consegue ler isso, sua impressora foi corretamente configurada no sistema.", PrintFunc.titulo, PrintFunc.centro, 1);

                PrintFunc.PrintaSpooler();
                MessageBoxResult result = MessageBox.Show("A impressão saiu corretamente na impressora desejada?", "Pergunta", MessageBoxButton.YesNo, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        break;
                    case MessageBoxResult.No:
                        MessageBox.Show("Favor selecionar a impressora correta e verificar se ela está devidamente instalada no sistema.");
                        break;
                }
            }
        }

        #endregion Events

        #region Methods

        private bool PorcentagemAdicional()
        {
            
            if (!Porcentagemtxb.Text.IsNumbersOnly() || Porcentagemtxb.Text.Safeint() < 1)
            {
                log.Debug("PorcentagemAdicional não encontrou valor válido.");
                Porcentagemtxb.Text = "";
                COD10PORCENTO = -1;
                log.Debug($"COD10PORCENTO = {COD10PORCENTO}");
                log.Debug("Configs salvas");
                return true;
            }
            else
            {
                int cod10porcento = Porcentagemtxb.Text.Safeint();
                using (FbConnection fbConn = new FbConnection(MontaStringDeConexao("localhost", localpath)))
                using (FbCommand fbComm = new FbCommand() { Connection = fbConn })
                {
                    fbComm.CommandType = System.Data.CommandType.Text;
                    fbComm.Parameters.AddWithValue("pID_IDENTIFICADOR", cod10porcento);
                    fbComm.CommandText = "SELECT ID_TIPOITEM FROM TB_ESTOQUE A " +
                                         "JOIN TB_EST_IDENTIFICADOR B ON A.ID_ESTOQUE = B.ID_ESTOQUE " +
                                         "WHERE B.ID_IDENTIFICADOR = @pID_IDENTIFICADOR";
                    if (fbConn.State != System.Data.ConnectionState.Open) fbConn.Open();
                    int idTipo = ((string)fbComm.ExecuteScalar()).Safeint();
                    fbConn.Close();
                    if (idTipo != 9)
                    {
                        DialogBox.Show("CONFIG TÉCNICA", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Por favor escolha um código cadastrado como SERVIÇO");
                        return false;
                    }
                    else
                    {
                        COD10PORCENTO = cod10porcento;
                    }
                }
                return true;
            }
        }



        private void GravarTipoLicenca()
        {
            try
            {
                int intTipo = 0;
                intTipo = cbb_TipoLicenca.SelectedIndex switch
                {
                    0 => 0,
                    1 => 1,
                    _ => throw new NotImplementedException("Tipo de licença inválido!"),
                };

                if ((int)TIPO_LICENCA != intTipo)
                {
                    TIPO_LICENCA = (TipoLicenca)intTipo;

                    Settings.Default.Save();
                    Settings.Default.Reload();
                }
            }
            catch (Exception ex)
            {
                log.Error("Falha ao gravar o tipo de licenca", ex);
                DialogBox.Show("Licença de Uso", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao gravar o tipo.");
            }
        }

        private void PreencherTipoLicenca()
        {
            try
            {
                cbb_TipoLicenca.SelectedIndex = (int)TIPO_LICENCA;
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                DialogBox.Show("Licença de Uso", DialogBoxButtons.No, DialogBoxIcons.Error, false, "Erro ao consultar o tipo.");
            }
        }

        #endregion Methods

        private void GroupBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (swat)
            {
                (new SWATMain()).ShowDialog();
                swat = false;

            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                swat = false;
            }
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            if (DialogBox.Show("Configurações do sistema", DialogBoxButtons.YesNo, DialogBoxIcons.Warn, false, "Deseja salvar e aplicar as alterações feitas?") == true)
            {
                if (!PorcentagemAdicional()) return;
                BALBAUD = Convert.ToInt32(cbb_Baud.Text);
                BALPORTA = cbb_Ports.Text.Substring(3).Safeshort();
                BALPARITY = cbb_Parity.SelectedIndex.Safeshort();
                BALBITS = 8;
                #region AmbiMAITRE
                IMPRESSORA_USB_PED = cbb_printers.SelectedItem.ToString();
                #endregion AmbiMAITRE
                BALMODELO = cbb_Marca.SelectedIndex.Safeshort();
                ACREFERENCIA = tgl_Referencia.IsChecked.ToShort();
                string[] dbinfo = txb_DB.Text.Split('|');
                SERVERCATALOG = dbinfo[1];
                SERVERNAME = dbinfo[0];
                ConfiguracoesXML configuracoesXML = new ConfiguracoesXML();
                configuracoesXML.Serializa();
                SalvaConfigsNaBase();
                MessageBox.Show("Configurações Salvas.");
                Close();
            }
        }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tgl_usatef_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

    }

}
