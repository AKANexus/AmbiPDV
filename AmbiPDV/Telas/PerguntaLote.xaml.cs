using FirebirdSql.Data.FirebirdClient;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Lógica interna para PerguntaLote.xaml
    /// </summary>
    public partial class PerguntaLote : Window
    {
        private readonly FbConnection _connectionLocalDb;        
        private readonly int _idIdentificador;

        public string[] IdentificadoresLote { get; set; } = Array.Empty<string>();
        public decimal QuantidadeItem { get; set; }

        public PerguntaLote(int idIdentificador, string descricaoitem, decimal quantidadeItem)
        {
            InitializeComponent();
            lbl_descricao.Content = descricaoitem;
            QuantidadeItem = quantidadeItem;
            _idIdentificador = idIdentificador;
            _connectionLocalDb = new(connectionString: MontaStringDeConexao("localhost", localpath));
            txb_Lote.Focus();
        }

        private void PerguntaLote_KeyDown(object sender, KeyEventArgs e)
        {            
            if (e.Key == Key.Enter)
            {                
                using (var taLote = new DataSets.FDBDataSetVendaTableAdapters.TB_LOTETableAdapter() { Connection = _connectionLocalDb })
                using (var dtLote = new DataSets.FDBDataSetVenda.TB_LOTEDataTable())
                {
                    if (IdentificadoresLote.Any(predicate: x => x.Equals(txb_Lote.Text.Trim())))
                    {
                        DialogBox.Show(title: "Atenção", DialogBoxButtons.No, DialogBoxIcons.Warn, false, linhas: "Lote já informado para este item!");
                        return;                        
                    }

                    taLote.FillByNumLote(dataTable: dtLote, ID_IDENTIFICADOR: _idIdentificador, NUM_LOTE: txb_Lote.Text.Trim());
                    if (dtLote.Rows.Count == 1)
                    {
                        var loteSelecionado = dtLote[0];
                        if (loteSelecionado.QTD_ATUAL >= QuantidadeItem)
                        {
                            IdentificadoresLote = IdentificadoresLote.Append(txb_Lote.Text).ToArray();
                            DialogResult = true;
                            this.Close();
                        }
                        else if (loteSelecionado.QTD_ATUAL > 0)
                        {                            
                            IdentificadoresLote = IdentificadoresLote.Append(txb_Lote.Text).ToArray();
                            txb_qtdRestante.Text = (QuantidadeItem - loteSelecionado.QTD_ATUAL).ToString();
                            QuantidadeItem -= loteSelecionado.QTD_ATUAL;
                            txb_aviso.Visibility = Visibility.Visible;
                            DialogBox.Show(title: "Atenção", DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                linhas: "Lote informado não tem quantidade suficiente.\nDigite mais lotes para continuar.");
                            txb_Lote.Clear();
                            txb_Lote.Focus();
                        }
                        else
                        {
                            DialogBox.Show(title: "Atenção", DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                linhas: "A quantidade em estoque do lote passado é zero. Por favor informe o número de outro lote para continuar.");
                            txb_Lote.Focus();
                            txb_Lote.SelectAll();
                        }
                    }
                    else
                    {
                        DialogBox.Show(title: "Atenção", DialogBoxButtons.No, DialogBoxIcons.Warn, false,
                                linhas: "Nenhum lote localizado com o numero informado");
                        txb_Lote.Focus();
                        txb_Lote.SelectAll();
                    }
                }
            }
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                this.Close();
            }
        }
    }
}
