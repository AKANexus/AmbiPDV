using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        private DialogBoxButtons dbb { get; set; }
        private DialogBox() { }
        //[Obsolete("Use DialogBox.Show(string, string, DialogBoxButtons, DialogBoxIcons, bool) instead.")]
        public DialogBox(string title, string line1, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None, bool showtimestamp = false)
        {
            if (dbbuttons == DialogBoxButtons.None)
            {
                // Para essa classe funcionar como popup, é necessário chamar a classe DialogBoxHandler.cs.
                // Mas a formatação da caixa não se comporta como o padrão...
                // Portanto, é necessário setar algumas properties:

                // Deve se comportar como um popup
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;

                //ShowInTaskbar = false;
                //Width = 470; // Não adiantou, não arruma a largura com as configurações acima.
                //MinWidth = 470;
                //MaxWidth = 470;

                //icn_dolan.Source = new BitmapImage(new Uri(""));

            }

            dbb = dbbuttons;

            Topmost = true;

            InitializeComponent();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run = new Run { Text = line1 };
            tbl_Body.Inlines.Add(run);
            Run runtimestamp = new Run(DateTime.Now.ToString("HH:mm:ss - dd-MM-yy"));
            if (showtimestamp)
            {
                tbl_Body.Inlines.Add(new LineBreak());
                tbl_Body.Inlines.Add(runtimestamp);
            }
            switch (dbbuttons)
            {
                case DialogBoxButtons.Yes:
                    but_Cancelar.Visibility = Visibility.Collapsed;
                    lbl_Da.Content = "OK";
                    break;
                case DialogBoxButtons.No:
                    but_Confirmar.Visibility = Visibility.Collapsed;
                    lbl_Nyet.Content = "OK";
                    break;
                case DialogBoxButtons.YesNo:
                    break;
                case DialogBoxButtons.None:
                    but_Cancelar.Visibility = Visibility.Collapsed;
                    but_Confirmar.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
            switch (dbicons)
            {
                case DialogBoxIcons.None:
                    lbl_Title.Margin = new Thickness(0, 31, 0, 0);
                    break;
                case DialogBoxIcons.Info:
                    icn_info.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Warn:
                    icn_warn.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Error:
                    icn_error.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Dolan:
                    icn_dolan.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Sangria:
                    icn_sang.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Suprimento:
                    icn_supr.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

        }
        ////[Obsolete("Use DialogBox.Show(string, string, string, DialogBoxButtons, DialogBoxIcons, bool) instead.")]
        ////public DialogBox(string title, string line1, string line2, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None, bool showtimestamp = false)
        ////{
        ////    dbb = dbbuttons;

        ////    InitializeComponent();
        ////    tbl_Body.Inlines.Clear();
        ////    lbl_Title.Text = title.ToUpper();

        ////    Run run1 = new Run();
        ////    run1.Text = line1;
        ////    Run run2 = new Run();
        ////    run2.Text = line2;
        ////    Run runtimestamp = new Run(DateTime.Now.ToString("HH:mm:ss - dd-MM-yy"));
        ////    tbl_Body.Inlines.Add(run1);
        ////    tbl_Body.Inlines.Add(new LineBreak());
        ////    tbl_Body.Inlines.Add(run2);
        ////    if (showtimestamp)
        ////    {
        ////        tbl_Body.Inlines.Add(new LineBreak());
        ////        tbl_Body.Inlines.Add(runtimestamp);
        ////    }
        ////    switch (dbbuttons)
        ////    {
        ////        case DialogBoxButtons.Yes:
        ////            but_Cancelar.Visibility = Visibility.Collapsed;
        ////            lbl_Da.Content = "OK";
        ////            break;
        ////        case DialogBoxButtons.No:
        ////            but_Confirmar.Visibility = Visibility.Collapsed;
        ////            lbl_Nyet.Content = "OK";
        ////            break;
        ////        case DialogBoxButtons.YesNo:
        ////            break;
        ////        case DialogBoxButtons.None:
        ////            but_Cancelar.Visibility = Visibility.Collapsed;
        ////            but_Confirmar.Visibility = Visibility.Collapsed;
        ////            break;
        ////        default:
        ////            break;
        ////    }
        ////    switch (dbicons)
        ////    {
        ////        case DialogBoxIcons.None:
        ////            lbl_Title.Margin = new Thickness(0, 31, 0, 0);
        ////            break;
        ////        case DialogBoxIcons.Info:
        ////            icn_info.Visibility = Visibility.Visible;
        ////            break;
        ////        case DialogBoxIcons.Warn:
        ////            icn_warn.Visibility = Visibility.Visible;
        ////            break;
        ////        case DialogBoxIcons.Error:
        ////            icn_error.Visibility = Visibility.Visible;
        ////            break;
        ////        default:
        ////            break;
        ////    }

        ////}
        ////[Obsolete("Use DialogBox.Show(string, string, string, DialogBoxButtons, DialogBoxIcons, bool) instead.")]
        ////public DialogBox(string title, string line1, string line2, string line3, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None, bool showtimestamp = false)
        ////{
        ////    dbb = dbbuttons;

        ////    InitializeComponent();
        ////    tbl_Body.Inlines.Clear();
        ////    lbl_Title.Text = title.ToUpper();
        ////    Run run1 = new Run();
        ////    run1.Text = line1;
        ////    Run run2 = new Run();
        ////    run2.Text = line2;
        ////    Run run3 = new Run();
        ////    run3.Text = line3;
        ////    Run runtimestamp = new Run(DateTime.Now.ToString("HH:mm:ss - dd-MM-yy"));
        ////    tbl_Body.Inlines.Add(run1);
        ////    tbl_Body.Inlines.Add(new LineBreak());
        ////    tbl_Body.Inlines.Add(run2);
        ////    tbl_Body.Inlines.Add(new LineBreak());
        ////    tbl_Body.Inlines.Add(run3);
        ////    if (showtimestamp)
        ////    {
        ////        tbl_Body.Inlines.Add(new LineBreak());
        ////        tbl_Body.Inlines.Add(runtimestamp);
        ////    }
        ////    switch (dbbuttons)
        ////    {
        ////        case DialogBoxButtons.Yes:
        ////            but_Cancelar.Visibility = Visibility.Collapsed;
        ////            break;
        ////        case DialogBoxButtons.No:
        ////            but_Confirmar.Visibility = Visibility.Collapsed;
        ////            break;
        ////        case DialogBoxButtons.YesNo:
        ////            break;
        ////        case DialogBoxButtons.None:
        ////            but_Cancelar.Visibility = Visibility.Collapsed;
        ////            but_Confirmar.Visibility = Visibility.Collapsed;
        ////            break;
        ////        default:
        ////            break;
        ////    }
        ////    switch (dbicons)
        ////    {
        ////        case DialogBoxIcons.None:
        ////            lbl_Title.Margin = new Thickness(0, 31, 0, 0);
        ////            break;
        ////        case DialogBoxIcons.Info:
        ////            icn_info.Visibility = Visibility.Visible;
        ////            break;
        ////        case DialogBoxIcons.Warn:
        ////            icn_warn.Visibility = Visibility.Visible;
        ////            break;
        ////        case DialogBoxIcons.Error:
        ////            icn_error.Visibility = Visibility.Visible;
        ////            break;
        ////        default:
        ////            break;
        ////    }

        ////}
        //[Obsolete("Use DialogBox.Show(string, DialogBoxButtons, DialogBoxIcons, bool, string[]) instead")]
        //public static bool? Show(string title, string line1, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None, bool showtimestamp = false)
        //{
        //    DialogBox dbox = new DialogBox();
        //    if (dbbuttons == DialogBoxButtons.None)
        //    {
        //        // Para essa classe funcionar como popup, é necessário chamar a classe DialogBoxHandler.cs.
        //        // Mas a formatação da caixa não se comporta como o padrão...
        //        // Portanto, é necessário setar algumas properties:

        //        // Deve se comportar como um popup
        //        dbox.WindowState = WindowState.Maximized; // Deixa o window esticado dos lados, mas a vertical não.
        //        dbox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        //        dbox.WindowStyle = WindowStyle.None;
        //        dbox.ResizeMode = ResizeMode.NoResize;
        //        dbox.Left = 0;
        //        dbox.Top = 0;
        //        dbox.Width = SystemParameters.VirtualScreenWidth;
        //        dbox.Height = SystemParameters.VirtualScreenHeight;

        //        //ShowInTaskbar = false;
        //        //Width = 470; // Não adiantou, não arruma a largura com as configurações acima.
        //        //MinWidth = 470;
        //        //MaxWidth = 470;



        //    }

        //    dbox.dbb = dbbuttons;

        //    dbox.Topmost = true;

        //    dbox.InitializeComponent();
        //    dbox.tbl_Body.Inlines.Clear();
        //    dbox.lbl_Title.Text = title.ToUpper();
        //    Run run = new Run { Text = line1 };
        //    dbox.tbl_Body.Inlines.Add(run);
        //    Run runtimestamp = new Run(DateTime.Now.ToString("HH:mm:ss - dd-MM-yy"));
        //    if (showtimestamp)
        //    {
        //        dbox.tbl_Body.Inlines.Add(new LineBreak());
        //        dbox.tbl_Body.Inlines.Add(runtimestamp);
        //    }
        //    switch (dbbuttons)
        //    {
        //        case DialogBoxButtons.Yes:
        //            dbox.but_Cancelar.Visibility = Visibility.Collapsed;
        //            dbox.lbl_Da.Content = "OK";
        //            break;
        //        case DialogBoxButtons.No:
        //            dbox.but_Confirmar.Visibility = Visibility.Collapsed;
        //            dbox.lbl_Nyet.Content = "OK";
        //            break;
        //        case DialogBoxButtons.YesNo:
        //            break;
        //        case DialogBoxButtons.None:
        //            dbox.but_Cancelar.Visibility = Visibility.Collapsed;
        //            dbox.but_Confirmar.Visibility = Visibility.Collapsed;
        //            break;
        //        default:
        //            break;
        //    }
        //    switch (dbicons)
        //    {
        //        case DialogBoxIcons.None:
        //            dbox.lbl_Title.Margin = new Thickness(0, 31, 0, 0);
        //            break;
        //        case DialogBoxIcons.Info:
        //            dbox.icn_info.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Warn:
        //            dbox.icn_warn.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Error:
        //            dbox.icn_error.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Dolan:
        //            dbox.icn_dolan.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Sangria:
        //            dbox.icn_sang.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Suprimento:
        //            dbox.icn_supr.Visibility = Visibility.Visible;
        //            break;
        //        default:
        //            break;
        //    }
        //    switch (dbox.ShowDialog())
        //    {
        //        case true:
        //            return true;
        //        case false:
        //            return false;
        //        case null:
        //            return null;

        //    }
        //}
        //[Obsolete("Use DialogBox.Show(string, DialogBoxButtons, DialogBoxIcons, bool, string[]) instead")]
        //public static bool? Show(string title, string line1, string line2, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None, bool showtimestamp = false)
        //{
        //    DialogBox dbox = new DialogBox();
        //    if (dbbuttons == DialogBoxButtons.None)
        //    {
        //        // Para essa classe funcionar como popup, é necessário chamar a classe DialogBoxHandler.cs.
        //        // Mas a formatação da caixa não se comporta como o padrão...
        //        // Portanto, é necessário setar algumas properties:

        //        // Deve se comportar como um popup
        //        dbox.WindowState = WindowState.Maximized; // Deixa o window esticado dos lados, mas a vertical não.
        //        dbox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        //        dbox.WindowStyle = WindowStyle.None;
        //        dbox.ResizeMode = ResizeMode.NoResize;
        //        dbox.Left = 0;
        //        dbox.Top = 0;
        //        dbox.Width = SystemParameters.VirtualScreenWidth;
        //        dbox.Height = SystemParameters.VirtualScreenHeight;

        //        //ShowInTaskbar = false;
        //        //Width = 470; // Não adiantou, não arruma a largura com as configurações acima.
        //        //MinWidth = 470;
        //        //MaxWidth = 470;



        //    }

        //    dbox.dbb = dbbuttons;

        //    dbox.Topmost = true;

        //    dbox.InitializeComponent();
        //    dbox.tbl_Body.Inlines.Clear();
        //    dbox.lbl_Title.Text = title.ToUpper();
        //    Run run1 = new Run();
        //    run1.Text = line1;
        //    Run run2 = new Run();
        //    run2.Text = line2;
        //    Run runtimestamp = new Run(DateTime.Now.ToString("HH:mm:ss - dd-MM-yy"));
        //    dbox.tbl_Body.Inlines.Add(run1);
        //    dbox.tbl_Body.Inlines.Add(new LineBreak());
        //    dbox.tbl_Body.Inlines.Add(run2);
        //    if (showtimestamp)
        //    {
        //        dbox.tbl_Body.Inlines.Add(new LineBreak());
        //        dbox.tbl_Body.Inlines.Add(runtimestamp);
        //    }
        //    switch (dbbuttons)
        //    {
        //        case DialogBoxButtons.Yes:
        //            dbox.but_Cancelar.Visibility = Visibility.Collapsed;
        //            dbox.lbl_Da.Content = "OK";
        //            break;
        //        case DialogBoxButtons.No:
        //            dbox.but_Confirmar.Visibility = Visibility.Collapsed;
        //            dbox.lbl_Nyet.Content = "OK";
        //            break;
        //        case DialogBoxButtons.YesNo:
        //            break;
        //        case DialogBoxButtons.None:
        //            dbox.but_Cancelar.Visibility = Visibility.Collapsed;
        //            dbox.but_Confirmar.Visibility = Visibility.Collapsed;
        //            break;
        //        default:
        //            break;
        //    }
        //    switch (dbicons)
        //    {
        //        case DialogBoxIcons.None:
        //            dbox.lbl_Title.Margin = new Thickness(0, 31, 0, 0);
        //            break;
        //        case DialogBoxIcons.Info:
        //            dbox.icn_info.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Warn:
        //            dbox.icn_warn.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Error:
        //            dbox.icn_error.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Dolan:
        //            dbox.icn_dolan.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Sangria:
        //            dbox.icn_sang.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Suprimento:
        //            dbox.icn_supr.Visibility = Visibility.Visible;
        //            break;
        //        default:
        //            break;
        //    }
        //    switch (dbox.ShowDialog())
        //    {
        //        case true:
        //            return true;
        //        case false:
        //            return false;
        //        case null:
        //            return null;
        //    }
        //}
        //[Obsolete("Use DialogBox.Show(string, DialogBoxButtons, DialogBoxIcons, bool, string[]) instead")]
        //public static bool? Show(string title, string line1, string line2, string line3, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None, bool showtimestamp = false)
        //{
        //    DialogBox dbox = new DialogBox();
        //    if (dbbuttons == DialogBoxButtons.None)
        //    {
        //        // Para essa classe funcionar como popup, é necessário chamar a classe DialogBoxHandler.cs.
        //        // Mas a formatação da caixa não se comporta como o padrão...
        //        // Portanto, é necessário setar algumas properties:

        //        // Deve se comportar como um popup
        //        dbox.WindowState = WindowState.Maximized; // Deixa o window esticado dos lados, mas a vertical não.
        //        dbox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        //        dbox.WindowStyle = WindowStyle.None;
        //        dbox.ResizeMode = ResizeMode.NoResize;
        //        dbox.Left = 0;
        //        dbox.Top = 0;
        //        dbox.Width = SystemParameters.VirtualScreenWidth;
        //        dbox.Height = SystemParameters.VirtualScreenHeight;

        //        //ShowInTaskbar = false;
        //        //Width = 470; // Não adiantou, não arruma a largura com as configurações acima.
        //        //MinWidth = 470;
        //        //MaxWidth = 470;



        //    }

        //    dbox.dbb = dbbuttons;

        //    dbox.Topmost = true;

        //    dbox.InitializeComponent();
        //    dbox.tbl_Body.Inlines.Clear();
        //    dbox.lbl_Title.Text = title.ToUpper();
        //    Run run1 = new Run();
        //    run1.Text = line1;
        //    Run run2 = new Run();
        //    run2.Text = line2;
        //    Run run3 = new Run();
        //    run3.Text = line3;
        //    Run runtimestamp = new Run(DateTime.Now.ToString("HH:mm:ss - dd-MM-yy"));
        //    dbox.tbl_Body.Inlines.Add(run1);
        //    dbox.tbl_Body.Inlines.Add(new LineBreak());
        //    dbox.tbl_Body.Inlines.Add(run2);
        //    dbox.tbl_Body.Inlines.Add(new LineBreak());
        //    dbox.tbl_Body.Inlines.Add(run3);
        //    if (showtimestamp)
        //    {
        //        dbox.tbl_Body.Inlines.Add(new LineBreak());
        //        dbox.tbl_Body.Inlines.Add(runtimestamp);
        //    }
        //    switch (dbbuttons)
        //    {
        //        case DialogBoxButtons.Yes:
        //            dbox.but_Cancelar.Visibility = Visibility.Collapsed;
        //            dbox.lbl_Da.Content = "OK";
        //            break;
        //        case DialogBoxButtons.No:
        //            dbox.but_Confirmar.Visibility = Visibility.Collapsed;
        //            dbox.lbl_Nyet.Content = "OK";
        //            break;
        //        case DialogBoxButtons.YesNo:
        //            break;
        //        case DialogBoxButtons.None:
        //            dbox.but_Cancelar.Visibility = Visibility.Collapsed;
        //            dbox.but_Confirmar.Visibility = Visibility.Collapsed;
        //            break;
        //        default:
        //            break;
        //    }
        //    switch (dbicons)
        //    {
        //        case DialogBoxIcons.None:
        //            dbox.lbl_Title.Margin = new Thickness(0, 31, 0, 0);
        //            break;
        //        case DialogBoxIcons.Info:
        //            dbox.icn_info.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Warn:
        //            dbox.icn_warn.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Error:
        //            dbox.icn_error.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Dolan:
        //            dbox.icn_dolan.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Sangria:
        //            dbox.icn_sang.Visibility = Visibility.Visible;
        //            break;
        //        case DialogBoxIcons.Suprimento:
        //            dbox.icn_supr.Visibility = Visibility.Visible;
        //            break;
        //        default:
        //            break;
        //    }
        //    switch (dbox.ShowDialog())
        //    {
        //        case true:
        //            return true;
        //        case false:
        //            return false;
        //        case null:
        //            return null;
        //    }
        //}

        public static bool? Show(string title, DialogBoxButtons dbbuttons, DialogBoxIcons dbicons, bool showtimestamp, params string[] linhas)
        {
            TimedBox.stateDialog = false;
            DialogBox dbox = new DialogBox();
            if (dbbuttons == DialogBoxButtons.None)
            {
                // Para essa classe funcionar como popup, é necessário chamar a classe DialogBoxHandler.cs.
                // Mas a formatação da caixa não se comporta como o padrão...
                // Portanto, é necessário setar algumas properties:

                // Deve se comportar como um popup
                dbox.WindowState = WindowState.Maximized; // Deixa o window esticado dos lados, mas a vertical não.
                dbox.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                dbox.WindowStyle = WindowStyle.None;
                dbox.ResizeMode = ResizeMode.NoResize;
                dbox.Left = 0;
                dbox.Top = 0;
                dbox.Width = SystemParameters.VirtualScreenWidth;
                dbox.Height = SystemParameters.VirtualScreenHeight;

                //ShowInTaskbar = false;
                //Width = 470; // Não adiantou, não arruma a largura com as configurações acima.
                //MinWidth = 470;
                //MaxWidth = 470;



            }

            dbox.dbb = dbbuttons;

            dbox.Topmost = true;

            dbox.InitializeComponent();
            dbox.tbl_Body.Inlines.Clear();
            dbox.lbl_Title.Text = title.ToUpper();
            Run runtimestamp = new Run(DateTime.Now.ToString("HH:mm:ss - dd-MM-yy"));
            foreach (string linha in linhas)
            {
                dbox.tbl_Body.Inlines.Add(new Run(linha));
                dbox.tbl_Body.Inlines.Add(new LineBreak());
            }
            if (showtimestamp)
            {
                dbox.tbl_Body.Inlines.Add(new LineBreak());
                dbox.tbl_Body.Inlines.Add(runtimestamp);
            }
            switch (dbbuttons)
            {
                case DialogBoxButtons.Yes:
                    dbox.but_Cancelar.Visibility = Visibility.Collapsed;
                    dbox.lbl_Da.Content = "OK";
                    break;
                case DialogBoxButtons.No:
                    dbox.but_Confirmar.Visibility = Visibility.Collapsed;
                    dbox.lbl_Nyet.Content = "OK";
                    break;
                case DialogBoxButtons.YesNo:
                    break;
                case DialogBoxButtons.None:
                    dbox.but_Cancelar.Visibility = Visibility.Collapsed;
                    dbox.but_Confirmar.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
            switch (dbicons)
            {
                case DialogBoxIcons.None:
                    dbox.lbl_Title.Margin = new Thickness(0, 31, 0, 0);
                    break;
                case DialogBoxIcons.Info:
                    dbox.icn_info.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Warn:
                    dbox.icn_warn.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Error:
                    dbox.icn_error.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Dolan:
                    dbox.icn_dolan.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Sangria:
                    dbox.icn_sang.Visibility = Visibility.Visible;
                    break;
                case DialogBoxIcons.Suprimento:
                    dbox.icn_supr.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            switch (dbox.ShowDialog())
            {
                case true:
                    return true;
                case false:
                    return false;
                case null:
                    return null;
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

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (dbb)
            {
                case DialogBoxButtons.Yes:
                    if (e.Key == Key.Enter)
                    {
                        DialogResult = true;
                        this.Close();
                    }
                    return;
                case DialogBoxButtons.No:
                    if (e.Key == Key.Enter)
                    {
                        DialogResult = false;
                        this.Close();
                    }
                    return;
                case DialogBoxButtons.YesNo:
                    if (e.Key == Key.Enter)
                    {
                        DialogResult = true;
                        this.Close();
                    }
                    else if (e.Key == Key.Escape)
                    {
                        DialogResult = false;
                        this.Close();
                    }
                    return;
                default:
                    break;
            }
        }

    }
}
