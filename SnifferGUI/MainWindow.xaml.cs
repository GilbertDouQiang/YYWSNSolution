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
using YyWsnDeviceLibrary;
using System.Collections;



namespace SnifferGUI
{
  
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<M1> m1groups = new List<M1>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenfile_Click(object sender, RoutedEventArgs e)
        {
            //打开数据，更新表格


        }
    }
}
