using Clearcove.Logging;
using FirebirdSql.Data.FirebirdClient;
using MessagingToolkit.QRCode.Codec;
using PDV_WPF.DataSets;
using PDV_WPF.DataSets.FDBDataSetVendaTableAdapters;
using PDV_WPF.FDBDataSetTableAdapters;
using PDV_WPF.Objetos;
using PDV_WPF.Properties;
using PDV_WPF.Telas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Zen.Barcode;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;
using static PDV_WPF.Funcoes.Extensions;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.PrintFunc;
using DarumaDLL = LocalDarumaFrameworkDLL.UnsafeNativeMethods;

namespace PDV_WPF
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
    }
    public class PrintFunc
    {
        public static Fonte titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        public static Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        public static Fonte italico = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Italic) };
        public static Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        public static Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        public static Fonte mini = new Fonte { tipo = new Font("Arial Narrow", 4f, FontStyle.Regular | FontStyle.Italic) };
        public static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        public static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        public static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        public static Alinhamento rtl = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near, FormatFlags = StringFormatFlags.DirectionRightToLeft } };
        private static List<Linha> transmitir = new List<Linha>();
        private static List<Linha> retransmitir = new List<Linha>();
        public float lastusedheight = 0f;

        public static void RecebePrint(string texto, Fonte font, Alinhamento alignment, int breakline, bool retransmissao = false)
        {
            //audit("PRINTFUNC>> Recebe print: " + texto + " Breakline: " + breakline);
            Linha line = new Linha()
            {
                linha = texto,
                fonte = font.tipo,
                alinhamento = alignment.align,
                quebralinha = breakline
            };
            transmitir.Add(line);
        }
        public static void GeraTransmissao(object sender, PrintPageEventArgs ev)
        {
            int largura_pagina = 280; //de 290
            //if (IMPRESSORA_USB.Contains("78COL")) //TODO -- DONE --: Oficial, deve ser 78COL
            //{
            //    largura_pagina = 280;
            //}


            SizeF size = new SizeF();
            float currentUsedHeight = 0f;
            foreach (Linha line in transmitir)
            {
                if (line.linha == "--")
                {
                    ev.Graphics.DrawString("00000", line.fonte, Brushes.White, new RectangleF(0, currentUsedHeight, largura_pagina, 99999999), line.alinhamento);
                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("CODABAR>>"))
                {
                    Code128BarcodeDraw bdf = BarcodeDrawFactory.Code128WithChecksum;

                    ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(12, line.linha.Length - 12), 40), 0f, currentUsedHeight);
                    size.Height = 40f;
                }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("CENTBAR>>"))
                {
                    Code128BarcodeDraw bdf = BarcodeDrawFactory.Code128WithChecksum;

                    ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(12, line.linha.Length - 12), 40), 90f, currentUsedHeight);
                    size.Height = 40f;
                }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("EAN13>>"))
                {
                    CodeEan13BarcodeDraw bdf = BarcodeDrawFactory.CodeEan13WithChecksum;

                    ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(12, line.linha.Length - 12), 40), 0f, currentUsedHeight);
                    size.Height = 40f;
                }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("QR_CODE>>"))
                {
                    //CodeQrBarcodeDraw bdf = BarcodeDrawFactory.CodeQr;
                    //ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(9, line.linha.Length - 9) + PrintCUPOM.assinatura64, 3, 3), 70f, currentUsedHeight);
                    ////ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(9, line.linha.Length - 9), 4, 4), 75f, currentUsedHeight);
                    QRCodeEncoder qrCodecEncoder = new QRCodeEncoder
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
                    ev.Graphics.DrawImage(imageQRCode, 64f, currentUsedHeight);
                    imageQRCode.Dispose();
                    size.Height = 100f;
                }
                else
                {
                    ev.Graphics.DrawString(line.linha, line.fonte, Brushes.Black, new RectangleF(0, currentUsedHeight, largura_pagina, 99999999), line.alinhamento);
                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                }
                if (line.quebralinha > 0)
                {
                    if (ev.Graphics.MeasureString(line.linha, line.fonte).Width > largura_pagina)
                    {
                        currentUsedHeight += size.Height * (line.quebralinha + 1);
                    }
                    else
                    {
                        currentUsedHeight += size.Height * line.quebralinha;
                    }
                }
                else if (line.quebralinha == 0)
                {

                }
            }
        }

        public static void GeraReTransmissao(object sender, PrintPageEventArgs ev)
        {
            int largura_pagina = 280; //de 290
            //if (IMPRESSORA_USB.Contains("78COL")) //TODO -- DONE --: Oficial, deve ser 78COL
            //{
            //    largura_pagina = 280;
            //}

            SizeF size = new SizeF();
            float currentUsedHeight = 0f;
            foreach (Linha line in retransmitir)
            {
                if (line.linha == "--")
                {
                    ev.Graphics.DrawString("00000", line.fonte, Brushes.White, new RectangleF(0, currentUsedHeight, largura_pagina, 99999999), line.alinhamento);
                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("CODABAR>>"))
                {
                    Code128BarcodeDraw bdf = BarcodeDrawFactory.Code128WithChecksum;
                    ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(12, line.linha.Length - 12), 40), 0f, currentUsedHeight);
                    size.Height = 40f;
                }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("QR_CODE>>"))
                {
                    QRCodeEncoder qrCodecEncoder = new QRCodeEncoder
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
                    String data = line.linha.Substring(9, line.linha.Length - 9);
                    imageQRCode = qrCodecEncoder.Encode(data);
                    ev.Graphics.DrawImage(imageQRCode, 68f, currentUsedHeight);
                    imageQRCode.Dispose();
                    size.Height = 120f;
                }
                else
                {
                    ev.Graphics.DrawString(line.linha, line.fonte, Brushes.Black, new RectangleF(0, currentUsedHeight, largura_pagina, 99999999), line.alinhamento);
                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                }
                if (line.quebralinha > 0)
                {
                    if (ev.Graphics.MeasureString(line.linha, line.fonte).Width > largura_pagina)
                    {
                        currentUsedHeight += size.Height * (line.quebralinha + 1);
                    }
                    else
                    {
                        currentUsedHeight += size.Height * line.quebralinha;
                    }
                }
                else if (line.quebralinha == 0)
                {

                }
            }
        }
        //public static void LimpaSpooler()
        //{
        //    retransmitir.AddRange(transmitir);
        //    transmitir.Clear();
        //}
        //public static void LimpaRePrint()
        //{
        //    retransmitir.Clear();
        //}

        public static PrintDocument PrintaSpooler(bool cozinha = false, bool duasVias = false)
        {
            using PrintDocument printDoc = new PrintDocument
            {
                PrintController = new StandardPrintController()
            };
            if (IMPRESSORA_USB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
            { return null; }
            #region AmbiMAITRE
            if (cozinha)
            {
                printDoc.PrinterSettings.PrinterName = IMPRESSORA_USB_PED;
            }
            #endregion AmbiMAITRE
            else
            {
                printDoc.PrinterSettings.PrinterName = IMPRESSORA_USB;
            }
            var server = new LocalPrintServer();
            if (!IMPRESSORA_USB.StartsWith(@"\\"))
            {
                PrintQueue queue = server.GetPrintQueue(IMPRESSORA_USB, new string[0] { });
                if (queue.IsInError) throw new Exception("A impressora está em estado de erro.");
                if (queue.IsOutOfPaper) throw new Exception("A impressora está sem papel.");
                if (queue.IsOffline) throw new Exception("A impressora está desligada.");
                if (queue.IsBusy) throw new Exception("A impressora está de boca cheia.");
            }
            printDoc.DocumentName = "Cupom";
            if (!printDoc.PrinterSettings.IsValid && !IMPRESSORA_USB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
                throw new Exception("Não foi possível localizar a impressora");
            printDoc.PrintPage += new PrintPageEventHandler(GeraTransmissao);
            if (!IMPRESSORA_USB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
            {
                //else
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
                        logErroAntigo(RetornarMensagemErro(ex, true));
                        MessageBox.Show("Erro ao imprimir. Certifique-se de não ter selecionado um \"Impressor de PDF\", pois o sistema não oferece suporte a tais programas");
                        return null;
                    }
                }
            }
            return null;
        }
        //public static void RePrinta()
        //{
        //    if (retransmitir.Count == 0) return;
        //    using PrintDocument printDoc = new PrintDocument
        //    {
        //        PrintController = new StandardPrintController()
        //    };
        //    //PaperSource paperSrc = new PaperSource
        //    //{
        //    //    SourceName = "Bobina"
        //    //};
        //    //printDoc.PrinterSettings.PrinterName = "DR800";
        //    if (IMPRESSORA_USB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
        //    { return; }
        //    //MessageBox.Show("PDV_WPF.Properties.Settings.Default.ImpressoraUSB: " + PDV_WPF.Properties.Settings.Default.ImpressoraUSB);
        //    printDoc.PrinterSettings.PrinterName = IMPRESSORA_USB;
        //    //printDoc.PrinterSettings.PrinterName = "Foxit Reader PDF Printer"; 
        //    //printDoc.DefaultPageSettings.PaperSource = paperSrc;
        //    //printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
        //    printDoc.DocumentName = "Cupom";
        //    if (!printDoc.PrinterSettings.IsValid && !IMPRESSORA_USB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
        //        throw new Exception("Não foi possível localizar a impressora");
        //    printDoc.PrintPage += new PrintPageEventHandler(GeraReTransmissao);
        //    if (!IMPRESSORA_USB.Equals("nenhuma", StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        //else
        //        {
        //            try
        //            {
        //                printDoc.Print();
        //                return;
        //            }
        //            catch (System.ComponentModel.Win32Exception ex)
        //            {
        //                logErroAntigo(RetornarMensagemErro(ex, true));
        //                MessageBox.Show("Erro ao imprimir. Certifique-se de não ter selecionado um \"Impressor de PDF\", pois o sistema não oferece suporte a tais programas");
        //                return;
        //            }
        //        }
        //    }
        //    return;
        //}
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
        public decimal trib_mun;
        public decimal valortotal;

    }
    internal class PrintCANCL
    {
        public bool usouTEF = false;
        public int numerodoextrato;
        public int cupomcancelado;
        public string cpfcnpjconsumidor = "";
        public string _operador;
        public int numerosat;
        public string chavenfe;
        public string assinaturaQRCODE;
        public decimal total;

        private void LinhaHorizontal()
        {
            /*if (IMPRESSORA_USB.Contains("78COL")) */
            RecebePrint(new string('-', 87), negrito, centro, 1);
            //else RecebePrint(new string('-', 91), negrito, centro, 1);
        }

        public void IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);

            RecebePrint(Emitente.NomeFantasia, negrito, centro, 1);
            RecebePrint(Emitente.RazaoSocial, corpo, centro, 1);
            RecebePrint(Emitente.EnderecoCompleto, corpo, centro, 1);
            RecebePrint($"CNPJ: {Emitente.CNPJ}  IE: {Emitente.IM}  IM: {Emitente.IM}", corpo, centro, 1);
            LinhaHorizontal();
            RecebePrint("Extrato No. " + numerodoextrato, titulo, centro, 1);
            RecebePrint("CUPOM FISCAL ELETRÔNICO - SAT", titulo, centro, 1);
            if (numerosat > 99999999) RecebePrint("### HOMOLOGAÇÃO - SEM VALOR FISCAL ###", titulo, centro, 1);
            RecebePrint("CANCELAMENTO", titulo, centro, 1);
            LinhaHorizontal();
            RecebePrint("DADOS DO CUPOM FISCAL ELETRÔNICO CANCELADO", negrito, esquerda, 1);
            RecebePrint("", negrito, esquerda, 1);
            if (cpfcnpjconsumidor == "" || cpfcnpjconsumidor == null)
            {
                RecebePrint("CNPJ do Consumidor: Não informado.", corpo, esquerda, 1);
            }
            else if (cpfcnpjconsumidor.Length == 11)
            {
                //string cpfcons = cpfcnpjconsumidor.ToString().Substring(0, 3) + "." + cpfcnpjconsumidor.ToString().Substring(3, 3) + "." + cpfcnpjconsumidor.ToString().Substring(6, 3) + "-" + cpfcnpjconsumidor.ToString().Substring(9, 2);
                RecebePrint("CPF do Consumidor: " + cpfcnpjconsumidor, corpo, esquerda, 1);
            }
            else if (cpfcnpjconsumidor.Length == 14)
            {
                //string cnpjcons = cpfcnpjconsumidor.ToString().Substring(0, 2) + "." + cpfcnpjconsumidor.ToString().Substring(2, 3) + "." + cpfcnpjconsumidor.ToString().Substring(5, 3) + "/" + cpfcnpjconsumidor.ToString().Substring(8, 4) + "-" + cpfcnpjconsumidor.ToString().Substring(12, 2);
                RecebePrint("CNPJ do Consumidor: " + cpfcnpjconsumidor, corpo, esquerda, 1);
            }
            RecebePrint("TOTAL: R$ " + total.ToString("n2"), titulo, esquerda, 1);
            RecebePrint("CUPOM CANCELADO: " + cupomcancelado, corpo, esquerda, 1);
            RecebePrint("", negrito, esquerda, 1);
            if (usouTEF)
            {
                RecebePrint("Exija o cancelamento da cobrança em seu cartão", negrito, centro, 1);
                RecebePrint("", negrito, esquerda, 1);
            }
            if (numerosat > 99999999) RecebePrint("### HOMOLOGAÇÃO - SEM VALOR FISCAL ###", titulo, centro, 1);
            RecebePrint("SAT No. " + numerosat.ToString(), negrito, centro, 1);
            RecebePrint(DateTime.Now.ToString(), negrito, centro, 1);
            RecebePrint("", negrito, esquerda, 1);
            RecebePrint(Regex.Replace(chavenfe, " {4}", "$0,"), corpo, centro, 1);
            RecebePrint("CODABAR>>" + chavenfe, corpo, centro, 1);
            RecebePrint("QR_CODE>>" + assinaturaQRCODE, corpo, centro, 1);
            RecebePrint("Consulte o QR Code pelo aplicativo \"De olho na nota\",", corpo, centro, 1);
            RecebePrint("disponível na Play Store e na AppStore", corpo, centro, 1);
            if (numerosat > 99999999) RecebePrint("### HOMOLOGAÇÃO - SEM VALOR FISCAL ###", titulo, centro, 1);
            LinhaHorizontal();
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            PrintaSpooler();
        }
    }
    internal class PrintSANSUP
    {
        public string operacao;
        public decimal valor;
        public string numcaixa;
        public bool reimpressao = false;

        public bool IMPRIME()
        {
            if (IMPRESSORA_USB != "Nenhuma")
            { return IMPRIME_SPOOLER(); }
            else if (ECF_ATIVA == true)
            {
                try
                {
                    #region Checa se a impressora está pronta para imprimir relatórios gerenciais
                    int resposta = 0;
                    resposta = DarumaDLL.confCadastrar_ECF_Daruma("RG", "TROCA DE TURNO", "");
                    resposta = DarumaDLL.iRGAbrir_ECF_Daruma("TROCA DE TURNO"); // Convertido diretamente do AmbisoftPDV (VB6)
                    if (resposta == 1)
                    {
                        int erro = 0;
                        erro = DarumaDLL.eRetornarErro_ECF_Daruma();
                        switch (erro)
                        {
                            case 0:
                                break;
                            case 78:
                                DarumaDLL.iCFCancelar_ECF_Daruma();
                                DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Havia um cupom aberto, que foi cancelado");
                                break;
                            case 88:
                                DarumaDLL.iCFCancelar_ECF_Daruma();
                                DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Redução Z pendente");
                                return false;
                            case 89:
                                DarumaDLL.iCFCancelar_ECF_Daruma();
                                DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Redução Z já foi feita.");
                                return false;
                            default:
                                DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, $"Erro: {erro}.");
                                return false;
                        }
                    }
                    #endregion
                    PrintRELATORIOECF rELATORIOECF = new PrintRELATORIOECF();
                    rELATORIOECF.CentraECF("<b>" + "Comprovante de ".ToUpper() + operacao + "</b>");
                    rELATORIOECF.CentraECF("Caixa Nº  " + numcaixa);
                    rELATORIOECF.DivisorECF();
                    rELATORIOECF.CentraECF(DateTime.Now.ToShortDateString() + ", " + DateTime.Now.ToLongTimeString());
                    rELATORIOECF.TextoECF("<e>Valor: " + valor.ToString("c2") + "</e>");
                    rELATORIOECF.TextoECF("<b>Operador: " + operador + "</b>");
                    rELATORIOECF.TextoECF("<e>Recebido por: ________________________</e>");
                    rELATORIOECF.DivisorECF();
                    rELATORIOECF.CentraECF("<c>" + "Trilha Informática - Soluções e Tecnologia".ToUpper());
                    rELATORIOECF.CentraECF("(11) 4304-7778</c>");
                    rELATORIOECF.CentraECF(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO);
                    rELATORIOECF.ImprimeTextoGuardado();
                    DarumaDLL.eAbrirGaveta_ECF_Daruma();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    DarumaDLL.iRGFechar_ECF_Daruma();
                }
            }
            return false;
        }

        private bool IMPRIME_SPOOLER()
        {

            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            RecebePrint("Comprovante de ".ToUpper() + operacao, titulo, centro, 1);
            RecebePrint("Caixa Nº  " + numcaixa, titulo, centro, 1);
            if (reimpressao) RecebePrint(">>>>>>> REIMPRESSÃO <<<<<<<", titulo, centro, 1);
            RecebePrint(new string('-', 81), negrito, centro, 1);
            RecebePrint(DateTime.Now.ToShortDateString() + ", " + DateTime.Now.ToLongTimeString(), negrito, centro, 1);
            //PrintFunc.RecebePrint(" ", Titulo, centro, true);
            RecebePrint("Valor: " + valor.ToString("c2"), titulo, esquerda, 1);
            //PrintFunc.RecebePrint("Operação: " + operacao, negrito, esquerda, true);
            RecebePrint("Operador: " + operador, negrito, esquerda, 1);
            if (!reimpressao) RecebePrint("Recebido por: ________________________", titulo, esquerda, 1);
            if (reimpressao) RecebePrint(">>>>>>> REIMPRESSÃO <<<<<<<", titulo, centro, 1);
            RecebePrint(new string('-', 81), corpo, esquerda, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            #endregion

            PrintaSpooler();
            return true;
        }
    }
    internal class PrintFECHA : IDisposable
    {
        #region Fields & Properties

#pragma warning disable CS0649
        public string num_caixa;
        public string cnpjempresa;
        public string nomefantasia;
        public string enderecodaempresa;

        public FDBDataSetVenda.TRI_PDV_OPERDataTable fecha_infor_dt;
        public FDBDataSetVenda.TRI_PDV_OPERDataTable fecha_oper_dt;
        public List<double> val_pagos { get; set; }


        public decimal val_recargas;
        public decimal qte_cancelado;
        public decimal val_cancelado;
        public decimal cups_cancelados;
        public decimal qte_estornado;
        public decimal val_estornado;
        public decimal tot_vendas = 0;
        public decimal tot_informado;
        public decimal med_vendas = 0;
        public decimal tot_itens = 0;
        public decimal totaissistema { get; set; }
        public decimal totalMovdiario { get; set; }
#pragma warning restore CS0649
        //public static bool reimpressao; //TODO: Permitir a reimpressão do fechamento.
        public DateTime fechamento = DateTime.Now;

        #endregion Fields & Properties

        #region (De)Constructor

        public PrintFECHA()
        {
            fecha_infor_dt = new FDBDataSetVenda.TRI_PDV_OPERDataTable();
            fecha_oper_dt = new FDBDataSetVenda.TRI_PDV_OPERDataTable();
        }

        #endregion (De)Constructor

        #region Methods

        public void Dispose()
        {
            fecha_infor_dt?.Dispose();
            fecha_oper_dt?.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtmFechado">Deve ser preenchido apenas na reimpressão</param>
        /// <param name="intIdCaixa">Deve ser preenchido apenas na reimpressão</param>
        /// <param name="blnFazerFechamento">True apenas se for chamado durante um fechamento de caixa. Para reimpressão, deve ser false.</param>
        /// <returns></returns>
        public bool IMPRIME(DateTime dtmFechado, FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable METODOS_DT, int intIdCaixa, bool blnFazerFechamento = true)
        {
            if (IMPRESSORA_USB != "Nenhuma")
            {
                try
                {
                    return IMPRIME_SPOOLER(dtmFechado, METODOS_DT, intIdCaixa, blnFazerFechamento);
                }
                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show("Erro ao imprimir pelo spooler. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                    Environment.Exit(0); // DEURUIM();
                    return false;
                }
            }
            if (ECF_ATIVA)
            {
                try
                {
                    return IMPRIME_ECF(dtmFechado, METODOS_DT, intIdCaixa, blnFazerFechamento);
                }

                catch (Exception ex)
                {
                    logErroAntigo(RetornarMensagemErro(ex, true));
                    MessageBox.Show("Erro ao imprimir pela ECF. \n\nO aplicativo deve ser fechado. \n\nSe o problema persistir, por favor entre em contato com a equipe de suporte.");
                    Environment.Exit(0); // DEURUIM();
                    return false;
                }
            }
            return false;
        }
        private bool IMPRIME_ECF(DateTime dtmFechado, FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable METODOS_DT, int intIdCaixa = 0, bool blnFazerFechamento = true)
        {
            Logger log = new Logger("Imprime ECF");
            int numcupons = 0;
            totaissistema = 0;
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var FMPGTO_TA = new SP_TRI_CONTAFMPGTOTableAdapter();
            using var Oper = new TRI_PDV_OPERTableAdapter();
            using var taCupomPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_CUPOMTableAdapter();
            FMPGTO_TA.Connection = LOCAL_FB_CONN;
            Oper.Connection = LOCAL_FB_CONN;
            taCupomPdv.Connection = LOCAL_FB_CONN;

            FDBDataSet.SP_TRI_CONTANFVPAGTODataTable contagemNaoFiscal = new FDBDataSet.SP_TRI_CONTANFVPAGTODataTable();
            FDBDataSet.SP_TRI_CONTANFVPAGTODataTable contagemFiscal = new FDBDataSet.SP_TRI_CONTANFVPAGTODataTable();
            DateTime abertura = new DateTime();
            DateTime fechamento = new DateTime();
            if (blnFazerFechamento)
            {
                fecha_oper_dt = Oper.GetByCaixaAberto(NO_CAIXA);
                if (fecha_oper_dt.Count > 1) logErroAntigo("IMPRIME ECF >> Mais de uma entrada em GetByCaixaAberto foi encontrada. Estado inválido.");
                abertura = Oper.GetByCaixaAberto(NO_CAIXA)[0].CURRENTTIME;
                fechamento = DateTime.Now;
            }
            else
            {
                Oper.FillByCaixaFech(fecha_oper_dt, intIdCaixa, dtmFechado);
                if (fecha_oper_dt.Count > 1) logErroAntigo("IMPRIME ECF >> Mais de uma entrada em GetByCaixaAberto foi encontrada. Estado inválido.");
                abertura = fecha_oper_dt[0].CURRENTTIME;
                fechamento = dtmFechado;
            }
            PrintRELATORIOECF rELATORIOECF = new PrintRELATORIOECF();
            using (var ContaFormasPagto = new SP_TRI_CONTANFVPAGTOTableAdapter())
            {
                ContaFormasPagto.Connection = LOCAL_FB_CONN;
                if (ECF_ATIVA) ContaFormasPagto.Fill(contagemFiscal, "E" + NO_CAIXA.ToString(), abertura, fechamento, "I");
                if (SAT_USADO) ContaFormasPagto.Fill(contagemFiscal, NO_CAIXA.ToString(), abertura, fechamento, "I");
                ContaFormasPagto.Fill(contagemNaoFiscal, "N" + NO_CAIXA.ToString(), abertura, fechamento, "I");
            }

            rELATORIOECF.CentraECF($"<b>{nomefantasia}</b>");
            rELATORIOECF.CentraECF($"<b>{enderecodaempresa}</b>");
            rELATORIOECF.CentraECF($"<b>{enderecodaempresa}</b>");
            rELATORIOECF.DivisorECF();
            rELATORIOECF.CentraECF($"<e> FECH. DO CAIXA Nº{NO_CAIXA:000}</e>");
            rELATORIOECF.CentraECF($"Abertura: {abertura}");
            if (!blnFazerFechamento)
            {
                rELATORIOECF.CentraECF($"<e>-- Reimpressão --</e>");
            }
            rELATORIOECF.DivisorECF();
            decimal sangrias = 0, suprimentos = 0;
            var statuses = METODOS_DT.Select(x => new { COD_CFE = x.ID_NFCE, x.STATUS, x.DESCRICAO, x.ID_FMANFCE });
            List<(string COD_CFE, decimal VALOR, int ID_FMANFCE, string DESCRICAO)> valoresOperacionais = new List<(string, decimal, int, string)>();
            using (var SomaValoresFmapagto = new SomaValoresFmapagtoTableAdapter())
            {
                foreach (var metodo in statuses)
                {
                    SomaValoresFmapagto.Connection = LOCAL_FB_CONN;
                    decimal valorSomado, valorSAT, valorNAOFISCAL, valorECF;
                    valorSAT = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, NO_CAIXA.ToString(), fechamento) ?? 0M;
                    valorNAOFISCAL = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "N" + NO_CAIXA.ToString(), fechamento) ?? 0M;
                    valorECF = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "E" + NO_CAIXA.ToString(), fechamento) ?? 0M;
                    log.Debug($"SAT: {valorSAT} - NAOFISCAL: {valorNAOFISCAL} - ECF: {valorECF}");
                    valorSomado = valorSAT + valorNAOFISCAL + valorECF;
                    log.Debug($"valorSomado: {valorSomado}");
                    totalMovdiario += valorSomado;
                    tot_vendas += valorSomado;
                    if (metodo.COD_CFE == "01")
                    {
                        sangrias = (decimal?)SomaValoresFmapagto.GetSangriasByCaixa(abertura, NO_CAIXA) ?? 0M;
                        suprimentos = (decimal?)SomaValoresFmapagto.GetSuprimentosByCaixa(abertura, NO_CAIXA) ?? 0M;
                        log.Debug($"Sangrias: {sangrias} - Suprimentos: {suprimentos}");
                        valorSomado -= sangrias;
                        valorSomado += suprimentos;

                    }
                    log.Debug($"Adicionando nova tupla: (COD_CFE: {metodo.COD_CFE}, VALOR: {valorSomado}, ID_FMANFCE: {metodo.ID_FMANFCE}, DESCRICAO: {metodo.DESCRICAO}");
                    valoresOperacionais.Add((metodo.COD_CFE, valorSomado, metodo.ID_FMANFCE, metodo.DESCRICAO));
                    totaissistema += valorSomado;
                }
            }

            rELATORIOECF.CentraECF("<b>" + "  TOTAL GAVETA  ".PadLeft(16, '>').PadRight(16, '<') + "</b>");
            decimal totaisgaveta = 0;
            decimal valorASerImpresso = 0;
            foreach (var metodo in valoresOperacionais)
            {
                log.Debug($"metodo{metodo},valoresOperacinais: {valoresOperacionais}");
                if ((metodo.COD_CFE == "03" || metodo.COD_CFE == "04") && USATEF)
                {
                    //Caso o sistema use TEF, nenhum valor de cartão será informado - o sistema pega o valor diretamente da base.
                    valorASerImpresso = metodo.VALOR;
                }
                else
                {
                    valorASerImpresso = GetValorMetodoFromOper(fecha_infor_dt[0], metodo.ID_FMANFCE);
                }
                rELATORIOECF.EntradaValor(metodo.DESCRICAO, valorASerImpresso);
                totaisgaveta += valorASerImpresso;
            }
            rELATORIOECF.EntradaValor("TOTAL", totaisgaveta);
            rELATORIOECF.PulaLinhaECF();
            rELATORIOECF.EntradaValor("TROCAS", (decimal)fecha_infor_dt[0]["TROCAS"]);
            rELATORIOECF.EntradaValor("SUPRIMENTOS", (decimal)fecha_infor_dt[0]["SUPRIMENTOS"]);
            rELATORIOECF.EntradaValor("SANGRIAS", (decimal)fecha_infor_dt[0]["SANGRIAS"]);

            rELATORIOECF.CentraECF("<b>" + "  TOTAL SISTEMA  ".PadLeft(16, '>').PadRight(15, '<') + "</b>");

            foreach (var metodo in valoresOperacionais)
            {
                log.Debug($"metodo{metodo},valoresOperacinais: {valoresOperacionais}");
                //CALCULA NÚMERO DE OPERAÇÕES
                int contador = 0;
                if (!(contagemNaoFiscal.Count == 0 || contagemNaoFiscal[0][0] is DBNull))
                    contador += (from linha in contagemNaoFiscal.AsEnumerable() where linha.RID_FMANCFE == metodo.ID_FMANFCE select linha.RCOUNT_FMANCE).FirstOrDefault();
                if (!(contagemFiscal.Count == 0 || contagemFiscal[0][0] is DBNull))
                    contador += (from linha in contagemFiscal.AsEnumerable() where linha.RID_FMANCFE == metodo.ID_FMANFCE select linha.RCOUNT_FMANCE).FirstOrDefault();

                numcupons += contador;
                rELATORIOECF.EntradaValor(metodo.DESCRICAO, metodo.VALOR, contador);
            }

            rELATORIOECF.EntradaValor("TOTAL", totaissistema);
            rELATORIOECF.PulaLinhaECF();
            rELATORIOECF.EntradaValor("TROCAS", (decimal)fecha_oper_dt[0]["TROCAS"]);
            rELATORIOECF.EntradaValor("SUPRIMENTOS", suprimentos);
            rELATORIOECF.EntradaValor("SANGRIAS", sangrias);
            rELATORIOECF.CentraECF("<b>" + "  DIVERGÊNCIA  ".PadLeft(17, '>').PadRight(16, '<') + "</b>");
            decimal totdiverg = 0;
            using (var SomaValoresFmapagto = new SomaValoresFmapagtoTableAdapter())
            {

                foreach (var metodo in valoresOperacionais)
                {
                    log.Debug($"metodo {metodo},valoresOperacinais: {valoresOperacionais}");
                    decimal valorInformado = GetValorMetodoFromOper(fecha_infor_dt[0], metodo.ID_FMANFCE);
                    if ((metodo.COD_CFE == "03" || metodo.COD_CFE == "04") && USATEF)
                    {
                        valorInformado = metodo.VALOR;
                    }
                    rELATORIOECF.EntradaValor(metodo.DESCRICAO, (valorInformado - metodo.VALOR));
                    totdiverg += valorInformado - metodo.VALOR;
                }
            }
            rELATORIOECF.EntradaValor("TOTAL", totdiverg < 0 ? totdiverg * -1 : totdiverg);
            rELATORIOECF.PulaLinhaECF();
            rELATORIOECF.EntradaValor("TROCAS", ((decimal)fecha_infor_dt[0]["TROCAS"]) - ((decimal)fecha_oper_dt[0]["TROCAS"]));
            rELATORIOECF.EntradaValor("SUPRIMENTOS", ((decimal)fecha_infor_dt[0]["SUPRIMENTOS"]) - suprimentos);
            rELATORIOECF.EntradaValor("SANGRIAS", ((decimal)fecha_infor_dt[0]["SANGRIAS"]) - sangrias);
            #region registradores
            rELATORIOECF.CentraECF("<b>" + "  REGISTRADORES  ".PadLeft(16, '>').PadRight(15, '<') + "</b>");

            #region Busca quantidade e valor total (com descontos) de cancelamentos
            #region Quantidade
            using (var FbComm = new FbCommand())
            {
                FbComm.Connection = LOCAL_FB_CONN;
                FbComm.CommandType = CommandType.Text;
                FbComm.CommandText = "SELECT COUNT(1) FROM TB_NFVENDA " +
                    $"WHERE CAST ((DT_SAIDA || ' ' || HR_SAIDA) AS TIMESTAMP) BETWEEN '{abertura:yyyy-MM-dd HH:mm:ss}' AND '{fechamento:yyyy-MM-dd HH:mm:ss}' " +
                    "AND STATUS = 'C' " +
                    $"AND (NF_SERIE = '{NO_CAIXA}' OR NF_SERIE = 'N{NO_CAIXA}' OR NF_SERIE = 'E{NO_CAIXA}')";
                if (LOCAL_FB_CONN.State != ConnectionState.Open) LOCAL_FB_CONN.Open();
                cups_cancelados = (int)FbComm.ExecuteScalar();
                if (cups_cancelados > 0)
                {
                    FbComm.CommandText = "SELECT SUM(B.TOT_NF) FROM TB_NFVENDA A JOIN TB_NFVENDA_TOT B ON A.ID_NFVENDA = B.ID_NFVENDA " +
                      $"WHERE CAST ((A.DT_SAIDA || ' ' || A.HR_SAIDA) AS TIMESTAMP) BETWEEN '{abertura:yyyy-MM-dd HH:mm:ss}' AND '{fechamento:yyyy-MM-dd HH:mm:ss}' " +
                      "AND A.STATUS = 'C' " +
                      $"AND (A.NF_SERIE = '{NO_CAIXA}' OR A.NF_SERIE = 'N{NO_CAIXA}' OR A.NF_SERIE = 'E{NO_CAIXA}')";
                    val_cancelado = (decimal)(FbComm.ExecuteScalar() ?? 0m);
                }
                else val_cancelado = 0;
            }
            #endregion Quantidade
            #region Valor Total
            #endregion Valor Total
            #endregion Busca quantidade e valor total (com descontos) de cancelamentos
            rELATORIOECF.EntradaValor("CANC. DE CUP.", val_cancelado, Convert.ToInt32(cups_cancelados));

            switch (USARECARGAS)
            {
                case true:
                    rELATORIOECF.EntradaValor("RECARGAS", val_recargas);
                    break;
            }
            rELATORIOECF.EntradaValor("TOT. VENDAS", tot_vendas, numcupons);

            if (numcupons <= 0)
            {
                med_vendas = 0;
            }
            else
            {
                med_vendas = tot_vendas / numcupons;
            }
            rELATORIOECF.EntradaValor("VAL. MÉD. CUPOM", med_vendas);
            rELATORIOECF.PulaLinhaECF();
            rELATORIOECF.TextoECF("<b> OPERADOR(A) " + operador.Split(' ')[0] + "</b>");
            rELATORIOECF.DivisorECF();
            rELATORIOECF.PulaLinhaECF();
            rELATORIOECF.TextoECF("<b>ASS.: OPERADOR(A):      ______________________________</b>");
            rELATORIOECF.PulaLinhaECF();
            rELATORIOECF.TextoECF("<b>ASS.: SUPERVISOR:      _______________________________</b>");
            if (blnFazerFechamento)
            {
                rELATORIOECF.CentraECF($"<b> Fechamento: {DateTime.Now.ToShortDateString()} - {DateTime.Now.ToLongTimeString()} </b>");
            }
            else
            {
                rELATORIOECF.CentraECF($"<b> Fechamento: {dtmFechado.ToShortDateString()} - {dtmFechado.ToLongTimeString()} </b>");
                rELATORIOECF.CentraECF($"<b> Reimpressão: {DateTime.Now.ToShortDateString()} - {DateTime.Now.ToLongTimeString()} </b>");
            }

            rELATORIOECF.DivisorECF();
            rELATORIOECF.CentraECF("Trilha Informática - Soluções e Tecnologia".ToUpper());
            rELATORIOECF.CentraECF("(11) 4304 - 7778");
            rELATORIOECF.CentraECF(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO);
            #endregion
            try
            {
                rELATORIOECF.ImprimeTextoGuardado();
                med_vendas = tot_vendas = numcupons = 0;
                if (blnFazerFechamento)
                {
                    Oper.SP_TRI_LANCACAIXA_CLIPP(NO_CAIXA.ToString("###"), "X", totalMovdiario, 1);
                }
                fecha_infor_dt.Clear();
                fecha_oper_dt.Clear();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao finalizar cupom: " + RetornarMensagemErro(ex, false));
                throw ex;
            }
            return true;
        }
        private void LinhaHorizontal()
        {
            RecebePrint(new string('-', 87), negrito, centro, 1);
        }

        private bool IMPRIME_SPOOLER(DateTime dtmFechado, FDBDataSetVenda.TB_FORMA_PAGTO_NFCEDataTable METODOS_DT, int intIdCaixa, bool blnFazerFechamento = true)
        {

            Logger log = new Logger("Imprime Spooler");
            int numcupons = 0;
            decimal SomatoriaMensal = 0;
            decimal TotalVendasAlternativo = 0;
            totaissistema = 0;
            using var LOCAL_FB_CONN = new FbConnection { ConnectionString = MontaStringDeConexao("localhost", localpath) };
            using var FMPGTO_TA = new SP_TRI_CONTAFMPGTOTableAdapter();
            using var Oper = new TRI_PDV_OPERTableAdapter();
            using var taCupomPdv = new DataSets.FDBDataSetVendaTableAdapters.TB_CUPOMTableAdapter();
            FMPGTO_TA.Connection = LOCAL_FB_CONN;
            Oper.Connection = LOCAL_FB_CONN;
            taCupomPdv.Connection = LOCAL_FB_CONN;
            int IdOperMenosUm;


            float[] tabstops = { 33f, 33f, 33f, 33f, 33f };
            esquerda.align.SetTabStops(33f, tabstops);
            direita.align.SetTabStops(33f, tabstops);
            rtl.align.SetTabStops(33f, tabstops);

            FDBDataSet.SP_TRI_CONTANFVPAGTODataTable contagemNaoFiscal = new FDBDataSet.SP_TRI_CONTANFVPAGTODataTable();
            FDBDataSet.SP_TRI_CONTANFVPAGTODataTable contagemFiscal = new FDBDataSet.SP_TRI_CONTANFVPAGTODataTable();
            FDBDataSetVenda.TRI_PDV_OPERDataTable TabelaTurnoAnterior = new FDBDataSetVenda.TRI_PDV_OPERDataTable();
            DateTime abertura = new DateTime();
            DateTime aberturaAnterior = new DateTime();
            DateTime fechamento = new DateTime();
            if (blnFazerFechamento)
            {
                fecha_oper_dt = Oper.GetByCaixaAberto(intIdCaixa);

                if (fecha_oper_dt.Count > 1) log.Debug("Mais de uma entrada em GetByCaixaAberto foi encontrada. Estado inválido.");
                abertura = Oper.GetByCaixaAberto(intIdCaixa)[0].CURRENTTIME;
                fechamento = DateTime.Now;
                IdOperMenosUm = fecha_oper_dt[0].ID_OPER;
                IdOperMenosUm -= 1;
            }
            else
            {
                Oper.FillByCaixaFech(fecha_oper_dt, intIdCaixa, dtmFechado);
                if (fecha_oper_dt.Count > 1) log.Debug("Mais de uma entrada em GetByCaixaAberto foi encontrada. Estado inválido.");
                abertura = fecha_oper_dt[0].CURRENTTIME;
                fechamento = dtmFechado;
            }

            using (var ContaFormasPagto = new SP_TRI_CONTANFVPAGTOTableAdapter())
            {
                ContaFormasPagto.Connection = LOCAL_FB_CONN;
                if (ECF_ATIVA) ContaFormasPagto.Fill(contagemFiscal, "E" + intIdCaixa.ToString(), abertura, fechamento, "I");
                if (SAT_USADO) ContaFormasPagto.Fill(contagemFiscal, intIdCaixa.ToString(), abertura, fechamento, "I");
                ContaFormasPagto.Fill(contagemNaoFiscal, "N" + intIdCaixa.ToString(), abertura, fechamento, "I");
            }

            log.Debug("Novo fechamento de caixa ====================");
            RecebePrint(nomefantasia, negrito, centro, 1);
            RecebePrint(enderecodaempresa, corpo, centro, 1);
            RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2), corpo, centro, 1);
            LinhaHorizontal();
            RecebePrint("FECHAMENTO DO CAIXA Nº " + intIdCaixa.ToString("000"), titulo, centro, 1);
            RecebePrint("Abertura: " + abertura.ToString(), corpo, centro, 1);
            if (!blnFazerFechamento)
            {
                RecebePrint("-- Reimpressão --", titulo, centro, 1);
            }
            RecebePrint(new string('>', 15), negrito, esquerda, 0);
            RecebePrint(new string('<', 15), negrito, direita, 0);
            RecebePrint("TOTAL GAVETA", negrito, centro, 1);

            decimal sangrias = 0, suprimentos = 0;
            var statuses = METODOS_DT.Select(x => new { COD_CFE = x.ID_NFCE, x.STATUS, x.DESCRICAO, x.ID_FMANFCE });
            List<(string COD_CFE, decimal VALOR, int ID_FMANFCE, string DESCRICAO)> valoresOperacionais = new List<(string, decimal, int, string)>();
            using (var SomaValoresFmapagto = new SomaValoresFmapagtoTableAdapter())
            {
                // Atributos para definir as datas corretas 
                DateTime DataAtual = DateTime.Now;
                DateTime PrimeiroDiaMes = DateTime.Today;

                DataAtual = DataAtual.AddDays(-1);

                PrimeiroDiaMes = PrimeiroDiaMes.AddDays(-DataAtual.Day);

                foreach (var metodo in statuses)
                {
                    SomaValoresFmapagto.Connection = LOCAL_FB_CONN;
                    log.Debug("Processando método de pagamento====================");
                    decimal valorSomado, valorSAT, valorNAOFISCAL, valorECF;
                    //decimal  pvalorSAT, pvalorNAOFISCAL, pvalorECF;
                    valorSAT = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, intIdCaixa.ToString(), fechamento) ?? 0M;
                    valorNAOFISCAL = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "N" + intIdCaixa.ToString(), fechamento) ?? 0M;
                    valorECF = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "E" + intIdCaixa.ToString(), fechamento) ?? 0M;
                    log.Debug($"SAT: {valorSAT} - NAOFISCAL: {valorNAOFISCAL} - ECF: {valorECF}");
                    #region Total Venda editado por vinícius  
                    //pvalorSAT = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, intIdCaixa.ToString(), fechamento) ?? 0M;
                    //pvalorNAOFISCAL = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "N" + intIdCaixa.ToString(), fechamento) ?? 0M;
                    //pvalorECF = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "E" + intIdCaixa.ToString(), fechamento) ?? 0M;
                    //valorTotalVendas = valorTotalVendas + pvalorSAT + pvalorNAOFISCAL + pvalorECF; //pora
                    #endregion
                    valorSomado = valorSAT + valorNAOFISCAL + valorECF;
                    log.Debug($"valorSomado: {valorSomado}");
                    totalMovdiario += valorSomado;
                    tot_vendas += valorSomado;
                    if (metodo.COD_CFE == "01")
                    {
                        sangrias = (decimal?)SomaValoresFmapagto.GetSangriasByCaixa(abertura, NO_CAIXA) ?? 0M;
                        suprimentos = (decimal?)SomaValoresFmapagto.GetSuprimentosByCaixa(abertura, NO_CAIXA) ?? 0M;
                        log.Debug($"Sangrias: {sangrias} - Suprimentos: {suprimentos}");
                        valorSomado -= sangrias;
                        valorSomado += suprimentos;
                        //Soma das vendas no começo do mês até o presente.
                        SomatoriaMensal = SomatoriaMensal + (decimal?)SomaValoresFmapagto.SomaDeValores(PrimeiroDiaMes, (int)metodo.ID_FMANFCE, intIdCaixa.ToString(), fechamento) ?? 0M;

                    }
                    log.Debug($"Adicionando nova tupla: (COD_CFE: {metodo.COD_CFE}, VALOR: {valorSomado}, ID_FMANFCE: {metodo.ID_FMANFCE}, DESCRICAO: {metodo.DESCRICAO}");
                    valoresOperacionais.Add((metodo.COD_CFE, valorSomado, metodo.ID_FMANFCE, metodo.DESCRICAO));
                    totaissistema += valorSomado;
                }
            }
            #region Rendimento Produto/Servico
            DataSets.FDBDataSetVenda.SP_TRI_RENDIMENTO_SOMADataTable RendimentoSoma;
            try
            {
                using (var modelos = new DataSets.FDBDataSetVendaTableAdapters.SP_TRI_RENDIMENTO_SOMATableAdapter())
                {
                    RendimentoSoma = modelos.SP_TRI_RENDIMENTO_SOMA(fechamento, abertura);

                    int a;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
                #endregion




                #region Fluxo Turno Anterior
                FbCommand FBCOMMAND = new FbCommand();
            FbConnection fbConnect = new FbConnection(MontaStringDeConexao("localhost", localpath));
            //FbDataReader Reader;
            DateTime Inicio_dia = DateTime.Today;
            string ID_operAlternativo;
            int ID_operInt;
            DateTime AberturaAlternativa;
            DateTime FechamentoAlternativo;
            DataTable ResultadoTurnosAnteriores = new DataTable();
            try
            {
                FBCOMMAND.Connection = fbConnect;

                if (fbConnect.State == ConnectionState.Closed)
                {
                    fbConnect.Open();
                }



                FBCOMMAND.CommandText = $"SELECT ID_CAIXA, CURRENTTIME,ABERTO,HASH,FECHADO,ID_OPER,ID_USER,DIN,CHEQUE,CREDITO,DEBITO,LOJA,	ALIMENTACAO,REFEICAO,	PRESENTE,COMBUSTIVEL,OUTROS,SANGRIAS,SUPRIMENTOS,TROCAS TRI_PDV_DT_UPD FROM TRI_PDV_OPER WHERE ID_CAIXA = @intIdCaixa AND FECHADO BETWEEN CAST(@Inicio_dia AS TIMESTAMP) AND CAST(@abertura AS TIMESTAMP)";
                FBCOMMAND.Parameters.AddWithValue("@intIdCaixa", intIdCaixa);//Adiciona o valor recebido pela assinatura a uma variavel no SQL
                FBCOMMAND.Parameters.AddWithValue("@Inicio_dia", Inicio_dia);//Adiciona o valor recebido pela assinatura a uma variavel no SQL
                FBCOMMAND.Parameters.AddWithValue("@abertura", abertura);
                //FBCOMMAND.ExecuteNonQuery();
                FBCOMMAND.CommandType = CommandType.Text;
                var DataReader = FBCOMMAND.ExecuteReader();



                ResultadoTurnosAnteriores.Load(DataReader);

               
                var b = ResultadoTurnosAnteriores.Rows.Count;
                 foreach (DataRow a in ResultadoTurnosAnteriores.Rows)
                //for(int i = 0; i <= b; i++)
                 {
                 

                    ID_operInt = (int)a["ID_OPER"];

                  

                    TabelaTurnoAnterior = Oper.GetByCaixa(ID_operInt);

                    AberturaAlternativa = TabelaTurnoAnterior[0].CURRENTTIME;
                    FechamentoAlternativo = TabelaTurnoAnterior[0].FECHADO;

                    decimal sangriasAlternativa = 0, suprimentosAlternativo = 0;
                    var statusesAlternativo = METODOS_DT.Select(x => new { COD_CFE = x.ID_NFCE, x.STATUS, x.DESCRICAO, x.ID_FMANFCE });
                    List<(string COD_CFE, decimal VALOR, int ID_FMANFCE, string DESCRICAO)> valoresOperacionaisAlternativos = new List<(string, decimal, int, string)>();
                    using (var SomaValoresFmapagto = new SomaValoresFmapagtoTableAdapter())
                    {
                        // Atributos para definir as datas corretas 
                        DateTime DataAtual = DateTime.Now;
                        DateTime PrimeiroDiaMes = DateTime.Today;
                        //DateTime DiaAnterior;
                        DataAtual = DataAtual.AddDays(-1);
                        // DiaAnterior = DataAtual;
                        PrimeiroDiaMes = PrimeiroDiaMes.AddDays(-DataAtual.Day);

                        foreach (var metodo in statusesAlternativo)
                        {
                            SomaValoresFmapagto.Connection = LOCAL_FB_CONN;
                            log.Debug("Processando método de pagamento====================");
                            decimal valorSomadoAlternativo, valorSATAlternativo, valorNAOFISCALAlternativo, valorECFAlternativo;
                            //decimal  pvalorSAT, pvalorNAOFISCAL, pvalorECF;
                            valorSATAlternativo = (decimal?)SomaValoresFmapagto.SomaDeValores(AberturaAlternativa, metodo.ID_FMANFCE, intIdCaixa.ToString(), FechamentoAlternativo) ?? 0M;
                            valorNAOFISCALAlternativo = (decimal?)SomaValoresFmapagto.SomaDeValores(AberturaAlternativa, metodo.ID_FMANFCE, "N" + intIdCaixa.ToString(), FechamentoAlternativo) ?? 0M;
                            valorECFAlternativo = (decimal?)SomaValoresFmapagto.SomaDeValores(AberturaAlternativa, metodo.ID_FMANFCE, "E" + intIdCaixa.ToString(), FechamentoAlternativo) ?? 0M;
                            log.Debug($"SAT: {valorSATAlternativo} - NAOFISCAL: {valorNAOFISCALAlternativo} - ECF: {valorECFAlternativo}");
                            #region Total Venda editado por vinícius  
                            //pvalorSAT = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, intIdCaixa.ToString(), fechamento) ?? 0M;
                            //pvalorNAOFISCAL = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "N" + intIdCaixa.ToString(), fechamento) ?? 0M;
                            //pvalorECF = (decimal?)SomaValoresFmapagto.SomaDeValores(abertura, metodo.ID_FMANFCE, "E" + intIdCaixa.ToString(), fechamento) ?? 0M;
                            //valorTotalVendas = valorTotalVendas + pvalorSAT + pvalorNAOFISCAL + pvalorECF; //pora
                            #endregion
                            valorSomadoAlternativo = valorSATAlternativo + valorNAOFISCALAlternativo + valorECFAlternativo;
                            log.Debug($"valorSomado: {valorSomadoAlternativo}");
                            // totalMovdiario += valorSomado;
                            TotalVendasAlternativo += valorSomadoAlternativo;
                            if (metodo.COD_CFE == "01")
                            {
                                sangriasAlternativa = (decimal?)SomaValoresFmapagto.GetSangriasByCaixa(AberturaAlternativa, NO_CAIXA) ?? 0M;
                                suprimentosAlternativo = (decimal?)SomaValoresFmapagto.GetSuprimentosByCaixa(AberturaAlternativa, NO_CAIXA) ?? 0M;
                                log.Debug($"Sangrias: {sangriasAlternativa} - Suprimentos: {suprimentosAlternativo}");
                                valorSomadoAlternativo -= sangriasAlternativa;
                                valorSomadoAlternativo += suprimentosAlternativo;
                                //Soma das vendas no começo do mês até o presente.
                                SomatoriaMensal = SomatoriaMensal + (decimal?)SomaValoresFmapagto.SomaDeValores(PrimeiroDiaMes, (int)metodo.ID_FMANFCE, intIdCaixa.ToString(), FechamentoAlternativo) ?? 0M;

                            }
                            log.Debug($"Adicionando nova tupla: (COD_CFE: {metodo.COD_CFE}, VALOR: {valorSomadoAlternativo}, ID_FMANFCE: {metodo.ID_FMANFCE}, DESCRICAO: {metodo.DESCRICAO}");
                            valoresOperacionais.Add((metodo.COD_CFE, valorSomadoAlternativo, metodo.ID_FMANFCE, metodo.DESCRICAO));
                            //totaissistema += valorSomadoAlternativo;
                        }
                    }

                    

                 }

                FBCOMMAND.Connection.Close();

            }
            catch (Exception ex)
            {
                FBCOMMAND.Connection.Close();
                MessageBox.Show("", ex.Message);

            }










            #endregion


            decimal totaisgaveta = 0;
            decimal valorASerImpresso = 0;
            foreach (var metodo in valoresOperacionais)
            {
                log.Debug($"metodo{metodo},valoresOperacinais: {valoresOperacionais}");
                if ((metodo.COD_CFE == "03" || metodo.COD_CFE == "04") && USATEF)
                {
                    //Caso o sistema use TEF, nenhum valor de cartão será informado - o sistema pega o valor diretamente da base.
                    valorASerImpresso = metodo.VALOR;
                }
                else
                {
                    valorASerImpresso = GetValorMetodoFromOper(fecha_infor_dt[0], metodo.ID_FMANFCE);
                }

                RecebePrint(metodo.DESCRICAO, corpo, esquerda, 0);
                RecebePrint("\t\t:\tR$", corpo, esquerda, 0);
                RecebePrint(String.Format("\t{0}", valorASerImpresso.ToString("0.00")), corpo, rtl, 1);
                totaisgaveta += valorASerImpresso;
            }

            RecebePrint("TOTAL\t\t:\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + totaisgaveta.ToString("0.00"), corpo, rtl, 1);
            RecebePrint(" ", mini, esquerda, 1);
            RecebePrint("TROCAS\t\t:   " + "\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + ((decimal)fecha_infor_dt[0]["TROCAS"]).ToString("0.00"), corpo, rtl, 1);
            RecebePrint("SUPRIMENTOS\t:   " + "\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + ((decimal)fecha_infor_dt[0]["SUPRIMENTOS"]).ToString("0.00"), corpo, rtl, 1);
            RecebePrint("SANGRIA\t\t:   " + "\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + ((decimal)fecha_infor_dt[0]["SANGRIAS"]).ToString("0.00"), corpo, rtl, 1);



            RecebePrint(new string('>', 15), negrito, esquerda, 0);
            RecebePrint(new string('<', 15), negrito, direita, 0);
            RecebePrint("TOTAL SISTEMA", negrito, centro, 1);



            foreach (var metodo in valoresOperacionais)
            {
                log.Debug($"metodo{metodo},valoresOperacinais: {valoresOperacionais}");
                //CALCULA NÚMERO DE OPERAÇÕES
                int contador = 0;
                if (!(contagemNaoFiscal.Count == 0 || contagemNaoFiscal[0][0] is DBNull))
                    contador += (from linha in contagemNaoFiscal.AsEnumerable() where linha.RID_FMANCFE == metodo.ID_FMANFCE select linha.RCOUNT_FMANCE).FirstOrDefault();
                if (!(contagemFiscal.Count == 0 || contagemFiscal[0][0] is DBNull))
                    contador += (from linha in contagemFiscal.AsEnumerable() where linha.RID_FMANCFE == metodo.ID_FMANFCE select linha.RCOUNT_FMANCE).FirstOrDefault();
                RecebePrint("\t\t:" + contador.ToString() + "\tR$", corpo, esquerda, 0);
                numcupons += contador;

                RecebePrint(metodo.DESCRICAO + "\t", corpo, esquerda, 0);

                RecebePrint(String.Format("\t{0}", (metodo.VALOR).ToString("0.00")), corpo, rtl, 1);

            }


            RecebePrint("TOTAL\t\t:\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + totaissistema.ToString("0.00"), corpo, rtl, 1);
            RecebePrint(" ", mini, esquerda, 1);
            RecebePrint("TROCAS\t\t:   " + "" + "\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + ((decimal)fecha_oper_dt[0]["TROCAS"]).ToString("0.00"), corpo, rtl, 1);
            RecebePrint("SUPRIMENTOS\t:   " + "" + "\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + suprimentos.ToString("0.00"), corpo, rtl, 1);
            RecebePrint("SANGRIA\t\t:   " + "" + "\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + sangrias.ToString("0.00"), corpo, rtl, 1);
            //tot_vendas -= (decimal)fecha_oper_dt[0]["SUPRIMENTOS"];
            //tot_vendas += (decimal)fecha_oper_dt[0]["SANGRIAS"];

            RecebePrint(new string('>', 15), negrito, esquerda, 0);
            RecebePrint(new string('<', 15), negrito, direita, 0);
            RecebePrint("DIVERGÊNCIA", negrito, centro, 1);
            decimal totdiverg = 0;
            using (var SomaValoresFmapagto = new SomaValoresFmapagtoTableAdapter())
            {

                foreach (var metodo in valoresOperacionais)
                {
                    log.Debug($"metodo {metodo},valoresOperacinais: {valoresOperacionais}");
                    decimal valorInformado = GetValorMetodoFromOper(fecha_infor_dt[0], metodo.ID_FMANFCE);
                    if ((metodo.COD_CFE == "03" || metodo.COD_CFE == "04") && USATEF)
                    {
                        valorInformado = metodo.VALOR;
                    }
                    RecebePrint(metodo.DESCRICAO, corpo, esquerda, 0);
                    RecebePrint("\t\t:\tR$", corpo, esquerda, 0);

                    if (valorInformado < (metodo.VALOR))
                    {
                        RecebePrint("\t" + (metodo.VALOR - valorInformado).ToString("0.00") + "-", corpo, rtl, 0);
                        RecebePrint("(NEGATIVO)", corpo, direita, 1);
                    }
                    else
                    {
                        RecebePrint("\t" + (valorInformado - metodo.VALOR).ToString("0.00"), corpo, rtl, 1);

                    }
                    totdiverg += valorInformado - metodo.VALOR;

                }
            }
            RecebePrint("TOTAL\t\t:   \tR$", corpo, esquerda, 0);
            if (totdiverg < 0)
            {
                RecebePrint("\t" + (totdiverg * -1).ToString("0.00") + "-", corpo, rtl, 0);
                RecebePrint("(NEGATIVO)", corpo, direita, 1);
            }
            else
            {
                RecebePrint("\t" + totdiverg.ToString("0.00"), corpo, rtl, 1);

            }
            RecebePrint(" ", mini, esquerda, 1);
            RecebePrint("TROCAS\t\t:   \tR$", corpo, esquerda, 0);
            if (((decimal)fecha_infor_dt[0]["TROCAS"]) < ((decimal)fecha_oper_dt[0]["TROCAS"]))
            {
                RecebePrint("\t" + (((decimal)fecha_oper_dt[0]["TROCAS"]) - ((decimal)fecha_infor_dt[0]["TROCAS"])).ToString("0.00") + "-", corpo, rtl, 0);
                RecebePrint("(NEGATIVO)", corpo, direita, 1);
            }
            else
            {
                RecebePrint("\t" + (((decimal)fecha_infor_dt[0]["TROCAS"]) - ((decimal)fecha_oper_dt[0]["TROCAS"])).ToString("0.00"), corpo, rtl, 1);
            }
            RecebePrint("SUPRIMENTOS\t:   \tR$", corpo, esquerda, 0);
            if (((decimal)fecha_infor_dt[0]["SUPRIMENTOS"]) < suprimentos)
            {
                RecebePrint("\t" + (suprimentos - ((decimal)fecha_infor_dt[0]["SUPRIMENTOS"])).ToString("0.00") + "-", corpo, rtl, 0);
                RecebePrint("(NEGATIVO)", corpo, direita, 1);
            }
            else
            {
                RecebePrint("\t" + (((decimal)fecha_infor_dt[0]["SUPRIMENTOS"]) - suprimentos).ToString("0.00"), corpo, rtl, 1);
            }
            RecebePrint("SANGRIA\t\t:   \tR$", corpo, esquerda, 0);
            if (((decimal)fecha_infor_dt[0]["SANGRIAS"]) < sangrias)
            {
                RecebePrint("\t" + (sangrias - ((decimal)fecha_infor_dt[0]["SANGRIAS"])).ToString("0.00") + "-", corpo, rtl, 0);
                RecebePrint("(NEGATIVO)", corpo, direita, 1);
            }
            else
            {
                RecebePrint("\t" + (((decimal)fecha_infor_dt[0]["SANGRIAS"]) - sangrias).ToString("0.00"), corpo, rtl, 1);
            }
            #region registradores
            RecebePrint(new string('>', 15), negrito, esquerda, 0);
            RecebePrint(new string('<', 15), negrito, direita, 0);
            RecebePrint("REGISTRADORES", negrito, centro, 1);
            #region Busca quantidade e valor total (com descontos) de cancelamentos
            #region Quantidade
            using (var FbComm = new FbCommand())
            {
                FbComm.Connection = LOCAL_FB_CONN;
                FbComm.CommandType = CommandType.Text;
                FbComm.CommandText = "SELECT COUNT(1) FROM TB_NFVENDA " +
                    $"WHERE CAST ((DT_SAIDA || ' ' || HR_SAIDA) AS TIMESTAMP) BETWEEN '{abertura:yyyy-MM-dd HH:mm:ss}' AND '{fechamento:yyyy-MM-dd HH:mm:ss}' " +
                    "AND STATUS = 'C' " +
                    $"AND (NF_SERIE = '{NO_CAIXA}' OR NF_SERIE = 'N{NO_CAIXA}' OR NF_SERIE = 'E{NO_CAIXA}')";
                if (LOCAL_FB_CONN.State != ConnectionState.Open) LOCAL_FB_CONN.Open();
                cups_cancelados = (int)FbComm.ExecuteScalar();
                if (cups_cancelados > 0)
                {
                    FbComm.CommandText = "SELECT SUM(B.TOT_NF) FROM TB_NFVENDA A JOIN TB_NFVENDA_TOT B ON A.ID_NFVENDA = B.ID_NFVENDA " +
                      $"WHERE CAST ((A.DT_SAIDA || ' ' || A.HR_SAIDA) AS TIMESTAMP) BETWEEN '{abertura:yyyy-MM-dd HH:mm:ss}' AND '{fechamento:yyyy-MM-dd HH:mm:ss}' " +
                      "AND A.STATUS = 'C' " +
                      $"AND (A.NF_SERIE = '{NO_CAIXA}' OR A.NF_SERIE = 'N{NO_CAIXA}' OR A.NF_SERIE = 'E{NO_CAIXA}')";
                    val_cancelado = (decimal)(FbComm.ExecuteScalar() ?? 0m);
                }
                else val_cancelado = 0;
            }
            #endregion Quantidade
            #region Valor Total
            #endregion Valor Total
            #endregion Busca quantidade e valor total (com descontos) de cancelamentos
            RecebePrint("CANC. DE CUP.", corpo, esquerda, 0);
            RecebePrint("\t\t" + cups_cancelados.ToString("00") + "   -\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + val_cancelado.ToString("0.00"), corpo, rtl, 1);

            switch (USARECARGAS)
            {
                case true:
                    RecebePrint("RECARGAS\t\t\tR$", corpo, esquerda, 0);
                    RecebePrint("\t" + val_recargas.ToString("0.00"), corpo, rtl, 1);
                    break;
            }

            RecebePrint("TOT. VENDAS\t\t", corpo, esquerda, 0);
            RecebePrint("\t\t" + numcupons.ToString("00") + "   -\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + tot_vendas.ToString("0.00"), corpo, rtl, 1);

            if (numcupons <= 0)
            {
                med_vendas = 0;
            }
            else
            {
                med_vendas = tot_vendas / numcupons;
            }
            RecebePrint("VAL. MÉD. CUPOM\t\tR$", corpo, esquerda, 0);
            RecebePrint("\t" + med_vendas.ToString("0.00"), corpo, rtl, 1);

            #region Registradores Vini
            RecebePrint("SOMA DO DIA\t\t\tR$", corpo, esquerda, 0);
            RecebePrint($"\t{TotalVendasAlternativo:N2}", corpo, direita, 1);


            #endregion Registradores Vini

            RecebePrint(" ", negrito, esquerda, 0);
            RecebePrint("OPERADOR(A) " + operador.Split(' ')[0], negrito, esquerda, 1);
            LinhaHorizontal();
            RecebePrint("--", mini, esquerda, 1);
            RecebePrint("ASS.: OPERADOR(A):      ______________________________", negrito, esquerda, 1);
            RecebePrint("--", mini, esquerda, 1);
            RecebePrint("ASS.: SUPERVISOR:      _______________________________", negrito, esquerda, 1);

            if (blnFazerFechamento)
            {
                RecebePrint("Fechamento: " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString(), negrito, centro, 1);
            }
            else
            {
                RecebePrint("Fechamento: " + dtmFechado.ToShortDateString() + " - " + dtmFechado.ToLongTimeString(), negrito, centro, 1);
                RecebePrint("Reimpressão: " + DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString(), negrito, centro, 1);
            }

            LinhaHorizontal();
            RecebePrint("Trilha Informática - Soluções e Tecnologia".ToUpper(), corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            #endregion
            try
            {
                PrintaSpooler();
                med_vendas = tot_vendas = numcupons = 0;
                if (blnFazerFechamento)
                {
                    Oper.SP_TRI_LANCACAIXA_CLIPP(NO_CAIXA.ToString("###"), "X", totalMovdiario, 1);
                }
                fecha_infor_dt.Clear();
                fecha_oper_dt.Clear();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                MessageBox.Show("Erro ao finalizar cupom: " + RetornarMensagemErro(ex, false));
                throw ex;
            }
            return true;
        }

        private decimal GetValorMetodoFromOper(FDBDataSetVenda.TRI_PDV_OPERRow tRI_PDV_OPERRow, int key)
        {
            decimal retorno = 0m;

            switch (key)
            {
                case 1:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["DIN"]);
                    break;
                case 2:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["CHEQUE"]);
                    break;
                case 3:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["CREDITO"]);
                    break;
                case 4:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["DEBITO"]);
                    break;
                case 5:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["LOJA"]);
                    break;
                case 6:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["ALIMENTACAO"]);
                    break;
                case 7:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["REFEICAO"]);
                    break;
                case 8:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["PRESENTE"]);
                    break;
                case 9:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["COMBUSTIVEL"]);
                    break;
                case 10:
                    retorno = Convert.ToDecimal(tRI_PDV_OPERRow["OUTROS"]);
                    break;
                default:
                    break;
                    //throw new NotImplementedException("ID do método de pagamento inválido: " + key.ToString());
            }

            return retorno;
        }

        #endregion Methods

    }
    internal class PrintDEVOLOld
    {
        public static int numerodocupom;
        public static List<Produto> produtos = new List<Produto>();

        private static void LinhaHorizontal()
        {
            RecebePrint(new string('-', 87), negrito, centro, 1);
        }

        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, decimal Xqtde, decimal Xvalorunit, decimal Xdesconto, decimal Xtribest, decimal Xtribfed)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Xdescricao.TruncateLongString(), tipounid = Xtipounid, qtde = Xqtde, valorunit = Xvalorunit, valortotal = Xqtde * (Xvalorunit - Xdesconto).RoundABNT(), desconto = Xdesconto, trib_est = Xtribest, trib_fed = Xtribfed };
            produtos.Add(prod);
        }
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            decimal subtotal = 0M;
            #region Region1
            RecebePrint(Emitente.NomeFantasia, negrito, centro, 1);
            RecebePrint(Emitente.EnderecoCompleto, corpo, centro, 1);
            RecebePrint($"CNPJ: {Emitente.CNPJ}  IE: {Emitente.IM}  IM: {Emitente.IM}", corpo, centro, 1);
            LinhaHorizontal();
            RecebePrint("CUPOM DE DEVOLUÇÃO", titulo, centro, 1);
            //RecebePrint(String.Format("CUPOM Nº {0}", NO_CAIXA.ToString() + "-" + numerodocupom), Titulo, centro, 1);
            LinhaHorizontal();
            RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  VL ITEM R$", corpo, centro, 1);
            LinhaHorizontal();
            int linha = 1;
            foreach (Produto prod in produtos)
            {
                RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda, 1);
                RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + prod.valorunit.ToString("n2"), corpo, esquerda, 0);
                RecebePrint(prod.valortotal.ToString("n2"), corpo, direita, 1);
                subtotal += prod.valortotal;
                linha += 1;
            }
            RecebePrint("VALOR DA DEVOLUÇÃO R$", titulo, esquerda, 0);
            RecebePrint(subtotal.ToString("n2"), titulo, direita, 1);
            RecebePrint(" ", corpo, esquerda, 1);
            LinhaHorizontal();
            RecebePrint(DateTime.Now.ToString(), corpo, esquerda, 1);
            RecebePrint("OBRIGADO VOLTE SEMPRE!!", corpo, esquerda, 1);
            RecebePrint("Operador: " + operador, corpo, esquerda, 1);
            LinhaHorizontal();
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            #endregion
            PrintaSpooler();
            produtos.Clear();
            linha = 1;
            return true;
        }
    }

    internal class PrintDEVOL
    {
        private static void LinhaHorizontal()
        {
            RecebePrint(new string('-', 87), negrito, centro, 1);
        }

        public static bool IMPRIME(int numeroVale, decimal valorDevolucao)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            RecebePrint(Emitente.NomeFantasia, negrito, centro, 1);
            RecebePrint(Emitente.EnderecoCompleto, corpo, centro, 1);
            RecebePrint($"CNPJ: {Emitente.CNPJ}  IE: {Emitente.IE}  IM: {Emitente.IM}", corpo, centro, 1);
            LinhaHorizontal();
            RecebePrint("VALE DE DEVOLUÇÃO", titulo, centro, 1);
            RecebePrint(String.Format("VALE Nº {0}", numeroVale), titulo, centro, 1);
            LinhaHorizontal();
            RecebePrint("VALOR DA DEVOLUÇÃO R$", titulo, esquerda, 0);
            RecebePrint(valorDevolucao.ToString("n2"), titulo, direita, 1);
            LinhaHorizontal();
            RecebePrint(DateTime.Now.ToString(), corpo, esquerda, 1);
            RecebePrint("OBRIGADO VOLTE SEMPRE!!", corpo, esquerda, 1);
            RecebePrint("Operador: " + operador, corpo, esquerda, 1);
            LinhaHorizontal();
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            #endregion
            PrintaSpooler();
            return true;
        }
    }

    internal class RelNegativ
    {

        public static string cpfcnpjconsumidor = "";
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();

        public static void RecebeProduto(string Xcodigo, string Xdescricao, decimal Xvalorunit)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Xdescricao.TruncateLongString(), valorunit = Xvalorunit };
            produtos.Add(prod);
        }
        public static bool IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region Region1
            //RecebePrint(nomefantasia, negrito, centro, true);
            //RecebePrint(nomedaempresa, corpo, centro, true);
            //RecebePrint(enderecodaempresa, corpo, centro, true);
            //RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2) + "  IE: " + ieempresa + "  IM: " + imempresa, corpo, centro, true);
            RecebePrint(new string('-', 91), negrito, centro, 1);
            RecebePrint("Relatório de itens", titulo, centro, 1);
            RecebePrint("com estoque negativo", titulo, centro, 1);
            RecebePrint(new string('-', 91), negrito, centro, 1);
            RecebePrint("COD  DESC", corpo, esquerda, 1);
            RecebePrint("VL ITEM R$", corpo, esquerda, 1);
            RecebePrint(new string('-', 91), corpo, centro, 1);

            //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
            foreach (Produto prod in produtos)
            {
                RecebePrint(prod.codigo + "\t" + prod.descricao, corpo, esquerda, 1);
                RecebePrint(prod.valorunit.ToString("n2"), corpo, esquerda, 1);
            }
            //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv

            RecebePrint("Núm. de prod. c/ est. neg.", titulo, esquerda, 0);
            RecebePrint(produtos.Count.ToString(), titulo, direita, 1);
            RecebePrint(new string('-', 91), corpo, centro, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            #endregion
            PrintaSpooler();
            pagamentos.Clear();
            produtos.Clear();
            return true;
        }
    }
    public class VendaImpressa
    {

        public static int numerodocupom;
        public static string chavenfe;
        public static int no_pedido;
        public static string vendedor;
        public static string assinaturaQRCODE;
        public static string troco;
        public static decimal valor_prazo;
        public static decimal desconto;
        public static string cliente;
        public static (string, string) observacaoFisco;
        public static DateTime vencimento;
        public static bool prazo;
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();
        public static Dictionary<string, string> ReciboTEF { get; set; }

        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, decimal Xqtde, decimal Xvalorunit, decimal Xdesconto, decimal Xtribest, decimal Xtribfed, decimal Xtribmun)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Xdescricao.TruncateLongString(), tipounid = Xtipounid, qtde = Xqtde, valorunit = Xvalorunit, valortotal = (Xqtde * Xvalorunit).RoundABNT(), desconto = Xdesconto, trib_est = Xtribest, trib_fed = Xtribfed, trib_mun = Xtribmun };
            produtos.Add(prod);
        }

        public static void RecebePagamento(string Xmetodo, decimal Xvalor)
        {
            MetodoPagamento method = new MetodoPagamento { NomeMetodo = Xmetodo, ValorDoPgto = Xvalor };
            pagamentos.Add(method);
        }

        private static void LinhaHorizontal()
        {
            /*if (IMPRESSORA_USB.Contains("78COL")) */
            RecebePrint(new string('-', 87), negrito, centro, 1);
            //else RecebePrint(new string('-', 91), negrito, centro, 1);
        }

        public static PrintDocument IMPRIME(int vias_prazo, CFe cFeDeRetorno = null)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            decimal subtotal = 0M;
            decimal total_trib_fed = 0M;
            decimal total_trib_est = 0M;
            decimal total_trib_mun = 0M;
            int linha = 1;
            if (!(cFeDeRetorno is null))
            {
                #region Cumpom de Venda
                RecebePrint(cFeDeRetorno.infCFe.emit.xFant, negrito, centro, 1);
                RecebePrint(cFeDeRetorno.infCFe.emit.xNome, corpo, centro, 1);
                string enderecodaempresa = string.Format("{0}, {1} - {2}, {3}", cFeDeRetorno.infCFe.emit.enderEmit.xLgr,
                                                                           cFeDeRetorno.infCFe.emit.enderEmit.nro,
                                                                           cFeDeRetorno.infCFe.emit.enderEmit.xBairro,
                                                                           cFeDeRetorno.infCFe.emit.enderEmit.xMun);
                RecebePrint(enderecodaempresa, corpo, centro, 1);
                string cnpjempresa = cFeDeRetorno.infCFe.emit.CNPJ;
                RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "."
                                     + cnpjempresa.ToString().Substring(2, 3) + "."
                                     + cnpjempresa.ToString().Substring(5, 3) + "/"
                                     + cnpjempresa.ToString().Substring(8, 4) + "-"
                                     + cnpjempresa.ToString().Substring(12, 2) + "  IE: "
                                     + cFeDeRetorno.infCFe.emit.IE + "  IM: "
                                     + cFeDeRetorno.infCFe.emit.IM, corpo, centro, 1);
                LinhaHorizontal();
                RecebePrint("Extrato No. " + numerodocupom, titulo, centro, 1);
                RecebePrint("CUPOM FISCAL ELETRÔNICO - SAT", titulo, centro, 1);
                int.TryParse(cFeDeRetorno.infCFe.ide.nserieSAT, out int numerosat);
                if (numerosat > 99999999) RecebePrint("### HOMOLOGAÇÃO - SEM VALOR FISCAL ###", titulo, centro, 1);
                LinhaHorizontal();
                try
                {
                    if (String.IsNullOrWhiteSpace(cFeDeRetorno.infCFe.dest.Item))
                    {
                        RecebePrint("CPF/CNPJ do Consumidor: Consumidor não Informado", corpo, esquerda, 1);
                    }
                    else
                    {
                        string id_dest;
                        if (cFeDeRetorno.infCFe.dest.Item.Length == 11)
                        {
                            id_dest = cFeDeRetorno.infCFe.dest.Item.Substring(0, 3) + "." +
                                      cFeDeRetorno.infCFe.dest.Item.Substring(3, 3) + "." +
                                      cFeDeRetorno.infCFe.dest.Item.Substring(6, 3) + "-" +
                                      cFeDeRetorno.infCFe.dest.Item.Substring(9, 2);

                            RecebePrint("CPF do Consumidor: " + id_dest, corpo, esquerda, 1);

                        }
                        else if (cFeDeRetorno.infCFe.dest.Item.Length == 14)
                        {
                            id_dest = cFeDeRetorno.infCFe.dest.Item.Substring(0, 2) + "." +
                                      cFeDeRetorno.infCFe.dest.Item.Substring(2, 3) + "." +
                                      cFeDeRetorno.infCFe.dest.Item.Substring(5, 3) + "/" +
                                      cFeDeRetorno.infCFe.dest.Item.Substring(8, 4) + "-" +
                                      cFeDeRetorno.infCFe.dest.Item.Substring(12, 2);

                            RecebePrint("CNPJ do Consumidor: " + id_dest, corpo, esquerda, 1);

                        }
                    }
                }
                catch
                {

                }
                LinhaHorizontal();
                RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  (VLTR R$)*  VL ITEM R$", corpo, centro, 1);
                LinhaHorizontal();
                //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
                foreach (Produto prod in produtos)
                {
                    RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda, 1);
                    RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + prod.valorunit.ToString("n2"), corpo, esquerda, 0);
                    RecebePrint("\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(" + (prod.valorunit * (prod.trib_est + prod.trib_fed + prod.trib_mun) / 100).ToString("n2") + ")", corpo, esquerda, 0);
                    RecebePrint(prod.valortotal.ToString("n2"), corpo, direita, 1);
                    if (prod.desconto > 0)
                    {
                        RecebePrint("(DESCONTO)", italico, esquerda, 0);
                        RecebePrint("-" + prod.desconto.ToString("n2"), italico, direita, 1);
                    }
                    total_trib_fed += prod.trib_fed * prod.valorunit * prod.qtde;
                    total_trib_est += prod.trib_est * prod.valorunit * prod.qtde;
                    total_trib_mun += prod.trib_mun * prod.valorunit * prod.qtde;
                    subtotal += prod.valortotal - prod.desconto;
                    linha += 1;
                }
                //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv
                RecebePrint(" ", corpo, esquerda, 1);
                RecebePrint("VALOR TOTAL R$", titulo, esquerda, 0);
                if (desconto != 0)
                {
                    RecebePrint(subtotal.ToString("n2"), titulo, direita, 1);
                    RecebePrint("Desconto R$", corpo, esquerda, 0);
                    RecebePrint(desconto.ToString("n2"), corpo, direita, 1);
                }
                else
                {
                    RecebePrint(subtotal.ToString("n2"), titulo, direita, 1);
                }
                foreach (MetodoPagamento met in pagamentos)
                {
                    RecebePrint(met.NomeMetodo, corpo, esquerda, 0);
                    RecebePrint(met.ValorDoPgto.ToString("n2"), corpo, direita, 1);
                }
                if (troco != "0,00")
                {
                    RecebePrint("TROCO R$", corpo, esquerda, 0);
                    RecebePrint(troco, corpo, direita, 1);
                }
                else
                {
                    RecebePrint("", corpo, esquerda, 0);
                }
                RecebePrint(" ", corpo, esquerda, 1);
                LinhaHorizontal();
                if (DETALHADESCONTO)
                {
                    bool existeDetalhamento = false;
                    foreach (Produto prod in produtos)
                    {
                        if (prod.desconto > 0)
                        {
                            RecebePrint($"Desconto no item {prod.numero}", corpo, esquerda, 0);
                            RecebePrint($"{prod.desconto:C2}", corpo, direita, 1);
                            existeDetalhamento = true;
                        }
                    }
                    if (existeDetalhamento) LinhaHorizontal();
                }

                if (!String.IsNullOrWhiteSpace(observacaoFisco.Item1))
                {
                    RecebePrint($"{observacaoFisco.Item1} - {observacaoFisco.Item2}", corpo, esquerda, 1);
                }
                RecebePrint(DateTime.Now.ToString(), corpo, esquerda, 1);
                RecebePrint(MENSAGEM_RODAPE, corpo, esquerda, 2);
                if (SYSCOMISSAO > 0 && !String.IsNullOrWhiteSpace(vendedor))
                {
                    RecebePrint("Você foi atendido por: " + vendedor, corpo, esquerda, 1);
                }
                if (no_pedido > 0) RecebePrint("Pedido nº: " + no_pedido, titulo, esquerda, 1);
                LinhaHorizontal();
                RecebePrint("* - Valor aproximado dos tributos do item", corpo, esquerda, 1);
                RecebePrint("Valor aproximado dos tributos deste cupom R$", corpo, esquerda, 0);
                RecebePrint(((total_trib_est + total_trib_fed + total_trib_mun) / 100).ToString("n2"), negrito, direita, 1);
                RecebePrint("Tributos Federais R$", corpo, esquerda, 0);
                RecebePrint((total_trib_fed / 100).ToString("n2"), negrito, direita, 1);
                RecebePrint("Tributos Estaduais R$", corpo, esquerda, 0);
                RecebePrint((total_trib_est / 100).ToString("n2"), negrito, direita, 1);
                RecebePrint("Tributos Municipais R$", corpo, esquerda, 0);
                RecebePrint((total_trib_mun / 100).ToString("n2"), negrito, direita, 1);
                RecebePrint("(conforme Lei Fed. 12.741/2012)", corpo, esquerda, 1);
                RecebePrint("Operador: " + operador, corpo, esquerda, 1);
                LinhaHorizontal();
                if (numerosat > 99999999) RecebePrint("### HOMOLOGAÇÃO - SEM VALOR FISCAL ###", titulo, centro, 1);
                RecebePrint("SAT No. " + numerosat.ToString(), titulo, centro, 1);
                string dEmi = cFeDeRetorno.infCFe.ide.dEmi;
                string hEmi = cFeDeRetorno.infCFe.ide.hEmi;
                string tsEmi = $"{dEmi.Substring(6, 2)}/{dEmi.Substring(4, 2)}/{dEmi.Substring(0, 4)} {hEmi.Substring(0, 2)}:{hEmi.Substring(2, 2)}:{hEmi.Substring(4, 2)}";
                RecebePrint(tsEmi, titulo, centro, 1);
                RecebePrint(Regex.Replace(chavenfe, " {4}", "$0,"), corpo, centro, 1);
                //----------------------------^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                RecebePrint("CODABAR>>" + chavenfe, corpo, centro, 1);
                RecebePrint("QR_CODE>>" + assinaturaQRCODE, corpo, centro, 1);
                RecebePrint("Consulte o QR Code pelo aplicativo \"De olho na nota\",", corpo, centro, 1);
                RecebePrint("disponível na Play Store e na AppStore", corpo, centro, 1);
                if (numerosat > 99999999) RecebePrint("### HOMOLOGAÇÃO - SEM VALOR FISCAL ###", titulo, centro, 1);
                LinhaHorizontal();
                RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
                RecebePrint("(11) 4304-7778", corpo, centro, 1);
                RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
                #endregion
            }

            for (int i = 0; i < vias_prazo; i++)
            {
                #region Comprovante de Venda a Prazo
                if (cFeDeRetorno is null)
                {
                    RecebePrint("COMPROVANTE DE VENDA A PRAZO", titulo, centro, 2);
                }
                RecebePrint("Cliente: " + cliente, titulo, esquerda, 3);
                if (cFeDeRetorno is null)
                {
                    RecebePrint("Cupom: " + numerodocupom, corpo, esquerda, 1);
                    RecebePrint("Venda a prazo no valor: " + valor_prazo.ToString("C2"), negrito, esquerda, 1);
                }
                RecebePrint("Vencimento: " + vencimento.ToShortDateString(), titulo, esquerda, 2);
                RecebePrint("  ", titulo, centro, 1);
                if (cFeDeRetorno is null)
                { RecebePrint("Assinatura:_____________________________", titulo, esquerda, 2); }
                RecebePrint("Terminal: " + NO_CAIXA.ToString("D3") + "\t\tOperador: " + operador, corpo, esquerda, 2);
                if (cFeDeRetorno is null)
                {
                    RecebePrint("Sr(a) Operador(a), guarde este canhoto para o", negrito, centro, 1);
                    RecebePrint("lançamento durante o fechamento do turno.", negrito, centro, 1);
                    RecebePrint(DateTime.Today.ToShortDateString(), titulo, centro, 1);
                }
                #endregion
                LinhaHorizontal();
            }

            try
            {

                return PrintaSpooler();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                Clear();
            }

        }
        public static void Clear()
        {
            pagamentos.Clear();
            produtos.Clear();
        }

    }
    public class ComprovanteTEF
    {
        public static Dictionary<string, string> ReciboTEF { get; set; }

        public static PrintDocument IMPRIME(int vias_cliente, int vias_estab, int vias_unica, int vias_redux)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);

            for (int i = 0; i < vias_cliente; i++)
            {
                #region TEF - Via Cliente
                if (ReciboTEF.ContainsKey("712-000") && ReciboTEF["712-000"] != "0")
                {
                    foreach (var item in ReciboTEF)
                    {
                        if (item.Key.Contains("713"))
                        {
                            RecebePrint(item.Value, corpo, centro, 1);
                        }
                    }
                }
                #endregion
                RecebePrint(new string('-', 91), negrito, centro, 3);
            }

            for (int i = 0; i < vias_unica; i++)
            {
                #region TEF - Via Única
                foreach (var item in ReciboTEF)
                {
                    if (item.Key.Contains("029"))
                    {
                        RecebePrint(item.Value, corpo, centro, 1);
                    }
                }
                #endregion
                RecebePrint(new string('-', 91), negrito, centro, 1);
            }

            for (int i = 0; i < vias_redux; i++)
            {
                #region TEF - Via Reduzida
                foreach (var item in ReciboTEF)
                {
                    if (item.Key.Contains("711"))
                    {
                        RecebePrint(item.Value, corpo, centro, 1);
                    }
                }
                #endregion
                RecebePrint(new string('-', 91), negrito, centro, 1);
            }

            for (int i = 0; i < vias_estab; i++)
            {
                #region TEF - Via Estabelecimento
                if (ReciboTEF.ContainsKey("714-000") && ReciboTEF["714-000"] != "0")
                {
                    foreach (var item in ReciboTEF)
                    {
                        if (item.Key.Contains("715"))
                        {
                            RecebePrint(item.Value, corpo, centro, 1);
                        }
                    }
                }
                #endregion
                RecebePrint(new string('-', 91), negrito, centro, 1);
            }


            try
            {

                return PrintaSpooler();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
    public class ComprovanteSiTEF
    {
        //public static List<string> ReciboTEF { get; set; }

        public PrintDocument IMPRIME(List<string> ReciboTEF)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            #region TEF - Via Cliente
            foreach (var item in ReciboTEF)
            {
                RecebePrint(item, corpo, centro, 1);
            }
            #endregion
            try
            {
                return PrintaSpooler();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
    public class PrintMaitrePEDIDO
    {
        public static string nomedaempresa;
        public static string cnpjempresa;
        public static string nomefantasia;
        public static string enderecodaempresa;
        public static string ieempresa;
        public static string imempresa;
        public static int no_pedido;
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();

        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, decimal Xqtde, decimal Xvalorunit, decimal Xdesconto, decimal Xtribest, decimal Xtribfed, decimal Xtribmun)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Xdescricao.TruncateLongString(), tipounid = Xtipounid, qtde = Xqtde, valorunit = Xvalorunit, valortotal = Xqtde * (Xvalorunit - Xdesconto), desconto = Xdesconto, trib_est = Xtribest, trib_fed = Xtribfed, trib_mun = Xtribmun };
            produtos.Add(prod);
        }

        public static PrintDocument IMPRIME(bool contingencia = false)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            int linha = 1;
            #region Cumpom de Venda
            RecebePrint(nomefantasia, negrito, centro, 1);
            RecebePrint(nomedaempresa, corpo, centro, 1);
            RecebePrint(enderecodaempresa, corpo, centro, 1);
            RecebePrint("CNPJ: " + cnpjempresa.ToString().Substring(0, 2) + "." + cnpjempresa.ToString().Substring(2, 3) + "." + cnpjempresa.ToString().Substring(5, 3) + "/" + cnpjempresa.ToString().Substring(8, 4) + "-" + cnpjempresa.ToString().Substring(12, 2) + "  IE: " + ieempresa + "  IM: " + imempresa, corpo, centro, 1);
            RecebePrint(new string('-', 91), negrito, centro, 1);
            RecebePrint("Pedido de Fabricação", titulo, centro, 1);
            RecebePrint("Pedido nº: " + no_pedido.ToString("D3"), titulo, centro, 1);
            RecebePrint(new string('-', 91), negrito, centro, 1);
            //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
            foreach (Produto prod in produtos)
            {
                RecebePrint("\t" + prod.descricao, titulo, esquerda, 1);
                RecebePrint("QTD: " + prod.qtde + "\t\t\t" + "CÓD: " + prod.codigo, corpo, esquerda, 1);
                //TODO: falta observações
                linha += 1;
            }
            //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv
            RecebePrint("Terminal de Venda: " + operador, corpo, esquerda, 0);
            RecebePrint("Caixa: " + NO_CAIXA.ToString("000"), corpo, direita, 1);
            RecebePrint(DateTime.Now.ToString(), titulo, centro, 1);
            RecebePrint(new string('-', 91), corpo, centro, 1);
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            #endregion


            try
            {
                return PrintaSpooler(!contingencia);
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                pagamentos.Clear();
                produtos.Clear();
            }
            /*
             De alguma forma, o compilador agora não reclama mais disso... Vai entender 🤔
            audit("PrintDocumentIMPRIME>> IMPRIME(...) NÃO deveria chegar aqui, mas o compiler reclama que nem todos os caminhos retornam um valor.");
            return null;
            */
        }

    }
    public class VendaDEMO
    {
        public static string operadorStr;
        public static int num_caixa;
        public static int numerodocupom;
        public static string chavenfe;
        public static int no_pedido;
        public static string vendedor;
        public static string assinaturaQRCODE;
        public static string troco;
        public static decimal valor_prazo;
        public static decimal desconto;
        public static string cliente;
        public static (string, string) observacaoFisco;
        public static DateTime vencimento;
        public static bool prazo;
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();

        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, decimal Xqtde, decimal Xvalorunit, decimal Xdesconto, decimal Xtribest, decimal Xtribfed, decimal Xtribmun)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Xdescricao.TruncateLongString(), tipounid = Xtipounid, qtde = Xqtde, valorunit = Xvalorunit, valortotal = (Xqtde * Xvalorunit).RoundABNT(), desconto = Xdesconto, trib_est = Xtribest, trib_fed = Xtribfed, trib_mun = Xtribmun };
            produtos.Add(prod);
        }

        public static void RecebePagamento(string Xmetodo, decimal Xvalor)
        {
            MetodoPagamento method = new MetodoPagamento { NomeMetodo = Xmetodo, ValorDoPgto = Xvalor };
            pagamentos.Add(method);
        }

        private static void LinhaHorizontal()
        {
            RecebePrint(new string('-', 87), negrito, centro, 1);
        }

        public static PrintDocument IMPRIME(int vias_prazo, CFe cFeDeRetorno = null)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            decimal subtotal = 0M;
            decimal total_trib_fed = 0M;
            decimal total_trib_est = 0M;
            decimal total_trib_mun = 0M;
            int linha = 1;
            if (!(cFeDeRetorno is null))
            {
                #region Cumpom de Venda
                RecebePrint(Emitente.NomeFantasia, negrito, centro, 1);
                RecebePrint(Emitente.EnderecoCompleto, corpo, centro, 1);
                RecebePrint($"CNPJ: {Emitente.CNPJ}  IE: {Emitente.IM}  IM: {Emitente.IM}", corpo, centro, 1);
                LinhaHorizontal();
                RecebePrint("Extrato No. " + numerodocupom, titulo, centro, 1);
                RecebePrint("CUPOM PROVISÓRIO", titulo, centro, 1);
                LinhaHorizontal();
                RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  (VLTR R$)*  VL ITEM R$", corpo, centro, 1);
                LinhaHorizontal();
                //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
                foreach (Produto prod in produtos)
                {
                    prod.numero = linha;
                    switch (MODOBAR)
                    {
                        case true:
                            RecebePrint(prod.descricao, corpo, esquerda, 1);
                            RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid, corpo, esquerda, 0);
                            RecebePrint((prod.valortotal.ToString("n2")), corpo, direita, 1);
                            break;
                        default:
                        case false:
                            RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda, 1);
                            RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + (prod.valorunit).ToString("n2"), corpo, esquerda, 0);
                            RecebePrint("\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t(" + ((prod.valorunit) * (prod.trib_est + prod.trib_fed + prod.trib_mun) / 100).ToString("n2") + ")", corpo, esquerda, 0);
                            RecebePrint((prod.valortotal.ToString("n2")), corpo, direita, 1);
                            break;
                    }
                    if (prod.desconto > 0)
                    {
                        RecebePrint("(DESCONTO)", italico, esquerda, 0);
                        RecebePrint("-" + prod.desconto.ToString("n2"), italico, direita, 1);
                    }
                    subtotal += prod.valortotal - prod.desconto;
                    total_trib_fed += prod.trib_fed * prod.valorunit * prod.qtde;
                    total_trib_est += prod.trib_est * prod.valorunit * prod.qtde;
                    total_trib_mun += prod.trib_mun * prod.valorunit * prod.qtde;
                    linha += 1;
                }
                //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv
                RecebePrint(" ", corpo, esquerda, 1);
                RecebePrint("VALOR TOTAL R$", titulo, esquerda, 0);
                if (desconto != 0)
                {
                    RecebePrint(subtotal.ToString("n2"), titulo, direita, 1);
                    RecebePrint("Desconto R$", corpo, esquerda, 0);
                    RecebePrint(desconto.ToString("n2"), corpo, direita, 1);
                }
                else
                {
                    RecebePrint(subtotal.ToString("n2"), titulo, direita, 1);
                }
                foreach (MetodoPagamento met in pagamentos)
                {
                    RecebePrint(met.NomeMetodo, corpo, esquerda, 0);
                    RecebePrint(met.ValorDoPgto.ToString("n2"), corpo, direita, 1);
                }
                if (troco != "0,00")
                {
                    RecebePrint("TROCO R$", corpo, esquerda, 0);
                    RecebePrint(troco, corpo, direita, 1);
                }
                else
                {
                    RecebePrint("", corpo, esquerda, 0);
                }
                RecebePrint(" ", corpo, esquerda, 1);
                LinhaHorizontal();
                if (DETALHADESCONTO)
                {
                    bool existeDetalhamento = false;
                    foreach (Produto prod in produtos)
                    {
                        if (prod.desconto > 0)
                        {
                            RecebePrint($"Desconto no item {prod.numero}", corpo, esquerda, 0);
                            RecebePrint($"{prod.desconto:C2}", corpo, direita, 1);
                            existeDetalhamento = true;
                        }
                    }
                    if (existeDetalhamento) LinhaHorizontal();
                }
                RecebePrint(DateTime.Now.ToString(), corpo, esquerda, 1);
                RecebePrint(MENSAGEM_RODAPE, corpo, esquerda, 2);
                if (SYSCOMISSAO > 0 && !String.IsNullOrWhiteSpace(vendedor))
                {
                    RecebePrint("Você foi atendido por: " + vendedor, corpo, esquerda, 1);
                }
                RecebePrint("Operador(a): " + operador, corpo, esquerda, 0);
                RecebePrint("Caixa: " + num_caixa.ToString("000"), corpo, direita, 1);
                LinhaHorizontal();
                RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
                RecebePrint("(11) 4304-7778", corpo, centro, 1);
                RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
                #endregion
            }

            for (int i = 0; i < vias_prazo; i++)
            {
                #region Comprovante de Venda a Prazo
                if (cFeDeRetorno is null)
                {
                    RecebePrint("COMPROVANTE DE VENDA A PRAZO", titulo, centro, 2);
                }
                RecebePrint("Cliente: " + cliente, titulo, esquerda, 3);
                if (cFeDeRetorno is null)
                {
                    RecebePrint("Cupom: " + numerodocupom, corpo, esquerda, 1);
                    RecebePrint("Venda a prazo no valor: " + valor_prazo.ToString("C2"), negrito, esquerda, 1);
                }
                RecebePrint("Vencimento: " + vencimento.ToShortDateString(), titulo, esquerda, 2);
                RecebePrint("  ", titulo, centro, 1);
                if (cFeDeRetorno is null)
                { RecebePrint("Assinatura:_____________________________", titulo, esquerda, 2); }
                RecebePrint("Terminal: " + NO_CAIXA.ToString("D3") + "\t\tOperador: " + operador, corpo, esquerda, 2);
                if (cFeDeRetorno is null)
                {
                    RecebePrint("Sr(a) Operador(a), guarde este canhoto para o", negrito, centro, 1);
                    RecebePrint("lançamento durante o fechamento do turno.", negrito, centro, 1);
                    RecebePrint(DateTime.Today.ToShortDateString(), titulo, centro, 1);
                }
                #endregion
                LinhaHorizontal();
            }

            try
            {
                return PrintaSpooler();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }
            finally
            {
                Clear();
            }
        }
        public static void Clear()
        {
            pagamentos.Clear();
            produtos.Clear();
            prazo = false;
        }
    }

    public class Remessa
    {
        public static string operadorStr;
        public static int num_caixa;
        public static int numerodocupom;
        public static string chavenfe;
        public static int no_pedido;
        public static string vendedor;
        public static string assinaturaQRCODE;
        public static string troco;
        public static decimal valor_prazo;
        public static decimal desconto;
        public static string cliente;
        public static (string, string) observacaoFisco;
        public static DateTime vencimento;
        public static bool prazo;
        public static List<Produto> produtos = new List<Produto>();
        public static List<MetodoPagamento> pagamentos = new List<MetodoPagamento>();

        public static void RecebeProduto(string Xcodigo, string Xdescricao, string Xtipounid, decimal Xqtde)
        {
            Produto prod = new Produto() { codigo = Xcodigo, descricao = Xdescricao.TruncateLongString(), tipounid = Xtipounid, qtde = Xqtde };
            produtos.Add(prod);
        }

        private static void LinhaHorizontal()
        {
            RecebePrint(new string('-', 87), negrito, centro, 1);
        }


        public static PrintDocument IMPRIME()
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);
            int linha = 1;
            {
                #region Cumpom de Venda
                RecebePrint("Documento Auxiliar de Remessa (DAR)", titulo, centro, 1);
                RecebePrint("Centro de Distriuição Trilha Informática", negrito, centro, 1);
                RecebePrint("Via do remetente", negrito, centro, 1);
                LinhaHorizontal();
                RecebePrint("Saída: ", negrito, esquerda, 0);
                RecebePrint("\t\t\t" + Emitente.NomeFantasia, corpo, esquerda, 1);
                RecebePrint(" ", corpo, esquerda, 1);
                RecebePrint("Destino: ", negrito, esquerda, 0);
                RecebePrint("\t\t\t" + "CLIENTE", corpo, esquerda, 1);
                LinhaHorizontal();
                RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  (VLTR R$)*  VL ITEM R$", corpo, centro, 1);
                LinhaHorizontal();
                //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
                foreach (Produto prod in produtos)
                {
                    prod.numero = linha;
                    RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda, 1);
                    RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + (prod.valorunit).ToString("n2"), corpo, esquerda, 1);
                    linha += 1;
                }
                //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv
                RecebePrint(" ", corpo, esquerda, 1);
                LinhaHorizontal();
                RecebePrint(DateTime.Now.ToString(), corpo, esquerda, 1);
                RecebePrint("Requisitante: ", titulo, esquerda, 2);
                LinhaHorizontal();
                LinhaHorizontal();
                RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
                RecebePrint("(11) 4304-7778", corpo, centro, 1);
                RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
                #endregion
            }

            try
            {
                PrintaSpooler();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                System.Windows.MessageBox.Show(ex.Message);
                return null;
            }


            linha = 1;

            #region Cumpom de Venda
            RecebePrint("Documento Auxiliar de Remessa (DAR)", titulo, centro, 1);
            RecebePrint("Centro de Distriuição Trilha Informática", negrito, centro, 1);
            RecebePrint("Via do destinatário", negrito, centro, 1);
            LinhaHorizontal();
            RecebePrint("Saída: ", negrito, esquerda, 0);
            RecebePrint("\t\t\t" + Emitente.NomeFantasia, corpo, esquerda, 1);
            RecebePrint(" ", corpo, esquerda, 1);
            RecebePrint("Destino: ", negrito, esquerda, 0);
            RecebePrint("\t\t\t" + "CLIENTE", corpo, esquerda, 1);
            LinhaHorizontal();
            RecebePrint("#  COD  DESC  QTD  UN  VL UN R$  (VLTR R$)*  VL ITEM R$", corpo, centro, 1);
            LinhaHorizontal();
            //-----------------------------------------^^^^^^^^^^^^^^^^^^^^^^^^
            foreach (Produto prod in produtos)
            {
                prod.numero = linha;
                RecebePrint(linha.ToString("000") + "\t" + prod.codigo + "\t" + prod.descricao, corpo, esquerda, 1);
                RecebePrint(prod.qtde + "\t\t\t\t\t" + prod.tipounid + "\t\t X " + (prod.valorunit).ToString("n2"), corpo, esquerda, 1);
                linha += 1;
            }
            //----------------------------------------------vvvvvvvvvvvvvvvvvvvvvv
            RecebePrint(" ", corpo, esquerda, 1);
            LinhaHorizontal();
            RecebePrint(DateTime.Now.ToString(), corpo, esquerda, 1);
            RecebePrint("Requisitante: ", titulo, esquerda, 2);
            LinhaHorizontal();
            LinhaHorizontal();
            RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
            RecebePrint("(11) 4304-7778", corpo, centro, 1);
            RecebePrint(Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO, corpo, centro, 1);
            #endregion
            PrintDocument retorno = new PrintDocument();
            try
            {
                retorno = PrintaSpooler();
            }
            catch (Exception ex)
            {
                logErroAntigo(RetornarMensagemErro(ex, true));
                System.Windows.MessageBox.Show(ex.Message);
                retorno = null;
            }

            finally
            {
                Clear();
            }
            return retorno;
        }
        public static void Clear()
        {
            pagamentos.Clear();
            produtos.Clear();
            prazo = false;
        }
    }

    public class PrintDEMOCANCL
    {
        private static readonly Fonte Titulo = new Fonte { tipo = new Font("Arial Narrow", 10f, FontStyle.Bold) };
        private static readonly Fonte corpo = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Regular) };
        private static readonly Fonte negrito = new Fonte { tipo = new Font("Arial Narrow", 8f, FontStyle.Bold) };
        private static readonly Fonte conts = new Fonte { tipo = new Font("Arial Narrow", 9f, FontStyle.Bold | FontStyle.Italic) };
        private static Alinhamento direita = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Far } };
        private static Alinhamento centro = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Center } };
        private static Alinhamento esquerda = new Alinhamento { align = new StringFormat { Alignment = StringAlignment.Near } };
        private static readonly PaperSize Inicial = new PaperSize("Inicio", 400, 180);
        private static readonly PaperSize Linha = new PaperSize("Inicio", 400, 5);
        private static readonly PaperSize inf = new PaperSize("Inicio", 400, 999999);
        public static bool usouTEF = false;
        public static int cupomcancelado;
        public static bool contingencia;
        public static int numerodoextrato;
        public static string cpfcnpjconsumidor = "";
        public static string operador;
        public static int numerosat;
        public static string chavenfe;
        public static string assinaturaQRCODE;
        public static decimal total;

        public static void IMPRIME(int modelo)
        {
            float[] tabstops = { 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            esquerda.align.SetTabStops(10f, tabstops);
            direita.align.SetTabStops(10f, tabstops);

            RecebePrint(Emitente.NomeFantasia, negrito, centro, 1);
            RecebePrint(Emitente.EnderecoCompleto, corpo, centro, 1);
            RecebePrint($"CNPJ: {Emitente.CNPJ}  IE: {Emitente.IM}  IM: {Emitente.IM}", corpo, centro, 1);
            RecebePrint(new string('-', 89), negrito, centro, 1);
            RecebePrint("CUPOM NO. " + numerodoextrato, Titulo, centro, 1);
            RecebePrint("CUPOM PROVISÓRIO", Titulo, centro, 1);
            RecebePrint("CANCELAMENTO", Titulo, centro, 1);
            RecebePrint(new string('-', 89), corpo, centro, 1);
            RecebePrint("DADOS DO CUPOM CANCELADO", negrito, esquerda, 1);
            RecebePrint("", negrito, esquerda, 1);
            RecebePrint("TOTAL: R$ " + total.ToString("n2"), Titulo, esquerda, 1);
            RecebePrint("", negrito, esquerda, 1);
            if (usouTEF)
            {
                RecebePrint("Exija o cancelamento da cobrança em seu cartão", negrito, centro, 1);
                RecebePrint("", negrito, esquerda, 1);
            }
            RecebePrint(System.DateTime.Now.ToString(), negrito, centro, 1);
            if (modelo == 1)
            {
                RecebePrint("COD_BARRAS>>" + chavenfe, corpo, centro, 1);
                RecebePrint(" ", negrito, esquerda, 1);
                RecebePrint("QR_CODE>>" + assinaturaQRCODE, corpo, centro, 1);
                RecebePrint("Consulte nossas ofertas disponível em nosso site", corpo, centro, 1);
                RecebePrint(new string('-', 89), corpo, centro, 1);
                RecebePrint("Trilha Informática - Soluções e Tecnologia", corpo, centro, 1);
                RecebePrint("(11) 4304-7778", corpo, centro, 1);
            }
            PrintaSpooler();
        }

    }
    public class PrintRELATORIOECF
    {
        //private bool RelatorioAberto = false;
        public bool AbrirNovoRG(string TipoRelatorio)
        {
            int resposta = 0;
            resposta = DarumaDLL.confCadastrar_ECF_Daruma("RG", TipoRelatorio, "");
            resposta = DarumaDLL.iRGAbrir_ECF_Daruma(TipoRelatorio); // Convertido diretamente do AmbisoftPDV (VB6)
            if (resposta == 1)
            {
                int erro = 0;
                erro = DarumaDLL.eRetornarErro_ECF_Daruma();
                switch (erro)
                {
                    case 0:
                        break;
                    case 78:
                        DarumaDLL.iCFCancelar_ECF_Daruma();
                        DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Havia um cupom aberto, que foi cancelado");
                        resposta = DarumaDLL.confCadastrar_ECF_Daruma("RG", TipoRelatorio, "");
                        resposta = DarumaDLL.iRGAbrir_ECF_Daruma(TipoRelatorio); // Convertido diretamente do AmbisoftPDV (VB6)
                        break;
                    case 88:
                        DarumaDLL.iCFCancelar_ECF_Daruma();
                        DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Redução Z pendente.");
                        return false;
                    case 89:
                        DarumaDLL.iCFCancelar_ECF_Daruma();
                        DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, "Redução Z já foi feita.");
                        return false;
                    default:
                        DialogBox.Show("Sangria/Suprimento", DialogBoxButtons.No, DialogBoxIcons.Info, false, $"Erro {erro}.");
                        return false;
                }
            }
            return false;
        }
        public void CentraECF(string texto)
        {
            if (texto.RemoveDarumaFormatting().Length > 48)
            {
                foreach (var item in texto.SplitNChars(48))
                {
                    if (item.Length == 48)
                    { texto_noob_pra_sair_na_ecf.Append(item + (char)13 + (char)10); }
                    else
                    {
                        int espacamento = (48 - item.Length) / 2;
                        texto_noob_pra_sair_na_ecf.Append(item.PadLeft(espacamento + item.Length) + (char)13 + (char)10);
                    }

                }
            }
            else
            {
                int espacamento = (48 - texto.Length) / 2;
                texto_noob_pra_sair_na_ecf.Append(texto.PadLeft(espacamento + texto.Length) + (char)13 + (char)10);
            }
        }
        public void TextoECF(string texto)
        {
            texto_noob_pra_sair_na_ecf.Append(texto + (char)13 + (char)10);
        }
        public void EntradaValor(string texto, decimal valor)
        {
            texto_noob_pra_sair_na_ecf.Append(texto.PadRight(27) + new string(' ', 10) + "R$" + valor.ToString("0.00").PadLeft(8) + (char)13 + (char)10);
        }
        public void EntradaValor(string texto, decimal valor, int quantidade)
        {
            texto_noob_pra_sair_na_ecf.Append(texto.PadRight(27) + new string(' ', 1) + quantidade.ToString().PadRight(9) + "R$" + valor.ToString("0.00").PadLeft(8) + (char)13 + (char)10);
        }
        public void DivisorECF()
        {
            texto_noob_pra_sair_na_ecf.Append(new string('-', 48) + (char)13 + (char)10);
        }
        public void PulaLinhaECF()
        {
            texto_noob_pra_sair_na_ecf.Append("" + (char)13 + (char)10);
        }

        public StringBuilder texto_noob_pra_sair_na_ecf = new StringBuilder();

        public void ImprimeTextoGuardado()
        {
            DarumaDLL.iRGImprimirTexto_ECF_Daruma(texto_noob_pra_sair_na_ecf.ToString());
            DarumaDLL.iRGFechar_ECF_Daruma();
            //RelatorioAberto = false;
            texto_noob_pra_sair_na_ecf.Clear();
        }
    }

}
