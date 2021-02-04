using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using YyWsnDeviceLibrary;
using YyWsnCommunicatonLibrary;

using System.Windows;
using System.Windows.Input;

namespace PictureTransfer
{
    public partial class FormMain : Form
    {
        BinaryFile bFile;
        SerialPortHelper Serial;

        public FormMain()
        {
            InitializeComponent();
        }

        public void Serial_Init()
        {
            string ComportName = SerialPortHelper.GetSerialPortName(cbCOMPort.SelectedItem.ToString());

            int BaudRate = 115200;

            if (tbxBaudRate.Text != string.Empty && tbxBaudRate.Text.Length != 0)
            {
                try
                {
                    BaudRate = Convert.ToInt32(tbxBaudRate.Text);
                }catch(Exception)
                {
                    MessageBox.Show("串口波特率错误！");
                }                
            }            

            Serial = new SerialPortHelper();
            Serial.IsLogger = true;
            Serial.InitCOM(ComportName, BaudRate);
            Serial.ReceiveDelayMs = 2;
            Serial.OpenPort();
        }

        public void Serial_Close()
        {
            if (Serial != null)
            {
                Serial.ClosePort();
            }
        }

        private void ComportRefersh()
        {
            string[] serials = SerialPortHelper.GetSerialPorts();

            cbCOMPort.Items.Clear();

            foreach (string item in serials)
            {
                cbCOMPort.Items.Add(item);
            }
            if (cbCOMPort.Items.Count > 0)
            {
                cbCOMPort.SelectedIndex = 0;
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            ComportRefersh();            
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ComportRefersh();
        }

        /// <summary>
        /// 获取文件名称中的扩展名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string GetExtension(string fileName)
        {
            // 拆分字符串
            string[] Sections = fileName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (Sections == null || Sections.Length == 0)
            {
                return null;
            }

            return Sections[Sections.Length - 1];
        }

        private byte[] Content { get; set; }
        const int FileSize = 0x20000;
        UInt32 StartAddr = 0;
        string SafeFileName = null;
        bool IsTxtImage = false;                // 是不是烧录的镜像文件

        private int CreateImageBuf(string filePath, int fileSize)
        {            
            // 构造全0x00的字节数组
            Content = new byte[fileSize];
            for (int iX = 0; iX < Content.Length; iX++)
            {
                Content[iX] = 0x00;
            }

            // 读取烧录文件的内容
            StreamReader FileReader = new StreamReader(filePath, Encoding.ASCII);
            string fileTxt = FileReader.ReadToEnd();
            FileReader.Close();
            FileReader = null;

            // 拆分字符串
            string[] Sections = fileTxt.Split(new char[] { '@', 'q' }, StringSplitOptions.RemoveEmptyEntries);
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

                if(iX == 0)
                {
                    StartAddr = Addr;
                }

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

                    Sum = (UInt32)(Sum + ByteBuf.Length);
                }

            }

            return 0;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            IsTxtImage = false;

            //open file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "所有文件|*.*|JPG文件|*.jpg|PNG文件|*.png|ICO文件|*.ico";
            openFileDialog.RestoreDirectory = false;
            openFileDialog.FilterIndex = 0;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtOpenFile.Text = openFileDialog.SafeFileName;

                string extension = GetExtension(txtOpenFile.Text);

                if (extension.Equals("txt") == true)
                {
                    IsTxtImage = true;
                    SafeFileName = openFileDialog.SafeFileName;
                    CreateImageBuf(openFileDialog.FileName, FileSize);
                }
                else
                {
                    // 显示图片
                    pic1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pic1.Load(openFileDialog.FileName);                    
                }

                // 获取文件的字节流
                bFile = new BinaryFile();
                bFile.SafeFileName = openFileDialog.SafeFileName;
                bFile.PacketSize = Convert.ToInt32(numericUpDown1.Value);

                bFile.Open(openFileDialog.FileName);
                lbFileSize.Text = "File Size: " + bFile.FileSize.ToString();
                lbPacketCount.Text = "Packet Count: " + bFile.PacketCount;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime startDatetime = DateTime.Now;
                DateTime endDatetime;
                btnSend.Enabled = false;
                lbStatus.Text = "Status: Working;";
                lbStatus.Refresh();

