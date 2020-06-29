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
using System.Windows.Shapes;
using System.IO;
using FirebirdSql.Data.FirebirdClient;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Funcoes.SiTEFDLL;
using PDV_WPF.Objetos;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for ParamsTEF.xaml
    /// </summary>
    public partial class ParamsTEF : Window
    {
        public ParamsTEF()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SiTEFBox tefPendencias = new SiTEFBox();
            tefPendencias.ShowTEF(TipoTEF.PendenciasTerminal, 0, "0", DateTime.Now, -1, true);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //PendenciasDoTEF pendenciasDoTEF = new PendenciasDoTEF();
            //pendenciasDoTEF.ListaPendencias();

            //foreach (Pendencia pendencia in pendenciasDoTEF.ListaPendencias().Pendencias)
            //{
            //    FinalizaFuncaoSiTefInterativo(0, pendencia.NoCupom, pendencia.DataFiscal, pendencia.HoraFiscal, $"NumeroPagamentoCupom={pendencia.IdPag}");
            //}
            //MessageBox.Show("Todas as pendencias foram canceladas com sucesso.");

        }
    }
}
