using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para ExibirGif.xaml
    /// </summary>
    public partial class ExibirGif : Window
    {
        private string CaminhoGif = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Resources\loading_anim.gif";
        DispatcherTimer timer = new DispatcherTimer();
        public static volatile bool stateGif;
        //(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB"
        public ExibirGif()
        {
            stateGif = true;
            Uri uri = new Uri(CaminhoGif);
            
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += new EventHandler(CheckGifStatus);
            timer.Start();
            mediaElement.Source = uri;
        }
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        { 
            mediaElement.Position = new TimeSpan(0, 0, 1);
            mediaElement.Play();                        
        }
        private void FinalizaThread()
        {
            mediaElement.Close();
            mediaElement.Source = null;
            timer.Stop();
            timer = null;
            this.Close();
        }
        private void CheckGifStatus(object sender, EventArgs e)
        {
            if (!stateGif) FinalizaThread();            
        }
    }
}
