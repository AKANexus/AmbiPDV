using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDV_WPF.Telas;

namespace PDV_WPF.ViewModels
{
    public class CaixaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ComboBoxBindingDTO_Produto> LstProdutos { get; set; } = new();

    }
}
