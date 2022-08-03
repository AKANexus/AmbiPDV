using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;

namespace PDV_ORCAMENTO
{
    /// <summary>
    /// Lógica interna para Parametros.xaml
    /// </summary>
    public partial class Parametros : Window
    {
        FDBOrcaDataSetTableAdapters.TRI_PDV_CONFIGTableAdapter TRI_PDV_CONFIGTableAdapter = new FDBOrcaDataSetTableAdapters.TRI_PDV_CONFIGTableAdapter();
        public bool conf_inicial { get; set; }
        public bool caixaaberto { get; set; }
        public Parametros()
        {
            var macAddr = (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();
            InitializeComponent();
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                lbl_Versao.Content = String.Format("Versão do PDV: {0}", System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString());
            }
            else
            {
                lbl_Versao.Content = "Versão do PDV: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
            
            FDBDataSet.TRI_PDV_CONFIGDataTable dt = new FDBDataSet.TRI_PDV_CONFIGDataTable();
            TRI_PDV_CONFIGTableAdapter.FillByMacAdress(dt, macAddr);
            if (dt.Rows.Count != 0)
            {

                Properties.Settings.Default.no_caixa = (short)dt.Rows[0]["NO_CAIXA"];
                txb_No_Caixa.Text = ((short)dt.Rows[0]["NO_CAIXA"]).ToString("000");
                chk_Exige_Sangria.IsChecked = txb_Valor_Max.IsEnabled = Convert.ToBoolean(Convert.ToInt16((string)dt.Rows[0]["EXIGE_SANGRIA"]));
                txb_Valor_Max.Value = Convert.ToDecimal((double)dt.Rows[0]["VALOR_MAX_CAIXA"]);
                chk_Bloqueia_Limite.IsChecked = Convert.ToBoolean((Convert.ToInt16((string)dt.Rows[0]["BLOQUEIA_NO_LIMITE"])));
                txb_Cortesia.Text = (string)dt.Rows[0]["MENSAGEM_CORTESIA"];
                txb_Rodape.Text = (string)dt.Rows[0]["MENSAGEM_RODAPE"];
                txb_Valor_Folga.Value = Convert.ToDecimal((double)dt.Rows[0]["VALOR_DE_FOLGA"]);
                chk_Permite_Folga.IsChecked = txb_Valor_Folga.IsEnabled = Convert.ToBoolean((Convert.ToInt16((string)dt.Rows[0]["PERMITE_FOLGA_SANGRIA"])));
                chk_Interrompe_Nao_Encontrado.IsChecked = Convert.ToBoolean((Convert.ToInt16((string)dt.Rows[0]["INTERROMPE_NAO_ENCONTRADO"])));
                txb_ICMS.PercentValue = Convert.ToDouble((float)dt.Rows[0]["ICMS_CONT"] * 100);
                txb_CSOSN.PercentValue = Convert.ToDouble((float)dt.Rows[0]["CSOSN_CONT"] * 100);
                cbb_Pede_CPF.SelectedIndex = (int)dt.Rows[0]["PEDE_CPF"];
                cbb_Mod_CUP.SelectedIndex = Convert.ToInt32((short)dt.Rows[0]["MODELO_CUPOM"]-1);
                switch (dt.Rows[0]["PERMITE_ESTOQUE_NEGATIVO"])
                {
                    case -1:
                        chk_Permite_Venda_Negativa.IsChecked = null;
                        break;
                    case 0:
                        chk_Permite_Venda_Negativa.IsChecked = false;
                        break;
                    case 1:
                        chk_Permite_Venda_Negativa.IsChecked = true;
                        break;
                }
            }
            else
            {
                chk_Bloqueia_Limite.IsChecked = true;
                chk_Exige_Sangria.IsChecked = true;
                chk_Permite_Folga.IsChecked = true;
                cbb_Pede_CPF.SelectedIndex = 0;
                cbb_Mod_CUP.SelectedIndex = 1;
            }
            dt.Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ACI aCI = new ACI();
            aCI.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SenhaTecnico senhaTecnico = new SenhaTecnico();
            senhaTecnico.ShowDialog();
            if (senhaTecnico.DialogResult == true)
            {
                ParamsTecs paramsTecs = new ParamsTecs(false);
                paramsTecs.ShowDialog();
            }
            else { }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (!caixaaberto)
            {
                MetodosPGT metodos = new MetodosPGT();
                metodos.ShowDialog();
            }
        }

        private void chk_Bloqueia_Limite_Checked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Folga.IsEnabled = true;
        }

