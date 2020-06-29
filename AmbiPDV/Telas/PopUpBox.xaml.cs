using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for PopUpBox.xaml
    /// </summary>
    public partial class PopUpBox : Window
    {
        public enum PopUpBoxIcons { None, Info, Warn, Error, Dolan, Sangria, Suprimento }
        public PopUpBox(string title, string line1, PopUpBoxIcons dbicons)
        {
            // Para essa classe funcionar como popup, é necessário chamar a classe PopUpBoxHandler.cs.
            // Mas a formatação da caixa não se comporta como o padrão...
            // Portanto, é necessário setar algumas properties:

            // Deve se comportar como um popup
            WindowState = WindowState.Maximized; // Deixa o window esticado dos lados, mas a vertical não.
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Left = 0;
            Top = 0;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            //ShowInTaskbar = false;
            //Width = 470; // Não adiantou, não arruma a largura com as configurações acima.
            //MinWidth = 470;
            //MaxWidth = 470;




            Topmost = true;

            InitializeComponent();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run = new Run { Text = line1 };
            tbl_Body.Inlines.Add(run);
            switch (dbicons)
            {
                case PopUpBoxIcons.None:
                    lbl_Title.Margin = new Thickness(0, 31, 0, 0);
                    break;
                case PopUpBoxIcons.Info:
                    icn_info.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Warn:
                    icn_warn.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Error:
                    icn_error.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Dolan:
                    icn_dolan.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Sangria:
                    icn_sang.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Suprimento:
                    icn_supr.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

        }
        public PopUpBox(string title, string line1, string line2, PopUpBoxIcons dbicons)
        {
            InitializeComponent();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();

            Run run1 = new Run();
            run1.Text = line1;
            Run run2 = new Run();
            run2.Text = line2;
            tbl_Body.Inlines.Add(run1);
            tbl_Body.Inlines.Add(new LineBreak());
            tbl_Body.Inlines.Add(run2);

            switch (dbicons)
            {
                case PopUpBoxIcons.None:
                    lbl_Title.Margin = new Thickness(0, 31, 0, 0);
                    break;
                case PopUpBoxIcons.Info:
                    icn_info.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Warn:
                    icn_warn.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Error:
                    icn_error.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

        }
        public PopUpBox(string title, string line1, string line2, string line3, PopUpBoxIcons dbicons)
        {
            InitializeComponent();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run1 = new Run();
            run1.Text = line1;
            Run run2 = new Run();
            run2.Text = line2;
            Run run3 = new Run();
            run3.Text = line3;
            tbl_Body.Inlines.Add(run1);
            tbl_Body.Inlines.Add(new LineBreak());
            tbl_Body.Inlines.Add(run2);
            tbl_Body.Inlines.Add(new LineBreak());
            tbl_Body.Inlines.Add(run3);

            switch (dbicons)
            {
                case PopUpBoxIcons.None:
                    lbl_Title.Margin = new Thickness(0, 31, 0, 0);
                    break;
                case PopUpBoxIcons.Info:
                    icn_info.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Warn:
                    icn_warn.Visibility = Visibility.Visible;
                    break;
                case PopUpBoxIcons.Error:
                    icn_error.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }


        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            this.Close();
            return;
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            this.Close();
            return;
        }
    }
}
