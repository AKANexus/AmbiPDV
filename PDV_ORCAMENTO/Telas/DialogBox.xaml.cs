﻿using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        public DialogBoxButtons dbb { get; set; }
        public enum DialogBoxButtons { Yes, No, YesNo, None }
        public enum DialogBoxIcons { None, Info, Warn, Error, Dolan, Sangria, Suprimento }
        public DialogBox(string title, string line1, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None)
        {
            if (dbbuttons == DialogBoxButtons.None)
            {
                // Para essa classe funcionar como popup, é necessário chamar a classe DialogBoxHandler.cs.
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
                


            }

            Topmost = true;

            InitializeComponent();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run = new Run { Text = line1 };
            tbl_Body.Inlines.Add(run);
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
        public DialogBox(string title, string line1, string line2, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None)
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
                default:
                    break;
            }

        }
        public DialogBox(string title, string line1, string line2, string line3, DialogBoxButtons dbbuttons = DialogBoxButtons.No, DialogBoxIcons dbicons = DialogBoxIcons.None)
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
            switch (dbbuttons)
            {
                case DialogBoxButtons.Yes:
                    but_Cancelar.Visibility = Visibility.Collapsed;
                    break;
                case DialogBoxButtons.No:
                    but_Confirmar.Visibility = Visibility.Collapsed;
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
