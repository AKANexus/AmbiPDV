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

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para ExibirGif.xaml
    /// </summary>
    public partial class ExibirGif : Window
    {
        private string CaminhoGif = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Resources\loading_anim.gif";       
        //(Assembly.GetExecutingAssembly().Location) + @"\LocalDB\CLIPP.FDB"
        public ExibirGif()
        {
            Login.stateGif = true;
            Uri uri = new Uri(CaminhoGif);
            InitializeComponent();
            mediaElement.Source = uri;
        }
        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        { 
            mediaElement.Position = new TimeSpan(0, 0, 1);
            mediaElement.Play();            
            if(Login.stateGif == false)
            {
                FinalizaThread();
            }
        }

        private void FinalizaThread()
        {
            this.Close();
        }
    }
}
