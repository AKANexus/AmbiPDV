using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PDV_WPF
{
    public partial class MetodosPGTO : Form
    {
        public FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter TRI_PDV_OPERTableAdapter = new FDBDataSetTableAdapters.TRI_PDV_OPERTableAdapter();
        public MetodosPGTO()
        {
            InitializeComponent();
            this.Text = Properties.Settings.Default.NomeSoftware + " - Cadastro de Métodos de Pagamento";
        }
        bool editando = false;
        List<string> modos = new List<string> { "C", "F" };
        Dictionary<int, string> Interno_CFE = new Dictionary<int, string>
        {
                        { 0, "01" },
                        { 1, "02" },
                        { 2, "03" },
                        { 3, "04" },
                        { 4, "05" },
                        { 5, "10" },
                        { 6, "11" },
                        { 7, "12" },
                        { 8, "13" },
                        { 9, "99" }
        };
        private DataGridViewTextBoxColumn iDPAGAMENTODataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn dESCRICAODataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn dIASDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn mETODODataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn pGTOCFEDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn ATIVO;
        Dictionary<string, int> CFE_Interno = new Dictionary<string, int>
        {
                        { "01", 0 },
                        { "02", 1 },
                        { "03", 2 },
                        { "04", 3 },
                        { "05", 4 },
                        { "10", 5 },
                        { "11", 6 },
                        { "12", 7 },
                        { "13", 8 },
                        { "99", 9 }
        };
        private void MetodosPGTO_Load(object sender, EventArgs e)
        {
            this.tRI_PDV_METODOSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_METODOS);

        }
        int _cod;
        private void button2_Click(object sender, EventArgs e)
        {
            if (editando == true)
            {
                editando = false;
                but_2.Enabled = true;
                but_1.Text = "&Ativar/Desativar";
                but_2.Text = "&Editar";
                txb_Descr.Clear(); txb_Cod.Clear(); txb_Receb.Clear(); cbb_Fiscal.SelectedIndex = 0; cbb_Modo.SelectedIndex = 0;
                return;
            }
            else
            {
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                //int columnindex = dataGridView1.CurrentCell.ColumnIndex;
                int.TryParse(dataGridView1.Rows[rowindex].Cells["iDPAGAMENTODataGridViewTextBoxColumn"].Value.ToString(), out _cod);
                txb_Cod.Text = (_cod).ToString();
                txb_Descr.Text = dataGridView1.Rows[rowindex].Cells["dESCRICAODataGridViewTextBoxColumn"].Value.ToString();
                txb_Receb.Text = dataGridView1.Rows[rowindex].Cells["dIASDataGridViewTextBoxColumn"].Value.ToString();
                switch (dataGridView1.Rows[rowindex].Cells["mETODODataGridViewTextBoxColumn"].Value.ToString())
                {
                    case ("C"):
                        cbb_Modo.SelectedIndex = 0;
                        break;
                    case ("F"):
                        cbb_Modo.SelectedIndex = 1;
                        break;
                    default:
                        break;
                }
                cbb_Fiscal.SelectedIndex = CFE_Interno[dataGridView1.Rows[rowindex].Cells["pGTOCFEDataGridViewTextBoxColumn"].Value.ToString()];
                but_1.Text = "&Salvar";
                but_2.Text = "&Cancelar";
                editando = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (editando == true)
            {
                int.TryParse(txb_Receb.Text, out int _res1);
                int.TryParse(txb_Cod.Text, out int _res2);
                tRI_PDV_METODOSTableAdapter.AtualizaMetodo(txb_Descr.Text, _res1, modos[cbb_Modo.SelectedIndex], Interno_CFE[cbb_Fiscal.SelectedIndex], _res2);
                editando = false;
                but_1.Text = "&Ativar/Desativar";
                but_2.Text = "&Editar";
                txb_Descr.Clear(); txb_Cod.Clear(); txb_Receb.Clear(); cbb_Fiscal.SelectedIndex = 0; cbb_Modo.SelectedIndex = 0;
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                int firstrowindex = dataGridView1.FirstDisplayedCell.RowIndex;
                this.tRI_PDV_METODOSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_METODOS);
                dataGridView1.FirstDisplayedScrollingRowIndex = firstrowindex;
                dataGridView1.CurrentCell = dataGridView1.Rows[rowindex].Cells[0];
            }
            else
            {
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                int firstrowindex = dataGridView1.FirstDisplayedCell.RowIndex;
                int.TryParse(dataGridView1.Rows[rowindex].Cells["iDPAGAMENTODataGridViewTextBoxColumn"].Value.ToString(), out _cod);
                TRI_PDV_OPERTableAdapter.SP_TRI_TOGGLEMETODO(_cod);
                this.tRI_PDV_METODOSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_METODOS);
                dataGridView1.FirstDisplayedScrollingRowIndex = firstrowindex;
                dataGridView1.CurrentCell = dataGridView1.Rows[rowindex].Cells[0];
            }
        }

        private void but_3_Click(object sender, EventArgs e)
        {
            int rowindex = dataGridView1.CurrentCell.RowIndex;
            int firstrowindex = dataGridView1.FirstDisplayedCell.RowIndex;
            int.TryParse(dataGridView1.Rows[rowindex].Cells["iDPAGAMENTODataGridViewTextBoxColumn"].Value.ToString(), out _cod);
            TRI_PDV_OPERTableAdapter.SP_TRI_TOGGLEMETODO(_cod);
            this.tRI_PDV_METODOSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_METODOS);
            dataGridView1.FirstDisplayedScrollingRowIndex = firstrowindex;
            dataGridView1.CurrentCell = dataGridView1.Rows[rowindex].Cells[0];
        }
        #region Design
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.iDPAGAMENTODataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dESCRICAODataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dIASDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mETODODataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pGTOCFEDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ATIVO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tRIPDVMETODOSBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.fDBDataSet = new PDV_WPF.FDBDataSet();
            this.label1 = new System.Windows.Forms.Label();
            this.txb_Cod = new System.Windows.Forms.TextBox();
            this.txb_Descr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txb_Receb = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbb_Fiscal = new System.Windows.Forms.ComboBox();
            this.cbb_Modo = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.but_1 = new System.Windows.Forms.Button();
            this.but_2 = new System.Windows.Forms.Button();
            this.but_3 = new System.Windows.Forms.Button();
            this.tRI_PDV_METODOSTableAdapter = new PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tRIPDVMETODOSBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fDBDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iDPAGAMENTODataGridViewTextBoxColumn,
            this.dESCRICAODataGridViewTextBoxColumn,
            this.dIASDataGridViewTextBoxColumn,
            this.mETODODataGridViewTextBoxColumn,
            this.pGTOCFEDataGridViewTextBoxColumn,
            this.ATIVO});
            this.dataGridView1.DataSource = this.tRIPDVMETODOSBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.Location = new System.Drawing.Point(12, 176);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(841, 218);
            this.dataGridView1.TabIndex = 0;
            // 
            // iDPAGAMENTODataGridViewTextBoxColumn
            // 
            this.iDPAGAMENTODataGridViewTextBoxColumn.DataPropertyName = "ID_PAGAMENTO";
            this.iDPAGAMENTODataGridViewTextBoxColumn.FillWeight = 20F;
            this.iDPAGAMENTODataGridViewTextBoxColumn.HeaderText = "CÓD";
            this.iDPAGAMENTODataGridViewTextBoxColumn.Name = "iDPAGAMENTODataGridViewTextBoxColumn";
            this.iDPAGAMENTODataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dESCRICAODataGridViewTextBoxColumn
            // 
            this.dESCRICAODataGridViewTextBoxColumn.DataPropertyName = "DESCRICAO";
            this.dESCRICAODataGridViewTextBoxColumn.HeaderText = "MÉTODO";
            this.dESCRICAODataGridViewTextBoxColumn.Name = "dESCRICAODataGridViewTextBoxColumn";
            this.dESCRICAODataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // dIASDataGridViewTextBoxColumn
            // 
            this.dIASDataGridViewTextBoxColumn.DataPropertyName = "DIAS";
            this.dIASDataGridViewTextBoxColumn.FillWeight = 20F;
            this.dIASDataGridViewTextBoxColumn.HeaderText = "DIAS";
            this.dIASDataGridViewTextBoxColumn.Name = "dIASDataGridViewTextBoxColumn";
            this.dIASDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // mETODODataGridViewTextBoxColumn
            // 
            this.mETODODataGridViewTextBoxColumn.DataPropertyName = "METODO";
            this.mETODODataGridViewTextBoxColumn.FillWeight = 25F;
            this.mETODODataGridViewTextBoxColumn.HeaderText = "MODO";
            this.mETODODataGridViewTextBoxColumn.Name = "mETODODataGridViewTextBoxColumn";
            this.mETODODataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // pGTOCFEDataGridViewTextBoxColumn
            // 
            this.pGTOCFEDataGridViewTextBoxColumn.DataPropertyName = "PGTOCFE";
            this.pGTOCFEDataGridViewTextBoxColumn.HeaderText = "COD_SAT";
            this.pGTOCFEDataGridViewTextBoxColumn.Name = "pGTOCFEDataGridViewTextBoxColumn";
            this.pGTOCFEDataGridViewTextBoxColumn.ReadOnly = true;
            this.pGTOCFEDataGridViewTextBoxColumn.Visible = false;
            // 
            // ATIVO
            // 
            this.ATIVO.DataPropertyName = "ATIVO";
            this.ATIVO.FillWeight = 20F;
            this.ATIVO.HeaderText = "ATIVO";
            this.ATIVO.Name = "ATIVO";
            this.ATIVO.ReadOnly = true;
            // 
            // tRIPDVMETODOSBindingSource
            // 
            this.tRIPDVMETODOSBindingSource.DataMember = "TRI_PDV_METODOS";
            this.tRIPDVMETODOSBindingSource.DataSource = this.fDBDataSet;
            // 
            // fDBDataSet
            // 
            this.fDBDataSet.DataSetName = "FDBDataSet";
            this.fDBDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 25);
            this.label1.TabIndex = 1;
            this.label1.Text = "Código";
            // 
            // txb_Cod
            // 
            this.txb_Cod.Enabled = false;
            this.txb_Cod.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txb_Cod.Location = new System.Drawing.Point(96, 10);
            this.txb_Cod.Name = "txb_Cod";
            this.txb_Cod.Size = new System.Drawing.Size(100, 33);
            this.txb_Cod.TabIndex = 2;
            // 
            // txb_Descr
            // 
            this.txb_Descr.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txb_Descr.Location = new System.Drawing.Point(221, 47);
            this.txb_Descr.Name = "txb_Descr";
            this.txb_Descr.Size = new System.Drawing.Size(227, 33);
            this.txb_Descr.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(13, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(202, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Descrição do Método";
            // 
            // txb_Receb
            // 
            this.txb_Receb.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txb_Receb.Location = new System.Drawing.Point(147, 88);
            this.txb_Receb.Name = "txb_Receb";
            this.txb_Receb.Size = new System.Drawing.Size(100, 33);
            this.txb_Receb.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(13, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 25);
            this.label3.TabIndex = 5;
            this.label3.Text = "Recebimento";
            // 
            // cbb_Fiscal
            // 
            this.cbb_Fiscal.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbb_Fiscal.DropDownWidth = 250;
            this.cbb_Fiscal.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbb_Fiscal.FormattingEnabled = true;
            this.cbb_Fiscal.Items.AddRange(new object[] {
            "01 - Dinheiro",
            "02 - Cheque",
            "03 - Cartão de Crédito",
            "04 - Cartão de Débito",
            "05 - Crédito Loja",
            "10 - V. Alimentação",
            "11 - V. Refeição",
            "12 - V. Presente",
            "13 - V. Combustível",
            "99 - Outros"});
            this.cbb_Fiscal.Location = new System.Drawing.Point(386, 10);
            this.cbb_Fiscal.Name = "cbb_Fiscal";
            this.cbb_Fiscal.Size = new System.Drawing.Size(121, 33);
            this.cbb_Fiscal.TabIndex = 7;
            // 
            // cbb_Modo
            // 
            this.cbb_Modo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbb_Modo.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbb_Modo.FormattingEnabled = true;
            this.cbb_Modo.Items.AddRange(new object[] {
            "C - CORRIDOS",
            "F - FIXO"});
            this.cbb_Modo.Location = new System.Drawing.Point(253, 88);
            this.cbb_Modo.Name = "cbb_Modo";
            this.cbb_Modo.Size = new System.Drawing.Size(121, 33);
            this.cbb_Modo.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(251, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(129, 25);
            this.label4.TabIndex = 9;
            this.label4.Text = "Código Fiscal";
            // 
            // but_1
            // 
            this.but_1.AutoSize = true;
            this.but_1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.but_1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.but_1.Location = new System.Drawing.Point(64, 139);
            this.but_1.Name = "but_1";
            this.but_1.Size = new System.Drawing.Size(137, 31);
            this.but_1.TabIndex = 10;
            this.but_1.Text = "&Ativar/Desativar";
            this.but_1.UseVisualStyleBackColor = true;
            this.but_1.Click += new System.EventHandler(this.button1_Click);
            // 
            // but_2
            // 
            this.but_2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.but_2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.but_2.Location = new System.Drawing.Point(216, 139);
            this.but_2.Name = "but_2";
            this.but_2.Size = new System.Drawing.Size(80, 31);
            this.but_2.TabIndex = 11;
            this.but_2.Text = "&Editar";
            this.but_2.UseVisualStyleBackColor = true;
            this.but_2.Click += new System.EventHandler(this.button2_Click);
            // 
            // but_3
            // 
            this.but_3.AutoSize = true;
            this.but_3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.but_3.Location = new System.Drawing.Point(310, 139);
            this.but_3.Name = "but_3";
            this.but_3.Size = new System.Drawing.Size(137, 31);
            this.but_3.TabIndex = 12;
            this.but_3.Text = "Ativar/&Desativar";
            this.but_3.UseVisualStyleBackColor = true;
            this.but_3.Click += new System.EventHandler(this.but_3_Click);
            // 
            // tRI_PDV_METODOSTableAdapter
            // 
            this.tRI_PDV_METODOSTableAdapter.ClearBeforeFill = true;
            // 
            // MetodosPGTO
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 406);
            this.Controls.Add(this.but_3);
            this.Controls.Add(this.but_2);
            this.Controls.Add(this.but_1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbb_Modo);
            this.Controls.Add(this.cbb_Fiscal);
            this.Controls.Add(this.txb_Receb);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txb_Descr);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txb_Cod);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MetodosPGTO";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MetodosPGTO";
            this.Load += new System.EventHandler(this.MetodosPGTO_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tRIPDVMETODOSBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fDBDataSet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private PDV_WPF.FDBDataSet fDBDataSet;
        private System.Windows.Forms.BindingSource tRIPDVMETODOSBindingSource;
        private PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_METODOSTableAdapter tRI_PDV_METODOSTableAdapter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txb_Cod;
        private System.Windows.Forms.TextBox txb_Descr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txb_Receb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbb_Fiscal;
        private System.Windows.Forms.ComboBox cbb_Modo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button but_1;
        private System.Windows.Forms.Button but_2;
        private System.Windows.Forms.Button but_3;
        #endregion

    }
}
