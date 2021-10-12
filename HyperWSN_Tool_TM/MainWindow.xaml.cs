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
    public struct Calendar
    {
        public byte year;      /* years since 2000           - [0,99]  */
        public byte month;     /* months since January       - [1,12]  */
        public byte mday;      /* day of the month           - [1,31]  */
        public byte hour;      /* hours after the midnight   - [0,23]  */
        public byte minute;    /* minutes after the hour     - [0,59]  */
        public byte second;    /* seconds after the minute   - [0,59]  */
    }


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

        bool leapyear(UInt32 y)
        {
            if(((y) % 4 == 0 && ((y) % 100 != 0 || (y) % 400 == 0)))
            {   // 闰年
                return true;
            }
            else
            {
                return false;
            }
        }

        UInt32 Calendar_To_UTC_2(Calendar ct)
        {
            UInt16[] mon_day = new UInt16[]{ 0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
                304, 334, 365 };

            const UInt32 days_in_year = 365;      // 一个平年有多少天
            const UInt32 secs_in_day = 86400;     // 一天有多少秒

            const UInt32 epoch_base_secs = 946656000; // 1970/01/01 00:00:00 ~ 2000/01/01 00:00:00

            UInt32 result;
            UInt32 mdays = 0;                     // 天的数量
            UInt32 secs = 0;
            byte iX;

            if (ct.second >= 60 || ct.minute >= 60 || ct.hour >= 24)
            {
                return 0xFFFFFFFF;
            }

            if (ct.mday == 0 || ct.mday > 31 || ct.month == 0 || ct.month > 12
                    || ct.year > 99)
            {
                return 0xFFFFFFFF;
            }

            // 计算从2000年到年初，总共有多少天
            for (iX = 0; iX < ct.year; iX++)
            {
                if (leapyear((UInt32)(iX + 2000)))
                {   // 闰年
                    mdays += days_in_year + 1;
                }
                else
                {   // 平年
                    mdays += days_in_year;
                }
            }   // for

            // 计算从年初到这个月初，总共有多少天
            mdays += mon_day[ct.month - 1];
            if (ct.month > 2 && leapyear((UInt32)(ct.year + 2000)))
            {   // 闰月
                mdays += 1;
            }

            // 计算从年初到今天凌晨，总共有多少天
            mdays += (UInt32)(ct.mday - 1);

            // 计算从今天凌晨到现在，总共有多少秒

            if(ct.hour != 0)
            {
                secs += (UInt32)(3600 * ct.hour);
            }

            if (ct.minute != 0)
            {
                secs += (UInt32)(60 * ct.minute);
            }

            secs += ct.second;

            // 计算最终的UTC时间
            result = epoch_base_secs + mdays * secs_in_day + secs;

            return result;
        }

        Calendar UTC_To_Calendar_2(UInt32 utc)
        {
             UInt16[] mon_day = new UInt16[]{ 0, 31, 59, 90, 120, 151, 181, 212, 243, 273,
                304, 334, 365 };

            const UInt32 days_in_year = 365;      // 一个平年有多少天
            const UInt32 secs_in_day = 86400;     // 一天有多少秒
            const UInt32 secs_in_hour = 3600;     // 一小时有多少秒
            const UInt32 secs_in_minute = 60;     // 一分钟有多少秒

            const UInt32 epoch_base_secs = 946656000; // 1970/01/01 00:00:00 ~ 2000/01/01 00:00:00
            const UInt32 epoch_end_secs = 4102415999; // 2099/12/31 23:59:59

            UInt32 mdays = 0;
            UInt32 secs = 0;
            byte iX;

            Calendar ct = new Calendar();

            ct.year = 0;
            ct.month = 1;
            ct.mday = 1;
            ct.hour = 0;
            ct.minute = 0;
            ct.second = 0;

            if (utc <= epoch_base_secs)
            {
                return ct;
            }

            if (utc > epoch_end_secs)
            {
                utc = epoch_end_secs;
            }

            // 计算21世纪的偏移量
            utc -= epoch_base_secs;

            // 计算时分秒
            secs = utc % secs_in_day;
            utc -= secs;

            ct.hour = (byte)(secs / secs_in_hour);
            secs = secs % secs_in_hour;

            ct.minute = (byte)(secs / secs_in_minute);
            secs = secs % secs_in_minute;

            ct.second = (byte)secs;

            // 计算总天数
            mdays = utc / secs_in_day;

            // 计算年份
            UInt32 pdays = 0;
            UInt32 tdays = 0;
            for (iX = 0; ; iX++)
            {
                pdays = tdays;

                if (leapyear((UInt32)(iX + 2000)))
                {   // 闰年
                    tdays += days_in_year + 1;
                }
                else
                {   // 平年
                    tdays += days_in_year;
                }

                if (tdays < mdays)
                {
                    continue;
                }
                else if (tdays == mdays)
                {
                    ct.year = (byte)(iX + 1);
                    mdays -= tdays;
                    break;
                }
                else
                {
                    ct.year = iX;
                    mdays -= pdays;
                    break;
                }

            }   // for

            // 计算月份
            pdays = 0;
            tdays = 0;                  // 从年初到月初，总共有多少天
            for (iX = 1; ; iX++)
            {
                pdays = tdays;

                tdays = mon_day[iX - 1];
                if (iX > 2 && leapyear((UInt32)(ct.year + 2000)))
                {   // 闰月
                    tdays += 1;
                }

                if (tdays < mdays)
                {
                    continue;
                }
                else if (tdays == mdays)
                {
                    ct.month = iX;
                    mdays -= tdays;
                    break;
                }
                else
                {
                    ct.month = (byte)(iX - 1);
                    mdays -= pdays;
                    break;
                }

            }   // for

            // 计算日数
            ct.mday = (byte)(mdays + 1);

            return ct;
        }

        bool breakpoint = false;

        /// <summary>
        /// 算法验证
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            UInt32 ut;
            Calendar ct = new Calendar();
            UInt32 ut_res;

            DateTime dt;

            string myRes = string.Empty;
            string expRes = string.Empty;

            while (true)
            {
                for (ut = 946656000; ut < 0xFFFFFFFF;)
                {
                    breakpoint = false;

                    ct = UTC_To_Calendar_2(ut);

                    ut_res = Calendar_To_UTC_2(ct);

                    dt = MyCustomFxn.UTC_to_DateTime(ut);

                    myRes = "20" + ct.year.ToString("D2") + "/" + ct.month.ToString("D2") + "/" + ct.mday.ToString("D2") + " " + ct.hour.ToString("D2") + ":" + ct.minute.ToString("D2") + ":" + ct.second.ToString("D2");
                    expRes = dt.Year.ToString("D4") + "/" + dt.Month.ToString("D2") + "/" + dt.Day.ToString("D2") + " " + dt.Hour.ToString("D2") + ":" + dt.Minute.ToString("D2") + ":" + dt.Second.ToString("D2");

                    if (ut != ut_res || myRes != expRes)
                    {
                        breakpoint = true;
                    }
                    else
                    {
                        ut++;
                    }

                }


                breakpoint = true;
            }

        }   //



    }
}
