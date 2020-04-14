using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace HyperWSN_Setup_IR20
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper SerialPort;
        IR20 aIR20 = null;
        bool CmdMode = false;           // false = 十六进制命令模式; true = 字符串命令模式;

        public MainWindow()
        {
            InitializeComponent();

            aIR20 = new IR20();

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
            if (btnOpenComport.Content.ToString() == "Open")
            {
                SerialPort = new SerialPortHelper();
                SerialPort.SerialPortReceived += Comport_SerialPortReceived;
                string portname = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());
                SerialPort.InitCOM(portname, 115200);
                if (SerialPort.OpenPort())
                {
                    btnOpenComport.Content = "Close";
                }
            }
            else
            {
                if (SerialPort != null)
                {
                    SerialPort.ClosePort();
                    btnOpenComport.Content = "Open";
                }
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ConsoleLog(string direct, byte[] Txt, UInt16 IndexOfStart, UInt16 TxtLen)
        {
            if (Txt.Length != 0)
            {

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

        private void btnSwitchCmdMode_Click(object sender, RoutedEventArgs e)
        {
            if (CmdMode == false)
            {
                CmdMode = true;
            }
            else
            {
                CmdMode = false;
            }

            byte[] txPkt = aIR20.TxPkt_SwitchCmdMode(CmdMode);

            SerialPort_Send(txPkt, 0, (UInt16)txPkt.Length);
        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            byte[] txPkt = aIR20.TxPkt_Ping();

            SerialPort_Send(txPkt, 0, (UInt16)txPkt.Length);
        }

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

                // 显示Log
                ConsoleLog("RX", e.ReceivedBytes, 0, (UInt16)e.ReceivedBytes.Length);

                UInt16 SrcLen = (UInt16)e.ReceivedBytes.Length;

                /*
                Int16 HandleLen = 0;
                Int16 ExeError = 0;

                for (UInt16 iCnt = 0; iCnt < SrcLen; iCnt++)
                {
                    try
                    {
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("处理接收数据包错误" + ex.Message);
                    }
                }
                */
            }));
        }

        private void btnAuthenticate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGetBodyTemp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGetAllTemp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReadCfg_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
