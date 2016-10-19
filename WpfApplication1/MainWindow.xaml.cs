using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TestCompanyDBEntities entity = new TestCompanyDBEntities();

        public MainWindow()
        {
            InitializeComponent();

            cmbEmployee.ItemsSource = entity.Crews.OrderBy(x => x.CrewId).ToList();
        }

        private void cmbEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var aa = cmbEmployee.SelectedItem;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            cmbEmployee.SelectedValue = 1;
        }
    }
}
