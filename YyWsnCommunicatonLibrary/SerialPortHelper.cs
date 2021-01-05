using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using YyWsnDeviceLibrary;
using System.Timers;
using System.Threading;
using System.Management;

namespace YyWsnCommunicatonLibrary
{
    public class SerialPortHelper
    {
        /// <summary>
        /// 正在接收
        /// </summary>
        private bool Receiving = false;

        /// <summary>
        /// 是否自动生成日志文件
        /// </summary>
        public bool IsLogger { get; set; }        

        /// <summary>
        /// 抓到异常时，产生的提示语
        /// </summary>
        public string ExceptString { get; set; }

        public delegate void SerialPortReceivedEventHandler(object sender, SerialPortEventArgs e);
        public event SerialPortReceivedEventHandler SerialPortReceived;

        SerialPort Port1;

        bool EnableRxAfterTimer = false;

        System.Timers.Timer WaitTotalTimer;             // 等待接收数据的超时计时器
        System.Timers.Timer AfterReceivedTimer;         // 收到数据后，开始计时，若是一定时间内没有再收到新的数据，则退出接收；

        bool WaitTotalTimeout = false;                  // 计时器超时标志位；接收数据时，最大等待时间；
        bool AfterReceivedTimeout = false;              // 计时器超时标志位；收到数据后，开始计时，若是一定时间内没有再收到新的数据，则退出接收；

        bool ReceivedData = false;                      // 是否已经收到了数据

        string RxByteStr;                               // 从串口接收到的数据字节数组的字符串形式，可累计
        byte[] RxByteBuf;                               // 从串口接收到的数据字节数组

        string RxExpStr;                                // 接收过程中，希望收到的字符串
        bool ReceivedExpStr = false;                    // 是否已经收到了期望的字符串了

        public int ReceiveDelayMs = 60;                 // 接收的延时时间

