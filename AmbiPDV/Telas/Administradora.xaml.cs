using FirebirdSql.Data.FirebirdClient;
using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para Administradora.xaml
    /// </summary>
    public partial class Administradora : Window
    {
        public static int idAdm;
        public Administradora()
        {
            InitializeComponent();
            PreencheComboBox();
            cbb_Administradora.Focus();
        }
        private void cbb_Administradora_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Enter:
                    if (cbb_Administradora.Text == null || cbb_Administradora.Text == "")
                    {
                        MessageBox.Show("Infome corretamente a maquininha utilizada!       ", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    }
                    else
                    {
                        GravaAdministradora();
                        this.Close();
                        break;
                    }
                case Key.Escape:
                    this.Close();
                    break;
                case Key.F5:
                    try
                    {
                        administradoraOC.Clear();
                        cbb_Administradora.Items.Clear();
                        CarregaAdministradoras();
                        PreencheComboBox();
                        MessageBox.Show("Cadastro de maquininhas atualizado com sucesso!", "Confirmação", MessageBoxButton.OK, MessageBoxImage.Information);                        
                    }
                    catch
                    {
                        MessageBox.Show("Erro ao atualizar cadastro de maquininhas!     ", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
            }           
        }              
        private void PreencheComboBox()
        {
            cbb_Administradora.Items.Clear();
            foreach(var admins in administradoraOC)
            {
                cbb_Administradora.Items.Add(admins);
            }
        }
        private void button_Cancelar_Click(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void button_Confirmar_Click(Object sender, RoutedEventArgs e)
        {
            if (cbb_Administradora.Text == null || cbb_Administradora.Text == "")            
                MessageBox.Show("Infome corretamente a maquininha utilizada.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                GravaAdministradora();
                this.Close();
            }                
        }
        private void GravaAdministradora()
        {
            //AQUI PASSA PRA VARIAVEL GLOBAR QUAL É A ADMINISTRADORA 
            using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })                        
            {
                using var TB_CARTAO_ADMINISTRADORA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CARTAO_ADMINISTRADORATableAdapter { Connection = LOCAL_FB_CONN };
                short? ID_ADMINISTRADORA = (short?)TB_CARTAO_ADMINISTRADORA.PegaIDPorDesc(DESCRICAO: cbb_Administradora.Text);
                if(ID_ADMINISTRADORA is not null)
                    idAdm = Convert.ToInt16(ID_ADMINISTRADORA);
            }
        }   
    }
}
