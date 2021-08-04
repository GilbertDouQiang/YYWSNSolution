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
using YyWsnDeviceLibrary;

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

            this.Title += " v" +
               System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
            byte[] CrcBuf = MyCustomFxn.HexStringToByteArray(tbx_Buf.Text);
            if (CrcBuf == null || CrcBuf.Length == 0)
            {
                return;
            }

            // 获取Seed
            byte[] SeedBuf = MyCustomFxn.HexStringToByteArray(tbx_Seed.Text);
            UInt32 Seed = 0;
            if (SeedBuf == null)
            {
                Seed = 0;
            }
            else if (SeedBuf.Length == 1)
            {
                Seed = (UInt16)SeedBuf[0];
            }
            else if (SeedBuf.Length == 2)
            {
                Seed = (UInt16)((SeedBuf[0] << 8) | (SeedBuf[1] << 0));
            }
            else
            {
                Seed = (UInt32)((SeedBuf[0] << 24) | (SeedBuf[1] << 16) | (SeedBuf[2] << 8) | (SeedBuf[3] << 0));
            }

            // 获取生成多项式
            byte[] PolynomialBuf = MyCustomFxn.HexStringToByteArray(tbx_Polynomial.Text);
            UInt32 Polynomial = 0;
            if (PolynomialBuf == null)
            {
                Polynomial = 0;
            }
            else if (PolynomialBuf.Length == 1)
            {
                Polynomial = (UInt32)PolynomialBuf[0];
            }
            else if (PolynomialBuf.Length == 2)
            {
                Polynomial = (UInt32)((PolynomialBuf[0] << 8) | (PolynomialBuf[1] << 0));
            }
            else if (PolynomialBuf.Length == 4)
            {
                Polynomial = (UInt32)((PolynomialBuf[0] << 24) | (PolynomialBuf[1] << 16) | (PolynomialBuf[2] << 8) | (PolynomialBuf[3] << 0));
            }

            // 计算CRC
            UInt32 CRC = 0;
            switch (cbbAlgorithm.SelectedIndex)
            {
                case 0:     // YYWSN_CRC16
                    {
                        CRC = MyCustomFxn.CRC16((UInt16)(Polynomial & 0xFFFF), (UInt16)(Seed & 0xFFFF), CrcBuf, 0, (UInt16)CrcBuf.Length);
                        tbx_Crc.Text = CRC.ToString("X4");
                        break;
                    }
                case 1:     // CHISCDC
                    {
                        CRC = MyCustomFxn.CRC16_By_CHISCDC(CrcBuf, 0, (UInt16)CrcBuf.Length);
                        tbx_Crc.Text = CRC.ToString("X4");
                        break;
                    }
                case 2:     // Zigin
                    {
                        CRC = MyCustomFxn.CRC16_By_Zigin(CrcBuf, 0, (UInt16)CrcBuf.Length);
                        tbx_Crc.Text = CRC.ToString("X4");
                        break;
                    }
                case 3:     // YYWSN_CRC8
                    {
                        CRC = MyCustomFxn.CRC8((byte)Polynomial, (byte)Seed, CrcBuf, 0, (UInt16)CrcBuf.Length);
                        tbx_Crc.Text = CRC.ToString("X2");
                        break;
                    }
                case 4:     // Modbus_CRC16
                    {
                        CRC = MyCustomFxn.CRC16_Modbus((UInt16)(Polynomial & 0xFFFF), (UInt16)(Seed & 0xFFFF), CrcBuf, 0, (UInt16)CrcBuf.Length);
                        tbx_Crc.Text = CRC.ToString("X4");
                        break;
                    }
                case 5:     // CheckSum 8bit
                    {
                        CRC = MyCustomFxn.CheckSum8((byte)Seed, CrcBuf, 0, (UInt16)CrcBuf.Length);
                        tbx_Crc.Text = CRC.ToString("X2");
                        break;
                    }
                case 6:     // CRC32
                    {
                        CRC = MyCustomFxn.CRC32(Polynomial, Seed, 0xFFFFFFFF, CrcBuf, 0, (UInt32)CrcBuf.Length, true, true);
                        tbx_Crc.Text = CRC.ToString("X8");
                        break;
                    }
                default:
                    {
                        return;
                    }
            }

            // 显示字节数组的长度
            tbx_Len.Text = CrcBuf.Length.ToString();
        }

        private void cbbAlgorithm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbbAlgorithm.SelectedIndex)
            {
                case 0:     // YYWSN_CRC16
                    {
                        tbx_Seed.Text = "0000";
                        tbx_Polynomial.Text = "1021";

                        tbx_Seed.IsEnabled = true;
                        tbx_Polynomial.IsEnabled = true;

                        tbx_Seed.MaxLength = 4;
                        tbx_Polynomial.MaxLength = 4;
                        break;
                    }
                case 1:     // CHISCDC
                    {
                        tbx_Seed.Text = "";
                        tbx_Polynomial.Text = "";

                        tbx_Seed.IsEnabled = false;
                        tbx_Polynomial.IsEnabled = false;
                        break;
                    }
                case 2:     // Zigin
                    {
                        tbx_Seed.Text = "";
                        tbx_Polynomial.Text = "";

                        tbx_Seed.IsEnabled = false;
                        tbx_Polynomial.IsEnabled = false;
                        break;
                    }
                case 3:     // YYWSN_CRC8
                    {
                        tbx_Seed.Text = "00";
                        tbx_Polynomial.Text = "31";

                        tbx_Seed.IsEnabled = true;
                        tbx_Polynomial.IsEnabled = true;

                        tbx_Seed.MaxLength = 2;
                        tbx_Polynomial.MaxLength = 2;
                        break;
                    }
                case 4:     // Modbus_CRC16
                    {
                        tbx_Seed.Text = "FFFF";
                        tbx_Polynomial.Text = "A001";

                        tbx_Seed.IsEnabled = true;
                        tbx_Polynomial.IsEnabled = true;

                        tbx_Seed.MaxLength = 4;
                        tbx_Polynomial.MaxLength = 4;
                        break;
                    }
                case 5:     // CheckSum 8bit
                    {
                        tbx_Seed.Text = "00";
                        tbx_Polynomial.Text = "";

                        tbx_Seed.IsEnabled = true;
                        tbx_Polynomial.IsEnabled = false;

                        tbx_Seed.MaxLength = 2;
                        tbx_Polynomial.MaxLength = 2;
                        break;
                    }
                case 6:     // CRC32
                    {
                        tbx_Seed.Text = "FFFFFFFF";
                        tbx_Polynomial.Text = "04C11DB7";

                        tbx_Seed.IsEnabled = true;
                        tbx_Polynomial.IsEnabled = true;

                        tbx_Seed.MaxLength = 8;
                        tbx_Polynomial.MaxLength = 8;
                        break;
                    }
                default:
                    {
                        return;
                    }
            }
        }
    }
}
