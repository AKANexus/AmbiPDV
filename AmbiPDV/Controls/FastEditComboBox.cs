using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using static PDV_WPF.Funcoes.Statics;

namespace PDV_WPF.Controls

{
    public class FastEditComboBox : ComboBox
    {
        //PARTS
        private TextBox _TextBoxPart = null;

        //DEPENDENCY PROPERTIES
        public new static readonly DependencyProperty TextProperty
            = DependencyProperty.Register("Text", typeof(string), typeof(FastEditComboBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(FastEditComboBox.OnTextChanged)));

        private List<string> _CompletionStrings = new List<string>();
        private int _textBoxSelectionStart;
        private bool _updatingText;
        private bool _updatingSelectedItem;
        private static Dictionary<TextBox, FastEditComboBox> _TextBoxDictionary = new Dictionary<TextBox, FastEditComboBox>();

        static FastEditComboBox()
        {
            EventManager.RegisterClassHandler(typeof(TextBox), TextBoxBase.TextChangedEvent, new TextChangedEventHandler(FastEditComboBox.OnTextChanged));
            EventManager.RegisterClassHandler(typeof(TextBox), TextBoxBase.SelectionChangedEvent, new RoutedEventHandler(FastEditComboBox.OnSelectionChanged));
        }

        public string Text
        {
            get
            {
                return (string)base.GetValue(TextProperty);
            }
            set
            {
                base.SetValue(TextProperty, value);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F4)
            {
                base.OnKeyDown(e);
            }
            base.OnPreviewKeyDown(e);
        }

        public event EventHandler MultiplyAdded;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _TextBoxPart = base.GetTemplateChild("PART_EditableTextBox") as TextBox;
            if (!_TextBoxDictionary.ContainsKey(_TextBoxPart)) _TextBoxDictionary.Add(_TextBoxPart, this);
        }

