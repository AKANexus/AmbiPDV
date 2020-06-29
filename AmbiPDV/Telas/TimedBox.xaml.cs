using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class TimedBox : Window, IDisposable
    {
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
        bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    timer.Stop();
                }
            }
            //dispose unmanaged resources
            disposed = true;
        }
        public DialogBoxButtons dbb { get; set; }
        public enum DialogBoxButtons { Yes, No, YesNo }
        public enum DialogBoxIcons { None, Info, Warn, Error, Dolan }
        private static double elapsedseconds = 0;
        private static double timelimit = 1;
        DispatcherTimer timer = new DispatcherTimer();
        private void Timer_tick(object sender, EventArgs e)
        {
            elapsedseconds += 1;
            if (elapsedseconds == timelimit)
            {
                elapsedseconds = 0;
                this.Dispatcher.Invoke(() =>
                {
                    Close();
                });
                return;
            }
        }
        public TimedBox(string title, string line1, DialogBoxButtons dbbuttons, DialogBoxIcons dbicons, double seconds)
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(1);
            timelimit = seconds;
            timer.Tick += new EventHandler(Timer_tick);
            timer.Start();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run = new Run
            {
                Text = line1
            };
            tbl_Body.Inlines.Add(run);
            dbb = dbbuttons;


        }
        public TimedBox(string title, string line1, string line2, DialogBoxButtons dialogBoxButtons, DialogBoxIcons dbicons, double seconds)
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(1);
            timelimit = seconds;
            timer.Tick += new EventHandler(Timer_tick);
            timer.Start();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();

            Run run1 = new Run
            {
                Text = line1
            };
            Run run2 = new Run
            {
                Text = line2
            };
            tbl_Body.Inlines.Add(run1);
            tbl_Body.Inlines.Add(new LineBreak());
            tbl_Body.Inlines.Add(run2);

            dbb = dialogBoxButtons;


        }
        public TimedBox(string title, string line1, string line2, string line3, DialogBoxButtons dialogBoxButtons, DialogBoxIcons dbicons, double seconds)
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(1);
            timelimit = seconds;
            timer.Tick += new EventHandler(Timer_tick);
            timer.Start();
            tbl_Body.Inlines.Clear();
            lbl_Title.Text = title.ToUpper();
            Run run1 = new Run
            {
                Text = line1
            };
            Run run2 = new Run
            {
                Text = line2
            };
            Run run3 = new Run
            {
                Text = line3
            };
            tbl_Body.Inlines.Add(run1);
            tbl_Body.Inlines.Add(new LineBreak());
            tbl_Body.Inlines.Add(run2);
            tbl_Body.Inlines.Add(new LineBreak());
            tbl_Body.Inlines.Add(run3);


            dbb = dialogBoxButtons;


        }


        private void cancelar_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void confirmar_Click(object sender, MouseButtonEventArgs e)
        {
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}
