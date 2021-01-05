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

        System.Timers.Timer TimerOfBsl;
        UInt16 TimeCntOfBsl = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.Title += "  v" +
           System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            TimerOfBsl = new System.Timers.Timer(1000);
            TimerOfBsl.Elapsed += TimerEventOfBsl2;

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
        /// 复位原因选择
        /// </summary>
        private string SelectResetSource(byte ResetSource)
        {
            switch (ResetSource)
            {
                case 0x00:
                    {
                        return "掉电/复位";
                    }
                case 0x01:
                    {
                        return "关机再开机";
                    }
                case 0x02:
                    {
                        return "低电关机长按开机";
                    }
                case 0x04:
                    {
                        return "低电关机接电开机";
                    }
                case 0x08:
                    {
                        return "系统保护";
                    }
                case 0x10:
                    {
                        return "自动重启";
                    }
                case 0x20:
                    {
                        return "系统退出";
                    }
                case 0x40:
                    {
                        return "系统错误";
                    }
                default:
                    {
                        return "未知重启";
                    }
            }
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
                tbxProtocolRevision.Text = SrcData[IndexOfStart + 5].ToString("D");

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

                tbxBps.Text = ((dynamic)tbxBpsNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
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

                Int32 CalibrationOfCO2 = 0;
                tbxStdCaliOfCO2.Text = CalibrationOfCO2.ToString("F2");

                tbxTimeSource.Text = ((dynamic)tbxTimeSourceNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
                iCnt += 1;

                tbxAlertWay.Text = ((dynamic)tbxAlertWayNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
                iCnt += 1;

                tbxLcdPolicy.Text = ((dynamic)tbxLcdPolicyNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
                iCnt += 1;

                tbxHeatLowThr.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxHeatHighThr.Text = SrcData[IndexOfStart + iCnt + 0].ToString("D");
                iCnt += 1;

                tbxReservedOfCfg.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                byte resetSource = SrcData[IndexOfStart + iCnt + 0];
                tbxResetSource.Text = SelectResetSource(resetSource);
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

                tbxStdCaliOfCO2.Text = "";
            }
            else if (SrcData[IndexOfStart + 5] == 0x02)
            {
                SrcLen = (UInt16)(SrcData.Length - IndexOfStart);
                if (SrcLen < 111)
                {
                    return -1;
                }

                tbxProtocolRevision.Text = SrcData[IndexOfStart + 5].ToString("D");

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

                //Object Bps = tbxBpsNew.Items[SrcData[IndexOfStart + iCnt + 0]];
                //dynamic bps = ((dynamic)Bps).Content;
                tbxBps.Text = ((dynamic)tbxBpsNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
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

                double CalibrationOfCO2 = (double)(SrcData[IndexOfStart + iCnt + 0] * 256 * 256 + SrcData[IndexOfStart + iCnt + 1] * 256 + SrcData[IndexOfStart + iCnt + 2]) / 10000.0f;
                tbxStdCaliOfCO2.Text = CalibrationOfCO2.ToString("F2");
                iCnt += 3;

                tbxTimeSource.Text = ((dynamic)tbxTimeSourceNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
                iCnt += 1;

                tbxAlertWay.Text = ((dynamic)tbxAlertWayNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
                iCnt += 1;

                tbxLcdPolicy.Text = ((dynamic)tbxLcdPolicyNew.Items[SrcData[IndexOfStart + iCnt + 0]]).Content;
                iCnt += 1;

                double HeatLowThr = (double)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHeatLowThr.Text = (HeatLowThr / 100.0f).ToString("F2");
                iCnt += 2;

                double HeatHighThr = (double)(SrcData[IndexOfStart + iCnt + 0] * 256 + SrcData[IndexOfStart + iCnt + 1]);
                tbxHeatHighThr.Text = (HeatHighThr / 100.0f).ToString("F2");
                iCnt += 2;

                tbxReservedOfCfg.Text = SrcData[IndexOfStart + iCnt + 0].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 1].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 2].ToString("X2") + " " + SrcData[IndexOfStart + iCnt + 3].ToString("X2");
                iCnt += 4;

                byte resetSource = SrcData[IndexOfStart + iCnt + 0];
                tbxResetSource.Text = SelectResetSource(resetSource);
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
            if (SrcData[IndexOfStart + 5] != 0x01 && SrcData[IndexOfStart + 5] != 0x02)
            {
                return -2;
            }

            // Error
            if (SrcData[IndexOfStart + 10] != 0x00)
            {
                return -3;
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
        /// 查询是否支持Bootloader，若是支持，则直接进入Bootloader状态
        /// </summary>
        /// <param name="SrcData"></param>
        /// <param name="IndexOfStart"></param>
        /// <returns></returns>
        private Int16 RxPkt_Bootloader(byte[] SrcData, UInt16 IndexOfStart)
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

            // State
            if (SrcData[IndexOfStart + 10] == 1)
            {
                // 支持BootLoader
            }

            return 0;
        }

        /// <summary>
        /// 在Console显示日志
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void ConsoleLog(string direct, byte[] Buf, UInt16 Start, UInt16 Len)
        {
            if (Buf == null)
            {
                return;
            }

            if (Buf.Length == 0 || Len == 0)
            {
                return;
            }

            if (chkLockLog.IsChecked == true)
            {
                return;
            }

            tbxConsole.Text = Logger.GetTimeString() + "\t" + direct + "\t" + MyCustomFxn.ToHexString(Buf, Start, Len) + "\r\n" + tbxConsole.Text;

            UInt16 ConsoleMaxLine = Convert.ToUInt16(txtLogLineLimit.Text);
            if (tbxConsole.LineCount > ConsoleMaxLine)
            {
                int start = tbxConsole.GetCharacterIndexFromLineIndex(ConsoleMaxLine);  // 末尾行第一个字符的索引
                int length = tbxConsole.GetLineLength(ConsoleMaxLine);                  // 末尾行字符串的长度
                tbxConsole.Select(start, start + length);                               // 选中末尾一行
                tbxConsole.SelectedText = "END";
            }
        }

        private int SerialPort_isReady()
        {
            if (SerialPort == null)
            {
                MessageBox.Show("请初始化串口！");
                return -1;
            }

            if (SerialPort.IsOpen() == false)
            {
                MessageBox.Show("请打开串口！");
                return -2;
            }

            return 0;
        }

        /// <summary>
        /// 串口发送数据，并在Console显示日志
        /// </summary>
        /// <param name="TxBuf"></param>
        /// <param name="IndexOfStart"></param>
        /// <param name="TxLen"></param>
        private void SerialPort_Send(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen)
        {
            if (SerialPort_isReady() < 0)
            {
                return;
            }

            // 在Console显示Log
            ConsoleLog("TX", TxBuf, IndexOfStart, TxLen);

            // 发送数据
            SerialPort.Send(TxBuf, IndexOfStart, TxLen);
        }

        private byte[] SerialPort_SendReceive(byte[] TxBuf, UInt16 IndexOfStart, UInt16 TxLen, UInt16 RxTimeoutMs)
        {
            if (SerialPort_isReady() < 0)
            {
                return null;
            }

            // 在Console显示Log
            ConsoleLog("TX", TxBuf, IndexOfStart, TxLen);

            // 发送数据
            byte[] RxBuf = SerialPort.SendReceive(TxBuf, IndexOfStart, TxLen, RxTimeoutMs);

            // 在Console显示Log
            ConsoleLog("RX", RxBuf, 0, (UInt16)RxBuf.Length);

            return RxBuf;
        }

        /// <summary>
        /// 处理接收到的数据
        /// </summary>
        /// <param name="SrcData"></param>
        /// <returns></returns>
        private Int16 RxPkt_Handle(byte[] SrcData)
        {
            if (SrcData == null)
            {
                return -1;
            }

            UInt16 SrcLen = (UInt16)SrcData.Length;

            Int16 HandleLen = 0;
            Int16 ExeError = 0;

            for (UInt16 iCnt = 0; iCnt < SrcLen; iCnt++)
            {
                try
                {
                    HandleLen = RxPkt_IsRight(SrcData, iCnt);
                    if (HandleLen < 0)
                    {
                        continue;
                    }

                    switch (SrcData[iCnt + 4])
                    {
                        case 0x00:
                            {
                                ExeError = RxPkt_ReadCfg(SrcData, iCnt);
                                break;
                            }
                        case 0x01:
                            {
                                ExeError = RxPkt_SetFactoryCfg(SrcData, iCnt);
                                break;
                            }
                        case 0x02:
                            {
                                ExeError = RxPkt_SetAppCfg(SrcData, iCnt);
                                break;
                            }
                        case 0x03:
                            {
                                ExeError = RxPkt_DeleteHistory(SrcData, iCnt);
                                break;
                            }
                        case 0x38:
                            {
                                ExeError = RxPkt_Bootloader(SrcData, iCnt);
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
            return 0;
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

                RxPkt_Handle(e.ReceivedBytes);

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
            tbxStdCaliOfCO2New.Text = tbxStdCaliOfCO2.Text;
            tbxReservedOfCfgNew.Text = tbxReservedOfCfg.Text;
        }

        /// <summary>
        /// 将左列的配置清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearCfg_Click(object sender, RoutedEventArgs e)
        {
            tbxProtocolRevision.Text = "";
            tbxPrimaryMac.Text = "";

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
            tbxStdCaliOfCO2.Text = "";
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
        /// M30:修改应用配置，协议版本V1
        /// </summary>
        private Int16 SetAppCfg_V1()
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
            TxBuf[TxLen++] = 0x02;

            // Customer
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCustomerNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDebugNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCategoryNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Pattern
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxPatternNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -4;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Bps
            TxBuf[TxLen++] = (byte)tbxBpsNew.SelectedIndex;

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
            TxBuf[TxLen++] = (byte)tbxTimeSourceNew.SelectedIndex;

            // Alert Way
            TxBuf[TxLen++] = (byte)(tbxAlertWayNew.SelectedIndex);

            // Lcd Policy
            TxBuf[TxLen++] = (byte)(tbxLcdPolicyNew.SelectedIndex);

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
                return -6;
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

            return 0;
        }

        /// <summary>
        /// M30:修改应用配置，协议版本V2
        /// </summary>
        private Int16 SetAppCfg_V2()
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
            TxBuf[TxLen++] = 0x02;

            // Customer
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCustomerNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -1;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Debug
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxDebugNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 2)
            {
                return -2;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];
            TxBuf[TxLen++] = ByteBufTmp[1];

            // Category
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxCategoryNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -3;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Pattern
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxPatternNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 1)
            {
                return -4;
            }
            TxBuf[TxLen++] = ByteBufTmp[0];

            // Bps
            TxBuf[TxLen++] = (byte)(tbxBpsNew.SelectedIndex);

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
            TxBuf[TxLen++] = (byte)(tbxTimeSourceNew.SelectedIndex);

            // Alert Way
            TxBuf[TxLen++] = (byte)(tbxAlertWayNew.SelectedIndex);

            // Lcd Policy
            TxBuf[TxLen++] = (byte)(tbxLcdPolicyNew.SelectedIndex);

            // CO2 Compensate
            Int32 CompensateOfCO2 = Convert.ToInt32(tbxCO2CompensateNew.Text);
            TxBuf[TxLen++] = (byte)((CompensateOfCO2 & 0xFF0000) >> 16);
            TxBuf[TxLen++] = (byte)((CompensateOfCO2 & 0x00FF00) >> 8);
            TxBuf[TxLen++] = (byte)((CompensateOfCO2 & 0x0000FF) >> 0);

            // CO2 Calibration
            Int32 CalibrationOfCO2 = (Int32)(Convert.ToDouble(tbxStdCaliOfCO2New.Text) * 10000.0f);
            TxBuf[TxLen++] = (byte)((CalibrationOfCO2 & 0xFF0000) >> 16);
            TxBuf[TxLen++] = (byte)((CalibrationOfCO2 & 0x00FF00) >> 8);
            TxBuf[TxLen++] = (byte)((CalibrationOfCO2 & 0x0000FF) >> 0);

            // 加热下限
            Int16 HratLow = (Int16)(Convert.ToDouble(tbxHeatLowThrNew.Text) * 100.0f);
            TxBuf[TxLen++] = (byte)((HratLow & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((HratLow & 0x00FF) >> 0);

            // 加热上限
            Int16 HratHigh = (Int16)(Convert.ToDouble(tbxHeatHighThrNew.Text) * 100.0f);
            TxBuf[TxLen++] = (byte)((HratHigh & 0xFF00) >> 8);
            TxBuf[TxLen++] = (byte)((HratHigh & 0x00FF) >> 0);

            // Reserved of Cfg
            ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxReservedOfCfgNew.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                return -6;
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

            return 0;
        }

        /// <summary>
        /// 修改应用配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetAppCfg_Click(object sender, RoutedEventArgs e)
        {
            Int16 error = 0;

            try
            {
                byte Protocol = Convert.ToByte(tbxProtocolRevision.Text);

                if (Protocol == 0x01)
                {
                    error = SetAppCfg_V1();
                    if (error < 0)
                    {
                        return;
                    }
                }
                else if (Protocol == 0x02)
                {
                    error = SetAppCfg_V2();
                    if (error < 0)
                    {
                        return;
                    }
                }
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

        //=========================================================================================================================

        Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();                  // BSL

        BSL bsl = null;

        Thread bslThread = null;
        string LoadStatus = string.Empty;
        bool? OnlyLoadImage = false;
        string SerialPortName = string.Empty;
        bool Bsling = false;                        // 表示正在执行Bootloader过程

        void TimerEventOfBsl2(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                TimeCntOfBsl++;
                tbkCount2.Text = TimeCntOfBsl.ToString();
                tbxLoadStatus.Text = LoadStatus;

                // 显示定位到最后一行
                tbxLoadStatus.ScrollToEnd();

                if (Bsling == false)
                {
                    TimerOfBsl.Enabled = false;
                }
            }));
        }

        private void btnClearLoadStatus2_Click(object sender, RoutedEventArgs e)
        {
            tbxLoadStatus.Text = LoadStatus = string.Empty;
        }

        private void btnBrowse2_Click(object sender, RoutedEventArgs e)
        {
            if (ofd.ShowDialog() == true)
            {
                tbxLoadImage2.Text = ofd.SafeFileName;
            }
        }

        public void BSL_SerialPort_Init(System.IO.Ports.Parity parity)
        {
            if (SerialPort != null)
            {
                if (SerialPort.IsOpen() == true)
                {
                    SerialPort.ClosePort();
                }
            }

            SerialPort = new SerialPortHelper();
            SerialPort.IsLogger = true;
            SerialPort.InitCOM(SerialPortName, 115200, parity);
            SerialPort.ReceiveDelayMs = 2;
            SerialPort.OpenPort();
        }

        public void BSL_SerialPort_Close()
        {
            if (SerialPort != null)
            {
                if (SerialPort.IsOpen() == true)
                {
                    SerialPort.ClosePort();
                }
            }
        }

        public int BslReady_Gateway(ref bool SupportBsl)
        {
            SupportBsl = false;

            byte[] TxBuf = { 0xCB, 0xCB, 0x06, 0x02, 0x01, 0xC9, 0xFD, 0xBC, 0xB6, 0x00, 0x00, 0xBC, 0xBC };        // 网关
            byte[] RxBuf = null;

            for (int iX = 0; iX < 3; iX++)
            {
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 1000, 50);
                if (RxBuf == null || RxBuf.Length != 19)
                {
                    continue;
                }

                if (RxBuf[0] != 0xBC || RxBuf[1] != 0xBC || RxBuf[2] != 0x0C || RxBuf[3] != 0x02 || RxBuf[4] != 0x01 || RxBuf[17] != 0xCB || RxBuf[18] != 0xCB)
                {
                    return -2;
                }

                if (RxBuf[10] != 0x01)
                {
                    SupportBsl = false;
                }else
                {
                    SupportBsl = true;
                }

                return 0;
            }

            return -1;
        }

        public int BslReady_M44(ref bool SupportBsl)
        {
            SupportBsl = false;

            byte[] TxBuf = { 0xCB, 0xCB, 0x09, 0x00, 0x38, 0x01, 0xC9, 0xFD, 0xBC, 0xB6, 0x00, 0x00, 0x00, 0x00, 0xBC, 0xBC };        // M30/M44
            byte[] RxBuf = null;

            for (int iX = 0; iX < 3; iX++)
            {
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 1000, 50);
                if (RxBuf == null || RxBuf.Length != 19)
                {
                    continue;
                }

                if (RxBuf[0] != 0xBC || RxBuf[1] != 0xBC || RxBuf[2] != 0x0C || RxBuf[4] != 0x38 || RxBuf[5] != 0x01 || RxBuf[17] != 0xCB || RxBuf[18] != 0xCB)
                {
                    return -2;
                }

                if (RxBuf[10] != 0x01)
                {
                    SupportBsl = false;
                }
                else
                {
                    SupportBsl = true;
                }

                return 0;
            }

            return -1;
        }

        public int BslReady_Router(ref bool SupportBsl)
        {
            SupportBsl = false;

            byte[] TxBuf = { 0x05, 0xCB, 0x3B, 0x02, 0x01, 0xC9, 0xFD, 0xBC, 0xB6 };        // Router
            byte[] RxBuf = null;

            for (int iX = 0; iX < 3; iX++)
            {
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 1000, 100);
                if (RxBuf == null || RxBuf.Length != 18)
                {
                    continue;
                }

                if (RxBuf[0] < 0x0E || RxBuf[1] != 0xBC || RxBuf[3] != 0x02 || RxBuf[4] != 0x01)
                {
                    return -2;
                }

                if (RxBuf[10] != 0x01)
                {
                    SupportBsl = false;
                }
                else
                {
                    SupportBsl = true;
                }

                return 0;
            }

            return -1;
        }

        public int BslReady()
        {
            bool SupportBsl = false;

            int error = BslReady_Router(ref SupportBsl);
            if (error >= 0)
            {
                if (SupportBsl == true)
                {
                    return 0;
                }
                else
                {
                    return -64;
                }
            }

            error = BslReady_Gateway(ref SupportBsl);
            if (error >= 0)
            {
                if (SupportBsl == true)
                {
                    return 0;
                }
                else
                {
                    return -64;
                }
            }

            error = BslReady_M44(ref SupportBsl);
            if (error >= 0)
            {
                if (SupportBsl == true)
                {
                    return 0;
                }
                else
                {
                    return -64;
                }
            }

            return -36;
        }

        public int Ping(UInt16 MaxTryNum)
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            bool Suc = false;

            for (int iX = 0; iX < MaxTryNum; iX++)
            {
                TxBuf = BSL.MyPing();
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 50);
                if (RxBuf == null || RxBuf.Length == 0)
                {
                    System.Threading.Thread.Sleep(50);
                    continue;
                }

                if (RxBuf.Length != 1)
                {
                    continue;
                }

                if (RxBuf[0] == 0x00)
                {
                    continue;
                }

                if (RxBuf[0] == 0x51)
                {
                    Suc = true;
                    break;
                }
            }

            if (Suc == false)
            {
                return -1;
            }

            return 0;
        }

        public int Password()
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            int error = 0;

            for (int iX = 0; iX < 4; iX++)
            {
                TxBuf = BSL.MyPassword();
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 100);

                error = bsl.MyRxRight(RxBuf, TxBuf);
                if (error < 0)
                {
                    continue;
                }

                break;
            }

            return error;
        }

        public int Read()
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            byte[] Buf = null;
            UInt16 Len = 0;

            UInt32 Addr = 0x3F000;
            UInt16 Size = 1024;

            int error = 0;

            for (int iX = 0; iX < 4; iX++)
            {
                Buf = new byte[Size];
                Len = 0;

                TxBuf = bsl.MyRead(Addr, Size);
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 4000, 100);
                if (RxBuf == null || RxBuf.Length < 8 || RxBuf.Length < Size)
                {
                    continue;
                }

                bool First = true;
                int BufStart = 0;
                UInt16 BufLen = 0;
                int PktLen = 0;

                for (int iJ = 0; iJ < RxBuf.Length;)
                {
                    error = bsl.MyRxRight_Read(RxBuf, iJ, First, ref BufStart, ref BufLen, ref PktLen);
                    if (error < 0)
                    {
                        iJ++;
                        continue;
                    }

                    First = false;
                    iJ += PktLen;

                    if (Len + BufLen > Size)
                    {
                        return -21;
                    }

                    for (int iK = 0; iK < BufLen; iK++)
                    {
                        Buf[Len++] = RxBuf[BufStart + iK];
                    }

                    if (Len < Size)
                    {
                        continue;
                    }

                    bsl.MyOverWrite(Addr, Buf);

                    return 0;
                }
            }

            return error;
        }

        public int MassErase()
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            int error = 0;

            for (int iX = 0; iX < 4; iX++)
            {   
                TxBuf = BSL.MyMassErase();
                RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 400, 20);

                error = bsl.MyRxRight(RxBuf, TxBuf);
                if (error < 0)
                {
                    continue;
                }

                break;
            }

            return error;
        }

        public int Check()
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            int error = 0;

            UInt32 ExpTotal = 256 * 1024;
            UInt32 CurTotal = 0;

            const UInt16 Unit = 4096;
            UInt16 Size = 0;

            while (CurTotal < ExpTotal)
            {
                if (ExpTotal - CurTotal >= Unit)
                {
                    Size = Unit;
                }
                else
                {
                    Size = (UInt16)(ExpTotal - CurTotal);
                }

                for (int iX = 0; iX < 4; iX++)
                {
                    TxBuf = bsl.MyCheck(CurTotal, Size);
                    RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 800, 10);

                    error = bsl.MyRxRight(RxBuf, TxBuf);
                    if (error < 0)
                    {
                        continue;
                    }

                    break;
                }

                if (error < 0)
                {
                    return error;
                }

                CurTotal += Size;
            }

            return error;
        }

        public int Write()
        {
            byte[] TxBuf = null;
            byte[] RxBuf = null;

            int error = 0;

            UInt32 ExpTotal = 256 * 1024;
            UInt32 CurTotal = 0;

            const UInt16 Unit = 256;
            UInt16 Len = 0;

            bool Ignore = false;

            while (CurTotal < ExpTotal)
            {
                if (ExpTotal - CurTotal >= Unit)
                {
                    Len = Unit;
                }
                else
                {
                    Len = (UInt16)(ExpTotal - CurTotal);
                }

                for (int iX = 0; iX < 4; iX++)
                {
                    TxBuf = bsl.MyWrite(CurTotal, Len, ref Ignore);
                    if (Ignore == true)
                    {
                        break;
                    }

                    RxBuf = SerialPort.SendReceive(TxBuf, 0, (UInt16)TxBuf.Length, 200, 2);

                    error = bsl.MyRxRight(RxBuf, TxBuf);
                    if (error < 0)
                    {
                        continue;
                    }

                    break;
                }

                if (error < 0)
                {
                    break;
                }

                CurTotal += Len;
            }

            return error;
        }

        public int RebootReset()
        {
            byte[] TxBuf = null;

            TxBuf = BSL.MyRebootReset();
            SerialPort.Send(TxBuf);

            return 0;
        }

        private void bslThreadFxn()
        {
            bsl = new BSL();

            int error = 0;
            bool Suc = false;

            try
            {
                do
                {
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   开始烧录\t" + ofd.SafeFileName + "\r\n";

                    if (OnlyLoadImage == false)
                    {   // 需要手动进入BSL
                        BSL_SerialPort_Init(System.IO.Ports.Parity.None);

                        error = BslReady();
                        if (error < 0)
                        {
                            LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   进入失败\t" + error.ToString() + "\r\n";
                            break;
                        }
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   进入成功" + "\r\n";
                    }
                    else
                    {   // 无需手动进入BSL
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   无需进入" + "\r\n";
                    }

                    BSL_SerialPort_Init(System.IO.Ports.Parity.Even);

                    error = bsl.MyOpen(ofd.FileName, 256 * 1024);
                    if (error < 0)
                    {
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   打开失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   打开成功" + "\r\n";

                    if (OnlyLoadImage == false)
                    {   // 需要手动进入BSL
                        error = Ping(60);
                    }
                    else
                    {
                        error = Ping(8);
                    }

                    if (error < 0)
                    {
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   通信失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   通信成功" + "\r\n";

                    error = Password();
                    if (error < 0)
                    {
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   密码错误\t" + error.ToString() + "\r\n";
                        break;
                    }
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   密码正确" + "\r\n";

                    error = Read();
                    if (error < 0)
                    {
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   备份失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   备份成功" + "\r\n";

                    error = MassErase();
                    if (error < 0)
                    {
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   擦除失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   擦除成功" + "\r\n";

                    error = Write();
                    if (error < 0)
                    {
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   写入失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   写入成功" + "\r\n";

                    error = Check();
                    if (error < 0)
                    {
                        LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   校验失败\t" + error.ToString() + "\r\n";
                        break;
                    }
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   校验成功" + "\r\n";

                    RebootReset();
                    LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   设备重启" + "\r\n";

                    Suc = true;

                } while (false);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                BSL_SerialPort_Close();
            }

            if (Suc == true)
            {
                LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   烧录成功\t耗时" + TimeCntOfBsl.ToString() + "秒钟\r\n";
            }
            else
            {
                LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   烧录失败" + "\r\n";
            }

            LoadStatus += "\r\n*****************************************************************************\r\n";

            Bsling = false;
        }

        private void btnLoadImage2_Click(object sender, RoutedEventArgs e)
        {
            if (cbSerialPort.SelectedIndex < 0)
            {
                MessageBox.Show("请选择串行设备！");
                return;
            }

            if (ofd.FileName == "")
            {
                MessageBox.Show("请选择烧录文件！");
                return;
            }
            else if (ofd.SafeFileName.EndsWith(".txt") == false)
            {
                MessageBox.Show("请选择.txt格式的烧录文件！");
                return;
            }

            if (LoadStatus == string.Empty)
            {
                LoadStatus = "*****************************************************************************\r\n\r\n";
            }
            else
            {
                LoadStatus += "\r\n";
            }

            // TODO: 2021-01-05 暂时注释掉MSP432的Bootloader，调试CC1310的Bootloader。
            /* 
            OnlyLoadImage = cbxOnlyLoadImage2.IsChecked;
            SerialPortName = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());

            bslThread = new Thread(bslThreadFxn);
            bslThread.Start();

            Bsling = true;

            // 启动计时器
            TimeCntOfBsl = 0;
            tbkCount2.Text = "";
            TimerOfBsl = new System.Timers.Timer(1000);
            TimerOfBsl.Elapsed += TimerEventOfBsl2;
            TimerOfBsl.Enabled = true;
            */

            Bootloader aBootloader = new Bootloader(SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString()), ofd);
            aBootloader.Execute();
        }

        private void btnEnterBsl_Click(object sender, RoutedEventArgs e)
        {
            SerialPortName = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());

            BSL_SerialPort_Init(System.IO.Ports.Parity.None);

            int error = BslReady();
            if (error < 0)
            {
                LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   进入失败\t" + error.ToString() + "\r\n";
            }
            else
            {
                LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   进入成功" + "\r\n";
            }

            tbxLoadStatus.Text = LoadStatus;

            BSL_SerialPort_Close();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            SerialPortName = SerialPortHelper.GetSerialPortName(cbSerialPort.SelectedValue.ToString());

            BSL_SerialPort_Init(System.IO.Ports.Parity.Even);

            int error = Ping(4);
            if (error < 0)
            {
                LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   通信失败\t" + error.ToString() + "\r\n";
            }
            else
            {
                LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   通信成功" + "\r\n";
            }

            RebootReset();
            LoadStatus += System.DateTime.Now.ToString("HH:mm:ss.fff") + "   设备重启" + "\r\n";

            tbxLoadStatus.Text = LoadStatus;

            BSL_SerialPort_Close();
        }

        /*******************/
    }
}
