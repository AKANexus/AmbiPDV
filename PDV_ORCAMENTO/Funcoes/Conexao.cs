using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;

namespace PDV_ORCAMENTO
{
    class Conexao
    {
        static FbConnection _conexaoFb;
        public static bool RespostaConexao;

        public static FbConnection conexao
        {
            get
            {
                return _conexaoFb;
            }
        }

        public static bool conectar()
        {
            try
            {
                string strconexao1;

                strconexao1 = Properties.Settings.Default.NetworkDB;

                _conexaoFb = new FbConnection(strconexao1);
                _conexaoFb.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool desconectar()
        {
            _conexaoFb.Close();
            _conexaoFb = null;

            return false;
        }
    }
}
