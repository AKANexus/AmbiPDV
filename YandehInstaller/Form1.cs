using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FirebirdSql.Data.FirebirdClient;

namespace YandehInstaller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool fileChecked = false;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Base do ClippStore (CLIPP.FDB)|CLIPP.FDB";
            ofd.InitialDirectory = @"C:\Program Files (x86)\CompuFour\Clipp\Base\CLIPP.FDB";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(ofd.FileName))
                {
                    MessageBox.Show("O arquivo selecionado não existe. Tente novamente");
                    return;
                }
                FbConnection _conn = new FbConnection($@"initial catalog={ofd.FileName};data source=localhost;user id=SYSDBA;Password=masterkey");
                try
                {
                    _conn.Open();
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Falha ao abrir o arquivo\n" + exception.Message);
                    return;
                }

                txb_filePath.Text = ofd.FileName;
                fileChecked = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Instalar();
        }

        private void Instalar()
        {
            if (!fileChecked)
            {
                MessageBox.Show("Por favor, selecione o arquivo da base de dados e tente novamente");
                return;
            }

            if (string.IsNullOrWhiteSpace(txb_InstallDir.Text))
            {
                MessageBox.Show("Por favor, selecione o local de instalação, e tente novamente");
                return;
            }

            try
            {
                foreach (string filePath in Directory.GetFiles("Files"))
                {
                    File.Copy(filePath, Path.Combine(txb_InstallDir.Text, Path.GetFileName(filePath)));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

            try
            {
                File.WriteAllText(Path.Combine(txb_InstallDir.Text, "path.txt"), "localhost|"+txb_filePath.Text);

				Process.Start(@"C:\Windows\system32\sc.exe",
					$"CREATE \"Serviço de Carga Yandeh\" binpath= {Path.Combine(txb_InstallDir.Text, "YandehCargaWS.exe")} start=delayed-auto");

                Process.Start(@"C:\Windows\system32\sc.exe",
                    $"START \"Serviço de Carga Yandeh\"");

            }
            catch (Exception exception)
            {
                EventLog.WriteEntry("Instalador Yandeh", exception.Message, EventLogEntryType.Error);
                throw;
            }

            MessageBox.Show("Instalado com sucesso!\nVerifique se o serviço consta na lista de serviços.");
            Environment.Exit(0);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txb_InstallDir.Text = fbd.SelectedPath;
                Directory.CreateDirectory(fbd.SelectedPath);
            }
        }
        }
}
