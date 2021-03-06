﻿using System;
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

namespace MD5_Tool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Title += "  v" +
               System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// 计算MD5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Calc_Click(object sender, RoutedEventArgs e)
        {
            byte[] encrypt = null;

            if (cbx_EncryptType.SelectedIndex == 0)
            {   // 字符串
                encrypt = Encoding.ASCII.GetBytes(tbx_Buf.Text);             // 明文
                if (encrypt == null || encrypt.Length > 64)
                {
                    MessageBox.Show("明文字符串编码错误或长度超限！");
                    return;
                }
            }
            else
            {   // HEX
                // 将16进制的字符串转换为字节数组
                encrypt = MyCustomFxn.HexStringToByteArray(tbx_Buf.Text);
                if (encrypt == null || encrypt.Length == 0)
                {
                    return;
                }
            }            

            MD5 aMD5 = new MD5(tbx_Key.Text);

            byte[] decrypt = aMD5.Calc(encrypt, (UInt32)encrypt.Length);        // 密文
            if (decrypt == null || decrypt.Length != 16)
            {
                MessageBox.Show("MD5计算失败！");
                return;
            }

            // 显示密文
            tbx_Result.Text = "";

            UInt16 iX;
            for (iX = 0; iX < (UInt16)decrypt.Length; iX++)
            {
                if (iX == (UInt16)(decrypt.Length - 1))
                {
                    tbx_Result.Text += decrypt[iX].ToString("X2");
                }
                else
                {
                    tbx_Result.Text += decrypt[iX].ToString("X2") + " ";
                }
            }
        }
    }
}
