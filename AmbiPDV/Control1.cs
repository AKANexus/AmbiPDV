using System.Collections.Generic;
using System.Windows.Forms;
using static PDV_WPF.staticfunc;

namespace PDV
{
    public partial class Control1 : UserControl
    {
        public string novo_username;
        public string antigo_username { get; set; }
        public string novo_hash;
        public string antigo_hash { get; set; }
        public string novo_gere;
        public string antigo_gere { get; set; }
        public bool altera;
        public Control1()
        {
            InitializeComponent();
        }
        public void atualiza()
        {
            switch (altera)
            {
                case (true):
                    textBox1.ReadOnly = true;
                    textBox1.Text = antigo_username;
                    textBox2.Clear();
                    textBox3.Clear();
                    Dictionary<string, int> _dict = new Dictionary<string, int>()
                    {
                        { "NÃO", 0 }, { "SIM", 1 }
                    };

                    comboBox1.SelectedIndex = _dict[antigo_gere];
                    break;
                default:
                    textBox1.ReadOnly = false;
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                    comboBox1.SelectedIndex = 0;
                    break;
            }
        }
        public bool pegainfo()
        {
            switch (altera)
            {
                case (false):
                    if (textBox2.Text != textBox3.Text)
                    {
                        MessageBox.Show("As senhas não conferem.");
                        return false;
                    }
                    else if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
                    {
                        MessageBox.Show("Favor preencher todos os campos");
                        return false;
                    }
                    else
                    {
                        novo_username = textBox1.Text.ToUpper();
                        novo_hash = GenerateHash(textBox3.Text);
                        novo_gere = comboBox1.Text;
                        return true;
                    }
                case (true):
                    if (textBox2.Text != textBox3.Text)
                    {
                        MessageBox.Show("As senhas não conferem.");
                        return false;
                    }
                    else if (textBox1.Text == "")
                    {
                        MessageBox.Show("Favor preencher todos os campos");
                        return false;
                    }
                    else if ((textBox2.Text == "" && textBox3.Text == "") || (ChecaHash(textBox3.Text, antigo_hash)))
                    {
                        novo_username = antigo_username;
                        novo_hash = antigo_hash;
                        novo_gere = comboBox1.Text;
                        return true;
                    }
                    else
                    {
                        novo_username = textBox1.Text.ToUpper();
                        novo_hash = GenerateHash(textBox3.Text);
                        novo_gere = comboBox1.Text;
                        return true;
                    }
                default:
                    return false;
            }
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tRIPDVUSERSBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.tRIPDVUSERSBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "NÃO",
            "SIM"});
            this.comboBox1.Location = new System.Drawing.Point(489, 24);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(101, 29);
            this.comboBox1.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(486, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 21);
            this.label3.TabIndex = 14;
            this.label3.Text = "Supervisor";
            // 
            // textBox3
            // 
            this.textBox3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(314, 24);
            this.textBox3.MaxLength = 8;
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(137, 29);
            this.textBox3.TabIndex = 13;
            this.textBox3.UseSystemPasswordChar = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(171, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 21);
            this.label2.TabIndex = 12;
            this.label2.Text = "Senha";
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(171, 24);
            this.textBox2.MaxLength = 8;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(137, 29);
            this.textBox2.TabIndex = 11;
            this.textBox2.UseSystemPasswordChar = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 21);
            this.label1.TabIndex = 10;
            this.label1.Text = "Usuário";
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(3, 24);
            this.textBox1.MaxLength = 25;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(137, 29);
            this.textBox1.TabIndex = 9;
            // 
            // tRIPDVUSERSBindingSource
            // 
            this.tRIPDVUSERSBindingSource.DataMember = "TRI_PDV_USERS";
            // 
            // Control1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Name = "Control1";
            this.Size = new System.Drawing.Size(596, 59);
            ((System.ComponentModel.ISupportInitialize)(this.tRIPDVUSERSBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.BindingSource tRIPDVUSERSBindingSource;
        #endregion
    }
}
