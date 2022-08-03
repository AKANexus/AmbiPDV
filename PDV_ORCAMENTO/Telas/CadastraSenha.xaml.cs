using System.Windows;
using System.Windows.Input;
using static PDV_WPF.staticfunc;
using System;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Interaction logic for CadastraSenha.xaml
    /// </summary>
    public partial class CadastraSenha : Window
    {
        public string senha { get; set; }
        private int indice;
        public CadastraSenha(string Username, string password)
        {
            InitializeComponent();

            try
            {
                using (var TB_FUNC_PAPELTable = new FDBOrcaDataSetTableAdapters.TB_FUNC_PAPELTableAdapter())
                {
                    TB_FUNC_PAPELTable.Connection.ConnectionString = Properties.Settings.Default.NetworkDB;
                    indice = (int)TB_FUNC_PAPELTable.PegaIDClipp(Username);
                }
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao iniciar cadastro de nova senha.";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
                this.Close();
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

        private void PerguntaSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) && (txb_Senha1.Password != "") && (txb_Senha1.Password == txb_Senha2.Password))
            {
                using (var TRI_PDV_USERSTableAdapter = new FDBOrcaDataSetTableAdapters.TRI_PDV_USERSTableAdapter())
                using (var TB_FUNC_PAPELTable = new FDBOrcaDataSetTableAdapters.TB_FUNC_PAPELTableAdapter())
                {
                    string strConn = Properties.Settings.Default.NetworkDB;

                    TRI_PDV_USERSTableAdapter.Connection.ConnectionString = strConn;
                    TB_FUNC_PAPELTable.Connection.ConnectionString = strConn;

                    string hash = GenerateHash(txb_Senha1.Password);

                    try
                    {
                        if (TB_FUNC_PAPELTable.ChecaSeEGerente(indice) == 1)
                        {
                            TRI_PDV_USERSTableAdapter.UpsertUsuario((short)indice, txb_Usuario.Text, hash, "SIM");
                        }
                        else
                        {
                            TRI_PDV_USERSTableAdapter.UpsertUsuario((short)indice, txb_Usuario.Text, hash, "NAO");
                        }
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao cadastrar senha. \nPor favor verifique os dados e tente novamente.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        return;
                    }

                    MessageBox.Show("Senha cadastrada com sucesso. Já pode fazer login.");
                    senha = txb_Senha1.Password;
                    this.Close();
                    return;
                }
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
    }
}
