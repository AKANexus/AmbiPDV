using FirebirdSql.Data.FirebirdClient;
using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para perguntaSenha.xaml
    /// </summary>
    public partial class perguntaSenha : Window
    {
        #region Fields & Properties

        public bool? permiteescape { get; set; }
        public enum nivelDeAcesso { Nenhum, Funcionario, Gerente }
        public nivelDeAcesso NivelAcesso { get; set; }

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public perguntaSenha(string acao)
        {
            InitializeComponent();
            txb_Senha.Focus();
            lbl_Acao.Text = acao.ToUpper();
        }

        #endregion (De)Constructor

        #region Events

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Senha.Password.Length > 0)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    try
                    {
                        using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                        using var taUsersPdv = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
                        using var md5Hash = MD5.Create();
                        taUsersPdv.Connection = LOCAL_FB_CONN;// usa a string de conexão para conectar o tableadapter corretamente com o BD

                        if (taUsersPdv.ChecaPriv(GenerateHash(txb_Senha.Password)) > 0)// checa se a senha está correta e qual estado de privilégios ela tem 
                        {
                            DialogResult = true;
                            NivelAcesso = nivelDeAcesso.Gerente;
                        }
                        else
                        {
                            if (ChecaHash(txb_Senha.Password, (string)taUsersPdv.PegaHashPorUser(operador.ToUpper())))
                            {
                                DialogResult = true;
                                NivelAcesso = nivelDeAcesso.Funcionario;
                                return;
                            }
                            DialogBox.Show("Acesso restrito",
                                           DialogBoxButtons.No, DialogBoxIcons.None, false,
                                           "Senha inválida, por favor, tente novamente.");
                            txb_Senha.Password = String.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        logErroAntigo(RetornarMensagemErro(ex, true));
                        MessageBox.Show("Erro ao conferir senha de usuário. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                        Environment.Exit(0); // deuruim();
                        return;
                    }
                });
            }
            else if (e.Key == Key.Escape && permiteescape != false)
            {
                DialogResult = false;
                NivelAcesso = nivelDeAcesso.Nenhum;
                Close();
            }
        }

        #endregion Events

        #region Methods

        #endregion Methods
    }
}
