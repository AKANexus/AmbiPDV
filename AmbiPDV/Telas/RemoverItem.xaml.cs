using System.Windows;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Telas
{
    /// <summary>
    /// Interaction logic for RemoverItem.xaml
    /// </summary>
    public partial class RemoverItem : Window
    {
        #region Fields & Properties

        public int _int;
        private int _numProxItem;


        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        #endregion Fields & Properties

        #region (De)Constructor

        public RemoverItem(int pProxItem)
        {
            _numProxItem = pProxItem;
            InitializeComponent();
            textBox1.Focus();
        }

        #endregion (De)Constructor

        #region Events

        private void RemoveItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                return;
            }
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text) || !int.TryParse(textBox1.Text, out _int))
                {
                    DialogBox.Show("Estornar Item",
                        DialogBoxButtons.No,
                        DialogBoxIcons.None, false,
                        "Por favor digitar um número.");
                    textBox1.Clear();
                    textBox1.Focus();
                    return;
                }
                //if (_int <= 0 || _int > _numProxItem)
                //{
                //    DialogBox.Show("Estornar Item",
                //        DialogBoxButtons.No,
                //        DialogBoxIcons.None, false,
                //        $"Por favor digitar um número entre 1 e {_numProxItem}.");
                //    textBox1.Clear();
                //    textBox1.Focus();
                //    return;
                //}
                DialogResult = true;
                return;
            }
        }

        #endregion Events

        #region Methods


        #endregion Methods
    }
}
