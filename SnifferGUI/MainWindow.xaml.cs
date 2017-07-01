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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using YyWsnCommunicatonLibrary;

using ExcelExport;

namespace SnifferGUI
{
  
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<M1> m1groups = new ObservableCollection<M1>();
        //datagr

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenfile_Click(object sender, RoutedEventArgs e)
        {
            //打开数据，更新表格
            string SourceBinary = "EA 18 01 51 00 06 61 23 45 67 0E 61 00 8D 63 E6 64 0B C3 65 F6 A5 66 17 83 00 E0 CB AE C5 ";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary);

            M1 m1Single =(M1) DeviceFactory.CreateDevice(SourceByte);

            m1groups.Add(m1Single);

            dgM1.ItemsSource = m1groups;
            







        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenFile_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            //openFileDialog.Filter = "文本文件|*.*|C#文件|*.cs|所有文件|*.*";
            //openFileDialog.RestoreDirectory = true;
            //openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                ObservableCollection<Device> devices = FileHelper.ReadFile(filename);
                int i = 1;
                foreach (M1 item in devices)
                {
                    item.DisplayID = i;
                    i++;
                    m1groups.Add(item);

                }
            }
            dgM1.ItemsSource = m1groups;
        }

        private void btnClearData_Click(object sender, RoutedEventArgs e)
        {
            m1groups.Clear();
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {

            SaveFileDialog saveDlg = new SaveFileDialog();
            if (saveDlg.ShowDialog() == true)
            {
                ExportXLS export = new ExportXLS();
                export.ExportWPFDataGrid(dgM1, saveDlg.FileName);

            }

               
            //

        }
    }
}
