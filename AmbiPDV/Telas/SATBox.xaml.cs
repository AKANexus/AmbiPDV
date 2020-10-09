using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class SATBox : Window
    {
        private readonly FbRemoteEvent revent = new FbRemoteEvent(MontaStringDeConexao(SERVERNAME, SERVERCATALOG));

        public Dictionary<string, string> resposta { get; set; }
        public string[] retorno;
        public DialogBoxButtons dbb { get; set; }

        private int countdown = 0;
        public enum DialogBoxButtons { Yes, No, YesNo }
        public enum DialogBoxIcons { None, Info, Warn, Error, Dolan }
        public string cod_retorno { get; set; }
        private readonly DispatcherTimer timer;
        Logger log = new Logger("SatBOX");
        public SATBox(string title, string line1)
        {
            InitializeComponent();
            timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, SATTIMEOUT), DispatcherPriority.Normal, ChecaManualmentePorNovaEntrada, Dispatcher);
            try
            {
                revent.RemoteEventCounts += new EventHandler<FbRemoteEventCountsEventArgs>(OnEvent);
                revent.QueueEvents("NOVA_RESP_RECEBIDA");
                timer.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                FecharJanela(false);
                return;
            }

            #region Preparação de Interface - Não alterar
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run = new Run
            {
                Text = line1
            };
            tbl_Body.Inlines.Add(run);
            #endregion
        }


        private void ChecaManualmentePorNovaEntrada(object sender, EventArgs e)
        {
            countdown += 1;
            if (countdown >= SATSERVTIMEOUT)
            {
                timer.IsEnabled = false;
                log.Debug($"{countdown}s se passaram. Vou procurar eu mesmo.");
                switch (ObterRegistroASerProcessado())
                {
                    case true:
                        FecharJanela(true);
                        return;
                    case false:
                    case null:
                    default:
                        FecharJanela(false);
                        break;
                }
            }
        }

        private void FecharJanela(bool resultado)
        {
            try
            {
                DialogResult = resultado;
                timer.IsEnabled = false;
                revent.Dispose();
                this.Close();
                return;
            }
            catch (Exception ex)
            {
                log.Error("Erro ao fechar janela", ex);
            }
        }


        public void OnEvent(object source, EventArgs e)
        {
            log.Debug("Revent diz: Opa! Ouvi um \"NOVA_RESP_RECEBIDA\"!");
            switch (ObterRegistroASerProcessado())
            {
                case true:
                    FecharJanela(true);
                    return;
                case false:
                case null:
                default:
                    FecharJanela(false);
                    break;
            }
        }

        private bool? ObterRegistroASerProcessado()
        {
            log.Debug("Tentando ler um registro de SAT recebido.");
            using var SAT_REC_DT = new FDBDataSet.TRI_PDV_SAT_RECDataTable();
            using var SAT_REC_TA = new FDBDataSetTableAdapters.TRI_PDV_SAT_RECTableAdapter();
            try
            {
                log.Debug($"ServerName: {SERVERNAME}, ServerCatalog: {SERVERCATALOG}");
                SAT_REC_TA.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                SAT_REC_TA.FillByAbertos(SAT_REC_DT, NO_CAIXA);
                log.Debug($"SAT_REC_TA.FillbyAbertos: {SAT_REC_DT.Rows.Count}");
                if (SAT_REC_DT.Rows.Count > 0)
                {
                    log.Debug("Convertendo string em byte[] (retornoxml)");
                    byte[] retornoxml = (byte[])SAT_REC_DT.Rows[SAT_REC_DT.Rows.Count - 1]["XML_RECEB"];
                    cod_retorno = (string)SAT_REC_DT.Rows[SAT_REC_DT.Rows.Count - 1]["RETORNO_SAT"];
                    log.Debug($"cod_retorno: {cod_retorno}");
                    retorno = System.Text.Encoding.UTF8.GetString(retornoxml).Split('|');
                    SAT_REC_TA.SetaProcessado(NO_CAIXA);
                    log.Debug("Setou processado");
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error("Falha ao consultar registro a ser processado", ex);
                return null;
            }

            return false;

        }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            return;
        }
    }
}
