using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using System.ComponentModel;

namespace WPFControl
{
    public partial class AutoComplete : MultiSelector
    {
        private const string ElementTextBox = "PART_EditableTextBox";
        private const string ElementListBox = "PART_ListBox";
        private const string ElementToggleButton = "PART_ToggleButton";

        private AutoComplete _autoComplete;
        private TextBox _textBox;
        private ListBox _listBox;

        public AutoComplete()
        {
            this.Resources = Application.LoadComponent(new Uri("/WPFControl;component/Dictionary.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary;
        }

        public static readonly DependencyProperty IsAutoCompleteProperty = DependencyProperty.Register("IsAutoComplete", typeof(bool), typeof(AutoComplete),
                                                                           new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsDisplayAllItemsProperty = DependencyProperty.Register("IsDisplayAllItems", typeof(bool), typeof(AutoComplete),
                                                                               new PropertyMetadata(true));

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(AutoComplete),
                                                                           new PropertyMetadata());

        public static readonly DependencyProperty IsMultiSelectWithCheckBoxProperty = DependencyProperty.Register("IsMultiSelectWithCheckBox", typeof(bool), typeof(AutoComplete),
                                                                                      new PropertyMetadata(true));

        public static readonly DependencyProperty FilterColumnProperty = DependencyProperty.Register("FilterColumn", typeof(string), typeof(AutoComplete),
                                                                         new FrameworkPropertyMetadata());

        public static readonly DependencyProperty FilterModeProperty = DependencyProperty.Register("FilterMode", typeof(Enums.FilterMode), typeof(AutoComplete),
                                                                       new PropertyMetadata(Enums.FilterMode.StartsWith));

        public static readonly DependencyProperty AutoCompleteFilterProperty = DependencyProperty.Register("AutoCompleteFilter", typeof(Enums.AutoCompleteOption), typeof(AutoComplete),
                                                                               new PropertyMetadata(Enums.AutoCompleteOption.AutoCompleteText));

        public new static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble,
                                                                       typeof(SelectionChangedEventHandler), typeof(AutoComplete));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _autoComplete = this;
            _textBox = (TextBox)GetTemplateChild(ElementTextBox);
            _listBox = (ListBox)GetTemplateChild(ElementListBox);
            var toggleButton = (ToggleButton)GetTemplateChild(ElementToggleButton);

            if (_listBox != null)
            {
                _listBox.Width = _autoComplete.Width;
                _listBox.ItemsSource = _autoComplete.ItemsSource;

                if (_autoComplete.ItemTemplate != null) _listBox.ItemTemplate = _autoComplete.ItemTemplate;
                else if (!string.IsNullOrEmpty(this.DisplayMemberPath)) _listBox.DisplayMemberPath = _autoComplete.DisplayMemberPath;

                if (string.IsNullOrEmpty(_autoComplete.FilterColumn) && !string.IsNullOrEmpty(_autoComplete.DisplayMemberPath))
                    _autoComplete.FilterColumn = _autoComplete.DisplayMemberPath;

                _listBox.SelectionChanged += new SelectionChangedEventHandler(_listBox_SelectionChanged);
            }

            if (toggleButton != null)
            {
                toggleButton.Click += new RoutedEventHandler(toggleButton_Click);
            }

            if (_textBox != null && _autoComplete.IsAutoComplete)
            {
                _textBox.TextChanged += new TextChangedEventHandler(_textBox_TextChanged);
                _textBox.PreviewKeyDown += new KeyEventHandler(_textBox_PreviewKeyDown);
            }
        }

        public new event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public bool IsAutoComplete
        {
            get { return (bool)GetValue(IsAutoCompleteProperty); }
            set { SetValue(IsAutoCompleteProperty, value); }
        }

        public bool IsDisplayAllItems
        {
            get { return (bool)GetValue(IsDisplayAllItemsProperty); }
            set { SetValue(IsDisplayAllItemsProperty, value); }
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public bool IsMultiSelectWithCheckBox
        {
            get { return (bool)GetValue(IsMultiSelectWithCheckBoxProperty); }
            set { SetValue(IsMultiSelectWithCheckBoxProperty, value); }
        }

        public string FilterColumn
        {
            get { return (string)GetValue(FilterColumnProperty); }
            set { SetValue(FilterColumnProperty, value); }
        }

        public Enums.FilterMode FilterMode
        {
            get { return (Enums.FilterMode)GetValue(FilterModeProperty); }
            set { SetValue(FilterModeProperty, value); }
        }

        public Enums.AutoCompleteOption AutoCompleteFilter
        {
            get { return (Enums.AutoCompleteOption)GetValue(AutoCompleteFilterProperty); }
            set { SetValue(AutoCompleteFilterProperty, value); }
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayAllRecords();
        }

        private void _listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_autoComplete.FilterColumn))
            {
                var listbox = sender as ListBox;
                _autoComplete.SelectedItem = listbox.SelectedItem;
            }

            IsDropDownOpen = false;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_autoComplete.IsDisplayAllItems) UnRaiseTextBoxEvent();

            if (e.AddedItems.Count > 0)
            {
                _textBox.Text = e.AddedItems.Cast<object>().First().GetValueFromObject(_autoComplete.FilterColumn);
                base.OnSelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems));
            }
            else
            {
                _textBox.Text = string.Empty;
            }
            
            if (_autoComplete.IsDisplayAllItems) RaiseTextBoxEvent();
        }

        private void _textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var records = new List<object>();
            var textBox = sender as TextBox;

            if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrEmpty(_autoComplete.FilterColumn))
            {
                records = _autoComplete.GetItems(textBox);
            }

            if (records.Count == 0)
            {
                _autoComplete.SelectedItem = null;
            }

            IsDropDownOpen = string.IsNullOrEmpty(textBox.Text) || records.Count > 0 ? true : false;
            BindItemsSourceToListBox(records, string.IsNullOrEmpty(textBox.Text));
        }

        private void _textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (!IsDropDownOpen)
                {
                    IsDropDownOpen = !IsDropDownOpen;
                    DisplayAllRecords();
                }
            }
        }

        private void UnRaiseListBoxEvent()
        {
            _listBox.SelectionChanged -= _listBox_SelectionChanged;
        }

        private void UnRaiseTextBoxEvent()
        {
            _textBox.TextChanged -= _textBox_TextChanged;            
        }

        private void RaiseListBoxEvent()
        {
            _listBox.SelectionChanged += _listBox_SelectionChanged;
        }

        private void RaiseTextBoxEvent()
        {
            _textBox.TextChanged += _textBox_TextChanged;
        }

        private void DisplayAllRecords()
        {
            if (_autoComplete.IsDisplayAllItems && _autoComplete.Items.Count != _listBox.Items.Count)
            {
                BindItemsSourceToListBox(new List<object>(), true);
            }
        }

        private void BindItemsSourceToListBox(List<object> records, bool isDisplayAllItems)
        {
            UnRaiseTextBoxEvent();
            UnRaiseListBoxEvent();
            _listBox.ItemsSource = isDisplayAllItems && records.Count == 0 ? _autoComplete.ItemsSource : records;
            RaiseTextBoxEvent();
            RaiseListBoxEvent();
        }        

    }
}