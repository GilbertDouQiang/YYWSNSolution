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

namespace HyperWSN_Setup_M30
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

            this.Title += " v" +
           System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

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
            //btnOpenComport.Content.ToString();
            if (btnOpenComport.Content.ToString() == "Open")
            {
                SerialPort = new SerialPortHelper();
                SerialPort.SerialPortReceived += Comport_SerialPortReceived;
                string portname = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());
                SerialPort.InitCOM(portname);
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
            if (SrcData[IndexOfStart + 0] != 0xBC || SrcData[IndexOfStart + 1] != 0xBC)
            {
                return -2;
            }

            // 长度位
            byte pktLen = SrcData[IndexOfStart + 2];
            if (pktLen + 7 > SrcLen)
            {
                return -3;
            }

            if (SrcData[IndexOfStart + 3 + pktLen + 2] != 0xCB || SrcData[IndexOfStart + 3 + pktLen + 3] != 0xCB)
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
            if (SrcLen < 106)
            {
                return -1;
            }

            UInt16 iCnt = 6;

            // Protocol
            if (SrcData[IndexOfStart + 5] == 0x01)
            {
                tbxPrimaryMac.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxMac.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxHwRevision.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxSwRevision.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxSwRevision.Text += "  |  " + SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxSwRevision.Text += "  |  " + SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxCustomer.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxDebug.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxCategory.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                tbxPattern.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                tbxBps.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                tbxChannel.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                Int16 txPower = (Int16)SrcData[IndexOfStart + iCnt + 0];
                if (txPower >= 128)
                {
                    txPower = (Int16)(txPower - 256);
                }
                tbxTxPower.Text = txPower.ToString("D");
                iCnt += 1;

                tbxSampleInterval1.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxSampleInterval2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxSampleIntervalACO2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxTransferInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxHeatInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxCoolInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxCarouselInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxAlertInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxCalendar.Text = "20" + SrcData[IndexOfStart + iCnt + 0].ToString("X2") + "-" + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + "-" + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2") + ":" + SrcData[IndexOfStart + iCnt + 4].ToString("X2") + ":" + SrcData[IndexOfStart + iCnt + 5].ToString("X2");
                iCnt += 6;

                Int32 CompensateOfCO2 = (Int32)(SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]);
                tbxCO2Compensate.Text = CompensateOfCO2.ToString("D");
                iCnt += 3;

                tbxTimeSource.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxAlertWay.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxLcdPolicy.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxHeatLowThr.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxHeatHighThr.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxReservedOfCfg.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxResetSource.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                UInt16 voltMv = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                double Volt = (double)(voltMv & 0x7FFF) / 1000.0f;
                tbxVolt.Text = Volt.ToString("F3");
                if ((voltMv & 0x8000) == 0)
                {   // 未接充电器

                }
                else
                {   // 已接充电器
                    tbxVolt.Text += "+";
                }
                iCnt += 2;

                Int16 monTemp = SrcData[IndexOfStart + iCnt + 0];
                if (monTemp >= 0x80)
                {
                    monTemp = (Int16)(monTemp - 256);
                }
                tbxMonTemp.Text = monTemp.ToString("D");
                iCnt += 1;

                Int16 temp = (Int16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxTempOfCH1.Text = ((double)temp / 100.0f).ToString("F2");
                iCnt += 2;

                UInt16 hum = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHumOfCH1.Text = ((double)hum / 100.0f).ToString("F2");
                iCnt += 2;

                temp = (Int16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxTempOfCH2.Text = ((double)temp / 100.0f).ToString("F2");
                iCnt += 2;

                hum = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHumOfCH2.Text = ((double)hum / 100.0f).ToString("F2");
                iCnt += 2;

                temp = (Int16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxTempOfACO2.Text = ((double)temp / 100.0f).ToString("F2");
                iCnt += 2;

                hum = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHumOfACO2.Text = ((double)hum / 100.0f).ToString("F2");
                iCnt += 2;

                tbxCO2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxRamOfCH1.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxRamOfCH2.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxRamOfACO2.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxFlashOfCH1.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxFlashOfCH2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxFlashOfACO2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxReservedOfProtocol.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxStdCompOfCO2.Text = "";
            }
            else if (SrcData[IndexOfStart + 5] == 0x02)
            {
                SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
                if (SrcLen < 111)
                {
                    return -1;
                }

                tbxPrimaryMac.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxMac.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxHwRevision.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxSwRevision.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxSwRevision.Text += "  |  " + SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxSwRevision.Text += "  |  " + SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxCustomer.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxDebug.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2");
                iCnt += 2;

                tbxCategory.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                tbxPattern.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                tbxBps.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                tbxChannel.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                Int16 txPower = (Int16)SrcData[IndexOfStart + iCnt + 0];
                if (txPower >= 128)
                {
                    txPower = (Int16)(txPower - 256);
                }
                tbxTxPower.Text = txPower.ToString("D");
                iCnt += 1;

                tbxSampleInterval1.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxSampleInterval2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxSampleIntervalACO2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxTransferInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxHeatInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxCoolInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxCarouselInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxAlertInterval.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]).ToString("D");
                iCnt += 2;

                tbxCalendar.Text = "20" + SrcData[IndexOfStart + iCnt + 0].ToString("X2") + "-" + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + "-" + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2") + ":" + SrcData[IndexOfStart + iCnt + 4].ToString("X2") + ":" + SrcData[IndexOfStart + iCnt + 5].ToString("X2");
                iCnt += 6;

                Int32 CompensateOfCO2 = (Int32)(SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]);
                tbxCO2Compensate.Text = CompensateOfCO2.ToString("D");
                iCnt += 3;

                Int32 StdCompensateOfCO2 = (Int32)(SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]);
                tbxStdCompOfCO2.Text = StdCompensateOfCO2.ToString("D");
                iCnt += 3;

                tbxTimeSource.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxAlertWay.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxLcdPolicy.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                double HeatLowThr = (double)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1])/100.0f;
                tbxHeatLowThr.Text = HeatLowThr.ToString("F2");
                iCnt += 2;

                double HeatHighThr = (double)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]) / 100.0f;
                tbxHeatHighThr.Text = HeatHighThr.ToString("F2");
                iCnt += 2;

                tbxReservedOfCfg.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                tbxResetSource.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2");
                iCnt += 1;

                UInt16 voltMv = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                double Volt = (double)(voltMv & 0x7FFF) / 1000.0f;
                tbxVolt.Text = Volt.ToString("F3");
                if ((voltMv & 0x8000) == 0)
                {   // 未接充电器

                }
                else
                {   // 已接充电器
                    tbxVolt.Text += "+";
                }
                iCnt += 2;

                Int16 monTemp = SrcData[IndexOfStart + iCnt + 0];
                if (monTemp >= 0x80)
                {
                    monTemp = (Int16)(monTemp - 256);
                }
                tbxMonTemp.Text = monTemp.ToString("D");
                iCnt += 1;

                Int16 temp = (Int16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxTempOfCH1.Text = ((double)temp / 100.0f).ToString("F2");
                iCnt += 2;

                UInt16 hum = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHumOfCH1.Text = ((double)hum / 100.0f).ToString("F2");
                iCnt += 2;

                temp = (Int16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxTempOfCH2.Text = ((double)temp / 100.0f).ToString("F2");
                iCnt += 2;

                hum = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHumOfCH2.Text = ((double)hum / 100.0f).ToString("F2");
                iCnt += 2;

                temp = (Int16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxTempOfACO2.Text = ((double)temp / 100.0f).ToString("F2");
                iCnt += 2;

                hum = (UInt16)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHumOfACO2.Text = ((double)hum / 100.0f).ToString("F2");
                iCnt += 2;

                tbxCO2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxRamOfCH1.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxRamOfCH2.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxRamOfACO2.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxFlashOfCH1.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxFlashOfCH2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxFlashOfACO2.Text = (SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]).ToString("D");
                iCnt += 3;

                tbxReservedOfProtocol.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;
            }
            else
            {
                return -2;
            }

            return 0;
        }

        /// <summary>
        /// 修改出厂配置
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_SetFactoryCfg(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 18)
            {
                return -1;
            }

            // Protocol
            if (SrcData[IndexOfStart + 5] != 0x01)
            {
                return -2;
            }

            UInt16 iCnt = 6;

            tbxMac.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
            iCnt += 4;

            tbxHwRevision.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
            iCnt += 4;

            return 0;
        }

        /// <summary>
        /// 修改应用配置
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_SetAppCfg(byte[] SrcData, UInt16 IndexOfStart)
        {
            // 数据包的总长度
            UInt16 SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
            if (SrcLen < 19)
            {
                return -1;
            }

            // Protocol
            if (SrcData[IndexOfStart + 5] != 0x01)
            {
                return -2;
            }

            // Error
            if (SrcData[IndexOfStart + 10] != 0x00)
            {
                return -2;
            }

            // 重新读取配置
            ReadCfg();

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
            if (SrcLen < 25)
            {
                return -1;
            }

            // Protocol
            if (SrcData[IndexOfStart + 5] != 0x01)
            {
                return -2;
            }

            // Error
            if (SrcData[IndexOfStart + 11] != 0x00)
            {
                return -2;
            }

            // 重新读取配置
            ReadCfg();

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
            SerialPort.SendCommandByLength(TxBuf, IndexOfStart, TxLen);
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

                        switch (e.ReceivedBytes[iCnt + 4])
                        {
                            case 0x00:
                                {
                                    ExeError = RxPkt_ReadCfg(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x01:
                                {
                                    ExeError = RxPkt_SetFactoryCfg(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x02:
                                {
                                    ExeError = RxPkt_SetAppCfg(e.ReceivedBytes, iCnt);
                                    break;
                                }
                            case 0x03:
                                {
                                    ExeError = RxPkt_DeleteHistory(e.ReceivedBytes, iCnt);
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
        /// 组包，串口发送，读取配置
        /// </summary>
        private void ReadCfg()
        {
            byte[] TxBuf = new byte[16];
            UInt16 TxLen = 0;

            // Start
            TxBuf[TxLen++] = 0xCB;
            TxBuf[TxLen++] = 0xCB;

            // Length
            TxBuf[TxLen++] = 0x00;

            // 设备类型
            TxBuf[TxLen++] = 0x00;

            // Cmd
            TxBuf[TxLen++] = 0x00;

            // Protocol
            TxBuf[TxLen++] = 0x00;

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
            TxBuf[TxLen++] = 0xBC;
            TxBuf[TxLen++] = 0xBC;

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
                ReadCfg();
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
            tbxPatternNew.Text = tbxPattern.Text;
            tbxBpsNew.Text = tbxBps.Text;
            tbxChannelNew.Text = tbxChannel.Text;
            tbxTxPowerNew.Text = tbxTxPower.Text;
            tbxTransferIntervalNew.Text = tbxTransferInterval.Text;
            tbxSampleInterval1New.Text = tbxSampleInterval1.Text;
            tbxSampleInterval2New.Text = tbxSampleInterval2.Text;
            tbxSampleIntervalACO2New.Text = tbxSampleIntervalACO2.Text;
            tbxHeatIntervalNew.Text = tbxHeatInterval.Text;
            tbxCoolIntervalNew.Text = tbxCoolInterval.Text;
            tbxCarouselIntervalNew.Text = tbxCarouselInterval.Text;
            tbxAlertIntervalNew.Text = tbxAlertInterval.Text;
            tbxCO2CompensateNew.Text = tbxCO2Compensate.Text;
            tbxTimeSourceNew.Text = tbxTimeSource.Text;
            tbxAlertWayNew.Text = tbxAlertWay.Text;
            tbxLcdPolicyNew.Text = tbxLcdPolicy.Text;
            tbxHeatLowThrNew.Text = tbxHeatLowThr.Text;
            tbxHeatHighThrNew.Text = tbxHeatHighThr.Text;
            tbxStdCompOfCO2New.Text = tbxStdCompOfCO2.Text;
            tbxReservedOfCfgNew.Text = tbxReservedOfCfg.Text;
        }

        /// <summary>
        /// 将左列的配置清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearCfg_Click(object sender, RoutedEventArgs e)
        {
            tbxMac.Text = "";
            tbxSwRevision.Text = "";
            tbxHwRevision.Text = "";

            tbxCustomer.Text = "";
            tbxDebug.Text = "";
            tbxCategory.Text = "";
            tbxPattern.Text = "";
            tbxBps.Text = "";
            tbxChannel.Text = "";
            tbxTxPower.Text = "";
            tbxTransferInterval.Text = "";
            tbxSampleInterval1.Text = "";
            tbxSampleInterval2.Text = "";
            tbxSampleIntervalACO2.Text = "";
            tbxHeatInterval.Text = "";
            tbxCoolInterval.Text = "";
            tbxCarouselInterval.Text = "";
            tbxAlertInterval.Text = "";
            tbxCO2Compensate.Text = "";
            tbxTimeSource.Text = "";
            tbxAlertWay.Text = "";
            tbxLcdPolicy.Text = "";
            tbxHeatLowThr.Text = "";
            tbxHeatHighThr.Text = "";
            tbxStdCompOfCO2.Text = "";
            tbxReservedOfCfg.Text = "";

            tbxResetSource.Text = "";
            tbxVolt.Text = "";
            tbxMonTemp.Text = "";
            tbxTempOfCH1.Text = "";
            tbxHumOfCH1.Text = "";
            tbxTempOfCH2.Text = "";
            tbxHumOfCH2.Text = "";
            tbxTempOfACO2.Text = "";
            tbxHumOfACO2.Text = "";

            tbxRamOfCH1.Text = "";
            tbxRamOfCH2.Text = "";
            tbxRamOfACO2.Text = "";
            tbxFlashOfCH1.Text = "";
            tbxFlashOfCH2.Text = "";
            tbxFlashOfACO2.Text = "";
            tbxReservedOfProtocol.Text = "";          
        }

        /// <summary>
        /// 修改出厂配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetFactoryCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[20];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCB;
                TxBuf[TxLen++] = 0xCB;

                // Length
                TxBuf[TxLen++] = 0x00;

                // 设备类型
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x01;

                // Protocol
                TxBuf[TxLen++] = 0x01;

                // ID
                byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxMacNew.Text);
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

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xBC;
                TxBuf[TxLen++] = 0xBC;

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
        /// 修改应用配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetAppCfg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[60];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCB;
                TxBuf[TxLen++] = 0xCB;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Device Type
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x02;

                // Protocol
                TxBuf[TxLen++] = 0x01;

                // Customer
                byte[]  ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCustomerNew.Text);
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
                TxBuf[TxLen++] = Convert.ToByte(tbxChannelNew.Text);

                // TxPower 
                TxBuf[TxLen++] = (byte)Convert.ToInt16(tbxTxPowerNew.Text);

                // Sample Interval 1
                UInt16 Interval = Convert.ToUInt16(tbxSampleInterval1New.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Sample Interval 2
                Interval = Convert.ToUInt16(tbxSampleInterval2New.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Sample Interval ACO2
                Interval = Convert.ToUInt16(tbxSampleIntervalACO2New.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Transfer Interval 
                Interval = Convert.ToUInt16(tbxTransferIntervalNew.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Heat Interval 
                Interval = Convert.ToUInt16(tbxHeatIntervalNew.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Cool Interval 
                Interval = Convert.ToUInt16(tbxCoolIntervalNew.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Carousel Interval 
                Interval = Convert.ToUInt16(tbxCarouselIntervalNew.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // Alert Interval 
                Interval = Convert.ToUInt16(tbxAlertIntervalNew.Text);
                TxBuf[TxLen++] = (byte)((Interval & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((Interval & 0x00FF) >> 0);

                // 日期和时间
                tbxCalendar.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                DateTime ThisCalendar = Convert.ToDateTime(tbxCalendar.Text);
                ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                TxBuf[TxLen++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);

                // Time Source
                TxBuf[TxLen++] = Convert.ToByte(tbxTimeSourceNew.Text);

                // Alert Way
                TxBuf[TxLen++] = Convert.ToByte(tbxAlertWayNew.Text);

                // Lcd Policy
                TxBuf[TxLen++] = Convert.ToByte(tbxLcdPolicyNew.Text);

                // CO2 Compensate
                Int32 CompensateOfCO2 = Convert.ToInt32(tbxCO2CompensateNew.Text);
                TxBuf[TxLen++] = (byte)((CompensateOfCO2 & 0xFF0000) >> 16);
                TxBuf[TxLen++] = (byte)((CompensateOfCO2 & 0x00FF00) >> 8);
                TxBuf[TxLen++] = (byte)((CompensateOfCO2 & 0x0000FF) >> 0);

                // 加热下限
                TxBuf[TxLen++] = Convert.ToByte(tbxHeatLowThrNew.Text);

                // 加热上限
                TxBuf[TxLen++] = Convert.ToByte(tbxHeatHighThrNew.Text);

                // Reserved of Cfg
                ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxReservedOfCfgNew.Text);
                if (ByteBufTmp == null || ByteBufTmp.Length < 4)
                {
                    return;
                }
                TxBuf[TxLen++] = ByteBufTmp[0];
                TxBuf[TxLen++] = ByteBufTmp[1];
                TxBuf[TxLen++] = ByteBufTmp[2];
                TxBuf[TxLen++] = ByteBufTmp[3];

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xBC;
                TxBuf[TxLen++] = 0xBC;

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
        /// 删除已存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] TxBuf = new byte[16];
                UInt16 TxLen = 0;

                // Start
                TxBuf[TxLen++] = 0xCB;
                TxBuf[TxLen++] = 0xCB;

                // Length
                TxBuf[TxLen++] = 0x00;

                // Device Type
                TxBuf[TxLen++] = 0x00;

                // Cmd
                TxBuf[TxLen++] = 0x03;

                // Protocol
                TxBuf[TxLen++] = 0x01;

                // Which Flash Queue
                TxBuf[TxLen++] = (byte)cbxFlashQueue.SelectedIndex;

                // 协议保留位
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;
                TxBuf[TxLen++] = 0x00;

                // CRC16
                UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, (UInt16)(TxLen - 3));
                TxBuf[TxLen++] = (byte)((crc & 0xFF00) >> 8);
                TxBuf[TxLen++] = (byte)((crc & 0x00FF) >> 0);

                // End
                TxBuf[TxLen++] = 0xBC;
                TxBuf[TxLen++] = 0xBC;

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
        /// 清空Console中的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            tbxConsole.Text = "";
        }
    }
}
