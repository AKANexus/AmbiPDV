using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace PDV
{
    public partial class Usuarios : Form
    {
        public Usuarios()
        {
            InitializeComponent();
            this.Text = this.Text = Properties.Settings.Default.NomeSoftware + " - Cadastro de Usuários";
        }
        Control1 novo = new Control1();
        Control1 altera = new Control1();
        static MD5 md5Hash = MD5.Create();
        private void Usuarios_Load(object sender, EventArgs e)
        {
            this.tRI_PDV_USERSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_USERS);
        }
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                e.Value = "********";
            }
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[4].Value.ToString() == "NAO")
                {
                    row.DefaultCellStyle.ForeColor = SystemColors.ControlDark;
                }
            }
        }
        private void cadastrarNovoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel1.Controls.Add(novo);
            novo.atualiza();
        }
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
        }
        private void Usuarios_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && panel1.Controls.Contains(novo))
            {
                novo.pegainfo();
                tRI_PDV_USERSTableAdapter.NovoUsuario(novo.novo_username, novo.novo_hash, novo.novo_gere);
                panel1.Controls.Remove(novo);
                this.tRI_PDV_USERSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_USERS);
                dataGridView1.Update();
                dataGridView1.Refresh();
            }
            else if (e.KeyCode == Keys.Enter && panel1.Controls.Contains(altera))
            {
                if (altera.pegainfo() == true)
                {
                    if (altera.novo_username == "ADMIN" && altera.novo_gere != "SIM")
                    {
                        MessageBox.Show("Privilégios de Gerência sempre serão dados à conta \"ADMIN\"");
                        altera.novo_gere = "SIM";
                    }
                    tRI_PDV_USERSTableAdapter.AlteraSenha(altera.novo_hash, altera.novo_gere, altera.antigo_username);
                    panel1.Controls.Remove(altera);
                    this.tRI_PDV_USERSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_USERS);
                    dataGridView1.Update();
                    dataGridView1.Refresh();
                }
            }
            else if (e.KeyCode == Keys.Escape && (panel1.Controls.Contains(altera) || panel1.Controls.Contains(novo)))
            {
                panel1.Controls.Clear();
            }
        }
        private void removerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Deseja realmente desativar esse usuário?\nEssa ação não pode ser desfeita.", "Desativar usuário", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                    DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                    if (Convert.ToString(selectedRow.Cells[1].Value) == "ADMIN")
                    {
                        MessageBox.Show("Impossível remover administrador do sistema.");
                        return;
                    }
                    if (Convert.ToString(selectedRow.Cells["aTIVODataGridViewTextBoxColumn"].Value) == "NAO")
                    {
                        MessageBox.Show("Usuário já desativado.");
                        return;
                    }
                    tRI_PDV_USERSTableAdapter.CancelaUsuario(Convert.ToString(selectedRow.Cells[1].Value));
                }
                catch (Exception)
                {

                    MessageBox.Show("Erro ao remover usuário.");
                    return;
                }
                MessageBox.Show("Usuário removido com sucesso.");
            }
            else if (dialogResult == DialogResult.No)
            {
                return;
            }
            this.tRI_PDV_USERSTableAdapter.Fill(this.fDBDataSet.TRI_PDV_USERS);
        }
        private void alterarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            alteracao();
        }
        private void alteracao()
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            string _uname = Convert.ToString(selectedRow.Cells[1].Value);
            string _hash = Convert.ToString(selectedRow.Cells[2].Value);
            string _gere = Convert.ToString(selectedRow.Cells[3].Value);
            if (_hash == Properties.Settings.Default.HashCaixaAtual || _uname == funcoes.operador)
            {
                MessageBox.Show("Impossível alterar usuário atual. Favor logar com outro usuário com permissões gerenciais");
                return;
            }
            panel1.Controls.Add(altera);
            altera.altera = true;
            altera.antigo_username = _uname;
            altera.antigo_hash = _hash;
            altera.antigo_gere = _gere;
            altera.atualiza();
        }
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            alteracao();
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.cadastrarNovoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alterarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.iDUSERDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.uSERNAMEDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pASSWORDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gERENCIADataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.aTIVODataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tRIPDVUSERSBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.fDBDataSet = new PDV_WPF.FDBDataSet();
            this.tRI_PDV_USERSTableAdapter = new PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tRIPDVUSERSBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fDBDataSet)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cadastrarNovoToolStripMenuItem,
            this.alterarToolStripMenuItem,
            this.removerToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(652, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // cadastrarNovoToolStripMenuItem
            // 
            this.cadastrarNovoToolStripMenuItem.Name = "cadastrarNovoToolStripMenuItem";
            this.cadastrarNovoToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.cadastrarNovoToolStripMenuItem.Text = "Cadastrar";
            this.cadastrarNovoToolStripMenuItem.Click += new System.EventHandler(this.cadastrarNovoToolStripMenuItem_Click);
            // 
            // alterarToolStripMenuItem
            // 
            this.alterarToolStripMenuItem.Name = "alterarToolStripMenuItem";
            this.alterarToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.alterarToolStripMenuItem.Text = "Alterar";
            this.alterarToolStripMenuItem.Click += new System.EventHandler(this.alterarToolStripMenuItem_Click);
            // 
            // removerToolStripMenuItem
            // 
            this.removerToolStripMenuItem.Name = "removerToolStripMenuItem";
            this.removerToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.removerToolStripMenuItem.Text = "Desativar";
            this.removerToolStripMenuItem.Click += new System.EventHandler(this.removerToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(12, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(628, 83);
            this.panel1.TabIndex = 2;
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
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.iDUSERDataGridViewTextBoxColumn,
            this.uSERNAMEDataGridViewTextBoxColumn,
            this.pASSWORDDataGridViewTextBoxColumn,
            this.gERENCIADataGridViewTextBoxColumn,
            this.aTIVODataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.tRIPDVUSERSBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridView1.Location = new System.Drawing.Point(12, 116);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(628, 220);
            this.dataGridView1.TabIndex = 3;
            this.dataGridView1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView1_CellFormatting);
            this.dataGridView1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDoubleClick);
            // 
            // iDUSERDataGridViewTextBoxColumn
            // 
            this.iDUSERDataGridViewTextBoxColumn.DataPropertyName = "ID_USER";
            this.iDUSERDataGridViewTextBoxColumn.HeaderText = "ID_USER";
            this.iDUSERDataGridViewTextBoxColumn.Name = "iDUSERDataGridViewTextBoxColumn";
            this.iDUSERDataGridViewTextBoxColumn.ReadOnly = true;
            this.iDUSERDataGridViewTextBoxColumn.Visible = false;
            // 
            // uSERNAMEDataGridViewTextBoxColumn
            // 
            this.uSERNAMEDataGridViewTextBoxColumn.DataPropertyName = "USERNAME";
            this.uSERNAMEDataGridViewTextBoxColumn.HeaderText = "USUÁRIO";
            this.uSERNAMEDataGridViewTextBoxColumn.Name = "uSERNAMEDataGridViewTextBoxColumn";
            this.uSERNAMEDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // pASSWORDDataGridViewTextBoxColumn
            // 
            this.pASSWORDDataGridViewTextBoxColumn.DataPropertyName = "PASSWORD";
            this.pASSWORDDataGridViewTextBoxColumn.HeaderText = "SENHA";
            this.pASSWORDDataGridViewTextBoxColumn.Name = "pASSWORDDataGridViewTextBoxColumn";
            this.pASSWORDDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // gERENCIADataGridViewTextBoxColumn
            // 
            this.gERENCIADataGridViewTextBoxColumn.DataPropertyName = "GERENCIA";
            this.gERENCIADataGridViewTextBoxColumn.HeaderText = "GERENTE";
            this.gERENCIADataGridViewTextBoxColumn.Name = "gERENCIADataGridViewTextBoxColumn";
            this.gERENCIADataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // aTIVODataGridViewTextBoxColumn
            // 
            this.aTIVODataGridViewTextBoxColumn.DataPropertyName = "ATIVO";
            this.aTIVODataGridViewTextBoxColumn.HeaderText = "ATIVO";
            this.aTIVODataGridViewTextBoxColumn.Name = "aTIVODataGridViewTextBoxColumn";
            this.aTIVODataGridViewTextBoxColumn.ReadOnly = true;
            this.aTIVODataGridViewTextBoxColumn.Visible = false;
            // 
            // tRIPDVUSERSBindingSource
            // 
            this.tRIPDVUSERSBindingSource.DataMember = "TRI_PDV_USERS";
            this.tRIPDVUSERSBindingSource.DataSource = this.fDBDataSet;
            // 
            // fDBDataSet
            // 
            this.fDBDataSet.DataSetName = "FDBDataSet";
            this.fDBDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // tRI_PDV_USERSTableAdapter
            // 
            this.tRI_PDV_USERSTableAdapter.ClearBeforeFill = true;
            // 
            // Usuarios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 348);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Usuarios";
            this.Text = "Usuarios";
            this.Load += new System.EventHandler(this.Usuarios_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Usuarios_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tRIPDVUSERSBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fDBDataSet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem cadastrarNovoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alterarToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private PDV_WPF.FDBDataSet fDBDataSet;
        private System.Windows.Forms.BindingSource tRIPDVUSERSBindingSource;
        private PDV_WPF.FDBDataSetTableAdapters.TRI_PDV_USERSTableAdapter tRI_PDV_USERSTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn iDUSERDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn uSERNAMEDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pASSWORDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gERENCIADataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn aTIVODataGridViewTextBoxColumn;
    }
    #endregion
}

