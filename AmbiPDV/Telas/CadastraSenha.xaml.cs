using FirebirdSql.Data.FirebirdClient;
using PDV_WPF.Funcoes;
using System;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;



namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for CadastraSenha.xaml
    /// </summary>
    public partial class CadastraSenha : Window
    {
        #region Fields & Properties

        public string senha { get; set; }
        private int indice;
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public CadastraSenha(string Username, string password)
        {
            InitializeComponent();

            try
            {
                using var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) };
                using var taFuncPapelServ = new DataSets.FDBDataSetOperSeedTableAdapters.TB_FUNC_PAPELTableAdapter
                {
                    Connection = SERVER_FB_CONN
                };
                indice = (int)taFuncPapelServ.PegaIDClipp(Username);
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao iniciar cadastro de senha. \n\nO aplicativo deverá ser encerrado.\n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                Application.Current.Shutdown();
                return; // deuruim();
            }

            txb_Usuario.Text = Username.ToUpper();
            if (password != null)
            {
                txb_Senha1.Password = password;
                txb_Senha2.Focus();
            }
            else
            {
                txb_Senha1.Focus();
            }
        }

        #endregion (De)Constructor

        #region Events

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && (txb_Senha1.Password != "") && (txb_Senha1.Password == txb_Senha2.Password))
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    string hash = GenerateHash(txb_Senha1.Password);

                    using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
                    using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                    using (var taUsersPdv = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
                    using (var taUsersServ = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
                    using (var taFuncPapelServ = new DataSets.FDBDataSetOperSeedTableAdapters.TB_FUNC_PAPELTableAdapter())
                    {
                        taUsersPdv.Connection = LOCAL_FB_CONN;
                        taUsersServ.Connection = SERVER_FB_CONN;

                        taFuncPapelServ.Connection = SERVER_FB_CONN;

                        if (taFuncPapelServ.ChecaSeEGerente(indice) == 1)
                        {
                            taUsersPdv.NovoUsuario(indice, txb_Usuario.Text, hash, "SIM");
                            taUsersServ.NovoUsuario(indice, txb_Usuario.Text, hash, "SIM");
                        }
                        else
                        {
                            taUsersPdv.NovoUsuario(indice, txb_Usuario.Text, hash, "NAO");
                            taUsersServ.NovoUsuario(indice, txb_Usuario.Text, hash, "NAO");
                        }
                    }

                    MessageBox.Show("Senha cadastrada com sucesso. Já pode fazer login.");
                    senha = txb_Senha1.Password;

                    // Usuário cadastrado/alterado no servidor.
                    // Esse registro será consultado no local.
                    // Então...
                    new SincronizadorDB().SincronizarContingencyNetworkDbs(EnmTipoSync.cadastros, 0);

                    this.Close();
                    return;
                });
            }
            else if (e.Key == Key.Enter && txb_Senha1.Password != "" && txb_Senha1.Password != txb_Senha2.Password)
            {
                MessageBox.Show("Senhas não conferem. Tente novamente.");
                return;
            }
            else if ((txb_Senha1.Password != "") && (txb_Senha1.IsFocused == true))
            {
                txb_Senha2.Focus();
            }
            else if (e.Key == Key.Escape)
            {
                this.Close();
                return;
            }
        }

        #endregion Events

        #region Methods



        #endregion Methods
    }
}
