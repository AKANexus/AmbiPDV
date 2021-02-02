using System;
using System.Collections.Generic;
using System.Data;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;

namespace PDV_WPF.Funcoes
{
    public class Infopad
    {
        private Logger log = new Logger("Infopad");

        public List<decimal> Quantidades = new List<decimal>();
        public List<int> Codigos = new List<int>();
        public bool LeComandas(int comanda)
        {

            //var ComDS = new ComandasDS();

            //MessageBox.Show(Vendas_TA.Connection.ConnectionString);

            //List<decimal> qnts = new List<decimal>();
            //List<int> cods = new List<int>();

            try
            {
                log.Debug("LeComandas inicializado");
                log.Debug($"ConnString: {MontaStringDeConexao(SERVERNAME, COMANDASCATALOG)}");
                FbConnection fbConn = new FbConnection(MontaStringDeConexao(SERVERNAME, COMANDASCATALOG));
                log.Debug($"ConnString: {MontaStringDeConexao(SERVERNAME, COMANDASCATALOG)}");
                FbCommand fbComm = new FbCommand() { Connection = fbConn, CommandType = CommandType.StoredProcedure };
                fbConn.Open();
                fbComm.CommandText = "LECOMANDA";
                fbComm.Parameters.Add("COMM", comanda);
                using ComandasDS.VENDASDataTable dt = new ComandasDS.VENDASDataTable();
                dt.Load(fbComm.ExecuteReader());
                fbConn.Close();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt)
                    {
                        Quantidades.Add(Convert.ToDecimal(row[4]));
                        Codigos.Add(Convert.ToInt32(row[2]));
                    }
                }
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                return false;
            }
            return true;
        }
        public void FechaComanda(int comanda)
        {
            //using (ComandasDS ComandasDS = new ComandasDS())

            //using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
            //using ComandasDSTableAdapters.VENDASTableAdapter Vendas_TA = new ComandasDSTableAdapters.VENDASTableAdapter();
            FbConnection fbConn = new FbConnection(MontaStringDeConexao(SERVERNAME, COMANDASCATALOG));
            FbCommand fbComm = new FbCommand() { Connection = fbConn, CommandType = CommandType.Text };
            fbConn.Open();
            fbComm.CommandText = $"UPDATE  VENDAS SET FECHADA = 1 WHERE(COMANDA = {comanda})";
            fbComm.ExecuteNonQuery();
            fbConn.Close();
        }
    }
}
