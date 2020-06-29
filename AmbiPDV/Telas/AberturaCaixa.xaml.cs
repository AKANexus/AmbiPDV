using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;
using ECF = PDV_WPF.FuncoesECF;



namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for AberturaCaixa.xaml
    /// </summary>
    public partial class AberturaCaixa : Window
    {
        private DebounceDispatcher debounceTimer = new DebounceDispatcher();
        public enum nivelDeAcesso { Nenhum, Funcionario, Gerente }
        public nivelDeAcesso NivelAcesso { get; set; }
        public udx_pdv_oper_class _udx_pdv_oper { get; set; }
        private int _iduser;
        private int _terminal;

        public AberturaCaixa(int _operador, int terminal)
        {
            InitializeComponent();
            txb_Suprimento.Focus();
            run_Operador.Text = operador.Split(' ')[0];
            run_Terminal.Text = terminal.ToString("D3");
            _terminal = terminal;
            _iduser = _operador;
        }

        private void txb_Suprimento_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txb_Suprimento.Value < 0)
                { txb_Suprimento.Value = 0; }
                txb_Senha.Focus();
            }
        }

        private void txb_Senha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txb_Senha.Password.Length > 0)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                    using var taUsersPdv = new FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
                    using var md5Hash = MD5.Create();
                    taUsersPdv.Connection = LOCAL_FB_CONN;
                    {
                        if (ChecaHash(txb_Senha.Password, (string)taUsersPdv.PegaHashPorUser(operador.ToUpper())))
                        {
                            //AbreCaixa();
                            using (var TERMARIO_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TRI_PDV_TERMINAL_USUARIOTableAdapter())
                            {
                                TERMARIO_TA.Connection = LOCAL_FB_CONN;
                                TERMARIO_TA.SP_TRI_ABRENOVOCAIXA(_terminal, _iduser);
                            }
                            var _funcoes = new funcoesClass();
                            _udx_pdv_oper = _funcoes.VerificaPDVOper(operador);
                            //NivelAcesso = nivelDeAcesso.Funcionario;
                            if (txb_Suprimento.Value > 0)
                            {
                                if (IMPRESSORA_USB == "Nenhuma" && ECF_ATIVA)
                                {
                                    if (ECF.ChecaStatusReducaoZ() == false)
                                    {
                                        DialogBox.Show("Abertura de Caixa", DialogBoxButtons.No, DialogBoxIcons.Info, false, "É necessário fazer a redução Z antes de abrir o caixa.", "Efetue a redução Z com CTRL+O");
                                        return;
                                    }
                                    LocalDarumaFrameworkDLL.UnsafeNativeMethods.eAbrirGaveta_ECF_Daruma();
                                }
                                else
                                {
                                    //PrintFunc.RecebePrint(" ", PrintFunc.negrito, PrintFunc.centro, 1);
                                    //PrintFunc.PrintaSpooler();
                                }
                                ExecutaOperacao();
                            }

                            DialogResult = true;
                            return;
                        }
                        DialogBox.Show("Senha incorreta", DialogBoxButtons.No, DialogBoxIcons.None, false, "Por favor digite sua senha para confirmar a abertura do caixa.");
                        txb_Senha.Password = String.Empty;
                    }
                });
            }
        }

        //private void Window_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //    {
        //        DialogResult = false;
        //        this.Close();
        //    }
        //}

        private void ExecutaOperacao()
        {
            var SANSUP = new PrintSANSUP
            {
                operacao = "SUPRIMENTO DE ABERTURA DE CAIXA",
                valor = txb_Suprimento.Value,
                numcaixa = NO_CAIXA.ToString()
            };
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var Oper = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter() { Connection = LOCAL_FB_CONN };
            using FbCommand fbComm = new FbCommand() { CommandType = System.Data.CommandType.Text };
            DateTime abertura = Oper.GetByCaixaAberto(NO_CAIXA)[0].CURRENTTIME;
            try
            {
                //using (var PDV_OperTA = new DataSets.FDBDataSetVendaTableAdapters.TRI_PDV_OPERTableAdapter())

                fbComm.Parameters.AddWithValue("@pID_CAIXA", NO_CAIXA);
                fbComm.Parameters.AddWithValue("@pTS_ABERTURA", abertura);
                fbComm.Parameters.AddWithValue("@pVALOR", txb_Suprimento.Value);
                fbComm.Connection = LOCAL_FB_CONN;
                //PDV_OperTA.Connection = LOCAL_FB_CONN;
                //PDV_OperTA.SP_TRI_LANCASANSUP(NO_CAIXA, 0, (decimal)txb_Suprimento.Value);
                fbComm.CommandText = "INSERT INTO TRI_PDV_SANSUP " +
                                                 "(ID_SANSUP, ID_CAIXA, TS_ABERTURA, OPERACAO, VALOR, TS_OPERACAO) " +
                                                 "VALUES(0, @pID_CAIXA, @pTS_ABERTURA, 'U', @pVALOR, CURRENT_TIMESTAMP);";
                if (LOCAL_FB_CONN.State != System.Data.ConnectionState.Open)
                    LOCAL_FB_CONN.Open();
                fbComm.ExecuteNonQuery();
                LOCAL_FB_CONN.Close();
                //Lógica pra efetuar o procedimento e salvar no banco de dados.
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show(ex.Message);
                return;
            }
            var a = SANSUP.IMPRIME();//Imprime a primeira via,
            var b = SANSUP.IMPRIME();//E a segunda.
            if (!a | !b)
            {
                DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.Yes, DialogBoxIcons.Info, false, "A impressão falhou, porém o suprimento FOI contabilizado.", "Sr(a). Operador(a), favor anotar o valor do suprimento efetuado.", $"Suprimento: {SANSUP.valor:C2}.");
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                this.Close();
            }
        }
    }
}
