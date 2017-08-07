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

namespace DeviceSetup_HyperWSN
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper comport;
        String UartCommand;
        Timer monitorTimer;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FindComport();
            CBLoop.IsChecked = true;
            DisableControl();



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
                    EnableControls();
                }



            }
            else
            {
                if (comport != null)
                {
                    comport.ClosePort();
                    btnOpenComport.Content = "Open";
                    DisableControl();
                }
            }

        }

        private void DisableControl()
        {
            //throw new NotImplementedException();
            btnStartMonitor.IsEnabled = false;
            btnStopMonitor.IsEnabled = false;
            CBLoop.IsEnabled = false;



        }

        private void EnableControls()
        {
            //throw new NotImplementedException();
            btnStartMonitor.IsEnabled = true;
            CBLoop.IsEnabled = true;


        }

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定


            //在Log中体现
            Dispatcher.BeginInvoke(new Action(delegate
            {
                txtConsole.Text += Logger.GetTimeString() + "\t" + CommArithmetic.ToHexString(e.ReceivedBytes) + "\r\n";

                Device device = DeviceFactory.CreateDevice(e.ReceivedBytes);
                if (device.GetType() == typeof(M1))
                {
                    //发现了一个M1
                }
                //ObservableCollection<Device> devices = DeviceFactory.CreateDevices(e.ReceivedBytes);

                /*
                foreach (M1 item in devices)
                {
                    item.DisplayID = SerialNo;
                    SerialNo++;
                    m1groups.Add(item);

                }

    */

            }));




        }

        
        /// <summary>
        /// 启动串口监听，如果选中循环监听，则持续监听
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartMonitor_Click(object sender, RoutedEventArgs e)
        {
            UartCommand = "CE 03 01 01 02 00 00 EC";

            byte[] command = CommArithmetic.HexStringToByteArray(UartCommand);
            //启动第一次监听
            if (comport!=null)
            {
                int result = comport.SendCommand(command);
            }
            if(CBLoop.IsChecked==true)
            {
                monitorTimer = new Timer();
                monitorTimer.Elapsed += MonitorTimer_Elapsed;
                monitorTimer.Interval = 2050;
                monitorTimer.Enabled = true;
                btnStartMonitor.IsEnabled = false;
                btnStopMonitor.IsEnabled = true;

            }

            




        }

        private void MonitorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // new NotImplementedException();
            UartCommand = "CE 03 01 01 02 00 00 EC";

            byte[] command = CommArithmetic.HexStringToByteArray(UartCommand);
            //启动循环监听
            if (comport != null)
            {
                int result = comport.SendCommand(command);
            }

        }

        private void btnStopMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (monitorTimer!=null)
            {
                monitorTimer.Enabled = false;
                monitorTimer.Dispose();
                btnStartMonitor.IsEnabled = true;
                btnStopMonitor.IsEnabled = false;

            }

        }
    }
}
