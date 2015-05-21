﻿using System;
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
    public class AutoComplete : Selector
    {
        private const string ElementTextBox = "PART_EditableTextBox";
        private const string ElementListBox = "PART_ListBox";
        private const string ElementToggleButton = "PART_ToggleButton";
        private const string ElementPopup = "PART_Popup";

        private AutoComplete _autoComplete;
        private TextBox _textBox;
        private ListBox _listBox;
     

        public AutoComplete()
        {
            this.Resources.Source = new Uri("/WPFControl;component/Dictionary.xaml", UriKind.Relative);
            //var myResourceDictionary = new ResourceDictionary();
            //myResourceDictionary.Source = new Uri("/WPFControl;component/Dictionary.xaml", UriKind.Relative);
            //Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
        }

        public static readonly DependencyProperty IsAutoCompleteProperty = DependencyProperty.Register("IsAutoComplete", typeof(bool), typeof(AutoComplete),
                                                                           new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsFilterDisplayAllItemsProperty = DependencyProperty.Register("IsFilterDisplayAllItems", typeof(bool), typeof(AutoComplete),
                                                                               new PropertyMetadata(true));

        public static readonly DependencyProperty FilterColumnProperty = DependencyProperty.Register("FilterColumn", typeof(string), typeof(AutoComplete),
                                                                         new FrameworkPropertyMetadata());

        public static readonly DependencyProperty FilterModeProperty = DependencyProperty.Register("FilterMode", typeof(Enums.FilterMode), typeof(AutoComplete),
                                                                       new PropertyMetadata(Enums.FilterMode.StartsWith));

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
                _listBox.MinWidth = _autoComplete.Width;
                _listBox.ItemsSource = _autoComplete.ItemsSource;

                if (_autoComplete.ItemTemplate != null) _listBox.ItemTemplate = _autoComplete.ItemTemplate;
                else if (!string.IsNullOrEmpty(_autoComplete.DisplayMemberPath)) _listBox.DisplayMemberPath = _autoComplete.DisplayMemberPath;

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

        public Enums.FilterMode FilterMode
        {
            get { return (Enums.FilterMode)GetValue(FilterModeProperty); }
            set { SetValue(FilterModeProperty, value); }
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayAllRecords();
        }

        private void _listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listbox = sender as ListBox;
            _autoComplete.SelectedItem = listbox.SelectedItem;

            Popup.IsOpen = false;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (IsFilterDisplayAllItems) UnRaiseTextBoxEvent();
                _textBox.Text = e.AddedItems.Cast<object>().First().GetValueFromObject(FilterColumn);
                base.OnSelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, e.RemovedItems, e.AddedItems));
                if (IsFilterDisplayAllItems) RaiseTextBoxEvent();
            }
        }

        private void _textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var records = new List<object>();
            var textBox = sender as TextBox;

            if (!string.IsNullOrEmpty(textBox.Text.Trim()) && !string.IsNullOrEmpty(FilterColumn))
            {
                records = _autoComplete.GetItems(textBox);
            }

            if (records.Count == 0)
            {
                _autoComplete.SelectedItem = null;
            }

            Popup.IsOpen = string.IsNullOrEmpty(textBox.Text) || records.Count > 0 ? true : false;
            BindItemsSourceToListBox(records, string.IsNullOrEmpty(textBox.Text));
        }

        private void _textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (!Popup.IsOpen)
                {
                    Popup.IsOpen = !Popup.IsOpen;
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
            if (IsFilterDisplayAllItems && _autoComplete.Items.Count != _listBox.Items.Count)
            {
                BindItemsSourceToListBox(new List<object>(), true);
            }
        }

        private void BindItemsSourceToListBox(List<object> records, bool isFilterDisplayAllItems)
        {
            UnRaiseTextBoxEvent();
            UnRaiseListBoxEvent();
            _listBox.ItemsSource = null;
            _listBox.ItemsSource = isFilterDisplayAllItems && records.Count == 0 ? _autoComplete.ItemsSource : records;
            RaiseTextBoxEvent();
            RaiseListBoxEvent();
        }
    }
}