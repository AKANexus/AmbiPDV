using MessagingToolkit.QRCode.Codec;
using PDV_WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Printing;
using System.Windows.Forms;
using Zen.Barcode;
using static PDV_ORCAMENTO.PrintFunc;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO
{
    public class Alinhamento
    {
        public StringFormat align;
    }
    public class Fonte
    {
        public Font tipo;
    }
    public class Linha
    {
        public string linha;
        public Font fonte;
        public StringFormat alinhamento;
        public int quebralinha;
        public TipoImpressora tipoImpressora;
    }
    public class PrintFunc
    {
        public static Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        public static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        public static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        public static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        public static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        public static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        public static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        public static PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        public static PaperSize Linha = new PaperSize("Inicio", 400, 5);
        public static PaperSize inf = new PaperSize("Inicio", 400, 999999);


        static List<Linha> transmitir = new List<Linha>();
        public static void RecebePrint(string texto, Fonte font, StringFormat alignment, int breakline, TipoImpressora pTipoImpressora)
        {
            Linha line = new Linha()
            {
                linha = texto,
                fonte = font.tipo,
                alinhamento = alignment,
                quebralinha = breakline,
                tipoImpressora = pTipoImpressora
            };
            transmitir.Add(line);
        }
        public float lastusedheight = 0f;
        public static void GeraTransmissão(object sender, PrintPageEventArgs ev)
        {
            var size = new SizeF();
            float currentUsedHeight = 0f;
            foreach (Linha line in transmitir)
            {
                #region --
                if (line.linha == "--")
                {
                    
                    ev.Graphics.DrawString("00000", line.fonte, Brushes.White, new RectangleF(0, currentUsedHeight, 280, 99999999), line.alinhamento);
                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                }
                #endregion --
                #region COD_BARRAS
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("COD_BARRAS>>"))
                {
                    Code128BarcodeDraw bdf = BarcodeDrawFactory.Code128WithChecksum;
                    ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(12, line.linha.Length - 12), 40), 0f, currentUsedHeight);
                    size.Height = 40f;
                }
                #endregion COD_BARRAS
                #region QR_CODE
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("QR_CODE>>"))
                {
                    //CodeQrBarcodeDraw bdf = BarcodeDrawFactory.CodeQr;
                    //ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(9, line.linha.Length - 9) + PrintCUPOM.assinatura64, 3, 3), 70f, currentUsedHeight);
                    ////ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(9, line.linha.Length - 9), 4, 4), 75f, currentUsedHeight);
                    var qrCodecEncoder = new QRCodeEncoder
                    {
                        QRCodeBackgroundColor = Color.White,
                        QRCodeForegroundColor = Color.Black,
                        CharacterSet = "UTF-8",
                        QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE,
                        QRCodeScale = 2,
                        QRCodeVersion = 100,
                        QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M
                    };
                    Image imageQRCode;
                    //string a ser gerada
                    String data = line.linha.Substring(9, line.linha.Length - 9);
                    imageQRCode = qrCodecEncoder.Encode(data);
                    ev.Graphics.DrawImage(imageQRCode, 72f, currentUsedHeight);
                    imageQRCode.Dispose();
                    size.Height = 150f;
                }
                #endregion QR_CODE
                #region Linha comum
                else
                {
                    switch (line.tipoImpressora)
                    {
                        case TipoImpressora.officeA4:
                            ev.Graphics.DrawString(line.linha, line.fonte, Brushes.Black, new RectangleF(0, 
                                                                                                         currentUsedHeight + 584, //HACK: gambi para centralizar o texto na impressão de teste...
                                                                                                         826, 1169), 
                                                                                                         line.alinhamento);
                            break;
                        case TipoImpressora.thermal80:
                            ev.Graphics.DrawString(line.linha, line.fonte, Brushes.Black, new RectangleF(0, currentUsedHeight, 280, 99999999), line.alinhamento);
                            break;
                        case TipoImpressora.nenhuma:
                            break;
                        default:
                            break;
                    }
                    

                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                }
                #endregion Linha comum
                #region Quebra de linha
                if (line.quebralinha > 0)
                {
                    if (ev.Graphics.MeasureString(line.linha, line.fonte).Width > 280)
                    {
                        currentUsedHeight += size.Height * (line.quebralinha + 1);
                    }
                    else
                    {
                        currentUsedHeight += size.Height * line.quebralinha;
                    }
                }
                #endregion Quebra de linha
                //else if (line.quebralinha == 0)
                //{

                //}
            }
        }
        public static void LimpaSpooler()
        {
            transmitir.Clear();
        }

        //public static void Printa(PaperSize secao)
        //{
        //    using (var printDoc = new PrintDocument())
        //    {
        //        if (Properties.Settings.Default.ImpressoraUSB == "Nenhuma") { return; }

        //        printDoc.PrinterSettings.PrinterName = Properties.Settings.Default.ImpressoraUSB;
        //        printDoc.PrintController = new StandardPrintController();
        //        printDoc.DefaultPageSettings.PaperSize = secao;
        //        printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        //        printDoc.DocumentName = "Cupom";
        //        if (!printDoc.PrinterSettings.IsValid && Properties.Settings.Default.ImpressoraUSB != "Nenhuma")
        //            throw new Exception("Não foi possível localizar a impressora");
        //        printDoc.PrintPage += new PrintPageEventHandler(GeraTransmissão);
        //        if (Properties.Settings.Default.ImpressoraUSB != "Nenhuma")
        //        {
        //            try
        //            {
        //                printDoc.Print();
        //            }
        //            catch (System.ComponentModel.Win32Exception ex)
        //            {
        //                gravarMensagemErro(RetornarMensagemErro(ex, true));
        //                MessageBox.Show("Erro ao imprimir. Certifique-se de não ter selecionado um \"Impressor de PDF\", pois o sistema não oferece suporte a tais programas");
        //                return;
        //            }
        //            catch (Exception ex)
        //            {
        //                string strErrMess = "Erro ao imprimir. \nPor favor entre em contato com a equipe de suporte.";
        //                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
        //                MessageBox.Show(strErrMess);
        //                return;
        //            }
        //        }
        //    }
        //    LimpaSpooler();
        //}

        public static PrintDocument PrintaSpooler(bool cozinha = false, bool duasVias = false)
        {
            using PrintDocument printDoc = new PrintDocument
            {
                PrintController = new StandardPrintController()
            };
            if (Properties.Settings.Default.ImpressoraUSB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
            { return null; }
           
                printDoc.PrinterSettings.PrinterName = Properties.Settings.Default.ImpressoraUSB;
            
            var server = new LocalPrintServer();
            if (!Properties.Settings.Default.ImpressoraUSB.StartsWith(@"\\"))
            {
                PrintQueue queue = server.GetPrintQueue(Properties.Settings.Default.ImpressoraUSB, new string[0] { });
                if (queue.IsInError) throw new Exception("A impressora está em estado de erro.");
                if (queue.IsOutOfPaper) throw new Exception("A impressora está sem papel.");
                if (queue.IsOffline) throw new Exception("A impressora está desligada.");
                if (queue.IsBusy) throw new Exception("A impressora está de boca cheia.");
            }
            printDoc.DocumentName = "Cupom";
            if (!printDoc.PrinterSettings.IsValid && !Properties.Settings.Default.ImpressoraUSB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
                throw new Exception("Não foi possível localizar a impressora");
            printDoc.PrintPage += new PrintPageEventHandler(GeraTransmissão);
            if (!Properties.Settings.Default.ImpressoraUSB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
            {
                {
                    try
                    {
                        printDoc.Print();
                        if (duasVias)
                            printDoc.Print();
                        transmitir.Clear();
                        //LimpaSpooler();
                        return printDoc;
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        gravarMensagemErro(RetornarMensagemErro(ex, true));
                        MessageBox.Show("Erro ao imprimir. Certifique-se de não ter selecionado um \"Impressor de PDF\", pois o sistema não oferece suporte a tais programas");
                        return null;
                    }
                }
            }
            return null;
        }
    }
    public class MetodoPagamento
    {
        public string NomeMetodo;
        public decimal ValorDoPgto;
    }
    public class Produto
    {

        public int numero;
        public string codigo;
        public string descricao;
        public string tipounid;
        public decimal qtde;
        public decimal valorunit;
        public decimal desconto;
        public decimal trib_est;
        public decimal trib_fed;
        public decimal valortotal;

    }
    public class PrintORCA
    {
        public bool FISCAL { get; set; }
        public bool PRAZO { get; set; }
        public bool TEF { get; set; }

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

        public static DateTime? emissao;
        public static DateTime? validade;
        public static DateTime? prevEntrega;

        public static string nomecliente = "";
        public static string transportadora = "";
        public static string nomeSolicitante = "";
        public static string endereco = "";
        public static string bairro = "";
        public static string cep = "";
        public static string cidadeUf = "";
        public static string telComer = "";
        public static string telCelul = "";
        public static string telFax = "";
        public static string telResid = "";
        public static string email = "";
        public static string operador;
        public static int numerosat;
        public static string chavenfe;
        public static string assinaturaQRCODE;
        public static string troco;
        public static decimal valor_prazo;
        public static decimal desconto;
        public static decimal frete;
        public static decimal valorTotal;
        public static string observacoes;
        public static string cliente;
        public static DateTime vencimento;
        public static bool prazo;
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();
        public static Dictionary<string, string> ReciboTEF { get; set; }

        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, decimal Xqtde, decimal Xvalorunit, decimal Xdesconto, decimal Xvalortotal, decimal Xtribest, decimal Xtribfed)
        {
            var prod = new Produto()
            {
                codigo = Xcodigo,
                descricao = Extensoes.TruncateLongString(Xdescricao),
                tipounid = Xtipounid,
                qtde = Xqtde,
                valorunit = Xvalorunit,
                //valortotal = Xqtde * (Xvalorunit - Xdesconto),
                valortotal = Xvalortotal,
                desconto = Xdesconto,
                trib_est = Xtribest,
                trib_fed = Xtribfed
            };
            produtos.Add(prod);
        }

        public static void RecebePagamento(string Xmetodo, decimal Xvalor)
        {
            var method = new MetodoPagamento { NomeMetodo = Xmetodo, ValorDoPgto = Xvalor };
            pagamentos.Add(method);
        }

        public static bool IMPRIME(bool cupom, int vias_cliente, int vias_estab, int vias_unica, int vias_redux, int vias_prazo, TipoImpressora tipoImpressora)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            decimal subtotal = 0m;
            //decimal tributostot = 0m;
            int linha = 1;
            if (cupom)
            {
                #region Cumpom de Venda
                RecebePrint(nomefantasia, negrito, centro.align, 1, tipoImpressora);
                RecebePrint(nomedaempresa, corpo, centro.align, 1, tipoImpressora);
                RecebePrint(enderecodaempresa, corpo, centro.align, 1, tipoImpressora);
                RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2) + "  IE: " + ieempresa + "  IM: " + imempresa, corpo, centro.align, 1, tipoImpressora);
                RecebePrint(new string('-', 91), negrito, centro.align, 1, tipoImpressora);
                RecebePrint("Orçamento No. " + numerodoextrato, Titulo, centro.align, 1, tipoImpressora);

                // emissão
                if (emissao != null)
                {
                    RecebePrint("Emissão ", negrito, esquerda.align, 0, tipoImpressora);
                    RecebePrint(((DateTime)emissao).ToShortDateString(), corpo, direita.align, 1, tipoImpressora);
                }
                // validade
                if (validade != null)
                {
                    RecebePrint("Validade ", negrito, esquerda.align, 0, tipoImpressora);
                    RecebePrint(((DateTime)validade).ToShortDateString(), corpo, direita.align, 1, tipoImpressora);
                }
                // previsão de entrega
                if (prevEntrega != null)
                {
                    RecebePrint("Prev. Entrega ", negrito, esquerda.align, 0, tipoImpressora);
                    RecebePrint(((DateTime)prevEntrega).ToShortDateString(), corpo, direita.align, 1, tipoImpressora);
                }

                RecebePrint(new string('-', 91), negrito, centro.align, 1, tipoImpressora);
                if (!string.IsNullOrEmpty(nomecliente))
                {
                    if (nomecliente == "")
                    {
                        RecebePrint("CLIENTE: Consumidor não Informado", corpo, esquerda.align, 1, tipoImpressora);
                    }
                    else
                    {
                        //RecebePrint("CLIENTE: " + nomecliente, corpo, esquerda.align, 1, tipoImpressora);
                        RecebePrint("CLIENTE: ", negrito, esquerda.align, 0, tipoImpressora);
                        RecebePrint(nomecliente, corpo, direita.align, 1, tipoImpressora);
                    }
                }
                else
                {
                    RecebePrint("CLIENTE: Consumidor não Informado", corpo, esquerda.align, 1, tipoImpressora);
                }

                if (!string.IsNullOrWhiteSpace(nomeSolicitante))
                {
                    RecebePrint("Solicitante: " + nomeSolicitante, corpo, esquerda.align, 1, tipoImpressora);
                }

                if (!string.IsNullOrWhiteSpace(endereco))
                {
                    RecebePrint("Endereço: " + endereco, corpo, esquerda.align, 1, tipoImpressora);
                }
                if (!string.IsNullOrWhiteSpace(bairro))
                {
                    RecebePrint("Bairro: " + bairro, corpo, esquerda.align, 1, tipoImpressora);
                }
                if (!string.IsNullOrWhiteSpace(cep))
                {
                    RecebePrint("CEP: " + cep, corpo, esquerda.align, 1, tipoImpressora);
                }
                if (!string.IsNullOrWhiteSpace(cidadeUf))
                {
                    RecebePrint("Cidade/UF: " + cidadeUf, corpo, esquerda.align, 1, tipoImpressora);
                }

                if (!string.IsNullOrWhiteSpace(telComer))
                {
                    RecebePrint("Comercial: " + telComer, corpo, esquerda.align, 1, tipoImpressora);
                }
                if (!string.IsNullOrWhiteSpace(telCelul))
                {
                    RecebePrint("Celular: " + telCelul, corpo, esquerda.align, 1, tipoImpressora);
                }
                if (!string.IsNullOrWhiteSpace(telFax))
                {
                    RecebePrint("Fax: " + telFax, corpo, esquerda.align, 1, tipoImpressora);
                }
                if (!string.IsNullOrWhiteSpace(telResid))
                {
                    RecebePrint("Residencial: " + telResid, corpo, esquerda.align, 1, tipoImpressora);
                }
                if (!string.IsNullOrWhiteSpace(email))
                {
                    RecebePrint("E-mail: " + email, corpo, esquerda.align, 1, tipoImpressora);
                }

                if (!string.IsNullOrWhiteSpace(transportadora))
                {
                    RecebePrint("TRANSPORTADORA: ", negrito, esquerda.align, 0, tipoImpressora);
                    RecebePrint(transportadora, corpo, direita.align, 1, tipoImpressora);
                }

                RecebePrint(new string('-', 91), corpo, centro.align, 1, tipoImpressora);
                RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  (VLTR R$)*  VL ITEM R$", corpo, centro.align, 1, tipoImpressora);
                RecebePrint(new string('-', 91), corpo, centro.align, 1, tipoImpressora);

                //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
                foreach (Produto prod in produtos)
                {
                    RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda.align, 1, tipoImpressora);
                    RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + (prod.valorunit).ToString("n2"), corpo, esquerda.align, 0, tipoImpressora);
                    RecebePrint("\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(" + ((prod.valorunit) * (prod.trib_est) / 100).ToString("n2") + ")", corpo, esquerda.align, 0, tipoImpressora);
                    RecebePrint((prod.valortotal.ToString("n2")), corpo, direita.align, 1, tipoImpressora);
                    subtotal += prod.valortotal;
                    linha += 1;
                }
                //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv

                RecebePrint("SubTotal R$", negrito, esquerda.align, 0, tipoImpressora);
                RecebePrint((subtotal).ToString("n2"), Titulo, direita.align, 1, tipoImpressora);

                if (desconto != 0)
                {
                    RecebePrint("Desconto R$", corpo, esquerda.align, 0, tipoImpressora);
                    RecebePrint(desconto.ToString("n2"), corpo, direita.align, 1, tipoImpressora);
                }

                if (frete != 0)
                {
                    RecebePrint("Frete R$", corpo, esquerda.align, 0, tipoImpressora);
                    RecebePrint(frete.ToString("n2"), corpo, direita.align, 1, tipoImpressora);
                }

                foreach (MetodoPagamento met in pagamentos)
                {
                    RecebePrint(met.NomeMetodo, corpo, esquerda.align, 0, tipoImpressora);
                    RecebePrint(met.ValorDoPgto.ToString("n2"), corpo, direita.align, 1, tipoImpressora);
                }


                RecebePrint("VALOR TOTAL R$", Titulo, esquerda.align, 0, tipoImpressora);
                RecebePrint((valorTotal).ToString("n2"), Titulo, direita.align, 1, tipoImpressora);


                RecebePrint(" ", corpo, esquerda.align, 1, tipoImpressora);
                //RecebePrint("LINHA EXTRA DE INFORMAÇÃO", corpo, esquerda.align, true);

                if (!string.IsNullOrWhiteSpace(observacoes))
                {
                    RecebePrint(new string('-', 91), corpo, centro.align, 1, tipoImpressora);

                    RecebePrint("Observações", Titulo, esquerda.align, 1, tipoImpressora);
                    RecebePrint(observacoes, corpo, esquerda.align, 2, tipoImpressora);

                    RecebePrint(new string('-', 91), corpo, centro.align, 1, tipoImpressora);
                }

                RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro.align, 1, tipoImpressora);
                RecebePrint("(11) 4304-7778", corpo, centro.align, 1, tipoImpressora);
                #endregion
            }

            for (int i = 0; i < vias_prazo; i++)
            {
                #region Comprovante de Venda a Prazo
                if (cupom == false)
                {
                    RecebePrint("COMPROVANTE DE VENDA A PRAZO", Titulo, centro.align, 2, tipoImpressora);
                }
                RecebePrint("Cliente: " + cliente, Titulo, esquerda.align, 3, tipoImpressora);
                RecebePrint("Cupom: " + numerodoextrato, corpo, esquerda.align, 1, tipoImpressora);
                RecebePrint("Venda a prazo no valor: " + valor_prazo.ToString("C2"), negrito, esquerda.align, 1, tipoImpressora);
                RecebePrint("Vencimento: " + vencimento.ToShortDateString(), Titulo, esquerda.align, 2, tipoImpressora);
                RecebePrint("  ", Titulo, centro.align, 1, tipoImpressora);
                RecebePrint("Assinatura:_____________________________", Titulo, esquerda.align, 2, tipoImpressora);
                RecebePrint("Vendedor: " + operador, corpo, esquerda.align, 2, tipoImpressora);
                if (cupom == false)
                {
                    RecebePrint("Sr(a) Operador(a), guarde este canhoto para o", negrito, centro.align, 1, tipoImpressora);
                    RecebePrint("lançamento durante o fechamento do turno.", negrito, centro.align, 1, tipoImpressora);
                    RecebePrint(DateTime.Today.ToShortDateString(), Titulo, centro.align, 1, tipoImpressora);
                }

                #endregion

                RecebePrint(new string('-', 91), negrito, centro.align, 1, tipoImpressora);
            }

            try
            {
                PrintaSpooler();
            }
            catch (Exception ex)
            {
                gravarMensagemErro(RetornarMensagemErro(ex, true));
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

}
