using PDV_WPF.Telas;
using System;
using System.Threading;
using System.Windows.Threading;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.UIHandlers
{
    public class DialogBoxHandler
    {
        // O trecho comentado abaixo deixa um resquício de thread quando o app é encerrado...
        //#region Fields & Properties

        //private Thread statusThread = null;
        //private DialogBox popUp = null;
        //private AutoResetEvent popUpStarted = null;

        //#endregion Fields & Properties

        //#region Methods

        //public void Start(string title, string line1, DialogBox.DialogBoxIcons dialogBoxIcons)
        //{
        //    statusThread = new Thread(() => 
        //    {
        //        try
        //        {
        //            popUp = new DialogBox(title, line1, DialogBox.DialogBoxButtons.None, dialogBoxIcons);
        //            popUp.ShowDialog();
        //            popUp.Closed += (lsender, le) => 
        //            {
        //                popUp.Dispatcher.InvokeShutdown();
        //                popUp = null;
        //                statusThread = null;
        //                if (popUpStarted != null)
        //                {
        //                    popUpStarted.Dispose();
        //                    popUpStarted = null;
        //                }
        //            };
        //            //now we can let the main thread go ahead setting the AutoResetEvent
        //            popUpStarted.Set();
        //            Dispatcher.Run();
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //    });

        //    statusThread.SetApartmentState(ApartmentState.STA);
        //    //statusThread.IsBackground = true;
        //    statusThread.Priority = ThreadPriority.Normal;
        //    statusThread.Start();
        //    //create a new AutoResetEvent initially set to false
        //    popUpStarted = new AutoResetEvent(false);
        //    //and wait until the second thread signals to proceed
        //    popUpStarted.WaitOne();
        //}

        //public void Stop()
        //{
        //    if (popUp == null) return;

        //    // É necessário usar o dispatcher para chamar o método Close do DialogBox, pois esse window foi criado em outra thread, e esse método (Stop) roda na thread principal.
        //    popUp.Dispatcher.BeginInvoke(new Action(() => { popUp.Close(); }));
        //}

        //#endregion Methods

        // O trecho abaixo é uma cópia de
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/bab65512-8cbe-446a-b7b5-d46fdd7ef628/how-do-i-create-a-please-wait-window-using-wpfc-window-opens-but-text-doesnt-show?forum=wpf

        private Thread StatusThread = null;

        private DialogBox Popup = null;

        private AutoResetEvent PopupStarted = null;

        public void Start(string title, string line1, DialogBoxIcons dialogBoxIcons)
        {
            //create the thread with its ThreadStart method
            this.StatusThread = new Thread(() =>
            {
                try
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    this.Popup = new DialogBox(title, line1, DialogBoxButtons.None, dialogBoxIcons)
                    {
#pragma warning restore CS0618 // Type or member is obsolete
                        Width = 470
                    };
                    this.Popup.Show();
                    //this.Popup.ShowDialog(); // Não funciona com o DialogBox.cs. O app espera o resultado do dialog e não fecha mais.
                    this.Popup.Closed += (lsender, le) =>
                    {
                        //when the window closes, close the thread invoking the shutdown of the dispatcher
                        this.Popup.Dispatcher.InvokeShutdown();
                        this.Popup = null;
                        this.StatusThread = null;
                        this.PopupStarted.Dispose();
                        this.PopupStarted = null;
                    };
                    //now we can let the main thread go ahead setting the AutoResetEvent
                    this.PopupStarted.Set();
                    //this call is needed so the thread remains open until the dispatcher is closed
                    Dispatcher.Run();
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                    throw ex;
                }
            });

            //run the thread in STA mode to make it work correctly
            this.StatusThread.SetApartmentState(ApartmentState.STA);
            this.StatusThread.Priority = ThreadPriority.Normal;
            this.StatusThread.Start();
            //create a new AutoResetEvent initially set to false
            this.PopupStarted = new AutoResetEvent(false);
            //and wait until the second thread signals to proceed
            this.PopupStarted.WaitOne();
        }

        public void Stop()
        {
            if (this.Popup != null)
            {
                //need to use the dispatcher to call the Close method, because the window is created in another thread, and this method is called by the main thread
                this.Popup.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Popup.Close();
                }));
            }
        }
    }
}
