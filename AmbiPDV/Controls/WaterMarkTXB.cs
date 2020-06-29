using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PDV_WPF.Controls
{
    public class WaterMarkTXB : TextBox, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool TemTexto = !(bool)values[0];
            bool TemFoco = (bool)values[1];
            if (TemTexto || TemFoco) { return Visibility.Collapsed; }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

