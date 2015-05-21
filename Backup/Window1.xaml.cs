using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace WindowsApplication1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class Window1 : Window
    {

        public Window1()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            b.ContextMenu.Width = b.ActualWidth;
            b.ContextMenuOpening += new ContextMenuEventHandler(b_ContextMenuOpening);

            DependencyObject d = VisualTreeHelper.GetChild(b, 0);
            MenuItem m = LogicalTreeHelper.FindLogicalNode(d, "DropDownButton") as MenuItem;
            m.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(m_PreviewMouseLeftButtonUp);
        }

        void b_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            b.ContextMenu.IsOpen = false;
            e.Handled = true;
        }

        void m_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (b.ContextMenu != null)
            {
                b.ContextMenu.PlacementTarget = b;
                b.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                ContextMenuService.SetPlacement(b, System.Windows.Controls.Primitives.PlacementMode.Bottom);
                b.ContextMenu.IsOpen = true;
            }
        }

        void DoOpenCommand(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog o = new System.Windows.Forms.OpenFileDialog();
            o.Filter = "All files (*.*)|*.*";
            o.ShowDialog();
        }

        void DoSaveCommand(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog o = new System.Windows.Forms.SaveFileDialog();
            o.Filter = "All files (*.*)|*.*";
            o.ShowDialog();
        }

    }
}