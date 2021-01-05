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
using YyWsnDeviceLibrary;
using YyWsnCommunicatonLibrary;

namespace HyperWSN_Tool_TM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        SerialPortHelper SerialPort;


        public MainWindow()
        {
            InitializeComponent();

            this.Title += " v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

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
        /// 判断收到的数据包是否符合格式要求
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        private Int16 RxPkt_IsRight(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 11)
            {
                return -1;
            }

            // 起始位
            if (SrcData[IndexOfStart + 0] != 0xAC || SrcData[IndexOfStart + 1] != 0xAC)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 2];
            if (pktLen + 7 > SrcLen)
            {
                return -3;
            }

            if (SrcData[IndexOfStart + 3 + pktLen + 2] != 0xCA || SrcData[IndexOfStart + 3 + pktLen + 3] != 0xCA)
            {
                return -4;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, SrcData, (UInt16)(IndexOfStart + 3), pktLen);
            UInt16 crc_chk = (UInt16)(SrcData[IndexOfStart + 3 + pktLen + 0] * 256 + SrcData[IndexOfStart + 3 + pktLen + 1]);
            if (crc_chk != crc && crc_chk != 0)
            {
                return -5;
            }

            return (Int16)(pktLen + 7);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ConsoleLog(string direct, byte[] Txt, UInt16 IndexOfStart, UInt16 TxtLen)
        {
            if (Txt.Length != 0 && chkLockLog.IsChecked == false)
            {
                tbxConsole.Text = Logger.GetTimeString() + "\t" + direct + "\t" + MyCustomFxn.ToHexString(Txt, IndexOfStart, TxtLen) + "\r\n" + tbxConsole.Text;

                UInt16 ConsoleMaxLine = Convert.ToUInt16(txtLogLineLimit.Text);
                if (tbxConsole.LineCount > ConsoleMaxLine)
                {
                    int start = tbxConsole.GetCharacterIndexFromLineIndex(ConsoleMaxLine);  // 末尾行第一个字符的索引
                    int length = tbxConsole.GetLineLength(ConsoleMaxLine);                  // 末尾行字符串的长度
                    tbxConsole.Select(start, start + length);                               // 选中末尾一行
                    tbxConsole.SelectedText = "END";
                }
            }
        }


        /// <summary>
        /// 清空Console中的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            tbxConsole.Text = "";
        }

        /// <summary>
        /// 测试日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestLog_Click(object sender, RoutedEventArgs e)
        {
            
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

                Int16 HandleLen = 0;
                Int16 ExeError = 0;

                for (UInt16 iCnt = 0; iCnt < SrcLen; iCnt++)
                {
                    try
                    {
                        HandleLen = RxPkt_IsRight(e.ReceivedBytes, iCnt);
                        if (HandleLen < 0)
                        {
                            continue;
                        }

                        switch (e.ReceivedBytes[iCnt + 3])
                        {
                            case 0x00:
                                {
                                    break;
                                }                            
                            default:
                                {
                                    break;
                                }
                        }

                        if (ExeError < 0)
                        {
                            continue;
                        }

                        if (HandleLen > 0)
                        {
                            HandleLen--;        // 因为马上就要执行iCnt++
                        }

                        iCnt = (UInt16)(iCnt + HandleLen);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("处理接收数据包错误" + ex.Message);
                    }
                }
            }));
        }


    }
}
