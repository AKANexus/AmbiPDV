using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Drawing;
using Zen.Barcode;
using MessagingToolkit.QRCode.Codec;
using System.Windows.Forms;
namespace PrinterNS
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
        public static void RecebePrint(string texto, Fonte font, StringFormat alignment, int breakline)
        {
            Linha line = new Linha()
            {
                linha = texto,
                fonte = font.tipo,
                alinhamento = alignment,
                quebralinha = breakline
            };
            transmitir.Add(line);
        }
        public float lastusedheight = 0f;
        public static void NewLine(object sender, PrintPageEventArgs ev)
        {
            SizeF size = new SizeF();
            float currentUsedHeight = 0f;
            foreach (Linha line in transmitir)
            {
                if (line.linha == "--")
                {
                    ev.Graphics.DrawString("00000", line.fonte, Brushes.White, new RectangleF(0, currentUsedHeight, 290, 99999999), line.alinhamento);
                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                               }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("COD_BARRAS>>"))
                {
                    Code128BarcodeDraw bdf = BarcodeDrawFactory.Code128WithChecksum;
                    ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(12, line.linha.Length - 12), 40), 0f, currentUsedHeight);
                    size.Height = 40f;
                }
                else if (!String.IsNullOrEmpty(line.linha) && line.linha.StartsWith("QR_CODE>>"))
                {
                    //CodeQrBarcodeDraw bdf = BarcodeDrawFactory.CodeQr;
                    //ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(9, line.linha.Length - 9) + PrintCUPOM.assinatura64, 3, 3), 70f, currentUsedHeight);
                    ////ev.Graphics.DrawImage(bdf.Draw(line.linha.Substring(9, line.linha.Length - 9), 4, 4), 75f, currentUsedHeight);
                    QRCodeEncoder qrCodecEncoder = new QRCodeEncoder();
                    qrCodecEncoder.QRCodeBackgroundColor = System.Drawing.Color.White;
                    qrCodecEncoder.QRCodeForegroundColor = System.Drawing.Color.Black;
                    qrCodecEncoder.CharacterSet = "UTF-8";
                    qrCodecEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                    qrCodecEncoder.QRCodeScale = 4;
                    qrCodecEncoder.QRCodeVersion = 100;
                    qrCodecEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
                    Image imageQRCode;
                    //string a ser gerada
                    String data = line.linha.Substring(9, line.linha.Length - 9);
                    imageQRCode = qrCodecEncoder.Encode(data);
                    ev.Graphics.DrawImage(imageQRCode, 75f, currentUsedHeight);
                    imageQRCode.Dispose();
                    size.Height = 150f;
                }
                else
                {
                    ev.Graphics.DrawString(line.linha, line.fonte, Brushes.Black, new RectangleF(0, currentUsedHeight, 290, 99999999), line.alinhamento);
                    size = ev.Graphics.MeasureString(line.linha, line.fonte);
                }
                if (line.quebralinha > 0)
                {
                    if (ev.Graphics.MeasureString(line.linha, line.fonte).Width > 290)
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
        public static void LimpaSpooler()
        {
            transmitir.Clear();
        }
        public static void Printa(PaperSize secao)
        {
            PrintDocument printDoc = new PrintDocument();
            //printDoc.PrinterSettings.PrinterName = "DR800";
            if (PDV_WPF.Properties.Settings.Default.ImpressoraUSB == "Nenhuma")
            { return; }
            //MessageBox.Show("PDV_WPF.Properties.Settings.Default.ImpressoraUSB: " + PDV_WPF.Properties.Settings.Default.ImpressoraUSB);
            printDoc.PrinterSettings.PrinterName = PDV_WPF.Properties.Settings.Default.ImpressoraUSB;
            //printDoc.PrinterSettings.PrinterName = "Foxit Reader PDF Printer"; 
            printDoc.PrintController = new StandardPrintController();
            printDoc.DefaultPageSettings.PaperSize = secao;
            printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            printDoc.DocumentName = "Cupom";
            if (!printDoc.PrinterSettings.IsValid)
                throw new Exception("Não foi possível localizar a impressora");
            printDoc.PrintPage += new PrintPageEventHandler(NewLine);
            try
            {
                printDoc.Print();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("Erro ao imprimir. Certifique-se de não ter selecionado um \"Impressor de PDF\", pois o sistema não oferece suporte a tais programas");
                return;
            }
            printDoc.Dispose();
            LimpaSpooler();
        }
    }
}
