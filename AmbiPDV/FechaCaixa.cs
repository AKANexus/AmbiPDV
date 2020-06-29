using ImprimirCupom;
using System;
using System.Drawing;
using System.Windows.Forms;
using static PublicFunc.funcoes;

namespace PDV
{
    public partial class FechaCaixa : Form
    {
        public FechaCaixa()
        {
            InitializeComponent();
            lbl_Terminal.Text = Properties.Settings.Default.no_caixa.ToString("000");
            lbl_Date.Text = DateTime.Now.ToShortDateString();
        }
        public decimal total { get; set; }
        public decimal dinheiro { get; set; }
        public decimal debito { get; set; }
        public decimal credito { get; set; }
        public decimal cheque { get; set; }
        public decimal vale { get; set; }
        public decimal troca { get; set; }
        public decimal suprimento { get; set; }
        public decimal sangria { get; set; }


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                                txb_Credito.Focus();
            }
        }

        private void txb_Credito_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txb_Debito.Focus();
            }
        }

        private void txb_Debito_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txb_Cheques.Focus();
            }
        }

        private void txb_Cheques_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txb_Vales.Focus();
            }
        }

        private void txb_Vales_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txb_Troca.Focus();
            }
        }

        private void txb_Troca_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txb_Suprimento.Focus();
            }
        }

        private void txb_Suprimento_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txb_Sangria.Focus();
            }
        }

        private void txb_Sangria_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                but_Fechar.Focus();
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            ((TextBox)sender).BackColor = Color.Yellow;
        }
        private void TextBox_Leave(object sender, EventArgs e)
        {
            ((TextBox)sender).BackColor = SystemColors.Window;
            atualiza_valores();

        }
        private void atualiza_valores()
        {
            Decimal.TryParse(txb_Dinheiro.Text, out decimal _din);
            Decimal.TryParse(txb_Debito.Text, out decimal _deb);
            Decimal.TryParse(txb_Credito.Text, out decimal _cre);
            Decimal.TryParse(txb_Cheques.Text, out decimal _che);
            Decimal.TryParse(txb_Vales.Text, out decimal _val);
            Decimal.TryParse(txb_Troca.Text, out decimal _tro);
            Decimal.TryParse(txb_Suprimento.Text, out decimal _sup);
            Decimal.TryParse(txb_Sangria.Text, out decimal _san);
            dinheiro = _din;
            debito = _deb;
            credito = _cre;
            cheque = _che;
            vale = _val;
            troca = _tro;
            suprimento = _sup;
            sangria = _san;
            total = dinheiro + debito + credito + cheque + vale;
            txb_Dinheiro.Text = _din.ToString("0.00");
            txb_Debito.Text = _deb.ToString("0.00");
            txb_Credito.Text = _cre.ToString("0.00");
            txb_Cheques.Text = _che.ToString("0.00");
            txb_Vales.Text = _val.ToString("0.00");
            txb_Troca.Text = _tro.ToString("0.00");
            txb_Suprimento.Text = _sup.ToString("0.00");
            txb_Sangria.Text = _san.ToString("0.00");
            txb_Total.Text = total.ToString("0.00");
        }

        private void but_Fechar_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Deseja fechar o caixa?", Properties.Settings.Default.NomeSoftware + " - Fechamento", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2);
            switch (dr)
            {
                case DialogResult.Yes:
                    //Lógica para o fechamento do caixa. Por enquanto o sistema imprime uma prova de conceito.
                    atualiza_valores();
                    PrintFECHA.operador = operador;
                    PrintFECHA.num_caixa = Properties.Settings.Default.no_caixa.ToString();
                    PrintFECHA.fechamento = DateTime.Now;
                    PrintFECHA.IMPRIME();
                    this.Close();
                    Properties.Settings.Default.HashCaixaAtual = "";
                    break;
                case DialogResult.No:
                    break;
                default:
                    break;
            }
        }
        private void but_Cancelar_Click(object sender, EventArgs e)
        {
            Valores_de_teste();
        }
        private void Valores_de_teste()
        {
            MessageBox.Show("Valores pré-definidos foram alimentados no sistema para comparação.");
            txb_Dinheiro.Text = "253,58";
            txb_Debito.Text = "240,29";
            txb_Credito.Text = "108,32";
            txb_Cheques.Text = "0,00";
            txb_Vales.Text = "0,00";
            txb_Suprimento.Text = "500,00";
            txb_Sangria.Text = "240,00";
            txb_Troca.Text = "12,00";
            //PrintFECHA.din_sistema = 253.58m;
            //PrintFECHA.deb_sistema = 240.29m;
            //PrintFECHA.cre_sistema = 108.32m;
            //PrintFECHA.che_sistema = 0m;
            //PrintFECHA.val_sistema = 0m;
            //PrintFECHA.sup_sistema = 500m;
            //PrintFECHA.san_sistema = 240m;
            //PrintFECHA.tro_sistema = 12m;
            PrintFECHA.tot_vendas = 253.58m + 240.29m + 108.32m;
            PrintFECHA.cups_cancelados = 3;
            PrintFECHA.val_cancelado = 12.55m;
            PrintFECHA.med_vendas = (253.58m + 240.29m + 108.32m) / 12;
            PrintFECHA.qte_estornado = 3;
            PrintFECHA.qte_cancelado = 6;
            PrintFECHA.tot_itens = 12 * 4;
            PrintFECHA.val_estornado = 3.95m;

            atualiza_valores();

        }//HACK Alimenta valores de teste no sistema para o teste de layout de impressão.
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
            this.lbl_Titulo = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_Terminal = new System.Windows.Forms.Label();
            this.lbl_Date = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.txb_Troca = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txb_Vales = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txb_Cheques = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txb_Debito = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txb_Credito = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txb_Dinheiro = new System.Windows.Forms.TextBox();
            this.txb_Suprimento = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txb_Sangria = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txb_Total = new System.Windows.Forms.TextBox();
            this.but_Fechar = new System.Windows.Forms.Button();
            this.but_Cancelar = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_Titulo
            // 
            this.lbl_Titulo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_Titulo.AutoSize = true;
            this.lbl_Titulo.Font = new System.Drawing.Font("Segoe UI", 16.25F, System.Drawing.FontStyle.Bold);
            this.lbl_Titulo.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_Titulo.Location = new System.Drawing.Point(50, 9);
            this.lbl_Titulo.Name = "lbl_Titulo";
            this.lbl_Titulo.Size = new System.Drawing.Size(267, 30);
            this.lbl_Titulo.TabIndex = 1;
            this.lbl_Titulo.Text = "FECHAMENTO DE CAIXA";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lbl_Terminal, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lbl_Date, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 42);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(343, 49);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // lbl_Terminal
            // 
            this.lbl_Terminal.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_Terminal.AutoSize = true;
            this.lbl_Terminal.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lbl_Terminal.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_Terminal.Location = new System.Drawing.Point(215, 25);
            this.lbl_Terminal.Name = "lbl_Terminal";
            this.lbl_Terminal.Size = new System.Drawing.Size(84, 21);
            this.lbl_Terminal.TabIndex = 5;
            this.lbl_Terminal.Text = "TERMINAL";
            // 
            // lbl_Date
            // 
            this.lbl_Date.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_Date.AutoSize = true;
            this.lbl_Date.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Date.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lbl_Date.Location = new System.Drawing.Point(61, 25);
            this.lbl_Date.Name = "lbl_Date";
            this.lbl_Date.Size = new System.Drawing.Size(49, 21);
            this.lbl_Date.TabIndex = 4;
            this.lbl_Date.Text = "DATA";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label2.Location = new System.Drawing.Point(204, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "TERMINAL";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(54, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "DATA";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.txb_Troca, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.label8, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.txb_Vales, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.txb_Cheques, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.txb_Debito, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label5, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.txb_Credito, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.txb_Dinheiro, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.txb_Suprimento, 1, 7);
            this.tableLayoutPanel2.Controls.Add(this.label9, 0, 7);
            this.tableLayoutPanel2.Controls.Add(this.label10, 0, 8);
            this.tableLayoutPanel2.Controls.Add(this.txb_Sangria, 1, 8);
            this.tableLayoutPanel2.Controls.Add(this.label11, 0, 11);
            this.tableLayoutPanel2.Controls.Add(this.txb_Total, 1, 11);
            this.tableLayoutPanel2.Controls.Add(this.but_Fechar, 0, 12);
            this.tableLayoutPanel2.Controls.Add(this.but_Cancelar, 1, 12);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(12, 97);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 13;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 41F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(343, 508);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // txb_Troca
            // 
            this.txb_Troca.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Troca.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.txb_Troca.Location = new System.Drawing.Point(174, 188);
            this.txb_Troca.Name = "txb_Troca";
            this.txb_Troca.Size = new System.Drawing.Size(74, 31);
            this.txb_Troca.TabIndex = 16;
            this.txb_Troca.Text = "0,00";
            this.txb_Troca.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Troca.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Troca.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txb_Troca_KeyDown);
            this.txb_Troca.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label8.Location = new System.Drawing.Point(108, 193);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 21);
            this.label8.TabIndex = 15;
            this.label8.Text = "TROCA";
            // 
            // txb_Vales
            // 
            this.txb_Vales.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Vales.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.txb_Vales.Location = new System.Drawing.Point(174, 151);
            this.txb_Vales.Name = "txb_Vales";
            this.txb_Vales.Size = new System.Drawing.Size(74, 31);
            this.txb_Vales.TabIndex = 14;
            this.txb_Vales.Text = "0,00";
            this.txb_Vales.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Vales.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Vales.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txb_Vales_KeyDown);
            this.txb_Vales.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(63, 156);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 21);
            this.label7.TabIndex = 13;
            this.label7.Text = "TOTAL VALES";
            // 
            // txb_Cheques
            // 
            this.txb_Cheques.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Cheques.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.txb_Cheques.Location = new System.Drawing.Point(174, 114);
            this.txb_Cheques.Name = "txb_Cheques";
            this.txb_Cheques.Size = new System.Drawing.Size(74, 31);
            this.txb_Cheques.TabIndex = 12;
            this.txb_Cheques.Text = "0,00";
            this.txb_Cheques.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Cheques.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Cheques.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txb_Cheques_KeyDown);
            this.txb_Cheques.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(89, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 21);
            this.label6.TabIndex = 11;
            this.label6.Text = "CHEQUES";
            // 
            // txb_Debito
            // 
            this.txb_Debito.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Debito.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.txb_Debito.Location = new System.Drawing.Point(174, 77);
            this.txb_Debito.Name = "txb_Debito";
            this.txb_Debito.Size = new System.Drawing.Size(74, 31);
            this.txb_Debito.TabIndex = 10;
            this.txb_Debito.Text = "0,00";
            this.txb_Debito.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Debito.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Debito.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txb_Debito_KeyDown);
            this.txb_Debito.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label5.Location = new System.Drawing.Point(56, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 21);
            this.label5.TabIndex = 9;
            this.label5.Text = "TOTAL DÉBITO";
            // 
            // txb_Credito
            // 
            this.txb_Credito.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Credito.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.txb_Credito.Location = new System.Drawing.Point(174, 40);
            this.txb_Credito.Name = "txb_Credito";
            this.txb_Credito.Size = new System.Drawing.Size(74, 31);
            this.txb_Credito.TabIndex = 8;
            this.txb_Credito.Text = "0,00";
            this.txb_Credito.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Credito.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Credito.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txb_Credito_KeyDown);
            this.txb_Credito.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label4.Location = new System.Drawing.Point(45, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "TOTAL CRÉDITO";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label3.Location = new System.Drawing.Point(86, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(82, 21);
            this.label3.TabIndex = 5;
            this.label3.Text = "DINHEIRO";
            // 
            // txb_Dinheiro
            // 
            this.txb_Dinheiro.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Dinheiro.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txb_Dinheiro.Location = new System.Drawing.Point(174, 3);
            this.txb_Dinheiro.Name = "txb_Dinheiro";
            this.txb_Dinheiro.Size = new System.Drawing.Size(74, 31);
            this.txb_Dinheiro.TabIndex = 6;
            this.txb_Dinheiro.Text = "0,00";
            this.txb_Dinheiro.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Dinheiro.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Dinheiro.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.txb_Dinheiro.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // txb_Suprimento
            // 
            this.txb_Suprimento.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Suprimento.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.txb_Suprimento.Location = new System.Drawing.Point(174, 266);
            this.txb_Suprimento.Name = "txb_Suprimento";
            this.txb_Suprimento.Size = new System.Drawing.Size(74, 31);
            this.txb_Suprimento.TabIndex = 20;
            this.txb_Suprimento.Text = "0,00";
            this.txb_Suprimento.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Suprimento.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Suprimento.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txb_Suprimento_KeyDown);
            this.txb_Suprimento.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.DarkGreen;
            this.label9.Location = new System.Drawing.Point(61, 271);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(107, 21);
            this.label9.TabIndex = 22;
            this.label9.Text = "SUPRIMENTO";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.DarkRed;
            this.label10.Location = new System.Drawing.Point(92, 308);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(76, 21);
            this.label10.TabIndex = 19;
            this.label10.Text = "SANGRIA";
            // 
            // txb_Sangria
            // 
            this.txb_Sangria.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Sangria.Font = new System.Drawing.Font("Segoe UI Semibold", 13F, System.Drawing.FontStyle.Bold);
            this.txb_Sangria.Location = new System.Drawing.Point(174, 303);
            this.txb_Sangria.Name = "txb_Sangria";
            this.txb_Sangria.Size = new System.Drawing.Size(74, 31);
            this.txb_Sangria.TabIndex = 23;
            this.txb_Sangria.Text = "0,00";
            this.txb_Sangria.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txb_Sangria.Enter += new System.EventHandler(this.TextBox_Enter);
            this.txb_Sangria.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txb_Sangria_KeyDown);
            this.txb_Sangria.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label11.Location = new System.Drawing.Point(109, 385);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(59, 21);
            this.label11.TabIndex = 21;
            this.label11.Text = "TOTAL";
            // 
            // txb_Total
            // 
            this.txb_Total.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txb_Total.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txb_Total.Location = new System.Drawing.Point(174, 381);
            this.txb_Total.Name = "txb_Total";
            this.txb_Total.ReadOnly = true;
            this.txb_Total.Size = new System.Drawing.Size(74, 29);
            this.txb_Total.TabIndex = 24;
            this.txb_Total.Text = "0,00";
            this.txb_Total.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // but_Fechar
            // 
            this.but_Fechar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.but_Fechar.AutoSize = true;
            this.but_Fechar.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.but_Fechar.Location = new System.Drawing.Point(48, 416);
            this.but_Fechar.Name = "but_Fechar";
            this.but_Fechar.Size = new System.Drawing.Size(120, 30);
            this.but_Fechar.TabIndex = 25;
            this.but_Fechar.Text = "FECHAR CAIXA";
            this.but_Fechar.UseVisualStyleBackColor = true;
            this.but_Fechar.Click += new System.EventHandler(this.but_Fechar_Click);
            // 
            // but_Cancelar
            // 
            this.but_Cancelar.AutoSize = true;
            this.but_Cancelar.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold);
            this.but_Cancelar.Location = new System.Drawing.Point(174, 416);
            this.but_Cancelar.Name = "but_Cancelar";
            this.but_Cancelar.Size = new System.Drawing.Size(84, 30);
            this.but_Cancelar.TabIndex = 26;
            this.but_Cancelar.Text = "CANCELA";
            this.but_Cancelar.UseVisualStyleBackColor = true;
            this.but_Cancelar.Click += new System.EventHandler(this.but_Cancelar_Click);
            // 
            // FechaCaixa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 617);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.lbl_Titulo);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FechaCaixa";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FechaCaixa";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_Titulo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lbl_Terminal;
        private System.Windows.Forms.Label lbl_Date;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox txb_Troca;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txb_Vales;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txb_Cheques;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txb_Debito;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txb_Credito;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txb_Dinheiro;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txb_Suprimento;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txb_Sangria;
        private System.Windows.Forms.TextBox txb_Total;
        private System.Windows.Forms.Button but_Fechar;
        private System.Windows.Forms.Button but_Cancelar;

        #endregion

    }

}
