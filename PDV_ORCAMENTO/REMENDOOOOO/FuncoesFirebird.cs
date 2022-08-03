using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using PDV_ORCAMENTO.Properties;

namespace PDV_ORCAMENTO.REMENDOOOOO
{
    public class FuncoesFirebird
    {
        public DataTable PegaTodosOsItens()
        {
            FbConnection connection = new FbConnection(Settings.Default.NetworkDB);

            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            FbCommand command = new FbCommand();
            command.Connection = connection;
            command.CommandType = CommandType.Text;

            command.CommandText = $"SELECT a.ID_IDENTIFICADOR, a.COD_BARRA, c.DESCRICAO, a.REFERENCIA FROM TB_EST_PRODUTO a JOIN TB_EST_IDENTIFICADOR b ON b.ID_IDENTIFICADOR = a.ID_IDENTIFICADOR JOIN TB_ESTOQUE c ON c.ID_ESTOQUE = b.ID_ESTOQUE ORDER BY c.DESCRICAO";

            DataTable produtosTable = new();

            produtosTable.Load(command.ExecuteReader());

            return produtosTable;
        }
    }
}
