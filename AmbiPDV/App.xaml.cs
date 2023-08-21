using PDV_WPF.Telas;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace PDV_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1 && !Environment.GetCommandLineArgs().Contains("-Logoff"))
            {
                // If ther is more than one, than it is already running.
                MessageBox.Show("AmbiPDV já está sendo executado. Apenas uma instância do programa é permitida por vez, aguarde...", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(0);
                return;
            }
            System.IO.Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}\Logs");
            //Checa se o Firebird se encontra em execução
            if (Process.GetProcessesByName("fbserver").Length < 1 && Process.GetProcessesByName("firebird").Length < 1)
            {
                MessageBox.Show("O Firebird não está instalado, ou não se encontra em execução. Instale e/ou configure o Firebird para rodar como um processo.", "AmbiPDV", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return;
            }
            TimedBox dialog = new TimedBox("Iniciando AmbiPDV", "Carregando interface e sincronizando informações, aguarde ...  ", TimedBox.DialogBoxButtons.No, TimedBox.DialogBoxIcons.None, 200);
            dialog.Show();
            base.OnStartup(e);
        }
    }
}
