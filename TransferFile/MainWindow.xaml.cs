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
using System.Configuration;
using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;

namespace TransferFile
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper SerialPort;
        Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
        BinaryFile bFile;

        UInt16 PktMaxLen = 255;         // 数据包的最大长度
        UInt16 PassCode = 0;            // 传输码
        UInt32 TransSerial = 0;         // 传输序列号
        bool TransSuc = false;          // 传输成功？

        UInt32 PartMaxLen = 124 * 1024; // 文件分片的标准，单位：B

        public MainWindow()
        {
            InitializeComponent();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += "  [Rev " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "]";

            FindComport();
        }

        private void btnFindComport_Click(object sender, RoutedEventArgs e)
        {
            FindComport();
        }

        public void Serial_Init()
        {
            string ComportName = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedItem.ToString());

            int baudRate = 115200;

            if (tbxBaudRate.Text != string.Empty && tbxBaudRate.Text.Length != 0)
            {
                try
                {
                    baudRate = Convert.ToInt32(tbxBaudRate.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("串口波特率错误！");
                }
            }

            SerialPort = new SerialPortHelper();
            SerialPort.IsLogger = true;
            SerialPort.InitCOM(ComportName, baudRate);
            SerialPort.ReceiveDelayMs = 2;
            SerialPort.OpenPort();
        }

        public void Serial_Close()
        {
            if (SerialPort != null)
            {
                SerialPort.ClosePort();
            }
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            ofd.Filter = "所有文件|*.*|JPG文件|*.jpg|PNG文件|*.png|ICO文件|*.ico";
            ofd.RestoreDirectory = false;
            ofd.FilterIndex = 0;

            if (ofd.ShowDialog() == true)
            {
                tbxFilePath.Text = ofd.SafeFileName;

                // 显示图片
                try
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(ofd.FileName);
                    bmp.EndInit();

                    imgFile.Source = bmp;
                   
                }
                catch (Exception)
                {
                    imgFile.Source = null;
                }

                // 获取文件的字节流
                bFile = new BinaryFile();
                bFile.SafeFileName = ofd.SafeFileName;

                bFile.Open(ofd.FileName);
                double fileSize = (double)bFile.FileSize / 1024.0f;
                tbxFileInfo.Text = "文件大小=" + bFile.FileSize.ToString() + "B=" + fileSize.ToString("F2") + "KB；";
            }
        }

        public int RxBuf_Ping(byte[] RxBuf)
        {
            tbxStatus.Text += "Ping OK";

            return 0;
        }

        public int RxBuf_SyncStatus(byte[] RxBuf)
        {
            tbxStatus.Text += "";

            if (RxBuf.Length < 7)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -1;
            }

            int ios = 5;

            byte protocol = RxBuf[ios];
            if (protocol != 1)
            {
                tbxStatus.Text += "协议版本错误；";
                return -2;
            }
            ios += 1;

            Int16 result = CommArithmetic.ByteBuf_to_Int8(RxBuf, ios);
            if (result < 0)
            {
                tbxStatus.Text += "反馈结果=失败=" + result.ToString() + "；";
                return -3;
            }
            ios += 1;

            if (RxBuf.Length < 35)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -4;
            }

            // Ntp OK
            byte ntpOk = RxBuf[ios];
            if (ntpOk == 0)
            {
                tbxStatus.Text += "未授时；";
            }
            else
            {
                tbxStatus.Text += "已授时；";
            }
            ios += 1;

            // 当前时间
            UInt32 u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            DateTime ct = MyCustomFxn.UTC_to_DateTime(u32);
            tbxStatus.Text += ct.ToString("yyyy-MM-dd HH:mm:ss") + "；";

            ios += 4;

            // 组网状态
            byte adhocOk = RxBuf[ios];
            if (adhocOk == 0)
            {
                tbxStatus.Text += "未组网；";
            }
            else
            {
                tbxStatus.Text += "已组网；";
            }
            ios += 1;

            // Hop
            tbxStatus.Text += "HOP=" + RxBuf[ios].ToString() + "；";
            ios += 1;

            // 路径数量
            tbxStatus.Text += "路径数量=" + RxBuf[ios].ToString() + "；";
            ios += 1;

            // 文件总量
            u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            tbxStatus.Text += "文件总量=" + u32.ToString() + "；";
            ios += 4;

            // 已发送文件的数量
            u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            tbxStatus.Text += "已传文件的数量=" + u32.ToString() + "；";
            ios += 4;

            // 待传文件的总数量
            u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            tbxStatus.Text += "待传文件的数量=" + u32.ToString() + "；";
            ios += 4;

            // 待传文件的总大小
            u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            tbxStatus.Text += "待传文件的总大小=" + u32.ToString() + "；";
            ios += 4;

            // 任务数量
            tbxStatus.Text += "任务数量=" + RxBuf[ios].ToString() + "；";
            ios += 1;

            // 状态
            u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            tbxStatus.Text += "当前状态=" + u32.ToString("X8") + "；";
            ios += 4;

            return 0;
        }

        public int RxBuf_ClaimTask(byte[] RxBuf)
        {
            tbxStatus.Text += "";

            if (RxBuf.Length < 7)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -1;
            }

            int ios = 5;

            byte protocol = RxBuf[ios];
            if (protocol != 1)
            {
                tbxStatus.Text += "协议版本错误；";
                return -2;
            }
            ios += 1;

            Int16 result = CommArithmetic.ByteBuf_to_Int8(RxBuf, ios);
            if (result < 0)
            {
                tbxStatus.Text += "反馈结果=失败=" + result.ToString() + "；";
                return -3;
            }
            ios += 1;

            if (RxBuf.Length < 12)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -4;
            }

            // OP
            byte op = RxBuf[ios];
            if (op == 0)
            {
                tbxStatus.Text += "无操作；";
            }
            else if (op == 1)
            {
                tbxStatus.Text += "点播缩略图；";
            }
            else if (op == 2)
            {
                tbxStatus.Text += "点播原图；";
            }
            else
            {
                tbxStatus.Text += "未知操作；";
            }
            ios += 1;

            // 起始地址
            UInt32 u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            UInt32 sAddrInFile = u32;
            tbxStatus.Text += "起始地址=" + u32.ToString("X8") + "；";
            ios += 4;

            // 结束地址
            u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            UInt32 eAddrInFile = u32;
            tbxStatus.Text += "结束地址=" + u32.ToString("X8") + "；";
            ios += 4;

            tbxStatus.Text += "Size=" + (eAddrInFile - sAddrInFile).ToString() + "；";

            // 负载长度
            byte payLen = RxBuf[ios];
            ios += 1;
            if (payLen == 0)
            {
                tbxStatus.Text += "无负载；";
            }
            else
            {
                if(payLen > RxBuf.Length - ios)
                {
                    tbxStatus.Text += "负载数据不完整；";
                    return -5;
                }

                int PayBufEnd = ios + payLen;

                UInt16 dataType = 0;
                byte dataLen = 0;

                while (ios < PayBufEnd)
                {
                    if(PayBufEnd - ios < 3)
                    {
                        tbxStatus.Text += "负载数据有缺失；";
                        return -6;
                    }

                    dataType = CommArithmetic.ByteBuf_to_UInt16(RxBuf, ios);
                    dataLen = RxBuf[ios + 2];

                    ios += 3;

                    switch (dataType)
                    {
                        case 0x100D:
                            {
                                if(dataLen > 64)
                                {
                                    tbxStatus.Text += "数据长度错误；";
                                    return -6;
                                }

                                if (dataLen > PayBufEnd - ios)
                                {
                                    tbxStatus.Text += "文件名不完整；";
                                    return -7;
                                }

                                tbxStatus.Text += "文件名=" + Encoding.Default.GetString(RxBuf, ios, dataLen);

                                ios += dataLen;
                                break;
                            }
                        default:
                            {
                                if (dataLen > PayBufEnd - ios)
                                {
                                    tbxStatus.Text += "文件名不完整；";
                                    return -8;
                                }

                                tbxStatus.Text += "未知的数据类型；";

                                ios += dataLen;
                                break;
                            }
                    }                    
                }                
            }


            return 0;
        }

        public int RxBuf_Apply(byte[] RxBuf)
        {
            tbxStatus.Text += "";

            if (RxBuf.Length < 7)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -1;
            }

            int ios = 5;

            byte protocol = RxBuf[ios];
            if (protocol != 1)
            {
                tbxStatus.Text += "协议版本错误；";
                return -2;
            }
            ios += 1;

            Int16 result = CommArithmetic.ByteBuf_to_Int8(RxBuf, ios);
            if (result < 0)
            {
                tbxStatus.Text += "反馈结果=失败=" + result.ToString() + "；";
                return -3;
            }
            ios += 1;

            if (RxBuf.Length < 19)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -4;
            }

            // 传输码
            PassCode = CommArithmetic.ByteBuf_to_UInt16(RxBuf, ios);
            tbxStatus.Text += "传输码=" + PassCode.ToString("X4") + "；";
            tbxFileInfo.Text += "传输码=" + PassCode.ToString("X4") + "；";
            ios += 2;

            // 数据包的最大长度
            UInt16 rxPktMaxLen = CommArithmetic.ByteBuf_to_UInt16(RxBuf, ios);
            UInt16 aPktMaxLen = 0;

            if(tbxPktMaxLen.Text != null && tbxPktMaxLen.Text != string.Empty)
            {
                try
                {
                    aPktMaxLen = Convert.ToUInt16(tbxPktMaxLen.Text);
                }
                catch (Exception)
                {
                    aPktMaxLen = 255;
                }
            }

            PktMaxLen = rxPktMaxLen < aPktMaxLen ? rxPktMaxLen : aPktMaxLen;

            tbxStatus.Text += "数据包大小=" + PktMaxLen.ToString() + "；";
            tbxStatus.Text += "设置支持的最大数据包大小=" + rxPktMaxLen.ToString() + "；";
            tbxPktMaxLen.Text = PktMaxLen.ToString();

            ios += 2;

            // 文件大小的最大值
            UInt32 u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            tbxStatus.Text += "文件大小不得超过" + u32.ToString() + "；";
            ios += 4;

            // 文件地址
            u32 = CommArithmetic.ByteBuf_to_UInt32(RxBuf, ios);
            tbxStatus.Text += "文件存入地址=" + u32.ToString() + "；";
            ios += 4;

            return 0;
        }

        public int RxBuf_Transfer(byte[] RxBuf)
        {
            tbxStatus.Text += "";

            if (RxBuf.Length < 8)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -1;
            }

            int ios = 5;

            byte protocol = RxBuf[ios];
            if (protocol != 1)
            {
                tbxStatus.Text += "协议版本错误；";
                return -2;
            }
            ios += 1;

            Int16 result = CommArithmetic.ByteBuf_to_Int8(RxBuf, ios);
            if (result < 0)
            {
                tbxStatus.Text += "反馈结果=失败=" + result.ToString() + "；";
                return -3;
            }
            ios += 1;

            // 传输序列号
            byte expTransSerial = (byte)(TransSerial & 0x000000FF);
            if (RxBuf[ios] != expTransSerial)
            {
                tbxStatus.Text += "反馈结果：传输序列号错误；";
                return -4;
            }
            ios += 1;

            TransSuc = true;
            tbxStatus.Text += TransSerial.ToString("D3") + "帧成功；";

            return 0;
        }

        public int RxBuf_Check(byte[] RxBuf)
        {
            tbxStatus.Text += "";

            if (RxBuf.Length < 7)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -1;
            }

            int ios = 5;

            byte protocol = RxBuf[ios];
            if (protocol != 1)
            {
                tbxStatus.Text += "协议版本错误；";
                return -2;
            }
            ios += 1;

            Int16 result = CommArithmetic.ByteBuf_to_Int8(RxBuf, ios);
            if (result < 0)
            {
                tbxStatus.Text += "反馈结果=失败=" + result.ToString() + "；";
                return -3;
            }
            ios += 1;

            tbxStatus.Text += "校验成功；";

            return 0;
        }

        public int RxBuf_Cancel(byte[] RxBuf)
        {
            tbxStatus.Text += "";

            if (RxBuf.Length < 7)
            {
                tbxStatus.Text += "反馈数据包不完整；";
                return -1;
            }

            int ios = 5;

            byte protocol = RxBuf[ios];
            if (protocol != 1)
            {
                tbxStatus.Text += "协议版本错误；";
                return -2;
            }
            ios += 1;

            Int16 result = CommArithmetic.ByteBuf_to_Int8(RxBuf, ios);
            if (result < 0)
            {
                tbxStatus.Text += "反馈结果=失败=" + result.ToString() + "；";
                return -3;
            }
            ios += 1;

            tbxStatus.Text += "取消成功；";

            return 0;
        }

        public int RxBuf_IsRight(byte[] RxBuf, byte[] TxBuf)
        {
            if (RxBuf.Length == 1 && RxBuf[0] == 0x5A)
            {
                tbxStatus.Text += "唤醒成功；";
                return 1;
            }

            if (RxBuf.Length < 5)
            {
                return -1;
            }

            UInt16 Len = CommArithmetic.ByteBuf_to_UInt16(RxBuf, 0);

            if (Len != RxBuf.Length - 5)
            {
                return -2;
            }

            if (RxBuf[2] != 0xDF)
            {
                return -3;
            }

            byte sum = MyCustomFxn.CheckSum8(0, RxBuf, 4, (UInt16)(Len + 1));
            if (sum != RxBuf[3])
            {
                return -4;
            }

            if (RxBuf[4] != TxBuf[4])
            {
                return -5;
            }

            int error = 0;

            switch (RxBuf[4])
            {
                case 0x00:          // Ping
                    {
                        error = RxBuf_Ping(RxBuf);
                        break;
                    }
                case 0x05:          // 同步状态
                    {
                        error = RxBuf_SyncStatus(RxBuf);
                        break;
                    }
                case 0x06:          // 领取任务
                    {
                        error = RxBuf_ClaimTask(RxBuf);
                        break;
                    }
                case 0x0A:          // 申请传输
                    {
                        error = RxBuf_Apply(RxBuf);
                        break;
                    }
                case 0x0B:          // 传输
                    {
                        error = RxBuf_Transfer(RxBuf);
                        break;
                    }
                case 0x0C:          // 校验
                    {
                        error = RxBuf_Check(RxBuf);
                        break;
                    }
                case 0x0D:          // 取消
                    {
                        error = RxBuf_Cancel(RxBuf);
                        break;
                    }
                default:
                    {
                        error = -6;
                        break;
                    }
            }

            if (error < 0)
            {
                error = error - 40;
            }

            return error;
        }

        public int SendWake()
        {
            byte[] TxBuf = new byte[1];
            TxBuf[0] = 0x00;

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 300);

            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            if (RxBuf.Length != 1)
            {
                return -2;
            }

            if (RxBuf[0] == 0x5A)
            {
                tbxStatus.Text += "唤醒成功；";
            }
            else if (RxBuf[0] == 0x00)
            {
                tbxStatus.Text += "已被唤醒；";
            }
            else
            {
                return -3;
            }

            return 0;
        }

        private void btnWake_Click(object sender, RoutedEventArgs e)
        {
            Serial_Init();

            SendWake();

            Serial_Close();
            MoveToEnd();
        }

        public int SendPing()
        {
            byte[] TxBuf = new byte[5];
            TxBuf[0] = 0x00;
            TxBuf[1] = 0x00;
            TxBuf[2] = 0xFD;
            TxBuf[3] = 0x00;
            TxBuf[4] = 0x00;

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 300);

            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            Serial_Init();

            SendPing();

            Serial_Close();
            MoveToEnd();
        }

        public int SyncStatus()
        {
            byte[] TxBuf = new byte[32];
            UInt16 TxLen = 0;

            // 长度
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 方向
            TxBuf[TxLen++] = 0xFD;

            // 校验和
            TxBuf[TxLen++] = 0x00;

            // 命令
            TxBuf[TxLen++] = 0x05;

            // 协议版本
            TxBuf[TxLen++] = 0x01;

            // 当前时间/UTC时间
            UInt32 u32 = MyCustomFxn.DateTime_to_UTC(System.DateTime.Now);
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 电量
            UInt16 u16 = 5987;
            TxBuf[TxLen++] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((u16 & 0x00FF) >> 0);

            // 待传文件的总数量
            u32 = 365;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 待传文件的总大小
            u32 = 108000;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // GPS有效？
            TxBuf[TxLen++] = 0x01;

            // 经度 
            u32 = 120482747;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 纬度
            u32 = 36147429;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 海拔高度
            u16 = 8048;
            TxBuf[TxLen++] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((u16 & 0x00FF) >> 0);

            // 计算校验和
            TxBuf[3] = MyCustomFxn.CheckSum8(0, TxBuf, 4, (UInt16)(TxLen - 5 + 1));

            // 重写长度位
            u16 = (UInt16)(TxLen - 5);
            TxBuf[0] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[1] = (byte)((u16 & 0x00FF) >> 0);

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, TxLen, 400);

            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnSyncStatus_Click(object sender, RoutedEventArgs e)
        {
            Serial_Init();

            SyncStatus();

            Serial_Close();
            MoveToEnd();
        }

        public int ClaimTask()
        {
            byte[] TxBuf = new byte[8];
            UInt16 TxLen = 0;

            // 长度
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 方向
            TxBuf[TxLen++] = 0xFD;

            // 校验和
            TxBuf[TxLen++] = 0x00;

            // 命令
            TxBuf[TxLen++] = 0x06;

            // 协议版本
            TxBuf[TxLen++] = 0x01;

            // 计算校验和
            TxBuf[3] = MyCustomFxn.CheckSum8(0, TxBuf, 4, (UInt16)(TxLen - 5 + 1));

            // 重写长度位
            UInt16 u16 = (UInt16)(TxLen - 5);
            TxBuf[0] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[1] = (byte)((u16 & 0x00FF) >> 0);

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, TxLen, 400);

            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnClaimTask_Click(object sender, RoutedEventArgs e)
        {
            Serial_Init();

            ClaimTask();

            Serial_Close();
            MoveToEnd();
        }

        public int SendApply(UInt32 partSize, byte partTotal, byte partNo, UInt32 shotTime, UInt32 fileCode, UInt32 partAddrInFile)
        {
            if (bFile.SafeFileName == string.Empty)
            {
                return -1;
            }

            byte[] NameBuf = Encoding.ASCII.GetBytes(bFile.SafeFileName);
            if (NameBuf == null || NameBuf.Length == 0)
            {
                return -2;
            }

            if (NameBuf.Length > 64)
            {
                return -3;
            }

            if (partTotal == 0 || partNo >= partTotal)
            {
                return -4;
            }

            byte[] TxBuf = new byte[132];
            UInt16 TxLen = 0;

            // 长度
            TxBuf[TxLen++] = 0;
            TxBuf[TxLen++] = 0;

            // 方向
            TxBuf[TxLen++] = 0xFD;

            // 校验和
            TxBuf[TxLen++] = 0x00;

            // 命令
            TxBuf[TxLen++] = 0x0A;

            // 协议版本
            TxBuf[TxLen++] = 0x01;

            // 文件大小
            UInt32 u32 = partSize;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 分片总数
            TxBuf[TxLen++] = partTotal;

            // 分片编号
            TxBuf[TxLen++] = partNo;

            // 协议版本
            TxBuf[TxLen++] = 0x01;

            // 设备类型
            TxBuf[TxLen++] = (byte)'C';
            TxBuf[TxLen++] = (byte)'A';
            TxBuf[TxLen++] = (byte)'N';
            TxBuf[TxLen++] = (byte)'G';
            TxBuf[TxLen++] = (byte)'L';
            TxBuf[TxLen++] = (byte)'U';
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 保护区编码
            TxBuf[TxLen++] = (byte)'W';
            TxBuf[TxLen++] = (byte)'i';
            TxBuf[TxLen++] = (byte)'l';
            TxBuf[TxLen++] = (byte)'d';
            TxBuf[TxLen++] = (byte)'P';
            TxBuf[TxLen++] = (byte)'r';
            TxBuf[TxLen++] = (byte)'3';
            TxBuf[TxLen++] = (byte)'9';

            // 设备编码
            TxBuf[TxLen++] = (byte)'B';
            TxBuf[TxLen++] = (byte)'M';
            TxBuf[TxLen++] = (byte)'0';
            TxBuf[TxLen++] = (byte)'5';
            TxBuf[TxLen++] = (byte)'0';
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 触发方式
            TxBuf[TxLen++] = (byte)'T';

            // 拍摄时间
            if(shotTime == 0)
            {
                u32 = MyCustomFxn.DateTime_to_UTC(System.DateTime.Now);
            }
            else
            {
                u32 = shotTime;
            }
            
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 温度
            TxBuf[TxLen++] = 0x0A;
            TxBuf[TxLen++] = 0x13;

            // 经度 
            u32 = 120482747;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 纬度
            u32 = 36147429;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 海拔高度
            UInt16 u16 = 8048;
            TxBuf[TxLen++] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((u16 & 0x00FF) >> 0);

            // 电量
            u16 = 5987;
            TxBuf[TxLen++] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((u16 & 0x00FF) >> 0);

            // 文件标志码
            if (fileCode == 0)
            {
                u32 = MyCustomFxn.DateTime_to_UTC(System.DateTime.Now);
            }
            else
            {
                u32 = fileCode;
            }
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 文件总大小
            u32 = (UInt32)bFile.FileSize;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 起始地址
            u32 = partAddrInFile;
            TxBuf[TxLen++] = (byte)((u32 & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((u32 & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((u32 & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((u32 & 0x000000FF) >> 0);

            // 文件名称
            byte NameMaxLen = 64;

            for (int iX = 0; iX < NameMaxLen; iX++)
            {
                if (iX < NameBuf.Length)
                {
                    TxBuf[TxLen++] = NameBuf[iX];
                }
                else
                {
                    TxBuf[TxLen++] = 0x00;
                }
            }

            // 计算校验和
            TxBuf[3] = MyCustomFxn.CheckSum8(0, TxBuf, 4, (UInt16)(TxLen - 5 + 1));

            // 重写长度位
            u16 = (UInt16)(TxLen - 5);
            TxBuf[0] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[1] = (byte)((u16 & 0x00FF) >> 0);

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, TxLen, 1000);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (bFile == null)
            {
                tbxStatus.Text += "未选择文件；";
                return;
            }

            UpdatePartMaxLen();

            if(bFile.FileSize > PartMaxLen)
            {
                tbxStatus.Text += "文件太大，需要分片传输；";
                return;
            }

            Serial_Init();

            SendApply((UInt32)bFile.FileSize, 1, 0, 0, 0, 0);

            Serial_Close();
            MoveToEnd();
        }

        public int SendFrameOfFile(byte[] Buf, int IndexOfStart, UInt16 Len)
        {
            byte[] TxBuf = new byte[5 + 4 + Len];
            UInt16 TxLen = 0;

            // 长度
            TxBuf[TxLen++] = 0;
            TxBuf[TxLen++] = 0;

            // 方向
            TxBuf[TxLen++] = 0xFD;

            // 校验和
            TxBuf[TxLen++] = 0x00;

            // 命令
            TxBuf[TxLen++] = 0x0B;

            // 协议版本
            TxBuf[TxLen++] = 1;

            // 传输码
            UInt16 u16 = PassCode;
            TxBuf[TxLen++] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((u16 & 0x00FF) >> 0);

            // 序列号
            TxBuf[TxLen++] = (byte)(TransSerial & 0x000000FF);

            // 文件内容
            for (int iX = 0; iX < Len; iX++)
            {
                TxBuf[TxLen++] = Buf[IndexOfStart + iX];
            }

            // 计算校验和
            TxBuf[3] = MyCustomFxn.CheckSum8(0, TxBuf, 4, (UInt16)(TxLen - 5 + 1));

            // 重写长度位
            u16 = (UInt16)(TxLen - 5);
            TxBuf[0] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[1] = (byte)((u16 & 0x00FF) >> 0);

            TransSuc = false;

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, TxLen, 200);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            if (TransSuc == false)
            {
                return -3;
            }

            if (++TransSerial == 0)
            {
                TransSerial++;
            }

            return 0;
        }

        public int TransferFile(byte[] Buf, int partAddrInFile, int partSize)
        {
            if (partAddrInFile > Buf.Length)
            {
                return -1;
            }

            if(partAddrInFile + partSize > Buf.Length)
            {
                return -2;
            }

            if (partAddrInFile == Buf.Length)
            {
                return 1;
            }           

            int totalLength = partSize;                             // 文件的总大小
            int offset = 0;                                         // 已发送成功的大小        

            TransSerial = 0;

            tbxTransInfo.Text = "";

            int error = 0;

            UInt16 TxUnitLen = PktMaxLen;                            // 一帧数据中最多可以有X个字节的有效数据
            UInt16 TxLen = 0;

            while (offset < totalLength)
            {
                if (totalLength - offset >= TxUnitLen)
                {
                    //需要传输的字节数量为一个满编的PacketLength,用于非最后一次传输
                    TxLen = TxUnitLen;
                }
                else
                {
                    //需要传输的字节数量不满一个PacketLength,基本上就是最后一次传输
                    TxLen = (UInt16)(totalLength - offset);
                }

                // 开始传输

                error = SendFrameOfFile(Buf, partAddrInFile + offset, TxLen);
                if (error < 0)
                {
                    error = SendFrameOfFile(Buf, partAddrInFile + offset, TxLen);
                    if (error < 0)
                    {
                        System.Threading.Thread.Sleep(100);
                        error = SendFrameOfFile(Buf, partAddrInFile + offset, TxLen);
                        if (error < 0)
                        {
                            break;
                        }
                    }
                }               

                offset += TxLen;
            }          

            if (offset < totalLength)
            {
                tbxStatus.Text += "文件传输：失败；" + TransSerial.ToString() + "；";
                return -2;
            }

            return 0;
        }

        public int SendTransfer(UInt32 partAddrInFile, UInt32 partSize)
        {
            tbxStatus.Text += "文件传输：开始；";

            DateTime startDatetime = DateTime.Now;
            DateTime endDatetime;

            int error = TransferFile(bFile.Content, (int)partAddrInFile, (int)partSize);

            endDatetime = DateTime.Now;
            TimeSpan ts1 = new TimeSpan(startDatetime.Ticks);
            TimeSpan ts2 = new TimeSpan(endDatetime.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            double msec = ts.TotalSeconds;
            tbxStatus.Text += "文件传输：结束；耗时：" + msec.ToString("0.00") + "；";

            return error;
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bFile == null)
                {
                    tbxStatus.Text += "未选择文件；";
                    return;
                }

                UpdatePartMaxLen();

                if (bFile.FileSize > PartMaxLen)
                {
                    tbxStatus.Text += "文件太大，需要分片传输；";
                    return;
                }

                Serial_Init();

                SendTransfer(0, (UInt32)bFile.FileSize);

                Serial_Close();
                MoveToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                tbxStatus.Text += "文件传输：异常终止；";
            }
        }

        public int SendCheck(UInt32 partAddrInFile, UInt32 partSize)
        {
            byte[] TxBuf = new byte[10];
            UInt16 TxLen = 0;

            // 长度
            TxBuf[TxLen++] = 0;
            TxBuf[TxLen++] = 0;

            // 方向
            TxBuf[TxLen++] = 0xFD;

            // 校验和
            TxBuf[TxLen++] = 0x00;

            // 命令
            TxBuf[TxLen++] = 0x0C;

            // 协议版本
            TxBuf[TxLen++] = 0x01;

            // 传输码
            UInt16 u16 = PassCode;
            TxBuf[TxLen++] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((u16 & 0x00FF) >> 0);

            // CRC16
            u16 = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, bFile.Content, partAddrInFile, (UInt32)partSize);
            TxBuf[TxLen++] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((u16 & 0x00FF) >> 0);

            // 计算校验和
            TxBuf[3] = MyCustomFxn.CheckSum8(0, TxBuf, 4, (UInt16)(TxLen - 5 + 1));

            // 重写长度位
            u16 = (UInt16)(TxLen - 5);
            TxBuf[0] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[1] = (byte)((u16 & 0x00FF) >> 0);

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, TxLen, 5000);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -2;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -3;
            }

            return 0;
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (bFile == null)
            {
                tbxStatus.Text += "未选择文件；";
                return;
            }

            Serial_Init();

            SendCheck(0, (UInt32)bFile.FileSize);

            Serial_Close();
            MoveToEnd();
        }

        public int SendCancel()
        {
            byte[] TxBuf = new byte[6];
            UInt16 TxLen = 0;

            // 长度
            TxBuf[TxLen++] = 0;
            TxBuf[TxLen++] = 0;

            // 方向
            TxBuf[TxLen++] = 0xFD;

            // 校验和
            TxBuf[TxLen++] = 0x00;

            // 命令
            TxBuf[TxLen++] = 0x0D;

            // 协议版本
            TxBuf[TxLen++] = 0x01;

            // 计算校验和
            TxBuf[3] = MyCustomFxn.CheckSum8(0, TxBuf, 4, (UInt16)(TxLen - 5 + 1));

            // 重写长度位
            UInt16 u16 = (UInt16)(TxLen - 5);
            TxBuf[0] = (byte)((u16 & 0xFF00) >> 8);
            TxBuf[1] = (byte)((u16 & 0x00FF) >> 0);

            byte[] RxBuf = SerialPort.SendReceive(TxBuf, 0, TxLen, 400);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnCannel_Click(object sender, RoutedEventArgs e)
        {
            Serial_Init();

            SendCancel();

            Serial_Close();
            MoveToEnd();
        }

        private void btnApplyTransferCheck_Click(object sender, RoutedEventArgs e)
        {
            if (bFile == null)
            {
                tbxStatus.Text += "未选择文件；";
                return;
            }

            int error = 0;
            int Suc = 0;

            DateTime fileStartTransferTime = DateTime.Now;

            Serial_Init();

            UInt32 shotTime = MyCustomFxn.DateTime_to_UTC(System.DateTime.Now);
            UInt32 fileCode = MyCustomFxn.DateTime_to_UTC(System.DateTime.Now);

            UpdatePartMaxLen();

            UInt32 FileSize = (UInt32)bFile.FileSize;
            UInt32 partSize = 0;

            UInt32 SentTotal = 0;

            byte partTotal = 0;
            byte partNo = 0;

            UInt16 reTry = 0;

            if (FileSize > PartMaxLen)
            {
                if ((FileSize % PartMaxLen) == 0)
                {
                    partTotal = (byte)(FileSize / PartMaxLen);
                }
                else
                {
                    partTotal = (byte)((FileSize / PartMaxLen) + 1);
                }
            }
            else
            {
                partTotal = 1;
            }

            for(; SentTotal < FileSize; )
            {
                if(FileSize - SentTotal > PartMaxLen)
                {
                    partSize = PartMaxLen;
                }
                else
                {
                    partSize = FileSize - SentTotal;
                }

                if (partTotal > 1)
                {
                    tbxStatus.Text += "\n文件分片传输" + (partNo + 1).ToString("D3") + "；";
                }

                 error = 0;
                 Suc = 0;

                do
                {
                    error = SendWake();
                    if (error < 0)
                    {
                        Suc = -1;
                        break;
                    }

                    error = SendCancel();
                    if (error < 0)
                    {
                        Suc = -2;
                        break;
                    }

                    error = SendApply(partSize, partTotal, partNo, shotTime, fileCode, SentTotal);
                    if (error < 0)
                    {
                        Suc = -3;
                        break;
                    }

                    error = SendTransfer(SentTotal, partSize);
                    if (error < 0)
                    {
                        Suc = -4;
                        break;
                    }

                    error = SendCheck(SentTotal, partSize);
                    if (error < 0)
                    {
                        error = SendCheck(SentTotal, partSize);
                        if (error < 0)
                        {
                            Suc = -5;
                            break;
                        }                            
                    }

                } while (false);

                if (Suc < 0)
                {
                    if(++reTry < 600)
                    {                      
                        System.Threading.Thread.Sleep(200);

                        tbxStatus.Text += "\n文件分片传输" + (partNo + 1).ToString("D3") + "；重新传输；";
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                reTry = 0;

                SentTotal += partSize;
                partNo++;
            }

            tbxStatus.Text += "\n总计耗时：" + MyCustomFxn.CalcTimeDiff(fileStartTransferTime).ToString("0.00") + "秒；\n";

            Serial_Close();
            MoveToEnd();            
        }

        private void btnClearStatus_Click(object sender, RoutedEventArgs e)
        {
            tbxStatus.Text = "";
        }

        private void MoveToEnd()
        {   // 将光标移动到末尾
            tbxStatus.Focus();                                  // 获取焦点
            tbxStatus.Select(tbxStatus.Text.Length, 0);         // 光标定位到文本最后
            tbxStatus.ScrollToEnd();                            // 滚动到光标处
        }

        private void UpdatePartMaxLen()
        {
            UInt16 aPartMaxLenInKB = 124;                       // 单位：KB

            if (tbxPartMaxLen.Text != null && tbxPartMaxLen.Text != string.Empty)
            {
                try
                {
                    aPartMaxLenInKB = Convert.ToUInt16(tbxPartMaxLen.Text);
                }
                catch (Exception)
                {
                    aPartMaxLenInKB = 124;
                }
            }

            PartMaxLen = (UInt32)(aPartMaxLenInKB * 1024);      // 文件分片的标准
        }
    }
}
