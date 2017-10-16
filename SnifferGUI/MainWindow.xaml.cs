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
using System.ComponentModel;

namespace SnifferGUI
{
  
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<M1> m1groups = new ObservableCollection<M1>(); //DataGrid 中对应M1的表格
        ObservableCollection<M2> m2groups = new ObservableCollection<M2>(); //DataGrid 中对应M2的表格

        SerialPortHelper comport;
        int SerialNo = 1;
        int SerialNoM2 = 1;

        /// <summary>
        /// Main Windows 初始化
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            FindComport();

            dgM1.ItemsSource = m1groups;
            dgM2.ItemsSource = m2groups;


            //M1 排序用
            ICollectionView v = CollectionViewSource.GetDefaultView(dgM1.ItemsSource);
            v.SortDescriptions.Clear();
            ListSortDirection d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //M2 排序用
            ICollectionView vM2 = CollectionViewSource.GetDefaultView(dgM2.ItemsSource);
            vM2.SortDescriptions.Clear();
            ListSortDirection dM2 = ListSortDirection.Descending;
            vM2.SortDescriptions.Add(new SortDescription("DisplayID", dM2));
            vM2.Refresh();
        }


        private void btnOpenfile_Click(object sender, RoutedEventArgs e)
        {
            /*
            //打开数据，更新表格
            string SourceBinary = "EA 18 01 51 00 06 61 23 45 67 0E 61 00 8D 63 E6 64 0B C3 65 F6 A5 66 17 83 00 E0 CB AE C5 ";
            byte[] SourceByte = CommArithmetic.HexStringToByteArray(SourceBinary);

            M1 m1Single =(M1) DeviceFactory.CreateDevice(SourceByte);

            m1groups.Add(m1Single);

           
            */
   



        }

       
        
        /// <summary>
        /// 打开文件<br/>
        /// 
        /// 打开日志文件或从串口调试助手保存的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenFile_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = "c:\\";//注意这里写路径时要用c:\\而不是c:\
            openFileDialog.Filter = "文本文件|*.txt|Log文件|*.log|所有文件|*.*";
            //openFileDialog.RestoreDirectory = true;
            //openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                //TxtOpenFileName.Text = openFileDialog.FileName;
                ObservableCollection<Device> devices = FileHelper.ReadFile(filename);
                int i = 1;
                foreach (M1 item in devices)
                {
                    //增加ClientID的过滤
                   
                    if (txtFilterClientID.Text.Length > 0)
                    {
                        //符合过滤调价的才加入
                        byte[] checkClientID= CommArithmetic.HexStringToByteArray(item.ClientID);
                        byte[] byteFilterClientID = CommArithmetic.HexStringToByteArray(txtFilterClientID.Text);
                        if(checkClientID[0] == byteFilterClientID[0] && checkClientID[1]==byteFilterClientID[1])
                        {
                            item.DisplayID = i;
                            i++;
                            m1groups.Add(item);
                        }
                        //if (item.ClientID)
                    }
                    else
                    {
                        item.DisplayID = i;
                        i++;
                        m1groups.Add(item);
                    }
                }
            }
            
        }

        private void btnClearData_Click(object sender, RoutedEventArgs e)
        {
            SerialNo = 1;
            m1groups.Clear();

            SerialNoM2 = 1;
            m2groups.Clear();

        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "XLS文件|*.xls|所有文件|*.*";
            saveDlg.FileName = "UART_Export_" + DateTime.Now.ToString("MMddhhmmss");

            if (tabData.SelectedIndex == 0)
            {
                

                if (saveDlg.ShowDialog() == true)
                {
                    ExportXLS export = new ExportXLS();
                    export.ExportWPFDataGrid(dgM1, saveDlg.FileName, m1groups);

                }
            }

            if (tabData.SelectedIndex == 1)
            {


                if (saveDlg.ShowDialog() == true)
                {
                    ExportXLS export = new ExportXLS();
                    export.ExportWPFDataGridM2(dgM2, saveDlg.FileName,m2groups);

                }
            }


            //

        }


        private void FindComport()
        {
            cbSerialPort.Items.Clear();
            string[] spis = SerialPortHelper.GetSerialPorts();
            foreach (var portname in spis)
            {
                cbSerialPort.Items.Add(portname);

            }
            if (cbSerialPort.Items.Count > 0)
            {
                cbSerialPort.SelectedIndex = 0;
            }
        }

      

     

        private void btnResersh_Click(object sender, RoutedEventArgs e)
        {
            FindComport();

        }

        /// <summary>
        /// 发送监控指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            comport.SendCommand(txtSendCommand.Text);

        }


        /// <summary>
        /// Open Comport
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenComPort_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenComPort.Content.ToString() == "Open")
            {
                comport = new SerialPortHelper();
                comport.SerialPortReceived += Comport_SerialPortReceived;
                string portname = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());
                comport.InitCOM(portname);
                if (comport.OpenPort())
                {
                    btnOpenComPort.Content = "Close";
                    txtFilterClientID.IsEnabled = false;
                    //EnableControls();
                }



            }
            else
            {
                if (comport != null)
                {
                    comport.ClosePort();
                    btnOpenComPort.Content = "Open";
                    txtFilterClientID.IsEnabled = true;
                    //DisableControl();
                }
            }

        }

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定


            //在Log中体现
            Dispatcher.BeginInvoke(new Action(delegate
            {
                txtConsole.Text += Logger.GetTimeString() +"\t"+CommArithmetic.ToHexString(e.ReceivedBytes)+"\r\n";

                ObservableCollection<Device> devices = DeviceFactory.CreateDevices(e.ReceivedBytes);

                //YyWsnDeviceLibrary.M2”的对象强制转换为类型“YyWsnDeviceLibrary.M1”。
                //TODO:

                for (int i = 0; i < devices.Count; i++)
                {
                    //通用有效性验证

                    if (devices[i].DeviceMac == null)
                    {
                        continue;
                    }

                    if (devices[i].GetType() == typeof(M1))
                    {
                        //处理合法的M1数据
                        if (txtFilterClientID.Text.Length > 0)
                        {



                            //符合过滤调价的才加入
                            byte[] checkClientID = CommArithmetic.HexStringToByteArray(devices[i].ClientID);
                            byte[] byteFilterClientID = CommArithmetic.HexStringToByteArray(txtFilterClientID.Text);
                            if (checkClientID[0] == byteFilterClientID[0] && checkClientID[1] == byteFilterClientID[1])
                            {
                                devices[i].DisplayID = SerialNo;
                                SerialNo++;

                                m1groups.Add((M1)devices[i]);
                            }
                            //if (item.ClientID)
                        }
                        else
                        {

                            devices[i].DisplayID = SerialNo;
                            SerialNo++;

                            m1groups.Add((M1)devices[i]);
                        }


                    }

                    if (devices[i].GetType() == typeof(M2))
                    {
                        //处理合法的M1数据
                        if (txtFilterClientID.Text.Length > 0)
                        {



                            //符合过滤调价的才加入
                            byte[] checkClientID = CommArithmetic.HexStringToByteArray(devices[i].ClientID);
                            byte[] byteFilterClientID = CommArithmetic.HexStringToByteArray(txtFilterClientID.Text);
                            if (checkClientID[0] == byteFilterClientID[0] && checkClientID[1] == byteFilterClientID[1])
                            {
                                devices[i].DisplayID = SerialNoM2;
                                SerialNoM2++;

                                m2groups.Add((M2)devices[i]);
                            }
                            //if (item.ClientID)
                        }
                        else
                        {

                            devices[i].DisplayID = SerialNoM2;
                            SerialNoM2++;

                            m2groups.Add((M2)devices[i]);
                        }


                    }




                }
                      
            }));




        }
    }
}
