using CfeRecepcao_0007;
using MySql.Data.MySqlClient;
using PDV_WPF.DataSets.FDBDataSetVendaTableAdapters;
using PDV_WPF.Objetos;
using PDV_WPF.Properties;
using System;
using System.Data;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;
using static PDV_WPF.Configuracoes.ConfiguracoesPDV;


namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para PerguntaWhats.xaml
    /// </summary>
    public partial class PerguntaNumWhats : Window
    {
        string chavecfe;
        private Venda _atual;
        decimal total = 0;
        string MetodoPagto;
        StringBuilder number = new StringBuilder();
        public PerguntaNumWhats(string ChaveCFEWhats, Venda a)
        {

            InitializeComponent();
            chavecfe = ChaveCFEWhats;

            _atual = a;
            WaterMark_lbl.Content = "Digite dessa forma, exemplo: 1199999-9999";

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (MessageBox.Show("Tem certeza que deseja finalizar?", "Atenção!!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        using (var whats = new TRI_PDV_WHATSTableAdapter())
                        {
                            whats.Connection.ConnectionString = MontaStringDeConexao(SERVERNAME, SERVERCATALOG);
                            envCFeCFeInfCFeDetProd b = new envCFeCFeInfCFeDetProd();
                            StringBuilder sub = new StringBuilder();
                            //envCFeCFeInfCFePgtoMP items;

                            var abc = _atual.RetornaCFe().infCFe.pgto;

                            switch (abc.MP[0].cMP)
                            {
                                case "01":
                                    MetodoPagto = "Dinheiro";
                                    break;
                                case "02":
                                    MetodoPagto = "Cheque";
                                    break;
                                case "03":
                                    MetodoPagto = "Cartão Crédito";
                                    break;
                                case "04":
                                    MetodoPagto = "Cartão Débito";
                                    break;
                                case "05":
                                    MetodoPagto = "Crédito Loja";
                                    break;
                                case "10":
                                    MetodoPagto = "Vale Aliment.";
                                    break;
                                case "11":
                                    MetodoPagto = "Vale Refeição";
                                    break;
                                case "12":
                                    MetodoPagto = "Vale Presente";
                                    break;
                                case "13":
                                    MetodoPagto = "Vale Combustível";
                                    break;
                                case "99":
                                    MetodoPagto = "Outros";
                                    break;
                                default:
                                    break;
                            }

                            if (chavecfe.Equals("NF"))
                            {


                                #region Em caso de  venda fiscal
                                sub.Append("*" + Emitente.NomeFantasia.Trim() + "*" + "\n");
                                sub.Append(Emitente.EnderecoCompleto + "\n");
                                sub.Append("CNPJ : " + Emitente.CNPJ + "" + "\n");
                                sub.Append("---------------------------------------\n");
                                sub.Append("```      Extrato No." + VendaDEMO.numerodocupom + "\n");
                                sub.Append("      CUPOM PROVISÓRIO```\n");
                                sub.Append("---------------------------------------\n");
                                sub.Append("```#COD    DESC\n  UN QTD VL UN R$ (VLTR R$)* VL ITEM R$```\n\n");
                                sub.Append("```");
                                foreach (envCFeCFeInfCFeDet item in _atual.RetornaCFe().infCFe.det)
                                {

                                    sub = sub.Append($"{item.prod.cProd + "   "}  " + $" {item.prod.xProd + "   "}\n" + $"{item.prod.uCom + "   "}" + $"{item.prod.qCom.Substring(0, VendaDEMO.troco.Length - 0) + "   "}" + $"{item.prod.vUnCom.Substring(0, VendaDEMO.troco.Length + 1) + "   "}" + /*+ $"{item.prod.vDesc.Substring(0, VendaDEMO.troco.Length - 0) + ""}*/"\n");
                                    //string.Concat(a,"Prod" + $"{item.prod.vDesc}" + $"{item.prod.vItem}"+$"{item.prod.xProd}");
                                    // sub.Append("```");
                                    total += Convert.ToDecimal(item.prod.vUnCom);
                                }


                                sub.Append("\nDescontos R$ :      " + VendaDEMO.desconto.ToString("F") + "\n");
                                sub.Append("Troco R$ :          " + VendaDEMO.troco + "\n");
                                total = total - VendaDEMO.desconto;
                                sub.Append("Total R$ :          " + total.ToString("F") + "\n\n");
                                sub.Append("Pagamento : " + MetodoPagto + "```\n\n");
                                sub.Append("    " + DateTime.Now.ToString() + "\n");
                                sub.Append("Operador(a) : " + operador + "     \n" + "Caixa:" + VendaDEMO.num_caixa + "\n");
                                sub.Append("\n*Em caso de dúvidas, entre em contato com o estabelecimento da compra.*");
                                sub.Append("\nTel : " + Emitente.DDD_Comer + Emitente.Tel_Comer + "\n\n");

                                sub.Append("_" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO + "_");
                                #endregion
                            }
                            else
                            {
                                CFe cFeDeRetorno = new CFe();

                                sub.Append("*" + PrintMaitrePEDIDO.nomedaempresa/*_atual.RetornaCFe().infCFe.emit.xNome.ToString()*/ + "*\n");
                                sub.Append(PrintMaitrePEDIDO.enderecodaempresa/*_atual.RetornaCFe().infCFe.emit.enderEmit.xLgr + "," + _atual.RetornaCFe().infCFe.emit.enderEmit.nro + "-" + _atual.RetornaCFe().infCFe.emit.enderEmit.xBairro + "," + _atual.RetornaCFe().infCFe.emit.enderEmit.xMun */+ "\n");
                                sub.Append("CNPJ : " + PrintMaitrePEDIDO.cnpjempresa/*_atual.RetornaCFe().infCFe.emit.CNPJ*/ + "\n");
                                sub.Append("```      Extrato No." + VendaImpressa.numerodocupom /*_atual.RetornaCFe().infCFe.ide.nCFe*/ + "\n");
                                sub.Append("      CUPOM PROVISÓRIO```\n");
                                sub.Append("_# COD DESC QTD UN VL UN R$ (VLTR R$)* VL ITEM R$_ \n\n");
                                sub.Append("```");
                                foreach (envCFeCFeInfCFeDet item in _atual.RetornaCFe().infCFe.det)
                                {
                                    sub = sub.Append($"{item.prod.cProd + "   "}  " + $" {item.prod.xProd + "   "}\n" + $"{item.prod.uCom + "   "}" + $"{item.prod.qCom.Substring(0, VendaImpressa.troco.Length - 0) + "   "}" + $"{item.prod.vUnCom.Substring(0, VendaImpressa.troco.Length + 1) + "   "}" + $"{item.prod.vDesc.Substring(0, VendaImpressa.troco.Length - 0) + ""}\n");
                                    //string.Concat(a,"Prod" + $"{item.prod.vDesc}" + $"{item.prod.vItem}"+$"{item.prod.xProd}");

                                    total += Convert.ToDecimal(item.prod.vUnCom);
                                }

                                sub.Append("\nDescontos R$ :      " + VendaImpressa.desconto.ToString("F") /*_atual.DescontoAplicado()*/+ "\n");
                                sub.Append("Troco R$ :          " + VendaImpressa.troco/*_atual.RetornaCFe().infCFe.pgto.vTroco */+ "\n");
                                total = total - VendaImpressa.desconto;
                                sub.Append("Total R$ :          " + total.ToString("F") /*_atual.RetornaCFe().infCFe.total.vCFe*/+ "\n\n");
                                sub.Append("Pagamento : " + MetodoPagto + "```\n\n");
                                sub.Append("    " + DateTime.Now.ToString() + "\n\n");
                                //sub.Append("Operador(a) : " + operador + "     \n" + "Caixa:" + VendaImpressa.n + "\n");
                                sub.Append("Chave CFE : " + chavecfe + "\n\n");
                                sub.Append("_Lembre-se, você pode validar sua chave CFE no site abaixo._\n\n");
                                sub.Append("https://satsp.fazenda.sp.gov.br/COMSAT/Public/ConsultaPublica/ConsultaPublicaCfe.aspx" + "\n\n");
                                sub.Append("\n*Em caso de dúvidas, entre em contato com o estabelecimento da compra.*");
                                sub.Append("\nTel : " + Emitente.DDD_Comer + Emitente.Tel_Comer + "\n\n");
                                sub.Append("_" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + strings.VERSAO_ADENDO + "_");

                            }

                            DateTime dt = DateTime.Now;
                            number.Append("55");
                            string whatsTxt = Whats_txb.Text;
                            number.Append(whatsTxt);
                            using (var WHATSConn = new MySqlConnection("server=turkeyshit.mysql.dbaas.com.br;user id=turkeyshit;password=Pfes2018;persistsecurityinfo=True;database=turkeyshit;CharSet=utf8;"))
                            using (var MYSQLWHATS = new MySqlCommand())
                            {
                                MYSQLWHATS.Connection = WHATSConn;
                                MYSQLWHATS.CommandType = CommandType.Text;
                                MYSQLWHATS.Parameters.AddWithValue("@NUMERO", number);
                                MYSQLWHATS.Parameters.AddWithValue("@MENSAGEM", sub);
                                MYSQLWHATS.Parameters.AddWithValue("@ENVIADA", "Espera");
                                MYSQLWHATS.Parameters.AddWithValue("@CNPJ", Emitente.CNPJ.ToString());
                                MYSQLWHATS.CommandText = "INSERT INTO TRI_PDV_WHATS (NUMERO,MENSAGEM,ENVIADA,CNPJ) VALUES (@NUMERO,@MENSAGEM,@ENVIADA,@CNPJ);";
                                try
                                {
                                    WHATSConn.Open();
                                    MYSQLWHATS.ExecuteNonQuery();
                                    WHATSConn.Close();
                                }
                                catch (MySqlException ex)
                                {
                                    WHATSConn.Close();
                                    logErroAntigo($"ERRODEINSERTWHATS>> {ex.Message}");
                                    MessageBox.Show("Houve um erro ao registrar", "Atenção");
                                }

                            }

                            // whats.SP_TRI_WHATSINSERE(number.ToString(), sub.ToString(), dt, "Espera", Emitente.CNPJ.ToString());


                        }
                        DialogResult = true;
                        this.Close();
                    }
                }
                catch (Exception)

                {
                    DialogResult = null;
                    Close();
                }
            }
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();

            }
        }
    }
}
