﻿using System;
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

using System.Windows.Threading;



namespace UartGatewayGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper comport;
        UartGateway device;
        DispatcherTimer timer; //不用Timer



        public MainWindow()
        {
            InitializeComponent();
            DisableControl();
            FindComport();
        }

        private void btnOpenComport_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenComport.Content.ToString() == "Open")
            {
                comport = new SerialPortHelper();
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

        private void btnReadInfo_Click(object sender, RoutedEventArgs e)
        {
            //清空数据
            txtOldMac.Text = "";
            txtNewMac.Text = "";
            txtHarewareVersion.Text = "";
            txtSoftwareVersion.Text = "";
            txtClientID.Text = "";
            txtDebug.Text = "";
            txtCategory.Text = "";
            txtInterval.Text = "";
            txtWorkFunction.Text = "";
            txtCalendar.Text = "";
            txtSymbolRate.Text = "";
            txtROMCount.Text = "";
            txtFrontPoint.Text = "";
            txtRearPoint.Text = "";

            //System.Threading.Thread.Sleep(500);

            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes = comport.SendCommand(command.ReadInfo(), 500);

            device = (UartGateway)DeviceFactory.CreateDevice(resultBytes);
            if (device !=null)
            {
                txtOldMac.Text = device.DeviceMacS;
                txtNewMac.Text = device.DeviceMacS;
                txtHarewareVersion.Text = device.HwVersionS;
                txtSoftwareVersion.Text = device.SwVersionS;
                txtClientID.Text = device.CustomerS;
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
            device.HwVersionS = txtHarewareVersion.Text;
            device.CustomerS = txtClientID.Text;
            device.Category = Convert.ToByte(txtCategory.Text);
            device.Debug = CommArithmetic.HexStringToByteArray(txtDebug.Text);
            device.Interval = Convert.ToInt32(txtInterval.Text);
            device.SymbolRate = Convert.ToByte(txtSymbolRate.Text);
            device.WorkFunction = Convert.ToByte(txtWorkFunction.Text);

            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes = comport.SendCommand(command.UpdateInfo(device), 500);

            device = (UartGateway)DeviceFactory.CreateDevice(resultBytes);



        }

        /// <summary>
        /// 执行授时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupDate_Click(object sender, RoutedEventArgs e)
        {
            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes = comport.SendCommand(command.DateTimeSyncchronization(device), 500);


        }

        private void ReadData_Click(object sender, RoutedEventArgs e)
        {
            if (chkLoop.IsChecked== true)
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(1000);
                timer.Tick += Timer_Tick; ;
                timer.Start();



            }
            else
            {
                ReadSingleDate(1000);
            }

            
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (chkLoop.IsChecked == false)
                timer.Stop();

            ReadSingleDate(1000);
        }


        /// <summary>
        /// 读取单条数据
        /// </summary>
        /// <param name="Timeout"></param>
        private void ReadSingleDate(int Timeout)
        {
            //TODO: Timeout 的参数化设置
            UartGatewayCommand command = new UartGatewayCommand();
            byte[] resultBytes = comport.SendCommand(command.ReadData(Timeout), Timeout);
            
            if (resultBytes !=null)
            {
                txtConsole.Text += "\r\n"+Logger.GetTimeString() +"\t"+ CommArithmetic.ToHexString(resultBytes);
            }

        }

        private void btnFindComport_Click(object sender, RoutedEventArgs e)
        {
            FindComport();
        }

        private  void FindComport()
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
    }
}