                if (bFile != null)
                {
                    if (IsTxtImage == true)
                    {   // 烧录          
                        TransferPicture(Content, (int)StartAddr);
                    }
                    else
                    {   // 传输图片
                        TransferPicture(bFile.Content, 0);
                    }                    

                    endDatetime = DateTime.Now;
                    TimeSpan ts1 = new TimeSpan(startDatetime.Ticks);
                    TimeSpan ts2 = new TimeSpan(endDatetime.Ticks);
                    TimeSpan ts = ts1.Subtract(ts2).Duration();
                    double msec = ts.TotalSeconds;
                    lbTime.Text = "Time Span(s): " + msec.ToString("0.00");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnSend.Enabled = true;
                lbStatus.Text = "Status: Idle";
            }
        }

        public int SendApply()
        {          
            if(bFile.SafeFileName == string.Empty)
            {
                return -1;
            }

            byte[] NameBuf = Encoding.ASCII.GetBytes(bFile.SafeFileName);
            if(NameBuf == null || NameBuf.Length == 0)
            {
                return -2;
            }

            if(NameBuf.Length > 32)
            {
                return -3;
            }

            byte Len = (byte) (45 + 8 + 1 + NameBuf.Length);

            byte[] TxBuf = new byte[4 + Len];
            UInt16 TxLen = 0;

            TxBuf[TxLen++] = Len;
            TxBuf[TxLen++] = 0xCA;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x0A;

            // 设备类型
            TxBuf[TxLen++] = (byte)'C';
            TxBuf[TxLen++] = (byte)'A';
            TxBuf[TxLen++] = (byte)'N';
            TxBuf[TxLen++] = (byte)'G';
            TxBuf[TxLen++] = (byte)'L';
            TxBuf[TxLen++] = (byte)'U';
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 保护区编码
            TxBuf[TxLen++] = (byte)'W';
            TxBuf[TxLen++] = (byte)'i';
            TxBuf[TxLen++] = (byte)'l';
            TxBuf[TxLen++] = (byte)'d';
            TxBuf[TxLen++] = (byte)'P';
            TxBuf[TxLen++] = (byte)'r';
            TxBuf[TxLen++] = (byte)'3';
            TxBuf[TxLen++] = (byte)'9';

            // 设备编码
            TxBuf[TxLen++] = (byte)'B';
            TxBuf[TxLen++] = (byte)'M';
            TxBuf[TxLen++] = (byte)'0';
            TxBuf[TxLen++] = (byte)'5';
            TxBuf[TxLen++] = (byte)'0';
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 触发方式
            TxBuf[TxLen++] = (byte)'T';

            // 拍摄时间
            DateTime ThisCalendar = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            byte[] ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 温度
            TxBuf[TxLen++] = 0x0A;
            TxBuf[TxLen++] = 0x13;

            // 经纬度
            TxBuf[TxLen++] = 0x05;
            TxBuf[TxLen++] = 0xE2;
            TxBuf[TxLen++] = 0x3B;
            TxBuf[TxLen++] = 0x60;
            TxBuf[TxLen++] = 0x01;
            TxBuf[TxLen++] = 0xB8;
            TxBuf[TxLen++] = 0xB3;
            TxBuf[TxLen++] = 0xDA;

            // 高度
            TxBuf[TxLen++] = 0x0F;
            TxBuf[TxLen++] = 0xD4;

            // 电量
            TxBuf[TxLen++] = 0x16;
            TxBuf[TxLen++] = 0x89;

            // 文件标志码
            TxBuf[TxLen++] = 0x12;
            TxBuf[TxLen++] = 0x34;
            TxBuf[TxLen++] = 0x56;
            TxBuf[TxLen++] = 0x78;

            // 文件大小
            TxBuf[TxLen++] = (byte)((bFile.FileSize & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((bFile.FileSize & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((bFile.FileSize & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((bFile.FileSize & 0x000000FF) >> 0);

            // 文件名称
            TxBuf[TxLen++] = (byte) NameBuf.Length;

            for (int iX = 0; iX < NameBuf.Length; iX++)
            {
                TxBuf[TxLen++] = NameBuf[iX];
            }

            // 计算校验和
            TxBuf[2] = MyCustomFxn.CheckSum8(0, TxBuf, 3, (UInt16)(Len + 1));

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 1000);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if(error < 0)
            {
                return -2;
            }

            return 0;
        }

        public int SendApplyTxt()
        {
            if (SafeFileName == string.Empty)
            {
                return -1;
            }

            byte[] NameBuf = Encoding.ASCII.GetBytes(SafeFileName);
            if (NameBuf == null || NameBuf.Length == 0)
            {
                return -2;
            }

            if (NameBuf.Length > 32)
            {
                return -3;
            }

            byte Len = (byte)(45 + 8 + 1 + NameBuf.Length);

            byte[] TxBuf = new byte[4 + Len];
            UInt16 TxLen = 0;

            TxBuf[TxLen++] = Len;
            TxBuf[TxLen++] = 0xCA;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x0A;

            // 设备类型
            TxBuf[TxLen++] = (byte)'C';
            TxBuf[TxLen++] = (byte)'A';
            TxBuf[TxLen++] = (byte)'N';
            TxBuf[TxLen++] = (byte)'G';
            TxBuf[TxLen++] = (byte)'L';
            TxBuf[TxLen++] = (byte)'U';
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 保护区编码
            TxBuf[TxLen++] = (byte)'W';
            TxBuf[TxLen++] = (byte)'i';
            TxBuf[TxLen++] = (byte)'l';
            TxBuf[TxLen++] = (byte)'d';
            TxBuf[TxLen++] = (byte)'P';
            TxBuf[TxLen++] = (byte)'r';
            TxBuf[TxLen++] = (byte)'3';
            TxBuf[TxLen++] = (byte)'9';

            // 设备编码
            TxBuf[TxLen++] = (byte)'B';
            TxBuf[TxLen++] = (byte)'M';
            TxBuf[TxLen++] = (byte)'0';
            TxBuf[TxLen++] = (byte)'5';
            TxBuf[TxLen++] = (byte)'0';
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 触发方式
            TxBuf[TxLen++] = (byte)'T';

            // 拍摄时间
            DateTime ThisCalendar = Convert.ToDateTime(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            byte[] ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
            TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

            // 温度
            TxBuf[TxLen++] = 0x0A;
            TxBuf[TxLen++] = 0x13;

            // 经纬度
            TxBuf[TxLen++] = 0x05;
            TxBuf[TxLen++] = 0xE2;
            TxBuf[TxLen++] = 0x3B;
            TxBuf[TxLen++] = 0x60;
            TxBuf[TxLen++] = 0x01;
            TxBuf[TxLen++] = 0xB8;
            TxBuf[TxLen++] = 0xB3;
            TxBuf[TxLen++] = 0xDA;

            // 高度
            TxBuf[TxLen++] = 0x0F;
            TxBuf[TxLen++] = 0xD4;

            // 电量
            TxBuf[TxLen++] = 0x16;
            TxBuf[TxLen++] = 0x89;

            // 文件标志码
            TxBuf[TxLen++] = 0x12;
            TxBuf[TxLen++] = 0x34;
            TxBuf[TxLen++] = 0x56;
            TxBuf[TxLen++] = 0x78;

            // 文件大小
            UInt32 fileSize = (UInt32)(Content.Length - StartAddr);
            TxBuf[TxLen++] = (byte)((fileSize & 0xFF000000) >> 24);
            TxBuf[TxLen++] = (byte)((fileSize & 0x00FF0000) >> 16);
            TxBuf[TxLen++] = (byte)((fileSize & 0x0000FF00) >> 8);
            TxBuf[TxLen++] = (byte)((fileSize & 0x000000FF) >> 0);

            // 文件名称
            TxBuf[TxLen++] = (byte)NameBuf.Length;

            for (int iX = 0; iX < NameBuf.Length; iX++)
            {
                TxBuf[TxLen++] = NameBuf[iX];
            }

            // 计算校验和
            TxBuf[2] = MyCustomFxn.CheckSum8(0, TxBuf, 3, (UInt16)(Len + 1));

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 1000);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        /// <summary>
        /// 申请传输文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnApply_Click(object sender, EventArgs e)
        {
            if (bFile == null)
            {
                return;
            }

            Serial_Init();

            if (IsTxtImage == true)
            {
                SendApplyTxt();
            }
            else
            {
                SendApply();
            }

            Serial_Close();
        }

        /// <summary>
        /// 发送图片的一帧数据
        /// </summary>
        /// <param name="Buf"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="Len"></param>
        /// <returns></returns>
        public int SendFrameOfPicture(byte[] Buf, int IndexOfStart, byte Len)
        {
            byte[] TxBuf = new byte[4 + Len];

            TxBuf[0] = Len;
            TxBuf[1] = 0xCA;
            TxBuf[2] = 0x00;
            TxBuf[3] = 0x0B;

            for(int iX = 0; iX < Len; iX++)
            {
                TxBuf[4 + iX] = Buf[IndexOfStart + iX];
            }

            // 计算校验和
            TxBuf[2] = MyCustomFxn.CheckSum8(0, TxBuf, 3, (UInt16)(Len + 1));

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 200);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if(error < 0)
            {
                return -2;
            }

            return 0;
        }


        int FailCnt = 0;

        public int TransferPicture(byte[] Buf, int startOfIndex)
        {
            if(startOfIndex > Buf.Length)
            {
                return -1;
            }

            if(startOfIndex == Buf.Length)
            {
                return 1;
            }

            int totalLength = Buf.Length - startOfIndex;            // 文件的总大小
            int offset = 0;                                         // 已发送成功的大小

            int frames = 0;                                         // 帧

            Serial_Init();

            FailCnt = 0;

            int iX = 0;
            labError.Text = "Error: ";

            const byte TxUnitLen = 255;                     // 一帧数据中最多可以有255个字节的有效数据
            byte TxLen = 0;

            while (offset < totalLength)
            {
                if (totalLength - offset >= TxUnitLen)
                {
                    //需要传输的字节数量为一个满编的PacketLength,用于非最后一次传输
                    TxLen = TxUnitLen;
                }
                else
                {
                    //需要传输的字节数量不满一个PacketLength,基本上就是最后一次传输
                    TxLen = (byte)(totalLength - offset);
                }

                // 开始传输
                if (SendFrameOfPicture(Buf, startOfIndex + offset, TxLen) < 0)
                {
                    FailCnt++;

                    labError.Text += "  " + iX.ToString("G");
                }

                iX++;

                offset += TxLen;
                frames++;
            }

            Serial_Close();

            return frames;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Serial_Init();

            byte[] TxBuf = new byte[14];
            TxBuf[0] = (byte)'H';
            TxBuf[1] = (byte)'y';
            TxBuf[2] = (byte)'p';
            TxBuf[3] = (byte)'e';
            TxBuf[4] = (byte)'r';
            TxBuf[5] = (byte)'W';
            TxBuf[6] = (byte)'S';
            TxBuf[7] = (byte)'N';
            TxBuf[8] = (byte)'_';
            TxBuf[9] = (byte)'D';
            TxBuf[10] = (byte)'e';
            TxBuf[11] = (byte)'b';
            TxBuf[12] = (byte)'u';
            TxBuf[13] = (byte)'g';

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, 14, 100);
            if (RxBuf == null)
            {

            }

            Serial_Close();
        }

        public int RxBuf_IsRight(byte[] RxBuf, byte[] TxBuf)
        {
            byte Len = RxBuf[0];

            if (Len != RxBuf.Length - 4)
            {
                return -1;
            }

            if (RxBuf[1] != 0xAC)
            {
                return -2;
            }

            byte sum = MyCustomFxn.CheckSum8(0, RxBuf, 3, (UInt16)(Len + 1));
            if (sum != RxBuf[2])
            {
                return -3;
            }

            if (RxBuf[3] != TxBuf[3])
            {
                return -4;
            }

            int error = 0;

            switch (RxBuf[3])
            {
                case 0x00:          // Ping
                    {
                        error = RxBuf_Ping(RxBuf);
                        break;
                    }
                case 0x01:          // Sleep
                    {
                        error = RxBuf_Sleep(RxBuf);
                        break;
                    }
                case 0x0A:          // 申请传输
                    {
                        error = RxBuf_Apply(RxBuf);
                        break;
                    }
                case 0x0B:          // 传输
                    {
                        error = RxBuf_Transfer(RxBuf);
                        break;
                    }
                case 0x0C:          // 校验
                    {
                        error = RxBuf_Check(RxBuf);
                        break;
                    }
                case 0x0D:          // 取消
                    {
                        error = RxBuf_Cancel(RxBuf);
                        break;
                    }
                case 0x0F:
                    {
                        error = RxBuf_Status(RxBuf);
                        break;
                    }
                case 0xFA:          // 格式化
                    {
                        error = RxBuf_Format(RxBuf);
                        break;
                    }
                default:
                    {
                        error = -5;
                        break;
                    }
            }

            if(error < 0)
            {
                error = error - 40;
            }

            return error;
        }

        public int RxBuf_Ping(byte[] RxBuf)
        {
            ExeStatus.Text = "Status: 00 0";

            return 0;
        }

        public int RxBuf_Sleep(byte[] RxBuf)
        {
            ExeStatus.Text = "Status: 01 0";

            return 0;
        }

        public int RxBuf_Apply(byte[] RxBuf)
        {
            if (RxBuf[0] < 1)
            {
                return -1;
            }

            Int16 error = RxBuf[4];
            if (error >= 0x80)
            {
                error -= 0x0100;
            }

            if (error < 0 || RxBuf[0] < 5)
            {
                ExeStatus.Text = "Status: 0A " + error.ToString();
            }
            else
            {
                UInt32 Addr = ((UInt32)RxBuf[5] << 24) | ((UInt32)RxBuf[6] << 16) | ((UInt32)RxBuf[7] << 8) | ((UInt32)RxBuf[8] << 0);
                ExeStatus.Text = "Status: 0A " + error.ToString() + "  " + Addr.ToString("X8");
            }
            

            if (error < 0)
            {
                return -2;              // 传输失败
            }

            return 0;
        }

        public int RxBuf_Transfer(byte[] RxBuf)
        {
            if (RxBuf[0] < 1)
            {
                return -1;
            }

            Int16 error = RxBuf[4];
            if (error >= 0x80)
            {
                error -= 0x0100;
            }

            ExeStatus.Text = "Status: 0B " + error.ToString();

            if (error < 0)
            {
                return -2;              // 传输失败
            }

            return 0;
        }

        public int RxBuf_Check(byte[] RxBuf)
        {
            if (RxBuf[0] < 1)
            {
                return -1;
            }

            Int16 error = RxBuf[4];
            if (error >= 0x80)
            {
                error -= 0x0100;
            }

            ExeStatus.Text = "Status: 0C " + error.ToString();

            if (error < 0)
            {
                return -2;              // 传输失败
            }

            return 0;
        }

        public int RxBuf_Cancel(byte[] RxBuf)
        {
            if (RxBuf[0] < 1)
            {
                return -1;
            }

            Int16 error = RxBuf[4];
            if (error >= 0x80)
            {
                error -= 0x0100;
            }

            ExeStatus.Text = "Status: 0D " + error.ToString();

            if (error < 0)
            {
                return -2;              // 传输失败
            }

            return 0;
        }

        public int RxBuf_Status(byte[] RxBuf)
        {
            if (RxBuf[0] < 1)
            {
                return -1;
            }

            Int16 error = RxBuf[4];
            if (error >= 0x80)
            {
                error -= 0x0100;
            }


            if (error < 0 || RxBuf[0] < 13)
            {
                ExeStatus.Text = "Status: 0F " + error.ToString();
            }
            else
            {
                UInt16 Total =  (UInt16)(((UInt16)RxBuf[5] << 8) | ((UInt16)RxBuf[6] << 0));
                UInt16 Num = (UInt16)(((UInt16)RxBuf[7] << 8) | ((UInt16)RxBuf[8] << 0));
                UInt16 WaitSend = (UInt16)(((UInt16)RxBuf[9] << 8) | ((UInt16)RxBuf[10] << 0));
                UInt16 Sent = (UInt16)(((UInt16)RxBuf[11] << 8) | ((UInt16)RxBuf[12] << 0));
                UInt16 Front = (UInt16)(((UInt16)RxBuf[13] << 8) | ((UInt16)RxBuf[14] << 0));
                UInt16 Rear = (UInt16)(((UInt16)RxBuf[15] << 8) | ((UInt16)RxBuf[16] << 0));
                ExeStatus.Text = "Status: 0F " + error.ToString() + "  总量：" + Total.ToString() + "  文件总数：" + Num.ToString() + "  等待发送：" + WaitSend.ToString() + "  已发送：" + Sent.ToString() + "  Front：" + Front.ToString() + "  Rear：" + Rear.ToString();
            }

            if (error < 0)
            {
                return -2;              // 传输失败
            }

            return 0;
        }

        public int RxBuf_Format(byte[] RxBuf)
        {
            if (RxBuf[0] < 1)
            {
                return -1;
            }

            Int16 error = RxBuf[4];
            if (error >= 0x80)
            {
                error -= 0x0100;
            }

            ExeStatus.Text = "Status: FA " + error.ToString();

            if (error < 0)
            {
                return -2;              // 传输失败
            }

            return 0;
        }

        public int SendPing()
        {
            byte[] TxBuf = new byte[4];
            TxBuf[0] = 0x00;
            TxBuf[1] = 0xCA;
            TxBuf[2] = 0x00;
            TxBuf[3] = 0x00;

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 200);

            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnPing_Click(object sender, EventArgs e)
        {
            Serial_Init();

            SendPing();

            Serial_Close();
        }

        public int SendSleep()
        {
            byte[] TxBuf = new byte[4];
            TxBuf[0] = 0x00;
            TxBuf[1] = 0xCA;
            TxBuf[2] = 0x01;
            TxBuf[3] = 0x01;

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 100);

            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnSleep_Click(object sender, EventArgs e)
        {
            Serial_Init();

            SendSleep();

            Serial_Close();
        }

        public int SendCheck()
        {
            byte[] TxBuf = new byte[6];
            TxBuf[0] = (byte)(TxBuf.Length - 4);
            TxBuf[1] = 0xCA;
            TxBuf[2] = 0x00;
            TxBuf[3] = 0x0C;

            UInt16 crc = 0;

            if(IsTxtImage == true)
            {
                crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, Content, StartAddr, (UInt32)(FileSize - StartAddr));
            }
            else
            {
                crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, bFile.Content, 0, (UInt32)bFile.Content.Length);
            }            

            TxBuf[4] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[5] = (byte)((crc & 0x00FF) >> 0);

            // 计算校验和
            TxBuf[2] = MyCustomFxn.CheckSum8(0, TxBuf, 3, (UInt16)(TxBuf.Length - 3));

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 600);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -2;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -3;
            }

            return 0;
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (bFile == null)
            {
                return;
            }

            Serial_Init();

            SendCheck();

            Serial_Close();
        }

