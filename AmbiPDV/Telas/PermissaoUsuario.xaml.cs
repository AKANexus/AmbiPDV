using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Funcoes;
using PDV_WPF.Objetos.Enums;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para PermissaoUsuario.xaml
    /// </summary>
    public partial class PermissaoUsuario : Window
    {
        public ObservableCollection<PermissionCheckBoxItem> Permissions { get; set; } = new();
        private bool Gerente { get; set; }

        public PermissaoUsuario(string usuario)
        {            
            InitializeComponent();
            DataContext = this;
            lbl_usuario.Content = usuario;
            PreenchePermissões(usuario);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(Gerente)
                MessageBox.Show("Este usuário está cadastrado como gerente, sendo assim, o mesmo tem acesso total.", "Informação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void PreenchePermissões(string user)
        {
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var taUsersPdv = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
            taUsersPdv.Connection = LOCAL_FB_CONN;

            Permissions.Clear();

            Gerente = taUsersPdv.ChecaPriv(user).Safeint() > 0;
            Permissoes permissoesUsuario = (Permissoes)taUsersPdv.GetPermissoes(USERNAME: user);

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
