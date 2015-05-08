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

        private bool IsDropDownOpen = false;
        private TextBox _textBox;

        public PopupHyperLink()
        {
            this.Resources.Source = new Uri("/WPFControl;component/Dictionary.xaml", UriKind.Relative);
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(PopupHyperLink), new FrameworkPropertyMetadata(true));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = (TextBox)GetTemplateChild(ElementTextBox);
            var saveButton = (Button)GetTemplateChild(ElementSaveButton);
            var closeButton = (Button)GetTemplateChild(ElementCloseButton);
            var hyperLink = (Hyperlink)GetTemplateChild(ElementHyperLink);

            hyperLink.Click += new System.Windows.RoutedEventHandler(hyperLink_Click);
            saveButton.Click += new System.Windows.RoutedEventHandler(saveButton_Click);
            closeButton.Click += new System.Windows.RoutedEventHandler(closeButton_Click);

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

        private void hyperLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _textBox.Text = Data.ToString();
            Popup.IsOpen = IsDropDownOpen = !IsDropDownOpen;
        }

        private void saveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ClosePopupWindow();
        }

        private void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ClosePopupWindow();
        }

        private void ClosePopupWindow()
        {
            Popup.IsOpen = false;
        }
    }
}