        public int SendCancel()
        {
            byte[] TxBuf = new byte[4];
            TxBuf[0] = (byte)(TxBuf.Length - 4);
            TxBuf[1] = 0xCA;
            TxBuf[2] = 0x0D;
            TxBuf[3] = 0x0D;

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 100);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -2;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -3;
            }

            return 0;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Serial_Init();

            SendCancel();

            Serial_Close();
        }

        public int SendFormat()
        {
            byte[] TxBuf = new byte[8];
            TxBuf[0] = (byte)(TxBuf.Length - 4);
            TxBuf[1] = 0xCA;
            TxBuf[2] = 0x0D;
            TxBuf[3] = 0xFA;
            TxBuf[4] = 0xB2;
            TxBuf[5] = 0xC1;
            TxBuf[6] = 0xB3;
            TxBuf[7] = 0xFD;

            // 计算校验和
            TxBuf[2] = MyCustomFxn.CheckSum8(0, TxBuf, 3, (UInt16)(TxBuf.Length - 3));

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 2000);
            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -2;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -3;
            }

            return 0;
        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            Serial_Init();

            SendFormat();

            Serial_Close();
        }

        public int SendStatus()
        {
            byte[] TxBuf = new byte[4];
            TxBuf[0] = 0x00;
            TxBuf[1] = 0xCA;
            TxBuf[2] = 0x0F;
            TxBuf[3] = 0x0F;

            byte[] RxBuf = Serial.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 2000);

            if (RxBuf == null || RxBuf.Length == 0)
            {
                return -1;
            }

            int error = RxBuf_IsRight(RxBuf, TxBuf);
            if (error < 0)
            {
                return -2;
            }

            return 0;
        }

        private void btnStatus_Click(object sender, EventArgs e)
        {
            Serial_Init();

            SendStatus();

            Serial_Close();
        }




        /*************/
    }
}
