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
        M1 m1Device;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FindComport();
            CBLoop.IsChecked = true;
            DisableControl();
            cbUserParameter.Items.Clear();
            //临时
            cbUserParameter.Items.Add("标准配置");
            cbUserApplication.Items.Add("标准配置");




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
                //需要过滤掉不符合长度
                Device device = DeviceFactory.CreateDevice(e.ReceivedBytes);
                if (device!=null && device.GetType() == typeof(M1))
                {
                    m1Device = (M1)device;
                    StackM1.DataContext = m1Device;
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
            StartMonitor();

        }

        private void StartMonitor()
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

        /// <summary>
        /// 更新工厂信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateFactory_Click(object sender, RoutedEventArgs e)
        {
            M1 updateDevice = new M1();
            try
            {
                updateDevice.DeviceMac = txtDeviceMAC.Text;
                updateDevice.DeviceNewMAC = txtNewDeviceMAC.Text;
                updateDevice.HardwareVersion = txtNewHardwareVersion.Text;

                byte[] updateCommand = updateDevice.UpdateFactory();
                //string updateString = CommArithmetic.ToHexString(updateCommand);
                if (monitorTimer.Enabled == true)
                {
                    monitorTimer.Enabled = false;

                    System.Threading.Thread.Sleep(2000); //界面会卡

                    comport.SendCommand(updateCommand);

                    System.Threading.Thread.Sleep(200); //界面会卡

                    StartMonitor();
                    monitorTimer.Enabled = true;




                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("参数错误"+ex.Message);
            }
           


            //comport.SendCommand(updateCommand);

            //updateDevice.UpdateFactory



        }

        private void cbUserParameter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO: 临时配置
            if (cbUserParameter.SelectedIndex==0)
            {
                txtNewClientID.Text = "D3 9A";
                txtNewDebug.Text = "00 02";
                txtNewCategory.Text = "0";
                txtNewWorkFunction.Text = "1";
                txtNewBPS.Text = "0";
                txtNewTXPower.Text = "13";
                txtNewMaxLength.Text = "4";
                txtNewTemperatureCompensation.Text = "0";
                txtNewHumidityCompensation.Text = "0";
                txtNewFrequency.Text = "0";


            }
            

             
        }

        /// <summary>
        /// 更新用户配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateUser_Click(object sender, RoutedEventArgs e)
        {

            M1 updateDevice = new M1();
            try
            {
                updateDevice.DeviceMac = txtDeviceMAC.Text;

                updateDevice.ClientID = txtNewClientID.Text;
                updateDevice.DebugString = txtNewDebug.Text;
                updateDevice.Category = Convert.ToByte(txtNewCategory.Text);
                updateDevice.WorkFunction = Convert.ToByte(txtNewWorkFunction.Text);
                updateDevice.SymbolRate = Convert.ToByte(txtNewBPS.Text);
                updateDevice.TXPower = Convert.ToByte(txtNewTXPower.Text);
                updateDevice.Frequency = Convert.ToByte(txtNewFrequency.Text);
                updateDevice.TemperatureCompensation = Convert.ToDouble(txtNewTemperatureCompensation.Text);
                updateDevice.HumidityCompensation = Convert.ToDouble(txtNewHumidityCompensation.Text);
                updateDevice.MaxLength = Convert.ToByte(txtNewMaxLength.Text);


                byte[] updateCommand = updateDevice.UpdateUserConfig();
                string updateString = CommArithmetic.ToHexString(updateCommand);
                if (monitorTimer.Enabled == true)
                {
                    monitorTimer.Enabled = false;

                    System.Threading.Thread.Sleep(2000); //界面会卡

                    comport.SendCommand(updateCommand);

                    System.Threading.Thread.Sleep(200); //界面会卡

                    StartMonitor();
                    monitorTimer.Enabled = true;




                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("参数错误：" + ex.Message);
            }

         

        }

        private void btnUpdateApplication_Click(object sender, RoutedEventArgs e)
        {
            M1 updateDevice = new M1();

            try
            {
                updateDevice.DeviceMac = txtDeviceMAC.Text;

                updateDevice.Interval = Convert.ToInt32(txtNewInterval.Text);
                updateDevice.TXTimers = Convert.ToByte(txtNewTXTimers.Text);
                txtCalendar.Text = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                updateDevice.Calendar = Convert.ToDateTime(txtCalendar.Text);



                byte[] updateCommand = updateDevice.UpdateApplicationConfig();
                string updateString = CommArithmetic.ToHexString(updateCommand);
                if (monitorTimer.Enabled == true)
                {
                    monitorTimer.Enabled = false;

                    System.Threading.Thread.Sleep(2000); //界面会卡

                    comport.SendCommand(updateCommand);

                    System.Threading.Thread.Sleep(250); //界面会卡

                    StartMonitor();
                    monitorTimer.Enabled = true;




                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("参数错误：" + ex.Message);
            }
          
        }

        private void cbUserApplication_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cbUserApplication.SelectedIndex == 0)
            {
                txtNewInterval.Text = "30";
                txtNewTXTimers.Text = "1";
            }
        }
    }
}