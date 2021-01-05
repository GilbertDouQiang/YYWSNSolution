using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using YyWsnDeviceLibrary;

namespace YyWsnCommunicatonLibrary
{
    public class BSL
    {
        public enum Process
        {
            Ping,
            Password,
            Read,
            Write,
            MassErase,
            RebootReset,
        }

        public byte[] Content { get; set; }

        public UInt16 CrcChk { get; set; }

        /// <summary>
        /// 读取烧录文件，并在RAM中生成镜像文件
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="FileSize"> 期望在RAM中生成的镜像文件的大小 </param>
        /// <returns></returns>
        public int MyOpen(string FilePath, int FileSize)
        {
            if (File.Exists(FilePath) == false)
            {
                return -1;                                      // 烧录文件不存在
            }

            // 构造全0xFF的字节数组
            Content = new byte[FileSize];
            for (int iX = 0; iX < Content.Length; iX++)
            {
                Content[iX] = 0xFF;
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

            for (int iX = 0; iX < Sections.Length; iX++)
            {
                string[] aSection = Sections[iX].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (aSection == null || aSection.Length < 2)
                {
                    continue;
                }

                // 第一个元素是地址                
                int Addr = (int)MyCustomFxn.HexStringToUInt32(aSection[0]);

                int Sum = 0;

                // 后面的元素都是数据内容
                for (int iJ = 1; iJ < aSection.Length; iJ++)
                {
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

                    Sum += ByteBuf.Length;
                }

            }

            return 0;
        }

        public static byte[] MyPing()
        {
            return new byte[1] { 0xFF };
        }

        public static byte[] MyPassword()
        {
            byte[] Buf = new byte[262];
            int Len = 0;

            // Header
            Buf[Len++] = 0x80;

            // Length
            Buf[Len++] = 0x01;          // 低
            Buf[Len++] = 0x01;          // 高

            // CMD
            Buf[Len++] = 0x21;

            // Data
            string Password = @"C9 BD B6 AB D1 D7 D2 BB D6 C7 C4 DC BF C6 BC BC D3 D0 CF DE B9 AB CB BE A3 BA C1 D6 BF CB BC E1 A1 A2 F1 BC C7 BF A1 A2 CE E2 C9 D9 C1 FA A1 A2 CD F5 B4 F3 CB A7 A1 A2 B8 DF BD F0 A1 A2 BC CD C2 B3 D1 DE A3 BB C2 ED C1 E9 C3 F4 A1 A2 C7 D8 CE F5 A1 A2 D5 D4 B4 E4 CF E3 A1 A2 B9 F9 D1 C7 C0 F6 A1 A2 B3 F5 EC BF D0 F9 A1 A2 CB EF D1 E0 B1 F2 A3 BB FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF F1 BC C7 BF";
            byte[] DataBuf = MyCustomFxn.HexStringToByteArray(Password);
            if (DataBuf == null || DataBuf.Length != 256)
            {
                return null;
            }
            DataBuf.CopyTo(Buf, Len);
            Len += DataBuf.Length;

            // CRC16: seed = 0xFFFF
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, Buf, 3, (UInt16)(Len - 3));
            Buf[Len++] = (byte)((crc & 0x00FF) >> 0);
            Buf[Len++] = (byte)((crc & 0xFF00) >> 8);

            return Buf;
        }

        public byte[] MyRead(UInt32 Addr, UInt16 Size)
        {
            if (Addr >= Content.Length || Addr + Size > Content.Length)
            {
                return null;
            }

            byte[] Buf = new byte[12];
            int Len = 0;

            // Header
            Buf[Len++] = 0x80;

            // Length
            Buf[Len++] = 0x07;          // 低
            Buf[Len++] = 0x00;          // 高

            // CMD
            Buf[Len++] = 0x28;

            // Addr
            Buf[Len++] = (byte)((Addr & 0x000000FF) >> 0);
            Buf[Len++] = (byte)((Addr & 0x0000FF00) >> 8);
            Buf[Len++] = (byte)((Addr & 0x00FF0000) >> 16);
            Buf[Len++] = (byte)((Addr & 0xFF000000) >> 24);

            // Size
            Buf[Len++] = (byte)((Size & 0x00FF) >> 0);
            Buf[Len++] = (byte)((Size & 0xFF00) >> 8);

            // CRC16: seed = 0xFFFF
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, Buf, 3, (UInt16)(Len - 3));
            Buf[Len++] = (byte)((crc & 0x00FF) >> 0);
            Buf[Len++] = (byte)((crc & 0xFF00) >> 8);

