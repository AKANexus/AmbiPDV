using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using PDV_WPF;
using System.Data;
using PublicFunc;

namespace ImprimirCupom
{
#pragma warning disable CS0649
    public class MetodoPagamento
    {
        public string NomeMetodo;
        public double ValorDoPgto;
    }
    public class Produto
    {

        public int numero;
        public string codigo;
        public string descricao;
        public string tipounid;
        public double qtde;
        public double valorunit;
        public double desconto;
        public double trib_est;
        public double trib_fed;
        public double valortotal;

    }
    class PrintCUPOM
    {

        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static string nomedaempresa;
        public static string cnpjempresa;
        public static string nomefantasia;
        public static string enderecodaempresa;
        public static string ieempresa;
        public static string imempresa;
        public static int numerodoextrato;
        public static string cpfcnpjconsumidor = "";
        public static string operador;
        public static int numerosat;
        public static string chavenfe;
        public static string assinaturaQRCODE;
        public static string troco;
        public static decimal desconto;
        public static string cliente;
        public static DateTime vencimento;
        public static bool prazo;
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();

        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, double Xqtde, double Xvalorunit, double Xdesconto, double Xtribest, double Xtribfed)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Extensoes.TruncateLongString(Xdescricao), tipounid = Xtipounid, qtde = Xqtde, valorunit = Xvalorunit, valortotal = Xqtde * (Xvalorunit - Xdesconto), desconto = Xdesconto, trib_est = Xtribest, trib_fed = Xtribfed };
            produtos.Add(prod);
        }

        public static void RecebePagamento(string Xmetodo, double Xvalor)
        {
            MetodoPagamento method = new MetodoPagamento { NomeMetodo = Xmetodo, ValorDoPgto = Xvalor };
            pagamentos.Add(method);
        }

        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            double subtotal = 0f;
            double tributostot = 0f;



            #region Region1
            RecebePrint(nomefantasia, negrito, centro.align, 1);
            RecebePrint(nomedaempresa, corpo, centro.align, 1);
            RecebePrint(enderecodaempresa, corpo, centro.align, 1);
            RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2) + "  IE: " + ieempresa + "  IM: " + imempresa, corpo, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("Extrato No. " + numerodoextrato, Titulo, centro.align, 1);
            RecebePrint("CUPOM FISCAL ELETRÔNICO - SAT", Titulo, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            if (!string.IsNullOrEmpty(cpfcnpjconsumidor))
            {
                if (cpfcnpjconsumidor == "")
                {
                    RecebePrint("CPF/CNPJ do Consumidor: Consumidor não Informado", corpo, esquerda.align, 1);
                }
                else if (cpfcnpjconsumidor.Length == 11)
                {
                    string cpfcons = cpfcnpjconsumidor.ToString().Substring(0, 3) + "." + cpfcnpjconsumidor.ToString().Substring(3, 3) + "." + cpfcnpjconsumidor.ToString().Substring(6, 3) + "-" + cpfcnpjconsumidor.ToString().Substring(9, 2);
                    RecebePrint("CPF do Consumidor: " + cpfcons, corpo, esquerda.align, 1);
                }
                else if (cpfcnpjconsumidor.Length == 14)
                {
                    string cnpjcons = cpfcnpjconsumidor.ToString().Substring(0, 2) + "." + cpfcnpjconsumidor.ToString().Substring(2, 3) + "." + cpfcnpjconsumidor.ToString().Substring(5, 3) + "/" + cpfcnpjconsumidor.ToString().Substring(8, 4) + "-" + cpfcnpjconsumidor.ToString().Substring(12, 2);
                    RecebePrint("CNPJ do Consumidor: " + cnpjcons, corpo, esquerda.align, 1);
                }

            }
            else
            {
                RecebePrint("CPF/CNPJ do Consumidor: Consumidor não Informado", corpo, esquerda.align, 1);
            }

            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  (VLTR R$)*  VL ITEM R$", corpo, centro.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);

            //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
            int linha = 1;
            foreach (Produto prod in produtos)
            {
                RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda.align, 1);
                RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + (prod.valorunit).ToString("n2"), corpo, esquerda.align, 0);
                RecebePrint("\t\t\t\t\t\t\t\t\t\t" + ((prod.valorunit) * (prod.trib_est) / 100).ToString("n2"), corpo, esquerda.align, 0);
                RecebePrint((prod.valortotal.ToString("n2")), corpo, direita.align, 1);
                subtotal += prod.valortotal;
                linha += 1;
            }
            //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv

            RecebePrint("VALOR TOTAL R$", Titulo, esquerda.align, 0);
            if (desconto != 0)
            {
                RecebePrint((subtotal).ToString("n2"), Titulo, direita.align, 1);
                RecebePrint("Desconto R$", corpo, esquerda.align, 0);
                RecebePrint(desconto.ToString("n2"), corpo, direita.align, 1);
            }
            else
            {
                RecebePrint((subtotal).ToString("n2"), Titulo, direita.align, 1);
            }
            foreach (MetodoPagamento met in pagamentos)
            {
                RecebePrint(met.NomeMetodo, corpo, esquerda.align, 0);
                RecebePrint(met.ValorDoPgto.ToString("n2"), corpo, direita.align, 1);
            }

            if (troco != "0,00")
            {
                RecebePrint("Troco R$", corpo, esquerda.align, 0);
                RecebePrint(troco, corpo, direita.align, 1);
            }
            else
            {
                RecebePrint("", corpo, esquerda.align, 0);
            }
            RecebePrint(" ", corpo, esquerda.align, 1);
            //RecebePrint("LINHA EXTRA DE INFORMAÇÃO", corpo, esquerda.align, true);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("OBSERVAÇÕES DO CONTRIBUINTE", Titulo, esquerda.align, 1);
            RecebePrint(System.DateTime.Now.ToString(), corpo, esquerda.align, 1);
            RecebePrint("OBRIGADO VOLTE SEMPRE!!", corpo, esquerda.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("* - Valor aproximado dos tributos do item", corpo, esquerda.align, 1);
            RecebePrint("Valor aproximado dos tributos deste cupom R$", corpo, esquerda.align, 0);
            RecebePrint(tributostot.ToString("n2"), negrito, direita.align, 1);
            RecebePrint("Tributos Federais R$", corpo, esquerda.align, 0);
            RecebePrint("xxxx", negrito, direita.align, 1);
            RecebePrint("Tributos Estaduais R$", corpo, esquerda.align, 0);
            RecebePrint("xxxx", negrito, direita.align, 1);
            RecebePrint("Tributos Municipais R$", corpo, esquerda.align, 0);
            RecebePrint("xxxx", negrito, direita.align, 1);
            RecebePrint("(conforme Lei Fed. 12.741/2012)", corpo, esquerda.align, 1);
            RecebePrint("Operador: " + operador, corpo, esquerda.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("SAT No. " + numerosat.ToString(), Titulo, centro.align, 1);
            RecebePrint(System.DateTime.Now.ToString(), Titulo, centro.align, 1);
            RecebePrint(Regex.Replace(chavenfe, " {4}", "$0,"), corpo, centro.align, 1);
            //----------------------------^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            RecebePrint("COD_BARRAS>>" + chavenfe, corpo, centro.align, 1);
            RecebePrint("QR_CODE>>" + assinaturaQRCODE, corpo, centro.align, 1);
            RecebePrint("Consulte o QR Code pelo aplicativo \"De olho na nota\",", corpo, centro.align, 1);
            RecebePrint("disponível na Play Store e na AppStore", corpo, centro.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            if (prazo)
            {
                RecebePrint("Cliente: " + cliente, negrito, esquerda.align, 1);
                RecebePrint("Vencimento: " + vencimento.ToShortDateString(), negrito, esquerda.align, 1);
                RecebePrint("Terminal: " + PDV_WPF.Properties.Settings.Default.no_caixa.ToString("D3") + "\t\tOperador: " + operador, corpo, esquerda.align, 1);
            }
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro.align, 1);
            RecebePrint("(11) 4304-7778", corpo, centro.align, 1);
            #endregion
            try
            {
                Printa(inf);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                pagamentos.Clear();
                produtos.Clear();
                linha = 1;
            }
            return true;
        }
    }
    class PrintCANCL
    {
        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static string nomedaempresa;
        public static string cnpjempresa;
        public static string nomefantasia;
        public static string enderecodaempresa;
        public static string ieempresa;
        public static string imempresa;
        public static int numerodoextrato;
        public static string cpfcnpjconsumidor = "";
        public static string operador;
        public static int numerosat;
        public static string chavenfe;
        public static string assinaturaQRCODE;
        public static double total;

        public static void IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);

            RecebePrint(nomefantasia, negrito, centro.align, 1);
            RecebePrint(nomedaempresa, corpo, centro.align, 1);
            RecebePrint(enderecodaempresa, corpo, centro.align, 1);
            RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2) + "  IE: " + ieempresa + "  IM: " + imempresa, corpo, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("Extrato No. " + numerodoextrato, Titulo, centro.align, 1);
            RecebePrint("CUPOM FISCAL ELETRÔNICO - SAT", Titulo, centro.align, 1);
            RecebePrint("CANCELAMENTO", Titulo, centro.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("DADOS DO CUPOM FISCAL ELETRÔNICO CANCELADO", negrito, esquerda.align, 1);
            RecebePrint("", negrito, esquerda.align, 1);
            if (cpfcnpjconsumidor == "" || cpfcnpjconsumidor == null)
            {
                RecebePrint("CNPJ do Consumidor: Não informado.", corpo, esquerda.align, 1);
            }
            else if (cpfcnpjconsumidor.Length == 11)
            {
                string cpfcons = cpfcnpjconsumidor.ToString().Substring(0, 3) + "." + cpfcnpjconsumidor.ToString().Substring(3, 3) + "." + cpfcnpjconsumidor.ToString().Substring(6, 3) + "-" + cpfcnpjconsumidor.ToString().Substring(9, 2);
                RecebePrint("CPF do Consumidor: " + cpfcons, corpo, esquerda.align, 1);
            }
            else if (cpfcnpjconsumidor.Length == 14)
            {
                string cnpjcons = cpfcnpjconsumidor.ToString().Substring(0, 2) + "." + cpfcnpjconsumidor.ToString().Substring(2, 3) + "." + cpfcnpjconsumidor.ToString().Substring(5, 3) + "/" + cpfcnpjconsumidor.ToString().Substring(8, 4) + "-" + cpfcnpjconsumidor.ToString().Substring(12, 2);
                RecebePrint("CNPJ do Consumidor: " + cnpjcons, corpo, esquerda.align, 1);
            }


            RecebePrint("TOTAL: R$ " + total.ToString("n2"), Titulo, esquerda.align, 1);
            RecebePrint("", negrito, esquerda.align, 1);
            RecebePrint("SAT No. " + numerosat.ToString(), negrito, centro.align, 1);
            RecebePrint(System.DateTime.Now.ToString(), negrito, centro.align, 1);
            RecebePrint("", negrito, esquerda.align, 1);
            RecebePrint(Regex.Replace(chavenfe, " {4}", "$0,"), corpo, centro.align, 1);
            RecebePrint("COD_BARRAS>>" + chavenfe, corpo, centro.align, 1);
            RecebePrint("QR_CODE>>" + assinaturaQRCODE, corpo, centro.align, 1);
            RecebePrint("Consulte o QR Code pelo aplicativo \"De olho na nota\",", corpo, centro.align, 1);
            RecebePrint("disponível na Play Store e na AppStore", corpo, centro.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro.align, 1);
            RecebePrint("(11) 4304-7778", corpo, centro.align, 1);
            Printa(inf);
        }
    }
    class PrintSANSUP
    {

        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 11f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static string operador;
        public static string operacao;
        public static decimal valor;
        public static string numcaixa;


        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            RecebePrint(("Comprovante de ").ToUpper() + operacao, Titulo, centro.align, 1);
            RecebePrint("Caixa Nº  " + numcaixa, Titulo, centro.align, 1);
            RecebePrint(new string('-', 81), negrito, centro.align, 1);
            RecebePrint(DateTime.Now.ToShortDateString() + ", " + DateTime.Now.ToLongTimeString(), negrito, centro.align, 1);
            //PrintFunc.RecebePrint(" ", Titulo, centro.align, true);
            RecebePrint("Valor: " + valor.ToString("c2"), Titulo, esquerda.align, 1);
            //PrintFunc.RecebePrint("Operação: " + operacao, negrito, esquerda.align, true);
            RecebePrint("Operador: " + operador, negrito, esquerda.align, 1);
            RecebePrint("Recebido por: ________________________", Titulo, esquerda.align, 1);
            RecebePrint(new string('-', 81), corpo, esquerda.align, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro.align, 1);
            RecebePrint("(11) 4304-7778", corpo, centro.align, 1);
            #endregion
            Printa(inf);
            return true;
        }
    }
    class PrintFECHA
    {

        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte mini = new Fonte { tipo = new Font("Arial Narrow", 4f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static Alinhamento rtl = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near, FormatFlags = StringFormatFlags.DirectionRightToLeft } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static string num_caixa;
        public static string cnpjempresa;
        public static string nomefantasia;
        public static string enderecodaempresa;
        public static string operador;

        public static FDBDataSet.TRI_PDV_OPERDataTable fecha_infor_dt = new FDBDataSet.TRI_PDV_OPERDataTable();
        public static FDBDataSet.TRI_PDV_OPERDataTable fecha_oper_dt = new FDBDataSet.TRI_PDV_OPERDataTable();
        public List<double> val_pagos { get; set; }



        public static decimal qte_cancelado;
        public static decimal val_cancelado;
        public static decimal cups_cancelados;
        public static decimal qte_estornado;
        public static decimal val_estornado;
        public static decimal tot_vendas;
        public static decimal tot_informado;
        public static decimal med_vendas;
        public static decimal tot_itens;
        //public static bool reimpressao; //TODO: Permitir a reimpressão do fechamento.
        public static DateTime fechamento = DateTime.Now;

        static PDV_WPF.DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter TB_EMITENTETableAdapter = new PDV_WPF.DataSets.FDBDataSetOperSeedTableAdapters.TB_EMITENTETableAdapter();
        public static bool IMPRIME()
        {
            float[] tabstops = { 33f, 33f, 33f, 33f, 33f };
            esquerda.align.SetTabStops(33f, tabstops);
            direita.align.SetTabStops(33f, tabstops);
            rtl.align.SetTabStops(33f, tabstops);
            Dictionary<int, string> metodos = new Dictionary<int, string>();

            PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter metodosTA = new PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter();
            FDBDataSet.TRI_PDV_METODOSDataTable dt = new FDBDataSet.TRI_PDV_METODOSDataTable();
            metodosTA.FillByAtivos(dt);
            foreach (DataRow row in dt)
            {
                if ((int)row["DIAS"] >= 0)
                {
                    metodos.Add(((int)row["ID_PAGAMENTO"]), (((string)row["DESCRICAO"])));
                }
            }
            RecebePrint(nomefantasia, negrito, centro.align, 1);
            RecebePrint(enderecodaempresa, corpo, centro.align, 1);
            RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2), corpo, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("FECHAMENTO DO CAIXA Nº " + PDV_WPF.Properties.Settings.Default.no_caixa.ToString("000"), Titulo, centro.align, 1);
            RecebePrint(new string('>', 15), negrito, esquerda.align, 0);
            RecebePrint(new string('<', 15), negrito, direita.align, 0);
            RecebePrint("TOTAL GAVETA", negrito, centro.align, 1);
            double totaisgaveta = 0;
            foreach (KeyValuePair<int, string> metodo in metodos)
            {
                RecebePrint(metodo.Value, corpo, esquerda.align, 0);
                RecebePrint("\t\t:\tR$", corpo, esquerda.align, 0);
                RecebePrint(String.Format("\t\t{0}", ((double)fecha_infor_dt[0][metodo.Key]).ToString("0.00")), corpo, rtl.align, 1);
                tot_vendas += Convert.ToDecimal((double)fecha_infor_dt[0][metodo.Key]);
                if ((int)metodosTA.ChecaPrazo(metodo.Key) == 0)
                {
                    totaisgaveta += (double)fecha_infor_dt[0][metodo.Key];
                }
            }
            RecebePrint("TOTAL\t\t:\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + totaisgaveta.ToString("0.00"), corpo, rtl.align, 1);
            RecebePrint(" ", mini, esquerda.align, 1);
            RecebePrint("TROCAS\t\t:   " + "\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + ((double)fecha_infor_dt[0]["TROCAS"]).ToString("0.00"), corpo, rtl.align, 1);
            RecebePrint("SUPRIMENTOS\t:   " + "\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + ((double)fecha_infor_dt[0]["SUPRIMENTOS"]).ToString("0.00"), corpo, rtl.align, 1);
            RecebePrint("SANGRIA\t\t:   " + "\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + ((double)fecha_infor_dt[0]["SANGRIAS"]).ToString("0.00"), corpo, rtl.align, 1);


            RecebePrint(new string('>', 15), negrito, esquerda.align, 0);
            RecebePrint(new string('<', 15), negrito, direita.align, 0);
            RecebePrint("TOTAL SISTEMA", negrito, centro.align, 1);
            double totaissistema = 0;
            PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter Oper = new PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter();
            Oper.FillByCaixaAberto(fecha_oper_dt, PDV_WPF.Properties.Settings.Default.no_caixa);
            foreach (KeyValuePair<int, string> metodo in metodos)
            {
                RecebePrint(metodo.Value, corpo, esquerda.align, 0);
                RecebePrint("\t\t:\tR$", corpo, esquerda.align, 0);
                if (metodo.Key == 1)
                {
                    RecebePrint(String.Format("\t\t{0}", ((double)fecha_oper_dt[0][metodo.Key] - (double)fecha_oper_dt[0]["SANGRIAS"] + (double)fecha_oper_dt[0]["SUPRIMENTOS"]).ToString("0.00")), corpo, rtl.align, 1);
                }
                else
                {
                    RecebePrint(String.Format("\t\t{0}", ((double)fecha_oper_dt[0][metodo.Key]).ToString("0.00")), corpo, rtl.align, 1);
                }
                tot_informado += Convert.ToDecimal((double)fecha_oper_dt[0][metodo.Key]);
                if ((int)metodosTA.ChecaPrazo(metodo.Key) == 0)
                {
                    totaissistema += (double)fecha_oper_dt[0][metodo.Key];
                }
            }
            PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter PDV_Oper = new PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter();
            RecebePrint("TOTAL\t\t:\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + totaissistema.ToString("0.00"), corpo, rtl.align, 1);
            RecebePrint(" ", mini, esquerda.align, 1);
            RecebePrint("TROCAS\t\t:   " + "" + "\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + ((double)fecha_oper_dt[0]["TROCAS"]).ToString("0.00"), corpo, rtl.align, 1);
            RecebePrint("SUPRIMENTOS\t:   " + "" + "\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + ((double)fecha_oper_dt[0]["SUPRIMENTOS"]).ToString("0.00"), corpo, rtl.align, 1);
            RecebePrint("SANGRIA\t\t:   " + "" + "\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + ((double)fecha_oper_dt[0]["SANGRIAS"]).ToString("0.00"), corpo, rtl.align, 1);


            RecebePrint(new string('>', 15), negrito, esquerda.align, 0);
            RecebePrint(new string('<', 15), negrito, direita.align, 0);
            RecebePrint("DIVERGÊNCIA", negrito, centro.align, 1);
            double totdiverg = 0;
            foreach (KeyValuePair<int, string> metodo in metodos)
            {
                if (metodo.Key == 1)
                {
                    RecebePrint(metodo.Value, corpo, esquerda.align, 0);
                    RecebePrint("\t\t:\tR$", corpo, esquerda.align, 0);
                    if (((double)fecha_infor_dt[0][metodo.Key]) < ((double)fecha_oper_dt[0][metodo.Key] - (double)fecha_oper_dt[0]["SANGRIAS"] + (double)fecha_oper_dt[0]["SUPRIMENTOS"]))
                    {
                        RecebePrint("\t\t" + (((double)fecha_oper_dt[0][metodo.Key]) - ((double)fecha_infor_dt[0][metodo.Key])).ToString("0.00") + "-", corpo, rtl.align, 0);
                        RecebePrint("(NEGATIVO)\t\t", corpo, direita.align, 1);
                        totdiverg += (((double)fecha_oper_dt[0][metodo.Key] - (double)fecha_oper_dt[0]["SANGRIAS"] + (double)fecha_oper_dt[0]["SUPRIMENTOS"]) - ((double)fecha_infor_dt[0][metodo.Key]));
                    }
                    else
                    {
                        RecebePrint("\t\t" + (((double)fecha_infor_dt[0][metodo.Key]) - ((double)fecha_oper_dt[0][metodo.Key])).ToString("0.00"), corpo, rtl.align, 1);
                        totdiverg += ((double)fecha_infor_dt[0][metodo.Key]) - ((double)fecha_oper_dt[0][metodo.Key]);
                    }
                }
                else
                {
                    RecebePrint(metodo.Value, corpo, esquerda.align, 0);
                    RecebePrint("\t\t:\tR$", corpo, esquerda.align, 0);
                    if (((double)fecha_infor_dt[0][metodo.Key]) < ((double)fecha_oper_dt[0][metodo.Key]))
                    {
                        RecebePrint("\t\t" + (((double)fecha_oper_dt[0][metodo.Key]) - ((double)fecha_infor_dt[0][metodo.Key])).ToString("0.00") + "-", corpo, rtl.align, 0);
                        RecebePrint("(NEGATIVO)\t\t", corpo, direita.align, 1);
                        totdiverg += (((double)fecha_oper_dt[0][metodo.Key]) - ((double)fecha_infor_dt[0][metodo.Key]));
                    }
                    else
                    {
                        RecebePrint("\t\t" + (((double)fecha_infor_dt[0][metodo.Key]) - ((double)fecha_oper_dt[0][metodo.Key])).ToString("0.00"), corpo, rtl.align, 1);
                        totdiverg += ((double)fecha_infor_dt[0][metodo.Key]) - ((double)fecha_oper_dt[0][metodo.Key]);
                    }

                }

            }
            RecebePrint(" ", mini, esquerda.align, 1);
            RecebePrint("TROCAS\t\t:   \tR$", corpo, esquerda.align, 0);
            if (((double)fecha_infor_dt[0]["TROCAS"]) < ((double)fecha_oper_dt[0]["TROCAS"]))
            {
                RecebePrint("\t\t" + (((double)fecha_oper_dt[0]["TROCAS"]) - ((double)fecha_infor_dt[0]["TROCAS"])).ToString("0.00") + "-", corpo, rtl.align, 0);
                RecebePrint("(NEGATIVO)\t\t", corpo, direita.align, 1);
            }
            else
            {
                RecebePrint("\t\t" + (((double)fecha_infor_dt[0]["TROCAS"]) - ((double)fecha_oper_dt[0]["TROCAS"])).ToString("0.00"), corpo, rtl.align, 1);
            }
            RecebePrint("SUPRIMENTOS\t:   \tR$", corpo, esquerda.align, 0);
            if (((double)fecha_infor_dt[0]["SUPRIMENTOS"]) < ((double)fecha_oper_dt[0]["SUPRIMENTOS"]))
            {
                RecebePrint("\t\t" + (((double)fecha_oper_dt[0]["SUPRIMENTOS"]) - ((double)fecha_infor_dt[0]["SUPRIMENTOS"])).ToString("0.00") + "-", corpo, rtl.align, 0);
                RecebePrint("(NEGATIVO)\t\t", corpo, direita.align, 1);
            }
            else
            {
                RecebePrint("\t\t" + (((double)fecha_infor_dt[0]["SUPRIMENTOS"]) - ((double)fecha_oper_dt[0]["SUPRIMENTOS"])).ToString("0.00"), corpo, rtl.align, 1);
            }
            RecebePrint("SANGRIA\t\t:   \tR$", corpo, esquerda.align, 0);
            if (((double)fecha_infor_dt[0]["SANGRIAS"]) < ((double)fecha_oper_dt[0]["SANGRIAS"]))
            {
                RecebePrint("\t\t" + (((double)fecha_oper_dt[0]["SANGRIAS"]) - ((double)fecha_infor_dt[0]["SANGRIAS"])).ToString("0.00") + "-", corpo, rtl.align, 0);
                RecebePrint("(NEGATIVO)\t\t", corpo, direita.align, 1);
            }
            else
            {
                RecebePrint("\t\t" + (((double)fecha_infor_dt[0]["SANGRIAS"]) - ((double)fecha_oper_dt[0]["SANGRIAS"])).ToString("0.00"), corpo, rtl.align, 1);
            }
            #region registradores
            RecebePrint(new string('>', 15), negrito, esquerda.align, 0);
            RecebePrint(new string('<', 15), negrito, direita.align, 0);
            RecebePrint("REGISTRADORES", negrito, centro.align, 1);
            cups_cancelados = (int)PDV_Oper.SP_TRI_CONTACUPONS(PDV_WPF.Properties.Settings.Default.no_caixa, "C");
            RecebePrint("CANC. DE CUP.", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + cups_cancelados.ToString("00") + "   -\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + val_cancelado.ToString("0.00"), corpo, rtl.align, 1);

            /*
            RecebePrint("QUAN. DE IT. CAN.", corpo, esquerda.align, false);
            RecebePrint("\t\t" + qte_cancelado.ToString("00"), corpo, esquerda.align, true);
            */

            /*
            RecebePrint("ESTOR. DE IT.", corpo, esquerda.align, false);
            RecebePrint("\t\t" + qte_estornado.ToString("00") + "   -\tR$", corpo, esquerda.align, false);
            RecebePrint("\t\t" + val_estornado.ToString("0.00"), corpo, rtl.align, true);
            */

            RecebePrint("VAL. TOT. VENDAS\t\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + tot_vendas.ToString("0.00"), corpo, rtl.align, 1);
            RecebePrint("VAL. TOT. INFORMADO\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + tot_informado.ToString("0.00"), corpo, rtl.align, 1);

            int numcupons = (int)(PDV_Oper.SP_TRI_CONTACUPONS(PDV_WPF.Properties.Settings.Default.no_caixa, "F"));
            if (numcupons <= 0)
            {
                med_vendas = 0;
            }
            else
            {
                med_vendas = tot_vendas / numcupons;
            }
            RecebePrint("VAL. MÉD. CUPOM\t\tR$", corpo, esquerda.align, 0);
            RecebePrint("\t\t" + med_vendas.ToString("0.00"), corpo, rtl.align, 1);

            /*
            RecebePrint("TOTAL DE ITENS\t\t  ", corpo, esquerda.align, false);
            RecebePrint("\t\t" + tot_itens.ToString("00"), corpo, rtl.align, true);
            */

            RecebePrint(" ", negrito, esquerda.align, 0);
            RecebePrint("OPERADOR(A)" + funcoes.operador.Split(' ')[0], negrito, esquerda.align, 0);
            RecebePrint("\t\t\t" + operador, negrito, esquerda.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("--", mini, esquerda.align, 1);
            RecebePrint("ASS.: OPERADOR(A):      ______________________________", negrito, esquerda.align, 1);
            RecebePrint("--", mini, esquerda.align, 1);
            RecebePrint("ASS.: SUPERVISOR:      _______________________________", negrito, esquerda.align, 1);

            //if (reimpressao == true)
            //{
            //    RecebePrint("Fechamento: " + fechamento.ToShortDateString() + " - " + fechamento.ToLongTimeString(), negrito, centro.align, true);
            //    RecebePrint("--", mini, esquerda.align, true);
            //    RecebePrint("Reimpressão: " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString(), negrito, centro.align, true);
            //}
            //else
                RecebePrint("Fechamento: " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString(), negrito, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia".ToUpper(), corpo, centro.align, 1);
            RecebePrint("(11) 4304-7778", corpo, centro.align, 1);
            #endregion
            try
            {
                Printa(inf);
                PDV_Oper.SP_TRI_LANCACAIXA_CLIPP(PDV_WPF.Properties.Settings.Default.no_caixa.ToString("###"), "X", (decimal?)totaissistema, 1);
                fecha_infor_dt.Clear();
                fecha_oper_dt.Clear();
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }
    }
    class PrintDEVOL
    {
        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static string cnpjempresa;
        public static string nomefantasia;
        public static string enderecodaempresa;
        public static string operador;
        public static int numerodocupom;
        public static List<Produto> produtos = new List<Produto>();
        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, double Xqtde, double Xvalorunit, double Xdesconto, double Xtribest, double Xtribfed)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Extensoes.TruncateLongString(Xdescricao), tipounid = Xtipounid, qtde = Xqtde, valorunit = Xvalorunit, valortotal = Xqtde * (Xvalorunit - Xdesconto), desconto = Xdesconto, trib_est = Xtribest, trib_fed = Xtribfed };
            produtos.Add(prod);
        }
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            double subtotal = 0f;
            #region Region1
            RecebePrint(nomefantasia, negrito, centro.align, 1);
            RecebePrint(enderecodaempresa, corpo, centro.align, 1);
            RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2), corpo, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("CUPOM DE DEVOLUÇÃO", Titulo, centro.align, 1);
            RecebePrint(String.Format("CUPOM Nº {0}", numerodocupom), Titulo, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  VL ITEM R$", corpo, centro.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            int linha = 1;
            foreach (Produto prod in produtos)
            {
                RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda.align, 1);
                RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + (prod.valorunit).ToString("n2"), corpo, esquerda.align, 0);
                RecebePrint((prod.valortotal.ToString("n2")), corpo, direita.align, 1);
                subtotal += prod.valortotal;
                linha += 1;
            }
            RecebePrint("VALOR DA DEVOLUÇÃO R$", Titulo, esquerda.align, 0);
            RecebePrint(subtotal.ToString("n2"), Titulo, direita.align, 1);
            RecebePrint(" ", corpo, esquerda.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint(System.DateTime.Now.ToString(), corpo, esquerda.align, 1);
            RecebePrint("OBRIGADO VOLTE SEMPRE!!", corpo, esquerda.align, 1);
            RecebePrint("Operador: " + operador, corpo, esquerda.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro.align, 1);
            RecebePrint("(11) 4304-7778", corpo, centro.align, 1);
            #endregion
            Printa(inf);
            produtos.Clear();
            linha = 1;
            return true;
        }
    }
    class RelNegativ
    {

        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static string cpfcnpjconsumidor = "";
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();

        public static void RecebeProduto(string Xcodigo, string Xdescricao, double Xvalorunit)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Extensoes.TruncateLongString(Xdescricao), valorunit = Xvalorunit };
            produtos.Add(prod);
        }
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            //RecebePrint(nomefantasia, negrito, centro.align, true);
            //RecebePrint(nomedaempresa, corpo, centro.align, true);
            //RecebePrint(enderecodaempresa, corpo, centro.align, true);
            //RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2) + "  IE: " + ieempresa + "  IM: " + imempresa, corpo, centro.align, true);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("Relatório de itens", Titulo, centro.align, 1);
            RecebePrint("com estoque negativo", Titulo, centro.align, 1);
            RecebePrint(new string('-', 91), negrito, centro.align, 1);
            RecebePrint("COD  DESC", corpo, esquerda.align, 1);
            RecebePrint("VL ITEM R$", corpo, esquerda.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);

            //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
            foreach (Produto prod in produtos)
            {
                RecebePrint(prod.codigo + "\t" + prod.descricao, corpo, esquerda.align, 1);
                RecebePrint((prod.valorunit.ToString("n2")), corpo, esquerda.align, 1);
            }
            //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv

            RecebePrint("Núm. de prod. c/ est. neg.", Titulo, esquerda.align, 0);
            RecebePrint(produtos.Count.ToString(), Titulo, direita.align, 1);
            RecebePrint(new string('-', 91), corpo, centro.align, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro.align, 1);
            RecebePrint("(11) 4304-7778", corpo, centro.align, 1);
            #endregion
            Printa(inf);
            pagamentos.Clear();
            produtos.Clear();
            return true;
        }
    }
    class PrintTEFCliente
    {
        public static Dictionary<string, string> ReciboTEF { get; set; }
        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 9999999);
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            if (ReciboTEF.ContainsKey("712-000") && ReciboTEF["712-000"] != "0")
            {
                foreach (var item in ReciboTEF)
                {
                    if (item.Key.Contains("713"))
                    {
                        RecebePrint(item.Value, corpo, centro.align, 1);
                    }
                }
            }
            #endregion
            Printa(inf);
            return true;
        }
    }
    class PrintTEFEstabel
    {
        public static Dictionary<string, string> ReciboTEF { get; set; }
        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 9999999);
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            if (ReciboTEF.ContainsKey("714-000") && ReciboTEF["714-000"] != "0")
            {
                foreach (var item in ReciboTEF)
                {
                    if (item.Key.Contains("715"))
                    {
                        RecebePrint(item.Value, corpo, centro.align, 1);
                    }
                }
            }
            #endregion
            Printa(inf);
            return true;
        }
    }
    class PrintTEFUnica
    {
        public static Dictionary<string, string> ReciboTEF { get; set; }
        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 9999999);
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            foreach (var item in ReciboTEF)
            {
                if (item.Key.Contains("029"))
                {
                    RecebePrint(item.Value, corpo, centro.align, 1);
                }
            }
            #endregion
            Printa(inf);
            return true;
        }
    }
    class PrintTEFRedux
    {
        public static Dictionary<string, string> ReciboTEF { get; set; }
        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            foreach (var item in ReciboTEF)
            {
                if (item.Key.Contains("711"))
                {
                    RecebePrint(item.Value, corpo, centro.align, 1);
                }
            }
            #endregion
            Printa(inf);
            return true;
        }
    }
    class PrintPRAZO
    {

        static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        static PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static string cliente;
        public static DateTime vencimento;
        public static int terminal;
        public static string operador;
        public static string cupom;
        public static decimal valor;

        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);

            #region Region1
            RecebePrint("Cliente: " + cliente, Titulo, esquerda.align, 1);
            RecebePrint("Cupom: " + cupom, corpo, esquerda.align, 1);
            RecebePrint("Venda a prazo no valor: " + valor.ToString("C2"), negrito, esquerda.align, 1);
            RecebePrint("Vencimento: " + vencimento.ToShortDateString(), negrito, esquerda.align, 1);
            RecebePrint("  ", Titulo, centro.align, 1);
            RecebePrint("Assinatura:_____________________________", Titulo, esquerda.align, 1);
            RecebePrint("Terminal: " + PDV_WPF.Properties.Settings.Default.no_caixa.ToString("D3") + "\t\tOperador: " + operador, corpo, esquerda.align, 1);
            #endregion
            try
            {
                Printa(inf);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }
    }

#pragma warning restore CS0649
}
