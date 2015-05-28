using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace WPFControl
{
    public static class Extension
    {
        public static string GetValueFromObject(this object selectedItem, string columnName)
        {
            if (selectedItem != null)
            {
                return selectedItem.GetType().GetProperty(columnName).GetValue(selectedItem, null).ToString();
            }
            return null;
        }

        public static List<object> GetItems(this AutoCompleteComboBox autoComplete, TextBox textBox, Enums.FilterMode filterMode = Enums.FilterMode.StartsWith)
        {
            var filterText = textBox.Text;
            var filterItems = autoComplete.ItemsSource.Cast<object>();
            switch (filterMode)
            {
                case Enums.FilterMode.StartsWith:
                    return filterItems.Where(x => x.GetValueFromObject(autoComplete.FilterColumn).ToLower().StartsWith(filterText.ToLower())).ToList();

                case Enums.FilterMode.StartsWithCaseSensitive:
                    return filterItems.ToList();

                case Enums.FilterMode.Contains:
                    return filterItems.ToList();

                case Enums.FilterMode.ContainsWithCaseSensitive:
                    return filterItems.ToList();

                default:
                    return filterItems.ToList();

            }
        }
    }
}
