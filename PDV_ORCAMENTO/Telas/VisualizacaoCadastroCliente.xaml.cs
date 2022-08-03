using PDV_WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static PDV_WPF.Extensoes;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Interaction logic for VisualizacaoCadastroCliente.xaml
    /// </summary>
    public partial class VisualizacaoCadastroCliente : Window
    {
        #region Fields & Properties

        //private int _IdCliente;

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public VisualizacaoCadastroCliente(int pIdCliente)
        {
            InitializeComponent();

            //_IdCliente = pIdCliente;

            CarregarDadosCliente(pIdCliente);
        }

        #endregion (De)Constructor

        #region Events

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    this.Close();
                });
            } // Fecha a tela de visualização de cadastro de cliente
        }

        #endregion Events

        #region Methods

        private void CarregarDadosCliente(int pIdCliente)
        {
            try
            {
                using (var taClienteServ = new FDBOrcaDataSetTableAdapters.SP_TRI_ORCA_CLIENTE_GETBY_IDTableAdapter())
                using (var tbClienteServ = new FDBOrcaDataSet.SP_TRI_ORCA_CLIENTE_GETBY_IDDataTable())
                {
                    try
                    {
                        taClienteServ.Fill(tbClienteServ, pIdCliente);
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao consultar dados do cliente. \nPor favor verifique os dados e tente novamente.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        this.Close();
                    }

                    if (tbClienteServ.Rows.Count <= 0)
                    {
                        string strMensagem = "Cadastro de cliente não encontrado! ID: " + pIdCliente.ToString();
                        gravarMensagemErro(strMensagem);
                        MessageBox.Show(strMensagem);
                        return;
                    }

                    #region Binding nos controles

                    #region Identificação

                    //  b.NOME AS CLIENTE_NOME
                    txbClienteNome.Text = tbClienteServ[0]["CLIENTE_NOME"].Safestring();
                    //, e.NOME_FANTA AS CLIENTE_NOME_FANTA
                    txbClienteNomeFanta.Text = tbClienteServ[0]["CLIENTE_NOME_FANTA"].Safestring();
                    //, d.CPF AS CLIENTE_CPF
                    //, e.CNPJ AS CLIENTE_CNPJ
                    txbClienteCpfCnpj.Text = tbClienteServ[0].IsCLIENTE_CNPJNull() ? (tbClienteServ[0].IsCLIENTE_CPFNull() ? (string.Empty)
                                                                                                                           : tbClienteServ[0]["CLIENTE_CPF"].Safestring())
                                                                                   : tbClienteServ[0]["CLIENTE_CNPJ"].Safestring();
                    //, d.IDENTIDADE AS CLIENTE_RG
                    txbClienteRg.Text = tbClienteServ[0].IsCLIENTE_RGNull() ? string.Empty : tbClienteServ[0]["CLIENTE_RG"].Safestring();
                    //, e.INSC_ESTAD
                    txbClienteInscricaoEstadual.Text = tbClienteServ[0].IsINSC_ESTADNull() ? string.Empty : tbClienteServ[0]["INSC_ESTAD"].Safestring();
                    //, e.INSC_MUNIC
                    txbClienteInscricaoMunicipal.Text = tbClienteServ[0].IsINSC_MUNICNull() ? string.Empty : tbClienteServ[0]["INSC_MUNIC"].Safestring();

                    #endregion Identificação

                    #region Endereço

                    //, b.END_TIPO
                    txbClienteEnderecoTipo.Text = tbClienteServ[0].IsEND_TIPONull() ? string.Empty : tbClienteServ[0]["END_TIPO"].Safestring();
                    //, b.END_LOGRAD
                    txbClienteEnderecoLogradouro.Text = tbClienteServ[0].IsEND_LOGRADNull() ? string.Empty : tbClienteServ[0]["END_LOGRAD"].Safestring();
                    //, b.END_NUMERO
                    txbClienteEnderecoNumero.Text = tbClienteServ[0].IsEND_NUMERONull() ? string.Empty : tbClienteServ[0]["END_NUMERO"].Safestring();
                    //, b.END_COMPLE
                    txbClienteEnderecoComplemento.Text = tbClienteServ[0].IsEND_COMPLENull() ? string.Empty : tbClienteServ[0]["END_COMPLE"].Safestring();
                    //, b.END_BAIRRO
                    txbClienteEnderecoBairro.Text = tbClienteServ[0].IsEND_BAIRRONull() ? string.Empty : tbClienteServ[0]["END_BAIRRO"].Safestring();
                    //, b.END_CEP
                    txbClienteEnderecoCep.Text = tbClienteServ[0].IsEND_CEPNull() ? string.Empty : tbClienteServ[0]["END_CEP"].Safestring();
                    //, c.NOME AS CIDADE_NOME
                    txbClienteEnderecoCidade.Text = tbClienteServ[0].IsCIDADE_NOMENull() ? string.Empty : tbClienteServ[0]["CIDADE_NOME"].Safestring();
                    //, c.SIGLA_UF
                    txbClienteEnderecoUf.Text = tbClienteServ[0].IsSIGLA_UFNull() ? string.Empty : tbClienteServ[0]["SIGLA_UF"].Safestring();

                    #endregion Endereço

                    #region Contato

                    //, b.DDD_COMER
                    //, b.FONE_COMER
                    txb_TelefoneComercial.Value = (tbClienteServ[0].IsDDD_COMERNull() ? string.Empty : tbClienteServ.Rows[0]["DDD_COMER"].Safestring()) +
                                                  (tbClienteServ[0].IsFONE_COMERNull() ? string.Empty : tbClienteServ[0]["FONE_COMER"].Safestring());
                    //, b.DDD_FAX
                    //, b.FONE_FAX
                    txb_Fax.Value = (tbClienteServ[0].IsDDD_FAXNull() ? string.Empty : tbClienteServ.Rows[0]["DDD_FAX"].Safestring()) +
                                    (tbClienteServ[0].IsFONE_FAXNull() ? string.Empty : tbClienteServ[0]["FONE_FAX"].Safestring());
                    //, b.DDD_CELUL
                    //, b.FONE_CELUL
                    txb_TelefoneCelular.Value = (tbClienteServ[0].IsDDD_CELULNull() ? string.Empty : tbClienteServ.Rows[0]["DDD_CELUL"].Safestring()) +
                                                (tbClienteServ[0].IsFONE_CELULNull() ? string.Empty : tbClienteServ[0]["FONE_CELUL"].Safestring());
                    //, b.DDD_RESID
                    //, b.FONE_RESID
                    txb_TelefoneResidencial.Value = (tbClienteServ[0].IsDDD_RESIDNull() ? string.Empty : tbClienteServ.Rows[0]["DDD_RESID"].Safestring()) +
                                                    (tbClienteServ[0].IsFONE_RESIDNull() ? string.Empty : tbClienteServ[0]["FONE_RESID"].Safestring());
                    //, b.EMAIL_CONT
                    txbClienteEmailCont.Text = tbClienteServ[0].IsEMAIL_CONTNull() ? string.Empty : tbClienteServ[0]["EMAIL_CONT"].Safestring();

                    #endregion Contato

                    #endregion Binding nos controles
                }
            }
            catch (Exception ex)
            {
                string strErrMess = "Erro ao consultar dados do cliente. \nPor favor tente novamente.";
                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                MessageBox.Show(strErrMess);
            }
        }

        #endregion Methods


    }
}
