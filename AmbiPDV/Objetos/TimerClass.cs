using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace PDV_WPF.Objetos
{
    public class TimerClass : INotifyPropertyChanged
    {
        //public void OnPropertyChanged(string name)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(name));
        //    }
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        public TimerClass(DateTime horalancamento)
        {
            DateTime horalimite = horalancamento.AddMinutes(30);
            DispatcherTimer _timer = new DispatcherTimer();
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (horalimite < DateTime.Now)
                {
                    TempoRestante = "00:00";
                    _timer.Stop();
                }
                else
                {
                    TempoRestante = horalimite.Subtract(DateTime.Now).ToString(@"mm\:ss");
                }
            }, Application.Current.Dispatcher);
            //_timer.Start();
        }

        private string _tempoRestante;
        public string TempoRestante
        {
            get
            {
                return _tempoRestante;
            }
            set
            {
                _tempoRestante = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TempoRestante"));
            }
        }
    }

}
