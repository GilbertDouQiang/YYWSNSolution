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
using System.Windows.Threading;
using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using ExcelExport;
using Microsoft.Win32;

namespace HyperWSN_Setup_GM
{
    enum ReadResult
    {
        Start = 0,              // 开始接收
        ReceiveOk = 1,          // 接收到合法数据
        ReceiveError = 2,       // 接收错误
        ReceiveNone = 3,        // 接收到，已读空
        ReceiveTimeout = 4,     // 接收超时
    }

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPortHelper SerialPort;

        ObservableCollection<M1> GridDataOfM1 = new ObservableCollection<M1>();     // M1的传感数据
        ObservableCollection<M20> GridDataOfM20 = new ObservableCollection<M20>();  // M20的传感数据
        ObservableCollection<AO2> GridDataOfAO2 = new ObservableCollection<AO2>();  // AO2的传感数据

        int GridLineOfM1 = 1;                                                       // M1表格的行编号  
        int GridLineOfM20 = 1;                                                      // M20表格的行编号   
        int GridLineOfAO2 = 1;                                                      // AO2表格的行编号         

        UInt16 ReadTotal = 0;           // 希望读取的数据的总数量
        UInt16 ReadCnt = 0;             // 读取数据的累计单元

        UInt16 BindTotal = 0;
        UInt32[] BindDevice = new UInt32[64];

        const byte UnitOfPage = 50;     // 一页中包含的绑定设备的数量
        byte IndexOfPage = 0;           // 页码
        byte TotalOfPage = 0;           // 总页数

        public MainWindow()
        {
            InitializeComponent();

            this.Title += " v" +
                System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            FindComport();

            GridOfM1.ItemsSource = GridDataOfM1;
            GridOfM20.ItemsSource = GridDataOfM20;
            GridOfAO2.ItemsSource = GridDataOfAO2;

            //M1 排序用
            ICollectionView v = CollectionViewSource.GetDefaultView(GridOfM1.ItemsSource);
            v.SortDescriptions.Clear();
            ListSortDirection d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //M20 排序用
            v = CollectionViewSource.GetDefaultView(GridOfM20.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();

            //AO2 排序用
            v = CollectionViewSource.GetDefaultView(GridOfAO2.ItemsSource);
            v.SortDescriptions.Clear();
            d = ListSortDirection.Descending;
            v.SortDescriptions.Add(new SortDescription("DisplayID", d));
            v.Refresh();
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
            //btnOpenComport.Content.ToString();
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

        /// <summary>
        /// 判断收到的数据包是否符合格式要求
        /// </summary>
        /// <param name="SrcBuf"></param>
        /// <param name="ExistRssi">true = 包含RSSI； false = 不包含RSSI；</param>
        /// <returns></returns>
        private Int16 RxPkt_IsRight(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 11)
            {
                return -1;
            }

            // 起始位
            if (SrcData[IndexOfStart + 0] != 0xAC || SrcData[IndexOfStart + 1] != 0xAC)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 2];
            if (pktLen + 7 > SrcLen)
            {
                return -3;
            }

            if (SrcData[IndexOfStart + 3 + pktLen + 2] != 0xCA || SrcData[IndexOfStart + 3 + pktLen + 3] != 0xCA)
            {
                return -4;
            }

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, SrcData, (UInt16)(IndexOfStart + 3), pktLen);
            UInt16 crc_chk = (UInt16)(SrcData[IndexOfStart + 3 + pktLen + 0] * 256 + SrcData[IndexOfStart + 3 + pktLen + 1]);
            if (crc_chk != crc && crc_chk != 0)
            {
                return -5;
            }

