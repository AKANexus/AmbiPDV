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
        //public async Task<decimal> SomaDeValoresAsync(System.DateTime DT_ABERTURA, int INT_FMANFCE, string STR_SERIE,
        //    System.DateTime DT_FECHAMENTO, FbConnection connection)
        //{
        //    try
        //    {
        //        await connection.OpenAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }

        //    FbCommand command = new FbCommand();
        //    command.Connection = connection;
        //    command.CommandType = CommandType.Text;
        //    string hrAbertura = DT_ABERTURA.ToString("hh:mm:ss");
        //    string hrFechamento = DT_FECHAMENTO.ToString("hh:mm:ss");
        //    string dtAbertura = DT_ABERTURA.ToString("yyyy-MM-dd");
        //    string dtFechamento = DT_FECHAMENTO.ToString("yyyy-MM-dd");

        //    command.CommandText = $"SELECT SUM(A.VLR_PAGTO) FROM TB_NFVENDA_FMAPAGTO_NFCE A INNER JOIN TB_NFVENDA B ON A.ID_NFVENDA = B.ID_NFVENDA WHERE B.DT_SAIDA BETWEEN {dtAbertura} AND {dtFechamento} AND B.HR_SAIDA BETWEEN {hrAbertura} AND {hrFechamento} AND B.STATUS = 'I' AND A.ID_FMANFCE = {INT_FMANFCE} AND B.NF_SERIE = {STR_SERIE}";

        //    decimal resultado;
        //    try
        //    {
        //        resultado = await command.ExecuteNonQueryAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //    finally
        //    {
        //        await connection.CloseAsync();
        //    }

        //    return resultado;
        //}

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
            string hrAbertura = DT_ABERTURA.ToString("HH:mm:ss");
            string hrFechamento = DT_FECHAMENTO.ToString("HH:mm:ss");
            string dtAbertura = DT_ABERTURA.ToString("yyyy-MM-dd");
            string dtFechamento = DT_FECHAMENTO.ToString("yyyy-MM-dd");

            command.CommandText = $"SELECT SUM(A.VLR_PAGTO) FROM TB_NFVENDA_FMAPAGTO_NFCE A INNER JOIN TB_NFVENDA B ON A.ID_NFVENDA = B.ID_NFVENDA WHERE B.DT_SAIDA BETWEEN '{dtAbertura}' AND '{dtFechamento}' AND B.HR_SAIDA BETWEEN '{hrAbertura}' AND '{hrFechamento}' AND B.STATUS = 'I' AND A.ID_FMANFCE = '{INT_FMANFCE}' AND B.NF_SERIE = '{STR_SERIE}'";

            decimal resultado;
            try
            {
                var result = command.ExecuteScalar();
                resultado = result is DBNull ? 0 : (decimal)result;
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

        public InfoAtacado? GetInfoAtacado(int idIdentificador, FbConnection connection)
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

            command.CommandText =
                $"SELECT PRC_ATACADO, QTD_ATACADO, OBSERVACAO FROM TB_ESTOQUE te JOIN TB_EST_IDENTIFICADOR tei ON te.ID_ESTOQUE = tei.ID_ESTOQUE WHERE tei.ID_IDENTIFICADOR = {idIdentificador}";
            DataTable infoTable = new();
            try
            {
                infoTable.Load(command.ExecuteReader());
            }
            finally
            {
                connection.Close();
            }

            if (infoTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                return new InfoAtacado
                {
                    PrcAtacado = infoTable.Rows[0]["PRC_ATACADO"] is DBNull ? 0 : infoTable.Rows[0]["PRC_ATACADO"] as decimal? ?? 0,
                    QtdAtacado = infoTable.Rows[0]["QTD_ATACADO"] is DBNull ? 0 : infoTable.Rows[0]["QTD_ATACADO"] as decimal? ?? 0,
                    Família = ""
                };
            }

        }

        public DadosDoItem? ObtemDadosDoItem(int codigoitem, FbConnection connection)
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

            command.CommandText =
                $"SELECT A.DESCRICAO, A.CFOP, A.UNI_MEDIDA, C.COD_NCM, C.COD_BARRA, C.CSOSN_CFE, " +
                $"C.CST_CFE, A.CST_PIS, A.CST_COFINS, A.PIS, A.COFINS, COALESCE(D.UF_SP, 0) AS RUF_SP, " +
                $"COALESCE(D.BASE_ICMS, 0)AS RBASE_ICMS, COALESCE(E.ISS_ALIQ, 0) AS RALIQ_ISS, " +
                $"A.ID_TIPOITEM, C.COD_CEST, A.OBSERVACAO FROM TB_ESTOQUE A INNER JOIN TB_EST_IDENTIFICADOR B ON " +
                $"(A.ID_ESTOQUE = B.ID_ESTOQUE) LEFT JOIN TB_TAXA_UF D ON A.ID_CTI_CFE = D.ID_CTI " +
                $"LEFT JOIN TB_EST_PRODUTO C ON B.ID_IDENTIFICADOR = C.ID_IDENTIFICADOR LEFT JOIN " +
                $"TB_EST_SERVICO E ON E.ID_IDENTIFICADOR = B.ID_IDENTIFICADOR WHERE B.ID_IDENTIFICADOR = {codigoitem}";

            DataTable infoDoItem = new();

            try
            {
                infoDoItem.Load(command.ExecuteReader());
            }
            finally
            {
                connection.Close();
            }

            if (infoDoItem.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                var row = infoDoItem.Rows[0];
                return new DadosDoItem
                {
                    DESCRICAO = row["DESCRICAO"] is DBNull ? "ITEM AVULSO" : row["DESCRICAO"] as string ?? "ITEM AVULSO",
                    CFOP = row["CFOP"] is DBNull ? "5102" : row["CFOP"] as string ?? "5102",
                    UNI_MEDIDA = row["UNI_MEDIDA"] is DBNull ? "UN" : row["UNI_MEDIDA"] as string ?? "UN",
                    COD_NCM = row["COD_NCM"] is DBNull ? "00" : row["COD_NCM"] as string ?? "00",
                    COD_BARRA = row["COD_BARRA"] is DBNull ? string.Empty : row["COD_BARRA"] as string ?? string.Empty,
                    RCSOSN_CFE = row["CSOSN_CFE"] is DBNull ? string.Empty : row["CSOSN_CFE"] as string ?? string.Empty,
                    RCST_CFE = row["CST_CFE"] is DBNull ? string.Empty : row["CST_CFE"] as string ?? string.Empty,
                    RCST_PIS = row["CST_PIS"] is DBNull ? string.Empty : row["CST_PIS"] as string ?? string.Empty,
                    RCST_COFINS = row["CST_COFINS"] is DBNull ? string.Empty : row["CST_COFINS"] as string ?? string.Empty,
                    RPIS = row["PIS"] is DBNull ? 0 : row["PIS"] as decimal? ?? 0,
                    RCOFINS = row["COFINS"] is DBNull ? 0 : row["COFINS"] as decimal? ?? 0,
                    RUF_SP = row["RUF_SP"] is DBNull ? 0 : row["RUF_SP"] as decimal? ?? 0,
                    RBASE_ICMS = row["RBASE_ICMS"] is DBNull ? 0 : row["RBASE_ICMS"] as decimal? ?? 0,
                    RALIQ_ISS = row["RALIQ_ISS"] is DBNull ? 0 : row["RALIQ_ISS"] as decimal? ?? 0,
                    RID_TIPOITEM = row["ID_TIPOITEM"] is DBNull ? "0" : row["ID_TIPOITEM"] as string ?? "0",
                    RSTR_CEST = row["COD_CEST"] is DBNull ? string.Empty : row["COD_CEST"] as string ?? string.Empty,
                    OBSERVACAO = row["OBSERVACAO"] is DBNull ? "Trabalho de corno do caralho" : row["OBSERVACAO"] as string ?? string.Empty
                };
            }
        }
    }

    public class InfoAtacado
    {
        public decimal QtdAtacado { get; set; }
        public decimal PrcAtacado { get; set; }
        public string Família { get; set; }
    }

    public class DadosDoItem
    {
        public string DESCRICAO { get; set; }
        public string CFOP { get; set; }
        public string UNI_MEDIDA { get; set; }
        public string COD_NCM { get; set; }
        public string COD_BARRA { get; set; }
        public string RCSOSN_CFE { get; set; }
        public string RCST_CFE { get; set; }
        public string RCST_PIS { get; set; }
        public string RCST_COFINS { get; set; }
        public decimal RPIS { get; set; }
        public decimal RCOFINS { get; set; }
        public decimal RUF_SP { get; set; }
        public decimal RBASE_ICMS { get; set; }
        public decimal RALIQ_ISS { get; set; }
        public string RID_TIPOITEM { get; set; }
        public string RSTR_CEST { get; set; }
        public string OBSERVACAO { get; set; }
    }
}
