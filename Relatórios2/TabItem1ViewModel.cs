using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FastReport;
using FastReport.Export.PdfSimple;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Win32;

namespace Relatórios2
{
    public class TabItem1VM : INotifyPropertyChanged
    {
        public TabItem1VM()
        {
            GerarRelatório = new GerarRelatórioCommand(this);
        }
        public DateTime startDate { get; set; } //In reality this should utilize INotifyPropertyChanged!

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new
            PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public ICommand GerarRelatório { get; set; }
    }

    internal class GerarRelatórioCommand : ICommand
    {
        private TabItem1VM tabItem1VM;

        public GerarRelatórioCommand(TabItem1VM tabItem1VM)
        {
            this.tabItem1VM = tabItem1VM;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object _)
        {
            return true;// tabItem1VM.StartDate < tabItem1VM.EndDate;
        }

        public void Execute(object _)
        {
            if (!File.Exists("caminho.ini"))
            {
                File.WriteAllText("caminho.ini","C:\\Program Files (x86)\\CompuFour\\Clipp\\Base\\CLIPP.FDB");
            }

            string path = File.ReadAllLines("caminho.ini")[0];
            path = @"D:\TRILHEIROS\D - Bases de Clientes\Base clipp loja 2 da trilha\CLIPP.FDB";
            if (!File.Exists(path))
            {
                MessageBox.Show("O arquivo da base de dados não foi encontrado. Ele deve estar numa pasta local");
                return;
            }
            using FbConnection fbConn =
                new FbConnection(
                    $@"initial catalog={path};data source=localhost;user id=sysdba;Password=masterkey;encoding=WIN1252");
            using FbCommand command = new FbCommand($"SELECT DT_EMISSAO, NF_SERIE, NF_NUMERO, TOT_NF, DESC_FORMAPAGAMENTO, VLR_PAGTO " +
                                                    $"FROM V_NFV vn JOIN V_NFVENDA_PAGAMENTOS vnp ON vn.ID_NFVENDA = vnp.ID_NFVENDA WHERE " +
                                                    $"vn.DT_SAIDA >= CAST ('{tabItem1VM.StartDate:yyyy-MM-dd}' AS DATE) AND vn.DT_SAIDA <= CAST " +
                                                    $"('{tabItem1VM.EndDate:yyyy-MM-dd}' AS DATE) ORDER BY NF_SERIE, NF_NUMERO;", fbConn);
            using DataTable vendas = new DataTable();

            command.CommandType = CommandType.Text;
            using FbDataAdapter vendasAdapter = new FbDataAdapter(command);
            vendasAdapter.Fill(vendas);

            string key = "";

            ReportObjectVenda rov = new ReportObjectVenda();
            rov.Vendas = new();
            rov.StartDate = tabItem1VM.StartDate;
            rov.EndDate = tabItem1VM.EndDate;


            foreach (DataRow vRow in vendas.Rows)
            {
                if ($"{vRow["NF_SERIE"]}-{vRow["NF_NUMERO"]}" == key)
                {
                    var found = rov.Vendas.First(x => x.NumCupom == key);
                    found.Pagamentos.Add(new((string)vRow["DESC_FORMAPAGAMENTO"], (decimal)vRow["VLR_PAGTO"]));
                }
                else
                {
                    key = $"{vRow["NF_SERIE"]}-{vRow["NF_NUMERO"]}";
                    List<Pagamento> pagamentos = new();
                    pagamentos.Add(new((string)vRow["DESC_FORMAPAGAMENTO"], (decimal)vRow["VLR_PAGTO"]));
                    DateTime _tsVenda = (DateTime)vRow["DT_EMISSAO"];
                    decimal _vlrVenda = (decimal)vRow["TOT_NF"];

                    rov.Vendas.Add(new(key, _tsVenda, _vlrVenda, pagamentos));
                }
            }


            //ReportObjectVenda protorov = new ReportObjectVenda();
            //var protoz = new List<ReportObjectVenda> { protorov };

            //Report protoreport = new Report();
            //protoreport.Load("ReportFrameWork.frx");
            //protoreport.Report.Dictionary.RegisterBusinessObject(protoz, "Vendas", 3, true);
            //protoreport.Save("ReportFrameWork.frx");

            //Debugger.Break();

            var z = new List<ReportObjectVenda> { rov };

            Debugger.Break();


            Report report = new();
            report.Load("ReportFrameWork.frx");
            report.RegisterData(z, "Vendas");
            report.Prepare();
            SaveFileDialog sfd = new()
            {
                DefaultExt = ".pdf",
                Filter = "Arquivos PDF (.pdf) | *.pdf",
                InitialDirectory = @"%UserProfile%\Documents"
            };
            if (sfd.ShowDialog() == true)
            {
                PDFSimpleExport reportExport = new();
                report.Export(reportExport, sfd.FileName);
                ProcessStartInfo pi = new ProcessStartInfo
                {
                    FileName = "explorer",
                    Arguments = $"\"{sfd.FileName}\""
                };
                Process.Start(pi);
            }


            //Debugger.Break();


        }
    }
}
