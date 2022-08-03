using System;

namespace PDV_ORCAMENTO.Reports
{
    public class DTO_Report_Emitente
    {
        public string LOGO { get; set; }
        public string NOME { get; set; }
        public string NOME_FANTA { get; set; }
        public string CNPJ { get; set; }
        public string INSC_ESTAD { get; set; }
        public string END_LOGRAD { get; set; }
        public string END_COMPLE { get; set; }
        public string END_NUMERO { get; set; }
        public string END_BAIRRO { get; set; }
        public string DDD_COMER { get; set; }
        public string FONE_COMER { get; set; }
        public string END_CEP { get; set; }
        public string CIDADE_NOME { get; set; }
        public string SIGLA_UF { get; set; }
        public string SITE { get; set; }
    }

    public class DTO_Report_Orcamento
    {
        public int ID_ORCAMENTO { get; set; }
        public string USERNAME { get; set; }
        public DateTime DT_VALIDADE { get; set; }
        public DateTime TODAY { get; set; }
        public DateTime NOW { get; set; }
    }

    public class DTO_Report_Solicitante
    {
        public string NOME { get; set; }
        public string CPF_CNPJ { get; set; }
        public string RG_IE { get; set; }
        public string INSC_MUNIC { get; set; }
        public string END_TIPO { get; set; }
        public string END_LOGRAD { get; set; }
        public string END_NUMERO { get; set; }
        public string END_COMPLE { get; set; }
        public string END_BAIRRO { get; set; }
        public string END_CEP { get; set; }
        public string CIDADE_NOME { get; set; }
        public string SIGLA_UF { get; set; }
        public string DDD_COMER { get; set; }
        public string FONE_COMER { get; set; }
        public string DDD_FAX { get; set; }
        public string FONE_FAX { get; set; }
        public string DDD_CELUL { get; set; }
        public string FONE_CELUL { get; set; }
        public string DDD_RESID { get; set; }
        public string FONE_RESID { get; set; }
        public string EMAIL_CONT { get; set; }
    }

    public class DTO_Report_Orcamento_Item
    {
        public int ID_PRODUTO { get; set; }
        public int ID_IDENTIFICADOR { get; set; }
        public string COD_BARRA { get; set; }
        public string DESCRICAO { get; set; }
        public decimal QUANT { get; set; }
        public string UNI_MEDIDA { get; set; }
        public decimal VALOR { get; set; }
        public decimal DESCONTO { get; set; }
        public decimal VALOR_TOT { get; set; }
    }
}
