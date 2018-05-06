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


using System.Data;
using System.Configuration;
using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
//using System.Web.UI.HtmlControls;
//using System.Drawing;
//using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
        int SerialNoM1 = 1;                 // M1表格的行编号              
        int SerialNoM2 = 1;                 // M2表格的行编号

        UInt32 ExSensorId = 0;              // 期望的Sensor ID
        UInt16 ExCustomer = 0;              // 期望的客户码

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

        /// <summary>
        /// 判断是否是符合要求的设备类型
        /// </summary>
        /// <param name="rxDeviceType"></param>
        /// <returns>
        /// true:    符合要求，显示
        /// false： 不符合要求，丢弃
        /// </returns>
        private bool isDesDeviceType(byte rxDeviceType) {

            byte ExDeviceType = 0x00;

            if (cbSensorType.SelectedIndex == 0) {   
                // All
                return true;
            } else if (cbSensorType.SelectedIndex == 1) {
                // M1
                ExDeviceType = 0x51;
            } else if (cbSensorType.SelectedIndex == 2) {
                // M1_NTC
                ExDeviceType = 0x5C;
            } else if (cbSensorType.SelectedIndex == 3) {
                // M1_Beetech
                ExDeviceType = 0x5D;
            }else {
                return false;
            }

            if(rxDeviceType != ExDeviceType) {
                return false;
            }

            return true;
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
                        byte[] checkClientID = CommArithmetic.HexStringToByteArray(item.ClientID);
                        byte[] byteFilterClientID = CommArithmetic.HexStringToByteArray(txtFilterClientID.Text);
                        if (checkClientID[0] == byteFilterClientID[0] && checkClientID[1] == byteFilterClientID[1])
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
            SerialNoM1 = 1;
            m1groups.Clear();

            SerialNoM2 = 1;
            m2groups.Clear();
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "XLS文件|*.xls|所有文件|*.*";
            saveDlg.FileName = "Export_" + DateTime.Now.ToString("MMdd_hhmmss");

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
                    export.ExportWPFDataGridM2(dgM2, saveDlg.FileName, m2groups);
                }
            }
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
        private void btnSendCommand_Click(object sender, RoutedEventArgs e) {
            // bps
            byte bps = (byte)cbBps.SelectedIndex;

            // freq
            UInt32 freq = Convert.ToUInt32(tbxFreq.Text);

            // Expect Number
            byte expNum = Convert.ToByte(tbxNum.Text);

            // Cmd
            string Cmd = "CE 09 A0 01" + bps.ToString("X2") + freq.ToString("X8") + expNum.ToString("X2") + "01 00 00 EC";

            comport.SendCommand(Cmd);
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
                    //EnableControls();
                    cbSerialPort.IsEnabled = false;
                    btnResersh.IsEnabled = false;
                }
            }
            else
            {
                if (comport != null)
                {
                    comport.ClosePort();
                    btnOpenComPort.Content = "Open";
                    //DisableControl();
                }
                cbSerialPort.IsEnabled = true;
                btnResersh.IsEnabled = true;
            }
        }

        /// <summary>
        /// 处理Sensor的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>       
        private Int16 HandleSensorData(Device device)
        {
            if (isDesDeviceType(device.DeviceTypeB) == false) {
                return -1;
            }

            // 更新期望的客户码
            if (txtFilterClientID.Text.Length > 0)
            {
                byte[] ExCustomerByte = CommArithmetic.HexStringToByteArray(txtFilterClientID.Text);
                if (ExCustomerByte != null && ExCustomerByte.Length >= 2)
                {
                    ExCustomer = (UInt16)(ExCustomerByte[0] * 256 + ExCustomerByte[1]);
                }
            }
            else
            {
                ExCustomer = 0x0000;
            }

            // 更新期望的Sensor ID
            if (txtDestMac.Text.Length > 0)
            {
                byte[] ExSensorIdByte = CommArithmetic.HexStringToByteArray(txtDestMac.Text);
                if (ExSensorIdByte != null && ExSensorIdByte.Length >= 4)
                {
                    ExSensorId = (UInt32)(ExSensorIdByte[0] * 256 * 256 * 256 + ExSensorIdByte[1] * 256 * 256 + ExSensorIdByte[2] * 256 + ExSensorIdByte[3]);
                }
            }
            else
            {
                ExSensorId = 0x00000000;
            }  
            
            // 接收到数据的设备类型
            Type deviceType = device.GetType();

            if (deviceType == typeof(M1))
            {
                // 接收到数据的Sensor ID
                byte[] RxSensorIdByte = CommArithmetic.HexStringToByteArray(device.DeviceMac);
                UInt32 RxSensorId = (UInt32)(RxSensorIdByte[0] * 256 * 256 * 256 + RxSensorIdByte[1] * 256 * 256 + RxSensorIdByte[2] * 256 + RxSensorIdByte[3]);
                if (ExSensorId != 0 && ExSensorId != RxSensorId)
                {
                    return -1;
                }

                // 接收到数据的客户码
                byte[] RxCustomerByte = CommArithmetic.HexStringToByteArray(device.ClientID);
                UInt16 RxCustomer = (UInt16)(RxCustomerByte[0] * 256 + RxCustomerByte[1]);
                if (ExCustomer != 0 && ExCustomer != RxCustomer)
                {
                    return -2;
                }                

                // 显示数据
                device.DisplayID = SerialNoM1++;
                m1groups.Add((M1)device);
            }
            else if (deviceType == typeof(M2))
            {
                // 接收到数据的Sensor ID
                byte[] RxSensorIdByte = CommArithmetic.HexStringToByteArray(device.DeviceMac);
                UInt32 RxSensorId = (UInt32)(RxSensorIdByte[0] * 256 * 256 * 256 + RxSensorIdByte[1] * 256 * 256 + RxSensorIdByte[2] * 256 + RxSensorIdByte[3]);
                if (ExSensorId != 0 && ExSensorId != RxSensorId)
                {
                    return -3;
                }

                // 接收到数据的客户码
                byte[] RxCustomerByte = CommArithmetic.HexStringToByteArray(device.ClientID);
                UInt16 RxCustomer = (UInt16)(RxCustomerByte[0] * 256 + RxCustomerByte[1]);
                if (ExCustomer != 0 && ExCustomer != RxCustomer)
                {
                    return -4;
                }

                // 显示数据
                device.DisplayID = SerialNoM2++;
                m2groups.Add((M2)device);                
            }

            return 0;
        }

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定

            //在Log中体现
            Dispatcher.BeginInvoke(new Action(delegate
            {
                txtConsole.Text += Logger.GetTimeString() + "\t" + CommArithmetic.ToHexString(e.ReceivedBytes) + "\r\n";

                ObservableCollection<Device> devices = DeviceFactory.CreateDevices(e.ReceivedBytes);

                for (int i = 0; i < devices.Count; i++)
                {
                    //通用有效性验证

                    if (devices[i].DeviceMac == null)
                    {
                        continue;
                    }

                    HandleSensorData(devices[i]);                                  
                }
            }));
        }
    }
}
