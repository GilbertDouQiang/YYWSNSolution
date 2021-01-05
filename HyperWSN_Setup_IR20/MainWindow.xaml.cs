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
        bool CmdMode = false;                       // false = 十六进制命令模式; true = 字符串命令模式;

        byte[] Encrypt = new byte[8];               // 身份认证时的明文
        UInt32 PassCode = 0;                        // 身份认证成功后得到的认证码

        public MainWindow()
        {
            InitializeComponent();

            this.Title += "  v" +
           System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

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
        /// 在Console显示日志
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ConsoleLog(string direct, byte[] Buf, UInt16 Start, UInt16 Len)
        {
            if (direct == null && (Buf == null || Buf.Length == 0 || Len == 0))
            {
                return;
            }

            if (chkLockLog.IsChecked == true)
            {
                return;
            }

            if (Buf == null)
            {
                tbxConsole.Text = "\r\n" + Logger.GetTimeString() + "\t" + direct + "\r\n" + tbxConsole.Text;
            }
            else
            {
                tbxConsole.Text = Logger.GetTimeString() + "\t" + direct + "\t" + MyCustomFxn.ToHexString(Buf, Start, Len) + "\r\n" + tbxConsole.Text;
            }

            UInt16 ConsoleMaxLine = Convert.ToUInt16(txtLogLineLimit.Text);
            if (tbxConsole.LineCount > ConsoleMaxLine)
            {
                int start = tbxConsole.GetCharacterIndexFromLineIndex(ConsoleMaxLine);  // 末尾行第一个字符的索引
                int length = tbxConsole.GetLineLength(ConsoleMaxLine);                  // 末尾行字符串的长度
                tbxConsole.Select(start, start + length);                               // 选中末尾一行
                tbxConsole.SelectedText = "END";
            }
        }

        private void ConsoleLog(string direct, byte[] Buf)
        {
            if (Buf == null)
            {
                ConsoleLog(direct, null, 0, 0);
            }
            else
            {
                ConsoleLog(direct, Buf, 0, (UInt16)Buf.Length);
            }
        }

        private int SerialPort_isReady()
        {
            if (SerialPort == null)
            {
                MessageBox.Show("请初始化串口！");
                return -1;
            }

            if (SerialPort.IsOpen() == false)
            {
                MessageBox.Show("请打开串口！");
                return -2;
            }

            return 0;
        }

        private byte[] SerialPort_SendReceive(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen, UInt16 RxTimeoutMs)
        {
            if (SerialPort_isReady() < 0)
            {
                return null;
            }

            // 在Console显示Log
            ConsoleLog("TX", TxBuf, IndexOfStart, TxLen);

            // 发送数据
            byte[] RxBuf = SerialPort.SendReceive(TxBuf, IndexOfStart, TxLen, RxTimeoutMs);

            // 在Console显示Log
            ConsoleLog("RX", RxBuf);

            return RxBuf;
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

            SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 200);
        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            byte[] txPkt = aIR20.TxPkt_Ping();

            byte[] RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 200);
            if (RxBuf == null)
            {
                return;
            }

            int error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
            }
        }

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (e.ReceivedBytes == null)
                {
                    return;
                }

                // 显示Log
                ConsoleLog("RX", e.ReceivedBytes, 0, (UInt16)e.ReceivedBytes.Length);

                // 处理接收到的数据包
                RxPkt_Handle(e.ReceivedBytes);
            }));
        }

        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        /// <param name="SrcData"></param>
        /// <returns></returns>
        private Int16 RxPkt_Handle(byte[] RxBuf)
        {
            if (RxBuf == null)
            {
                return -1;
            }

            if (RxBuf.Length == 1 && (RxBuf[0] == 0 || RxBuf[0] == 0x30))
            {
                if (RxBuf[0] == 0)
                {
                    tbxPing.Text = RxBuf[0].ToString("X2");
                }
                else
                {
                    tbxPing.Text = "\"0\"";
                }
            }

            int HandleLen = 0;
            int ExeError = 0;

            for (int iCnt = 0; iCnt < RxBuf.Length; iCnt++)
            {
                try
                {
                    HandleLen = RxPkt_IsRight(RxBuf, iCnt);
                    if (HandleLen < 0)
                    {
                        continue;
                    }

                    switch (RxBuf[iCnt + 2])
                    {
                        case 0x01:
                            {
                                ExeError = RxPkt_ReadCfg(RxBuf, iCnt);
                                break;
                            }
                        case 0x05:
                            {
                                ExeError = RxPkt_Authenticate(RxBuf, iCnt);
                                break;
                            }
                        case 0x06:
                            {
                                ExeError = RxPkt_GetBodyTemp(RxBuf, iCnt);
                                break;
                            }
                        case 0x08:
                            {
                                ExeError = RxPkt_GetAllTemp(RxBuf, iCnt);
                                break;
                            }
                        case 0x37:
                            {
                                ExeError = RxPkt_WriteCfg(RxBuf, iCnt);
                                break;
                            }
                        case 0x49:
                            {
                                ExeError = RxPkt_WriteKey(RxBuf, iCnt);
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

                    iCnt = iCnt + HandleLen;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("处理接收数据包错误" + ex.Message);
                }
            }
            return 0;
        }

        /// <summary>
        /// 判断收到的数据包是否符合格式要求
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        private Int16 RxPkt_IsRight(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(RxBuf.Length - Start);
            if (SrcLen < 7)
            {
                return -1;
            }

            // 起始位
            if (RxBuf[Start + 0] != 0xDF)
            {
                return -2;
            }

            // 长度位
            byte pktLen = RxBuf[Start + 1];
            if (pktLen + 5 > SrcLen)
            {
                return -3;
            }

            if (RxBuf[Start + 2 + pktLen + 2] != 0xFD)
            {
                return -4;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, RxBuf, (UInt16)(Start + 2), pktLen);
            UInt16 crc_chk = (UInt16)(RxBuf[Start + 2 + pktLen + 0] * 256 + RxBuf[Start + 2 + pktLen + 1]);
            if (crc_chk != crc && crc_chk != 0)
            {
                return -5;
            }

            return (Int16)(pktLen + 5);
        }

        /// <summary>
        /// 查询本地配置
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_ReadCfg(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            UInt16 PktLen = (UInt16)(RxBuf.Length - Start);
            if (PktLen < 6)
            {
                return -1;
            }

            // Protocol  
            if (RxBuf[Start + 3] != 0x01)
            {
                return -2;
            }

            byte total = RxBuf[Start + 4];
            byte iPage = RxBuf[Start + 5];

            if (iPage >= total)
            {
                return -3;
            }

            if (tabItemCfg.IsSelected == true)
            {
                if (iPage == 0)
                {
                    tbxCfgInCfgTab1.Text = "[设备类型=" + RxBuf[Start + 6].ToString("X2") + "]";

                    tbxDeviceMac.Text = RxBuf[Start + 7].ToString("X2") + " " + RxBuf[Start + 8].ToString("X2") + " " + RxBuf[Start + 9].ToString("X2") + " " + RxBuf[Start + 10].ToString("X2");

                    tbxCfgInCfgTab1.Text += ",  [BLE MAC=" + RxBuf[Start + 11].ToString("X2") + " " + RxBuf[Start + 12].ToString("X2") + " " + RxBuf[Start + 13].ToString("X2") + " " + RxBuf[Start + 14].ToString("X2") + " " + RxBuf[Start + 15].ToString("X2") + " " + RxBuf[Start + 16].ToString("X2") + "]";
                }
                else if (iPage == 1)
                {
                    tbxHwRevision.Text = RxBuf[Start + 6].ToString("X2") + " " + RxBuf[Start + 7].ToString("X2") + " " + RxBuf[Start + 8].ToString("X2") + " " + RxBuf[Start + 9].ToString("X2");

                    tbxCfgInCfgTab1.Text += ",  [软件版本=" + RxBuf[Start + 10].ToString("X2") + " " + RxBuf[Start + 11].ToString("X2") + " " + RxBuf[Start + 12].ToString("X2") + " " + RxBuf[Start + 13].ToString("X2") + "]";
                    tbxCfgInCfgTab1.Text += ",  [传感器=" + RxBuf[Start + 14].ToString("X2") + " " + RxBuf[Start + 15].ToString("X2") + " " + RxBuf[Start + 16].ToString("X2") + "]";

                    if (RxBuf[Start + 15] == 0x01)
                    {
                        tbxCfgInCfgTab1.Text += ", [Medical]";
                    }
                    else if (RxBuf[Start + 15] == 0x02)
                    {
                        tbxCfgInCfgTab1.Text += ", [Standard]";
                    }
                }
                else if (iPage == 2)
                {
                    tbxCfgInCfgTab2.Text = "[重启原因=" + RxBuf[Start + 6].ToString("X2") + "]";

                    cbxCmdMode.SelectedIndex = RxBuf[Start + 7];

                    Int16 objCom_i = (Int16)(((UInt16)RxBuf[Start + 8] << 8) | ((UInt16)RxBuf[Start + 9] << 0));
                    double objCom_f = (double)objCom_i / 100.0f;
                    tbxObjTempCom.Text = objCom_f.ToString("F2");
                }
            }
            else
            {
                if (iPage == 0)
                {
                    tbxReadCfg.Text = "[" + RxBuf[Start + 7].ToString("X2") + " " + RxBuf[Start + 8].ToString("X2") + " " + RxBuf[Start + 9].ToString("X2") + " " + RxBuf[Start + 10].ToString("X2") + "], ";
                    tbxReadCfg.Text += "[" + RxBuf[Start + 11].ToString("X2") + " " + RxBuf[Start + 12].ToString("X2") + " " + RxBuf[Start + 13].ToString("X2") + " " + RxBuf[Start + 14].ToString("X2") + " " + RxBuf[Start + 15].ToString("X2") + " " + RxBuf[Start + 16].ToString("X2") + "]";
                }
                else if (iPage == 1)
                {
                    tbxReadCfg.Text += ", [" + RxBuf[Start + 6].ToString("X2") + " " + RxBuf[Start + 7].ToString("X2") + " " + RxBuf[Start + 8].ToString("X2") + " " + RxBuf[Start + 9].ToString("X2") + "], ";
                    tbxReadCfg.Text += "[" + RxBuf[Start + 10].ToString("X2") + " " + RxBuf[Start + 11].ToString("X2") + " " + RxBuf[Start + 12].ToString("X2") + " " + RxBuf[Start + 13].ToString("X2") + "]";
                }
                else if (iPage == 2)
                {
                    tbxReadCfg.Text += ", [重启原因=" + RxBuf[Start + 6].ToString("X2") + "]";
                    if (RxBuf[Start + 7] == 0)
                    {
                        tbxReadCfg.Text += ", [HEX]";
                    }
                    else
                    {
                        tbxReadCfg.Text += ", [String]";
                    }

                    Int16 objCom_i = (Int16)(((UInt16)RxBuf[Start + 8] << 8) | ((UInt16)RxBuf[Start + 9] << 0));
                    double objCom_f = (double)objCom_i / 100.0f;
                    tbxReadCfg.Text += ", [Obj补偿=" + objCom_f.ToString("F2") + "]";
                }
            }

            return 0;
        }

        /// <summary>
        /// 身份认证
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_Authenticate(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            UInt16 PktLen = (UInt16)(RxBuf.Length - Start);
            if (PktLen < 13)
            {
                return -1;
            }

            // Protocol  
            if (RxBuf[Start + 3] != 0x01)
            {
                return -2;
            }

            // 步骤
            byte step = RxBuf[Start + 4];
            if (step < 1 || step > 2)
            {
                return -3;
            }

            if (step == 1)
            {   // 明文

                if (PktLen < 16)
                {
                    return -4;
                }

                tbxAuthenticate.Text = "[Encrypt=" + RxBuf[Start + 5].ToString("X2") + " " + RxBuf[Start + 6].ToString("X2") + " " + RxBuf[Start + 7].ToString("X2") + " " + RxBuf[Start + 8].ToString("X2") + " " + RxBuf[Start + 9].ToString("X2") + " " + RxBuf[Start + 10].ToString("X2") + " " + RxBuf[Start + 11].ToString("X2") + " " + RxBuf[Start + 12].ToString("X2") + "]";

                Encrypt[0] = RxBuf[Start + 5];
                Encrypt[1] = RxBuf[Start + 6];
                Encrypt[2] = RxBuf[Start + 7];
                Encrypt[3] = RxBuf[Start + 8];
                Encrypt[4] = RxBuf[Start + 9];
                Encrypt[5] = RxBuf[Start + 10];
                Encrypt[6] = RxBuf[Start + 11];
                Encrypt[7] = RxBuf[Start + 12];
            }
            else if (step == 2)
            {
                // 认证结果
                Int16 Result = RxBuf[Start + 5];
                if (Result >= 0x80)
                {
                    Result = (Int16)(Result - 0x100);
                }
                if (Result < 0)
                {   // 认证失败
                    tbxAuthenticate.Text += ", [认证失败]";
                }
                else
                {   // 认证成功
                    tbxAuthenticate.Text += ", [认证成功]";
                }

                // 认证码
                PassCode = ((UInt32)RxBuf[Start + 6] << 24) | ((UInt32)RxBuf[Start + 7] << 16) | ((UInt32)RxBuf[Start + 8] << 8) | ((UInt32)RxBuf[Start + 9] << 0);
                tbxAuthenticate.Text += ", [" + RxBuf[Start + 6].ToString("X2") + " " + RxBuf[Start + 7].ToString("X2") + " " + RxBuf[Start + 8].ToString("X2") + " " + RxBuf[Start + 9].ToString("X2") + "]";
            }

            return 0;
        }

        /// <summary>
        /// 测量一次体温
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        private Int16 RxPkt_GetBodyTemp(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            UInt16 PktLen = (UInt16)(RxBuf.Length - Start);
            if (PktLen < 15)
            {
                return -1;
            }

            // Protocol  
            if (RxBuf[Start + 3] != 0x01)
            {
                return -2;
            }

            // 执行结果
            Int16 Result = (Int16)RxBuf[Start + 4];
            if (Result >= 0x80)
            {
                Result = (Int16)(Result - 0x100);
            }

            if (Result < 0)
            {
                tbxGetBodyTemp.Text = "[失败]";
                return -3;              // 执行失败
            }

            tbxGetBodyTemp.Text = "[成功]";

            // 状态
            byte Status = RxBuf[Start + 5];
            if (Status > 2)
            {
                return -4;
            }

            if (Status == 0)
            {
                tbxGetBodyTemp.Text += ", [正常]";
            }
            else if (Status == 1)
            {
                tbxGetBodyTemp.Text += ", [过低]";
            }
            else if (Status == 2)
            {
                tbxGetBodyTemp.Text += ", [过高]";
            }

            // 体温
            Int16 temp_i = (Int16)(((UInt16)RxBuf[Start + 6] << 8) | ((UInt16)RxBuf[Start + 7] << 0));
            double temp_f = (double)temp_i / 100.0f;
            tbxGetBodyTemp.Text += ", [Body=" + temp_f.ToString("F2") + "℃]";

            // 物体温度
            temp_i = (Int16)(((UInt16)RxBuf[Start + 8] << 8) | ((UInt16)RxBuf[Start + 9] << 0));
            temp_f = (double)temp_i / 100.0f;
            tbxGetBodyTemp.Text += ", [Object=" + temp_f.ToString("F2") + "℃]";

            // 环境温度
            temp_i = (Int16)(((UInt16)RxBuf[Start + 10] << 8) | ((UInt16)RxBuf[Start + 11] << 0));
            temp_f = (double)temp_i / 100.0f;
            tbxGetBodyTemp.Text += ", [Ambient=" + temp_f.ToString("F2") + "℃]";

            return 0;
        }

        /// <summary>
        /// 测量一次温度
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        private Int16 RxPkt_GetAllTemp(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            UInt16 PktLen = (UInt16)(RxBuf.Length - Start);
            if (PktLen < 14)
            {
                return -1;
            }

            // Protocol  
            if (RxBuf[Start + 3] != 0x01)
            {
                return -2;
            }

            // 执行结果
            Int16 Result = (Int16)RxBuf[Start + 4];
            if (Result >= 0x80)
            {
                Result = (Int16)(Result - 0x100);
            }

            if (Result < 0)
            {
                tbxGetAllTemp.Text = "[失败]";
                return -3;              // 执行失败
            }

            tbxGetAllTemp.Text = "[成功]";

            // 体温
            Int16 temp_i = (Int16)(((UInt16)RxBuf[Start + 5] << 8) | ((UInt16)RxBuf[Start + 6] << 0));
            double temp_f = (double)temp_i / 100.0f;
            tbxGetAllTemp.Text += ", [Body=" + temp_f.ToString("F2") + "℃]";

            // 物体温度
            temp_i = (Int16)(((UInt16)RxBuf[Start + 7] << 8) | ((UInt16)RxBuf[Start + 8] << 0));
            temp_f = (double)temp_i / 100.0f;
            tbxGetAllTemp.Text += ", [Object=" + temp_f.ToString("F2") + "℃]";

            // 环境温度
            temp_i = (Int16)(((UInt16)RxBuf[Start + 9] << 8) | ((UInt16)RxBuf[Start + 10] << 0));
            temp_f = (double)temp_i / 100.0f;
            tbxGetAllTemp.Text += ", [Ambient=" + temp_f.ToString("F2") + "℃]";

            return 0;
        }

        /// <summary>
        /// 修改设备配置
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        private Int16 RxPkt_WriteCfg(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            UInt16 PktLen = (UInt16)(RxBuf.Length - Start);
            if (PktLen < 8)
            {
                return -1;
            }

            // Protocol  
            if (RxBuf[Start + 3] != 0x01)
            {
                return -2;
            }

            // 执行结果
            Int16 Result = (Int16)RxBuf[Start + 4];
            if (Result >= 0x80)
            {
                Result = (Int16)(Result - 0x100);
            }

            if (Result < 0)
            {
                tbxResult.Text = "[失败]";
                return -3;              // 执行失败
            }

            tbxResult.Text = "[成功]";

            return 0;
        }

        /// <summary>
        /// 设置密钥
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        private Int16 RxPkt_WriteKey(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            UInt16 PktLen = (UInt16)(RxBuf.Length - Start);
            if (PktLen < 8)
            {
                return -1;
            }

            // Protocol  
            if (RxBuf[Start + 3] != 0x01)
            {
                return -2;
            }

            // 执行结果
            Int16 Result = (Int16)RxBuf[Start + 4];
            if (Result >= 0x80)
            {
                Result = (Int16)(Result - 0x100);
            }

            if (Result < 0)
            {
                tbxResult.Text = "[失败]";
                return -3;              // 执行失败
            }

            tbxResult.Text = "[成功]";

            return 0;
        }

        private void btnAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            byte[] txPkt = aIR20.TxPkt_Authenticate(null, tbxKey.Text);
            byte[] RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 400);
            if (RxBuf == null)
            {
                return;
            }

            int error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
            }

            txPkt = aIR20.TxPkt_Authenticate(Encrypt, tbxKey.Text);
            RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 400);
            if (RxBuf == null)
            {
                return;
            }

            error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
            }
        }

        private void btnGetBodyTemp_Click(object sender, RoutedEventArgs e)
        {
            double bodyTempThrLow = 35.0f;
            double bodyTempThrHigh = 37.3f;

            try
            {
                bodyTempThrLow = Convert.ToDouble(tbxBodyTempThrLow.Text);
                bodyTempThrHigh = Convert.ToDouble(tbxBodyTempThrHigh.Text);
            }
            catch
            {
                MessageBox.Show("正常温度范围错误");
                return;
            }            
            
            if(bodyTempThrHigh < bodyTempThrLow)
            {   // 交换上下限
                double temp = 0.0f;
                temp = bodyTempThrHigh;
                bodyTempThrHigh = bodyTempThrLow;
                bodyTempThrLow = temp;
            }  

            byte[] txPkt = aIR20.TxPkt_GetBodyTemp(PassCode, bodyTempThrLow, bodyTempThrHigh);
            byte[] RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 400);
            if (RxBuf == null)
            {
                return;
            }

            int error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
            }
        }

        private void btnGetAllTemp_Click(object sender, RoutedEventArgs e)
        {
            byte[] txPkt = aIR20.TxPkt_GetAllTemp(PassCode);
            byte[] RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 400);
            if (RxBuf == null)
            {
                return;
            }

            int error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            byte[] txPkt = aIR20.TxPkt_Reset();
            SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 0);
        }

        private void btnReadCfg_Click(object sender, RoutedEventArgs e)
        {
            byte[] txPkt = aIR20.TxPkt_ReadCfg();

            byte[] RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 400);
            if (RxBuf == null)
            {
                return;
            }

            int error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
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

        private void btnReadCfgInCfgTab_Click(object sender, RoutedEventArgs e)
        {
            btnReadCfg_Click(sender, e);
        }

        private void btnCopyInCfgTab_Click(object sender, RoutedEventArgs e)
        {
            tbxNewDeviceMac.Text = tbxDeviceMac.Text;
            tbxNewHwRevision.Text = tbxHwRevision.Text;
            cbxNewCmdMode.SelectedIndex = cbxCmdMode.SelectedIndex;
            tbxNewObjTempCom.Text = tbxObjTempCom.Text;
        }

        private void btnClearInCfgTab_Click(object sender, RoutedEventArgs e)
        {
            tbxDeviceMac.Text = "";
            tbxHwRevision.Text = "";
            tbxCfgInCfgTab1.Text = "";
            tbxCfgInCfgTab2.Text = "";
            cbxCmdMode.SelectedIndex = -1;
            tbxObjTempCom.Text = "";
        }

        private void btnWriteCfgInCfgTab_Click(object sender, RoutedEventArgs e)
        {
            UInt32 NewDeviceMac = 0;
            UInt32 NewHwRevision = 0;
            byte Mode = (byte)cbxNewCmdMode.SelectedIndex;
            double objTempCom = 0.0f;

            byte[] ByteBuf = MyCustomFxn.HexStringToByteArray(tbxNewDeviceMac.Text);
            if (ByteBuf == null || ByteBuf.Length < 4)
            {
                MessageBox.Show("Device Mac错误！");
                return;
            }

            NewDeviceMac = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);

            ByteBuf = MyCustomFxn.HexStringToByteArray(tbxNewHwRevision.Text);
            if (ByteBuf == null || ByteBuf.Length < 4)
            {
                MessageBox.Show("硬件版本错误！");
                return;
            }

            NewHwRevision = ((UInt32)ByteBuf[0] << 24) | ((UInt32)ByteBuf[1] << 16) | ((UInt32)ByteBuf[2] << 8) | ((UInt32)ByteBuf[3] << 0);

            try
            {
                objTempCom = Convert.ToDouble(tbxNewObjTempCom.Text);
            }
            catch
            {
                MessageBox.Show("表面温度补偿错误！");
                return;
            }

            tbxResult.Text = "";

            byte[] txPkt = aIR20.TxPkt_WriteCfg(NewDeviceMac, NewHwRevision, Mode, objTempCom);

            byte[] RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 400);
            if (RxBuf == null)
            {
                return;
            }

            int error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
            }

            btnReadCfgInCfgTab_Click(sender, e);
        }

        private void btnWriteKeyInCfgTab_Click(object sender, RoutedEventArgs e)
        {
            byte[] NewKey = MyCustomFxn.HexStringToByteArray(tbxNewKey.Text);
            if (NewKey == null || NewKey.Length < 16)
            {
                MessageBox.Show("密钥错误！");
                return;
            }

            tbxResult.Text = "";

            byte[] txPkt = aIR20.TxPkt_WriteKey(NewKey);

            byte[] RxBuf = SerialPort_SendReceive(txPkt, 0, (UInt16)txPkt.Length, 400);
            if (RxBuf == null)
            {
                return;
            }

            int error = RxPkt_Handle(RxBuf);
            if (error < 0)
            {
                return;
            }
        }

        /// <summary>
        /// 将密钥设为默认值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDefaultKeyInCfgTab_Click(object sender, RoutedEventArgs e)
        {
            tbxNewKey.Text = "67452301EFCDAB8998BADCFE10325476";
            btnWriteKeyInCfgTab_Click(sender, e);
        }
    }
}
