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

using System.Timers;
using System.IO;

namespace Split_Tool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly object obj = new object();      // 互斥锁

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 单个文件分割函数， 可以将指定文件分割为若干个子文件
        /// fileIn: 子文件名形如"D:\\file.rar@_1.split"
        /// MaxLenKB: 单个子文件最大容量，单位：KB
        /// delete: 分割完成后是否删除原文件
        /// </summary>
        public Int16 fileSplit(String fileIn, UInt32 MaxLenKB, bool delete)
        {
            //输入文件校验
            if (fileIn == null || System.IO.File.Exists(fileIn) == false)
            {
                MessageBox.Show("文件:\n\n" + fileIn + "\n\n不存在！");
                return -1;
            }

            //取文件名和后缀

            //从文件创建输入流
            FileStream FileIn = new FileStream(fileIn, FileMode.Open);

            // 若本身就符合大小要求，则不必再拆分
            if (FileIn.Length <= MaxLenKB * 1024)
            {
                FileIn.Close();                                     // 关闭输入流
                MessageBox.Show("文件:\n\n" + fileIn + "\n\n符合大小要求，无需再拆分！");
                return 1;
            }

            byte[] ReadBuf = new byte[1024];    // 流读取,缓存空间
            UInt32 FileSize = 0;                // 记录子文件累积读取的大小，单位：KB
            UInt32 FileIndex = 0;               // 子文件序号

            string fileName = System.IO.Path.GetFileName(fileIn);   // 文件名
            string extension = System.IO.Path.GetExtension(fileIn); // 扩展名 
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileIn);// 没有扩展名的文件名
            string directory = System.IO.Path.GetDirectoryName(fileIn); // 路径

            FileStream FileOut = null;          //输出流
            int ReadLen = 0;                    //每次实际读取的字节大小
            while (ReadLen > 0 || (ReadLen = FileIn.Read(ReadBuf, 0, ReadBuf.Length)) > 0) //读取数据
            {
                //创建分割后的子文件，已有则覆盖
                if (FileSize == 0)
                {
                    FileOut = new FileStream(directory + "\\" + fileNameWithoutExtension + "_" + ++FileIndex + extension, FileMode.Create);
                }

                //输出，缓存数据写入子文件
                FileOut.Write(ReadBuf, 0, ReadLen);
                FileOut.Flush();

                //预读下一轮缓存数据
                ReadLen = FileIn.Read(ReadBuf, 0, ReadBuf.Length);
                if (++FileSize >= MaxLenKB || ReadLen == 0)     // 子文件达到指定大小，或文件已读完
                {
                    FileOut.Close();                            // 关闭当前输出流
                    FileSize = 0;
                }
            }

            FileIn.Close();                                     // 关闭输入流

            if (delete == true)
            {
                System.IO.File.Delete(fileIn);                  // 删除源文件
            }

            MessageBox.Show("文件:\n\n" + fileIn + "\n\n拆分完成！");

            return 0;
        }

        private void btn_Split_Click(object sender, RoutedEventArgs e)
        {
            //System.IO.FileInfo fileInfo = null;
            //StreamWriter streamWriter = null;

            try
            {
                fileSplit(tbx_File.Text, 20 * 1024, false);
            }
            catch (Exception ex)
            {
                tbx_Status.Text = ex.Message;
            }
        }
    }
}
