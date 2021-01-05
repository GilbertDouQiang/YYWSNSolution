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

namespace HyperWSN_Test_M44
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
                SerialPort.InitCOM(portname, 9600);
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

            if (Buf == null)
            {
                tbxConsole.Text = "\r\n" + Logger.GetTimeString() + "\t" + direct + "\r\n" + tbxConsole.Text;
            }
            else
            {
                tbxConsole.Text = Logger.GetTimeString() + "\t" + direct + "\t" + MyCustomFxn.ToHexString(Buf, Start, Len) + "\r\n" + tbxConsole.Text;
            }            

            UInt16 ConsoleMaxLine = 24;
            if (tbxConsole.LineCount > ConsoleMaxLine)
            {
                int start = tbxConsole.GetCharacterIndexFromLineIndex(ConsoleMaxLine);  // 末尾行第一个字符的索引
                int length = tbxConsole.GetLineLength(ConsoleMaxLine);                  // 末尾行字符串的长度
                tbxConsole.Select(start, start + length);                               // 选中末尾一行
                tbxConsole.SelectedText = "END";
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

        /// <summary>
        /// 串口发送数据，并在Console显示日志
        /// </summary>
        /// <param name="TxBuf"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="TxLen"></param>
        private void SerialPort_Send(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen)
        {
            if (SerialPort_isReady() < 0)
            {
                return;
            }

            // 在Console显示Log
            ConsoleLog("TX", TxBuf, IndexOfStart, TxLen);

            // 发送数据
            SerialPort.Send(TxBuf, IndexOfStart, TxLen);
        }

        private int RxPkt_isRight_CO2(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            int PktLen = RxBuf.Length - Start;
            if (PktLen < 8)
            {
                return -1;
            }            

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16_Modbus(0xA001, 0xFFFF, RxBuf, (UInt16)(Start), (UInt16)(PktLen - 2));
            UInt16 crc_chk = (UInt16)(RxBuf[Start + 7] * 256 + RxBuf[Start + 6]);
            if (crc_chk != crc)
            {
                return -2;
            }

            // 设备地址
            if (RxBuf[Start + 0] != 0x08)
            {
                return -3;
            }

            // 数据地址
            UInt16 Addr = (UInt16)(RxBuf[Start + 2] * 256 + RxBuf[Start + 3]);
            if (Addr != 4116)
            {
                return -4;
            }

            // 读取数据的个数
            UInt16 Num = (UInt16)(RxBuf[Start + 4] * 256 + RxBuf[Start + 5]);
            if (Num != 8)
            {
                return -5;
            }

            return PktLen;
        }

        private int RxPkt_isRight_AC2(byte[] RxBuf, int Start)
        {
            // 数据包的总长度
            int PktLen = RxBuf.Length - Start;
            if (PktLen < 8)
            {
                return -1;
            }            

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16_Modbus(0xA001, 0xFFFF, RxBuf, (UInt16)(Start), (UInt16)(PktLen - 2));
            UInt16 crc_chk = (UInt16)(RxBuf[Start + 7] * 256 + RxBuf[Start + 6]);
            if (crc_chk != crc)
            {
                return -2;
            }

            // 设备地址
            if (RxBuf[Start + 0] != 0x01)
            {
                return -3;
            }

            // 数据地址
            UInt16 Addr = (UInt16)(RxBuf[Start + 2] * 256 + RxBuf[Start + 3]);
            if (Addr != 0)
            {
                return -4;
            }

            // 读取数据的个数
            UInt16 Num = (UInt16)(RxBuf[Start + 4] * 256 + RxBuf[Start + 5]);
            if (Num != 6)
            {
                return -5;
            }

            return PktLen;
        }

        private int RxPkt_isRight(byte[] RxBuf, int Start)
        {
            if (cbxDeviceOf485.SelectedIndex == 0)
            {
                return RxPkt_isRight_CO2(RxBuf, Start);
            }
            else
            {
                return RxPkt_isRight_AC2(RxBuf, Start);
            }
        }

        /// <summary>
        /// 处理接收到的数据包
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <returns></returns>
        private int RxPkt_Handle(byte[] RxBuf)
        {
            if (RxBuf == null || RxBuf.Length < 8)
            {
                return -1;
            }

            int HandleLen = 0;
            int ExeError = 0;

            for (int iCnt = 0; iCnt < RxBuf.Length; iCnt++)
            {
                try
                {
                    HandleLen = RxPkt_isRight(RxBuf, iCnt);
                    if (HandleLen < 0)
                    {
                        continue;
                    }

                    switch (RxBuf[iCnt + 1])
                    {
                        case 0x03:
                            {
                                ExeError = RxPkt_ReadValue(RxBuf, iCnt);
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

            return 0;
        }

        /// <summary>
        /// 二氧化碳培养箱
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        private int RxPkt_ReadValueOfCO2(byte[] RxBuf, int Start)
        {
            byte[] TxBuf = new byte[24];
            int TxLen = 0;

            // 设备地址
            TxBuf[TxLen++] = RxBuf[0];

            // 功能码
            TxBuf[TxLen++] = RxBuf[1];

            // 数据字节
            TxBuf[TxLen++] = (byte)(RxBuf[5] * 2);

            // T_pv
            DateTime dt = DateTime.UtcNow;
            Random aRandomNum = new Random(dt.Millisecond);
            double T_pv_f = (aRandomNum.NextDouble() - 0.5) * 100.0f;
            Int16 T_pv_i = (Int16)(T_pv_f * 100.0f);
            TxBuf[TxLen++] = (byte)((T_pv_i & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((T_pv_i & 0x00FF) >> 0);

            // CO2_pv
            double CO2_pv_f = aRandomNum.NextDouble() * 100.0f;
            UInt16 CO2_pv_u = (UInt16)(CO2_pv_f * 100.0f);
            TxBuf[TxLen++] = (byte)((CO2_pv_u & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((CO2_pv_u & 0x00FF) >> 0);

            // O2_pv
            double O2_pv_f = aRandomNum.NextDouble() * 100.0f;
            UInt16 O2_pv_u = (UInt16)(O2_pv_f * 100.0f);
            if (cbxO2IsZero.IsChecked == true)
            {
                O2_pv_f = 0.0f;
                O2_pv_u = 0;
            }
            TxBuf[TxLen++] = (byte)((O2_pv_u & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((O2_pv_u & 0x00FF) >> 0);

            // RH_pv
            double RH_pv_f = aRandomNum.NextDouble() * 100.0f;
            UInt16 RH_pv_u = (UInt16)(RH_pv_f * 100.0f);
            TxBuf[TxLen++] = (byte)((RH_pv_u & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((RH_pv_u & 0x00FF) >> 0);

            // T_ofs
            double T_ofs_f = (aRandomNum.NextDouble() - 0.5f) * 10.0f;
            Int16 T_ofs_i = (Int16)(T_ofs_f * 100.0f);
            TxBuf[TxLen++] = (byte)((T_ofs_i & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((T_ofs_i & 0x00FF) >> 0);

            // CO2_ofs
            double CO2_ofs_f = (aRandomNum.NextDouble() - 0.5f) * 10.0f;
            Int16 CO2_ofs_i = (Int16)(CO2_ofs_f * 100.0f);
            TxBuf[TxLen++] = (byte)((CO2_ofs_i & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((CO2_ofs_i & 0x00FF) >> 0);

            // O2_ofs
            double O2_ofs_f = (aRandomNum.NextDouble() - 0.5f) * 10.0f;
            Int16 O2_ofs_i = (Int16)(O2_ofs_f * 100.0f);
            if (cbxO2IsZero.IsChecked == true)
            {
                O2_ofs_f = 0.0f;
                O2_ofs_i = 0;
            }
            TxBuf[TxLen++] = (byte)((O2_ofs_i & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((O2_ofs_i & 0x00FF) >> 0);

            // RH_ofs
            double RH_ofs_f = (aRandomNum.NextDouble() - 0.5f) * 10.0f;
            Int16 RH_ofs_i = (Int16)(RH_ofs_f * 100.0f);
            TxBuf[TxLen++] = (byte)((RH_ofs_i & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((RH_ofs_i & 0x00FF) >> 0);

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16_Modbus(0xA001, 0xFFFF, TxBuf, 0, (UInt16)TxLen);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);

            // 组包完成

            // 为防止反馈过快，特加一个延时
            System.Threading.Thread.Sleep(16);

            SerialPort_Send(TxBuf, 0, (UInt16)TxLen);

            // Console显示
            double Value = T_pv_f + T_ofs_f;
            string txt = "AN\tT_pv=" + Value.ToString("F2");

            Value = CO2_pv_f + CO2_ofs_f;
            Value = Value < 0.0f ? 0.0f : Value;
            Value = Value > 99.99f ? 99.99f : Value;
            txt += ", CO2_pv=" + Value.ToString("F2");

            Value = O2_pv_f + O2_ofs_f;
            Value = Value < 0.0f ? 0.0f : Value;
            Value = Value > 99.99f ? 99.99f : Value;
            txt += ", O2_pv=" + Value.ToString("F2");

            Value = RH_pv_f + RH_ofs_f;
            Value = Value < 0.0f ? 0.0f : Value;
            Value = Value > 99.99f ? 99.99f : Value;
            txt += ", RH_pv=" + Value.ToString("F2") + ";";

            ConsoleLog(txt, null, 0, 0);

            return 0;
        }

        // AC生物安全柜的状态值
        UInt16 inflow_status = 0;       // 0:uncalib; 1:fail; 2:OK;
        UInt16 fan_status = 0;          // 1:on; 0:off;
        UInt16 Sash_status = 0;         // 0:unsafe; 8:Safe; 16:fullyclose; 24:errorpos;

        /// <summary>
        /// AC2生物安全柜
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        private int RxPkt_ReadValueOfAC2(byte[] RxBuf, int Start)
        {
            byte[] TxBuf = new byte[24];
            int TxLen = 0;

            // 设备地址
            TxBuf[TxLen++] = RxBuf[0];

            // 功能码
            TxBuf[TxLen++] = RxBuf[1];

            // 数据字节
            TxBuf[TxLen++] = (byte)(RxBuf[5] * 2);

            // Inflow
            DateTime dt = DateTime.UtcNow;
            Random aRandomNum = new Random(dt.Millisecond);
            double Inflow_f = aRandomNum.NextDouble();
            UInt16 Inflow_u = (UInt16)(Inflow_f * 100.0f);
            TxBuf[TxLen++] = (byte)((Inflow_u & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Inflow_u & 0x00FF) >> 0);

            // downflow
            double downflow_f = aRandomNum.NextDouble();
            UInt16 downflow_u = (UInt16)(downflow_f * 100.0f);
            TxBuf[TxLen++] = (byte)((downflow_u & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((downflow_u & 0x00FF) >> 0);

            // temperature
            double temperature_f = (aRandomNum.NextDouble() - 0.5) * 100.0f;
            Int16 temperature_i = (Int16)(temperature_f * 100.0f);
            TxBuf[TxLen++] = (byte)((temperature_i & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((temperature_i & 0x00FF) >> 0);

            // inflow status
            TxBuf[TxLen++] = (byte)((inflow_status & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((inflow_status & 0x00FF) >> 0);

            // fan status
            TxBuf[TxLen++] = (byte)((fan_status & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((fan_status & 0x00FF) >> 0);

            // Sash status
            TxBuf[TxLen++] = (byte)((Sash_status & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((Sash_status & 0x00FF) >> 0);

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16_Modbus(0xA001, 0xFFFF, TxBuf, 0, (UInt16)TxLen);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);

            // 组包完成

            // 为防止反馈过快，特加一个延时
            System.Threading.Thread.Sleep(16);

            SerialPort_Send(TxBuf, 0, (UInt16)TxLen);

            // Console显示
            string txt = "AN\tInflow=" + Inflow_f.ToString("F2");

            txt += ", downflow=" + downflow_f.ToString("F2");

            txt += ", temperature=" + temperature_f.ToString("F2");

            switch (inflow_status)
            {
                case 0:
                    {
                        txt += ", inflow status=uncalib";
                        break;
                    }
                case 1:
                    {
                        txt += ", inflow status=fail";
                        break;
                    }
                case 2:
                    {
                        txt += ", inflow status=OK";
                        break;
                    }
                default:
                    {
                        txt += ", inflow status=unknown";
                        break;
                    }
            }

            switch (fan_status)
            {
                case 0:
                    {
                        txt += ", fan status=off";
                        break;
                    }
                case 1:
                    {
                        txt += ", fan status=on";
                        break;
                    }
                default:
                    {
                        txt += ", fan status=unknown";
                        break;
                    }
            }

            switch (Sash_status)
            {
                case 0:
                    {
                        txt += ", Sash status=unsafe;";
                        break;
                    }
                case 8:
                    {
                        txt += ", Sash status=Safe;";
                        break;
                    }
                case 16:
                    {
                        txt += ", Sash status=fullyclose;";
                        break;
                    }
                case 24:
                    {
                        txt += ", Sash status=errorpos;";
                        break;
                    }
                default:
                    {
                        txt += ", Sash status=unknown;";
                        break;
                    }
            }

            ConsoleLog(txt, null, 0, 0);

            if (++inflow_status > 2)
            {
                inflow_status = 0;
            }

            if (++fan_status > 1)
            {
                fan_status = 0;
            }

            Sash_status += 8;
            if (Sash_status > 24)
            {
                Sash_status = 0;
            }

            return 0;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="RxBuf"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        private int RxPkt_ReadValue(byte[] RxBuf, int Start)
        {
            // 延时
            DateTime dt = DateTime.UtcNow;
            Random aRandomNum = new Random(dt.Millisecond);

            int delayMin = 0;
            int delayMax = 100;

            if (tbxDelayMin.Text != null && tbxDelayMin.Text != string.Empty)
            {
                try
                {
                    delayMin = Convert.ToInt32(tbxDelayMin.Text);
                }
                catch
                {
                    MessageBox.Show("最小延时错误！");
                    return -1;
                }
            }

            if (tbxDelayMax.Text != null && tbxDelayMax.Text != string.Empty)
            {
                try
                {
                    delayMax = Convert.ToInt32(tbxDelayMax.Text);
                }
                catch
                {
                    MessageBox.Show("最大延时错误！");
                    return -2;
                }
            }

            int delayMs = aRandomNum.Next(delayMin, delayMax);

            System.Threading.Thread.Sleep(delayMs);


            if (cbxDeviceOf485.SelectedIndex == 0)
            {
                return RxPkt_ReadValueOfCO2(RxBuf, Start);
            }
            else
            {
                return RxPkt_ReadValueOfAC2(RxBuf, Start);
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            byte[] RxBuf = new byte[8];

            if (cbxDeviceOf485.SelectedIndex == 0)
            {
                RxBuf[0] = 0x08;
                RxBuf[1] = 0x03;
                RxBuf[2] = 0x10;
                RxBuf[3] = 0x14;
                RxBuf[4] = 0x00;
                RxBuf[5] = 0x08;
                RxBuf[6] = 0x00;
                RxBuf[7] = 0x61;
            }
            else
            {
                RxBuf[0] = 0x01;
                RxBuf[1] = 0x03;
                RxBuf[2] = 0x00;
                RxBuf[3] = 0x00;
                RxBuf[4] = 0x00;
                RxBuf[5] = 0x06;
                RxBuf[6] = 0xC5;
                RxBuf[7] = 0xC8;
            }

            RxPkt_ReadValue(RxBuf, 0);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            tbxConsole.Text = "";
        }

        private void cbxDeviceOf485_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxO2IsZero == null)
            {
                return;
            }

            if (cbxDeviceOf485.SelectedIndex == 0)
            {
                cbxO2IsZero.Visibility = Visibility.Visible;
            }
            else
            {
                cbxO2IsZero.Visibility = Visibility.Hidden;
            }
        }
    }
}
