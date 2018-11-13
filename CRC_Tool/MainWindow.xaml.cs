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

namespace CRC_Tool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 判断字符串中是否有非16进制数
        /// </summary>
        /// <param name="hexStr">此处输入的字符串已经去除了内部的空格</param>
        /// <returns></returns>
        private static UInt16 HexString_isRight(string hexStr)
        {
            byte thisValue = 0;
            UInt16 StrLen = 0;

            // 字符串长度保护
            if (hexStr.Length > 0xFFFE)
            {
                StrLen = 0xFFFE;
            }
            else
            {
                StrLen = (UInt16)hexStr.Length;
            }

            // 判断是否含有非16进制字符
            for (UInt16 iCnt = 0; iCnt < StrLen; iCnt++)
            {
                thisValue = (byte)hexStr[iCnt];

                if ((thisValue >= '0' && thisValue <= '9') || (thisValue >= 'a' && thisValue <= 'f') || (thisValue >= 'A' && thisValue <= 'F'))
                {   // 是16进制数
                    continue;
                }
                else
                {   // 字符串里含有非16进制数
                    return iCnt;
                }
            }

            return StrLen;
        }

        /// <summary>
        /// 将16进制的字符串转换为字节数组
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static byte[] HexStringToByteArray(string hexStr)
        {
            try
            {
                // 去除字符串中的空格
                hexStr = hexStr.Replace(" ", "");

                // 判断字符串中是否有非16进制数
                UInt16 StrLen = HexString_isRight(hexStr);
                if (StrLen == 0)
                {
                    return null;
                }

                // 计算字节数组的长度
                UInt16 BufLen = 0;
                if (0 == (StrLen % 2))
                {
                    BufLen = (UInt16)(StrLen / 2);
                }
                else
                {
                    BufLen = (UInt16)(StrLen / 2 + 1);
                }

                // 创建字节数组
                byte[] Buf = new byte[BufLen];

                // 填充字节数组的内容
                for (UInt16 iCnt = 0; iCnt < BufLen - 1; iCnt++)
                {
                    Buf[iCnt] = (byte)Convert.ToByte(hexStr.Substring(iCnt * 2, 2), 16);
                }
                if (0 == (StrLen % 2))
                {
                    Buf[BufLen - 1] = (byte)Convert.ToByte(hexStr.Substring((BufLen - 1) * 2, 2), 16);
                }
                else
                {
                    Buf[BufLen - 1] = (byte)Convert.ToByte(hexStr.Substring((BufLen - 1) * 2, 1), 16);
                }

                return Buf;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 计算CRC
        /// </summary>
        /// <param name="polynomial"></param>
        /// <param name="seed"></param>
        /// <param name="input"></param>
        /// <param name="nbrOfBytes"></param>
        /// <returns></returns>
        UInt16 CRC16(UInt16 polynomial, UInt16 seed, byte[] input, UInt16 nbrOfBytes)
        {
            UInt16 crc = seed, bit = 0, byteCtr = 0;

            for (byteCtr = 0; byteCtr < nbrOfBytes; byteCtr++)
            {
                crc ^= (UInt16)(input[byteCtr] << 8);

                for (bit = 8; bit > 0; bit--)
                {
                    if ((crc & 0x8000) == 0)
                    {
                        crc <<= 1;
                    }
                    else
                    {
                        crc = (UInt16)((crc << 1) ^ polynomial);
                    }
                }
            }

            return crc;
        }

        /// <summary>
        /// 计算CRC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Calc_Click(object sender, RoutedEventArgs e)
        {
            // 清除上一次的显示结果
            tbx_Len.Text = "";
            tbx_Crc.Text = "";

            // 将16进制的字符串转换为字节数组
            byte[] CrcBuf = HexStringToByteArray(tbx_Buf.Text);
            if (CrcBuf == null || CrcBuf.Length == 0)
            {
                return;
            }

            // 获取Seed
            byte[] SeedBuf = HexStringToByteArray(tbx_Seed.Text);
            UInt16 Seed = 0;
            if (SeedBuf == null)
            {
                Seed = 0;
            }
            else if (SeedBuf.Length == 1)
            {
                Seed = (UInt16)SeedBuf[0];
            }
            else
            {
                Seed = (UInt16)((SeedBuf[0] << 8) | (SeedBuf[1] << 0));
            }

            // 获取生成多项式
            byte[] PolynomialBuf = HexStringToByteArray(tbx_Polynomial.Text);
            UInt16 Polynomial = 0;
            if (PolynomialBuf == null)
            {
                Polynomial = 0;
            }
            else if (PolynomialBuf.Length == 1)
            {
                Polynomial = (UInt16)PolynomialBuf[0];
            }
            else
            {
                Polynomial = (UInt16)((PolynomialBuf[0] << 8) | (PolynomialBuf[1] << 0));
            }

            // 计算CRC
            UInt16 CRC = CRC16(Polynomial, Seed, CrcBuf, (UInt16)CrcBuf.Length);


            // 显示字节数组的长度
            tbx_Len.Text = CrcBuf.Length.ToString();

            // 显示计算得出的CRC值
            tbx_Crc.Text = CRC.ToString("X4");
        }
    }
}
