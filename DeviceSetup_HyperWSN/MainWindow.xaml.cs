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
using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;
using System.Timers;
using System.Windows.Threading;
using System.Configuration;

namespace DeviceSetup_HyperWSN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper SerialPort;

        Timer TimeoutTimer;
        bool TimeoutElapsed = true;                // 是否已经超时

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = ConfigurationManager.AppSettings["Title"] + " v" +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    
            FindComport();
            DisableControl();

            dtimer = new DispatcherTimer();
            dtimer.Interval = TimeSpan.FromSeconds(5);
            dtimer.Tick += Dtimer_Tick; ;

            TimeoutTimer = new Timer();
            TimeoutTimer.Elapsed += Timeout_Elapsed;
            TimeoutTimer.Interval = 3050;
            TimeoutTimer.Enabled = false;
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

        private void btnFindComport_Click(object sender, RoutedEventArgs e)
        {
            FindComport();
        }

        private void btnOpenComport_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenComport.Content.ToString() == "Open")
            {
                SerialPort = new SerialPortHelper();
                SerialPort.SerialPortReceived += Comport_SerialPortReceived;
                string portname = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());
                SerialPort.InitCOM(portname);
                if (SerialPort.OpenPort())
                {
                    btnStartMonitor.IsEnabled = true;
                    btnOpenComport.Content = "Close";
                }else
                {
                    MessageBox.Show(SerialPort.ExceptString);
                }
            }
            else
            {
                if (SerialPort != null)
                {
                    SerialPort.ClosePort();
                    btnOpenComport.Content = "Open";
                    DisableControl();
                }
            }
        }

        private void DisableControl()
        {
            btnStartMonitor.IsEnabled = false;
            btnStopMonitor.IsEnabled = false;
        }

        /// <summary>
        /// 显示M9的灵敏度
        /// </summary>
        /// <param name="RxMoveDetectThr"></param>
        /// <param name="RxStaticDetectThr"></param>
        /// <returns></returns>
        private Int16 DisplaySensitivityOfM9(UInt16 RxMoveDetectThr, UInt16 RxStaticDetectThr)
        {
            UInt16 MoveDetectThr = 0;
            UInt16 StaticDetectThr = 0;

            for (byte Sensitivity = 0; Sensitivity < 100; Sensitivity++)
            {
                MoveDetectThr = Convert.ToUInt16(ConfigurationManager.AppSettings["M9_MoveDetectThr_" + Sensitivity.ToString()]);
                StaticDetectThr = Convert.ToUInt16(ConfigurationManager.AppSettings["M9_StaticDetectThr_" + Sensitivity.ToString()]);

                if (MoveDetectThr == 0 || StaticDetectThr == 0)
                {
                    break;
                }

                if(RxMoveDetectThr != MoveDetectThr || RxStaticDetectThr != StaticDetectThr)
                {
                    continue;
                }

                tbkSensitivityDetailOfM9.Text = "";
                tbxSensitivityOfM9.Text = Sensitivity.ToString();
                return 0;
            }

            // 定义的灵敏度列表里没有与之相匹配的内容，则显示详细内容
            tbkSensitivityDetailOfM9.Text = "运动检测阈值：" + RxMoveDetectThr.ToString() + "mg, 静止检测阈值：" + RxStaticDetectThr.ToString() + "mg;";
            tbxSensitivityOfM9.Text = "";

            return 1;
        }

        /// <summary>
        /// 收到Comport反馈的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定
                //收到数据后的几种情况
                // 未收到任何反馈
                if (e.ReceivedBytes == null)
                {
                    return;
                }

                //
                if (e.ReceivedBytes.Length == 8)
                {
                    //收到更新反馈报
                    if (e.ReceivedBytes[2] == 0xA1 || e.ReceivedBytes[2] == 0xA2
                    || e.ReceivedBytes[2] == 0xA3 || e.ReceivedBytes[2] == 0xA4)
                    {
                        //立即进入读取状态
                        btnStartMonitor_Click(this, null);
                    }

                    return;
                }
            }));

            Dispatcher.BeginInvoke(new Action(delegate
            {
                ConsoleLog("RX", e.ReceivedBytes, 0, (UInt16) e.ReceivedBytes.Length);

                bool ReceivedOk = false;

                for (UInt16 iX = 0; iX < e.ReceivedBytes.Length; iX++)
                {
                    // 判断是否是上电自检数据包
                    Int16 Error = Device.IsPowerOnSelfTestPktFromUsbToPc(e.ReceivedBytes, iX);
                    if (Error < 0)
                    {
                        continue;
                    }

                    // 根据不同的设备类型来实例
                    Device.DeviceType deviceType = (Device.DeviceType)Error;
                    switch (deviceType)
                    {
                        case Device.DeviceType.M1:
                        case Device.DeviceType.M1_NTC:
                        case Device.DeviceType.M1_Beetech:
                        case Device.DeviceType.M2:
                        case Device.DeviceType.S1P:
                        case Device.DeviceType.ZQM1:
                        case Device.DeviceType.M10:
                        case Device.DeviceType.M6:
                        case Device.DeviceType.M20:
                        case Device.DeviceType.M1X:
                            {
                                M1 aM1 = new M1(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc, deviceType);
                                if (aM1 != null)
                                {
                                    TableOfM1.IsSelected = true;
                                    StackOfM1.DataContext = aM1;
                                }
                                break;
                            }
                        case Device.DeviceType.SK:
                            {
                                SK aSK = new SK(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc);
                                if (aSK != null)
                                {
                                    TableOfM4.IsSelected = true;
                                    StackOfSK.DataContext = aSK;
                                    ReceivedOk = true;
                                }
                                break;
                            }
                        case Device.DeviceType.ESK:
                            {
                                ESK aESK = new ESK(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc);
                                if (aESK != null)
                                {
                                    TableOfEsk.IsSelected = true;
                                    StackOfEsk.DataContext = aESK;
                                    ReceivedOk = true;
                                }
                                break;
                            }
                        case Device.DeviceType.AO2:
                            {
                                AO2 aAO2 = new AO2(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc, Device.DeviceType.AO2);
                                if (aAO2 != null)
                                {
                                    TableOfAO2.IsSelected = true;
                                    StackOfAO2.DataContext = aAO2;
                                    ReceivedOk = true;
                                }
                                break;
                            }
                        case Device.DeviceType.M9:
                            {
                                M9 aM9 = new M9(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc);
                                if (aM9 != null)
                                {
                                    TableOfM9.IsSelected = true;
                                    StackOfM9.DataContext = aM9;

                                    // 显示灵敏度
                                    DisplaySensitivityOfM9(aM9.MoveDetectThr, aM9.StaticDetectThr);

                                    if (cbxAlertCfgLockOfM9.IsChecked == false)
                                    {
                                        if (0 != (aM9.AlertCfg & 0x01))
                                        {   // 静止报警
                                            cbxAlertCfgStaticOfM9.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgStaticOfM9.IsChecked = false;
                                        }

                                        if (0 != (aM9.AlertCfg & 0x02))
                                        {   // 运动报警
                                            cbxAlertCfgMoveOfM9.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgMoveOfM9.IsChecked = false;
                                        }

                                        if (0 != (aM9.AlertCfg & 0x04))
                                        {   // 动->静报警
                                            cbxAlertCfgMoveStaticOfM9.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgMoveStaticOfM9.IsChecked = false;
                                        }

                                        if (0 != (aM9.AlertCfg & 0x08))
                                        {   // 静->动报警
                                            cbxAlertCfgStaticMoveOfM9.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgStaticMoveOfM9.IsChecked = false;
                                        }

                                        if (0 != (aM9.AlertCfg & 0x10))
                                        {   // 异常报警
                                            cbxAlertCfgExceptionOfM9.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgExceptionOfM9.IsChecked = false;
                                        }
                                    }
                                }
                                break;
                            }
                        case Device.DeviceType.M40:
                            {
                                M40 aM40 = new M40(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc);
                                if (aM40 != null)
                                {
                                    TableOfM40.IsSelected = true;
                                    StackOfM40.DataContext = aM40;

                                    if (cbxAlertCfgLockOfM40.IsChecked == false)
                                    {
                                        if (0 != (aM40.AlertCfg & 0x01))
                                        {   // 关门报警
                                            cbxAlertCfgClosedOfM40.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgClosedOfM40.IsChecked = false;
                                        }

                                        if (0 != (aM40.AlertCfg & 0x02))
                                        {   // 开门报警
                                            cbxAlertCfgOpenedOfM40.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgOpenedOfM40.IsChecked = false;
                                        }

                                        if (0 != (aM40.AlertCfg & 0x04))
                                        {   // 开->关报警
                                            cbxAlertCfgOpenedClosedOfM40.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgOpenedClosedOfM40.IsChecked = false;
                                        }

                                        if (0 != (aM40.AlertCfg & 0x08))
                                        {   // 关->开报警
                                            cbxAlertCfgClosedOpenedOfM40.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgClosedOpenedOfM40.IsChecked = false;
                                        }

                                        if (0 != (aM40.AlertCfg & 0x10))
                                        {   // 异常报警
                                            cbxAlertCfgExceptionOfM40.IsChecked = true;
                                        }
                                        else
                                        {
                                            cbxAlertCfgExceptionOfM40.IsChecked = false;
                                        }
                                    }
                                }
                                break;
                            }
                        case Device.DeviceType.L1:
                            {
                                L1 aL1 = new L1(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc);
                                if (aL1 != null)
                                {
                                    TableOfL1.IsSelected = true;
                                    StackOfL1.DataContext = aL1;
                                }
                                break;
                            }
                        case Device.DeviceType.WP:
                            {
                                WP aWP = new WP(e.ReceivedBytes, iX, Device.DataPktType.SelfTestFromUsbToPc);
                                if (aWP != null)
                                {
                                    TableOfWP.IsSelected = true;
                                    StackOfWP.DataContext = aWP;
                                }
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }

                if (ReceivedOk == false)
                {
                    ReceivedOk = false;
                }

            }));
        }

        /// <summary>
        /// 开始接收上电自检数据包
        /// </summary>
        /// <returns></returns>
        private Int16 StartReceive()
        {
            try
            {
                byte[] TxBuf = new byte[16];
                UInt16 TxLen = 0;

                bool ExistRssiThr = false;
                Int16 RssiThr = 0;

                if (tbxRssiThr.Text == "")
                {
                    ExistRssiThr = false;
                }
                else
                {
                    ExistRssiThr = true;
                    RssiThr = Convert.ToInt16(tbxRssiThr.Text);
                }

                // Start
                TxBuf[TxLen++] = 0xCE;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x01;

                // USB Protocol
                if (ExistRssiThr == false)
                {
                    TxBuf[TxLen++] = 0x01;
                }
                else
                {
                    TxBuf[TxLen++] = 0x02;
                }

                // Timeout
                if (tbxStartRxTimeoutThr.Text == string.Empty)
                {
                    TxBuf[TxLen++] = 0x03;
                }
                else
                {
                    byte TimeoutS = Convert.ToByte(tbxStartRxTimeoutThr.Text);
                    TxBuf[TxLen++] = (byte)(TimeoutS > 10 ? 10 : TimeoutS);
                }                

                if (ExistRssiThr == false)
                {

                }
                else
                {
                    // RSSI 阈值
                    TxBuf[TxLen++] = (byte)RssiThr;

                    // 保留
                    TxBuf[TxLen++] = 0x00;
                    TxBuf[TxLen++] = 0x00;
                    TxBuf[TxLen++] = 0x00;
                    TxBuf[TxLen++] = 0x00;
                }

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xEC;

                // 重写长度位
                TxBuf[1] = (byte)(TxLen - 5);

                SerialPort_Send(TxBuf, 0, TxLen);

                return 0;
            }
            catch (Exception)
            {
                return -1000;
            }
        }


        /// <summary>
        /// 启动串口监听，如果选中循环监听，则持续监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (SerialPort == null)
            {
                return;
            }

            if (TimeoutElapsed == false)
            {
                return;
            }

            btnClear_Click(sender, e);              // 清空上次的结果

            StartReceive();                         // 开始监听

            if (TimeoutTimer != null)
            {
                TimeoutElapsed = false;
                TimeoutTimer.Enabled = true;
            }

            //btnStartMonitor.IsEnabled = false;
            btnStopMonitor.IsEnabled = true;
        }

        private void Timeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeoutTimer.Enabled = false;
            TimeoutElapsed = true;
        }

        private void btnStopMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (TimeoutTimer != null)
            {
                TimeoutTimer.Enabled = false;
                TimeoutElapsed = true;
            }

            btnStartMonitor.IsEnabled = true;
            btnStopMonitor.IsEnabled = false;
        }

        /// <summary>
        /// 更新工厂信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateFactoryOfM1_Click(object sender, RoutedEventArgs e)
        {
            tbkResultOfM1.Text = "";

            M1 updateDevice = new M1();
            try
            {
                updateDevice.DeviceTypeS = tbxDeviceNameOfM1.Text;
                updateDevice.DeviceMacS = tbxDeviceMacOfM1.Text;
                updateDevice.DeviceMacNewS = tbxNewDeviceMacOfM1.Text;
                updateDevice.DeviceMacNewS = updateDevice.DeviceMacNewS.Replace("\r", "");
                updateDevice.DeviceMacNewS = updateDevice.DeviceMacNewS.Replace("\n", "");
                updateDevice.HwRevisionS = tbxNewHwRevisionOfM1.Text;

                byte[] updateCommand = updateDevice.UpdateFactory();
                string updateString = CommArithmetic.ToHexString(updateCommand);
                SerialPort.Send(updateCommand);
                System.Threading.Thread.Sleep(200);     //界面会卡
            }
            catch (Exception ex)
            {
                tbkResultOfM1.Text = "参数错误" + ex.Message;

                if (tbkResultOfM1.Foreground == System.Windows.Media.Brushes.Red)
                {
                    tbkResultOfM1.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (tbkResultOfM1.Foreground == System.Windows.Media.Brushes.Green)
                {
                    tbkResultOfM1.Foreground = System.Windows.Media.Brushes.Blue;
                }
                else
                {
                    tbkResultOfM1.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
        }

        /// <summary>
        /// M1: 修改用户配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteUserCfgOfM1()
        {
            byte[] TxBuf = new byte[38];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // 设备类型
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceTypeOfM1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            Device.DeviceType deviceType = (Device.DeviceType)ByteBufTmp[0];

            // Protocol
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxProtocolOfM1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -2;
            }
            byte Protocol = ByteBufTmp[0];

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA2;

            // USB Protocol
            if (Protocol == 0x02)
            {
                TxBuf[TxLen++] = 0x01;
            }
            else
            {
                TxBuf[TxLen++] = 0x02;
            }

            // 设备类型
            TxBuf[TxLen++] = (byte)deviceType;

            // Protocol
            TxBuf[TxLen++] = Protocol;

            // Sensor Mac
            if (cbxIsAllOfM1.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM1.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Customer
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfM1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfM1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            TxBuf[TxLen++] = Convert.ToByte(tbxNewCategoryOfM1.Text);

            // Pattern
            TxBuf[TxLen++] = Convert.ToByte(tbxNewPatternOfM1.Text);

            // bps
            TxBuf[TxLen++] = Convert.ToByte(tbxNewBpsOfM1.Text);

            // Tx Power
            Int16 txPower = Convert.ToInt16(tbxNewTxPowerOfM1.Text);
            TxBuf[TxLen++] = (byte)txPower;

            // channel
            TxBuf[TxLen++] = Convert.ToByte(tbxNewChannelOfM1.Text);

            if (Protocol == 0x02)
            {
                // 温度补偿 
                double tempCompF = Convert.ToDouble(tbxNewTemperatureCompensationOfM1.Text);    // 单位：℃
                Int16 tempComp = (Int16)Math.Round(tempCompF * 100.0f);                         // 单位：0.01℃
                TxBuf[TxLen++] = (byte)((tempComp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((tempComp & 0x00FF) >> 0);

                if (deviceType == Device.DeviceType.M2)
                {   // M2数据包里没有湿度补偿

                }
                else
                {
                    // 湿度补偿
                    double humCompF = Convert.ToDouble(tbxNewHumidityCompensationOfM1.Text);        // 单位：%
                    Int16 humComp = (Int16)Math.Round(humCompF * 100.0f);                           // 单位：0.01%
                    TxBuf[TxLen++] = (byte)((humComp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((humComp & 0x00FF) >> 0);
                }

                // MaxLength
                TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfM1.Text);
            }
            else if (Protocol == 0x03)
            {
                // MaxLength
                TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfM1.Text);

                // 日期和时间
                tbxCalendarOfM1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfM1.Text);
                ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // 温度补偿 
                double tempCompF = Convert.ToDouble(tbxNewTemperatureCompensationOfM1.Text);    // 单位：℃
                Int16 tempComp = (Int16)Math.Round(tempCompF * 100.0f);                         // 单位：0.01℃
                TxBuf[TxLen++] = (byte)((tempComp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((tempComp & 0x00FF) >> 0);

                // 湿度补偿
                double humCompF = Convert.ToDouble(tbxNewHumidityCompensationOfM1.Text);        // 单位：%
                Int16 humComp = (Int16)Math.Round(humCompF * 100.0f);                           // 单位：0.01%
                TxBuf[TxLen++] = (byte)((humComp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((humComp & 0x00FF) >> 0);

                // Reserved
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// 更新用户配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateUserOfM1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteUserCfgOfM1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// M1: 修改应用配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteAppCfgOfM1()
        {
            byte[] TxBuf = new byte[50];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // 设备类型
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceTypeOfM1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            Device.DeviceType deviceType = (Device.DeviceType)ByteBufTmp[0];

            // Protocol
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxProtocolOfM1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -2;
            }
            byte Protocol = ByteBufTmp[0];

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA3;

            // USB Protocol
            if (Protocol == 0x02)
            {
                TxBuf[TxLen++] = 0x01;
            }
            else
            {
                TxBuf[TxLen++] = 0x02;
            }            

            // 设备类型
            TxBuf[TxLen++] = (byte)deviceType;

            // Protocol
            TxBuf[TxLen++] = Protocol;

            // Sensor Mac
            if (cbxIsAllOfM1.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM1.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            if (Protocol == 0x02)
            {
                // 采集间隔
                UInt16 Interval = Convert.ToUInt16(tbxNewIntervalOfM1.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 日期和时间
                tbxCalendarOfM1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfM1.Text);
                ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // 采发倍数
                TxBuf[TxLen++] = Convert.ToByte(tbxNewSampleSendOfM1.Text);

                if (deviceType == Device.DeviceType.M2)
                {
                    // 温度预警上限
                    double tempF = Convert.ToDouble(tbxNewTemperatureWarnHighOfM1.Text);        // 单位：℃
                    Int16 temp = (Int16)Math.Round(tempF * 100.0f);                             // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                    // 温度预警下限
                    tempF = Convert.ToDouble(tbxNewTemperatureWarnLowOfM1.Text);                // 单位：℃
                    temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                    // 温度阈值上限
                    tempF = Convert.ToDouble(tbxNewTemperatureAlertHighOfM1.Text);              // 单位：℃
                    temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                    // 温度阈值下限
                    tempF = Convert.ToDouble(tbxNewTemperatureAlertLowOfM1.Text);               // 单位：℃
                    temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);
                }
                else
                {
                    // 温度预警上限
                    double tempF = Convert.ToDouble(tbxNewTemperatureWarnHighOfM1.Text);        // 单位：℃
                    Int16 temp = (Int16)Math.Round(tempF * 100.0f);                             // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                    // 温度预警下限
                    tempF = Convert.ToDouble(tbxNewTemperatureWarnLowOfM1.Text);                // 单位：℃
                    temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                    // 湿度预警上限
                    double humF = Convert.ToDouble(tbxNewHumidityWarnHighOfM1.Text);            // 单位：%
                    UInt16 hum = (UInt16)Math.Round(humF * 100.0f);                             // 单位：0.01%
                    TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

                    // 湿度预警下限
                    humF = Convert.ToDouble(tbxNewHumidityWarnLowOfM1.Text);                    // 单位：%
                    hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
                    TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

                    // 温度阈值上限
                    tempF = Convert.ToDouble(tbxNewTemperatureAlertHighOfM1.Text);              // 单位：℃
                    temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                    // 温度阈值下限
                    tempF = Convert.ToDouble(tbxNewTemperatureAlertLowOfM1.Text);               // 单位：℃
                    temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                    TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                    // 湿度阈值上限
                    humF = Convert.ToDouble(tbxNewHumidityAlertHighOfM1.Text);                  // 单位：%
                    hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
                    TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

                    // 湿度阈值下限
                    humF = Convert.ToDouble(tbxNewHumidityAlertLowOfM1.Text);                   // 单位：%
                    hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
                    TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                    TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);
                }
            }
            else if (Protocol == 0x03)
            {
                // 日期和时间
                tbxCalendarOfM1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfM1.Text);
                ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // 采集间隔
                UInt16 Interval = Convert.ToUInt16(tbxNewIntervalOfM1.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 正常间隔
                Interval = Convert.ToUInt16(tbxNewNormalIntervalOfM1.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 预警间隔
                Interval = Convert.ToUInt16(tbxNewWarnIntervalOfM1.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 报警间隔
                Interval = Convert.ToUInt16(tbxNewAlertIntervalOfM1.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 温度预警上限
                double tempF = Convert.ToDouble(tbxNewTemperatureWarnHighOfM1.Text);        // 单位：℃
                Int16 temp = (Int16)Math.Round(tempF * 100.0f);                             // 单位：0.01℃
                TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                // 温度预警下限
                tempF = Convert.ToDouble(tbxNewTemperatureWarnLowOfM1.Text);                // 单位：℃
                temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                // 湿度预警上限
                double humF = Convert.ToDouble(tbxNewHumidityWarnHighOfM1.Text);            // 单位：%
                UInt16 hum = (UInt16)Math.Round(humF * 100.0f);                             // 单位：0.01%
                TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

                // 湿度预警下限
                humF = Convert.ToDouble(tbxNewHumidityWarnLowOfM1.Text);                    // 单位：%
                hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
                TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

                // 温度阈值上限
                tempF = Convert.ToDouble(tbxNewTemperatureAlertHighOfM1.Text);              // 单位：℃
                temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                // 温度阈值下限
                tempF = Convert.ToDouble(tbxNewTemperatureAlertLowOfM1.Text);               // 单位：℃
                temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
                TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

                // 湿度阈值上限
                humF = Convert.ToDouble(tbxNewHumidityAlertHighOfM1.Text);                  // 单位：%
                hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
                TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

                // 湿度阈值下限
                humF = Convert.ToDouble(tbxNewHumidityAlertLowOfM1.Text);                   // 单位：%
                hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
                TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

                // Reserved
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }            

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateApplicationOfM1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteAppCfgOfM1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }

            /*
            M1 updateDevice = new M1();

            try
            {
                updateDevice.DeviceMacS = tbxDeviceMacOfM1.Text;

                if (tbxDeviceNameOfM1.Text == "M1")
                {
                    updateDevice.DeviceTypeS = "51";
                }
                else if (tbxDeviceNameOfM1.Text == "M1P")
                {
                    updateDevice.DeviceTypeS = "53";
                }
                else if (tbxDeviceNameOfM1.Text == "M1_Beetech")
                {
                    updateDevice.DeviceTypeS = "5D";
                }
                else if (tbxDeviceNameOfM1.Text == "M2")
                {
                    updateDevice.DeviceTypeS = "57";
                }
                else if (tbxDeviceNameOfM1.Text == "M1_ZheQin")
                {
                    updateDevice.DeviceTypeS = "7D";
                }

                updateDevice.ProtocolVersion = 2;
                updateDevice.Interval = Convert.ToUInt16(txtNewInterval.Text);
                updateDevice.SampleSend = Convert.ToByte(txtNewTXTimers.Text);
                tbxCalendarOfM1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                updateDevice.Calendar = Convert.ToDateTime(tbxCalendarOfM1.Text);

                updateDevice.TempWarnLow = Convert.ToDouble(txtTemperatureInfoLow.Text);
                updateDevice.TempWarnHigh = Convert.ToDouble(txtTemperatureInfoHigh.Text);
                updateDevice.TempAlertLow = Convert.ToDouble(txtTemperatureWarnLow.Text);
                updateDevice.TempAlertHigh = Convert.ToDouble(txtTemperatureWarnHigh.Text);

                updateDevice.HumWarnLow = Convert.ToDouble(txtHumidityInfoLow.Text);
                updateDevice.HumWarnHigh = Convert.ToDouble(txtHumidityInfoHigh.Text);
                updateDevice.HumAlertLow = Convert.ToDouble(txtHumidityWarnLow.Text);
                updateDevice.HumAlertHigh = Convert.ToDouble(txtHumidityWarnHigh.Text);

                if (tbxProtocolOfM1.Text == "3")
                {
                    updateDevice.ProtocolVersion = 3;
                    updateDevice.Interval = Convert.ToUInt16(txtNewInterval.Text);
                    updateDevice.NormalInterval = Convert.ToUInt16(txtIntervalNormal.Text);
                    updateDevice.WarnInterval = Convert.ToUInt16(txtIntervalWarning.Text);
                    updateDevice.AlertInterval = Convert.ToUInt16(txtIntervalAlarm.Text);
                }

                byte[] updateCommand = updateDevice.UpdateApplicationConfig();

                if (cbxIsAllOfM1.IsChecked == true)
                {
                    updateCommand[6] = 0x00;
                    updateCommand[7] = 0x00;
                    updateCommand[8] = 0x00;
                    updateCommand[9] = 0x00;
                }

                string updateString = CommArithmetic.ToHexString(updateCommand);

                // monitorTimer.Enabled = false;

                //System.Threading.Thread.Sleep(2000); //界面会卡

                SerialPort.SendCommand(updateCommand);

                System.Threading.Thread.Sleep(250); //界面会卡

                btnStartMonitor_Click(this, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
            */
        }

        /// <summary>
        /// M1: 删除历史数据
        /// </summary>
        /// <returns></returns>
        private Int16 DeleteHistoryOfM1()
        {
            byte[] TxBuf = new byte[21];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA4;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceTypeOfM1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            if (cbxIsAllOfM1.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM1.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Front
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Rear
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnDeleteDataOfM1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteHistoryOfM1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
        }

        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(delegate (object f)
                {
                    ((DispatcherFrame)f).Continue = false;

                    return null;
                }
                    ), frame);
            Dispatcher.PushFrame(frame);
        }


        /// <summary>
        /// M4: 发送串口指令，修改出厂配置
        /// </summary>
        private Int16 WriteFactoryCfgOfM4()
        {
            byte[] TxBuf = new byte[25];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA1;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = 0x58;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM4.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // New Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMacOfM4.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Hardware Revision
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewHwRevisionOfM4.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M4: 修改出厂配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateFactoryOfM4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteFactoryCfgOfM4();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// M4: 修改用户
        /// </summary>
        /// <returns></returns>
        private Int16 WriteUserCfgOfM4()
        {
            byte[] TxBuf = new byte[36];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA2;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;          // 由于老版USB修改工具的问题，此USB Porotocol必须是1，否则修改失败；

            // 设备类型
            TxBuf[TxLen++] = 0x58;

            // Protocol
            byte Protocol = Convert.ToByte(tbxProtocolOfM4.Text);
            TxBuf[TxLen++] = Protocol;

            // Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM4.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Customer
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfM4.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfM4.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            TxBuf[TxLen++] = Convert.ToByte(tbxNewCategoryOfM4.Text);

            // Pattern
            TxBuf[TxLen++] = Convert.ToByte(tbxNewPatternOfM4.Text);

            // Bps
            TxBuf[TxLen++] = Convert.ToByte(tbxNewBpsOfM4.Text);

            // TX Power
            Int16 txPower = Convert.ToInt16(tbxNewTxPowerOfM4.Text);
            TxBuf[TxLen++] = (byte)txPower;

            // Channel
            TxBuf[TxLen++] = Convert.ToByte(tbxNewChannelOfM4.Text);

            if (Protocol == 0x02)
            {
                // 功率补偿
                Int16 powerComp = Convert.ToInt16(tbxNewPowerCompensationOfM4.Text);       // 单位：V；
                TxBuf[TxLen++] = (byte)((powerComp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((powerComp & 0x00FF) >> 0);

                // 电压补偿
                Int16 voltComp = Convert.ToInt16(tbxNewVoltageCompensationOfM4.Text);       // 单位：V；
                TxBuf[TxLen++] = (byte)((voltComp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((voltComp & 0x00FF) >> 0);

                // 存储容量
                TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfM4.Text);
            }
            else if (Protocol == 0x03)
            {
                // 存储容量
                TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfM4.Text);

                // 日期和时间
                tbxCalendarOfM4.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime NowCalendar = Convert.ToDateTime(tbxCalendarOfM4.Text);
                ByteBufTmp = MyCustomFxn.DataTimeToByteArray(NowCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // 功率补偿
                Int16 powerComp = Convert.ToInt16(tbxNewPowerCompensationOfM4.Text);       // 单位：V；
                TxBuf[TxLen++] = (byte)((powerComp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((powerComp & 0x00FF) >> 0);

                // 电压补偿
                Int16 voltComp = Convert.ToInt16(tbxNewVoltageCompensationOfM4.Text);       // 单位：V；
                TxBuf[TxLen++] = (byte)((voltComp & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((voltComp & 0x00FF) >> 0);

                // 保留
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                return -3;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M4: 修改用户配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateUserSK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteUserCfgOfM4();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// M4: 修改应用配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteAppCfgOfM4()
        {
            byte[] TxBuf = new byte[58];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA3;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;      // 由于老版USB修改工具的问题，此USB Porotocol必须是1，否则修改失败；

            // 设备类型
            TxBuf[TxLen++] = 0x58;

            // Protocol
            byte Protocol = Convert.ToByte(tbxProtocolOfM4.Text);
            TxBuf[TxLen++] = Protocol;

            // Sensor Mac
            if (cbxIsAllOfM4.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM4.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            if (Protocol == 0x02)
            {
                // 采集间隔
                UInt16 Interval = Convert.ToUInt16(tbxNewIntervalOfM4.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 日期和时间
                tbxCalendarOfM4.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfM4.Text);
                ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // Sample Send
                TxBuf[TxLen++] = Convert.ToByte(tbxNewSampleSendOfM4.Text);

                // 功率预警上限
                UInt16 power = Convert.ToUInt16(tbxNewPowerWarnHighOfM4.Text);              // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 功率预警下限
                power = Convert.ToUInt16(tbxNewPowerWarnLowOfM4.Text);                      // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 电压预警上限
                UInt16 volt = Convert.ToUInt16(tbxNewVoltageWarnHighOfM4.Text);             // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);

                // 电压预警下限
                volt = Convert.ToUInt16(tbxNewVoltageWarnLowOfM4.Text);                     // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);

                // 功率阈值上限
                power = Convert.ToUInt16(tbxNewPowerAlertHighOfM4.Text);                    // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 功率阈值下限
                power = Convert.ToUInt16(tbxNewPowerAlertLowOfM4.Text);                     // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 电压阈值上限
                volt = Convert.ToUInt16(tbxNewVoltageAlertHighOfM4.Text);                   // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);

                // 电压阈值下限
                volt = Convert.ToUInt16(tbxNewVoltageAlertLowOfM4.Text);                    // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);
            }
            else if (Protocol == 0x03)
            {
                // 日期和时间
                tbxCalendarOfM4.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfM4.Text);
                ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // 采集间隔
                UInt16 Interval = Convert.ToUInt16(tbxNewIntervalOfM4.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 正常间隔
                Interval = Convert.ToUInt16(tbxNewIntervalNormalOfM4.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 预警间隔
                Interval = Convert.ToUInt16(tbxNewIntervalWarningOfM4.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 报警间隔
                Interval = Convert.ToUInt16(tbxNewIntervalAlarmOfM4.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 功率预警上限
                UInt16 power = Convert.ToUInt16(tbxNewPowerWarnHighOfM4.Text);              // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 功率预警下限
                power = Convert.ToUInt16(tbxNewPowerWarnLowOfM4.Text);                      // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 电压预警上限
                UInt16 volt = Convert.ToUInt16(tbxNewVoltageWarnHighOfM4.Text);             // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);

                // 电压预警下限
                volt = Convert.ToUInt16(tbxNewVoltageWarnLowOfM4.Text);                     // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);

                // 功率阈值上限
                power = Convert.ToUInt16(tbxNewPowerAlertHighOfM4.Text);                    // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 功率阈值下限
                power = Convert.ToUInt16(tbxNewPowerAlertLowOfM4.Text);                     // 单位：W
                TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

                // 电压阈值上限
                volt = Convert.ToUInt16(tbxNewVoltageAlertHighOfM4.Text);                   // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);

                // 电压阈值下限
                volt = Convert.ToUInt16(tbxNewVoltageAlertLowOfM4.Text);                    // 单位：V
                TxBuf[TxLen++] = (byte)((volt & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((volt & 0x00FF) >> 0);

                // 保留位
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                return -2;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M4: 修改应用配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateApplicationOfM4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteAppCfgOfM4();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Dtimer_Tick(object sender, EventArgs e)
        {
            if (updateIndex == 0)
            {
                btnUpdateApplicationOfM1_Click(this, null);
                updateIndex++;
            }
            else if (updateIndex == 1)
            {
                btnUpdateUserOfM1_Click(this, null);
                updateIndex++;
            }
            else if (updateIndex == 2)
            {
                btnDeleteDataOfM1_Click(this, null);
                updateIndex++;
                dtimer.IsEnabled = false;
            }
        }

        System.Windows.Threading.DispatcherTimer dtimer;
        int updateIndex = 0;

        private void cbSerialPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// M4: 删除历史数据
        /// </summary>
        /// <returns></returns>
        private Int16 DeleteHistoryOfM4()
        {
            byte[] TxBuf = new byte[21];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA4;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = 0x58;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            if (cbxIsAllOfM4.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM4.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Front
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Rear
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M4: 删除历史数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteDataOfM4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteHistoryOfM4();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
        }

        /// <summary>
        /// AO2: 修改出厂配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteFactoryCfgOfAO2()
        {
            byte[] TxBuf = new byte[25];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA1;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = 0x7A;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfAO2.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // New Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMacOfAO2.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Hardware Revision
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewHwRevisionOfAO2.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// AO2: 修改出厂配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateFactoryOfAO2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteFactoryCfgOfAO2();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// AO2: 修改用户配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteUserCfgOfAO2()
        {
            byte[] TxBuf = new byte[38];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA2;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = 0x7A;

            // Protocol
            TxBuf[TxLen++] = 0x03;

            // Sensor Mac
            if (cbxIsAllOfAO2.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfAO2.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Customer
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfAO2.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfAO2.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            TxBuf[TxLen++] = Convert.ToByte(tbxNewCategoryOfAO2.Text);

            // Pattern
            TxBuf[TxLen++] = Convert.ToByte(tbxNewPatternOfAO2.Text);

            // bps
            TxBuf[TxLen++] = Convert.ToByte(tbxNewBpsOfAO2.Text);

            // Tx Power
            Int16 txPower = Convert.ToInt16(tbxNewTxPowerOfAO2.Text);
            TxBuf[TxLen++] = (byte)txPower;

            // channel
            TxBuf[TxLen++] = Convert.ToByte(tbxNewChannelOfAO2.Text);

            // MaxLength
            TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfAO2.Text);

            // 日期和时间
            tbxCalendarOfAO2.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfAO2.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 温度补偿
            double tempCompF = Convert.ToDouble(tbxNewTemperatureCompensationOfAO2.Text);   // 单位：℃
            Int16 tempComp = (Int16)Math.Round(tempCompF * 100.0f);                         // 单位：0.01℃
            TxBuf[TxLen++] = (byte)((tempComp & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((tempComp & 0x00FF) >> 0);

            // 湿度补偿
            double humCompF = Convert.ToDouble(tbxNewHumidityCompensationOfAO2.Text);       // 单位：%
            Int16 humComp = (Int16)Math.Round(humCompF * 100.0f);                           // 单位：0.01%
            TxBuf[TxLen++] = (byte)((humComp & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((humComp & 0x00FF) >> 0);

            // 气体浓度补偿
            double gasCompF = Convert.ToDouble(tbxNewGasCompensationOfAO2.Text);            // 单位：%
            Int32 gasComp = (Int32)Math.Round(gasCompF * 10000.0f);                         // 单位：ppm
            TxBuf[TxLen++] = (byte)((gasComp & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((gasComp & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((gasComp & 0x000000FF) >> 0);

            // Reserved
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// AO2: 修改用户配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateUserOfAO2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteUserCfgOfAO2();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
        }

        /// <summary>
        /// AO2: 修改应用配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteAppCfgOfAO2()
        {
            byte[] TxBuf = new byte[58];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA3;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = 0x7A;

            // Protocol
            TxBuf[TxLen++] = 0x03;

            // Sensor Mac
            if (cbxIsAllOfAO2.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfAO2.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // 日期和时间
            tbxCalendarOfAO2.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfAO2.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 采集间隔
            UInt16 Interval = Convert.ToUInt16(tbxNewIntervalOfAO2.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 正常间隔
            Interval = Convert.ToUInt16(tbxNewNormalIntervalOfAO2.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 预警间隔
            Interval = Convert.ToUInt16(tbxNewWarnIntervalOfAO2.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 报警间隔
            Interval = Convert.ToUInt16(tbxNewAlertIntervalOfAO2.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 温度预警上限
            double tempF = Convert.ToDouble(tbxNewTemperatureWarnHighOfAO2.Text);       // 单位：℃
            Int16 temp = (Int16)Math.Round(tempF * 100.0f);                             // 单位：0.01℃
            TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

            // 温度预警下限
            tempF = Convert.ToDouble(tbxNewTemperatureWarnLowOfAO2.Text);               // 单位：℃
            temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
            TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

            // 湿度预警上限
            double humF = Convert.ToDouble(tbxNewHumidityWarnHighOfAO2.Text);           // 单位：%
            UInt16 hum = (UInt16)Math.Round(humF * 100.0f);                             // 单位：0.01%
            TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

            // 湿度预警下限
            humF = Convert.ToDouble(tbxNewHumidityWarnLowOfAO2.Text);                   // 单位：%
            hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
            TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

            // O2浓度预警上限
            double con = Convert.ToDouble(tbxNewGasWarnHighOfAO2.Text);                 // O2浓度，单位：%
            UInt32 conInPpm = (UInt32)Math.Round(con * 10000.0f);                       // O2浓度，单位：ppm
            TxBuf[TxLen++] = (byte)((conInPpm & 0xFF0000) >> 16);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x00FF00) >> 8);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x0000FF) >> 0);

            // O2浓度预警下限
            con = Convert.ToDouble(tbxNewGasWarnLowOfAO2.Text);                         // O2浓度，单位：%
            conInPpm = (UInt32)Math.Round(con * 10000.0f);                              // O2浓度，单位：ppm
            TxBuf[TxLen++] = (byte)((conInPpm & 0xFF0000) >> 16);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x00FF00) >> 8);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x0000FF) >> 0);

            // 温度阈值上限
            tempF = Convert.ToDouble(tbxNewTemperatureAlertHighOfAO2.Text);             // 单位：℃
            temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
            TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

            // 温度阈值下限
            tempF = Convert.ToDouble(tbxNewTemperatureAlertLowOfAO2.Text);              // 单位：℃
            temp = (Int16)Math.Round(tempF * 100.0f);                                   // 单位：0.01℃
            TxBuf[TxLen++] = (byte)((temp & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((temp & 0x00FF) >> 0);

            // 湿度阈值上限
            humF = Convert.ToDouble(tbxNewHumidityAlertHighOfAO2.Text);                 // 单位：%
            hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
            TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

            // 湿度阈值下限
            humF = Convert.ToDouble(tbxNewHumidityAlertLowOfAO2.Text);                  // 单位：%
            hum = (UInt16)Math.Round(humF * 100.0f);                                    // 单位：0.01%
            TxBuf[TxLen++] = (byte)((hum & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((hum & 0x00FF) >> 0);

            // O2浓度阈值上限
            con = Convert.ToDouble(tbxNewGasAlertHighOfAO2.Text);                       // O2浓度，单位：%
            conInPpm = (UInt32)Math.Round(con * 10000.0f);                              // O2浓度，单位：ppm
            TxBuf[TxLen++] = (byte)((conInPpm & 0xFF0000) >> 16);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x00FF00) >> 8);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x0000FF) >> 0);

            // O2浓度阈值下限
            con = Convert.ToDouble(tbxNewGasAlertLowOfAO2.Text);                        // O2浓度，单位：%
            conInPpm = (UInt32)Math.Round(con * 10000.0f);                              // O2浓度，单位：ppm
            TxBuf[TxLen++] = (byte)((conInPpm & 0xFF0000) >> 16);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x00FF00) >> 8);
            TxBuf[TxLen++] = (byte)((conInPpm & 0x0000FF) >> 0);

            // Reserved
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// AO2: 修改应用配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateApplicationOfAO2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteAppCfgOfAO2();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
        }

        /// <summary>
        /// AO2: 删除历史数据
        /// </summary>
        /// <returns></returns>
        private Int16 DeleteHistoryOfAO2()
        {
            byte[] TxBuf = new byte[21];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA4;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = 0x7A;

            // Protocol
            TxBuf[TxLen++] = 0x03;

            // Sensor Mac
            if (cbxIsAllOfAO2.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfAO2.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Front
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Rear
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Reserved
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// AO2: 删除历史数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteDataOfAO2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteHistoryOfAO2();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (TableOfM1.IsSelected == true)
            {
                tbxNewDeviceMacOfM1.Text = tbxDeviceMacOfM1.Text;
                tbxNewHwRevisionOfM1.Text = tbxHwRevisionOfM1.Text;

                tbxNewCustomerOfM1.Text = tbxCustomerOfM1.Text;
                tbxNewDebugOfM1.Text = tbxDebugOfM1.Text;
                tbxNewCategoryOfM1.Text = tbxCategoryOfM1.Text;
                tbxNewPatternOfM1.Text = tbxPatternOfM1.Text;
                tbxNewBpsOfM1.Text = tbxBpsOfM1.Text;
                tbxNewTxPowerOfM1.Text = tbxTxPowerOfM1.Text;
                tbxNewChannelOfM1.Text = tbxChannelOfM1.Text;
                tbxNewTemperatureCompensationOfM1.Text = tbxTemperatureCompensationOfM1.Text;
                tbxNewHumidityCompensationOfM1.Text = tbxHumidityCompensationOfM1.Text;
                tbxNewMaxLengthOfM1.Text = tbxMaxLengthOfM1.Text;

                tbxNewSampleSendOfM1.Text = tbxSampleSendOfM1.Text;
                tbxNewIntervalOfM1.Text = tbxIntervalOfM1.Text;
                tbxNewTemperatureWarnLowOfM1.Text = tbxTemperatureWarnLowOfM1.Text;
                tbxNewTemperatureWarnHighOfM1.Text = tbxTemperatureWarnHighOfM1.Text;
                tbxNewNormalIntervalOfM1.Text = tbxNormalIntervalOfM1.Text;
                tbxNewTemperatureAlertLowOfM1.Text = tbxTemperatureAlertLowOfM1.Text;
                tbxNewTemperatureAlertHighOfM1.Text = tbxTemperatureAlertHighOfM1.Text;
                tbxNewWarnIntervalOfM1.Text = tbxWarnIntervalOfM1.Text;
                tbxNewHumidityWarnLowOfM1.Text = tbxHumidityWarnLowOfM1.Text;
                tbxNewHumidityWarnHighOfM1.Text = tbxHumidityWarnHighOfM1.Text;
                tbxNewAlertIntervalOfM1.Text = tbxAlertIntervalOfM1.Text;
                tbxNewHumidityAlertLowOfM1.Text = tbxHumidityAlertLowOfM1.Text;
                tbxNewHumidityAlertHighOfM1.Text = tbxHumidityAlertHighOfM1.Text;
            }
            else if (TableOfAO2.IsSelected == true)
            {
                tbxNewDeviceMacOfAO2.Text = tbxDeviceMacOfAO2.Text;
                tbxNewHwRevisionOfAO2.Text = tbxHwRevisionOfAO2.Text;

                tbxNewCustomerOfAO2.Text = tbxCustomerOfAO2.Text;
                tbxNewDebugOfAO2.Text = tbxDebugOfAO2.Text;
                tbxNewCategoryOfAO2.Text = tbxCategoryOfAO2.Text;
                tbxNewPatternOfAO2.Text = tbxPatternOfAO2.Text;
                tbxNewBpsOfAO2.Text = tbxBpsOfAO2.Text;
                tbxNewTxPowerOfAO2.Text = tbxTxPowerOfAO2.Text;
                tbxNewChannelOfAO2.Text = tbxChannelOfAO2.Text;
                tbxNewTemperatureCompensationOfAO2.Text = tbxTemperatureCompensationOfAO2.Text;
                tbxNewHumidityCompensationOfAO2.Text = tbxHumidityCompensationOfAO2.Text;
                tbxNewMaxLengthOfAO2.Text = tbxMaxLengthOfAO2.Text;
                tbxNewGasCompensationOfAO2.Text = tbxGasCompensationOfAO2.Text;

                tbxNewSampleSendOfAO2.Text = tbxSampleSendOfAO2.Text;
                tbxNewIntervalOfAO2.Text = tbxIntervalOfAO2.Text;
                tbxNewGasWarnLowOfAO2.Text = tbxGasWarnLowOfAO2.Text;
                tbxNewGasWarnHighOfAO2.Text = tbxGasWarnHighOfAO2.Text;
                tbxNewGasAlertLowOfAO2.Text = tbxGasAlertLowOfAO2.Text;
                tbxNewGasAlertHighOfAO2.Text = tbxGasAlertHighOfAO2.Text;
                tbxNewTemperatureWarnLowOfAO2.Text = tbxTemperatureWarnLowOfAO2.Text;
                tbxNewTemperatureWarnHighOfAO2.Text = tbxTemperatureWarnHighOfAO2.Text;
                tbxNewNormalIntervalOfAO2.Text = tbxNormalIntervalOfAO2.Text;
                tbxNewTemperatureAlertLowOfAO2.Text = tbxTemperatureAlertLowOfAO2.Text;
                tbxNewTemperatureAlertHighOfAO2.Text = tbxTemperatureAlertHighOfAO2.Text;
                tbxNewWarnIntervalOfAO2.Text = tbxWarnIntervalOfAO2.Text;
                tbxNewHumidityWarnLowOfAO2.Text = tbxHumidityWarnLowOfAO2.Text;
                tbxNewHumidityWarnHighOfAO2.Text = tbxHumidityWarnHighOfAO2.Text;
                tbxNewAlertIntervalOfAO2.Text = tbxAlertIntervalOfAO2.Text;
                tbxNewHumidityAlertLowOfAO2.Text = tbxHumidityAlertLowOfAO2.Text;
                tbxNewHumidityAlertHighOfAO2.Text = tbxHumidityAlertHighOfAO2.Text;
            }
            else if (TableOfM9.IsSelected == true)
            {
                tbxNewDeviceMacOfM9.Text = tbxDeviceMacOfM9.Text;
                tbxNewHwRevisionOfM9.Text = tbxHwRevisionOfM9.Text;

                tbxNewCustomerOfM9.Text = tbxCustomerOfM9.Text;
                tbxNewDebugOfM9.Text = tbxDebugOfM9.Text;
                tbxNewCategoryOfM9.Text = tbxCategoryOfM9.Text;
                tbxNewPatternOfM9.Text = tbxPatternOfM9.Text;
                tbxNewBpsOfM9.Text = tbxBpsOfM9.Text;
                tbxNewTxPowerOfM9.Text = tbxTxPowerOfM9.Text;
                tbxNewChannelOfM9.Text = tbxChannelOfM9.Text;
                tbxNewMaxLengthOfM9.Text = tbxMaxLengthOfM9.Text;

                tbxNewSampleSendOfM9.Text = tbxSampleSendOfM9.Text;
                tbxNewIntervalOfM9.Text = tbxIntervalOfM9.Text;
                tbxNewSensitivityOfM9.Text = tbxSensitivityOfM9.Text;

                tbxNewMoveDetectTimeOfM9.Text = tbxMoveDetectTimeOfM9.Text;
                tbxNewStaticDetectTimeOfM9.Text = tbxStaticDetectTimeOfM9.Text;
            }
            else if (TableOfM40.IsSelected == true)
            {
                tbxNewDeviceMacOfM40.Text = tbxDeviceMacOfM40.Text;
                tbxNewHwRevisionOfM40.Text = tbxHwRevisionOfM40.Text;

                tbxNewCustomerOfM40.Text = tbxCustomerOfM40.Text;
                tbxNewDebugOfM40.Text = tbxDebugOfM40.Text;
                tbxNewCategoryOfM40.Text = tbxCategoryOfM40.Text;
                tbxNewPatternOfM40.Text = tbxPatternOfM40.Text;
                tbxNewBpsOfM40.Text = tbxBpsOfM40.Text;
                tbxNewTxPowerOfM40.Text = tbxTxPowerOfM40.Text;
                tbxNewChannelOfM40.Text = tbxChannelOfM40.Text;
                tbxNewMaxLengthOfM40.Text = tbxMaxLengthOfM40.Text;

                tbxNewSampleSendOfM40.Text = tbxSampleSendOfM40.Text;
                tbxNewIntervalOfM40.Text = tbxIntervalOfM40.Text;
                tbxNewClosedTimeoutOfM40.Text = tbxClosedTimeoutOfM40.Text;
                tbxNewOpenedTimeoutOfM40.Text = tbxOpenedTimeoutOfM40.Text;
                tbxNewAlertSampleIntervalOfM40.Text = tbxAlertSampleIntervalOfM40.Text;
            }
            else if (TableOfM4.IsSelected == true)
            {
                tbxNewDeviceMacOfM4.Text = tbxDeviceMacOfM4.Text;
                tbxNewHwRevisionOfM4.Text = tbxHwRevisionOfM4.Text;

                tbxNewCustomerOfM4.Text = tbxCustomerOfM4.Text;
                tbxNewDebugOfM4.Text = tbxDebugOfM4.Text;
                tbxNewCategoryOfM4.Text = tbxCategoryOfM4.Text;
                tbxNewPatternOfM4.Text = tbxPatternOfM4.Text;
                tbxNewBpsOfM4.Text = tbxBpsOfM4.Text;
                tbxNewTxPowerOfM4.Text = tbxTxPowerOfM4.Text;
                tbxNewChannelOfM4.Text = tbxChannelOfM4.Text;
                tbxNewPowerCompensationOfM4.Text = tbxPowerCompensationOfM4.Text;
                tbxNewVoltageCompensationOfM4.Text = tbxVoltageCompensationOfM4.Text;
                tbxNewMaxLengthOfM4.Text = tbxMaxLengthOfM4.Text;

                tbxNewSampleSendOfM4.Text = tbxSampleSendOfM4.Text;
                tbxNewIntervalOfM4.Text = tbxIntervalOfM4.Text;
                tbxNewPowerWarnLowOfM4.Text = tbxPowerWarnLowOfM4.Text;
                tbxNewPowerWarnHighOfM4.Text = tbxPowerWarnHighOfM4.Text;
                tbxNewIntervalNormalOfM4.Text = tbxIntervalNormalOfM4.Text;
                tbxNewPowerAlertLowOfM4.Text = tbxPowerAlertnLowOfM4.Text;
                tbxNewPowerAlertHighOfM4.Text = tbxPowerAlertHighOfM4.Text;
                tbxNewIntervalWarningOfM4.Text = tbxIntervalWarningOfM4.Text;
                tbxNewVoltageWarnLowOfM4.Text = tbxVoltageWarnLowOfM4.Text;
                tbxNewVoltageWarnHighOfM4.Text = tbxVoltageWarnHighOfM4.Text;
                tbxNewIntervalAlarmOfM4.Text = tbxIntervalAlarmOfM4.Text;
                tbxNewVoltageAlertLowOfM4.Text = tbxVoltageAlertLowOfM4.Text;
                tbxNewVoltageAlertHighOfM4.Text = tbxVoltageAlertHighOfM4.Text;
            }
            else if (TableOfEsk.IsSelected == true)
            {
                tbxNewDeviceMacOfEsk.Text = tbxDeviceMacOfEsk.Text;
                tbxNewHwRevisionOfEsk.Text = tbxHwRevisionOfEsk.Text;

                tbxNewCustomerOfEsk.Text = tbxCustomerOfEsk.Text;
                tbxNewDebugOfEsk.Text = tbxDebugOfEsk.Text;
                tbxNewCategoryOfEsk.Text = tbxCategoryOfEsk.Text;
                tbxNewPatternOfEsk.Text = tbxPatternOfEsk.Text;
                tbxNewBpsOfEsk.Text = tbxBpsOfEsk.Text;
                tbxNewTxPowerOfEsk.Text = tbxTxPowerOfEsk.Text;
                tbxNewChannelOfEsk.Text = tbxChannelOfEsk.Text;
                tbxNewMaxLengthOfEsk.Text = tbxMaxLengthOfEsk.Text;

                tbxNewSampleSendOfEsk.Text = tbxSampleSendOfEsk.Text;
                tbxNewCfgReservedOfEsk.Text = tbxCfgReservedOfEsk.Text;
                tbxNewBlockCurrentOfEsk.Text = tbxBlockCurrentOfEsk.Text;
                tbxNewBlockDurationOfEsk.Text = tbxBlockDurationOfEsk.Text;
            }
            else if (TableOfL1.IsSelected == true)
            {
                tbxNewDeviceMacOfL1.Text = tbxDeviceMacOfL1.Text;
                tbxNewHwRevisionOfL1.Text = tbxHwRevisionOfL1.Text;

                tbxNewCustomerOfL1.Text = tbxCustomerOfL1.Text;
                tbxNewDebugOfL1.Text = tbxDebugOfL1.Text;
                tbxNewCategoryOfL1.Text = tbxCategoryOfL1.Text;
                tbxNewPatternOfL1.Text = tbxPatternOfL1.Text;
                tbxNewBpsOfL1.Text = tbxBpsOfL1.Text;
                tbxNewTxPowerOfL1.Text = tbxTxPowerOfL1.Text;
                tbxNewChannelOfL1.Text = tbxChannelOfL1.Text;
                tbxNewLuxCompensationOfL1.Text = tbxLuxCompensationOfL1.Text;
                tbxNewMaxLengthOfL1.Text = tbxMaxLengthOfL1.Text;

                tbxNewSampleSendOfL1.Text = tbxSampleSendOfL1.Text;
                tbxNewIntervalOfL1.Text = tbxIntervalOfL1.Text;
                tbxNewLuxWarnLowOfL1.Text = tbxLuxWarnLowOfL1.Text;
                tbxNewLuxWarnHighOfL1.Text = tbxLuxWarnHighOfL1.Text;
                tbxNewIntervalNormalOfL1.Text = tbxIntervalNormalOfL1.Text;
                tbxNewLuxAlertLowOfL1.Text = tbxLuxAlertnLowOfL1.Text;
                tbxNewLuxAlertHighOfL1.Text = tbxLuxAlertHighOfL1.Text;
                tbxNewIntervalWarningOfL1.Text = tbxIntervalWarningOfL1.Text;
                tbxNewIntervalAlarmOfL1.Text = tbxIntervalAlarmOfL1.Text;
            }
            else if (TableOfWP.IsSelected == true)
            {
                tbxNewDeviceMacOfWP.Text = tbxDeviceMacOfWP.Text;
                tbxNewHwRevisionOfWP.Text = tbxHwRevisionOfWP.Text;

                tbxNewCustomerOfWP.Text = tbxCustomerOfWP.Text;
                tbxNewDebugOfWP.Text = tbxDebugOfWP.Text;
                tbxNewCategoryOfWP.Text = tbxCategoryOfWP.Text;
                tbxNewPatternOfWP.Text = tbxPatternOfWP.Text;
                tbxNewBpsOfWP.Text = tbxBpsOfWP.Text;
                tbxNewTxPowerOfWP.Text = tbxTxPowerOfWP.Text;
                tbxNewChannelOfWP.Text = tbxChannelOfWP.Text;

                tbxNewSampleIntervalOfWP.Text = tbxSampleIntervalOfWP.Text;
                tbxNewUploadIntervalOfWP.Text = tbxUploadIntervalOfWP.Text;
                tbxNewAdhocIntervalSucOfWP.Text = tbxAdhocIntervalSucOfWP.Text;
                tbxNewAdhocIntervalFaiOfWP.Text = tbxAdhocIntervalFaiOfWP.Text;

                tbxNewLogModeOfWP.Text = tbxLogModeOfWP.Text;

                tbxNewAdhocRssiThrOfWP.Text = tbxAdhocRssiThrOfWP.Text;
                tbxNewTransRssiThrOfWP.Text = tbxTransRssiThrOfWP.Text;

                tbxNewUartBaudRateOfWP.Text = tbxUartBaudRateOfWP.Text;

                tbxNewHopOfWP.Text = tbxHopOfWP.Text;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (cbxLock.IsChecked == true)
            {
                return;
            }

            if (TableOfM1.IsSelected == true)
            {
                tbxDeviceNameOfM1.Text = "";
                tbxDeviceTypeOfM1.Text = "";
                tbxProtocolOfM1.Text = "";

                tbxPrimaryMacOfM1.Text = "";
                tbxDeviceMacOfM1.Text = "";
                tbxSwRevisionOfM1.Text = "";
                tbxHwRevisionOfM1.Text = "";

                tbxCustomerOfM1.Text = "";
                tbxDebugOfM1.Text = "";
                tbxCategoryOfM1.Text = "";
                tbxPatternOfM1.Text = "";
                tbxBpsOfM1.Text = "";
                tbxTxPowerOfM1.Text = "";
                tbxChannelOfM1.Text = "";
                tbxTemperatureCompensationOfM1.Text = "";
                tbxHumidityCompensationOfM1.Text = "";
                tbxMaxLengthOfM1.Text = "";

                tbxCalendarOfM1.Text = "";
                tbxSampleSendOfM1.Text = "";
                tbxIntervalOfM1.Text = "";
                tbxTemperatureWarnLowOfM1.Text = "";
                tbxTemperatureWarnHighOfM1.Text = "";
                tbxNormalIntervalOfM1.Text = "";
                tbxTemperatureAlertLowOfM1.Text = "";
                tbxTemperatureAlertHighOfM1.Text = "";
                tbxWarnIntervalOfM1.Text = "";
                tbxHumidityWarnLowOfM1.Text = "";
                tbxHumidityWarnHighOfM1.Text = "";
                tbxAlertIntervalOfM1.Text = "";
                tbxHumidityAlertLowOfM1.Text = "";
                tbxHumidityAlertHighOfM1.Text = "";

                txtICTemperature.Text = "";
                txtTemperature.Text = "";
                txtHumidity.Text = "";
                txtVolt.Text = "";
                txtRSSI.Text = "";
                txtFlashID.Text = "";
                txtFlashFront.Text = "";
                txtFlashRear.Text = "";
                txtFlashQueueLength.Text = "";
                tbxRstSrc.Text = "";
            }
            else if (TableOfAO2.IsSelected == true)
            {
                tbxDeviceNameOfAO2.Text = "";
                tbxDeviceTypeOfAO2.Text = "";
                tbxProtocolOfAO2.Text = "";

                tbxPrimaryMacOfAO2.Text = "";
                tbxDeviceMacOfAO2.Text = "";
                tbxSwRevisionOfAO2.Text = "";
                tbxHwRevisionOfAO2.Text = "";

                tbxCustomerOfAO2.Text = "";
                tbxDebugOfAO2.Text = "";
                tbxCategoryOfAO2.Text = "";
                tbxPatternOfAO2.Text = "";
                tbxBpsOfAO2.Text = "";
                tbxTxPowerOfAO2.Text = "";
                tbxChannelOfAO2.Text = "";
                tbxTemperatureCompensationOfAO2.Text = "";
                tbxHumidityCompensationOfAO2.Text = "";
                tbxMaxLengthOfAO2.Text = "";
                tbxGasCompensationOfAO2.Text = "";

                tbxCalendarOfAO2.Text = "";
                tbxSampleSendOfAO2.Text = "";
                tbxIntervalOfAO2.Text = "";
                tbxGasWarnLowOfAO2.Text = "";
                tbxGasWarnHighOfAO2.Text = "";
                tbxGasAlertLowOfAO2.Text = "";
                tbxGasAlertHighOfAO2.Text = "";
                tbxTemperatureWarnLowOfAO2.Text = "";
                tbxTemperatureWarnHighOfAO2.Text = "";
                tbxNormalIntervalOfAO2.Text = "";
                tbxTemperatureAlertLowOfAO2.Text = "";
                tbxTemperatureAlertHighOfAO2.Text = "";
                tbxWarnIntervalOfAO2.Text = "";
                tbxHumidityWarnLowOfAO2.Text = "";
                tbxHumidityWarnHighOfAO2.Text = "";
                tbxAlertIntervalOfAO2.Text = "";
                tbxHumidityAlertLowOfAO2.Text = "";
                tbxHumidityAlertHighOfAO2.Text = "";

                tbxConcentrationOfAO2.Text = "";
                tbxTemperatureOfAO2.Text = "";
                tbxHumidityOfAO2.Text = "";
                tbxVoltOfAO2.Text = "";
                tbxRssiOfAO2.Text = "";
                tbxFlashIdOfAO2.Text = "";
                tbxFlashFrontOfAO2.Text = "";
                tbxFlashRearOfAO2.Text = "";
                tbxFlashQueueLengthOfAO2.Text = "";
                tbxIcTemperatureOfAO2.Text = "";
            }
            else if (TableOfM9.IsSelected == true)
            {
                tbxDeviceNameOfM9.Text = "";
                tbxDeviceTypeOfM9.Text = "";
                tbxProtocolOfM9.Text = "";

                tbxPrimaryMacOfM9.Text = "";
                tbxDeviceMacOfM9.Text = "";
                tbxSwRevisionOfM9.Text = "";
                tbxHwRevisionOfM9.Text = "";

                tbxCustomerOfM9.Text = "";
                tbxDebugOfM9.Text = "";
                tbxCategoryOfM9.Text = "";
                tbxPatternOfM9.Text = "";
                tbxBpsOfM9.Text = "";
                tbxTxPowerOfM9.Text = "";
                tbxChannelOfM9.Text = "";
                tbxMaxLengthOfM9.Text = "";

                tbxCalendarOfM9.Text = "";
                tbxSampleSendOfM9.Text = "";
                tbxIntervalOfM9.Text = "";
                if (cbxAlertCfgLockOfM9.IsChecked == false)
                {
                    cbxAlertCfgStaticOfM9.IsChecked = false;
                    cbxAlertCfgMoveOfM9.IsChecked = false;
                    cbxAlertCfgMoveStaticOfM9.IsChecked = false;
                    cbxAlertCfgStaticMoveOfM9.IsChecked = false;
                    cbxAlertCfgExceptionOfM9.IsChecked = false;
                }
                tbxSensitivityOfM9.Text = "";
                tbxMoveDetectTimeOfM9.Text = "";
                tbxStaticDetectTimeOfM9.Text = "";
                tbkSensitivityDetailOfM9.Text = "";

                tbxICTemperatureOfM9.Text = "";
                tbxVoltOfM9.Text = "";
                tbxRssiOfM9.Text = "";
                tbxMoveStateOfM9.Text = "";
                tbxFlashIdOfM9.Text = "";
                tbxFlashFrontOfM9.Text = "";
                tbxFlashRearOfM9.Text = "";
                tbxFlashQueueLengthOfM9.Text = "";
            }
            else if (TableOfM40.IsSelected == true)
            {
                tbxDeviceNameOfM40.Text = "";
                tbxDeviceTypeOfM40.Text = "";
                tbxProtocolOfM40.Text = "";

                tbxPrimaryMacOfM40.Text = "";
                tbxDeviceMacOfM40.Text = "";
                tbxSwRevisionOfM40.Text = "";
                tbxHwRevisionOfM40.Text = "";

                tbxCustomerOfM40.Text = "";
                tbxDebugOfM40.Text = "";
                tbxCategoryOfM40.Text = "";
                tbxPatternOfM40.Text = "";
                tbxBpsOfM40.Text = "";
                tbxTxPowerOfM40.Text = "";
                tbxChannelOfM40.Text = "";
                tbxMaxLengthOfM40.Text = "";

                tbxCalendarOfM40.Text = "";
                tbxSampleSendOfM40.Text = "";
                tbxIntervalOfM40.Text = "";
                tbxClosedTimeoutOfM40.Text = "";
                tbxOpenedTimeoutOfM40.Text = "";
                tbxAlertSampleIntervalOfM40.Text = "";
                if (cbxAlertCfgLockOfM40.IsChecked == false)
                {
                    cbxAlertCfgClosedOfM40.IsChecked = false;
                    cbxAlertCfgOpenedOfM40.IsChecked = false;
                    cbxAlertCfgOpenedClosedOfM40.IsChecked = false;
                    cbxAlertCfgClosedOpenedOfM40.IsChecked = false;
                    cbxAlertCfgExceptionOfM40.IsChecked = false;
                }

                tbxICTemperatureOfM40.Text = "";
                tbxVoltOfM40.Text = "";
                tbxRssiOfM40.Text = "";
                tbxOpenStateOfM40.Text = "";
                tbxFlashIdOfM40.Text = "";
                tbxFlashFrontOfM40.Text = "";
                tbxFlashRearOfM40.Text = "";
                tbxFlashQueueLengthOfM40.Text = "";
            }
            else if (TableOfM4.IsSelected == true)
            {
                tbxDeviceNameOfM4.Text = "";
                tbxDeviceTypeOfM4.Text = "";
                tbxProtocolOfM4.Text = "";

                tbxPrimaryMacOfM4.Text = "";
                tbxDeviceMacOfM4.Text = "";
                tbxSwRevisionOfM4.Text = "";
                tbxHwRevisionOfM4.Text = "";

                tbxCustomerOfM4.Text = "";
                tbxDebugOfM4.Text = "";
                tbxCategoryOfM4.Text = "";
                tbxPatternOfM4.Text = "";
                tbxBpsOfM4.Text = "";
                tbxTxPowerOfM4.Text = "";
                tbxChannelOfM4.Text = "";
                tbxPowerCompensationOfM4.Text = "";
                tbxVoltageCompensationOfM4.Text = "";
                tbxMaxLengthOfM4.Text = "";

                tbxCalendarOfM4.Text = "";
                tbxSampleSendOfM4.Text = "";
                tbxIntervalOfM4.Text = "";
                tbxPowerWarnLowOfM4.Text = "";
                tbxPowerWarnHighOfM4.Text = "";
                tbxIntervalNormalOfM4.Text = "";
                tbxPowerAlertnLowOfM4.Text = "";
                tbxPowerAlertHighOfM4.Text = "";
                tbxIntervalWarningOfM4.Text = "";
                tbxVoltageWarnLowOfM4.Text = "";
                tbxVoltageWarnHighOfM4.Text = "";
                tbxIntervalAlarmOfM4.Text = "";
                tbxVoltageAlertLowOfM4.Text = "";
                tbxVoltageAlertHighOfM4.Text = "";

                tbxICTemperatureOfM4.Text = "";
                tbxLoadPowerOfM4.Text = "";
                tbxSupplyVoltageOfM4.Text = "";
                tbxVoltOfM4.Text = "";
                tbxRssiOfM4.Text = "";
                tbxFlashIdOfM4.Text = "";
                tbxFlashFrontOfM4.Text = "";
                tbxFlashRearOfM4.Text = "";
                tbxFlashQueueLengthOfM4.Text = "";
            }
            else if (TableOfEsk.IsSelected == true)
            {
                tbxDeviceNameOfEsk.Text = "";
                tbxDeviceTypeOfEsk.Text = "";
                tbxProtocolOfEsk.Text = "";

                tbxPrimaryMacOfEsk.Text = "";
                tbxDeviceMacOfEsk.Text = "";
                tbxSwRevisionOfEsk.Text = "";
                tbxHwRevisionOfEsk.Text = "";

                tbxCustomerOfEsk.Text = "";
                tbxDebugOfEsk.Text = "";
                tbxCategoryOfEsk.Text = "";
                tbxPatternOfEsk.Text = "";
                tbxBpsOfEsk.Text = "";
                tbxTxPowerOfEsk.Text = "";
                tbxChannelOfEsk.Text = "";
                tbxMaxLengthOfEsk.Text = "";

                tbxCalendarOfEsk.Text = "";
                tbxSampleSendOfEsk.Text = "";
                tbxCfgReservedOfEsk.Text = "";
                tbxBlockCurrentOfEsk.Text = "";
                tbxBlockDurationOfEsk.Text = "";

                tbxIdleCurrentOfEsk.Text = "";
                tbxStartCurrentOfEsk.Text = "";
                tbxRaceCurrentOfEsk.Text = "";
                tbxSupplyVoltageOfEsk.Text = "";

                tbxICTemperatureOfEsk.Text = "";                
                tbxVoltOfEsk.Text = "";
                tbxRssiOfEsk.Text = "";
                tbxFlashIdOfEsk.Text = "";
                tbxFlashFrontOfEsk.Text = "";
                tbxFlashRearOfEsk.Text = "";
                tbxFlashQueueLengthOfEsk.Text = "";
            }
            else if (TableOfL1.IsSelected == true)
            {
                tbxDeviceTypeOfL1.Text = "";
                tbxPrimaryMacOfL1.Text = "";
                tbxDeviceMacOfL1.Text = "";
                tbxProtocolOfL1.Text = "";
                tbxSwRevisionOfL1.Text = "";
                tbxHwRevisionOfL1.Text = "";

                tbxCustomerOfL1.Text = "";
                tbxDebugOfL1.Text = "";
                tbxCategoryOfL1.Text = "";
                tbxPatternOfL1.Text = "";
                tbxBpsOfL1.Text = "";
                tbxTxPowerOfL1.Text = "";
                tbxChannelOfL1.Text = "";
                tbxLuxCompensationOfL1.Text = "";
                tbxMaxLengthOfL1.Text = "";


                tbxCalendarOfL1.Text = "";
                tbxSampleSendOfL1.Text = "";
                tbxIntervalOfL1.Text = "";
                tbxLuxWarnLowOfL1.Text = "";
                tbxLuxWarnHighOfL1.Text = "";
                tbxIntervalNormalOfL1.Text = "";
                tbxLuxAlertnLowOfL1.Text = "";
                tbxLuxAlertHighOfL1.Text = "";
                tbxIntervalWarningOfL1.Text = "";
                tbxIntervalAlarmOfL1.Text = "";

                tbxICTemperatureOfL1.Text = "";
                tbxLuxOfL1.Text = "";
                tbxVoltOfL1.Text = "";
                tbxRssiOfL1.Text = "";
                tbxFlashIdOfL1.Text = "";
                tbxFlashFrontOfL1.Text = "";
                tbxFlashRearOfL1.Text = "";
                tbxFlashQueueLengthOfL1.Text = "";
            }
            else if (TableOfWP.IsSelected == true)
            {
                tbxDeviceNameOfWP.Text = "";
                tbxDeviceTypeOfWP.Text = "";
                tbxPrimaryMacOfWP.Text = "";
                tbxDeviceMacOfWP.Text = "";
                tbxProtocolOfWP.Text = "";
                tbxSwRevisionOfWP.Text = "";
                tbxHwRevisionOfWP.Text = "";

                tbxCustomerOfWP.Text = "";
                tbxDebugOfWP.Text = "";
                tbxCategoryOfWP.Text = "";
                tbxPatternOfWP.Text = "";
                tbxBpsOfWP.Text = "";
                tbxTxPowerOfWP.Text = "";
                tbxChannelOfWP.Text = "";


                tbxCalendarOfWP.Text = "";
                tbxSampleIntervalOfWP.Text = "";
                tbxUploadIntervalOfWP.Text = "";
                tbxAdhocIntervalSucOfWP.Text = "";
                tbxAdhocIntervalFaiOfWP.Text = "";

                tbxLogModeOfWP.Text = "";
                tbxAdhocRssiThrOfWP.Text = "";
                tbxTransRssiThrOfWP.Text = "";
                tbxUartBaudRateOfWP.Text = "";
                tbxHopOfWP.Text = "";

                tbxICTemperatureOfWP.Text = "";
                tbxVoltOfWP.Text = "";
                tbxRstSrcOfWP.Text = "";
                tbxRssiOfWP.Text = "";
                tbxSaveErrorOfWP.Text = "";
                tbxFileNumOfWP.Text = "";
                tbxStatusPktNumOfWP.Text = "";
            }
        }

        private void ConsoleLog(string direct, byte[] Buf, UInt16 Start, UInt16 Len)
        {
            if (direct == null && (Buf == null || Buf.Length == 0 || Len == 0))
            {
                return;
            }

            if (chkLockLog.IsChecked == true)
            {
                return;
            }

            if (Buf == null)
            {
                tbxConsole.Text = "\r\n" + Logger.GetTimeString() + "\t" + direct + "\r\n" + tbxConsole.Text;
            }
            else
            {
                tbxConsole.Text = Logger.GetTimeString() + "\t" + direct + "\t" + MyCustomFxn.ToHexString(Buf, Start, Len) + "\r\n" + tbxConsole.Text;
            }

            UInt16 ConsoleMaxLine = Convert.ToUInt16(txtLogLineLimit.Text);
            if (tbxConsole.LineCount > ConsoleMaxLine)
            {
                int start = tbxConsole.GetCharacterIndexFromLineIndex(ConsoleMaxLine);  // 末尾行第一个字符的索引
                int length = tbxConsole.GetLineLength(ConsoleMaxLine);                  // 末尾行字符串的长度
                tbxConsole.Select(start, start + length);                               // 选中末尾一行
                tbxConsole.SelectedText = "END";
            }
        }

        /// <summary>
        /// 串口发送数据，并记录日志
        /// </summary>
        /// <param name="TxBuf"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="TxLen"></param>
        private void SerialPort_Send(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen)
        {
            // 显示Log
            ConsoleLog("TX", TxBuf, IndexOfStart, TxLen);

            // 发送数据
            SerialPort.Send(TxBuf, IndexOfStart, TxLen);
        }

        /// <summary>
        /// M9: 发送串口指令，修改出厂配置
        /// </summary>
        /// <param name="Cmd"></param>
        private Int16 WriteFactoryCfgOfM9()
        {
            byte[] TxBuf = new byte[25];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA1;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceTypeOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // New Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMacOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Hardware Revision
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewHwRevisionOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -4;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M9: 修改出厂配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateFactoryOfM9_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteFactoryCfgOfM9();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// M9： 发送串口指令，修改用户配置
        /// </summary>
        /// <param name="Cmd"></param>
        private Int16 WriteUserCfgOfM9()
        {
            byte[] TxBuf = new byte[32];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA2;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceTypeOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Protocol
            TxBuf[TxLen++] = 0x03;

            // Sensor Mac
            if (cbxIsAllOfM9.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM9.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -2;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Customer
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -4;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            TxBuf[TxLen++] = Convert.ToByte(tbxNewCategoryOfM9.Text);

            // Pattern
            TxBuf[TxLen++] = Convert.ToByte(tbxNewPatternOfM9.Text);

            // bps
            TxBuf[TxLen++] = Convert.ToByte(tbxNewBpsOfM9.Text);

            // Tx Power
            Int16 txPower = Convert.ToInt16(tbxNewTxPowerOfM9.Text);
            TxBuf[TxLen++] = (byte)txPower;

            // channel
            TxBuf[TxLen++] = Convert.ToByte(tbxNewChannelOfM9.Text);

            // MaxLength
            TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfM9.Text);

            // 日期和时间
            tbxCalendarOfM9.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfM9.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // Reserved
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M9: 修改用户配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateUserOfM9_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteUserCfgOfM9();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// M9: 删除历史数据
        /// </summary>
        /// <returns></returns>
        private Int16 DeleteHistoryOfM9()
        {
            byte[] TxBuf = new byte[21];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA4;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceTypeOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Protocol
            TxBuf[TxLen++] = 0x03;

            // Sensor Mac
            if (cbxIsAllOfM9.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM9.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -2;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Front
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Rear
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Reserved
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M9: 删除历史数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteDataOfM9_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteHistoryOfM9();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// M9: 修改应用配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteAppCfgOfM9()
        {
            byte[] TxBuf = new byte[36];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA3;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceTypeOfM9.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            byte deviceType = ByteBufTmp[0];

            // Protocol
            TxBuf[TxLen++] = 0x03;

            // Sensor Mac
            if (cbxIsAllOfM9.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM9.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -2;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // 日期和时间
            tbxCalendarOfM9.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfM9.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 采集间隔
            UInt16 Interval = Convert.ToUInt16(tbxNewIntervalOfM9.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // Sample/Send
            TxBuf[TxLen++] = Convert.ToByte(tbxNewSampleSendOfM9.Text);

            if (deviceType == (byte)Device.DeviceType.M9)
            {
                UInt16 MoveDetectThr = 240;
                UInt16 MoveDetectTime = 0;
                UInt16 StaticDetectThr = 400;
                UInt16 StaticDetectTime = 0;

                byte Sensitivity = Convert.ToByte(tbxNewSensitivityOfM9.Text);

                MoveDetectThr = Convert.ToUInt16(ConfigurationManager.AppSettings["M9_MoveDetectThr_"+ Sensitivity.ToString()]);
                StaticDetectThr = Convert.ToUInt16(ConfigurationManager.AppSettings["M9_StaticDetectThr_" + Sensitivity.ToString()]);

                // 运动检测阈值，单位：mg
                TxBuf[TxLen++] = (byte)((MoveDetectThr & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((MoveDetectThr & 0x00FF) >> 0);

                // 运动检测时间，单位：ms
                MoveDetectTime = Convert.ToUInt16(tbxNewMoveDetectTimeOfM9.Text);
                TxBuf[TxLen++] = (byte)((MoveDetectTime & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((MoveDetectTime & 0x00FF) >> 0);

                // 静止检测阈值，单位：mg
                TxBuf[TxLen++] = (byte)((StaticDetectThr & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((StaticDetectThr & 0x00FF) >> 0);

                // 静止检测时间，单位：ms
                StaticDetectTime = Convert.ToUInt16(tbxNewStaticDetectTimeOfM9.Text);
                TxBuf[TxLen++] = (byte)((StaticDetectTime & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((StaticDetectTime & 0x00FF) >> 0);
            }                       

            // Alert Cfg
            byte AlertCfg = 0;
            if (cbxAlertCfgStaticOfM9.IsChecked == true)
            {   // 静止报警
                AlertCfg |= 0x01;
            }

            if (cbxAlertCfgMoveOfM9.IsChecked == true)
            {   // 运动报警
                AlertCfg |= 0x02;
            }

            if (cbxAlertCfgMoveStaticOfM9.IsChecked == true)
            {   // 动->静报警
                AlertCfg |= 0x04;
            }

            if (cbxAlertCfgStaticMoveOfM9.IsChecked == true)
            {   // 静->动报警
                AlertCfg |= 0x08;
            }

            if (cbxAlertCfgExceptionOfM9.IsChecked == true)
            {   // 异常报警
                AlertCfg |= 0x10;
            }
            TxBuf[TxLen++] = AlertCfg;

            // Reserved
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M9: 修改应用配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateApplicationOfM9_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteAppCfgOfM9();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// M4: 标定
        /// </summary>
        /// <returns></returns>
        private Int16 CalibrateOfM4()
        {
            byte[] TxBuf = new byte[24];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            bool ExistVolt = false;
            bool ExistPower = false;

            // 是否存在市电电压
            if (MyCustomFxn.HexStringToByteArray(tbxCaliVoltOfM4.Text) == null)
            {
                ExistVolt = false;
            }
            else
            {
                ExistVolt = true;
            }

            // 是否存在负载功率
            if (MyCustomFxn.HexStringToByteArray(tbxCaliPowerOfM4.Text) == null)
            {
                ExistPower = false;
            }
            else
            {
                ExistPower = true;
            }

            if (ExistVolt == false && ExistPower == false)
            {   // 既不存在市电电压，也不存在负载功率
                return -1;
            }

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA6;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = 0x58;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM4.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // GW MAC
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x11;

            // 标定方式
            TxBuf[TxLen++] = 0x01;

            // 标定内容

            UInt16 Value = 0;

            if (ExistVolt == true && ExistPower == true)
            {   // 既存在市电电压，也存在负载功率
                TxBuf[TxLen++] = 0x03;

                // 市电电压
                Value = Convert.ToUInt16(tbxCaliVoltOfM4.Text);
                TxBuf[TxLen++] = (byte)((Value & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Value & 0x00FF) >> 0);

                // 负载功率
                Value = Convert.ToUInt16(tbxCaliPowerOfM4.Text);
                TxBuf[TxLen++] = (byte)((Value & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Value & 0x00FF) >> 0);
            }
            else if (ExistVolt == true && ExistPower == false)
            {   // 只有市电电压
                TxBuf[TxLen++] = 0x01;

                // 市电电压
                Value = Convert.ToUInt16(tbxCaliVoltOfM4.Text);
                TxBuf[TxLen++] = (byte)((Value & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Value & 0x00FF) >> 0);
            }
            else if (ExistVolt == false && ExistPower == true)
            {   // 只有负载功率
                TxBuf[TxLen++] = 0x02;

                // 负载功率
                Value = Convert.ToUInt16(tbxCaliPowerOfM4.Text);
                TxBuf[TxLen++] = (byte)((Value & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Value & 0x00FF) >> 0);
            }
            else
            {
                return -3;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        /// <summary>
        /// M4: 标定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalibrateOfM4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CalibrateOfM4();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// L1: 发送串口指令，修改出厂配置
        /// </summary>
        private Int16 WriteFactoryCfgOfL1()
        {
            byte[] TxBuf = new byte[25];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA1;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = 0x85;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfL1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // New Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMacOfL1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Hardware Revision
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewHwRevisionOfL1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateFactoryOfL1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteFactoryCfgOfL1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// L1: 修改用户
        /// </summary>
        /// <returns></returns>
        private Int16 WriteUserCfgOfL1()
        {
            byte[] TxBuf = new byte[36];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA2;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = 0x85;

            // Protocol
            byte Protocol = Convert.ToByte(tbxProtocolOfL1.Text);
            TxBuf[TxLen++] = Protocol;

            // Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfL1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Customer
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfL1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfL1.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            TxBuf[TxLen++] = Convert.ToByte(tbxNewCategoryOfL1.Text);

            // Pattern
            TxBuf[TxLen++] = Convert.ToByte(tbxNewPatternOfL1.Text);

            // Bps
            TxBuf[TxLen++] = Convert.ToByte(tbxNewBpsOfL1.Text);

            // TX Power
            Int16 txPower = Convert.ToInt16(tbxNewTxPowerOfL1.Text);
            TxBuf[TxLen++] = (byte)txPower;

            // Channel
            TxBuf[TxLen++] = Convert.ToByte(tbxNewChannelOfL1.Text);

            // 存储容量
            TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfL1.Text);

            // 日期和时间
            tbxCalendarOfL1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime NowCalendar = Convert.ToDateTime(tbxCalendarOfL1.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(NowCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 光照强度补偿
            Int16 luxComp = Convert.ToInt16(tbxNewLuxCompensationOfL1.Text);       // 单位：V；
            TxBuf[TxLen++] = (byte)((luxComp & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((luxComp & 0x00FF) >> 0);

            // 保留
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;


            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateUserOfL1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteUserCfgOfL1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// L1: 修改应用配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteAppCfgOfL1()
        {
            byte[] TxBuf = new byte[58];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA3;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.L1;

            // Protocol
            byte Protocol = Convert.ToByte(tbxProtocolOfL1.Text);
            TxBuf[TxLen++] = Protocol;

            // Sensor Mac
            if (cbxIsAllOfL1.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfL1.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // 日期和时间
            tbxCalendarOfL1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfL1.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 采集间隔
            UInt16 Interval = Convert.ToUInt16(tbxNewIntervalOfL1.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 正常间隔
            Interval = Convert.ToUInt16(tbxNewIntervalNormalOfL1.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 预警间隔
            Interval = Convert.ToUInt16(tbxNewIntervalWarningOfL1.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 报警间隔
            Interval = Convert.ToUInt16(tbxNewIntervalAlarmOfL1.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 光照强度预警上限
            UInt16 power = Convert.ToUInt16(tbxNewLuxWarnHighOfL1.Text);                // 单位：lux
            TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

            // 光照强度预警下限
            power = Convert.ToUInt16(tbxNewLuxWarnLowOfL1.Text);                        // 单位：lux
            TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

            // 光照强度阈值上限
            power = Convert.ToUInt16(tbxNewLuxAlertHighOfL1.Text);                      // 单位：lux
            TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

            // 光照强度阈值下限
            power = Convert.ToUInt16(tbxNewLuxAlertLowOfL1.Text);                       // 单位：lux
            TxBuf[TxLen++] = (byte)((power & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((power & 0x00FF) >> 0);

            // 保留位
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateApplicationOfL1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteAppCfgOfL1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// L1: 删除历史数据
        /// </summary>
        /// <returns></returns>
        private Int16 DeleteHistoryOfL1()
        {
            byte[] TxBuf = new byte[21];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA4;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = 0x58;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            if (cbxIsAllOfL1.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfL1.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Front
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Rear
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnDeleteDataOfL1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteHistoryOfL1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
        }

        private void cbxAlertCfgLockOfM9_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAlertCfgLockOfM9.IsChecked == false)
            {
                cbxAlertCfgStaticOfM9.IsEnabled = true;
                cbxAlertCfgMoveOfM9.IsEnabled = true;
                cbxAlertCfgMoveStaticOfM9.IsEnabled = true;
                cbxAlertCfgStaticMoveOfM9.IsEnabled = true;
                cbxAlertCfgExceptionOfM9.IsEnabled = true;
            }
            else
            {
                cbxAlertCfgStaticOfM9.IsEnabled = false;
                cbxAlertCfgMoveOfM9.IsEnabled = false;
                cbxAlertCfgMoveStaticOfM9.IsEnabled = false;
                cbxAlertCfgStaticMoveOfM9.IsEnabled = false;
                cbxAlertCfgExceptionOfM9.IsEnabled = false;
            }
        }

        /// <summary>
        /// ESK: 发送串口指令，修改出厂配置
        /// </summary>
        private Int16 WriteFactoryCfgOfEsk()
        {
            byte[] TxBuf = new byte[25];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA1;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.ESK;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfEsk.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // New Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMacOfEsk.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Hardware Revision
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewHwRevisionOfEsk.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateFactoryOfEsk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteFactoryCfgOfEsk();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// ESK: 修改用户
        /// </summary>
        /// <returns></returns>
        private Int16 WriteUserCfgOfEsk()
        {
            byte[] TxBuf = new byte[36];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA2;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;          // 由于老版USB修改工具的问题，此USB Porotocol必须是1，否则修改失败；

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.ESK;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfEsk.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Customer
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfEsk.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfEsk.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            TxBuf[TxLen++] = Convert.ToByte(tbxNewCategoryOfEsk.Text);

            // Pattern
            TxBuf[TxLen++] = Convert.ToByte(tbxNewPatternOfEsk.Text);

            // Bps
            TxBuf[TxLen++] = Convert.ToByte(tbxNewBpsOfEsk.Text);

            // TX Power
            Int16 txPower = Convert.ToInt16(tbxNewTxPowerOfEsk.Text);
            TxBuf[TxLen++] = (byte)txPower;

            // Channel
            TxBuf[TxLen++] = Convert.ToByte(tbxNewChannelOfEsk.Text);

            // 存储容量
            TxBuf[TxLen++] = Convert.ToByte(tbxNewMaxLengthOfEsk.Text);

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateUserEsk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteUserCfgOfEsk();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// ESK: 修改应用配置
        /// </summary>
        /// <returns></returns>
        private Int16 WriteAppCfgOfEsk()
        {
            byte[] TxBuf = new byte[32];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA3;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;      // 由于老版USB修改工具的问题，此USB Porotocol必须是1，否则修改失败；

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.ESK;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            if (cbxIsAllOfM4.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfEsk.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // 日期和时间
            tbxCalendarOfEsk.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfEsk.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // Sample Send
            TxBuf[TxLen++] = Convert.ToByte(tbxNewSampleSendOfEsk.Text);

            // 截停电流阈值
            UInt16 uValue = Convert.ToUInt16(tbxNewBlockCurrentOfEsk.Text);
            TxBuf[TxLen++] = (byte)((uValue & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((uValue & 0x00FF) >> 0);

            // 截停时间阈值
            uValue = Convert.ToUInt16(tbxNewBlockDurationOfEsk.Text);
            TxBuf[TxLen++] = (byte)((uValue & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((uValue & 0x00FF) >> 0);

            // 配置保留位
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCfgReservedOfEsk.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateApplicationOfEsk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteAppCfgOfEsk();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// ESK: 删除历史数据
        /// </summary>
        /// <returns></returns>
        private Int16 DeleteHistoryOfEsk()
        {
            byte[] TxBuf = new byte[21];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA4;

            // USB Protocol
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.ESK;

            // Protocol
            TxBuf[TxLen++] = 0x02;

            // Sensor Mac
            if (cbxIsAllOfM4.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfEsk.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Front
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Rear
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnDeleteDataOfEsk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteHistoryOfEsk();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误：" + ex.Message);
            }
        }

        private void btnUpdateFactoryOfM40_Click(object sender, RoutedEventArgs e)
        {
            byte Protocol = 1;
            UInt32 DstId = 0;

            M40 aM40 = new M40();

            try
            {
                byte[] ByteBuf = null;

                ByteBuf = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM40.Text);
                if (ByteBuf == null || ByteBuf.Length < 4)
                {
                    MessageBox.Show("DeviceMac错误！");
                    return;
                }
                DstId = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);

                ByteBuf = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMacOfM40.Text);
                if (ByteBuf == null || ByteBuf.Length < 4)
                {
                    MessageBox.Show("New Mac错误！");
                    return;
                }
                aM40.DeviceMacV = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);

                ByteBuf = MyCustomFxn.HexStringToByteArray(tbxNewHwRevisionOfM40.Text);
                if (ByteBuf == null || ByteBuf.Length < 4)
                {
                    MessageBox.Show("新硬件版本错误！");
                    return;
                }
                aM40.HwRevisionV = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);
            }
            catch
            {
                MessageBox.Show("参数错误！");
                return;
            }

            byte[] TxBuf = aM40.WriteFactoryCfg(Protocol, DstId);

            SerialPort_Send(TxBuf, 0, (UInt16)TxBuf.Length);
        }

        private void btnUpdateUserOfM40_Click(object sender, RoutedEventArgs e)
        {
            byte Protocol = 1;
            UInt32 DstId = 0;

            M40 aM40 = new M40();

            try
            {
                byte[] ByteBuf = null;

                if (cbxIsAllOfM40.IsChecked == false)
                {
                    ByteBuf = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM40.Text);
                    if (ByteBuf == null || ByteBuf.Length < 4)
                    {
                        MessageBox.Show("DeviceMac错误！");
                        return;
                    }
                    DstId = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);
                }

                ByteBuf = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfM40.Text);
                if (ByteBuf == null || ByteBuf.Length < 2)
                {
                    MessageBox.Show("客户码错误！");
                    return;
                }
                aM40.CustomerV = (UInt16)(((UInt16)ByteBuf[0] << 8) | ((UInt16)ByteBuf[1] << 0));

                ByteBuf = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfM40.Text);
                if (ByteBuf == null || ByteBuf.Length < 2)
                {
                    MessageBox.Show("Debug状态错误！");
                    return;
                }
                aM40.DebugV = (UInt16)(((UInt16)ByteBuf[0] << 8) | ((UInt16)ByteBuf[1] << 0));

                aM40.Category = Convert.ToByte(tbxNewCategoryOfM40.Text);

                aM40.Pattern = Convert.ToByte(tbxNewPatternOfM40.Text);

                aM40.Bps = Convert.ToByte(tbxNewBpsOfM40.Text);

                aM40.TxPower = Convert.ToInt16(tbxNewTxPowerOfM40.Text);

                aM40.Channel = Convert.ToByte(tbxNewChannelOfM40.Text);

                aM40.MaxLength = Convert.ToByte(tbxNewMaxLengthOfM40.Text);

                aM40.Calendar = System.DateTime.Now;
                tbxCalendarOfM40.Text = aM40.Calendar.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                MessageBox.Show("参数错误！");
                return;
            }

            byte[] TxBuf = aM40.WriteUserCfg(Protocol, DstId);

            SerialPort_Send(TxBuf, 0, (UInt16)TxBuf.Length);
        }

        private void btnUpdateApplicationOfM40_Click(object sender, RoutedEventArgs e)
        {
            byte Protocol = 1;
            UInt32 DstId = 0;

            M40 aM40 = new M40();

            try
            {
                byte[] ByteBuf = null;

                if (cbxIsAllOfM40.IsChecked == false)
                {
                    ByteBuf = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM40.Text);
                    if (ByteBuf == null || ByteBuf.Length < 4)
                    {
                        MessageBox.Show("DeviceMac错误！");
                        return;
                    }
                    DstId = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);
                }

                aM40.Calendar = System.DateTime.Now;
                tbxCalendarOfM40.Text = aM40.Calendar.ToString("yyyy-MM-dd HH:mm:ss");

                aM40.Interval = Convert.ToUInt16(tbxNewIntervalOfM40.Text);

                aM40.AlertInterval = Convert.ToUInt16(tbxNewAlertSampleIntervalOfM40.Text);

                aM40.SampleSend = Convert.ToByte(tbxNewSampleSendOfM40.Text);

                if (cbxAlertCfgClosedOfM40.IsChecked == true)
                {   // 关报警
                    aM40.AlertCfg |= 0x01;
                }

                if (cbxAlertCfgOpenedOfM40.IsChecked == true)
                {   // 开报警
                    aM40.AlertCfg |= 0x02;
                }

                if (cbxAlertCfgOpenedClosedOfM40.IsChecked == true)
                {   // 开->关报警
                    aM40.AlertCfg |= 0x04;
                }

                if (cbxAlertCfgClosedOpenedOfM40.IsChecked == true)
                {   // 关->开报警
                    aM40.AlertCfg |= 0x08;
                }

                if (cbxAlertCfgExceptionOfM40.IsChecked == true)
                {   // 异常报警
                    aM40.AlertCfg |= 0x10;
                }

                aM40.ClosedTimeoutS = Convert.ToUInt16(tbxNewClosedTimeoutOfM40.Text);

                aM40.OpenedTimeoutS = Convert.ToUInt16(tbxNewOpenedTimeoutOfM40.Text);
            }
            catch
            {
                MessageBox.Show("参数错误！");
                return;
            }

            byte[] TxBuf = aM40.WriteAppCfg(Protocol, DstId);

            SerialPort_Send(TxBuf, 0, (UInt16)TxBuf.Length);
        }

        private void btnDeleteDataOfM40_Click(object sender, RoutedEventArgs e)
        {
            byte Protocol = 1;
            UInt32 DstId = 0;

            M40 aM40 = new M40();

            try
            {
                byte[] ByteBuf = null;

                if (cbxIsAllOfM40.IsChecked == false)
                {
                    ByteBuf = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfM40.Text);
                    if (ByteBuf == null || ByteBuf.Length < 4)
                    {
                        MessageBox.Show("DeviceMac错误！");
                        return;
                    }
                    DstId = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);
                }
            }
            catch
            {
                MessageBox.Show("参数错误！");
                return;
            }

            byte[] TxBuf = aM40.DeleteHistory(Protocol, DstId);

            SerialPort_Send(TxBuf, 0, (UInt16)TxBuf.Length);
        }

        private void cbxAlertCfgLockOfM40_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAlertCfgLockOfM40.IsChecked == false)
            {
                cbxAlertCfgClosedOfM40.IsEnabled = true;
                cbxAlertCfgOpenedOfM40.IsEnabled = true;
                cbxAlertCfgOpenedClosedOfM40.IsEnabled = true;
                cbxAlertCfgClosedOpenedOfM40.IsEnabled = true;
                cbxAlertCfgExceptionOfM40.IsEnabled = true;
            }
            else
            {
                cbxAlertCfgClosedOfM40.IsEnabled = false;
                cbxAlertCfgOpenedOfM40.IsEnabled = false;
                cbxAlertCfgOpenedClosedOfM40.IsEnabled = false;
                cbxAlertCfgClosedOpenedOfM40.IsEnabled = false;
                cbxAlertCfgExceptionOfM40.IsEnabled = false;
            }
        }

        private void btnLogClear_Click(object sender, RoutedEventArgs e)
        {
            tbxConsole.Text = "";
        }

        private Int16 WriteFactoryCfgOfWP()
        {
            byte[] TxBuf = new byte[25];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA1;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.WP;

            // Protocol
            TxBuf[TxLen++] = 0x01;

            // Sensor Mac
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfWP.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // New Sensor Mac
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMacOfWP.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // Hardware Revision
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewHwRevisionOfWP.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];
            TxBuf[TxLen++] = ByteBufTmp[2];
            TxBuf[TxLen++] = ByteBufTmp[3];

            // 保留
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateFactoryOfWP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteFactoryCfgOfWP();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        private Int16 WriteUserCfgOfWP()
        {
            byte[] TxBuf = new byte[36];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA2;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.WP;

            // Protocol
            TxBuf[TxLen++] = 0x01;

            // Sensor Mac
            if (cbxIsAllOfWP.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfWP.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Customer
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewCustomerOfWP.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewDebugOfWP.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            TxBuf[TxLen++] = Convert.ToByte(tbxNewCategoryOfWP.Text);

            // Pattern
            TxBuf[TxLen++] = Convert.ToByte(tbxNewPatternOfWP.Text);

            // Bps
            TxBuf[TxLen++] = Convert.ToByte(tbxNewBpsOfWP.Text);

            // TX Power
            Int16 txPower = Convert.ToInt16(tbxNewTxPowerOfWP.Text);
            TxBuf[TxLen++] = (byte)txPower;

            // Channel
            TxBuf[TxLen++] = Convert.ToByte(tbxNewChannelOfWP.Text);

            // 日期和时间
            tbxCalendarOfL1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime NowCalendar = Convert.ToDateTime(tbxCalendarOfWP.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(NowCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 保留
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateUserOfWP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteUserCfgOfWP();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        private Int16 WriteAppCfgOfWP()
        {
            byte[] TxBuf = new byte[36];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA3;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.WP;

            // Protocol
            TxBuf[TxLen++] = 0x01;

            // Sensor Mac
            if (cbxIsAllOfWP.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfWP.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // 日期和时间
            tbxCalendarOfL1.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DateTime ThisCalendar = Convert.ToDateTime(tbxCalendarOfWP.Text);
            ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 采集间隔
            UInt16 Interval = Convert.ToUInt16(tbxNewSampleIntervalOfWP.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 上传间隔
            Interval = Convert.ToUInt16(tbxNewUploadIntervalOfWP.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 正常组网间隔
            Interval = Convert.ToUInt16(tbxNewAdhocIntervalSucOfWP.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 异常组网间隔
            Interval = Convert.ToUInt16(tbxNewAdhocIntervalFaiOfWP.Text);
            TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

            // 日志模式
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxNewLogModeOfWP.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // 组网时的RSSI阈值
            TxBuf[TxLen++] =(byte) Convert.ToInt16(tbxNewAdhocRssiThrOfWP.Text);

            // 传输时的RSSI阈值
            TxBuf[TxLen++] = (byte)Convert.ToInt16(tbxNewTransRssiThrOfWP.Text);

            // 串口波特率
            TxBuf[TxLen++] = Convert.ToByte(tbxNewUartBaudRateOfWP.Text);

            // Hop
            TxBuf[TxLen++] = Convert.ToByte(tbxNewHopOfWP.Text);

            // 保留位
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnUpdateApplicationOfWP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteAppCfgOfWP();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        private Int16 DeleteHistoryOfWP()
        {
            byte[] TxBuf = new byte[20];
            UInt16 TxLen = 0;

            byte[] ByteBufTmp = null;

            // Start
            TxBuf[TxLen++] = 0xCE;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0xA4;

            // USB Protocol
            TxBuf[TxLen++] = 0x02;

            // 设备类型
            TxBuf[TxLen++] = (byte)Device.DeviceType.WP;

            // Protocol
            TxBuf[TxLen++] = 0x01;

            // Sensor Mac
            if (cbxIsAllOfWP.IsChecked == true)
            {
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
            }
            else
            {
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDeviceMacOfWP.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return -1;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];
            }

            // Which
            TxBuf[TxLen++] = 0x00;

            // 保留位
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 2, (UInt16)(TxLen - 2));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xEC;

            // 重写长度位
            TxBuf[1] = (byte)(TxLen - 5);

            SerialPort_Send(TxBuf, 0, TxLen);

            return 0;
        }

        private void btnDeleteDataOfWP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteHistoryOfWP();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }


        private void tbxNewDeviceMacOfM1_inTextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbxBatch.IsChecked == false)
            {
                return;
            }

            if (tbxNewDeviceMacOfM1.Text == null || tbxNewDeviceMacOfM1.Text == string.Empty || tbxNewDeviceMacOfM1.Text.Length == 0)
            {
                return;
            }

            // 获取末尾的字符
            String lastChar = tbxNewDeviceMacOfM1.Text.Substring(tbxNewDeviceMacOfM1.Text.Length - 1, 1);

            // 将光标移到末尾
            this.tbxNewDeviceMacOfM1.Focus();
            this.tbxNewDeviceMacOfM1.Select(this.tbxNewDeviceMacOfM1.Text.Length, 0);
            this.tbxNewDeviceMacOfM1.ScrollToEnd();

            // 去除字符串中的空格和TAB符
            String inputStr = tbxNewDeviceMacOfM1.Text;
            inputStr = inputStr.Replace(" ", "");
            inputStr = inputStr.Replace("\t", "");
            if (inputStr.Length < 8)
            {
                return;
            }

            // 转为字符数组
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(inputStr);
            if (ByteBufTmp == null || ByteBufTmp.Length != 4)
            {
                return;
            }

            // 判断末尾是不是回车符
            if (lastChar.Equals("\n") == false)
            {
                return;
            }

            UInt32 InputVal = ((UInt32)ByteBufTmp[0] << 24) | ((UInt32)ByteBufTmp[1] << 16) | ((UInt32)ByteBufTmp[2] << 8) | ((UInt32)ByteBufTmp[3] << 0);

            if(InputVal == 0)
            {   // 上电自检
                btnStartMonitor_Click(sender, e);
            }else
            {   // 修改出厂配置
                btnUpdateFactoryOfM1_Click(sender, e);
            }
            

            // 执行成功后，清除之前输入的内容，并将光标移到此位置
            tbxNewDeviceMacOfM1.Text = "";
            // 将光标移到末尾
            this.tbxNewDeviceMacOfM1.Focus();
            this.tbxNewDeviceMacOfM1.Select(this.tbxNewDeviceMacOfM1.Text.Length, 0);
            this.tbxNewDeviceMacOfM1.ScrollToEnd();
        }

        private void tbxNewDeviceMacOfM1_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbxNewDeviceMacOfM1_inTextChanged(sender, e);

            if(tbxNewDeviceMacOfM1.Text.Contains("\n") == true || tbxNewDeviceMacOfM1.Text.Contains("\r") == true)
            {   // 删除字符串中的回车换行符
                tbxNewDeviceMacOfM1.Text = tbxNewDeviceMacOfM1.Text.Replace("\n", "");
                tbxNewDeviceMacOfM1.Text = tbxNewDeviceMacOfM1.Text.Replace("\r", "");
            }
        }
    }
}
