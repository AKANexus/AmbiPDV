using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for PerguntaUserServ.xaml
    /// </summary>
    public partial class PerguntaUserServ : Window
    {
        #region Fields & Properties
        public int id_cliente { get; set; }
        public string nome_cliente { get; set; }
        public DateTime? vencimento { get; set; }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        #endregion Fields & Properties

        #region Constructor

        public PerguntaUserServ()
        {
            InitializeComponent();
            if (Properties.Settings.Default.SWATCode == "10-50")
            {
                txb_Caminho_Arquivo.Text = Properties.Settings.Default.SWATInfo;
            }
            TimedBox.stateDialog = false;
            //DataContext = new MainViewModel();
        }

        #endregion Constructor

        #region Events

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                ConfirmarPermissaoCopiaDbServ();
            });
        }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void but_Confirmar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    ConfirmarPermissaoCopiaDbServ();
                });
            }
        }

        #endregion Events

        #region Methods

        private void ConfirmarPermissaoCopiaDbServ()
        {
            var popUpHandler = new UIHandlers.DialogBoxHandler();
            //WindowsIdentity wid_current = WindowsIdentity.GetCurrent();
            //WindowsImpersonationContext wic = null;
            bool blnSucesso = false;
            string strMensagemErro = string.Empty;

            try
            {
                popUpHandler.Start("Copiando arquivo...", "Por favor aguarde.", DialogBoxIcons.None);
                string localpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Directory.CreateDirectory(localpath + @"\LocalDB");
                File.Copy(txb_Caminho_Arquivo.Text.Replace("\"", ""), localpath + @"\LocalDB\CLIPP.FDB", true);
                blnSucesso = true;
            }
            catch (Exception ex)
            {
                int ret = Marshal.GetLastWin32Error();
                logErroAntigo("Erro ao copiar banco do servidor para o LocalDB (LastWin32ErrorCode " +
                                   ret.ToString() + "): \n" + RetornarMensagemErro(ex, true));
                strMensagemErro = ex.Message;
            }
            finally
            {
                popUpHandler.Stop();
                if (!string.IsNullOrWhiteSpace(strMensagemErro)) { MessageBox.Show(strMensagemErro); }
                FecharDialog(blnSucesso);
            }
        }

        private void FecharDialog(bool blnSucesso)
        {
            DialogResult = blnSucesso;
            Close();
        }

        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        #endregion Methods
    }
}
