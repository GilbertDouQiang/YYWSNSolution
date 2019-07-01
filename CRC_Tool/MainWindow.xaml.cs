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
            byte[] PolynomialBuf = MyCustomFxn.HexStringToByteArray(tbx_Polynomial.Text);
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
            //UInt16 CRC = MyCustomFxn.CRC16(Polynomial, Seed, CrcBuf, 0, (UInt16)CrcBuf.Length);
            UInt16 CRC = MyCustomFxn.CRC16_By_CHISCDC(CrcBuf, 0, (UInt16)CrcBuf.Length);

            // 显示字节数组的长度
            tbx_Len.Text = CrcBuf.Length.ToString();

            // 显示计算得出的CRC值
            tbx_Crc.Text = CRC.ToString("X4");
        }
    }
}
