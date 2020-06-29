using PayGo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class TEFBox : Window
    {
        public Dictionary<string, string> resposta { get; set; }
        FileSystemWatcher watcher = new FileSystemWatcher();
        static string path2 = @"C:\PAYGO\Resp";
        public DialogBoxButtons dbb { get; set; }
        public enum DialogBoxButtons { Yes, No, YesNo }
        public enum DialogBoxIcons { None, Info, Warn, Error, Dolan }
        public TEFBox(string title, string line1, DialogBoxButtons dbbuttons, DialogBoxIcons dbicons)
        {
            InitializeComponent();
            watcher = new FileSystemWatcher();
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.Filter = "*.001";
            watcher.Path = path2;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;
            watcher.InternalBufferSize = 64;

            watcher.EnableRaisingEvents = true;
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run = new Run
            {
                Text = line1
            };
            tbl_Body.Inlines.Add(run);
            dbb = dbbuttons;
            but_Confirmar.Visibility = dbbuttons switch
            {
                _ => but_Cancelar.Visibility = Visibility.Collapsed,
            };
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
                default:
                    break;
            }

        }


        private void FechaJanela()
        {
            //MessageBox.Show("Heh!");
            watcher.Dispose();
            resposta = General.LeResposta();
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    DialogResult = true;
                    //watcher.EnableRaisingEvents = false;
                    this.Close();
                    return;
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                }

            });
            //But.Content = "Gerado com sucesso!";
        }

        public void OnCreated(object source, FileSystemEventArgs e)
        {
            FechaJanela();
        }

        public void OnChanged(object source, FileSystemEventArgs e)
        {
            FechaJanela();
        }

        public void OnRenamed(object source, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith("001"))
            { FechaJanela(); }
        }

        public void OnError(object source, ErrorEventArgs e)
        {
            Exception exErro = e.GetException();
            logErroAntigo(RetornarMensagemErro(exErro, true));
            MessageBox.Show("ERRO");
            MessageBox.Show(exErro.Message);
        }


        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            watcher.Dispose();
            this.Close();
            return;
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            //DialogResult = true;
            //this.Close();
            //return;
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            return;
            //switch (dbb)
            //{
            //    //case DialogBoxButtons.Yes:
            //    //    if (e.Key == Key.Enter)
            //    //    {
            //    //        DialogResult = true;
            //    //        this.Close();
            //    //    }
            //    //    return;
            //    case DialogBoxButtons.No:
            //        if (e.Key == Key.Enter)
            //        {
            //            DialogResult = false;
            //            watcher.EnableRaisingEvents = false;
            //            this.Close();
            //        }
            //        return;
            //    case DialogBoxButtons.YesNo:
            //        //if (e.Key == Key.Enter)
            //        //{
            //        //    DialogResult = true;
            //        //    this.Close();
            //        //}
            //        if (e.Key == Key.Escape)
            //        {
            //            DialogResult = false;
            //            watcher.EnableRaisingEvents = false;
            //            this.Close();
            //        }
            //        return;
            //    default:
            //        break;
            //}
        }
    }
}
