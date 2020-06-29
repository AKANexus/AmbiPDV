using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace PDV_WPF.Controls
{
    public class FocusableAutoCompleteBox : AutoCompleteBox
    {
        public new void Focus()
        {
            if (Template.FindName("Text", this) is TextBox textbox)
            {
                textbox.Focus();
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                return;
            }
            else if (e.Key == Key.Escape && (Text != "" || Text != String.Empty))
            {
                Text = "";
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }
    }//Controle da caixa autocompletável.

}
