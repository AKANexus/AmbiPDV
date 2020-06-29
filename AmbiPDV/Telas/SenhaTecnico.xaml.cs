using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF
{
    /// <summary>
    /// Lógica interna para SenhaTecnico.xaml
    /// </summary>
    public partial class SenhaTecnico : Window
    {
        public SenhaTecnico()
        {
            InitializeComponent();
            txb_Senha.Focus();
            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 800), DispatcherPriority.Normal, delegate
            {
                this.lbl_Hora.Content = DateTime.Now.ToString();
            }, this.Dispatcher);

        }

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Senha.Password.Length > 0)
            {
                if (txb_Senha.Password == String.Format("{0}{1}{2}", DateTime.Now.ToString("dd"), DateTime.Now.ToString("HH"), "8181"))
                {
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("SENHA INVÁLIDA.");
                    Debug.WriteLine(String.Format("Usuário {0} tentou acessar funções de técnico. Método: {1}", operador, System.Reflection.MethodBase.GetCurrentMethod().Name));
                }
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

    }
}
