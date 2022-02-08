using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace YandehCarga
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int HWND_BROADCAST = 0xffff;
        private static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        private bool CanMigrate = false;
        private static Mutex mutex;

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        private static extern int RegisterWindowMessage(string message);
        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "YandehCarga";

            if (Mutex.TryOpenExisting(mutexName, out _))
            {
                MessageBox.Show("Processo YandehCarga já rodando.");
                PostMessage(
                    (IntPtr)HWND_BROADCAST,
                    WM_SHOWME,
                    IntPtr.Zero,
                    IntPtr.Zero);
                Shutdown();
                return;
            }

            mutex = new(true, mutexName);

            new MainWindow().Show();
            base.OnStartup(e);
        }
    }


}
