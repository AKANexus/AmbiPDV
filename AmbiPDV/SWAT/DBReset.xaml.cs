using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PDV_WPF.SWAT
{
    /// <summary>
    /// Interaction logic for DBReset.xaml
    /// </summary>
    public partial class DBReset : Page
    {
        public DBReset()
        {
            InitializeComponent();
            switch (Properties.Settings.Default.SWATCode)
            {
                case "10-92":
                    but_RemoveContFDB.IsEnabled = false;
                    but_CopiaFDB.IsEnabled = true;
                    if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB"))
                    {
                        try
                        {
                            //File.Delete(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB");
                            File.Move(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB.backup");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            return;
                        }

                    }
                    break;
                default:
                    break;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SWATCode = "10-92";
            Properties.Settings.Default.Save();
            MessageBox.Show("Reinicie o Programa.");
            Application.Current.Shutdown();
        }

        private void But_CopiaFDB_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog() { Filter = "Base de Dados do Firebird (*.FDB)|*.FDB", DefaultExt = ".FDB" };
            if ((bool)ofd.ShowDialog())
            {
                Properties.Settings.Default.SWATInfo = ofd.FileName;
                Properties.Settings.Default.SWATCode = "10-50";
                Properties.Settings.Default.Save();
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }

    }
}
