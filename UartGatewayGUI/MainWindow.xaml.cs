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
using YyWsnCommunicatonLibrary;

namespace UartGatewayGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper comport;
        UartGateway device;
        public MainWindow()
        {
            InitializeComponent();
            DisableControl();
        }

        private void btnOpenComport_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenComport.Content.ToString() == "Open")
            {
                comport = new SerialPortHelper();
                comport.InitCOM(TxtSerialPort.Text);
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

        private void btnReadInfo_Click(object sender, RoutedEventArgs e)
        {
            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes = comport.SendCommand(command.ReadInfo(), 500);

            device = (UartGateway)DeviceFactory.CreateDevice(resultBytes);
            if (device !=null)
            {
                txtOldMac.Text = device.DeviceMac;
                txtNewMac.Text = device.DeviceMac;
                txtHarewareVersion.Text = device.HardwareVersion;
                txtSoftwareVersion.Text = device.SoftwareVersion;
                txtClientID.Text = device.ClientID;
                txtDebug.Text = CommArithmetic.DecodeClientID(device.Debug,0);
                txtCategory.Text = device.Category.ToString("X2");
                txtInterval.Text = device.Interval.ToString();
                txtWorkFunction.Text = device.WorkFunction.ToString();
                txtCalendar.Text = device.Calendar.ToString();
                txtSymbolRate.Text = device.SymbolRate.ToString();
                txtROMCount.Text = device.ROMCount.ToString();
                txtFrontPoint.Text = device.FrontPoint.ToString();
                txtRearPoint.Text = device.RearPoint.ToString();




            }

        }

        public void EnableControls()
        {
            foreach (var item in btns.Children)
            {
                if (item.GetType() == typeof(Button))
                {
                    ((Button)(item)).IsEnabled = true;
                }


            }

        }

        public void DisableControl()
        {
            foreach (var item in btns.Children)
            {
                if (item.GetType() == typeof(Button))
                {
                   ( (Button)(item)).IsEnabled = false;
                }
                

            }

        }

        private void btnUpdateInfo_Click(object sender, RoutedEventArgs e)
        {
            //更新UartGateway 配置
            if (device == null)
            {
                return;
            }

            device.DeviceNewMAC = txtNewMac.Text;
            device.HardwareVersion = txtHarewareVersion.Text;
            device.ClientID = txtClientID.Text;
            device.Category = Convert.ToByte(txtCategory.Text);
            device.Debug = CommArithmetic.HexStringToByteArray(txtDebug.Text);
            device.Interval = Convert.ToInt32(txtInterval.Text);
            device.SymbolRate = Convert.ToByte(txtSymbolRate.Text);
            device.WorkFunction = Convert.ToByte(txtWorkFunction.Text);

            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes = comport.SendCommand(command.UpdateInfo(device), 500);

            device = (UartGateway)DeviceFactory.CreateDevice(resultBytes);



        }
    }
}
