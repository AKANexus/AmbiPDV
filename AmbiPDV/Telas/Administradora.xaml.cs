using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using System.Linq;
using PDV_WPF.Objetos;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para Administradora.xaml
    /// </summary>
    public partial class Administradora : Window
    {        
        public static InfoAdministradora infoAdministradora { get; set; }

        public Administradora()
        {
            InitializeComponent();
            PreencheComboBox();
            cbb_Administradora.Focus();
        }
        private void cbb_Administradora_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
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
            foreach (var admins in administradoraOC)
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
            // Passando pra variavel global as informaçõe da maquininha selecionada...

            infoAdministradora = PARAMETRO_ADMINISTRADORA.Where(predicate: x => x.Descricao == cbb_Administradora.Text).FirstOrDefault();                                   
        }
    }
}
