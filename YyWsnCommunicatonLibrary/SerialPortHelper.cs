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
        public delegate void SerialPortReceivedEventHandler(object sender, SerialPortEventArgs e);
        public event SerialPortReceivedEventHandler SerialPortReceived;

        SerialPort port1;


        System.Timers.Timer timer;
        bool isGetResult = false;
        bool isTimeout = false;
        byte[] commandResult;

        //初始化SerialPort对象方法.PortName为COM口名称,例如"COM1","COM2"等,注意是string类型
        
        public void InitCOM(string PortName)
        {
            port1 = new SerialPort(PortName);
            port1.BaudRate = 115200;//波特率
            port1.Parity = Parity.None;//无奇偶校验位
            port1.StopBits = StopBits.One;//两个停止位
            //port1.Handshake = Handshake.RequestToSend;//控制协议
            port1.ReadBufferSize = 8192;
            port1.WriteBufferSize = 8192;
            //port1.ReceivedBytesThreshold = 4;//设置 DataReceived 事件发生前内部输入缓冲区中的字节数
            port1.DataReceived += Port1_DataReceived;//DataReceived事件委托
        }

        public bool OpenPort()
        {
            try
            {
                port1.Open();
            }
            catch
            {

            }
            if (port1.IsOpen)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //关闭串口的方法
        public void ClosePort()
        {
            port1.Close();
            if (!port1.IsOpen)
            {
                Console.WriteLine("the port is already closed!");
            }
        }

        //向串口发送数据，内部可以自动过滤空格
        public void SendCommand(string CommandString)
        {
            byte[] WriteBuffer = CommArithmetic.HexStringToByteArray(CommandString);
            port1.Write(WriteBuffer, 0, WriteBuffer.Length);
            Logger.AddLogAutoTime("Send:\t\t" +CommArithmetic.ToHexString(WriteBuffer));
        }

        /// <summary>
        /// 发送协议指令并获得反馈，如果超时，返回null；
        /// </summary>
        /// <param name="CommandBytes"></param>
        public byte[] SendCommand(byte[] CommandBytes,int Timeout)
        {
            //操作端口前，确保端口已经打开
            if (!port1.IsOpen)
            {
                return null; 
            }


            timer = new System.Timers.Timer();
            timer.Interval = Timeout;
            timer.Enabled = true;
            timer.Elapsed += Timer_Elapsed;
            //byte[] WriteBuffer = CommArithmetic.HexStringToByteArray(CommandString);
            port1.Write(CommandBytes, 0, CommandBytes.Length);
            Logger.AddLogAutoTime("Send:\t\t" + CommArithmetic.ToHexString(CommandBytes));
            isGetResult = false;
            while(!isTimeout)
            {
                if (isGetResult)
                {
                    timer.Dispose();
                    return commandResult;
                }

                Thread.Sleep(10);


            }

           
            return null;
        }

        public int SendCommand(byte[] CommandBytes)
        {
            //操作端口前，确保端口已经打开
            if (!port1.IsOpen)
            {
                return -1;
            }

            try
            {
                port1.Write(CommandBytes, 0, CommandBytes.Length);
                Logger.AddLogAutoTime("Send:\t\t" + CommArithmetic.ToHexString(CommandBytes));
            }
            catch (Exception)
            {

                return -2;
            }

            
            
            return 0;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            isTimeout = true;
            
        }

        private void Port1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPortEventArgs args = new SerialPortEventArgs();
            try
            {
                StringBuilder currentline = new StringBuilder();
                Thread.Sleep(60); //尝试不要断开接收数据，在SurfaceBook 上，断开的时间大约为22ms
                while (port1.BytesToRead > 0)
                {
                    byte ch =(byte) port1.ReadByte();
                    currentline.Append(ch.ToString("X2"));

                }
                args.ReceivedBytes = CommArithmetic.HexStringToByteArray(currentline.ToString());
                commandResult = args.ReceivedBytes;
                Logger.AddLogAutoTime("Received:\t" + CommArithmetic.ToHexString(args.ReceivedBytes));

                isGetResult = true;

                

            }
            catch (Exception)
            {

                return;
            }



            SerialPortReceived?.Invoke(this, args);



        }

        public static String[] GetSerialPorts()
        {

            //String[] Portname = SerialPort.GetPortNames();
            String[] ss = MulGetHardwareInfo(HardwareEnum.Win32_PnPEntity, "Name");//调用方式通过WMI获取COM端口 
            return ss;
        }


        public static string[] MulGetHardwareInfo(HardwareEnum hardType, string propKey)
        {

            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
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

    }
}
