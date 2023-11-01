using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using static PDV_WPF.Funcoes.Statics;


namespace PDV_WPF.Controls
{
    class NexusCurrencyBox : TextBox, INotifyPropertyChanged
    {
        public static readonly CultureInfo ptBR = CultureInfo.GetCultureInfo("pt-BR");
        private decimal valor;

        public decimal Value
        {
            get
            {
                return valor;
            }
            set
            {
                valor = value;
                Text = value.ToString("C2", ptBR);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
            }
        }
        private bool caretIsOnDecimal = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public bool EnterToMoveNext { get; set; } = true;



        public NexusCurrencyBox()
        {
            Text = "R$ 0,00";
            Value = 0;
            HorizontalContentAlignment = HorizontalAlignment.Right;
            VerticalContentAlignment = VerticalAlignment.Center;
        }

        private int decimalCaretIndex()
        {
            if (!String.IsNullOrEmpty(Text))
            {
                return Text.Length - 3;
            }
            else return 0;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (CaretIndex <= Text.Length - 3)
            {
                caretIsOnDecimal = false;
            }
            else
            {
                caretIsOnDecimal = true;
            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                Value = 0;
                caretIsOnDecimal = false;
            }

            if (e.Key == Key.Back)
            {
                if (!caretIsOnDecimal)
                {
                    if (Text.Length == 7)
                    {
                        Value = decimal.Parse("0," + Text.Substring(decimalCaretIndex() + 1), ptBR);
                        e.Handled = true;
                        CaretIndex = Text.Length - 3;

                    }
                    else
                    {
                        Value = decimal.Parse($"{Text.Substring(3, Text.Length - 7)},{Text.Substring(decimalCaretIndex() + 1)}", ptBR);
                        e.Handled = true;
                        CaretIndex = Text.Length - 3;

                    }
                }
                else
                {
                    Value = decimal.Parse($"{Text.Substring(3, Text.Length - 6)},0{Text.Substring(Text.Length - 2, 1)}", ptBR);
                    //Text = $"R$ {Text.Substring(3, Text.Length - 6)},0{Text.Substring(Text.Length - 2, 1)}";
                    e.Handled = true;
                    CaretIndex = Text.Length;
                }
            }

            else if (e.Key == Key.Right || e.Key == Key.Left || e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {

            }
            else if (e.Key == Key.Space) e.Handled = true;
            else
            {
	            if (MaxLength != 0 && Text.Length >= MaxLength)
	            {
		            e.Handled = true;
		            return;
	            }

				if (this.SelectionLength == Text.Length)
                {
                    Value = 0;
                }
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (e.Text == "," || e.Text == ".")
            {
                caretIsOnDecimal = true;
                CaretIndex = Text.Length;
            }
            string inteiros = Text.Substring(3, Text.Length - 6);
            string decimais = Text.Substring(decimalCaretIndex() + 1);
            base.OnPreviewTextInput(e);

            if (ContemSoNumeros(e.Text))
            {
                if (!caretIsOnDecimal)
                {
                    if (decimal.Parse(inteiros) == 0)
                    {
                        Value = decimal.Parse($"R$ {e.Text},{decimais}".Substring(3), ptBR);
                        //Text = $"R$ {e.Text},{decimais}";
                    }
                    else
                    {
                        Value = decimal.Parse($"R$ {inteiros}{e.Text},{decimais}".Substring(3), ptBR);
                        //Text = $"R$ {inteiros}{e.Text},{decimais}";
                    }
                    CaretIndex = Text.Length - 3;
                }
                else
                {
                    Value = decimal.Parse($"R$ {inteiros},{decimais.Substring(1)}{e.Text}".Substring(3), ptBR);
                    //Text = $"R$ {inteiros},{decimais.Substring(1)}{e.Text}";
                    CaretIndex = Text.Length;
                }
            }
            //Value = decimal.Parse(Text.Substring(3), ptBR);
            e.Handled = true;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            caretIsOnDecimal = false;
            SelectAll();
            e.Handled = true;
            base.OnGotFocus(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (!this.IsFocused)
            {
                this.Focus();
                e.Handled = true;
            }
            else
            {
                if (CaretIndex <= Text.Length - 3)
                {
                    caretIsOnDecimal = false;
                }
                else
                {
                    caretIsOnDecimal = true;
                }
            }
            base.OnMouseDown(e);
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            var element = this as UIElement;
            if (e.Key == Key.Enter && EnterToMoveNext)
            {
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            base.OnKeyDown(e);
        }

        private static readonly System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[^0-9]+");

        private static bool ContemSoNumeros(string texto)
        {
            return !_regex.IsMatch(texto);
        }

    }
}
