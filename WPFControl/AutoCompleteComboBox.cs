﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace WPFControl
{
    public class AutoCompleteComboBox : Selector
    {
        private const string ElementPopup = "PART_Popup";
        private const string ElementTextBox = "PART_EditableTextBox";
        private const string ElementListBox = "PART_ListBox";
        private const string ElementToggleButton = "PART_ToggleButton";
        public const string ElementAddToggleButton = "PART_AddToggleButton";
        private const string ElementSortCheckbox = "PART_SortCheckBox";
        private const string ElementSortBorder = "PART_SortBorder";

        public string lastSelectedText;
        PreviewTextBox textBox;
        AutoCompleteComboBox autoComplete;
        ListBox listBox;
        CheckBox sortByCheckBox;
        Int32 selectedIndex = -1;
        Int32 defaultIndex = -1;
        string defaultFilterValue = "-1";

        public AutoCompleteComboBox()
        {
            this.Resources.Source = new Uri("/WPFControl;component/Dictionary.xaml", UriKind.Relative);
        }

        public static readonly DependencyProperty IsAddNewIconProperty = DependencyProperty.Register("IsAddNewIcon", typeof(bool), typeof(AutoCompleteComboBox), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsFilterDisplayAllItemsProperty = DependencyProperty.Register("IsFilterDisplayAllItems", typeof(bool), typeof(AutoCompleteComboBox), new PropertyMetadata(true));

        public static readonly DependencyProperty FilterColumnProperty = DependencyProperty.Register("FilterColumn", typeof(object), typeof(AutoCompleteComboBox), new FrameworkPropertyMetadata());

        public static readonly DependencyProperty SortColumnProperty = DependencyProperty.Register("SortColumn", typeof(object), typeof(AutoCompleteComboBox), new FrameworkPropertyMetadata());

        public new static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent("SelectionChanged", RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(AutoCompleteComboBox));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            autoComplete = this;
            textBox = (PreviewTextBox)GetTemplateChild(ElementTextBox);
            listBox = (ListBox)GetTemplateChild(ElementListBox);
            sortByCheckBox = (CheckBox)GetTemplateChild(ElementSortCheckbox);
            var sortBorder = (Border)GetTemplateChild(ElementSortBorder);
            var toggleButton = (ToggleButton)GetTemplateChild(ElementToggleButton);

            if (listBox != null)
            {
                autoComplete.Height = 23;
                listBox.MinWidth = double.IsNaN(autoComplete.Width) ? autoComplete.MinWidth : autoComplete.Width;
                listBox.ItemsSource = autoComplete.ItemsSource;

                if (autoComplete.ItemTemplate != null) listBox.ItemTemplate = autoComplete.ItemTemplate;
                else if (!string.IsNullOrEmpty(autoComplete.DisplayMemberPath)) listBox.DisplayMemberPath = autoComplete.DisplayMemberPath;

                autoComplete.SizeChanged += new SizeChangedEventHandler(autoComplete_SizeChanged);
                listBox.SelectionChanged += listBox_SelectionChanged;
            }

            if (toggleButton != null)
            {
                toggleButton.Click += new RoutedEventHandler(toggleButton_Click);
            }

            if (textBox != null && FilterColumn != null)
            {
                textBox.PreviewTextChanged += new PreviewTextChangedEventHandler(textBox_PreviewTextChanged);
                textBox.LostFocus += new RoutedEventHandler(textBox_LostFocus);
            }

            if (autoComplete.IsAddNewIcon)
            {
                var addToggleButton = (ToggleButton)GetTemplateChild(ElementAddToggleButton);
                addToggleButton.Visibility = Visibility.Visible;
            }

            if (sortByCheckBox != null && SortColumn != null)
            {
                sortByCheckBox.Checked += new RoutedEventHandler(sortByCheckBox_Checked);
                sortByCheckBox.Unchecked += new RoutedEventHandler(sortByCheckBox_Checked);
            }
            else
            {
                sortBorder.Visibility = Visibility.Collapsed;
            }

            this.PreviewKeyDown += AutoCompleteComboBox_PreviewKeyDown;
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

        public bool IsAddNewIcon
        {
            get { return (bool)GetValue(IsAddNewIconProperty); }
            set { SetValue(IsAddNewIconProperty, value); }
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

        public string SortColumn
        {
            get { return (string)GetValue(SortColumnProperty); }
            set { SetValue(SortColumnProperty, value); }
        }


        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayAllRecords();
            ClearBackgroundSelection();
        }

        private void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (autoComplete.SelectedItem == null)
            {
                textBox.Text = string.Empty;
            }
        }

        private void textBox_PreviewTextChanged(object sender, PreviewTextChangedEventArgs e)
        {
            selectedIndex = defaultIndex;
            var records = new List<object>();
            var filterTextBox = e.Text;

            autoComplete.SelectedItem = null;
            autoComplete.SelectedValue = null;

            if (!string.IsNullOrEmpty(filterTextBox.Trim()) && !string.IsNullOrEmpty(FilterColumn))
            {
                var filterItems = autoComplete.ItemsSource.Cast<object>();
                filterItems = filterItems.Where(x => GetValueFromObject(x, FilterColumn) != null && GetValueFromObject(x, FilterColumn).ToString().ToLower().StartsWith(filterTextBox.ToLower()));
                records = filterItems.ToList();
            }

            Popup.IsOpen = string.IsNullOrEmpty(filterTextBox) || records.Count > 0 ? true : false;
            BindItemsSourceToListBox(records, string.IsNullOrEmpty(filterTextBox));

            lastSelectedText = filterTextBox;
        }

        protected void AutoCompleteComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                    Popup.IsOpen = false;
                    break;

                case Key.Down:
                    if (!Popup.IsOpen)
                    {
                        Popup.IsOpen = !Popup.IsOpen;
                        DisplayAllRecords();
                        selectedIndex = defaultIndex;
                    }

                    if (selectedIndex < listBox.Items.Count - 1)
                    {
                        selectedIndex += 1;
                        listBox.ScrollIntoView(listBox.Items[selectedIndex]);
                        HighlightRowSelection();
                    }
                    break;

                case Key.Up:
                    if (selectedIndex > 0)
                    {
                        selectedIndex -= 1;
                        listBox.ScrollIntoView(listBox.Items[selectedIndex]);
                        HighlightRowSelection();
                    }
                    break;

                case Key.Enter:
                    if (selectedIndex > defaultIndex)
                    {
                        listBox.SelectedItem = listBox.Items[selectedIndex];
                        e.Handled = true;
                    }
                    break;
            }

        }

        private void autoComplete_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            listBox.MinWidth = autoComplete.ActualWidth;
        }

        private void sortByCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BindItemsSourceToListBox(new List<object>(), IsFilterDisplayAllItems);
        }

        protected void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                autoComplete.SelectedItem = e.AddedItems.Cast<object>().First();

                RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems));

                Popup.IsOpen = false;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var columnName = string.IsNullOrEmpty(FilterColumn) ? DisplayMemberPath : FilterColumn;
                if (string.IsNullOrEmpty(columnName) && autoComplete.ItemTemplate != null)
                {
                    ListBoxItem listBoxItem = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
                    TextBlock textBlock = FindFirstElementInVisualTree<TextBlock>(listBoxItem);
                    columnName = textBlock.GetBindingExpression(TextBlock.TextProperty).ParentBinding.Path.Path;
                }

                textBox.Text = GetValueFromObject(autoComplete.SelectedItem, columnName).ToString();
            }
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

            List<object> filterRecords = new List<object>();

            if (isFilterDisplayAllItems && records.Count == 0)
            {
                var autoCompleteRecords = autoComplete.ItemsSource.Cast<object>().ToList();
                filterRecords = GetOrderByRecord(autoCompleteRecords);
            }
            else
            {
                filterRecords = GetOrderByRecord(records);
            }

            listBox.ItemsSource = filterRecords;
        }

        private List<object> GetOrderByRecord(List<object> records)
        {
            if (sortByCheckBox.IsChecked == true)
            {
                IOrderedEnumerable<object> orderByRecords = null;

                var defaultRecord = records.SingleOrDefault(x => GetValueFromObject(x, autoComplete.SelectedValuePath).ToString() == defaultFilterValue);
                if (defaultRecord != null) records.Remove(defaultRecord);

                var splitSortColumns = SortColumn.Split(',').ToList();
                for (int i = 0; i < splitSortColumns.Count; i++)
                {
                    var columnName = splitSortColumns[i];

                    if (i == 0)
                    {
                        orderByRecords = records.OrderBy(x => GetValueFromObject(x, columnName));
                    }
                    else
                    {
                        orderByRecords = orderByRecords.ThenBy(x => GetValueFromObject(x, columnName));
                    }
                }

                records = orderByRecords.ToList();

                if (defaultRecord != null) records.Insert(0, defaultRecord);
            }

            return records;
        }

        private void HighlightRowSelection()
        {
            ClearBackgroundSelection();

            var item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[selectedIndex]) as ListBoxItem;
            if (item != null)
            {
                item.BorderBrush = Brushes.CornflowerBlue;
            }
        }

        private void ClearBackgroundSelection()
        {
            listBox.Items.Cast<object>().ToList().ForEach(x =>
            {
                var boxItem = listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem;
                if (boxItem != null)
                {
                    boxItem.BorderBrush = Brushes.Transparent;
                }
            });
        }

        private object GetValueFromObject(object selectedItem, string columnName)
        {
            if (selectedItem != null)
            {
                return selectedItem.GetType().GetProperty(columnName).GetValue(selectedItem, null);
            }
            return null;
        }

        private T FindFirstElementInVisualTree<T>(DependencyObject parentElement) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parentElement);
            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parentElement, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var result = FindFirstElementInVisualTree<T>(child);
                    if (result != null)
                        return result;

                }
            }
            return null;
        }
    }
}