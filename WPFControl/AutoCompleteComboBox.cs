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
    public class AutoCompleteComboBox : Selector
    {
        private const string ElementTextBox = "PART_EditableTextBox";
        private const string ElementListBox = "PART_ListBox";
        private const string ElementToggleButton = "PART_ToggleButton";
        private const string ElementPopup = "PART_Popup";

        private AutoCompleteComboBox autoComplete;
        public PreviewTextBox TextBox;
        private ListBox listBox;
        private Int32 countMoveItem = 0;
        private int selectedIndex = 0;

        public AutoCompleteComboBox()
        {
            this.Resources.Source = new Uri("/WPFControl;component/Dictionary.xaml", UriKind.Relative);
        }

        public static readonly DependencyProperty IsAutoCompleteProperty = DependencyProperty.Register("IsAutoComplete", typeof(bool), typeof(AutoCompleteComboBox),
                                                                           new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsFilterDisplayAllItemsProperty = DependencyProperty.Register("IsFilterDisplayAllItems", typeof(bool), typeof(AutoCompleteComboBox),
                                                                               new PropertyMetadata(true));

        public static readonly DependencyProperty FilterColumnProperty = DependencyProperty.Register("FilterColumn", typeof(string), typeof(AutoCompleteComboBox),
                                                                         new FrameworkPropertyMetadata());

        public static readonly DependencyProperty FilterModeProperty = DependencyProperty.Register("FilterMode", typeof(Enums.FilterMode), typeof(AutoCompleteComboBox),
                                                                       new PropertyMetadata(Enums.FilterMode.StartsWith));

        public new static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble,
                                                                       typeof(SelectionChangedEventHandler), typeof(AutoCompleteComboBox));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            autoComplete = this;
            TextBox = (PreviewTextBox)GetTemplateChild(ElementTextBox);
            listBox = (ListBox)GetTemplateChild(ElementListBox);
            var toggleButton = (ToggleButton)GetTemplateChild(ElementToggleButton);

            if (listBox != null)
            {
                autoComplete.Height = 23;
                listBox.MinWidth = double.IsNaN(autoComplete.Width) ? autoComplete.MinWidth : autoComplete.Width;
                listBox.ItemsSource = autoComplete.ItemsSource;

                if (autoComplete.ItemTemplate != null) listBox.ItemTemplate = autoComplete.ItemTemplate;
                else if (!string.IsNullOrEmpty(autoComplete.DisplayMemberPath)) listBox.DisplayMemberPath = autoComplete.DisplayMemberPath;
                else if (!string.IsNullOrEmpty(autoComplete.SelectedValuePath)) listBox.SelectedValuePath = autoComplete.SelectedValuePath;

                autoComplete.SizeChanged += new SizeChangedEventHandler(autoComplete_SizeChanged);
                listBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(listBox_PreviewMouseLeftButtonUp);
                listBox.PreviewKeyUp += new KeyEventHandler(listBox_PreviewKeyUp);
            }

            if (toggleButton != null)
            {
                toggleButton.Click += new RoutedEventHandler(toggleButton_Click);
            }

            if (TextBox != null && autoComplete.IsAutoComplete)
            {
                TextBox.PreviewTextChanged += new PreviewTextChangedEventHandler(TextBox_PreviewTextChanged);
                TextBox.LostFocus += new RoutedEventHandler(TextBox_LostFocus);
                TextBox.PreviewKeyUp += new KeyEventHandler(TextBox_PreviewKeyUp);
            }
        }

        public new event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        public Popup Popup
        {
            get { return (Popup)GetTemplateChild(ElementPopup); }
        }

        public bool IsAutoComplete
        {
            get { return (bool)GetValue(IsAutoCompleteProperty); }
            set { SetValue(IsAutoCompleteProperty, value); }
        }

        public bool IsFilterDisplayAllItems
        {
            get { return (bool)GetValue(IsFilterDisplayAllItemsProperty); }
            set { SetValue(IsFilterDisplayAllItemsProperty, value); }
        }

        public string FilterColumn
        {
            get { return (string)GetValue(FilterColumnProperty); }
            set { SetValue(FilterColumnProperty, value); }
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayAllRecords();
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (!Popup.IsOpen)
                {
                    Popup.IsOpen = !Popup.IsOpen;
                    DisplayAllRecords();
                    countMoveItem = 0;
                    listBox.ScrollIntoView(listBox.Items[0]);
                }
                else
                {
                    if (countMoveItem == 0)
                    {
                        listBox.ScrollIntoView(listBox.Items[0]);
                        var listItem = listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[countMoveItem]) as ListBoxItem;
                        if (listItem != null)
                        {
                            listItem.Focus();
                        }
                        countMoveItem = 1;
                    }
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (autoComplete.SelectedItem == null)
            {
                TextBox.Text = string.Empty;
            }
        }

        private void TextBox_PreviewTextChanged(object sender, PreviewTextChangedEventArgs e)
        {
            countMoveItem = selectedIndex = 0;
            var records = new List<object>();
            var filterTextBox = e.Text;

            autoComplete.SelectedItem = null;
            autoComplete.SelectedValue = null;

            if (!string.IsNullOrEmpty(filterTextBox.Trim()) && !string.IsNullOrEmpty(FilterColumn))
            {
                var filterItems = autoComplete.ItemsSource.Cast<object>();
                records = filterItems.Where(x => GetValueFromObject(x, FilterColumn).ToLower().StartsWith(filterTextBox.ToLower())).ToList();
            }

            Popup.IsOpen = string.IsNullOrEmpty(filterTextBox) || records.Count > 0 ? true : false;
            BindItemsSourceToListBox(records, string.IsNullOrEmpty(filterTextBox));
        }

        private void listBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    selectedIndex = (sender as ListBox).SelectedIndex;
                    break;

                case Key.Up:
                    selectedIndex = (sender as ListBox).SelectedIndex;
                    break;

                case Key.Enter:
                    var updateListBox = listBox.Items[selectedIndex];
                    AssingSelectedItem(updateListBox);
                    break;

            }
        }

        private void listBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listbox = sender as ListBox;

            if (listbox.SelectedItem == null)
            {
                return;
            }

            AssingSelectedItem(listbox.SelectedItem);
        }

        private void autoComplete_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            listBox.MinWidth = autoComplete.ActualWidth;
        }        

        private void DisplayAllRecords()
        {
            if (IsFilterDisplayAllItems && autoComplete.Items.Count != listBox.Items.Count)
            {
                BindItemsSourceToListBox(new List<object>(), true);
            }
        }

        private void BindItemsSourceToListBox(List<object> records, bool isFilterDisplayAllItems)
        {
            listBox.ItemsSource = null;
            listBox.ItemsSource = isFilterDisplayAllItems && records.Count == 0 ? autoComplete.ItemsSource : records;
        }

        private void AssingSelectedItem(object selectedItem)
        {
            autoComplete.SelectedItem = selectedItem;
            autoComplete.SelectedValue = GetValueFromObject(selectedItem, autoComplete.SelectedValuePath);
            RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, new List<object>(), new List<object>()));
            if (autoComplete.SelectedItem != null)
            {
                TextBox.Text = GetValueFromObject(autoComplete.SelectedItem, FilterColumn);
            }

            var listBoxItem = listBox.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListBoxItem;
            if (listBoxItem != null)
            {
                listBoxItem.IsSelected = false;
            }

            Popup.IsOpen = false;
        }

        private string GetValueFromObject(object selectedItem, string columnName)
        {
            if (selectedItem != null)
            {
                return selectedItem.GetType().GetProperty(columnName).GetValue(selectedItem, null).ToString();
            }
            return null;
        }
    }
}