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
        public MainWindow()
        {
            InitializeComponent();

            var list = new List<Employee>();
            list.Add(new Employee { EmployeeIdx = 1, EmpId = "A", EmpName = "dadsadasd" });
            list.Add(new Employee { EmployeeIdx = 2, EmpId = "AB", EmpName = "ssdsadsada" });
            list.Add(new Employee { EmployeeIdx = 3, EmpId = "ABC", EmpName = "dssdsdsadsad" });
            list.Add(new Employee { EmployeeIdx = 4, EmpId = "B", EmpName = "wqeqweqwe" });
            list.Add(new Employee { EmployeeIdx = 5, EmpId = "BA", EmpName = "gfdgfdgdf" });
            list.Add(new Employee { EmployeeIdx = 6, EmpId = "BAC", EmpName = "ipoiopipoi" });
            list.Add(new Employee { EmployeeIdx = 7, EmpId = "C", EmpName = "vxczxcxz" });
            list.Add(new Employee { EmployeeIdx = 8, EmpId = "CB", EmpName = "popoipoipo" });
            list.Add(new Employee { EmployeeIdx = 9, EmpId = "CBA", EmpName = "ljipkpk" });

            cmbEmployee.ItemsSource = list;
            cmbEmployee1.ItemsSource = list;

            var list1 = new List<Company>();
            list1.Add(new Company { CompanyIdx = 1, CompanyId = "A", CompanyName = "dadsadasd" });
            list1.Add(new Company { CompanyIdx = 2, CompanyId = "AB", CompanyName = "ssdsadsada" });
            list1.Add(new Company { CompanyIdx = 3, CompanyId = "ABC", CompanyName = "dssdsdsadsad" });
            cmbCompany.ItemsSource = list1;
        }

        private void cmbEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var aa = cmbEmployee.SelectedItem;
            cmbEmployee1.SelectedValue = cmbEmployee.SelectedValue;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            cmbEmployee1.SelectedValue = null;
        }
    }

    public class Employee
    {
        public int EmployeeIdx { get; set; }
        public string EmpName { get; set; }
        public string EmpId { get; set; }
    }

    public class Company
    {
        public int CompanyIdx { get; set; }
        public string CompanyName { get; set; }
        public string CompanyId { get; set; }
    }
}
