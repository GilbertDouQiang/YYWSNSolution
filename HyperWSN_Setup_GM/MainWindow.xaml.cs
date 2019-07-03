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
using System.Timers;
using System.Windows.Threading;
using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;

namespace HyperWSN_Setup_GM
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
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
                SerialPort = new SerialPortHelper();
                SerialPort.SerialPortReceived += Comport_SerialPortReceived;
                string portname = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());
                SerialPort.InitCOM(portname);
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
        /// 查询本地配置
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_ReadCfg(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 44)
            {
                return -1;
            }

            tbxMac.Text = SrcData[IndexOfStart + 4].ToString("X2") + " " + SrcData[IndexOfStart + 5].ToString("X2") + " " + SrcData[IndexOfStart + 6].ToString("X2") + " " + SrcData[IndexOfStart + 7].ToString("X2");
            tbxHwRevision.Text = SrcData[IndexOfStart + 8].ToString("X2") + " " + SrcData[IndexOfStart + 9].ToString("X2") + " " + SrcData[IndexOfStart + 10].ToString("X2") + " " + SrcData[IndexOfStart + 11].ToString("X2");
            tbxSwRevision.Text = SrcData[IndexOfStart + 12].ToString("X2") + " " + SrcData[IndexOfStart + 13].ToString("X2");
            tbxCustomer.Text = SrcData[IndexOfStart + 14].ToString("X2") + " " + SrcData[IndexOfStart + 15].ToString("X2");
            tbxDebug.Text = SrcData[IndexOfStart + 16].ToString("X2") + " " + SrcData[IndexOfStart + 17].ToString("X2");
            tbxCategory.Text = SrcData[IndexOfStart + 18].ToString("X2");
            tbxInterval.Text = (SrcData[IndexOfStart + 19] * 256 + SrcData[IndexOfStart + 20]).ToString("D");
            tbxCalendar.Text = "20"+ SrcData[IndexOfStart + 21].ToString("X2") + "-" + SrcData[IndexOfStart + 22].ToString("X2") + "-" + SrcData[IndexOfStart + 23].ToString("X2") + " " + SrcData[IndexOfStart + 24].ToString("X2") + ":" + SrcData[IndexOfStart + 25].ToString("X2") + ":" + SrcData[IndexOfStart + 26].ToString("X2");
            tbxPattern.Text = SrcData[IndexOfStart + 27].ToString("X2");
            tbxBps.Text = SrcData[IndexOfStart + 28].ToString("X2");
            tbxChannel.Text = SrcData[IndexOfStart + 29].ToString("D");
            tbxTxPower.Text = 15.ToString("D");
            tbxRam.Text = SrcData[IndexOfStart + 30].ToString("D");
            UInt16 Value = (UInt16)(SrcData[IndexOfStart + 31] * 256 + SrcData[IndexOfStart + 32]);
            tbxFront.Text = Value.ToString("D");
            Value = (UInt16)(SrcData[IndexOfStart + 33] * 256 + SrcData[IndexOfStart + 34]);
            tbxRear.Text = Value.ToString("D");
            Value = (UInt16)(SrcData[IndexOfStart + 35] * 256 + SrcData[IndexOfStart + 36]);
            tbxQueueLen.Text = Value.ToString("D");
            tbxSendOk.Text = SrcData[IndexOfStart + 37].ToString("X2");
            double Volt = (double)(SrcData[IndexOfStart + 38] * 256 + SrcData[IndexOfStart + 39]) / 1000.0f;
            tbxVolt.Text = Volt.ToString("F3");

            return 0;
        }

        /// <summary>
        /// 修改本地配置，命令位是0x02
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_WriteCfgV1(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 25)
            {
                return -1;
            }

            tbxMac.Text = SrcData[IndexOfStart + 4].ToString("X2") + " " + SrcData[IndexOfStart + 5].ToString("X2") + " " + SrcData[IndexOfStart + 6].ToString("X2") + " " + SrcData[IndexOfStart + 7].ToString("X2");
            tbxHwRevision.Text = SrcData[IndexOfStart + 8].ToString("X2") + " " + SrcData[IndexOfStart + 9].ToString("X2") + " " + SrcData[IndexOfStart + 10].ToString("X2") + " " + SrcData[IndexOfStart + 11].ToString("X2");
            tbxCustomer.Text = SrcData[IndexOfStart + 12].ToString("X2") + " " + SrcData[IndexOfStart + 13].ToString("X2");
            tbxDebug.Text = SrcData[IndexOfStart + 14].ToString("X2") + " " + SrcData[IndexOfStart + 15].ToString("X2");
            tbxCategory.Text = SrcData[IndexOfStart + 16].ToString("X2");
            tbxInterval.Text = (SrcData[IndexOfStart + 17] * 256 + SrcData[IndexOfStart + 18]).ToString("D");
            tbxPattern.Text = SrcData[IndexOfStart + 19].ToString("X2");
            tbxBps.Text = SrcData[IndexOfStart + 20].ToString("X2");

            return 0;
        }

        /// <summary>
        /// 修改本地配置，命令位是0x84
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_WriteCfgV2(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 31)
            {
                return -1;
            }

            tbxMac.Text = SrcData[IndexOfStart + 4].ToString("X2") + " " + SrcData[IndexOfStart + 5].ToString("X2") + " " + SrcData[IndexOfStart + 6].ToString("X2") + " " + SrcData[IndexOfStart + 7].ToString("X2");
            tbxHwRevision.Text = SrcData[IndexOfStart + 8].ToString("X2") + " " + SrcData[IndexOfStart + 9].ToString("X2") + " " + SrcData[IndexOfStart + 10].ToString("X2") + " " + SrcData[IndexOfStart + 11].ToString("X2");
            tbxCustomer.Text = SrcData[IndexOfStart + 12].ToString("X2") + " " + SrcData[IndexOfStart + 13].ToString("X2");
            tbxDebug.Text = SrcData[IndexOfStart + 14].ToString("X2") + " " + SrcData[IndexOfStart + 15].ToString("X2");
            tbxCategory.Text = SrcData[IndexOfStart + 16].ToString("X2");
            tbxInterval.Text = (SrcData[IndexOfStart + 17] * 256 + SrcData[IndexOfStart + 18]).ToString("D");
            tbxPattern.Text = SrcData[IndexOfStart + 19].ToString("X2");
            tbxBps.Text = SrcData[IndexOfStart + 20].ToString("X2");
            tbxChannel.Text = SrcData[IndexOfStart + 21].ToString("D");
            tbxTxPower.Text = 15.ToString("D");

            return 0;
        }

        /// <summary>
        /// 删除已存数据
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_DeleteHistory(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 18)
            {
                return -1;
            }

            tbxMac.Text = SrcData[IndexOfStart + 4].ToString("X2") + " " + SrcData[IndexOfStart + 5].ToString("X2") + " " + SrcData[IndexOfStart + 6].ToString("X2") + " " + SrcData[IndexOfStart + 7].ToString("X2");
            tbxFront.Text = (SrcData[IndexOfStart + 8] * 256 + SrcData[IndexOfStart + 9]).ToString("D");
            tbxRear.Text = (SrcData[IndexOfStart + 10] * 256 + SrcData[IndexOfStart + 11]).ToString("D");
            tbxQueueLen.Text = (SrcData[IndexOfStart + 12] * 256 + SrcData[IndexOfStart + 13]).ToString("D");

            return 0;
        }

        /// <summary>
        /// 授时
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_Ntp(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 21)
            {
                return -1;
            }

            if(SrcData[IndexOfStart + 8] != 0x08 || SrcData[IndexOfStart + 9] != 0x11 | SrcData[IndexOfStart + 10] != 0x06)
            {
                return -2;
            }

            tbxNtpCalendar.Text = "20" + SrcData[IndexOfStart + 11].ToString("X2") + "-" + SrcData[IndexOfStart + 12].ToString("X2") + "-" + SrcData[IndexOfStart + 13].ToString("X2") + " " + SrcData[IndexOfStart + 14].ToString("X2") + ":" + SrcData[IndexOfStart + 15].ToString("X2") + ":" + SrcData[IndexOfStart + 16].ToString("X2");

            return 0;
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
                            case 0x01:
                                {
                                    ExeError = RxPkt_ReadCfg(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x02:
                                {
                                    ExeError = RxPkt_WriteCfgV1(e.ReceivedBytes, iCnt);                                    
                                    break; 
                                }
                            case 0x03:
                                {
                                    ExeError = RxPkt_DeleteHistory(e.ReceivedBytes, iCnt);
                                    break; 
                                }
                            case 0x04:
                                {
                                    ExeError = RxPkt_Ntp(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x84:
                                {
                                    ExeError = RxPkt_WriteCfgV2(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }

                        if(ExeError < 0)
                        {
                            continue;
                        }

                        if(HandleLen > 0)
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

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReadCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[14];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x01;

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort.SendCommandByLength(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 将左列的配置复制填充到右列里去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopyCfg_Click(object sender, RoutedEventArgs e)
        {
            tbxMacNew.Text = tbxMac.Text;
            tbxHwRevisionNew.Text = tbxHwRevision.Text;
            tbxCustomerNew.Text = tbxCustomer.Text;
            tbxDebugNew.Text = tbxDebug.Text;
            tbxCategoryNew.Text = tbxCategory.Text;
            tbxIntervalNew.Text = tbxInterval.Text;
            tbxPatternNew.Text = tbxPattern.Text;
            tbxBpsNew.Text = tbxBps.Text;
            tbxChannelNew.Text = tbxChannel.Text;
            tbxTxPowerNew.Text = tbxTxPower.Text;
        }

        /// <summary>
        /// 将左列的配置清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearCfg_Click(object sender, RoutedEventArgs e)
        {
            tbxMac.Text = "";
            tbxHwRevision.Text = "";
            tbxSwRevision.Text = "";
            tbxCustomer.Text = "";
            tbxDebug.Text = "";
            tbxCategory.Text = "";
            tbxInterval.Text = "";
            tbxCalendar.Text = "";
            tbxPattern.Text = "";
            tbxBps.Text = "";
            tbxChannel.Text = "";
            tbxTxPower.Text = "";
            tbxRam.Text = "";
            tbxFront.Text = "";
            tbxRear.Text = "";
            tbxQueueLen.Text = "";
            tbxSendOk.Text = "";
            tbxVolt.Text = "";
        }

        /// <summary>
        /// 修改配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWriteCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[36];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                // 读取软件版本
                byte SwRevision = 0;
                byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxSwRevision.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                {
                    SwRevision = 0;
                }
                else
                {
                    SwRevision = ByteBufTmp[1];
                }
                // 若是软件版本>=0xA4 0x17，则使用0x84的修改指令，否则使用0x02的修改指令
                if (SwRevision >= 0x17)
                {
                    TxBuf[TxLen++] = 0x84;
                }
                else
                {
                    TxBuf[TxLen++] = 0x02;
                }

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // New GW ID
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxMacNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];

                // Hardware Revision
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxHwRevisionNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];

                // Customer
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCustomerNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];

                // Debug
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDebugNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];

                // Category
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCategoryNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 1)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];

                // Interval
                UInt16 Interval = Convert.ToUInt16(tbxIntervalNew.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Pattern
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxPatternNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 1)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];

                // Bps
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxBpsNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 1)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];

                // channel
                if (SwRevision >= 0x17)
                {
                    TxBuf[TxLen++] = Convert.ToByte(tbxChannelNew.Text);
                }
                else
                {
                    // 空
                }

                // TxPower and Reserved
                if (SwRevision >= 0x17)
                {
                    TxBuf[TxLen++] = (byte)Convert.ToInt16(tbxTxPowerNew.Text);
                    TxBuf[TxLen++] = 0x00;
                    TxBuf[TxLen++] = 0x00;
                    TxBuf[TxLen++] = 0x00;
                }
                else
                {
                    // 空
                }

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort.SendCommandByLength(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 删除已存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[18];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x03;

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // Front
                UInt16 Front = Convert.ToUInt16(tbxFrontNew.Text);
                TxBuf[TxLen++] = (byte)((Front & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Front & 0x00FF) >> 0);

                // Rear
                UInt16 Rear = Convert.ToUInt16(tbxRearNew.Text);
                TxBuf[TxLen++] = (byte)((Rear & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Rear & 0x00FF) >> 0);

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort.SendCommandByLength(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 授时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[24];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x04;

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // Payload length
                TxBuf[TxLen++] = 0x08;

                // 授时时间
                TxBuf[TxLen++] = 0x15;
                TxBuf[TxLen++] = 0x06;

                if(cbxAuto.IsChecked == true)
                {
                    tbxNtpCalendar.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                DateTime NtpCalendar = Convert.ToDateTime(tbxNtpCalendar.Text);
                byte[] ByteBufTmp = MyCustomFxn.DataTimeToByteArray(NtpCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort.SendCommandByLength(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 更新工厂信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdateFactory_Click(object sender, RoutedEventArgs e)
        {
            /*
            M1 updateDevice = new M1();
            try
            {
                updateDevice.DeviceMacS = txtDeviceMAC.Text;
                updateDevice.DeviceNewMAC = txtNewDeviceMAC.Text;
                updateDevice.HwVersionS = txtNewHardwareVersion.Text;
                //兼容M1 and M1P
                if (txtDeviceName.Text == "M1")
                {
                    updateDevice.DeviceTypeS = "51";
                }
                else if (txtDeviceName.Text == "M1P")
                {
                    updateDevice.DeviceTypeS = "53";
                }
                else if (txtDeviceName.Text == "M2")
                {
                    updateDevice.DeviceTypeS = "57";
                }

                byte[] updateCommand = updateDevice.UpdateFactory();
                string updateString = CommArithmetic.ToHexString(updateCommand);
                SerialPort.SendCommand(updateCommand);
                System.Threading.Thread.Sleep(200); //界面会卡
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
            */
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
    }
}
