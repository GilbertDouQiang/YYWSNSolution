using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using YyWsnCommunicatonLibrary;

namespace HyperWSN_Setup_M30
{
    /// <summary>
    /// CC1310的串口升级
    /// </summary>
    public class Bootloader
    {
        /// <summary>
        /// 串口设备的名称(含有COM口)
        /// </summary>
        public string SerialPortName { get; set; }

        /// <summary>
        /// 镜像文件
        /// </summary>
        public OpenFileDialog FileDialog { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string LogText = string.Empty;

        /// <summary>
        /// 子线程
        /// </summary>
        private Thread ThisThread = null;

        /// <summary>
        /// 串口
        /// </summary>
        private SerialPortHelper SerialPort;

        public Bootloader(string serialPortName, OpenFileDialog fileDialog)
        {
            SerialPortName = serialPortName;
            FileDialog = fileDialog;
        }

        private void SerialPort_Init()
        {
            if (SerialPort != null)
            {
                if (SerialPort.IsOpen() == true)
                {
                    SerialPort.ClosePort();
                }
            }

            SerialPort = new SerialPortHelper();
            SerialPort.IsLogger = true;
            SerialPort.InitCOM(SerialPortName, 115200);
            SerialPort.ReceiveDelayMs = 2;
            SerialPort.OpenPort();
        }

        private void SerialPort_Close()
        {
            if (SerialPort != null)
            {
                if (SerialPort.IsOpen() == true)
                {
                    SerialPort.ClosePort();
                }
            }
        }

        /// <summary>
        /// 创建一个新数据包，用于同步串口波特率
        /// </summary>
        /// <returns></returns>
        private byte[] NewPkt_AutoUartBaud()
        {
            return new byte[2] { 0x55, 0x55 };
        }

        /// <summary>
        /// 创建一个新数据包，用于Ping
        /// </summary>
        /// <returns></returns>
        private byte[] NewPkt_Ping()
        {
            return new byte[3] { 0x03, 0x20, 0x20 };
        }

        /// <summary>
        /// 同步波特率， MaxTryNum设为1
        /// </summary>
        /// <param name="MaxTryNum"></param>
        /// <returns></returns>
        public int AutoUartBaud(UInt16 MaxTryNum)
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            bool Suc = false;

            for (int iX = 0; iX < MaxTryNum; iX++)
            {
                TxBuf = NewPkt_AutoUartBaud();
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 4);      // 经验证，波特率是115200的情况下，设为4ms，同步是成功的。
                if (RxBuf == null || RxBuf.Length == 0)
                {
                    System.Threading.Thread.Sleep(5);
                    continue;
                }

                if (RxBuf.Length != 2)
                {
                    continue;
                }

                if (RxBuf[0] == 0x00 && RxBuf[1] == 0xCC)
                {
                    Suc = true;
                    break;
                }
            }

            if (Suc == false)
            {
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Ping，建议将MaxTryNum设为90
        /// </summary>
        /// <param name="MaxTryNum"></param>
        /// <returns></returns>
        public int Ping(UInt16 MaxTryNum)
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            bool Suc = false;

            for (int iX = 0; iX < MaxTryNum; iX++)
            {
                TxBuf = NewPkt_Ping();
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 4);      // 经验证，波特率是115200的情况下，设为4ms，Ping是成功的。
                if (RxBuf == null || RxBuf.Length == 0)
                {
                    System.Threading.Thread.Sleep(5);
                    continue;
                }

                if (RxBuf.Length != 2)
                {
                    continue;
                }

                if (RxBuf[0] == 0x00 && RxBuf[1] == 0xCC)
                {
                    Suc = true;
                    break;
                }
            }

            if (Suc == false)
            {
                return -1;
            }

            return 0;
        }

        private void ThreadFxn()
        {
            int error = 0;
            bool Suc = false;

            LogText = "";

            try
            {
                do
                {
                    LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   开始烧录\t" + FileDialog.SafeFileName + "\r\n";

                    SerialPort_Init();

                    error = AutoUartBaud(1);
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   同步失败\t" + error.ToString() + "\r\n";
                    }
                    else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   同步成功" + "\r\n";
                    }

                    error = Ping(90);
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   通信失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   通信成功" + "\r\n";
                    }

                    // error = Read();
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   备份失败\t" + error.ToString() + "\r\n";
                        break;
                    }else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   备份成功" + "\r\n";
                    }
                                     

                    // RebootReset();
                    LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   设备重启" + "\r\n";

                    Suc = true;

                } while (false);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                SerialPort_Close();
            }

            /*
            if (Suc == true)
            {
                LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   烧录成功\t耗时" + TimeCntOfBsl.ToString() + "秒钟\r\n";
            }
            else
            {
                LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   烧录失败" + "\r\n";
            }
            */

            LogText += "\r\n*****************************************************************************\r\n";
        }

        public void Execute()
        {
            ThisThread = new Thread(ThreadFxn);
            ThisThread.Start();
        }       
    }
}
