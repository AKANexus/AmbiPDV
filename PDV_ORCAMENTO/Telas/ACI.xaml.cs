using System.Windows;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Lógica interna para ACI.xaml
    /// </summary>
    public partial class ACI : Window
    {
        /*
        bool configurar_nf_usb = false;
        bool configurar_nf_serial = false;
        bool configurar_ecf = false;
        bool configurar_sat = false;
        */

        public bool configurar_nf_usb{ get; set; }
        public bool configurar_nf_serial { get; set; }
        public bool configurar_ecf { get; set; }
        public bool configurar_sat { get; set; }

        public ACI()
        {
            InitializeComponent();

        }

    }
    public class ImpressoraSerial
    {
        public string porta { get; set; }
        public string velocidade { get; set; }
        public bool ativada { get; set; }
    }
    public class ImpressoraUSB
    {
        public string impressora { get; set; }
    }
}
