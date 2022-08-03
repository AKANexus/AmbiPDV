using PDV_ORCAMENTO.DataSetes;
using PDV_ORCAMENTO.DataSetes.FDBDataSetReportsTableAdapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using static PDV_WPF.staticfunc;

namespace PDV_ORCAMENTO.Telas
{
    /// <summary>
    /// Interaction logic for ReportExibicao.xaml
    /// </summary>
    public partial class ReportExibicao : Window
    {
        private string _strNomeReport;
        private List<string> _lstParametros;

        public ReportExibicao(string strNomeReport, List<string> lstParametros)
        {
            InitializeComponent();

            _strNomeReport = string.IsNullOrWhiteSpace(strNomeReport) ? string.Empty : strNomeReport;
            _lstParametros = lstParametros;
        }

        private void ReportViewer_Load(object sender, EventArgs e)
        {
            switch (_strNomeReport)
            {
                case "PDV_ORCAMENTO.Reports.rptOrcamento.rdlc":

                    #region rptOrcamento.rdlc

                    try
                    {
                        using (var taRelOrcaEmit = new SP_TRI_REL_ORCA_EMITTableAdapter())
                        using (var dtRelOrcaEmit = new FDBDataSetReports.SP_TRI_REL_ORCA_EMITDataTable())
                        using (var taRelOrcaItem = new SP_TRI_REL_ORCA_ORCAMENTO_ITEMTableAdapter())
                        using (var dtRelOrcaItem = new FDBDataSetReports.SP_TRI_REL_ORCA_ORCAMENTO_ITEMDataTable())
                        using (var taRelOrcaSoli = new SP_TRI_REL_ORCA_SOLICITANTETableAdapter())
                        using (var dtRelOrcaSoli = new FDBDataSetReports.SP_TRI_REL_ORCA_SOLICITANTEDataTable())
                        using (var taRelOrcaOrca = new SP_TRI_REL_ORCA_ORCAMENTOTableAdapter())
                        using (var dtRelOrcaOrca = new FDBDataSetReports.SP_TRI_REL_ORCA_ORCAMENTODataTable())
                        {
                            string strConn = Properties.Settings.Default.NetworkDB.ToString();
                            taRelOrcaEmit.Connection.ConnectionString = strConn;
                            taRelOrcaItem.Connection.ConnectionString = strConn;
                            taRelOrcaSoli.Connection.ConnectionString = strConn;
                            taRelOrcaOrca.Connection.ConnectionString = strConn;

                            int idOrca = Convert.ToInt32(_lstParametros[0]);
                            DateTime teste = DateTime.MinValue;
                            try
                            {
                                taRelOrcaEmit.Fill(dtRelOrcaEmit);
                                taRelOrcaItem.Fill(dtRelOrcaItem, idOrca);
                                taRelOrcaSoli.Fill(dtRelOrcaSoli, idOrca);
                                taRelOrcaOrca.Fill(dtRelOrcaOrca, idOrca);
                            }
                            catch (Exception ex)
                            {
                                string strErrMess = "Erro ao consultar dados do orçamento para impressão. \nPor favor entre em contato com a equipe de suporte.";
                                gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                                MessageBox.Show(strErrMess);
                                this.Close();
                            }

                            var dataSourceOrcaEmit = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetOrcaEmit", (DataTable)dtRelOrcaEmit);
                            var dataSourceOrcaItem = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetOrcaItem", (DataTable)dtRelOrcaItem);
                            var dataSourceOrcaSoli = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetOrcaSolicitante", (DataTable)dtRelOrcaSoli);
                            var dataSourceOrcaOrca = new Microsoft.Reporting.WinForms.ReportDataSource("DataSetOrcamento", (DataTable)dtRelOrcaOrca);

                            ReportViewer.LocalReport.EnableExternalImages = true;
                            ReportViewer.LocalReport.DataSources.Clear();
                            ReportViewer.LocalReport.DataSources.Add(dataSourceOrcaEmit);
                            ReportViewer.LocalReport.DataSources.Add(dataSourceOrcaItem);
                            ReportViewer.LocalReport.DataSources.Add(dataSourceOrcaSoli);
                            ReportViewer.LocalReport.DataSources.Add(dataSourceOrcaOrca);
                            ReportViewer.LocalReport.ReportEmbeddedResource = "PDV_ORCAMENTO.Reports.rptOrcamento.rdlc";

                            ////var eita = ConvertImageToBase64(dtRelOrcaEmit[0].LOGO, ImageFormat.Jpeg);
                            //var eita = ConvertImageToBase64(new Bitmap("eita"), ImageFormat.Jpeg);

                            ReportViewer.RefreshReport();
                        }
                    }
                    catch (Exception ex)
                    {
                        string strErrMess = "Erro ao gerar relatório de orçamento. \nPor favor entre em contato com a equipe de suporte.";
                        gravarMensagemErro(strErrMess + "\n" + RetornarMensagemErro(ex, true));
                        MessageBox.Show(strErrMess);
                        this.Close();
                    }

                    #endregion rptOrcamento.rdlc

                    break;
                default:
                    throw new NotImplementedException("Tipo de relatório não implementado: " + _strNomeReport);
            }
        }
    }
}