        private void OnTextBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            this._textBoxSelectionStart = this._TextBoxPart.SelectionStart;
        }

        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsEditable)
            {
                TextUpdated(_TextBoxPart.Text, true);
            }
            MultiplyAdded?.Invoke(this, e);
        }

        private void TextUpdated(string newText, bool textBoxUpdated)
        {
            if (!_updatingText && !_updatingSelectedItem)
            {
                try
                {
                    _updatingText = true;
                    if (base.IsTextSearchEnabled)
                    {
                        int num = FindMatchingPrefix(newText);
                        if (num >= 0)
                        {
                            if (textBoxUpdated)
                            {
                                int selectionStart = this._TextBoxPart.SelectionStart;
                                if ((selectionStart == newText.Length) && (selectionStart > this._textBoxSelectionStart))
                                {
                                    string primaryTextFromItem = _CompletionStrings[num];
                                    this._TextBoxPart.Text = primaryTextFromItem;
                                    this._TextBoxPart.SelectionStart = newText.Length;
                                    this._TextBoxPart.SelectionLength = primaryTextFromItem.Length - newText.Length;
                                    newText = primaryTextFromItem;
                                }
                            }
                            else
                            {
                                string b = _CompletionStrings[num];
                                if (!string.Equals(newText, b, StringComparison.CurrentCulture))
                                {
                                    num = -1;
                                }
                            }
                        }
                        if (num != base.SelectedIndex)
                        {
                            SelectedIndex = num;
                        }
                    }
                    if (textBoxUpdated)
                    {
                        Text = newText;
                    }
                    else if (_TextBoxPart != null)
                    {
                        _TextBoxPart.Text = newText;
                    }
                }
                finally
                {
                    _updatingText = false;
                }
            }
        }

        internal void SelectedItemUpdated()
        {
            try
            {
                this._updatingSelectedItem = true;
                if (!this._updatingText)
                {
                    string primaryTextFromItem = GetPrimaryTextFromItem(SelectedItem);
                    Text = primaryTextFromItem;
                }
                this.Update();
            }
            finally
            {
                this._updatingSelectedItem = false;
            }
        }

        private void Update()
        {
            if (this.IsEditable)
            {
                this.UpdateEditableTextBox();
            }
            else
            {
                //this.UpdateSelectionBoxItem();
            }
        }

        private void UpdateEditableTextBox()
        {
            if (!_updatingText)
            {
                try
                {
                    this._updatingText = true;
                    string text = this.Text;
                    if ((this._TextBoxPart != null) && (this._TextBoxPart.Text != text))
                    {
                        this._TextBoxPart.Text = text;
                        this._TextBoxPart.SelectAll();
                    }
                }
                finally
                {
                    this._updatingText = false;
                }
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.RaiseEvent(e);
            this.SelectedItemUpdated();
            if (this.IsDropDownOpen)
            {
                object Item = SelectedItem;
                if (Item != null)
                {
                    base.OnSelectionChanged(e);
                }
                //object internalSelectedItem = base.InternalSelectedItem;
                //if (internalSelectedItem != null)
                //{
                //    base.NavigateToItem(internalSelectedItem, ItemsControl.ItemNavigateArgs.Empty);
                //}
            }
        }

        int FindMatchingPrefix(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return -1;
            if (_CompletionStrings.Count == 0) return -1;
            int index = _CompletionStrings.BinarySearch(s, StringComparer.CurrentCultureIgnoreCase);
            if (index >= 0) return index;
            index = ~index;
            string p = _CompletionStrings[index];
            if (p.StartsWith(s, StringComparison.CurrentCultureIgnoreCase)) return index;
            return -1;
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            FillCompletionStrings();
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    AddCompletionStrings(e.NewItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    RemoveCompletionStrings(e.OldItems);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    FillCompletionStrings();
                    break;
            }
        }

        private void FillCompletionStrings()
        {
            _CompletionStrings.Clear();
            AddCompletionStrings(Items);
        }

        private void RemoveCompletionStrings(IList items)
        {
            foreach (object o in items)
            {
                RemoveCompletionStringForItem(o);
            }
        }

        private void AddCompletionStrings(IList items)
        {
            foreach (object o in items)
            {
                AddCompletionStringForItem(o);
            }
        }

        private void AddCompletionStringForItem(object item)
        {
            Binding binding = new Binding(DisplayMemberPath);
            TextBlock tb = new TextBlock();
            tb.DataContext = item;
            tb.SetBinding(TextBlock.TextProperty, binding);
            string s = tb.Text;
            int index = _CompletionStrings.BinarySearch(s, StringComparer.CurrentCultureIgnoreCase);
            if (index < 0)
            {
                _CompletionStrings.Insert(~index, s);
            }
            else
            {
                _CompletionStrings.Insert(index, s);
            }
        }

        private string GetPrimaryTextFromItem(object item)
        {
            Binding binding = new Binding(DisplayMemberPath);
            TextBlock tb = new TextBlock();
            tb.DataContext = item;
            tb.SetBinding(TextBlock.TextProperty, binding);
            string s = tb.Text;
            return s;
        }

        private void RemoveCompletionStringForItem(object item)
        {
            Binding binding = new Binding(DisplayMemberPath);
            TextBlock tb = new TextBlock();
            tb.DataContext = item;
            tb.SetBinding(TextBlock.TextProperty, binding);
            string s = tb.Text;
            int index = _CompletionStrings.BinarySearch(s, StringComparer.CurrentCultureIgnoreCase);
            if (index >= 0) _CompletionStrings.RemoveAt(index);
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = e.Source as TextBox;
            if (tb.Name == "PART_EditableTextBox")
            {
                if (_TextBoxDictionary.ContainsKey(tb))
                {
                    FastEditComboBox combo = _TextBoxDictionary[tb];
                    combo.OnTextBoxTextChanged(sender, e);
                    e.Handled = true;
                }
            }
        }

        private static void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox tb = e.Source as TextBox;
            if (tb.Name == "PART_EditableTextBox")
            {
                if (_TextBoxDictionary.ContainsKey(tb))
                {
                    FastEditComboBox combo = _TextBoxDictionary[tb];
                    combo.OnTextBoxSelectionChanged(sender, e);
                    e.Handled = true;
                }
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FastEditComboBox actb = (FastEditComboBox)d;
            actb.TextUpdated((string)e.NewValue, false);
        }
    }
}