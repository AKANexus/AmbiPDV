using System;
using System.Windows;
using System.Windows.Input;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for EULA.xaml
    /// </summary>
    public partial class EULA : Window
    {
        public EULA()
        {
            InitializeComponent();
        }

        private void MessToolLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://platform.twit88.com/projects/messagingtoolkit/wiki/Wiki_-_Licensing");
        }

        private void FileHelpLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/MarcosMeli/FileHelpers/blob/master/LICENSE.txt");
        }

        private void LINQCSVLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/mperdeck/LINQtoCSV");
        }

        private void FirebirdLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://firebirdsql.org/en/licensing/");
        }

        private void SyncFLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.syncfusion.com/content/downloads/syncfusion_license.pdf");
        }

        private void ZenLink(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/dementeddevil/BarcodeRenderingFramework");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            { this.Close(); }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            //this.Close();
        }
    }
}
