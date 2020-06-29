using System;
using System.ComponentModel;

namespace PDV_WPF.Objetos
{
    public class CupomSAT : INotifyPropertyChanged
    {
        public decimal VALOR_TOTAL { get; set; }
        public DateTime TS_VENDA { get; set; }
        public string CHAVE_CFE { get; set; }
        public char CANCEL_CFE { get; set; }
        public string NF_SERIE { get; set; }
        public int ID_REGISTRO { get; set; }
        private TimerClass _countdown;
        public DateTime HoraLancamento
        {
            set
            {
                _countdown = new TimerClass(value);
                _countdown.PropertyChanged += _countdown_PropertyChanged;
                return;
            }
        }
        public string Countdown
        {
            get
            {
                return _countdown.TempoRestante;
            }
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Countdown"));
            }
        }
        public int NUM_CAIXA { get; set; }
        public int ID_NFVENDA { get; set; }
        //public string XML { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void _countdown_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Countdown = _countdown.TempoRestante;
        }
    }

}
