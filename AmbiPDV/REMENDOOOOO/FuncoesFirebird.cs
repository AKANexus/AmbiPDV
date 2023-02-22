using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using DateTime = System.DateTime;
using PDV_WPF.DataSets;
using static PDV_WPF.Funcoes.Statics;

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
                if(connection.State == ConnectionState.Closed) 
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

            //command.CommandText = $"SELECT SUM(A.VLR_PAGTO) FROM TB_NFVENDA_FMAPAGTO_NFCE A INNER JOIN TB_NFVENDA B ON A.ID_NFVENDA = B.ID_NFVENDA WHERE CAST(B.DT_SAIDA || ' ' || B.HR_SAIDA AS TIMESTAMP) BETWEEN '{DT_ABERTURA:yyyy-MM-dd HH-mm-ss}' AND '{DT_FECHAMENTO:yyyy-MM-dd HH-mm-ss}' AND B.STATUS = 'I' AND A.ID_FMANFCE = {INT_FMANFCE} AND B.NF_SERIE = '{STR_SERIE}'";
            command.CommandText = $"EXECUTE BLOCK RETURNS (VLR_TOT NUMERIC(18,4)) AS " +
                                  $"DECLARE VARIABLE DT_APLICADA DATE; DECLARE VARIABLE VLR_PAGO NUMERIC(18,4); " +
                                  $"BEGIN DT_APLICADA = DATEADD(-4 DAY TO CURRENT_DATE); VLR_TOT = 0; " +
                                  $"FOR SELECT A.VLR_PAGTO FROM TB_NFVENDA_FMAPAGTO_NFCE A INNER JOIN TB_NFVENDA B ON A.ID_NFVENDA = B.ID_NFVENDA WHERE B.DT_SAIDA >= :DT_APLICADA AND B.STATUS = 'I' AND A.ID_FMANFCE = {INT_FMANFCE} AND B.NF_SERIE = '{STR_SERIE}' AND (B.DT_SAIDA || ' ' || B.HR_SAIDA) BETWEEN '{DT_ABERTURA:yyyy-MM-dd HH:mm:ss}' AND '{DT_FECHAMENTO:yyyy-MM-dd HH:mm:ss}' INTO :VLR_PAGO DO " +
                                  $"BEGIN VLR_TOT = VLR_TOT + VLR_PAGO; END SUSPEND; END";

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
                $"A.ID_TIPOITEM, C.COD_CEST, A.OBSERVACAO, F.DESCRICAO AS COR, G.DESCRICAO AS TAMANHO FROM TB_ESTOQUE A INNER JOIN TB_EST_IDENTIFICADOR B ON " +
                $"(A.ID_ESTOQUE = B.ID_ESTOQUE) LEFT JOIN TB_TAXA_UF D ON A.ID_CTI_CFE = D.ID_CTI " +
                $"LEFT JOIN TB_EST_PRODUTO C ON B.ID_IDENTIFICADOR = C.ID_IDENTIFICADOR LEFT JOIN " +
                $"TB_EST_SERVICO E ON E.ID_IDENTIFICADOR = B.ID_IDENTIFICADOR LEFT JOIN TB_EST_PROD_NIVEL1 F ON " +
                $"C.ID_NIVEL1 = F.ID_NIVEL1 LEFT JOIN TB_EST_PROD_NIVEL2 G ON C.ID_NIVEL2 = G.ID_NIVEL2 WHERE B.ID_IDENTIFICADOR = {codigoitem}";

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
                using (var taPromoServ = new DataSets.FDBDataSetOperSeedTableAdapters.TB_PROMOCOES_ITENSTableAdapter())
                {
                    var row = infoDoItem.Rows[0];

                    taPromoServ.Connection = connection;
                    string parametro = row["COD_BARRA"].ToString();
                    int? idScannTech = (int?)taPromoServ.ScalarByCod(parametro); //gambiarraa da poha mas fodace                    
                                                                                             
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
                        OBSERVACAO = row["OBSERVACAO"] is DBNull ? "Trabalho de corno do caralho" : row["OBSERVACAO"] as string ?? string.Empty,
                        COR = row["COR"] is DBNull ? string.Empty : " - " + row["COR"] ?? string.Empty,
                        TAMANHO = row["TAMANHO"] is DBNull ? string.Empty : " / " + row["TAMANHO"] ?? string.Empty,
                        ID_SCANNTECH = idScannTech
                    };
                }
            }
        }

        public void EnsureTBOSTriggersCreated(FbConnection connection)
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
            FbTransaction transaction = connection.BeginTransaction();
            FbCommand tbOsBITrigger = new();
            FbCommand tbOsItemBITrigger = new();
            FbCommand tbOsBUTrigger = new();
            FbCommand tbOsItemBUTrigger = new();
            tbOsBITrigger.Connection = tbOsItemBITrigger.Connection = tbOsBUTrigger.Connection = tbOsItemBUTrigger.Connection = connection;
            tbOsBITrigger.CommandType = tbOsItemBITrigger.CommandType = tbOsBUTrigger.CommandType = tbOsItemBUTrigger.CommandType = CommandType.Text;
            tbOsBITrigger.Transaction = tbOsItemBITrigger.Transaction = tbOsBUTrigger.Transaction = tbOsItemBUTrigger.Transaction = transaction;
            tbOsBITrigger.CommandText = "CREATE OR ALTER TRIGGER TB_OS_AUX_SYNC_INS FOR TB_OS ACTIVE BEFORE INSERT AS DECLARE vNUMCAIXA TYPE OF COLUMN TRI_PDV_CONFIG.NO_CAIXA; BEGIN FOR SELECT NO_CAIXA FROM TRI_PDV_CONFIG ORDER BY NO_CAIXA INTO :VNUMCAIXA DO BEGIN INSERT INTO TRI_PDV_AUX_SYNC(SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER) VALUES(GEN_ID(GEN_PDV_AUX_SYNC_SEQ, 1), new.ID_OS, 'TB_OS', 'I', :VNUMCAIXA, CURRENT_TIMESTAMP); END END;";
            tbOsItemBITrigger.CommandText = " CREATE OR ALTER TRIGGER TB_OS_ITEM_AUX_SYNC_INS FOR TB_OS_ITEM ACTIVE BEFORE INSERT AS DECLARE vNUMCAIXA TYPE OF COLUMN TRI_PDV_CONFIG.NO_CAIXA; BEGIN FOR SELECT NO_CAIXA FROM TRI_PDV_CONFIG ORDER BY NO_CAIXA INTO :VNUMCAIXA DO BEGIN INSERT INTO TRI_PDV_AUX_SYNC (SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER) VALUES(GEN_ID(GEN_PDV_AUX_SYNC_SEQ, 1), new.ID_ITEMOS, 'TB_OS_ITEM', 'I', :VNUMCAIXA, CURRENT_TIMESTAMP); END END; ";
            tbOsBUTrigger.CommandText = " CREATE OR ALTER TRIGGER TB_OS_AUX_SYNC_UPD FOR TB_OS ACTIVE BEFORE UPDATE AS DECLARE vNUMCAIXA TYPE OF COLUMN TRI_PDV_CONFIG.NO_CAIXA; BEGIN IF ( OLD.ID_CLIENTE IS DISTINCT FROM NEW.ID_CLIENTE OR OLD.ID_STATUS IS DISTINCT FROM NEW.ID_STATUS) THEN BEGIN FOR SELECT NO_CAIXA FROM TRI_PDV_CONFIG ORDER BY NO_CAIXA INTO :VNUMCAIXA DO BEGIN IF (( SELECT COUNT (1) FROM TRI_PDV_AUX_SYNC WHERE ID_REG = OLD.ID_OS AND TABELA = 'TB_OS' AND (OPERACAO = 'I' OR OPERACAO = 'U') AND NO_CAIXA = :VNUMCAIXA)= 0) THEN BEGIN INSERT INTO TRI_PDV_AUX_SYNC (SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER) VALUES(GEN_ID(GEN_PDV_AUX_SYNC_SEQ, 1), old.ID_OS, 'TB_OS', 'U', :VNUMCAIXA, CURRENT_TIMESTAMP); END END END END;";
            tbOsItemBUTrigger.CommandText = "CREATE OR ALTER TRIGGER TB_OS_ITEM_AUX_SYNC_UPD FOR TB_OS_ITEM ACTIVE BEFORE UPDATE AS DECLARE vNUMCAIXA TYPE OF COLUMN TRI_PDV_CONFIG.NO_CAIXA; BEGIN IF ( OLD.QTD_ITEM IS DISTINCT FROM NEW.QTD_ITEM OR OLD.VLR_TOTAL IS DISTINCT FROM NEW.VLR_TOTAL OR OLD.ID_IDENTIFICADOR IS DISTINCT FROM NEW.ID_IDENTIFICADOR OR OLD.ID_OS IS DISTINCT FROM NEW.ID_OS OR OLD.COD_BARRA IS DISTINCT FROM NEW.COD_BARRA OR OLD.VLR_UNIT IS DISTINCT FROM NEW.VLR_UNIT) THEN BEGIN FOR SELECT NO_CAIXA FROM TRI_PDV_CONFIG ORDER BY NO_CAIXA INTO :VNUMCAIXA DO BEGIN IF (( SELECT COUNT (1) FROM TRI_PDV_AUX_SYNC WHERE ID_REG = OLD.ID_OS AND TABELA = 'TB_OS' AND (OPERACAO = 'I' OR OPERACAO = 'U') AND NO_CAIXA = :VNUMCAIXA)= 0) THEN BEGIN INSERT INTO TRI_PDV_AUX_SYNC (SEQ, ID_REG, TABELA, OPERACAO, NO_CAIXA, TS_OPER) VALUES(GEN_ID(GEN_PDV_AUX_SYNC_SEQ, 1), old.ID_OS, 'TB_OS', 'U', :VNUMCAIXA, CURRENT_TIMESTAMP); END END END END;";
            try
            {
                tbOsBITrigger.ExecuteNonQuery();
                tbOsItemBITrigger.ExecuteNonQuery();
                tbOsBUTrigger.ExecuteNonQuery();
                tbOsItemBUTrigger.ExecuteNonQuery();
                transaction.Commit();
            }
            catch(Exception ex)
            {
                
            }
            finally
            {
                connection.Close();
            }
        }

        //public List<CLIPP_OS> GetClippOsesNotSynced(FbConnection connection)
        //{
        //    List<CLIPP_OS> retorno = new();
        //    try
        //    {
        //        connection.Open();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }

        //    FbCommand command = new FbCommand();
        //    DataTable oses = new();
        //    DataTable osItems = new();
        //    command.Connection = connection;
        //    command.CommandType = CommandType.Text;

        //    command.CommandText = "SELECT * FROM TB_OS WHERE SYNCED = 0";

        //    try
        //    {
        //        oses.Load(command.ExecuteReader());
        //        command.CommandText = "SELECT toi.* FROM TB_OS_ITEM toi JOIN TB_OS tos ON toi.ID_OS = tos.ID_OS WHERE tos.SYNCED = 0";
        //        osItems.Load(command.ExecuteReader());
        //    }
        //    finally
        //    {
        //        connection.Close();
        //    }

        //    if (oses.Rows.Count != 0)
        //    {
        //        foreach (DataRow infoTableRow in oses.Rows)
        //        {
        //            retorno.Add(new CLIPP_OS
        //            {
        //                ID_OS = infoTableRow["ID_OS"] is DBNull ? 0 : infoTableRow["ID_OS"] as int? ?? 0,
        //                ID_CLIENTE = infoTableRow["ID_CLIENTE"] is DBNull ? 0 : infoTableRow["ID_CLIENTE"] as int? ?? 0,
        //                ID_VENDEDOR = infoTableRow["ID_VENDEDOR"] is DBNull ? 0 : infoTableRow["ID_VENDEDOR"] as int? ?? 0,
        //                DT_OS = infoTableRow["DT_OS"] is DBNull ? DateTime.Now : infoTableRow["DT_OS"] as DateTime? ?? DateTime.Now,
        //                HR_OS = infoTableRow["HR_OS"] is DBNull ? DateTime.Now : infoTableRow["HR_OS"] as DateTime? ?? DateTime.Now,
        //                ID_STATUS = infoTableRow["ID_STATUS"] is DBNull ? 0 : infoTableRow["ID_STATUS"] as int? ?? 0,
        //                ID_PARCELA = infoTableRow["ID_PARCELA"] is DBNull ? 0 : infoTableRow["ID_PARCELA"] as int? ?? 0,
        //                ClippOsItems = osItems.AsEnumerable().Where(x => x["ID_OS"] == infoTableRow["ID_OS"]).Select(x => new CLIPP_OS_ITEM
        //                {
        //                    ID_ITEMOS = x["ID_ITEMOS"] is DBNull ? 0 : x["ID_ITEMOS"] as int? ?? 0,
        //                    ID_IDENTIFICADOR = x["ID_IDENTIFICADOR"] is DBNull ? 0 : x["ID_IDENTIFICADOR"] as int? ?? 0,
        //                    ID_OS = x["ID_OS"] is DBNull ? 0 : x["ID_OS"] as int? ?? 0,
        //                    ITEM_CANCEL = x["ITEM_CANCEL"] is DBNull ? 'N' : x["ITEM_CANCEL"] as char? ?? 'N',
        //                    VLR_UNIT = x["VLR_UNIT"] is DBNull ? 0 : x["VLR_UNIT"] as decimal? ?? 0,
        //                }).ToList(),
        //            });
        //        }
        //    }

        //    return retorno;
        //}

        public CLIPP_OS? GetClippOsByID(FbConnection connection, int id)
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
            DataTable oses = new();
            DataTable osItems = new();
            command.Connection = connection;
            command.CommandType = CommandType.Text;


            try
            {

                command.CommandText = $"SELECT * FROM TB_OS WHERE ID_OS = {id} AND ID_STATUS <> 9";
                oses.Load(command.ExecuteReader());
                command.CommandText = $"SELECT * FROM TB_OS_ITEM WHERE ID_OS = {id}";
                osItems.Load(command.ExecuteReader());
            }
            finally
            {
                connection.Close();
            }

            if (oses.Rows.Count == 1)
            {
                var OsItems = osItems.AsEnumerable().Where(x => (int)x["ID_OS"] == (int)oses.Rows[0]["ID_OS"]).Select(x =>
                    new CLIPP_OS_ITEM
                    {
                        ID_ITEMOS = x["ID_ITEMOS"] is DBNull ? 0 : x["ID_ITEMOS"] as int? ?? 0,
                        ID_IDENTIFICADOR = x["ID_IDENTIFICADOR"] is DBNull ? 0 : x["ID_IDENTIFICADOR"] as int? ?? 0,
                        ID_OS = x["ID_OS"] is DBNull ? 0 : x["ID_OS"] as int? ?? 0,
                        ITEM_CANCEL = x["ITEM_CANCEL"] is DBNull ? 'N' : x["ITEM_CANCEL"] as char? ?? 'N',
                        VLR_UNIT = x["VLR_UNIT"] is DBNull ? 0 : x["VLR_UNIT"] as decimal? ?? 0,
                        QTD_ITEM = x["QTD_ITEM"] is DBNull ? 1 : x["QTD_ITEM"] as decimal? ?? 1
                    }).ToList();

                return new CLIPP_OS
                {
                    ID_OS = oses.Rows[0]["ID_OS"] is DBNull ? 0 : oses.Rows[0]["ID_OS"] as int? ?? 0,
                    ID_CLIENTE = oses.Rows[0]["ID_CLIENTE"] is DBNull ? 0 : oses.Rows[0]["ID_CLIENTE"] as int? ?? 0,
                    ID_VENDEDOR = oses.Rows[0]["ID_VENDEDOR"] is DBNull ? 0 : oses.Rows[0]["ID_VENDEDOR"] as int? ?? 0,
                    DT_OS = oses.Rows[0]["DT_OS"] is DBNull ? DateTime.Now : oses.Rows[0]["DT_OS"] as DateTime? ?? DateTime.Now,
                    HR_OS = oses.Rows[0]["HR_OS"] is DBNull ? DateTime.Now : oses.Rows[0]["HR_OS"] as DateTime? ?? DateTime.Now,
                    ID_STATUS = oses.Rows[0]["ID_STATUS"] is DBNull ? 0 : oses.Rows[0]["ID_STATUS"] as int? ?? 0,
                    ID_PARCELA = oses.Rows[0]["ID_PARCELA"] is DBNull ? 0 : oses.Rows[0]["ID_PARCELA"] as int? ?? 0,
                    ClippOsItems = OsItems,
                };
            }

            else
            {
                return null;
            }
        }

        public void FechaOrdemDeServico(FbConnection connection, int os)
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
            command.CommandText = $"UPDATE TB_OS SET ID_STATUS=9, DT_FECHADO='{DateTime.Now:yyyy-MM-dd}' WHERE ID_OS={os}; ";
            try
            {
                command.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
            }
        }

        public void ClearAuxSyncTable(FbConnection connection)
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

            command.CommandText = "DELETE FROM TRI_PDV_AUX_SYNC";

            try
            {
                command.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
            }
        }

    }

    public class CLIPP_OS
    {
        public int ID_OS { get; set; }
        public int ID_CLIENTE { get; set; }
        public int ID_VENDEDOR { get; set; }
        public DateTime? DT_OS { get; set; }
        public DateTime? HR_OS { get; set; }
        public DateTime? DT_ENTREGA { get; set; }
        public string? COMPRADOR { get; set; }
        public int ID_STATUS { get; set; }
        public string? OBSERVACAO { get; set; }
        public int? ID_MODULO { get; set; }
        public char? ENTREGA { get; set; }
        public string? CHAVE { get; set; }
        public int? ID_OSATEND { get; set; }
        public DateTime? DT_GARANTIA { get; set; }
        public int? ID_OBJETO_CONTRATO { get; set; }
        public DateTime? DT_RETIRADA { get; set; }
        public string? OBS_INTERNA { get; set; }
        public int? ID_TECNICO_RESP { get; set; }
        public DateTime? DT_FECHADO { get; set; }
        public char? IMPORTADO { get; set; }
        public int ID_PARCELA { get; set; }
        public List<CLIPP_OS_ITEM> ClippOsItems { get; set; }
    }

    public class CLIPP_OS_ITEM
    {
        public int ID_ITEMOS { get; set; }
        public decimal? QTD_ITEM { get; set; }
        public decimal? QTD_IMPORT { get; set; }
        public decimal? VLR_TOTAL { get; set; }
        public decimal? PRC_CUSTO { get; set; }
        public decimal? PRC_LISTA { get; set; }
        public decimal? VLR_DESC { get; set; }
        public int ID_IDENTIFICADOR { get; set; }
        public int ID_OS { get; set; }
        public char ITEM_CANCEL { get; set; }
        public DateTime? DT_LACTO { get; set; }
        public char? CASAS_QTD { get; set; }
        public char? CASAS_VLR { get; set; }
        public char? ST { get; set; }
        public decimal? ALIQUOTA { get; set; }
        public string? CHAVE { get; set; }
        public string? COD_BARRA { get; set; }
        public int? ID_FUNCIONARIO { get; set; }
        public DateTime? DT_ITEM { get; set; }
        public DateTime? HR_ITEM { get; set; }
        public decimal VLR_UNIT { get; set; }
        public decimal? POR_COMISSAO { get; set; }
    }

    public class KIT_PROMOCIONAL
    {       
        public List<KIT_PROMOCIONAL_ITEM> produtos { get; set; }
        public int ID_KIT { get; set; }
        public string DESCRICAO { get; set; }
        public char STATUS { get; set; }
        public DateTime DATA { get; set; }

        public KIT_PROMOCIONAL()
        {
            //ESTE ABSURDO DEVE-SE.... (FODA NÉ MAS FAZER OQ BY.. LUCAS G.)   // NORRRRRRRRMAAAAAL
            produtos = new List<KIT_PROMOCIONAL_ITEM>();// Instância uma nova lista da classe KIT_PROMOCIONAL_ITEM (SÓ MANTENDO NO PADRÃO QUE JÁ ACONTECIA NO ORÇAMENTO BY. LUCAS G.)
        }
        public void Clear()
        {
            try
            {
                produtos.Clear();
            }
            catch
            {
                Console.WriteLine("Erro ao limpar lista de produtos.");
            }
        }
        public bool LeKitPromocional(FbConnection connection, int idkit, out string nomeKit)
        {
            try
            {
                using (var KIT_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EST_KITTableAdapter())
                using (var KIT_TB = new FDBDataSetOperSeed.TB_EST_KITDataTable())
                {                    
                    KIT_TA.Connection = connection;
                    KIT_TA.FillByIdStatus(KIT_TB, idkit);
                    if (KIT_TB.Rows.Count <= 0) { nomeKit = null; return false; }
                    nomeKit = (string)KIT_TB.Rows[0][1];
                    return true;
                }
            }
            catch(Exception ex)
            {                
               Console.WriteLine("Erro ao tentar verificar numero do kit, segue erro: " + ex);
               nomeKit = null;
               return false;                              
            }
        }
        public bool LeKitItens(FbConnection connection, int kit)
        {
            try
            {
                using(var KITITEM_TA = new DataSets.FDBDataSetOperSeedTableAdapters.TB_EST_KIT_ITEMTableAdapter())
                using(var KITITEM_TB = new FDBDataSetOperSeed.TB_EST_KIT_ITEMDataTable())
                {
                    KITITEM_TA.Connection = connection;
                    KITITEM_TA.FillByEstKitStatus(KITITEM_TB, kit);
                    if (KITITEM_TB.Rows.Count > 0)
                    {
                        foreach(var rows in KITITEM_TB)
                        {
                            var Produtos = new KIT_PROMOCIONAL_ITEM
                            {
                                ID_IDENTIFICADOR = rows.ID_IDENTIFICADOR,
                                ID_KIT = rows.ID_KIT,
                                QTD_ITEM = rows.QTD_ITEM,
                                STATUS = rows.STATUS,
                                VLR_ITEM = rows.VLR_ITEM,
                                ID_ESTKIT = rows.ID_ESTKIT
                            };
                            produtos.Add(Produtos);
                        }                       
                    }
                    else { return false; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao tentar verificar itens do kit, segue erro:" + ex);
                return false;
            }
            return true;
        }
    }
    public class KIT_PROMOCIONAL_ITEM
    {
        public int ID_IDENTIFICADOR { get; set; }
        public int ID_KIT { get; set; }
        public decimal QTD_ITEM { get; set; }
        public string STATUS { get; set; }
        public decimal VLR_ITEM { get; set; }
        public int ID_ESTKIT { get; set; }
    }
    //public bool CheckIfDeletionExists(FbConnection connection)
    //{
    //    try
    //    {
    //        connection.Open();
    //    }
    //    catch (Exception e)
    //    {
    //        Console.WriteLine(e);
    //        throw;
    //    }

    //    FbCommand command = new FbCommand();
    //    command.Connection = connection;
    //    command.CommandType = CommandType.Text;

    //    command.CommandText = "SELECT COUNT (1) FROM TRI_PDV_AUX_SYNC"
    //}

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
    public string COR { get; set; }
    public string TAMANHO { get; set; }
    public int?  ID_SCANNTECH { get; set; }
}

