using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Documents;

namespace WPFControl
{
    public class PopupHyperLink : Control
    {
        private const string ElementTextBox = "PART_TextBox";
        private const string ElementSaveButton = "PART_SaveButton";
        private const string ElementCloseButton = "PART_CloseButton";
        private const string ElementHyperLink = "PART_HyperLink";
        private const string ElementPopup = "PART_Popup";

        private TextBox _textBox;

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(PopupHyperLink), new FrameworkPropertyMetadata(string.Empty));

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupHyperLink));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = (TextBox)GetTemplateChild(ElementTextBox);
            var saveButton = (Button)GetTemplateChild(ElementSaveButton);
            var closeButton = (Button)GetTemplateChild(ElementCloseButton);
            var hyperLink = (Hyperlink)GetTemplateChild(ElementHyperLink);

            _textBox.Text = Data != null ? Data.ToString() : string.Empty;

            hyperLink.Click += new RoutedEventHandler(hyperLink_Click);
            saveButton.Click += new RoutedEventHandler(saveButton_Click);
            closeButton.Click += new RoutedEventHandler(closeButton_Click);
        }

        public Popup Popup
        {
            get { return (Popup)GetTemplateChild(ElementPopup); }
        }

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private void hyperLink_Click(object sender, RoutedEventArgs e)
        {            
            Popup.IsOpen = !Popup.IsOpen;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Data = _textBox.Text;
            RaiseEvent(new RoutedEventArgs(PopupHyperLink.ClickEvent, this));
            Popup.IsOpen = false;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = false;
        }
    }
}
