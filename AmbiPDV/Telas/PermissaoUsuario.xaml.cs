using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using System.Windows;
using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Funcoes;
using PDV_WPF.Objetos.Enums;
using static PDV_WPF.Funcoes.Statics;
using PDV_WPF.Properties;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para PermissaoUsuario.xaml
    /// </summary>
    public partial class PermissaoUsuario : Window
    {
        public ObservableCollection<PermissionCheckBoxItem> Permissions { get; set; } = new();
        private Logger log = new("Permissões");
        private bool Gerente { get; set; }
        private string Usuario { get; set; }

        public PermissaoUsuario(string user)
        {            
            InitializeComponent();
            DataContext = this;
            lbl_usuario.Text = Usuario = user;
            PreenchePermissões();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(Gerente)
                MessageBox.Show("Este usuário está cadastrado como gerente, sendo assim, o mesmo tem acesso total.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void PreenchePermissões()
        {
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var taUsersPdv = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
            taUsersPdv.Connection = LOCAL_FB_CONN;

            Permissions.Clear();

            Gerente = taUsersPdv.ChecaPriv(USERNAME: Usuario).Safeint() > 0;
            Permissoes permissoesUsuario = (Permissoes)(taUsersPdv.GetPermissoes(USERNAME: Usuario) ?? 0);

            foreach (Permissoes value in Enum.GetValues(typeof(Permissoes)))
            {
                if (value == Permissoes.Nenhum || value == Permissoes.PermissaoTotal)                
                    continue;
                
                Permissions.Add(new PermissionCheckBoxItem(permissão: value.ToFriendly(),
                                                           isChecked: (permissoesUsuario & value) == value, 
                                                           isActive: !Gerente,
                                                           permissãoFlag: value, 
                                                           estiloFonte: "Normal", 
                                                           margens: "0,0,0,4"));
            }

            Permissions.Add(new PermissionCheckBoxItem(permissão: "Permissão total",
                                                       isChecked: (permissoesUsuario & Permissoes.PermissaoTotal) == Permissoes.PermissaoTotal,
                                                       isActive: !Gerente,
                                                       permissãoFlag: Permissoes.PermissaoTotal,
                                                       estiloFonte: "Bold",
                                                       margens: "0,13,0,0",
                                                       isPermissionTotal: true));
        }

        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {            
            if(sender is System.Windows.Controls.CheckBox permissionCurrent && permissionCurrent.Content.ToString() == "Permissão total")
            {               
                foreach(var permission in Permissions.Where(x => !x.IsPermissionTotal)) 
                {
                    permission.IsChecked = permissionCurrent.IsChecked ?? false;
                }
            }
        }

        private void but_Cancelar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void but_Confirmar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            using var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };           
            using var taUsersPdv = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
            taUsersPdv.Connection = SERVER_FB_CONN;
            
            try
            {
                if (Permissions.First(x => x.IsPermissionTotal).IsChecked)
                {
                    taUsersPdv.SetPermissions(PERMISSIONS: (int)Permissoes.PermissaoTotal, USERNAME: Usuario);
                }
                else
                {
                    Permissoes newPermissions = new();

                    foreach (var permission in Permissions.Where(x => x.IsChecked))
                    {
                        newPermissions |= permission.PermissãoFlag;
                    }
                    
                    taUsersPdv.SetPermissions(PERMISSIONS: (int)newPermissions, USERNAME: Usuario);                    
                }

                new SincronizadorDB().SincronizarContingencyNetworkDbs(EnmTipoSync.cadastros, Settings.Default.SegToleranciaUltSync);
                MessageBox.Show($"Permissões do usuário {Usuario} gravadas com sucesso!   ", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch(Exception ex) 
            {
                log.Error($"Erro na alteração de permissões: {ex.InnerException.Message ?? ex.Message}");
                MessageBox.Show($"Não foi possivel alterar permissões do usuário {Usuario}.\n\nVerifique os Logs de erro.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }                                         
        }
    }

    public class PermissionCheckBoxItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        public bool IsChecked 
        { 
            get { return _isChecked; } 
            set
            {
                if (_isChecked != value) 
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            } 
        }

        public bool IsActive { get; set; }
        public string Permissão { get; set; }
        public Permissoes PermissãoFlag { get; set; }
        public string EstiloFonte { get; set; }
        public string Margens { get; set; }
        public bool IsPermissionTotal { get; set; }

        public PermissionCheckBoxItem(string permissão, bool isChecked, bool isActive, Permissoes permissãoFlag, string estiloFonte, string margens, bool isPermissionTotal = false)
        {
            Permissão = permissão;
            IsChecked = isChecked;
            PermissãoFlag = permissãoFlag;
            EstiloFonte = estiloFonte;
            Margens = margens;
            IsActive = isActive;
            IsPermissionTotal = isPermissionTotal;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
