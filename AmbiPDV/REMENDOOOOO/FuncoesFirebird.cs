using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;

namespace PDV_WPF.REMENDOOOOO
{
    public class FuncoesFirebird
    {
        public async Task<decimal> SomaDeValoresAsync(System.DateTime DT_ABERTURA, int INT_FMANFCE, string STR_SERIE,
            System.DateTime DT_FECHAMENTO, FbConnection connection)
        {
            try
            {
                await connection.OpenAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            FbCommand command = new FbCommand();
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            string hrAbertura = DT_ABERTURA.ToString("hh:mm:ss");
            string hrFechamento = DT_FECHAMENTO.ToString("hh:mm:ss");
            string dtAbertura = DT_ABERTURA.ToString("yyyy-MM-dd");
            string dtFechamento = DT_FECHAMENTO.ToString("yyyy-MM-dd");

            command.CommandText = $"SELECT SUM(A.VLR_PAGTO) FROM TB_NFVENDA_FMAPAGTO_NFCE A INNER JOIN TB_NFVENDA B ON A.ID_NFVENDA = B.ID_NFVENDA WHERE B.DT_SAIDA BETWEEN {dtAbertura} AND {dtFechamento} AND B.HR_SAIDA BETWEEN {hrAbertura} AND {hrFechamento} AND B.STATUS = 'I' AND A.ID_FMANFCE = {INT_FMANFCE} AND B.NF_SERIE = {STR_SERIE}";

            decimal resultado;
            try
            {
                resultado = await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                await connection.CloseAsync();
            }

            return resultado;
        }

        public decimal SomaDeValores(System.DateTime DT_ABERTURA, int INT_FMANFCE, string STR_SERIE,
            System.DateTime DT_FECHAMENTO, FbConnection connection)
        {
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
            string hrAbertura = DT_ABERTURA.ToString("hh:mm:ss");
            string hrFechamento = DT_FECHAMENTO.ToString("hh:mm:ss");
            string dtAbertura = DT_ABERTURA.ToString("yyyy-MM-dd");
            string dtFechamento = DT_FECHAMENTO.ToString("yyyy-MM-dd");

            command.CommandText = $"SELECT SUM(A.VLR_PAGTO) FROM TB_NFVENDA_FMAPAGTO_NFCE A INNER JOIN TB_NFVENDA B ON A.ID_NFVENDA = B.ID_NFVENDA WHERE B.DT_SAIDA BETWEEN '{dtAbertura}' AND '{dtFechamento}' AND B.HR_SAIDA BETWEEN '{hrAbertura}' AND '{hrFechamento}' AND B.STATUS = 'I' AND A.ID_FMANFCE = '{INT_FMANFCE}' AND B.NF_SERIE = '{STR_SERIE}'";

            decimal resultado;
            try
            {
                var result = command.ExecuteScalar();
                resultado = result is DBNull ? 0 : (decimal) result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                connection.Close();
            }

            return resultado;
        }
    }
}
