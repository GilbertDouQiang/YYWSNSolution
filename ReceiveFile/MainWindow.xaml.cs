using System;
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
using System.IO;
using YyWsnDeviceLibrary;
using YyWsnCommunicatonLibrary;

namespace ReceiveFile
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper SerialPort;

        public MainWindow()
        {
            InitializeComponent();

            FindComport();
        }

        private void FindComport()
        {
            cbSerialPort.Items.Clear();
            string[] spis = SerialPortHelper.GetSerialPorts();
            foreach (var portname in spis)
            {
                cbSerialPort.Items.Add(portname);
            }
            if (cbSerialPort.Items.Count > 0)
            {
                cbSerialPort.SelectedIndex = 0;
            }
        }

        private void btnFindComport_Click(object sender, RoutedEventArgs e)
        {
            FindComport();
        }



        private void btnOpenComport_Click(object sender, RoutedEventArgs e)
        {
            if (btnOpenComport.Content.ToString() == "Open")
            {
                SerialPort = new SerialPortHelper();
                SerialPort.SerialPortReceived += Comport_SerialPortReceived;
                string portname = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());
                SerialPort.InitCOM(portname, 115200);
                if (SerialPort.OpenPort())
                {
                    btnOpenComport.Content = "Close";
                }
            }
            else
            {
                if (SerialPort != null)
                {
                    SerialPort.ClosePort();
                    btnOpenComport.Content = "Open";
                }
            }
        }

        UInt32 FileCode = 0;
        string FileName = string.Empty;
        byte[] FileBuf = null;
        UInt32 FileSizeSum = 0;                 // 已经传输的数量
        bool Checked = false;
        bool CheckOk = false;

        BinaryFile bFile;

        int SucEndIndex = 0;

        private void SaveFileAndDisplay()
        {
            string FilePath = Directory.GetCurrentDirectory() + "\\Log";

            if (Directory.Exists(FilePath) == false)  //工程目录下 Log目录 '目录是否存在,为true则没有此目录
            {
                Directory.CreateDirectory(FilePath); //建立目录　Directory为目录对象
            }

            string aFilePath = FilePath + "\\" + FileName;

            if (File.Exists(aFilePath) == true)
            {
                DateTime ct = DateTime.Now;
                if (FileName.Contains('.') == true)
                {   // 有扩展名       
                    int index = FileName.LastIndexOf('.');

                    FileName = FileName.Insert(index, "_" + ct.ToString("yyyyMMdd_HHmmss"));
                }
                else
                {   // 无扩展名
                    FileName += "_" + ct.ToString("yyyyMMdd_HHmmss");
                }
            }

            FilePath = FilePath + "\\" + FileName;

            bFile = new BinaryFile();
            bFile.Save(FilePath, FileBuf);

            // 显示图片
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(FilePath);
            bmp.EndInit();

            image.Source = bmp;

            tbxFileName.Text = FileName;
        }

        public int RxPkt_Handle(byte[] RxBuf, int IndexOfStart)
        {
            if (RxBuf.Length - IndexOfStart < 6)
            {
                return -1;
            }

            if (RxBuf[1 + IndexOfStart] != 0xAC)
            {
                return -2;
            }

            if (RxBuf[3 + IndexOfStart] != 0x0E)
            {
                return -3;
            }

            byte PktLen = RxBuf[0 + IndexOfStart];
            if (PktLen + 4 > RxBuf.Length - IndexOfStart)
            {
                return -4;
            }

            byte crc_chk = RxBuf[2 + IndexOfStart];
            byte crc = MyCustomFxn.CheckSum8(0, RxBuf, (UInt16)(3 + IndexOfStart), (UInt16)(PktLen + 1));
            if (crc != crc_chk)
            {
                return -5;
            }

            Int16 Error = (Int16)RxBuf[4 + IndexOfStart];
            if (Error >= 0x80)
            {
                Error -= 0x100;
            }

            if (Error < 0)
            {
                return -6;
            }

            byte Step = RxBuf[5 + IndexOfStart];

            if (Step == 1)
            {   // 文件头
                if (PktLen < 11)
                {
                    return -11;
                }

                FileCode = ((UInt32)RxBuf[6 + IndexOfStart] << 24) | ((UInt32)RxBuf[7 + IndexOfStart] << 16) | ((UInt32)RxBuf[8 + IndexOfStart] << 8) | ((UInt32)RxBuf[9 + IndexOfStart] << 0);
                UInt32 FileSize = ((UInt32)RxBuf[10 + IndexOfStart] << 24) | ((UInt32)RxBuf[11 + IndexOfStart] << 16) | ((UInt32)RxBuf[12 + IndexOfStart] << 8) | ((UInt32)RxBuf[13 + IndexOfStart] << 0);
                FileBuf = new byte[FileSize];

                byte FileNameLen = RxBuf[14 + IndexOfStart];
                if (FileNameLen > 32 || FileNameLen != PktLen - 11)
                {
                    return -7;
                }

                FileName = Encoding.Default.GetString(RxBuf, 15 + IndexOfStart, FileNameLen);
                FileSizeSum = 0;
                Checked = false;
                CheckOk = false;
            }
            else if (Step == 2)
            {   // 文件内容           
                byte FilePayLen = (byte)(PktLen - 2);
                int FilePayBufIndex = 6 + IndexOfStart;
                for (UInt16 iX = 0; iX < FilePayLen; iX++)
                {
                    FileBuf[FileSizeSum++] = RxBuf[FilePayBufIndex + iX];
                }
            }
            else if (Step == 3)
            {   // 校验
                if (PktLen < 4)
                {
                    return -13;
                }

                if (FileSizeSum < FileBuf.Length)
                {
                    return -14;
                }

                Checked = true;

                UInt16 RxFileCrc = (UInt16)(((UInt16)RxBuf[6 + IndexOfStart] << 8) | ((UInt16)RxBuf[7 + IndexOfStart] << 0));
                UInt16 MyFileCrc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, FileBuf, 0, (UInt32)FileBuf.Length);
                if (RxFileCrc != MyFileCrc)
                {
                    CheckOk = false;
                    return -15;
                }

                CheckOk = true;

                SaveFileAndDisplay();
            }
            else
            {
                return -7;
            }

            SucEndIndex = (int)(IndexOfStart + PktLen + 4);

            return 0;
        }

        byte[] PreRxBuf = null;
        byte[] CurRxBuf = null;

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定
                //收到数据后的几种情况
                // 未收到任何反馈
                if (e.ReceivedBytes == null || e.ReceivedBytes.Length == 0)
                {
                    return;
                }

                if (PreRxBuf != null && PreRxBuf.Length != 0)
                {
                    CurRxBuf = new byte[PreRxBuf.Length + e.ReceivedBytes.Length];
                    memcpy(CurRxBuf, 0, PreRxBuf, 0, PreRxBuf.Length);
                    memcpy(CurRxBuf, PreRxBuf.Length, e.ReceivedBytes, 0, e.ReceivedBytes.Length);
                }
                else
                {
                    CurRxBuf = new byte[e.ReceivedBytes.Length];
                    memcpy(CurRxBuf, 0, e.ReceivedBytes, 0, e.ReceivedBytes.Length);
                }

                int error = 0;

                for (int iX = 0; iX < CurRxBuf.Length;)
                {
                    error = RxPkt_Handle(CurRxBuf, iX);
                    if (error < 0)
                    {
                        iX++;
                    }
                    else
                    {
                        iX = SucEndIndex;
                    }
                }

                if(SucEndIndex < CurRxBuf.Length)
                {   // 末尾存在一个不完整的数据包
                    int RemainLen = CurRxBuf.Length - SucEndIndex;
                    PreRxBuf = new byte[RemainLen];
                    memcpy(PreRxBuf, 0, CurRxBuf, SucEndIndex, RemainLen);
                }
                else
                {
                    PreRxBuf = null;
                }

            }));
        }

        public void memcpy(byte[] DstBuf, int DstIndex, byte[] SrcBuf, int SrcIndex, int num)
        {
            for( int iX = 0; iX < num; iX++)
            {
                DstBuf[DstIndex + iX] = SrcBuf[SrcIndex + iX];
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "所有文件|*.*|JPG文件|*.jpg|PNG文件|*.png|ICO文件|*.ico";
            openFileDialog.RestoreDirectory = false;
            openFileDialog.FilterIndex = 0;
            if (openFileDialog.ShowDialog() == true)
            {
                tbxFileName.Text = openFileDialog.SafeFileName;

                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(openFileDialog.FileName);
                bmp.EndInit();

                image.Source = bmp;
            }
        }

        /*****/
    }
}
