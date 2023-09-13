using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para LoadingProccess.xaml
    /// </summary>
    public partial class LoadingProccess : Window
    {
        public IProgress<string> progress;
        public LoadingProccess()
        {
            InitializeComponent();  
            Topmost = true;
            progress = new Progress<string>(AtualizaUI);
        }
        public void AtualizaUI(string textProgress)
        {
            lbl_progress.Content = textProgress;
        }
    }
}
