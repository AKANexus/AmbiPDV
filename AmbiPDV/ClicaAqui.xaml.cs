using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PayGo;

namespace PDV_WPF
{
    /// <summary>
    /// Interaction logic for ClicaAqui.xaml
    /// </summary>
    public partial class ClicaAqui : Window
    {
        static FileSystemWatcher watcher = new FileSystemWatcher();
        public ClicaAqui()
        {
            InitializeComponent();
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Changed += new FileSystemEventHandler(OnCreated);
            watcher.Filter = "*.001";
            watcher.Path = path2;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;
            watcher.EnableRaisingEvents = true;
        }
        static string path2 = @"C:\PAYGO\Resp";
        static string path21 = @"C:\PAYGO\Resp\intpos.001";
        static string path3 = @"C:\PAYGO\Resp\intpos.sts";
        public static Dictionary<string, string> resposta = new Dictionary<string, string>();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Cthulhu();
        }
        private void Cthulhu()
        {
            
            CRT Venda1 = new CRT()
            {
                _002 = 223546,
                _003 = 100,
                _717 = DateTime.Now
            };
            Venda1.Exec();
            But.Content = "Aguardando resposta";
            
        }
        public void OnCreated(object source, FileSystemEventArgs e)
        {
            //MessageBox.Show("Heh!");
            resposta = General.LeResposta();
            this.Dispatcher.Invoke(() =>
            {
                But.Content = "Gerado com sucesso!";
            });
            //But.Content = "Gerado com sucesso!";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CNF Venda1 = new CNF()
            {
                _010 = resposta["010-000"],
                _002 = resposta["002-000"],
                _027 = resposta["027-000"],
                _717 = DateTime.Now
            };
            Venda1.Exec();
            But.Content = "Venda Confirmada";
        }
    }
}