            return Buf;
        }

        public byte[] MyWrite(UInt32 Addr, UInt16 Size, ref bool Ignore)
        {
            Ignore = false;

            if (Size > 256)
            {
                return null;
            }

            if (Content == null || Addr + Size > Content.Length)
            {
                return null;
            }

            byte[] Buf = new byte[10 + Size];
            int Len = 0;

            // Header
            Buf[Len++] = 0x80;

            // Length
            Buf[Len++] = (byte)(((Size + 5) & 0x00FF) >> 0);        // 低
            Buf[Len++] = (byte)(((Size + 5) & 0xFF00) >> 8);        // 高

            // CMD
            Buf[Len++] = 0x20;

            // Addr
            Buf[Len++] = (byte)((Addr & 0x000000FF) >> 0);
            Buf[Len++] = (byte)((Addr & 0x0000FF00) >> 8);
            Buf[Len++] = (byte)((Addr & 0x00FF0000) >> 16);
            Buf[Len++] = (byte)((Addr & 0xFF000000) >> 24);

            // Data
            byte AllIsF = 0xFF;

            for (int iX = 0; iX < Size; iX++)
            {
                AllIsF &= Content[Addr + iX];

                Buf[Len++] = Content[Addr + iX];
            }

            // 若内容全部是FF，则可以忽略此次写入
            if(AllIsF == 0xFF)
            {
                Ignore = true;
            }

            // CRC16: seed = 0xFFFF
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, Buf, 3, (UInt16)(Len - 3));
            Buf[Len++] = (byte)((crc & 0x00FF) >> 0);
            Buf[Len++] = (byte)((crc & 0xFF00) >> 8);