            return (Int16)(pktLen + 7);
        }

        /// <summary>
        /// 查询本地配置
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_ReadCfg(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 44)
            {
                return -1;
            }

            // 软件版本
            UInt16 SwRevision = (UInt16)(SrcData[IndexOfStart + 12] * 256 + SrcData[IndexOfStart + 13]);
            if(SwRevision >= 0xA41F)
            {
                ReadCfg(0x0E);
                return 1;
            }

            UInt16 iCnt = (UInt16)(IndexOfStart + 4);

            tbxMac.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2") + " " + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2");
            iCnt += 4;

            tbxHwRevision.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2") + " " + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2");
            iCnt += 4;

            tbxSwRevision.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxCustomer.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxDebug.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxCategory.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxInterval.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxCalendar.Text = "20" + SrcData[iCnt].ToString("X2") + "-" + SrcData[iCnt + 1].ToString("X2") + "-" + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2") + ":" + SrcData[iCnt + 4].ToString("X2") + ":" + SrcData[iCnt + 5].ToString("X2");
            iCnt += 6;

            tbxPattern.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxBps.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxChannel.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            tbxTxPower.Text = 15.ToString("D");
            iCnt += 0;

            tbxRam.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            tbxFront.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxRear.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxQueueLen.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxSendOk.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            double Volt = (double)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]) / 1000.0f;
            tbxVolt.Text = Volt.ToString("F3");
            iCnt += 2;

            return 0;
        }

        /// <summary>
        /// 查询本地配置
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_ReadCfgV2(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 50)
            {
                return -1;
            }

            UInt16 iCnt = (UInt16)(IndexOfStart + 4);

            tbxMac.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2") + " " + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2");
            iCnt += 4;

            tbxHwRevision.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2") + " " + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2");
            iCnt += 4;

            tbxSwRevision.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxCustomer.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxDebug.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxCategory.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxInterval.Text = (SrcData[iCnt] * 256 + SrcData[iCnt+1]).ToString("D");
            iCnt += 2;

            tbxCalendar.Text = "20" + SrcData[iCnt].ToString("X2") + "-" + SrcData[iCnt + 1].ToString("X2") + "-" + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2") + ":" + SrcData[iCnt + 4].ToString("X2") + ":" + SrcData[iCnt + 5].ToString("X2");
            iCnt += 6;

            tbxPattern.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxBps.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxChannel.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            Int16 txPower = SrcData[iCnt];
            if(txPower >= 128)
            {
                txPower -= 256;
            }
            tbxTxPower.Text = txPower.ToString("D");
            iCnt += 1;

            tbxTransPolicy.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            tbxReserved.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxRam.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            tbxFront.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxRear.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxQueueLen.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxSendOk.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            double Volt = (double)(SrcData[iCnt] * 256 + SrcData[iCnt + 1]) / 1000.0f;
            tbxVolt.Text = Volt.ToString("F3");
            iCnt += 2;

            tbxBindNum.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            return 0;
        }

        /// <summary>
        /// 修改本地配置，命令位是0x02
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_WriteCfgV1(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 25)
            {
                return -1;
            }

            tbxMac.Text = SrcData[IndexOfStart + 4].ToString("X2") + " " + SrcData[IndexOfStart + 5].ToString("X2") + " " + SrcData[IndexOfStart + 6].ToString("X2") + " " + SrcData[IndexOfStart + 7].ToString("X2");
            tbxHwRevision.Text = SrcData[IndexOfStart + 8].ToString("X2") + " " + SrcData[IndexOfStart + 9].ToString("X2") + " " + SrcData[IndexOfStart + 10].ToString("X2") + " " + SrcData[IndexOfStart + 11].ToString("X2");
            tbxCustomer.Text = SrcData[IndexOfStart + 12].ToString("X2") + " " + SrcData[IndexOfStart + 13].ToString("X2");
            tbxDebug.Text = SrcData[IndexOfStart + 14].ToString("X2") + " " + SrcData[IndexOfStart + 15].ToString("X2");
            tbxCategory.Text = SrcData[IndexOfStart + 16].ToString("X2");
            tbxInterval.Text = (SrcData[IndexOfStart + 17] * 256 + SrcData[IndexOfStart + 18]).ToString("D");
            tbxPattern.Text = SrcData[IndexOfStart + 19].ToString("X2");
            tbxBps.Text = SrcData[IndexOfStart + 20].ToString("X2");

            return 0;
        }

        /// <summary>
        /// 修改本地配置，命令位是0x84
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_WriteCfgV2(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 30)
            {
                return -1;
            }

            UInt16 iCnt = (UInt16)(IndexOfStart + 4);

            tbxMac.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2") + " " + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2");
            iCnt += 4;

            tbxHwRevision.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2") + " " + SrcData[iCnt + 2].ToString("X2") + " " + SrcData[iCnt + 3].ToString("X2");
            iCnt += 4;

            tbxCustomer.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxDebug.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            tbxCategory.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxInterval.Text = (SrcData[iCnt] * 256 + SrcData[iCnt + 1]).ToString("D");
            iCnt += 2;

            tbxPattern.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxBps.Text = SrcData[iCnt].ToString("X2");
            iCnt += 1;

            tbxChannel.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            Int16 txPower = SrcData[iCnt];
            if (txPower >= 128)
            {
                txPower -= 256;
            }
            tbxTxPower.Text = txPower.ToString("D");
            iCnt += 1;

            tbxTransPolicy.Text = SrcData[iCnt].ToString("D");
            iCnt += 1;

            tbxReserved.Text = SrcData[iCnt].ToString("X2") + " " + SrcData[iCnt + 1].ToString("X2");
            iCnt += 2;

            ReadCfg(0x01);

            return 0;
        }

        /// <summary>
        /// 删除已存数据
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_DeleteHistory(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 18)
            {
                return -1;
            }

            tbxMac.Text = SrcData[IndexOfStart + 4].ToString("X2") + " " + SrcData[IndexOfStart + 5].ToString("X2") + " " + SrcData[IndexOfStart + 6].ToString("X2") + " " + SrcData[IndexOfStart + 7].ToString("X2");
            tbxFront.Text = (SrcData[IndexOfStart + 8] * 256 + SrcData[IndexOfStart + 9]).ToString("D");
            tbxRear.Text = (SrcData[IndexOfStart + 10] * 256 + SrcData[IndexOfStart + 11]).ToString("D");
            tbxQueueLen.Text = (SrcData[IndexOfStart + 12] * 256 + SrcData[IndexOfStart + 13]).ToString("D");

            return 0;
        }

        /// <summary>
        /// 授时
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_Ntp(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 21)
            {
                return -1;
            }

            if (SrcData[IndexOfStart + 8] != 0x08 || SrcData[IndexOfStart + 9] != 0x11 | SrcData[IndexOfStart + 10] != 0x06)
            {
                return -2;
            }

            tbxNtpCalendar.Text = "20" + SrcData[IndexOfStart + 11].ToString("X2") + "-" + SrcData[IndexOfStart + 12].ToString("X2") + "-" + SrcData[IndexOfStart + 13].ToString("X2") + " " + SrcData[IndexOfStart + 14].ToString("X2") + ":" + SrcData[IndexOfStart + 15].ToString("X2") + ":" + SrcData[IndexOfStart + 16].ToString("X2");

            return 0;
        }

        /// <summary>
        /// 读取/设置开始时间
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_GetSetStart(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 20)
            {
                return -1;
            }

            // Error
            if (SrcData[IndexOfStart + 9] != 0x00)
            {
                tbxStartCalendar.Text = "";
                return -2;
            }
            else
            {
                tbxStartCalendar.Text = "20" + SrcData[IndexOfStart + 10].ToString("X2") + "-" + SrcData[IndexOfStart + 11].ToString("X2") + "-" + SrcData[IndexOfStart + 12].ToString("X2") + " " + SrcData[IndexOfStart + 13].ToString("X2") + ":" + SrcData[IndexOfStart + 14].ToString("X2") + ":" + SrcData[IndexOfStart + 15].ToString("X2");
                return 0;
            }
        }

        /// <summary>
        /// 读取一条Sensor数据，在处理反馈数据包时，判断数据包格式，并返回Sensor的设备类型
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_ReadDataFormat(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 63)
            {
                return -1;
            }

            // 数据包长度
            byte PktLen = SrcData[IndexOfStart + 2];

            // 负载长度
            byte PayLen = SrcData[IndexOfStart + 29];
            if (PayLen != PktLen - 32)
            {
                return -2;
            }

            // 判断数据类型
            if (SrcData[IndexOfStart + 30] != 0x61 || SrcData[IndexOfStart + 33] != 0x63 || SrcData[IndexOfStart + 35] != 0x64 || SrcData[IndexOfStart + 38] != 0x62 || SrcData[IndexOfStart + 45] != 0x67 || SrcData[IndexOfStart + 52] != 0x68)
            {
                return -3;
            }

            // Sensor的设备类型
            Device.DeviceType ThisDeviceType = (Device.DeviceType)SrcData[IndexOfStart + 27];

            switch (ThisDeviceType)
            {
                case Device.DeviceType.M1:
                case Device.DeviceType.S1P:
                case Device.DeviceType.S1:
                case Device.DeviceType.M1_NTC:
                case Device.DeviceType.M1_Beetech:
                case Device.DeviceType.M6:
                case Device.DeviceType.M2_PT100:
                case Device.DeviceType.M2_SHT30:
                case Device.DeviceType.S2:
                case Device.DeviceType.M30:
                    {
                        if (SrcLen < 69)
                        {
                            return -5;
                        }

                        if (SrcData[IndexOfStart + 54] != 0x65 || SrcData[IndexOfStart + 57] != 0x66)
                        {
                            return -6;
                        }

                        return (Int16)Device.DeviceType.M1;
                    }
                case Device.DeviceType.M2:
                    {
                        if (SrcLen < 66)
                        {
                            return -7;
                        }

                        if (SrcData[IndexOfStart + 54] != 0x65)
                        {
                            return -8;
                        }

                        return (Int16)Device.DeviceType.M2;
                    }
                case Device.DeviceType.M20:
                    {
                        if (SrcLen < 80)
                        {
                            return -7;
                        }

                        if (SrcData[IndexOfStart + 54] != 0x85)
                        {
                            return -8;
                        }

                        return (Int16)Device.DeviceType.M20;
                    }
                case Device.DeviceType.AO2:
                    {
                        if (SrcLen < 77)
                        {
                            return -7;
                        }

                        if (SrcData[IndexOfStart + 54] != 0x65 || SrcData[IndexOfStart + 57] != 0x66 || SrcData[IndexOfStart + 60] != 0x7D || SrcData[IndexOfStart + 64] != 0x7E)
                        {
                            return -8;
                        }

                        return (Int16)Device.DeviceType.AO2;
                    }
                default:
                    {
                        return -4;
                    }
            }
        }

        /// <summary>
        /// 读取一条Sensor数据
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_ReadData(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 13)
            {
                return -1;
            }

            // Error
            byte Error = SrcData[IndexOfStart + 8];
            if (Error == 0)
            {   // 返回一条Sensor的数据
                Int16 reValue = RxPkt_ReadDataFormat(SrcData, IndexOfStart);
                if (reValue < 0)
                {
                    return -3;
                }

                if ((Device.DeviceType)reValue == Device.DeviceType.M1 || (Device.DeviceType)reValue == Device.DeviceType.M2)
                {   // 创建一个M1的对象
                    M1 ThisM1 = new M1(SrcData, IndexOfStart, Device.DataPktType.SensorDataFromGmToPc, (Device.DeviceType)reValue);
                    if (ThisM1 == null)
                    {
                        return -5;
                    }

                    // 添加到显示列表中
                    ThisM1.DisplayID = GridLineOfM1;
                    if (++GridLineOfM1 == 0)
                    {
                        GridLineOfM1++;
                    }

                    GridDataOfM1.Add((M1)ThisM1);
                }
                else if ((Device.DeviceType)reValue == Device.DeviceType.M20)
                {   // 创建一个M20的对象
                    M20 ThisM20 = new M20(SrcData, IndexOfStart, Device.DataPktType.SensorDataFromGmToPc, (Device.DeviceType)reValue);
                    if (ThisM20 == null)
                    {
                        return -5;
                    }

                    // 添加到显示列表中
                    ThisM20.DisplayID = GridLineOfM20;
                    if (++GridLineOfM20 == 0)
                    {
                        GridLineOfM20++;
                    }

                    GridDataOfM20.Add(ThisM20);
                }
                else if ((Device.DeviceType)reValue == Device.DeviceType.AO2)
                {   // 创建一个M20的对象
                    AO2 ThisAO2 = new AO2(SrcData, IndexOfStart, Device.DataPktType.SensorDataFromGmToPc, (Device.DeviceType)reValue);
                    if (ThisAO2 == null)
                    {
                        return -5;
                    }

                    // 添加到显示列表中
                    ThisAO2.DisplayID = GridLineOfM20;
                    if (++GridLineOfM20 == 0)
                    {
                        GridLineOfM20++;
                    }

                    GridDataOfAO2.Add(ThisAO2);
                }

                // 读取下一条
                ReadCnt++;
                if (ReadCnt < ReadTotal)
                {
                    ReadOneData();
                }
                else
                {   // 结束
                    tbkReadResult.Text = "读取结束";
                }
            }
            else if (Error == 1)
            {   // 无数据
                tbkReadResult.Text = "已读空";
            }
            else
            {
                return -2;
            }

            return 0;
        }

        /// <summary>
        /// 查询绑定设备列表
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_ReadBind(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 17)
            {
                return -1;
            }

            byte Error = SrcData[IndexOfStart + 8];
            if (Error != 0)
            {
                return -2;
            }

            byte BindNum = SrcData[IndexOfStart + 9];
            if (BindNum > 64)
            {
                return -3;
            }

            // 总页数
            byte TotalPage = SrcData[IndexOfStart + 10];

            // 页码
            byte iPage = SrcData[IndexOfStart + 11];

            // 页内数量
            byte TotalInPage = SrcData[IndexOfStart + 12];
            if (TotalInPage * 4 + 10 != SrcData[IndexOfStart + 2])
            {
                return -7;
            }

            tbxBindNum.Text = BindNum.ToString("D");

            if (iPage == 0)
            {
                tbxBindDev.Text = "";
            }

            UInt16 IndexOfBind = (UInt16)(IndexOfStart + 13);

            for (byte iCnt = 0; iCnt < TotalInPage; iCnt++)
            {
                if (tbxBindDev.Text == "")
                {
                    tbxBindDev.Text += SrcData[IndexOfBind].ToString("X2") + SrcData[IndexOfBind + 1].ToString("X2") + SrcData[IndexOfBind + 2].ToString("X2") + SrcData[IndexOfBind + 3].ToString("X2");
                }else
                {
                    tbxBindDev.Text += ", " + SrcData[IndexOfBind].ToString("X2") + SrcData[IndexOfBind + 1].ToString("X2") + SrcData[IndexOfBind + 2].ToString("X2") + SrcData[IndexOfBind + 3].ToString("X2");
                }                

                IndexOfBind += 4;
            }

            return 0;
        }

        /// <summary>
        /// 保存绑定设备列表
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_SaveBind(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 14)
            {
                return -1;
            }

            byte Error = SrcData[IndexOfStart + 8];
            if (Error != 0)
            {
                return -2;
            }

            if(IndexOfPage < TotalOfPage)
            {
                SaveBind(IndexOfPage++, UnitOfPage);
                return 1;
            }

            ReadBind();         // 读取

            return 0;
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ConsoleLog(string direct, byte[] Txt, UInt16 IndexOfStart, UInt16 TxtLen)
        {
            if (Txt.Length != 0 && chkLockLog.IsChecked == false)
            {   
                // TODO: 2019-07-09 日志记录有问题
                
                tbxConsole.Text = Logger.GetTimeString() + "\t" + direct + "\t" + MyCustomFxn.ToHexString(Txt, IndexOfStart, TxtLen) + "\r\n" + tbxConsole.Text;               

                UInt16 ConsoleMaxLine = Convert.ToUInt16(txtLogLineLimit.Text);
                if (tbxConsole.LineCount > ConsoleMaxLine)
                {
                    int start = tbxConsole.GetCharacterIndexFromLineIndex(ConsoleMaxLine);  // 末尾行第一个字符的索引
                    int length = tbxConsole.GetLineLength(ConsoleMaxLine);                  // 末尾行字符串的长度
                    tbxConsole.Select(start, start + length);                               // 选中末尾一行
                    tbxConsole.SelectedText = "END";
                }                              
            }
        }

        /// <summary>
        /// 串口发送数据，并记录日志
        /// </summary>
        /// <param name="TxBuf"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="TxLen"></param>
        private void SerialPort_Send(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen)
        {
            // 显示Log
            ConsoleLog("TX", TxBuf, IndexOfStart, TxLen);

            // 发送数据
            SerialPort.Send(TxBuf, IndexOfStart, TxLen);
        }

        private void Comport_SerialPortReceived(object sender, SerialPortEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                //收到数据后的处理,临时用这个方法处理多线程问题,后续严格参考绑定
                //收到数据后的几种情况
                // 未收到任何反馈
                if (e.ReceivedBytes == null)
                {
                    return;
                }

                // 显示Log
                ConsoleLog("RX", e.ReceivedBytes, 0, (UInt16)e.ReceivedBytes.Length);

                UInt16 SrcLen = (UInt16)e.ReceivedBytes.Length;

                Int16 HandleLen = 0;
                Int16 ExeError = 0;

                for (UInt16 iCnt = 0; iCnt < SrcLen; iCnt++)
                {
                    try
                    {
                        HandleLen = RxPkt_IsRight(e.ReceivedBytes, iCnt);
                        if (HandleLen < 0)
                        {
                            continue;
                        }

                        switch (e.ReceivedBytes[iCnt + 3])
                        {
                            case 0x00:
                                {
                                    break;
                                }
                            case 0x01:
                                {
                                    ExeError = RxPkt_ReadCfg(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x02:
                                {
                                    ExeError = RxPkt_WriteCfgV1(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x03:
                                {
                                    ExeError = RxPkt_DeleteHistory(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x04:
                                {
                                    ExeError = RxPkt_Ntp(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x09:
                                {
                                    ExeError = RxPkt_GetSetStart(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x0A:
                                {
                                    ExeError = RxPkt_ReadData(e.ReceivedBytes, iCnt);                                    
                                    break;
                                }
                            case 0x0E:
                                {
                                    ExeError = RxPkt_ReadCfgV2(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x0F:
                                {
                                    ExeError = RxPkt_ReadBind(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x10:
                                {
                                    ExeError = RxPkt_SaveBind(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x84:
                                {
                                    ExeError = RxPkt_WriteCfgV2(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }

                        if (ExeError < 0)
                        {
                            continue;
                        }

                        if (HandleLen > 0)
                        {
                            HandleLen--;        // 因为马上就要执行iCnt++
                        }

                        iCnt = (UInt16)(iCnt + HandleLen);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("处理接收数据包错误" + ex.Message);
                    }
                }
            }));
        }    
        
        /// <summary>
        /// 发送串口指令，读取配置
        /// </summary>
        /// <param name="Cmd"></param>
        private void ReadCfg(byte Cmd)
        {
            byte[] TxBuf = new byte[14];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCA;
            TxBuf[TxLen++] = 0xCA;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = Cmd;

            // GW ID
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xAC;
            TxBuf[TxLen++] = 0xAC;

            // 重写长度位
            TxBuf[2] = (byte)(TxLen - 7);

            SerialPort_Send(TxBuf, 0, TxLen);
        }    

        /// <summary>
        /// 读取配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReadCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReadCfg(0x01);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 将左列的配置复制填充到右列里去
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopyCfg_Click(object sender, RoutedEventArgs e)
        {
            tbxMacNew.Text = tbxMac.Text;
            tbxHwRevisionNew.Text = tbxHwRevision.Text;
            tbxCustomerNew.Text = tbxCustomer.Text;
            tbxDebugNew.Text = tbxDebug.Text;
            tbxCategoryNew.Text = tbxCategory.Text;
            tbxIntervalNew.Text = tbxInterval.Text;
            tbxPatternNew.Text = tbxPattern.Text;
            tbxBpsNew.Text = tbxBps.Text;
            tbxChannelNew.Text = tbxChannel.Text;
            tbxTxPowerNew.Text = tbxTxPower.Text;
            tbxTransPolicyNew.Text = tbxTransPolicy.Text;
            tbxReservedNew.Text = tbxReserved.Text;
        }

        /// <summary>
        /// 将左列的配置清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearCfg_Click(object sender, RoutedEventArgs e)
        {
            tbxMac.Text = "";
            tbxHwRevision.Text = "";
            tbxSwRevision.Text = "";
            tbxCustomer.Text = "";
            tbxDebug.Text = "";
            tbxCategory.Text = "";
            tbxInterval.Text = "";
            tbxCalendar.Text = "";
            tbxPattern.Text = "";
            tbxBps.Text = "";
            tbxChannel.Text = "";
            tbxTxPower.Text = "";
            tbxRam.Text = "";
            tbxFront.Text = "";
            tbxRear.Text = "";
            tbxQueueLen.Text = "";
            tbxSendOk.Text = "";
            tbxVolt.Text = "";
            tbxTransPolicy.Text = "";
            tbxReserved.Text = "";
            tbxBindNum.Text = "";
        }

        string txStr = "";

        /// <summary>
        /// 修改配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWriteCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[36];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                // 读取软件版本
                byte SwRevisionH = 0;
                byte SwRevisionL = 0;
                byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxSwRevision.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                {
                    SwRevisionL = 0;
                }
                else
                {
                    SwRevisionH = ByteBufTmp[0];
                    SwRevisionL = ByteBufTmp[1];
                }
                // 若是软件版本>=0xA4 0x17，则使用0x84的修改指令，否则使用0x02的修改指令
                if (SwRevisionH == 0xA4 && SwRevisionL < 0x17)
                {
                    TxBuf[TxLen++] = 0x02;
                }
                else
                {
                    TxBuf[TxLen++] = 0x84;
                }

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // New GW ID
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxMacNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];

                // Hardware Revision
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxHwRevisionNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];

                // Customer
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCustomerNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];

                // Debug
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDebugNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];

                // Category
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCategoryNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 1)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];

                // Interval
                UInt16 Interval = Convert.ToUInt16(tbxIntervalNew.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Pattern
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxPatternNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 1)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];

                // Bps
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxBpsNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 1)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];

                // channel
                if (SwRevisionH == 0xA4 && SwRevisionL < 0x17)
                {
                    // 空
                }
                else
                {
                    TxBuf[TxLen++] = Convert.ToByte(tbxChannelNew.Text);
                }

                // TxPower and Reserved
                if (SwRevisionH == 0xA4 && SwRevisionL < 0x1F)
                {
                    if (SwRevisionL >= 0x1F)
                    {
                        TxBuf[TxLen++] = (byte)Convert.ToInt16(tbxTxPowerNew.Text);
                        TxBuf[TxLen++] = Convert.ToByte(tbxTransPolicyNew.Text);
                        ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxReservedNew.Text);
                        if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                        {
                            return;
                        }
                        TxBuf[TxLen++] = ByteBufTmp[0];
                        TxBuf[TxLen++] = ByteBufTmp[1];
                    }
                    else if (SwRevisionL >= 0x17)
                    {
                        TxBuf[TxLen++] = 0x00;
                        TxBuf[TxLen++] = 0x00;
                        TxBuf[TxLen++] = 0x00;
                        TxBuf[TxLen++] = 0x00;
                    }
                    else
                    {
                        // 空
                    }
                }else
                {
                    TxBuf[TxLen++] = (byte)Convert.ToInt16(tbxTxPowerNew.Text);
                    TxBuf[TxLen++] = Convert.ToByte(tbxTransPolicyNew.Text);
                    ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxReservedNew.Text);
                    if (ByteBufTmp == null || ByteBufTmp.Length < 2)
                    {
                        return;
                    }
                    TxBuf[TxLen++] = ByteBufTmp[0];
                    TxBuf[TxLen++] = ByteBufTmp[1];
                }
          
                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                txStr = MyCustomFxn.ToHexString(TxBuf, 0, (UInt16)TxBuf.Length);

                SerialPort_Send(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 删除已存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[18];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x03;

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // Front
                UInt16 Front = Convert.ToUInt16(tbxFrontNew.Text);
                TxBuf[TxLen++] = (byte)((Front & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Front & 0x00FF) >> 0);

                // Rear
                UInt16 Rear = Convert.ToUInt16(tbxRearNew.Text);
                TxBuf[TxLen++] = (byte)((Rear & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Rear & 0x00FF) >> 0);

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort_Send(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 授时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNtp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[24];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x04;

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // Payload length
                TxBuf[TxLen++] = 0x08;

                // 授时时间
                TxBuf[TxLen++] = 0x15;
                TxBuf[TxLen++] = 0x06;

                if (cbxAutoNtp.IsChecked == true)
                {
                    tbxNtpCalendar.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                DateTime NtpCalendar = Convert.ToDateTime(tbxNtpCalendar.Text);
                byte[] ByteBufTmp = MyCustomFxn.DataTimeToByteArray(NtpCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort_Send(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 查询开始时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[14];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x09;

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // Get/Set
                TxBuf[TxLen++] = 0x00;      // Get

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort_Send(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 设置开始时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[14];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCA;
                TxBuf[TxLen++] = 0xCA;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x09;

                // GW ID
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // Get/Set
                TxBuf[TxLen++] = 0x01;      // Set

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xAC;
                TxBuf[TxLen++] = 0xAC;

                // 重写长度位
                TxBuf[2] = (byte)(TxLen - 7);

                SerialPort_Send(TxBuf, 0, TxLen);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 读取一条数据
        /// </summary>
        private void ReadOneData()
        {
            byte[] TxBuf = new byte[18];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCA;
            TxBuf[TxLen++] = 0xCA;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0x0A;

            // GW ID
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Error
            TxBuf[TxLen++] = 0x00;

            // 确认序列号
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // ReadTimeoutMs
            // UInt16 ReadTimeoutMs = Convert.ToUInt16(tbxReadTimeoutMs.Text);
            UInt16 ReadTimeoutMs = 1000;
            TxBuf[TxLen++] = (byte)((ReadTimeoutMs & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((ReadTimeoutMs & 0x00FF) >> 0);

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xAC;
            TxBuf[TxLen++] = 0xAC;

            // 重写长度位
            TxBuf[2] = (byte)(TxLen - 7);

            SerialPort_Send(TxBuf, 0, TxLen);
        }

        /// <summary>
        /// 读取一条Sensor数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReadData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReadTotal = Convert.ToUInt16(tbxReadTotal.Text);
                ReadCnt = 0;

                tbkReadResult.Text = "读取中";

                ReadOneData();             
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportData_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Filter = "XLS文件|*.xls|所有文件|*.*";
            saveDlg.FileName = "Export_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xls";

            ExportXLS export = new ExportXLS();
            export.ExportWPFDataGrid(GridOfM1, saveDlg.FileName, GridDataOfM1);
        }

        /// <summary>
        /// 清空显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearData_Click(object sender, RoutedEventArgs e)
        {
            switch (tabControl.SelectedIndex)
            {
                case 2:
                    {
                        GridLineOfM1 = 1;
                        GridDataOfM1.Clear();
                        break;
                    }
                case 3:
                    {
                        GridLineOfM20 = 1;
                        GridDataOfM20.Clear();
                        break;
                    }
                case 4:
                    {
                        GridLineOfAO2 = 1;
                        GridDataOfAO2.Clear();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        /// <summary>
        /// 清空Console中的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            tbxConsole.Text = "";
        }

        /// <summary>
        /// 测试日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTestLog_Click(object sender, RoutedEventArgs e)
        {
            btnReadCfg_Click(sender, e);
        }

        /// <summary>
        /// 发送串口指令，读取绑定设备列表
        /// </summary>
        /// <param name="Cmd"></param>
        private void ReadBind()
        {
            byte[] TxBuf = new byte[18];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCA;
            TxBuf[TxLen++] = 0xCA;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0x0F;

            // GW ID
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // 保留
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xAC;
            TxBuf[TxLen++] = 0xAC;

            // 重写长度位
            TxBuf[2] = (byte)(TxLen - 7);

            SerialPort_Send(TxBuf, 0, TxLen);
        }

        /// <summary>
        /// 读取绑定设备列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReadBind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReadBind();
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 发送串口指令，保存绑定设备列表
        /// </summary>
        /// <param name="Cmd"></param>
        private void SaveBind(byte iPage, byte UnitOfPage)
        {
            byte[] TxBuf = new byte[240];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCA;
            TxBuf[TxLen++] = 0xCA;

            // Length
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0x10;

            // GW ID
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;
            TxBuf[TxLen++] = 0x00;

            // Cmd
            if(BindTotal == 0)
            {
                TxBuf[TxLen++] = 0x02;          // 删除所有
            }else
            {
                if (iPage == 0)
                {
                    TxBuf[TxLen++] = 0x03;      // 先删除所有，再添加
                }
                else
                {
                    TxBuf[TxLen++] = 0x00;      // 添加
                }
            }

            // 绑定数量
            byte TotalOfPage = (byte)(BindTotal / UnitOfPage + 1);
            byte TotalInPage = 0;
            if (iPage + 1 < TotalOfPage)
            {
                TotalInPage = UnitOfPage;
                TxBuf[TxLen++] = TotalInPage;
            }
            else
            {
                TotalInPage = (byte)(BindTotal % UnitOfPage);
                TxBuf[TxLen++] = TotalInPage;
            }   
            
            for(UInt16 iCnt = 0; iCnt < TotalInPage; iCnt++)
            {
                TxBuf[TxLen++] = (byte)((BindDevice[iPage * UnitOfPage + iCnt] & 0xFF000000) >> 24);
                TxBuf[TxLen++] = (byte)((BindDevice[iPage * UnitOfPage + iCnt] & 0x00FF0000) >> 16);
                TxBuf[TxLen++] = (byte)((BindDevice[iPage * UnitOfPage + iCnt] & 0x0000FF00) >> 8);
                TxBuf[TxLen++] = (byte)((BindDevice[iPage * UnitOfPage + iCnt] & 0x000000FF) >> 0);
            }  

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
            TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

            // End
            TxBuf[TxLen++] = 0xAC;
            TxBuf[TxLen++] = 0xAC;

            // 重写长度位
            TxBuf[2] = (byte)(TxLen - 7);

            SerialPort_Send(TxBuf, 0, TxLen);
        }

        /// <summary>
        /// 保存绑定设备列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveBind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Array.Clear(BindDevice, 0, BindDevice.Length);
                BindTotal = 0;

                // 按照逗号分隔符将字符串拆分为多个子字符串
                tbxBindDev.Text = tbxBindDev.Text.Replace(" ", "");
                tbxBindDev.Text = tbxBindDev.Text.Replace("\r", "");
                tbxBindDev.Text = tbxBindDev.Text.Replace("\n", "");
                tbxBindDev.Text = tbxBindDev.Text.Replace("\t", "");
                tbxBindDev.Text = tbxBindDev.Text.Replace("，", ",");
                string[] SID = tbxBindDev.Text.Split(',');

                foreach (string sid in SID)
                {
                    byte[] ExSensorIdByte = CommArithmetic.HexStringToByteArray(sid);
                    if (ExSensorIdByte != null && ExSensorIdByte.Length >= 4)
                    {
                        BindDevice[BindTotal++] = (UInt32)(ExSensorIdByte[0] * 256 * 256 * 256 + ExSensorIdByte[1] * 256 * 256 + ExSensorIdByte[2] * 256 + ExSensorIdByte[3]);
                    }
                }

                TotalOfPage = (byte)(BindTotal / UnitOfPage + 1);
                IndexOfPage = 0;

                SaveBind(IndexOfPage++, UnitOfPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show("参数错误" + ex.Message);
            }
        }

        /// <summary>
        /// 清除读取结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearBind_Click(object sender, RoutedEventArgs e)
        {
            tbxBindNum.Text = "";
            tbxBindDev.Text = "";
        }
    }
}
