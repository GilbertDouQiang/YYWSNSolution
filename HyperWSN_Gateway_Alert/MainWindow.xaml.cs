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

namespace HyperWSN_Gateway_Alert
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper comport;
        String UartCommand;

        bool continueFlag = false; //是否需要继续监听的控制位

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Load(object sender, RoutedEventArgs e)
        {
            FindComport();
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
            //btnOpenComport.Content.ToString();
            if (btnOpenComport.Content.ToString() == "Open")
            {
                comport = new SerialPortHelper();
                comport.SerialPortReceived += Comport_SerialPortReceived;
                string portname = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());
                comport.InitCOM(portname);
                if (comport.OpenPort())
                {
                    btnOpenComport.Content = "Close";
                    cbSerialPort.IsEnabled = false;
                    btnFindComport.IsEnabled = false;
                }
            }
            else
            {
                if (comport != null)
                {
                    comport.ClosePort();
                    btnOpenComport.Content = "Open";
                    cbSerialPort.IsEnabled = true;
                    btnFindComport.IsEnabled = true;
                }
            }
        }   //-

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            //Dispatcher.BeginInvoke(new Action(delegate
            //{
            //    //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定
            //    //收到数据后的几种情况
            //    // 未收到任何反馈
            //    if (e.ReceivedBytes == null)
            //    {
            //        if (CBLoop.IsChecked == true)
            //        {
            //            continueFlag = true;
            //        }
            //        return;
            //    }

            //    //
            //    if (e.ReceivedBytes.Length == 8)
            //    {
            //        //收到更新反馈报
            //        if (e.ReceivedBytes[2] == 0xA1 || e.ReceivedBytes[2] == 0xA2
            //        || e.ReceivedBytes[2] == 0xA3 || e.ReceivedBytes[2] == 0xA4)
            //        {   //立即进入读取状态
            //            btnStartMonitor_Click(this, null);
            //        }

            //        if (CBLoop.IsChecked == true)
            //        {
            //            continueFlag = true;
            //        }
            //        return;
            //    }
            //}));

            ////处理 M1 M1P M4的上电自检数据
            //if (e.ReceivedBytes.Length == 0x52)
            //{
            //    continueFlag = false;
            //    Dispatcher.BeginInvoke(new Action(delegate
            //    {
            //        txtConsole.Text += Logger.GetTimeString() + "\t" + CommArithmetic.ToHexString(e.ReceivedBytes) + "\r\n";
            //        //需要过滤掉不符合长度
            //        Device device = DeviceFactory.CreateDevice(e.ReceivedBytes);
            //        if (device != null && device.GetType() == typeof(M1))
            //        {
            //            m1Device = (M1)device;
            //            StackM1.DataContext = m1Device;
            //        }

            //        if (device != null && device.GetType() == typeof(Socket1))
            //        {
            //            socket1Device = (Socket1)device;
            //            StackSocket1.DataContext = socket1Device;
            //        }
            //        //enable monitor
            //        btnStopMonitor_Click(this, null);
            //    }));
            //}

            ////处理M2上电自检数据
            //if (e.ReceivedBytes.Length == 0x46)
            //{
            //    continueFlag = false;
            //    Dispatcher.BeginInvoke(new Action(delegate
            //    {
            //        txtConsole.Text += Logger.GetTimeString() + "\t" + CommArithmetic.ToHexString(e.ReceivedBytes) + "\r\n";
            //        //需要过滤掉不符合长度
            //        Device device = DeviceFactory.CreateDevice(e.ReceivedBytes);
            //        if (device != null && device.GetType() == typeof(M2))
            //        {
            //            m2Device = (M2)device;
            //            StackM1.DataContext = m2Device;
            //        }

            //        //enable monitor
            //        btnStopMonitor_Click(this, null);
            //    }));
        }   //-

    }
}
