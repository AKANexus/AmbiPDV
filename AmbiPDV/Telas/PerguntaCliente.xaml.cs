﻿using FirebirdSql.Data.FirebirdClient;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for PerguntaCliente.xaml
    /// </summary>
    public partial class PerguntaCliente : Window
    {
        #region Fields & Properties

        public int id_cliente { get; set; }
        public string nome_cliente { get; set; }
        public DateTime? vencimento { get; set; }
        private bool _modoteste;

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public PerguntaCliente(int id_pgto, bool modoTeste = false)
        {
            //DataContext = new ViewModels.MainViewModel();
            _modoteste = modoTeste;
            InitializeComponent();
            PreencherCombobox();
            cbb_Cliente.Focus();

            DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCERow metodoRow;
            try
            {
                using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
                using var Metodos_TA = new DataSets.FDBDataSetVendaTableAdapters.TB_FORMA_PAGTO_NFCETableAdapter();
                using var Metodos_DT = new DataSets.FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable();
                Metodos_TA.Connection = LOCAL_FB_CONN;
                metodoRow = (from linha in Metodos_DT.AsEnumerable()
                             where linha.ID_FMANFCE == id_pgto
                             select linha).FirstOrDefault();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao consultar método de pagamento. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                Environment.Exit(0); // deuruim();
                return;
            }

            try
            {
                /*if ((string)metodoRow[0]["METODO"] == "C" && (int)metodoRow[0]["DIAS"] > 0)
                {
                    dtp_Vencimento.SelectedDate = DateTime.Today.AddDays((int)metodoRow[0]["DIAS"]);
                }
                else*/
                if (/*(string)metodoRow[0]["METODO"] == "F" && (int)metodoRow[0]["DIAS"] > 0*/true)
                {
                    int dia = 10;
                    DateTime hoje = DateTime.Today;
                    DateTime vcto;

                    if (DateTime.Today.Day < dia)
                    {
                        vcto = new DateTime(hoje.Year, hoje.Month, dia);
                        dtp_Vencimento.SelectedDate = vcto;
                    }
                    else
                    {
                        if (hoje.Month == 12) vcto = new DateTime(hoje.Year + 1, 1, dia);
                        else vcto = new DateTime(hoje.Year, hoje.Month + 1, dia);
                        dtp_Vencimento.SelectedDate = vcto;
                    }
                }
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao processar método de pagamento. \n\nO aplicativo deverá ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                Environment.Exit(0); // deuruim();
                return;
            }

            //DataContext = new MainViewModel();

            if (modoTeste)
            {
                Random rand = new Random();
                cbb_Cliente.SelectedIndex = rand.Next(cbb_Cliente.Items.Count);
                ProcessarDataECliente();
            }
        }

        private void PreencherCombobox()
        {
            cbb_Cliente.Items.Clear();
            foreach (var item in clientesOC)
            {
                cbb_Cliente.Items.Add(item);
            }
        }

        #endregion (De)Constructor






        #region Events

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
            {
                ProcessarDataECliente();
            });
        }

        private void ProcessarDataECliente()
        {
            try
            {
                using (var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) })
                using (var Cliente_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter())
                {
                    Cliente_TA.Connection = LOCAL_FB_CONN;
                    id_cliente = (int)Cliente_TA.PegaIDPorCliente(cbb_Cliente.SelectedItem.ToString());
                    var ativo = (from cliente in Cliente_TA.GetData()
                                where cliente.ID_CLIENTE == id_cliente
                                select cliente.STATUS).FirstOrDefault();
                    if (ativo != "A")
                    {
                        var mensagem = (from cliente in Cliente_TA.GetData()
                                        where cliente.ID_CLIENTE == id_cliente
                                        select cliente.OBSERVACAO).FirstOrDefault();
                        DialogBox.Show("Venda a prazo", DialogBoxButtons.No, DialogBoxIcons.Warn, false, "Impossível fazer venda a prazo para esse cliente.", mensagem);
                        return;
                    }
                }
                nome_cliente = cbb_Cliente.SelectedItem.ToString();
                vencimento = dtp_Vencimento.SelectedDate;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao pescar ID_CLIENTE");
            }
        }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
            else if (e.Key == Key.F5)
            {
                CarregarClientesOC();
                PreencherCombobox();
                MessageBox.Show("Concluído. Confira!");
            }
        }

        private void cbb_Cliente_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dtp_Vencimento.Focus();
            }
        }

        private void dtp_Vencimento_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    ProcessarDataECliente();
                });
            }
        }

        private void but_Confirmar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                debounceTimer.Debounce(250, (p) => //DEBOUNCER: gambi pra não deixar o usuário clicar mais de uma vez enquanto não terminar o processamento.
                {
                    ProcessarDataECliente();
                });
            }
        }

        #endregion Events

        #region Methods

        #endregion Methods
    }
}