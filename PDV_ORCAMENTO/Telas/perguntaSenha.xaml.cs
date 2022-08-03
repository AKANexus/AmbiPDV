using PDV_WPF;
using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.Telas
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

        public perguntaSenha()
        {
            InitializeComponent();
            txb_Senha.Focus();
        }

        #endregion (De)Constructor

        #region Events

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Senha.Password.Length > 0)
            {
                debounceTimer.Debounce(250, (p) => //HACK: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    using (var taUsersPdv = new FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
                    using (var md5Hash = MD5.Create())
                    {
                        try
                        {
                            if (taUsersPdv.ChecaPriv(GenerateHash(txb_Senha.Password)) > 0)
                            {
                                DialogResult = true;
                                NivelAcesso = nivelDeAcesso.Gerente;
                            }
                            else
                            {
                                if (ChecaHash(txb_Senha.Password, (string)taUsersPdv.PegaHashPorUser(operador.DESCRICAO.ToUpper())))
                                {
                                    DialogResult = true;
                                    NivelAcesso = nivelDeAcesso.Funcionario;
                                    return;
                                }
                                DialogBox db = new DialogBox("Acesso restrito", "Senha inválida, por favor, tente novamente.", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.None);
                                db.ShowDialog();
                                txb_Senha.Password = String.Empty;
                                audit(String.Format("Usuário {0} tentou acessar funções de gerente. Método: {1}", operador, System.Reflection.MethodBase.GetCurrentMethod().Name));
                            }
                        }
                        catch (Exception ex)
                        {
                            string strErrMess = "Erro ao verificar senha. \nPor favor tente novamente.";
                            gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                            MessageBox.Show(strErrMess);
                        }
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
