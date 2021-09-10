using Clearcove.Logging;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


namespace PDV_WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _funcionarios;
        public ObservableCollection<string> Funcionarios
        {
            get { return _funcionarios; }
            set
            {
                _funcionarios = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Funcionarios"));
            }
        }
        private ObservableCollection<string> _clientes;
        public ObservableCollection<string> Clientes
        {
            get { return _clientes; }
            set
            {
                _clientes = value;
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Clientes"));
            }
        }
        private ObservableCollection<string> _parcelamentos;
        public ObservableCollection<string> Parcelamentos
        {
            get { return _parcelamentos; }
            set
            {
                _parcelamentos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Parcelamentos"));
            }
        }
        private ObservableCollection<string> _vendedores;

        public ObservableCollection<string> Vendedores
        {
            get { return _vendedores; }
            set
            {
                _vendedores = value;
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Vendedores"));
            }
        }

  

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel(bool funcionarios = true, bool clientes = true)
        {

            Logger log = new Logger("MainViewModel");
            var _funcoes = new funcoesClass();
            using var fUNCIONARIOTableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.TB_FUNCIONARIOTableAdapter();
            using var cLIENTETableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.TB_CLIENTETableAdapter();
            //using (var cLIENTETableAdapter = new DataSets.FDBDataSetOperSeedTableAdapters.SP_TRI_CLIENTESATIVOSTableAdapter())
            using var pARCELAMENTOTableAdapter = new DataSets.FDBDataSetVendaTableAdapters.TB_PARCELAMENTOTableAdapter();
            using var dt_parc = new DataSets.FDBDataSetVenda.TB_PARCELAMENTODataTable();
            using var dt_func = new DataSets.FDBDataSetOperSeed.TB_FUNCIONARIODataTable();
            using var dt_vend = new DataSets.FDBDataSetOperSeed.TB_FUNCIONARIODataTable();
            using var dt_cli = new DataSets.FDBDataSetOperSeed.TB_CLIENTEDataTable();
            bool? resultado = _funcoes.TestaConexaoComServidor(SERVERNAME, SERVERCATALOG, FBTIMEOUT);
            if (resultado == true)
            {
                resultado = true;
                log.Debug("funcoes.TestaConexaoComServidor terminou em menos de 3 segundos.");
                log.Debug(String.Format("ConexaoServidorOk=task.Result = {0}", true));
            }
            else
            {
                resultado = false;
                log.Debug("funcoes.TestaConexaoComServidor deu timeout.");
                log.Debug("ConexaoServidorOk = false");
            }

            string strConn = string.Empty;

            switch (resultado)
            {
                case true:
                    strConn = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                    _funcoes.ChangeConnectionString(strConn);
                    fUNCIONARIOTableAdapter.Connection.ConnectionString = strConn;
                    cLIENTETableAdapter.Connection.ConnectionString = strConn;
                    pARCELAMENTOTableAdapter.Connection.ConnectionString = strConn;
                    log.Debug("FDBConnString definido para DB na rede:");
                    log.Debug(Settings.Default.FDBConnString);
                    break;
                case false:
                    strConn = MontaStringDeConexao("localhost", localpath);
                    _funcoes.ChangeConnectionString(strConn);
                    fUNCIONARIOTableAdapter.Connection.ConnectionString = strConn;
                    cLIENTETableAdapter.Connection.ConnectionString = strConn;
                    pARCELAMENTOTableAdapter.Connection.ConnectionString = strConn;
                    log.Debug("FDBConnString definido para DB de contingência:");
                    log.Debug(Settings.Default.FDBConnString);
                    break;
                case null:
                    break;
            }

            try
            {
                if (funcionarios)
                {
                    fUNCIONARIOTableAdapter.FillByVendedores(dt_func);
                    fUNCIONARIOTableAdapter.FillByComissionados(dt_vend);
                }
                if (clientes)
                {
                    cLIENTETableAdapter.FillOrderByName(dt_cli);
                }
                pARCELAMENTOTableAdapter.Fill(dt_parc);
                log.Debug("DT's preenchidas");
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message.Contains("I/O Error"))
                {
                    DialogBox.Show("Erro de conexão.", DialogBoxButtons.No, DialogBoxIcons.Error, true, "Arquivo *.FDB não foi encontrado.", "Favor executar o assistente de configuração, e tente novamente");
                    Application.Current.Shutdown();
                    return;
                }
                else
                {
                    if (dt_func.Rows.Count > 0)
                    {
                        log.Error("Erro ao sincronizar dt_func:\nRegistro " +
                                           dt_func.GetErrors()[0][0] +
                                           " retornou um erro: " +
                                           dt_func.GetErrors()[0].RowError, ex);
                    }

                    if (dt_cli.Rows.Count > 0)
                    {
                        log.Error("Erro ao sincronizar dt_cli:\nRegistro " +
                                           dt_cli.GetErrors()[0][0] +
                                           " retornou um erro: " +
                                           dt_cli.GetErrors()[0].RowError, ex);
                    }

                    if (dt_parc.Rows.Count > 0)
                    {
                        log.Error("Erro ao sincronizar dt_parc:\nRegistro " +
                                           dt_parc.GetErrors()[0][0] +
                                           " retornou um erro: " +
                                           dt_parc.GetErrors()[0].RowError, ex);
                    }

                    if (dt_vend.Rows.Count > 0)
                    {
                        log.Error("Erro ao sincronizar dt_parc:\nRegistro " +
                                           dt_vend.GetErrors()[0][0] +
                                           " retornou um erro: " +
                                           dt_vend.GetErrors()[0].RowError, ex);
                    }
                }
                throw ex;

            }

            Funcionarios = new ObservableCollection<string>();
            foreach (DataRow row in dt_func)
            {
                Funcionarios.Add(row["NOME"].ToString());
            }

            Clientes = new ObservableCollection<string>();
            foreach (DataRow row in dt_cli)
            {
                Clientes.Add(row["NOME"].ToString());
            }
            Parcelamentos = new ObservableCollection<string>();
            foreach (DataRow row in dt_parc)
            {
                Parcelamentos.Add(row["DESCRICAO"].ToString());
            }
            Vendedores = new ObservableCollection<string>();
            foreach (DataRow row in dt_vend)
            {
                Vendedores.Add(row["NOME"].ToString());
            }
        }

    }
}
