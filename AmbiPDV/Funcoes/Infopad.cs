using System;
using System.Collections.Generic;
using System.Data;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Funcoes
{
    public class Infopad
    {
        public static List<decimal> Quantidades = new List<decimal>();
        public static List<int> Codigos = new List<int>();
        public static bool LeComandas(int comanda)
        {

            //var ComDS = new ComandasDS();

            //MessageBox.Show(Vendas_TA.Connection.ConnectionString);

            //List<decimal> qnts = new List<decimal>();
            //List<int> cods = new List<int>();

            try
            {
                using var Vendas_TA = new ComandasDSTableAdapters.VENDASTableAdapter();
                using ComandasDS.VENDASDataTable dt = Vendas_TA.LeComanda(comanda);
                Vendas_TA.Connection.ConnectionString = Properties.Settings.Default.ComandasCS;

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
        public static void FechaComanda(int comanda)
        {
            //using (ComandasDS ComandasDS = new ComandasDS())

            //using (var SERVER_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG) })
            using ComandasDSTableAdapters.VENDASTableAdapter Vendas_TA = new ComandasDSTableAdapters.VENDASTableAdapter();
            Vendas_TA.Connection.ConnectionString = Properties.Settings.Default.ComandasCS;
            Vendas_TA.FechaComanda(comanda);
        }
    }
}