        private void chk_Bloqueia_Limite_Unchecked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Folga.IsEnabled = false;
            txb_Valor_Folga.Value = 0;
        }

        private void chk_Exige_Sangria_Checked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Max.IsEnabled = true;

        }

        private void chk_Exige_Sangria_Unchecked(object sender, RoutedEventArgs e)
        {
            txb_Valor_Max.IsEnabled = false;
            txb_Valor_Max.Value = 0;
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
            DialogBox db = new DialogBox("Configurações do sistema", "Deseja salvar e aplicar as alterações feitas?", DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.Warn);
            db.ShowDialog();
            switch (db.DialogResult)
            {
                case true:
                    string exige_sangria = null;
                    string bloqueianolimite = null;
                    string permitefolga = null;
                    string interrompe = null;
                    int negativa = 0;
                    var macAddr = (
                        from nic in NetworkInterface.GetAllNetworkInterfaces()
                        where nic.OperationalStatus == OperationalStatus.Up
                        select nic.GetPhysicalAddress().ToString()
                        ).FirstOrDefault();
                    short.TryParse(txb_No_Caixa.Text, out short no_caixa);
                    switch (chk_Permite_Venda_Negativa.IsChecked)
                    {
                        case true:
                            negativa = 1;
                            break;
                        case false:
                            negativa = 0;
                            break;
                        case null:
                            negativa = -1;
                            break;
                    }
                    switch (chk_Exige_Sangria.IsChecked)
                    {
                        case true:
                            exige_sangria = "1";
                            break;
                        case false:
                            exige_sangria = "0";
                            break;
                    }
                    switch (chk_Bloqueia_Limite.IsChecked)
                    {
                        case true:
                            bloqueianolimite = "1";
                            break;
                        case false:
                            bloqueianolimite = "0";
                            break;
                    }
                    switch (chk_Permite_Folga.IsChecked)
                    {
                        case true:
                            permitefolga = "1";

                            break;
                        case false:
                            permitefolga = "0";

                            break;
                    }
                    switch (chk_Interrompe_Nao_Encontrado.IsChecked)
                    {
                        case true:
                            interrompe = "1";
                            break;
                        case false:
                            interrompe = "0";
                            break;
                    }
                    if ((int)TRI_PDV_CONFIGTableAdapter.ChecaPorNoCaixa(no_caixa, macAddr) > 0)
                    {
                        DialogBox db2 = new DialogBox("Falha na configuração", "O número de caixa informado já está designado a outro terminal. Por favor altere e tente novamente", DialogBox.DialogBoxButtons.No, DialogBox.DialogBoxIcons.Warn);
                        db2.ShowDialog();
                        return;
                    }
                    TRI_PDV_CONFIGTableAdapter.AtualizaConfig(macAddr, no_caixa, exige_sangria, (double)txb_Valor_Max.Value, bloqueianolimite, (double)txb_Valor_Folga.Value, permitefolga, interrompe, txb_Cortesia.Text, (float)txb_ICMS.PercentValue / 100, (float)txb_CSOSN.PercentValue / 100, (short)cbb_Pede_CPF.SelectedIndex, negativa, Convert.ToInt16(cbb_Mod_CUP.SelectedIndex+1), txb_Rodape.Text);
                    TRI_PDV_CONFIGTableAdapter.AtualizaIFS(no_caixa, no_caixa.ToString());
                    Properties.Settings.Default.cortesia = txb_Cortesia.Text;
                    Properties.Settings.Default.rodape = txb_Rodape.Text;
                    Properties.Settings.Default.no_caixa = (string.IsNullOrWhiteSpace(txb_No_Caixa.Text) ? 1 : Convert.ToInt32(txb_No_Caixa.Text)); // não adianta //HACK: verificar uma alternativa
                    Properties.Settings.Default.Save();
                    Properties.Settings.Default.Reload();
                    DialogBox db1 = new DialogBox("Configurações do sistema", "O sistema deverá ser reiniciado para que as alterações tenham efeito.", DialogBox.DialogBoxButtons.Yes, DialogBox.DialogBoxIcons.Info);
                    db1.ShowDialog();
                    Close();
                    break;
                case false:
                    //TODO Nada
                    break;
                default:
                    //TODO Nada
                    break;
            }
            }

        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TRI_PDV_CONFIGTableAdapter.Dispose();
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (caixaaberto) { but_Metodos.IsEnabled = false; }
            if (!conf_inicial) { return; }
            using (FDBOrcaDataSetTableAdapters.TRI_PDV_CONFIGTableAdapter CONFIG_TA = new FDBOrcaDataSetTableAdapters.TRI_PDV_CONFIGTableAdapter())
            using (FDBDataSet.TRI_PDV_CONFIGDataTable CONFIG_DT = new FDBDataSet.TRI_PDV_CONFIGDataTable())
            {
                CONFIG_TA.CaixaExistente(CONFIG_DT);
                if (CONFIG_DT.Rows.Count > 0)
                {
                    DialogBox db = new DialogBox("Configuração inicial", "Deseja copiar as configurações do caixa já configurado?", DialogBox.DialogBoxButtons.YesNo, DialogBox.DialogBoxIcons.Info);
                    db.ShowDialog();
                    switch (db.DialogResult)
                    {
                        case true:
                            Properties.Settings.Default.no_caixa = (short)CONFIG_DT.Rows[0]["NO_CAIXA"]+1;
                            txb_No_Caixa.Text = ((short)CONFIG_DT.Rows[0]["NO_CAIXA"]+1).ToString("000");
                            chk_Exige_Sangria.IsChecked = txb_Valor_Max.IsEnabled = Convert.ToBoolean(Convert.ToInt16((string)CONFIG_DT.Rows[0]["EXIGE_SANGRIA"]));
                            txb_Valor_Max.Value = Convert.ToDecimal((double)CONFIG_DT.Rows[0]["VALOR_MAX_CAIXA"]);
                            chk_Bloqueia_Limite.IsChecked = Convert.ToBoolean((Convert.ToInt16((string)CONFIG_DT.Rows[0]["BLOQUEIA_NO_LIMITE"])));
                            txb_Cortesia.Text = (string)CONFIG_DT.Rows[0]["MENSAGEM_CORTESIA"];
                            txb_Rodape.Text = (string)CONFIG_DT.Rows[0]["MENSAGEM_RODAPE"];
                            txb_Valor_Folga.Value = Convert.ToDecimal((double)CONFIG_DT.Rows[0]["VALOR_DE_FOLGA"]);
                            chk_Permite_Folga.IsChecked = txb_Valor_Folga.IsEnabled = Convert.ToBoolean((Convert.ToInt16((string)CONFIG_DT.Rows[0]["PERMITE_FOLGA_SANGRIA"])));
                            chk_Interrompe_Nao_Encontrado.IsChecked = Convert.ToBoolean((Convert.ToInt16((string)CONFIG_DT.Rows[0]["INTERROMPE_NAO_ENCONTRADO"])));
                            txb_ICMS.PercentValue = Convert.ToDouble((float)CONFIG_DT.Rows[0]["ICMS_CONT"] * 100);
                            txb_CSOSN.PercentValue = Convert.ToDouble((float)CONFIG_DT.Rows[0]["CSOSN_CONT"] * 100);
                            cbb_Pede_CPF.SelectedIndex = (int)CONFIG_DT.Rows[0]["PEDE_CPF"];
                            cbb_Mod_CUP.SelectedIndex = Convert.ToInt32((short)CONFIG_DT.Rows[0]["MODELO_CUPOM"] - 1);
                            switch (CONFIG_DT.Rows[0]["PERMITE_ESTOQUE_NEGATIVO"])
                            {
                                case -1:
                                    chk_Permite_Venda_Negativa.IsChecked = null;
                                    break;
                                case 0:
                                    chk_Permite_Venda_Negativa.IsChecked = false;
                                    break;
                                case 1:
                                    chk_Permite_Venda_Negativa.IsChecked = true;
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
