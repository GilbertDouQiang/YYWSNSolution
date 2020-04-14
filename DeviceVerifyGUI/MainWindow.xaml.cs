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

namespace DeviceVerifyGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<M1> m1groups = new ObservableCollection<M1>(); //DataGrid 中对应M1的表格
        SerialPortHelper comport;

        public MainWindow()
        {
            InitializeComponent();
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
            }
        }

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定
            Dispatcher.BeginInvoke(new Action(delegate
            {
                txtConsole.Text = Logger.GetTimeString() + "\t" + CommArithmetic.ToHexString(e.ReceivedBytes) + "\r\n" + txtConsole.Text;

                ObservableCollection<Device> devices = DeviceFactory.CreateDevices(e.ReceivedBytes);


            }));
        }

        private void btnResersh_Click(object sender, RoutedEventArgs e)
        {
            FindComport();

        }

        private void btnSendCommand_Click(object sender, RoutedEventArgs e)
        {
            if (comport != null)
            {
                comport.Send(txtSendCommand.Text);
            }
        }
    }
}
