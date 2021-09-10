using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PDV_WPF.Controls
{
    public class ComboBoxF4 : ComboBox
    {


        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                e.Handled = true;
                return;
            }
            base.OnPreviewKeyDown(e);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                e.Handled = true;
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
    }

    public static class ComboBoxHelper
    {
        public static readonly DependencyProperty DisableF4HotKeyProperty =
            DependencyProperty.RegisterAttached("DisableF4HotKey", typeof(bool),
                typeof(ComboBoxHelper), new PropertyMetadata(false, OnDisableF4HotKeyChanged));

        public static bool GetDisableF4HotKey(DependencyObject obj)
        {
            return (bool)obj.GetValue(DisableF4HotKeyProperty);
        }

        public static void SetDisableF4HotKey(DependencyObject obj, bool value)
        {
            obj.SetValue(DisableF4HotKeyProperty, value);
        }

        private static void OnDisableF4HotKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as ComboBox;
            if (d == null) return;

            box.PreviewKeyDown -= OnComboBoxKeyDown;
            box.PreviewKeyDown += OnComboBoxKeyDown;
        }

        private static void OnComboBoxKeyDown(object _, KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                e.Handled = true;
            }
        }
    }

}