        /// <summary>
        /// 获取符合特征值的SerialPort 端口名称
        /// </summary>
        /// <param name="id">特征字符串，比如 CP</param>
        /// <returns></returns>
        public static String[] GetSerialPorts(string id)
        {
            String[] ss = MulGetHardwareInfo(HardwareEnum.Win32_SerialPort, "Name");    //调用方式通过WMI获取COM端口 

            List<string> checkSS = new List<string>();
            foreach (string item in ss)
            {
                if (item.Contains(id))
                {
                    checkSS.Add(item);
                }
            }

            String[] result = new String[checkSS.Count];
            for (int i = 0; i < checkSS.Count; i++)
            {
                result[i] = checkSS[i];
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PortName"> 例如："COM5"  </param>
        /// <param name="BaudRate"></param>
        public void InitCOM(string PortName, int BaudRate, Parity parity)
        {
            //释放以前的端口
            if (Port1 != null)
            {
                Port1.Dispose();
                Thread.Sleep(50);
            }

            //port Name 有2中可能  COMX  和  Silicon Labs CP210x USB to UART Bridge (COM11)
            if (PortName.Substring(0, 3) != "COM")
            {   //获得新的名称
                PortName = GetSerialPortName(PortName);
            }

            Port1 = new SerialPort(PortName);
            Port1.BaudRate = BaudRate;                  // 波特率
            Port1.Parity = parity;                      // 奇偶校验位
            Port1.StopBits = StopBits.One;              // 一个停止位
            Port1.ReadBufferSize = 16384;
            Port1.WriteBufferSize = 16384;
            Port1.DataReceived += Port1_DataReceived;   // DataReceived事件委托

            ExceptString = String.Empty;

            ReceiveDelayMs = 60;

            AfterReceivedTimer = new System.Timers.Timer();
            AfterReceivedTimer.Interval = 100;
            AfterReceivedTimer.Enabled = false;
            AfterReceivedTimer.Elapsed += AfterReceivedTimer_Elapsed;
        }

        public void InitCOM(string PortName, int BaudRate)
        {
            InitCOM(PortName, BaudRate, Parity.None);
        }

        public void InitCOM(string PortName)
        {
            InitCOM(PortName, 115200);
        }

        public bool OpenPort()
        {
            if (Port1 == null)
            {
                return false;
            }

            if (Port1.IsOpen)
            {
                return true;
            }

            try
            {
                Port1.Open();
            }
            catch (Exception ex)
            {
                ExceptString = ex.Message;
                return false;
            }

            if (Port1.IsOpen)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsOpen()
        {
            if (Port1 == null)
            {
                return false;
            }

            return Port1.IsOpen;
        }

        //关闭串口的方法
        public void ClosePort()
        {
            if (Port1 == null)
            {
                return;
            }

            try
            {
                if(Port1.IsOpen)
                {
                    while (Receiving == true)
                    {
                        System.Threading.Thread.Sleep(100);
                    }

                    Port1.Close();
                }               
            }
            catch (Exception ex)
            {
                ExceptString = ex.Message;
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="tip"></param>
        /// <param name="Buf"></param>
        /// <param name="StartIndex"></param>
        /// <param name="Len"></param>
        private void AddLog(string tip, byte[] Buf, int StartIndex, int Len)
        {
            if (IsLogger == true)
            {
                Logger.AddLogAutoTime(tip + "\t" + CommArithmetic.ToHexString(Buf, StartIndex, Len));
            }
        }

        private void AddLog(string tip, byte[] Buf)
        {
            if (Buf == null)
            {
                AddLog(tip, null, 0, 0);
            }
            else
            {
                AddLog(tip, Buf, 0, Buf.Length);
            }
        }

        private int Write(byte[] Buf, int Start, int Len)
        {
            if (Buf == null || Start < 0 || Len < 0)
            {
                return -1;
            }

            if (Buf.Length == 0 || Len == 0)
            {
                return 0;
            }

            if (Port1 == null)
            {
                return -2;
            }

            if (Port1.IsOpen == false)
            {
                return -3;
            }

            Port1.Write(Buf, Start, Len);
            AddLog("TX", Buf, Start, Len);

            return 0;
        }

        public int Send(byte[] TxBuf, int IndexOfStart, int TxLen)
        {
            return Write(TxBuf, IndexOfStart, TxLen);
        }

        public int Send(byte[] TxBuf)
        {
            return Write(TxBuf, 0, TxBuf.Length);
        }

        public int Send(string Str)
        {
            byte[] TxBuf = CommArithmetic.HexStringToByteArray(Str);

            return Write(TxBuf, 0, TxBuf.Length);
        }

        /// <summary>
        /// 发送协议指令并获得反馈，如果超时，返回null；
        /// </summary>
        /// <param name="CommandBytes"></param>
        public byte[] SendReceive(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen, UInt16 RxTimeoutMs)
        {
            if (Port1 == null)
            {
                return null;
            }

            if (Port1.IsOpen == false)
            {
                return null;
            }

            Write(TxBuf, IndexOfStart, TxLen);

            if (RxTimeoutMs != 0)
            {
                RxByteStr = null;
                RxByteBuf = null;

                RxExpStr = null;
                ReceivedExpStr = false;

                ReceivedData = false;
                WaitTotalTimeout = false;                

                WaitTotalTimer = new System.Timers.Timer();
                WaitTotalTimer.Interval = RxTimeoutMs;
                WaitTotalTimer.Enabled = true;
                WaitTotalTimer.Elapsed += WaitTotalTimer_Elapsed;

                while (WaitTotalTimeout == false && ReceivedData == false)
                {
                    System.Threading.Thread.Sleep(1);
                }

                WaitTotalTimer.Enabled = false;
                WaitTotalTimer.Stop();
                WaitTotalTimer.Dispose();

                WaitTotalTimeout = false;
                ReceivedData = false;

                AddLog("RX", RxByteBuf);
            }

            return RxByteBuf;
        }

        public byte[] SendReceive(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen, UInt16 RxTotalMs, UInt16 RxAfterMs)
        {
            if (Port1 == null)
            {
                return null;
            }

            if (Port1.IsOpen == false)
            {
                return null;
            }

            Write(TxBuf, IndexOfStart, TxLen);

            if (RxTotalMs != 0)
            {
                RxByteStr = null;
                RxByteBuf = null;

                RxExpStr = null;
                ReceivedExpStr = false;

                ReceivedData = false;
                WaitTotalTimeout = false;
                AfterReceivedTimeout = false;

                WaitTotalTimer = new System.Timers.Timer();
                WaitTotalTimer.Interval = RxTotalMs;
                WaitTotalTimer.Enabled = true;
                WaitTotalTimer.Elapsed += WaitTotalTimer_Elapsed;

                if (RxAfterMs != 0)
                {
                    EnableRxAfterTimer = true;
                    AfterReceivedTimer.Interval = RxAfterMs;
                    AfterReceivedTimer.Enabled = false;
                }

                while (WaitTotalTimeout == false && AfterReceivedTimeout == false)
                {
                    System.Threading.Thread.Sleep(1);
                }

                WaitTotalTimer.Enabled = false;
                WaitTotalTimer.Stop();
                WaitTotalTimer.Dispose();

                AfterReceivedTimer.Enabled = false;
                EnableRxAfterTimer = false;

                WaitTotalTimeout = false;
                AfterReceivedTimeout = false;
                ReceivedData = false;

                AddLog("RX", RxByteBuf);
            }

            return RxByteBuf;
        }

        private void WaitTotalTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            WaitTotalTimeout = true;
        }

        private void AfterReceivedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            AfterReceivedTimeout = true;
        }

        private void inPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (SerialPortReceived == null)
            {
                StringBuilder currentline = new StringBuilder();
                System.Threading.Thread.Sleep(ReceiveDelayMs);           //

                // 非常重要  isOpen的判断是临时的
                while (Port1.IsOpen && Port1.BytesToRead > 0)
                {
                    byte ch = (byte)Port1.ReadByte();
                    currentline.Append(ch.ToString("X2"));
                }

                //补丁，防止收到单个字符
                if (currentline.Length > 0)
                {
                    RxByteStr += currentline.ToString();
                    RxByteBuf = CommArithmetic.HexStringToByteArray(RxByteStr);

                    ReceivedData = true;

                    if (RxExpStr != null && RxExpStr != "")
                    {
                        if (RxByteStr.Contains(RxExpStr) == true)
                        {
                            ReceivedExpStr = true;
                        }
                    }

                    // 刷新计时器，重新计时
                    if (EnableRxAfterTimer == true)
                    {
                        AfterReceivedTimer.Stop();
                        AfterReceivedTimer.Start();
                    }
                }
            }
            else
            {
                SerialPortEventArgs args = new SerialPortEventArgs();

                StringBuilder currentline = new StringBuilder();

                Thread.Sleep(60);                   //尝试不要断开接收数据，在SurfaceBook 上，断开的时间大约为22ms

                while (Port1.BytesToRead > 0)
                {
                    byte ch = (byte)Port1.ReadByte();
                    currentline.Append(ch.ToString("X2"));
                }

                args.ReceivedBytes = CommArithmetic.HexStringToByteArray(currentline.ToString());

                RxByteBuf = args.ReceivedBytes;

                Logger.AddLogAutoTime("RX:\t" + CommArithmetic.ToHexString(args.ReceivedBytes));

                ReceivedData = true;

                SerialPortReceived.Invoke(this, args);
            }
        }

        private void Port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Receiving = true;
                inPort1_DataReceived(sender, e);
                Receiving = false;
            }
            catch (Exception)
            {
                return;
            }
        }

        public static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
        {
            List<string> strs = new List<string>();
            try
            {
                string queryString = "select * from " + hardType;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(queryString))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null && hardInfo.Properties[propKey].Value.ToString().Contains("(COM"))
                        {
                            strs.Add(hardInfo.Properties[propKey].Value.ToString());
                        }
                    }
                    searcher.Dispose();
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            { strs = null; }
        }

        public static string GetSerialPortName(string DeviceName)
        {
            //临时测试
            int first = DeviceName.IndexOf('(');
            int last = DeviceName.IndexOf(')');
            return DeviceName.Substring(first + 1, last - first - 1);
        }

        public static String[] GetSerialPorts()
        {
            return MulGetHardwareInfo(HardwareEnum.Win32_SerialPort, "Name");    //调用方式通过WMI获取COM端口 
        }

    }
}
