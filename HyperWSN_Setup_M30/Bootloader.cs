using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;

namespace HyperWSN_Setup_M30
{
    /*
    public enum BslDeviceType
    {
        BslDeviceType_CC1310 = 0,
        BslDeviceType_CC1352P = 1
    }

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
        /// 烧录的设备类型
        /// </summary>
        public BslDeviceType deviceType { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string LogText = string.Empty;

        /// <summary>
        /// 子线程
        /// </summary>
        private Thread ThisThread = null;

        public const UInt16 ReadMaxUnit = 253;              // 读取数据时，一次读取的最大数量，单位：Byte；

        private byte[] Content { get; set; }

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
            SerialPort.InitCOM(SerialPortName, 115200);     // 简单测试，将波特率设为460800和230400，都无法完成整个烧录过程。
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
        /// 创建一个数据包，用于读取数据
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="Size">一次读取的最大数量是253个字节</param>
        /// <returns></returns>
        private byte[] NewPkt_Read(UInt32 Addr, UInt16 Size)
        {
            byte[] buf = new byte[9];

            // 数据包长度
            buf[0] = 0x09;

            // 校验和
            buf[1] = 0x00;

            // 命令
            buf[2] = 0x2A;

            // Addr
            buf[3] = (byte)((Addr & 0xFF000000) >> 24);
            buf[4] = (byte)((Addr & 0x00FF0000) >> 16);
            buf[5] = (byte)((Addr & 0x0000FF00) >> 8);
            buf[6] = (byte)((Addr & 0x000000FF) >> 0);

            // Access Type, 0: 8-bits; 1: 32-bits;
            buf[7] = 0x00;

            // Size, Max value is 253
            if (Size > ReadMaxUnit)
            {
                buf[8] = (byte)ReadMaxUnit;
            }
            else
            {
                buf[8] = (byte)Size;
            }

            // 计算校验和
            buf[1] = MyCustomFxn.CheckSum8(0, buf, 2, (UInt16)(buf.Length - 2));

            return buf;
        }

        /// <summary>
        /// 创建一个数据包，用于给与一个ACK
        /// </summary>
        /// <returns></returns>
        private byte[] NewPkt_Ack()
        {
            return new byte[1] { 0x03 };      
        }

        /// <summary>
        /// 创建一个数据包，用于擦除Sector
        /// </summary>
        /// <param name="Addr"></param>
        /// <returns></returns>
        private byte[] NewPkt_EraseSector(UInt32 Addr)
        {
            byte[] buf = new byte[7];

            // 数据包长度
            buf[0] = 0x07;

            // 校验和
            buf[1] = 0x00;

            // 命令
            buf[2] = 0x26;

            // Addr
            buf[3] = (byte)((Addr & 0xFF000000) >> 24);
            buf[4] = (byte)((Addr & 0x00FF0000) >> 16);
            buf[5] = (byte)((Addr & 0x0000FF00) >> 8);
            buf[6] = (byte)((Addr & 0x000000FF) >> 0);
                        
            // 计算校验和
            buf[1] = MyCustomFxn.CheckSum8(0, buf, 2, (UInt16)(buf.Length - 2));

            return buf;
        }

        /// <summary>
        /// 创建一个数据包，用于擦除Bank(整片)
        /// </summary>
        /// <returns></returns>
        private byte[] NewPkt_EraseBank()
        {
            return new byte[3] { 0x03, 0x2C, 0x2C };
        }

        /// <summary>
        /// 创建一个数据包，用于Download
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="Size"></param>
        /// <returns></returns>
        private byte[] NewPkt_Download(UInt32 Addr, UInt32 Size)
        {
            byte[] buf = new byte[11];

            // 数据包长度
            buf[0] = 0x0B;

            // 校验和
            buf[1] = 0x00;

            // 命令
            buf[2] = 0x21;

            // Addr
            buf[3] = (byte)((Addr & 0xFF000000) >> 24);
            buf[4] = (byte)((Addr & 0x00FF0000) >> 16);
            buf[5] = (byte)((Addr & 0x0000FF00) >> 8);
            buf[6] = (byte)((Addr & 0x000000FF) >> 0);

            // Size
            buf[7] = (byte)((Size & 0xFF000000) >> 24);
            buf[8] = (byte)((Size & 0x00FF0000) >> 16);
            buf[9] = (byte)((Size & 0x0000FF00) >> 8);
            buf[10] = (byte)((Size & 0x000000FF) >> 0);

            // 计算校验和
            buf[1] = MyCustomFxn.CheckSum8(0, buf, 2, (UInt16)(buf.Length - 2));

            return buf;
        }

        /// <summary>
        /// 创建一个数据包，用于Send Data
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="Size"></param>
        /// <returns></returns>
        private byte[] NewPkt_SendData(UInt32 Addr, byte Size)
        {
            byte[] buf = new byte[3 + Size];

            // 数据包长度
            buf[0] = (byte)( 3 + Size);

            // 校验和
            buf[1] = 0x00;

            // 命令
            buf[2] = 0x24;

            // 数据
            for(int iX = 0; iX < Size; iX++)
            {
                buf[3 + iX] = Content[Addr + iX];
            }

            // 计算校验和
            buf[1] = MyCustomFxn.CheckSum8(0, buf, 2, (UInt16)(buf.Length - 2));

            return buf;
        }

        /// <summary>
        /// 创建一个数据包，用于重启设备
        /// </summary>
        /// <returns></returns>
        private byte[] NewPkt_Reset()
        {
            return new byte[3] { 0x03, 0x25, 0x25 };
        }

        /// <summary>
        /// 创建一个数据包，用于设置CCFG中的Image Valid
        /// </summary>
        /// <returns></returns>
        private byte[] NewPkt_SetImageValid(UInt32 bootAddr)
        {
            byte[] buf = new byte[11];

            // 数据包长度
            buf[0] = 0x0B;

            // 校验和
            buf[1] = 0x00;

            // 命令
            buf[2] = 0x2D;

            // Field ID
            buf[3] = 0x00;
            buf[4] = 0x00;
            buf[5] = 0x00;
            buf[6] = 0x01;

            // Field Value
            buf[7] = (byte)((bootAddr & 0xFF000000) >> 24);
            buf[8] = (byte)((bootAddr & 0x00FF0000) >> 16);
            buf[9] = (byte)((bootAddr & 0x0000FF00) >> 8);
            buf[10] = (byte)((bootAddr & 0x000000FF) >> 0);

            // 计算校验和
            buf[1] = MyCustomFxn.CheckSum8(0, buf, 2, (UInt16)(buf.Length - 2));

            return buf;
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

        // 经过实测，同步波特率之后，单发一个0x03，连续三次，必然会得到0x00 0xCC的反馈。

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

        /// <summary>
        /// 读取一次数据
        /// </summary>
        /// <param name="Addr"></param>
        /// <param name="Size">此数值不能超过最大值限制</param>
        /// <returns></returns>
        public int ReadOnePacket(UInt32 Addr, UInt16 Size)
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            TxBuf = NewPkt_Read(Addr, Size);
            RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 200, 10);      // 经过简单验证，200和10是比较好的配置。
            if (RxBuf == null)
            {
                return -1;
            }

            // 给与ACK
            TxBuf = NewPkt_Ack();
            SerialPort.Send(TxBuf);

            if (RxBuf.Length < (Size + 4))
            {
                return -2;
            }

            if (RxBuf[0] != 0x00 || RxBuf[1] != 0xCC)
            {
                return -3;
            }

            // 长度位
            if(RxBuf[2] != Size + 2)
            {
                return -4;
            }

            // 校验和
            byte chkSum = MyCustomFxn.CheckSum8(0, RxBuf, 4, Size);
            if (chkSum != RxBuf[3])
            {
                return -5;
            }

            return 0;
        }

        /// <summary>
        /// 擦除Sector
        /// </summary>
        /// <param name="Addr"></param>
        /// <returns></returns>
        public int EraseSector(UInt32 Addr)
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            TxBuf = NewPkt_EraseSector(Addr);
            RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 100, 10);      // 经过简单验证，100和10是比较好的配置。
            if (RxBuf == null)
            {
                return -1;
            }

            if (RxBuf.Length < 2)
            {
                return -2;
            }

            if (RxBuf[0] != 0x00 || RxBuf[1] != 0xCC)
            {
                return -3;
            }

            return 0;
        }

        /// <summary>
        /// 擦除Bank(整片)
        /// </summary>
        /// <returns></returns>
        public int EraseBank()
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            TxBuf = NewPkt_EraseBank();
            RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 200, 10);      
            if (RxBuf == null)
            {
                return -1;
            }

            if (RxBuf.Length < 2)
            {
                return -2;
            }

            if (RxBuf[0] != 0x00 || RxBuf[1] != 0xCC)
            {
                return -3;
            }

            return 0;
        }

        public int MassErase2()
        {
            int error = 0;

            for (int iX = 0; iX < 32; iX++)
            {
                error = EraseSector((UInt32)(iX * 4096));
                if (error < 0)
                {
                    error = EraseSector((UInt32)(iX * 4096));
                    if (error < 0)
                    {
                        return error;
                    }
                }
            }

            return error;
        }

        public int MassErase()
        {
            return EraseBank();
        }

        /// <summary>
        /// 重启设备
        /// </summary>
        /// <returns></returns>
        public int Reset()
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            TxBuf = NewPkt_Reset();
            RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 100, 10);
            if (RxBuf == null)
            {
                return -1;
            }

            if (RxBuf.Length < 2)
            {
                return -2;
            }

            if (RxBuf[0] != 0x00 || RxBuf[1] != 0xCC)
            {
                return -3;
            }

            return 0;
        }

        public int Download(UInt32 Addr, UInt32 Size)
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            TxBuf = NewPkt_Download(Addr, Size);
            RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 100, 10);
            if (RxBuf == null)
            {
                return -1;
            }

            if (RxBuf.Length < 2)
            {
                return -2;
            }

            if (RxBuf[0] != 0x00 || RxBuf[1] != 0xCC)
            {
                return -3;
            }

            return 0;
        }

        public int SendData(UInt32 Addr, byte Size)
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            TxBuf = NewPkt_SendData(Addr, Size);
            RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 200, 10);
            if (RxBuf == null)
            {
                return -1;
            }

            if (RxBuf.Length < 2)
            {
                return -2;
            }

            if (RxBuf[0] != 0x00 || RxBuf[1] != 0xCC)
            {
                return -3;
            }

            return 0;
        }

        public int Read(UInt32 Addr, UInt16 Size)
        {
            const UInt16 Unit = ReadMaxUnit;
            UInt16 Total = 0;
            UInt16 aSize = 0;

            int error = 0;

            while(Total < Size)
            {
                if(Size - Total >= Unit)
                {
                    aSize = Unit;
                }
                else
                {
                    aSize = (UInt16)(Size - Total);
                }

                error = ReadOnePacket(Addr + Total, aSize);
                if(error < 0)
                {
                    error = ReadOnePacket(Addr + Total, aSize);
                    if (error < 0)
                    {
                        return error;
                    }
                }

                Total += aSize;
            }

            return 0;
        }

        public int Write()
        {
            const byte Unit = 252;
            UInt32 Total = 0;
            byte aSize = 0;

            UInt32 DstTotal = (UInt32)(Content.Length - 32);
            UInt32 Addr = 0;

            int error = 0;

            error = Download(0, DstTotal);
            if (error < 0)
            {
                error = Download(0, DstTotal);
                if (error < 0)
                {
                    return error;
                }
            }

            while (Total < DstTotal)
            {
                if (DstTotal - Total >= Unit)
                {
                    aSize = Unit;
                }
                else
                {
                    aSize = (byte)(DstTotal - Total);
                }

                error = SendData(Addr + Total, aSize);
                if (error < 0)
                {
                    error = SendData(Addr + Total, aSize);
                    if (error < 0)
                    {
                        return error - 10;
                    }
                }

                Total += aSize;
            }

            return 0;
        }

        public int SetImageValid()
        {
            UInt32 bootAddr = 0x00000000;

            byte[] TxBuf = null;
            byte[] RxBuf = null;

            TxBuf = NewPkt_SetImageValid(bootAddr);
            RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 2000, 500);
            if (RxBuf == null)
            {
                return -1;
            }

            if (RxBuf.Length < 2)
            {
                return -2;
            }

            if (RxBuf[0] != 0x00 || RxBuf[1] != 0xCC)
            {
                return -3;
            }

            return 0;
        }

        public int Open(string FilePath, int FileSize)
        {
            if (File.Exists(FilePath) == false)
            {
                return -1;                                      // 烧录文件不存在
            }

            // 构造全0x00的字节数组
            Content = new byte[FileSize];
            for (int iX = 0; iX < Content.Length; iX++)
            {
                Content[iX] = 0x00;
            }

            // 读取烧录文件的内容
            StreamReader FileReader = new StreamReader(FilePath, Encoding.ASCII);
            string FileTxt = FileReader.ReadToEnd();
            FileReader.Close();
            FileReader = null;

            // 拆分字符串
            string[] Sections = FileTxt.Split(new char[] { '@', 'q' }, StringSplitOptions.RemoveEmptyEntries);
            if (Sections == null || Sections.Length == 0)
            {
                return -2;
            }

            int error = 0;

            for (int iX = 0; iX < Sections.Length; iX++)
            {
                string[] aSection = Sections[iX].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (aSection == null || aSection.Length < 2)
                {
                    continue;
                }

                // 第一个元素是地址                
                UInt32 Addr = MyCustomFxn.HexStringToUInt32(aSection[0]);

                UInt32 Sum = 0;        // 累计该@后面跟随的数据的数量

                // 后面的元素都是数据内容
                for (int iJ = 1; iJ < aSection.Length; iJ++)
                {   // 处理一行数据
                    byte[] ByteBuf = MyCustomFxn.HexStringToByteArray(aSection[iJ]);
                    if (ByteBuf == null || ByteBuf.Length == 0)
                    {
                        continue;
                    }

                    if (Addr + Sum > Content.Length)
                    {
                        return -4;      // 烧录文件的大小超过了RAM镜像文件的大小
                    }

                    ByteBuf.CopyTo(Content, Addr + Sum);

                    // 如果这部分数据会跨越是一个Sector的起始地址，则先擦除，再写入。
                    const UInt32 SectorSize = 4096;                                             // 一个Sector的大小，单位：Byte

                    bool isSectorStart = false;                                                 // 起始地址是否是一个Sector的起始位置

                    if(0 == ((Addr + Sum) % SectorSize))
                    {
                        isSectorStart = true;                                                   // 该部分数据的起始地址正好是一个Sector的起始位置
                    }

                    UInt32 sectorStartNo = (Addr + Sum) / SectorSize;                               // 起始地址落在哪个sector
                    UInt32 sectorEndNo = (UInt32)((Addr + Sum + ByteBuf.Length - 1) / SectorSize);  // 结束地址落在哪个sector

                    if (isSectorStart == true || sectorEndNo > sectorStartNo)
                    {   // 认定，该部分数据必然跨越了某个Sector的起始地址，则擦除该Secotr

                        // 擦除，起始地址是：sectorEnd*SectorSize
                        error = EraseSector(sectorEndNo * SectorSize);
                        if (error < 0)
                        {   // 若失败，则重试一次
                            error = EraseSector(sectorEndNo * SectorSize);
                            if (error < 0)
                            {
                                return error;
                            }
                        }
                    }

                    // 将数据写入

     

                    Sum = (UInt32)(Sum + ByteBuf.Length);
                }

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

                    error = Open(FileDialog.FileName, 128 * 1024);
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   打开失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   打开成功" + "\r\n";
                    }

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

                    error = MassErase();
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   擦除失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   擦除成功" + "\r\n";
                    }

                    error = Write();
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   写入失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   写入成功" + "\r\n";
                    }

                    error = SetImageValid();
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   使能失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   使能成功" + "\r\n";
                    }

                    error = Read(0x1000, 1024);     // TODO: 2021-01-06 调试用
                    //error = Read(0x1FC000, 512);
                    if (error < 0)
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   备份失败\t" + error.ToString() + "\r\n";
                        break;
                    }else
                    {
                        LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   备份成功" + "\r\n";
                    }


                    error = Reset();
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

            if (Suc == true)
            {
                LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   烧录成功\t耗时" + TimeCntOfBsl.ToString() + "秒钟\r\n";
            }
            else
            {
                LogText += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   烧录失败" + "\r\n";
            }

            LogText += "\r\n*****************************************************************************\r\n";
        }

        public void Execute()
        {
            ThisThread = new Thread(ThreadFxn);
            ThisThread.Start();
        }       
    }
*/
}