            return Buf;
        }

        public byte[] MyCheck(UInt32 Addr, UInt16 Size)
        {
            if (Content == null || Addr + Size > Content.Length)
            {
                return null;
            }

            byte[] Buf = new byte[12];
            int Len = 0;

            // Header
            Buf[Len++] = 0x80;

            // Length
            Buf[Len++] = 0x07;                  // 低
            Buf[Len++] = 0x00;                  // 高

            // CMD
            Buf[Len++] = 0x26;

            // Addr
            Buf[Len++] = (byte)((Addr & 0x000000FF) >> 0);
            Buf[Len++] = (byte)((Addr & 0x0000FF00) >> 8);
            Buf[Len++] = (byte)((Addr & 0x00FF0000) >> 16);
            Buf[Len++] = (byte)((Addr & 0xFF000000) >> 24);

            // Size
            Buf[Len++] = (byte)((Size & 0x00FF) >> 0);      // 低
            Buf[Len++] = (byte)((Size & 0xFF00) >> 8);      // 高

            // CRC16: seed = 0xFFFF
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, Buf, 3, (UInt16)(Len - 3));
            Buf[Len++] = (byte)((crc & 0x00FF) >> 0);
            Buf[Len++] = (byte)((crc & 0xFF00) >> 8);

            // 记录正确的CrcChk值
            CrcChk = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, Content, Addr, Size);

            return Buf;
        }

         public static byte[] MyMassErase()
        {
            byte[] Buf = new byte[6];
            int Len = 0;

            // Header
            Buf[Len++] = 0x80;

            // Length
            Buf[Len++] = 0x01;          // 低
            Buf[Len++] = 0x00;          // 高

            // CMD
            Buf[Len++] = 0x15;

            // CRC16: seed = 0xFFFF
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, Buf, 3, (UInt16)(Len - 3));
            Buf[Len++] = (byte)((crc & 0x00FF) >> 0);
            Buf[Len++] = (byte)((crc & 0xFF00) >> 8);

            return Buf;
        }

        public static byte[] MyRebootReset()
        {
            byte[] Buf = new byte[6];
            int Len = 0;

            // Header
            Buf[Len++] = 0x80;

            // Length
            Buf[Len++] = 0x01;          // 低
            Buf[Len++] = 0x00;          // 高

            // CMD
            Buf[Len++] = 0x25;

            // CRC16: seed = 0xFFFF
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, Buf, 3, (UInt16)(Len - 3));
            Buf[Len++] = (byte)((crc & 0x00FF) >> 0);
            Buf[Len++] = (byte)((crc & 0xFF00) >> 8);

            return Buf;
        }

        public int MyRxRight(byte[] RxBuf, byte[] TxBuf)
        {
            if (RxBuf == null || RxBuf.Length < 8)
            {
                return -1;
            }

            // ACK
            if (RxBuf[0] != 0x00)
            {
                return -2;
            }

            // Header
            if (RxBuf[1] != 0x80)
            {
                return -3;
            }

            // Length
            UInt16 Length = (UInt16)(((UInt16)RxBuf[2] << 0) | ((UInt16)RxBuf[3] << 8));
            if (Length < 1 || Length != RxBuf.Length - 6)
            {
                return -4;
            }

            // CRC
            UInt16 CaCrc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, RxBuf, 4, Length);
            UInt16 RxCrc = (UInt16)(((UInt16)RxBuf[4 + Length] << 0) | ((UInt16)RxBuf[5 + Length] << 8));
            if (CaCrc != RxCrc)
            {
                return -5;
            }

            // CMD
            byte TxCmd = TxBuf[3];
            byte RxCmd = RxBuf[4];
            byte MSG = RxBuf[5];

            switch (TxCmd)
            {
                case 0x15:      // Mass Erase
                    {
                        if (RxCmd != 0x3B || MSG != 0x00)
                        {
                            return -7;
                        }
                        break;
                    }
                case 0x20:      // Write
                    {
                        if (RxCmd != 0x3B || MSG != 0x00)
                        {
                            return -8;
                        }
                        break;
                    }
                case 0x21:      // Password
                    {
                        if (RxCmd != 0x3B || MSG != 0x00)
                        {
                            return -9;
                        }
                        break;
                    }
                case 0x26:      // CRC Check
                    {
                        if (RxCmd != 0x3A || Length < 3)
                        {
                            return -10;
                        }

                        if (CrcChk != (UInt16)(((UInt16)RxBuf[5] << 0) | ((UInt16)RxBuf[6] << 8)))
                        {
                            return -11;
                        }
                        break;
                    }
                default:
                    {
                        return -6;
                    }
            }

            return 0;
        }

        public int MyRxRight_Read(byte[] RxBuf, int IndexOfStart, bool First, ref int BufStart, ref UInt16 BufLen, ref int PktLen)
        {
            if (RxBuf == null)
            {
                return -1;
            }

            int AckOft = 0;

            if (First == true)
            {
                if ((RxBuf.Length - IndexOfStart) < 8)
                {
                    return -2;
                }

                // ACK
                if (RxBuf[IndexOfStart] != 0x00)
                {
                    return -3;
                }

                AckOft = 1;
            }
            else
            {
                if ((RxBuf.Length - IndexOfStart) < 7)
                {
                    return -4;
                }

                AckOft = 0;
            }

            int RxLen = RxBuf.Length - IndexOfStart;

            IndexOfStart += AckOft;

            // Header
            if (RxBuf[IndexOfStart] != 0x80)
            {
                return -5;
            }

            // Length
            UInt16 Length = (UInt16)(((UInt16)RxBuf[IndexOfStart + 1] << 0) | ((UInt16)RxBuf[IndexOfStart + 2] << 8));
            if (Length < 1 || Length > RxLen - AckOft - 5)
            {
                return -6;
            }

            // CRC
            UInt16 CaCrc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0xFFFF, RxBuf, (UInt16)(IndexOfStart + 3), Length);
            UInt16 RxCrc = (UInt16)(((UInt16)RxBuf[IndexOfStart + 3 + Length] << 0) | ((UInt16)RxBuf[IndexOfStart + 4 + Length] << 8));
            if (CaCrc != RxCrc)
            {
                return -7;
            }

            // CMD
            byte RxCmd = RxBuf[IndexOfStart + 3];
            if (RxCmd != 0x3A)
            {
                return -8;
            }

            BufStart = IndexOfStart + 4;
            BufLen = (UInt16)( Length - 1);
            PktLen = Length + 5 + AckOft;

            return 0;
        }

        public int MyOverWrite(UInt32 Addr, byte[] Buf)
        {
            if (Content == null || Addr + Buf.Length > Content.Length)
            {
                return -1;
            }

            Buf.CopyTo(Content, Addr);

            return 0;
        }
    }
}
