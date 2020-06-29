using PDV_WPF.SWAT;
using System.Linq;
using System.Windows;
using System.Windows.Controls;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for SWAT.xaml
    /// </summary>
    public partial class SWATMain : Window
    {
        public SWATMain()
        {
            InitializeComponent();
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (DockPanel dockPanel in grd_Corpo.Children.OfType<DockPanel>())
            {
                dockPanel.Visibility = Visibility.Collapsed;
            }
            switch (cbx_Panel.SelectedIndex)
            {
                case 1:
                    frm_Frame.NavigationService.Navigate(new DBReset());
                    break;
                case 2:
                    frm_Frame.NavigationService.Navigate(new RunSQL());
                    break;
                case 3:
                    frm_Frame.NavigationService.Navigate(new ApagaCaixa());
                    break;
                case 4:
                    frm_Frame.NavigationService.Navigate(new ResetLicenca());
                    break;
                default:
                    break;
            }
        }
    }
}
