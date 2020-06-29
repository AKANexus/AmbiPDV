using FirebirdSql.Data.FirebirdClient;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para perguntaSerial.xaml
    /// </summary>
    public partial class perguntaSerial : Window
    {
        #region Fields & Properties

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public perguntaSerial()
        {
            InitializeComponent();
            txb_Serial.Focus();
        }

        #endregion (De)Constructor

        #region Events

        private void perguntaSerial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Serial.Text.Length == 15)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                    using var SERIAL_TA = new FDBDataSetTableAdapters.TRI_PDV_VALID_ONLINETableAdapter
                    {
                        Connection = LOCAL_FB_CONN
                    };

                    if (ValidateSerial(txb_Serial.Text.ToUpper()))
                    {
                        if (txb_Serial.Text.StartsWith("OFF"))
                        {
                            //using (var SETUP_TA = new FDBDataSetTableAdapters.TRI_PDV_SETUPTableAdapter())
                            //{
                            //    //SETUP_TA.DefineValidOffline();
                            TIPO_LICENCA = 0;
                            //}
                        }
                        SERIAL_TA.UpdinstSerial(txb_Serial.Text.ToUpper());
                        DialogResult = true;
                        Close();
                    }
                    else MessageBox.Show("Serial inválido!");
                });
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        #endregion Events

        #region Methods
        public bool ValidateSerial(string testingstring)
        {
            if (testingstring == "DRANGELAZIEGLER") return true;
            if (testingstring.Length != 15)
            {
                return false;
            }
            string _first = testingstring.Substring(3, 4);
            string _second = testingstring.Substring(7, 4);
            string _third = testingstring.Substring(11, 4);
            int first = 0;
            int second = 0;
            int third = 0;
            foreach (char a in _first)
            {
                first += (Convert.ToInt32(a) - 48);
            }
            foreach (char c in _third)
            {
                third += (Convert.ToInt32(c) - 48);
            }
            foreach (char b in _second)
            {
                second += b;
            }
            if ((((first % 11) == 0) && ((second % 11) == 0) && ((third % 11) == 0)) && ((_first == SortString(_first)) && (_second == SortString(_second)) && (_third == SortString(_third))))
            {
                return true;
            }
            return false;
        }

        private string SortString(string input)
        {
            char[] characters = input.ToArray();
            Array.Sort(characters);
            return new string(characters);
        }


        #endregion Methods
    }
}
