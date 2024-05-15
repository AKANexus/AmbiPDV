using System;
using System.Windows;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Clearcove.Logging;

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
        Logger log = new Logger("Gif");
        //(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB"
        public ExibirGif()
        {
            try
            {
                log.Debug($"Exibindo GIF de loading do caminho: {CaminhoGif}");
                stateGif = true;
                Uri uri = new Uri(CaminhoGif);
                InitializeComponent();
                timer.Interval = TimeSpan.FromMilliseconds(2000);
                timer.Tick += new EventHandler(CheckGifStatus);
                timer.Start();
                mediaElement.Source = uri;
            }
            catch(Exception ex) 
            {
                log.Error($"Erro ao carregar GIF de {CaminhoGif}. Erro retornado: {ex.InnerException?.Message ?? ex.Message}");
            }
           
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
