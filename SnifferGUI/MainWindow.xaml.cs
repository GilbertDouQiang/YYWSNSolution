
using System;
using System.Windows;
using System.Windows.Data;
using YyWsnDeviceLibrary;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using YyWsnCommunicatonLibrary;
using ExcelExport;
using System.ComponentModel;
using System.Configuration;
using System.Data;

namespace SnifferGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<M1> SensorDataOfM1 = new ObservableCollection<M1>();           // M1的传感数据
        ObservableCollection<M5> SensorDataOfM5 = new ObservableCollection<M5>();           // M5的传感数据
        ObservableCollection<M20> SensorDataOfM20 = new ObservableCollection<M20>();        // M20的传感数据
        ObservableCollection<M1> SensorDataOfMAX31855 = new ObservableCollection<M1>();     // MAX31855的传感数据
        ObservableCollection<M1> GroupFeedBack = new ObservableCollection<M1>();            // DataGrid 中对应M2的表格
        ObservableCollection<M9> SensorDataOfM9 = new ObservableCollection<M9>();           // M9的传感数据
        ObservableCollection<M40> SensorDataOfM40 = new ObservableCollection<M40>();        // M40的传感数据
        ObservableCollection<ACO2> SensorDataOfACO2 = new ObservableCollection<ACO2>();     // ACO2的传感数据
        ObservableCollection<SK> SensorDataOfSK = new ObservableCollection<SK>();           // SK的传感数据
        ObservableCollection<AO2> SensorDataOfAO2 = new ObservableCollection<AO2>();        // AO2的传感数据
        ObservableCollection<L1> SensorDataOfL1 = new ObservableCollection<L1>();           // L1的传感数据

        DataSet DS = new DataSet();

        UInt32[] ExSensorIdBuf = new UInt32[64];        // 期望的Sensor ID，允许是多个
        UInt16 ExSensorIdLen = 0;                       // 期望的Sensor ID的数量

        SerialPortHelper comport;

        int TableLineOfM1 = 1;                  // M1表格的行编号  
        int TableLineOfM5 = 1;                  // M5表格的行编号            
        int TableLineOfM20 = 1;                 // M20表格的行编号                  
        int TableLineOfMAX31855 = 1;            // M1表格的行编号              
        int TableLineOfAck = 1;                 // 传感器数据包的反馈表格的行编号
        int TableLineOfM9 = 1;                  // M9表格的行编号
        int TableLineOfM40 = 1;                 // M40表格的行编号
        int TableLineOfACO2 = 1;                // ACO2表格的行编号
        int TableLineOfSK = 1;                  // SK表格的行编号
        int TableLineOfAO2 = 1;                 // AO2表格的行编号
        int TableLineOfL1 = 1;                  // L1表格的行编号

        UInt16 ExCustomer = 0x0000;             // 期望的客户码

        /// <summary>
        /// Main Windows 初始化
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.Title += " v" +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            SearchComPort();

            TableOfM1.ItemsSource = SensorDataOfM1;
            TableOfM5.ItemsSource = SensorDataOfM5;
            TableOfM20.ItemsSource = SensorDataOfM20;
            TableOfMAX31855.ItemsSource = SensorDataOfMAX31855;
            TableOfAck.ItemsSource = GroupFeedBack;
            TableOfM9.ItemsSource = SensorDataOfM9;
            TableOfM40.ItemsSource = SensorDataOfM40;
            TableOfACO2.ItemsSource = SensorDataOfACO2;
            TableOfSK.ItemsSource = SensorDataOfSK;
            TableOfAO2.ItemsSource = SensorDataOfAO2;
            TableOfL1.ItemsSource = SensorDataOfL1;

            //M1 排序用
            ICollectionView v = CollectionViewSource.GetDefaultView(TableOfM1.ItemsSource);
            v.SortDescriptions.Clear();
            ListSortDirection d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            // M5 排序用
            v = CollectionViewSource.GetDefaultView(TableOfM5.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            // M20 排序用
            v = CollectionViewSource.GetDefaultView(TableOfM20.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //MAX31855 排序用
            v = CollectionViewSource.GetDefaultView(TableOfMAX31855.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //反馈包 排序用
            v = CollectionViewSource.GetDefaultView(TableOfAck.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //M9 排序用
            v = CollectionViewSource.GetDefaultView(TableOfM9.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //M40 排序用
            v = CollectionViewSource.GetDefaultView(TableOfM40.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //ACO2 排序用
            v = CollectionViewSource.GetDefaultView(TableOfACO2.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //SK 排序用
            v = CollectionViewSource.GetDefaultView(TableOfSK.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //AO2 排序用
            v = CollectionViewSource.GetDefaultView(TableOfAO2.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //L1 排序用
            v = CollectionViewSource.GetDefaultView(TableOfL1.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();
        }

        /// <summary>
        /// 判断是否是符合要求的设备类型
        /// </summary>
        /// <param name="rxDeviceType"></param>
        /// <returns>
        /// true:    符合要求，显示
        /// false： 不符合要求，丢弃
        /// </returns>
        private bool isDesDeviceType(byte rxDeviceType)
        {

            byte ExDeviceType = 0x00;

            if (cbSensorType.SelectedIndex == 0)
            {
                // All
                return true;
            }
            else if (cbSensorType.SelectedIndex == 1)
            {
                // M1
                ExDeviceType = 0x51;
            }
            else if (cbSensorType.SelectedIndex == 2)
            {
                // M1_NTC
                ExDeviceType = 0x5C;
            }
            else if (cbSensorType.SelectedIndex == 3)
            {
                // M1_Beetech
                ExDeviceType = 0x5D;
            }
            else if (cbSensorType.SelectedIndex == 4)
            {
                // M9
                ExDeviceType = 0x77;
            }
            else if (cbSensorType.SelectedIndex == 5)
            {
                // M30
                ExDeviceType = 0x79;
            }
            else if (cbSensorType.SelectedIndex == 6)
            {
                // ACO2
                ExDeviceType = ACO2.GetDeviceType();
            }
            else if (cbSensorType.SelectedIndex == 7)
            {
                // SK
                ExDeviceType = SK.GetDeviceType();
            }
            else if (cbSensorType.SelectedIndex == 8)
            {
                // AO2
                ExDeviceType = AO2.GetDeviceType();
            }
            else if (cbSensorType.SelectedIndex == 9)
            {
                // M20
                ExDeviceType = M20.GetDeviceType();
            }
            else if (cbSensorType.SelectedIndex == 10)
            {
                // L1
                ExDeviceType = L1.GetDeviceType();
            }else if(cbSensorType.SelectedIndex == 11)
            {
                // M40
                ExDeviceType = M40.GetDeviceType();
            }
            else
            {
                return false;
            }

            if (rxDeviceType != ExDeviceType)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 创建并记录日志
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
                        byte[] checkClientID = CommArithmetic.HexStringToByteArray(item.CustomerS);
                        byte[] byteFilterClientID = CommArithmetic.HexStringToByteArray(txtFilterClientID.Text);
                        if (checkClientID[0] == byteFilterClientID[0] && checkClientID[1] == byteFilterClientID[1])
                        {
                            item.DisplayID = i++;
                            SensorDataOfM1.Add(item);
                        }
                        //if (item.CustomerS)
                    }
                    else
                    {
                        item.DisplayID = i++;
                        SensorDataOfM1.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// 清空接收到的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearData_Click(object sender, RoutedEventArgs e)
        {
            if(TableM1.IsSelected == true)
            {
                TableLineOfM1 = 1;
                SensorDataOfM1.Clear();
            }

            if (TableM5.IsSelected == true)
            {
                TableLineOfM5 = 1;
                SensorDataOfM5.Clear();
            }

            if (TableM20.IsSelected == true)
            {
                TableLineOfM20 = 1;
                SensorDataOfM20.Clear();
            }

            if (TableMAX31855.IsSelected == true)
            {
                TableLineOfMAX31855 = 1;
                SensorDataOfMAX31855.Clear();
            }

            if (TableAck.IsSelected == true)
            {
                TableLineOfAck = 1;
                GroupFeedBack.Clear();
            }

            if (TableM9.IsSelected == true)
            {
                TableLineOfM9 = 1;
                SensorDataOfM9.Clear();
            }

            if (TableM40.IsSelected == true)
            {
                TableLineOfM40 = 1;
                SensorDataOfM40.Clear();
            }

            if (TableACO2.IsSelected == true)
            {
                TableLineOfACO2 = 1;
                SensorDataOfACO2.Clear();
            }

            if (TableSK.IsSelected == true)
            {
                TableLineOfSK = 1;
                SensorDataOfSK.Clear();
            }

            if (TableAO2.IsSelected == true)
            {
                TableLineOfAO2 = 1;
                SensorDataOfAO2.Clear();
            }

            if (TableL1.IsSelected == true)
            {
                TableLineOfL1 = 1;
                SensorDataOfL1.Clear();
            }
        }

        /// <summary>
        /// 清空所有接收到的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            TableLineOfM1 = 1;
            SensorDataOfM1.Clear();

            TableLineOfM5 = 1;
            SensorDataOfM5.Clear();

            TableLineOfM20 = 1;
            SensorDataOfM20.Clear();

            TableLineOfMAX31855 = 1;
            SensorDataOfMAX31855.Clear();

            TableLineOfAck = 1;
            GroupFeedBack.Clear();

            TableLineOfM9 = 1;
            SensorDataOfM9.Clear();

            TableLineOfM40 = 1;
            SensorDataOfM40.Clear();

            TableLineOfACO2 = 1;
            SensorDataOfACO2.Clear();

            TableLineOfSK = 1;
            SensorDataOfSK.Clear();

            TableLineOfAO2 = 1;
            SensorDataOfAO2.Clear();

            TableLineOfL1 = 1;
            SensorDataOfL1.Clear();
        }

        /// <summary>
        /// 将接收到的数据以Excel表的格式导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "XLS文件|*.xls|所有文件|*.*";
            saveDlg.FileName = "Export_" + DateTime.Now.ToString("yyyyMMdd_hhmmss");

            if (TableM1.IsSelected == true)
            {
                if (saveDlg.ShowDialog() == true)
                {
                    ExportXLS export = new ExportXLS();
                    export.ExportWPFDataGrid(TableOfM1, saveDlg.FileName, SensorDataOfM1);
                }
            }
           
            if (TableM9.IsSelected == true)
            {
                if (saveDlg.ShowDialog() == true)
                {
                    ExportXLS export = new ExportXLS();
                    export.ExportWPFDataGridFromM9(TableOfM9, saveDlg.FileName, SensorDataOfM9);
                }
            }
        }


        /// <summary>
        /// 搜索串口设备
        /// </summary>
        private void SearchComPort()
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

        /// <summary>
        /// 刷新，重新搜索串口设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnResersh_Click(object sender, RoutedEventArgs e)
        {
            SearchComPort();
        }

        /// <summary>
        /// 发送监控指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenComPort.Content.ToString() == "Open")
            {
                return;
            }

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
        /// 保存并启用输入的Sensor ID序列
        /// </summary>
        /// <param name=""></param>
        private void SaveExpSensorId()
        {
            // 更新期望的Sensor ID序列
            Array.Clear(ExSensorIdBuf, 0, ExSensorIdBuf.Length);
            ExSensorIdLen = 0;

            if (txtDestMac.Text.Length <= 0)
            {
                return;
            }

            // 按照逗号分隔符将字符串拆分为多个子字符串
            txtDestMac.Text = txtDestMac.Text.Replace(" ", "");
            txtDestMac.Text = txtDestMac.Text.Replace("\r", "");
            txtDestMac.Text = txtDestMac.Text.Replace("\n", "");
            txtDestMac.Text = txtDestMac.Text.Replace("\t", "");
            string[] SID = txtDestMac.Text.Split(',');

            foreach (string sid in SID)
            {
                byte[] ExSensorIdByte = CommArithmetic.HexStringToByteArray(sid);
                if (ExSensorIdByte != null && ExSensorIdByte.Length >= 4)
                {
                    ExSensorIdBuf[ExSensorIdLen++] = (UInt32)(ExSensorIdByte[0] * 256 * 256 * 256 + ExSensorIdByte[1] * 256 * 256 + ExSensorIdByte[2] * 256 + ExSensorIdByte[3]);
                }
            }
        }

        /// <summary>
        /// 保存并启用输入的客户码
        /// </summary>
        /// <param name=""></param>
        private void SaveExpCustomer()
        {
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
        }

        /// <summary>
        /// 保存并启用输入的Sensor ID序列和客户码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveExpSensorId();

            textBlock_ExpSensorIdLen.Text = ExSensorIdLen.ToString("G");

            SaveExpCustomer();
        }

        /// <summary>
        /// 判断输入的Sensor ID是否是期望的Sensor ID
        /// </summary>
        /// <param name="SID"></param>
        /// <returns></returns>
        private bool IsExpSensorId(UInt32 SID)
        {
            if(ExSensorIdLen == 0)
            {
                return true;
            }

            for(UInt16 iCnt = 0; iCnt < ExSensorIdLen; iCnt++)
            {
                if(SID == ExSensorIdBuf[iCnt])
                {
                    return true;
                }
            }

            return false;
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
                    string Cmd = "CE 09 A0 01 FF 19 CF 0E 40 00 01 00 00 EC";
                    comport.SendCommand(Cmd);

                    comport.ClosePort();
                    btnOpenComPort.Content = "Open";
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
        private Int16 HandleSensorData(Device device) {

            if (isDesDeviceType(device.DeviceTypeV) == false)
            {
                return -1;
            }

            // 接收到数据的设备类型
            byte Cmd = device.Pattern;
            byte STP1 = device.STP;

            if (Cmd == 0x01 || Cmd == 0x02 || Cmd == 0x03 || Cmd == 0x04)
            {
                // 接收到数据的客户码
                byte[] RxCustomerByte = CommArithmetic.HexStringToByteArray(device.CustomerS);
                UInt16 RxCustomer = (UInt16)(RxCustomerByte[0] * 256 + RxCustomerByte[1]);
                if (ExCustomer != 0 && ExCustomer != RxCustomer && RxCustomer != 0)
                {
                    return -1;
                }

                // 接收到数据的Sensor ID
                byte[] RxSensorIdByte = CommArithmetic.HexStringToByteArray(device.DeviceMacS);
                UInt32 RxSensorId = (UInt32)(RxSensorIdByte[0] * 256 * 256 * 256 + RxSensorIdByte[1] * 256 * 256 + RxSensorIdByte[2] * 256 + RxSensorIdByte[3]);
                if (IsExpSensorId(RxSensorId) == false)
                {
                    return -2;
                }

                if (STP1 == 0xEA)
                {
                    //显示数据
                    if (M1.isDeviceType(device.DeviceTypeV) == true)
                    {
                        M1 ThisDevice = (M1)device;
                        if (ThisDevice.GetDataPktType() == Device.DataPktType.SensorDataMax31855Debug)
                        {
                            device.DisplayID = TableLineOfMAX31855;
                            if (++TableLineOfMAX31855 == 0)
                            {
                                TableLineOfMAX31855++;
                            }

                            SensorDataOfMAX31855.Add(ThisDevice);
                        }
                        else
                        {
                            device.DisplayID = TableLineOfM1;
                            if (++TableLineOfM1 == 0)
                            {
                                TableLineOfM1++;
                            }

                            SensorDataOfM1.Add(ThisDevice);
                        }                        
                    }
                    else if (M5.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfM5;
                        if (++TableLineOfM5 == 0)
                        {
                            TableLineOfM5++;
                        }

                        SensorDataOfM5.Add((M5)device);
                    }
                    else if (M20.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfM20;
                        if (++TableLineOfM20 == 0)
                        {
                            TableLineOfM20++;
                        }

                        SensorDataOfM20.Add((M20)device);
                    }
                    else if (M9.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfM9;
                        if (++TableLineOfM9 == 0)
                        {
                            TableLineOfM9++;
                        }

                        SensorDataOfM9.Add((M9)device);
                    }
                    else if (M40.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfM40;
                        if (++TableLineOfM40 == 0)
                        {
                            TableLineOfM40++;
                        }

                        SensorDataOfM40.Add((M40)device);
                    }
                    else if (ACO2.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfACO2;
                        if (++TableLineOfACO2 == 0)
                        {
                            TableLineOfACO2++;
                        }

                        SensorDataOfACO2.Add((ACO2)device);
                    }
                    else if (SK.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfSK;
                        if (++TableLineOfSK == 0)
                        {
                            TableLineOfSK++;
                        }

                        SensorDataOfSK.Add((SK)device);
                    }
                    else if (AO2.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfAO2;
                        if (++TableLineOfAO2 == 0)
                        {
                            TableLineOfAO2++;
                        }

                        SensorDataOfAO2.Add((AO2)device);
                    }
                    else if (L1.isDeviceType(device.DeviceTypeV) == true)
                    {
                        device.DisplayID = TableLineOfL1;
                        if (++TableLineOfL1 == 0)
                        {
                            TableLineOfL1++;
                        }

                        SensorDataOfL1.Add((L1)device);
                    }
                }
                else if (STP1 == 0xAE)
                {
                    device.DisplayID = TableLineOfAck;
                    if (++TableLineOfAck == 0)
                    {
                        TableLineOfAck++;
                    }

                    GroupFeedBack.Add((M1)device);
                }
            }

            return 0;
        }

        /// <summary>
        /// 处理接收到的串口数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            // 在Log中体现
            Dispatcher.BeginInvoke(new Action(delegate
            {
                // 控制台里显示Log
                if (e.ReceivedBytes.Length != 0 && chkLockLog.IsChecked == false)
                {
                    txtConsole.Text = Logger.GetTimeString() + "\t" + CommArithmetic.ToHexString(e.ReceivedBytes) + "\r\n" + txtConsole.Text;

                    UInt16 ConsoleMaxLine = Convert.ToUInt16(txtLogLineLimit.Text);
                    if (txtConsole.LineCount > ConsoleMaxLine)
                    {
                        int start = txtConsole.GetCharacterIndexFromLineIndex(ConsoleMaxLine);  // 末尾行第一个字符的索引
                        int length = txtConsole.GetLineLength(ConsoleMaxLine);                  // 末尾行字符串的长度
                        txtConsole.Select(start, start + length);                               // 选中末尾一行
                        txtConsole.SelectedText = "END";                            
                    }

                }               

                ObservableCollection<Device> devices = DeviceFactory.CreateDevices(e.ReceivedBytes);

                for (int iCnt = 0; iCnt < devices.Count; iCnt++)
                {
                    //通用有效性验证

                    if (devices[iCnt].DeviceMacS == null)
                    {
                        continue;
                    }

                    HandleSensorData(devices[iCnt]);                                  
                }
            }));
        }

        /// <summary>
        /// 关闭程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            if(comport == null)
            {
                return;
            }

            if(comport.IsOpen() == true)
            {
                comport.ClosePort();
            }                        
        }

        /// <summary>
        /// 清空Console中的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogClear_Click(object sender, RoutedEventArgs e)
        {
            txtConsole.Text = "";
        }
    }
}
