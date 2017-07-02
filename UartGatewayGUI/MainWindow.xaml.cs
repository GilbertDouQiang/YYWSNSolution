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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenComport_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenComport.Content.ToString() == "Open")
            {
                comport = new SerialPortHelper();
                comport.InitCOM("COM10");
                comport.OpenPort();
                btnOpenComport.Content = "Close";
            }
            else
            {
                if (comport != null)
                {
                    comport.ClosePort();
                    btnOpenComport.Content = "Open";
                }
            }
            
        }

        private void btnReadInfo_Click(object sender, RoutedEventArgs e)
        {
            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes = comport.SendCommand(command.ReadInfo(), 500);

            UartGateway device = (UartGateway)DeviceFactory.CreateDevice(resultBytes);
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
    }
}
