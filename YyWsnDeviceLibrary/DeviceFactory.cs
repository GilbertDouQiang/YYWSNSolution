using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class DeviceFactory
    {
        /// <summary>
        /// 根据输入的参数，构造一个传感器或网关的对象
        /// </summary>
        /// <param name="SourceData"></param>
        /// <returns></returns>
        public static Device CreateDevice(byte[] SourceData)
        {
            Device device = null;
            if (SourceData == null)
            {
                return null;
            }                

            //处理从互联网端接收到的数据
            try
            {
                //可能的数据包括: 网关的上报信息包
                if (SourceData[0] == 0xBE && SourceData[1] == 0xBE &&
                    SourceData[49] == 0xEB && SourceData[50] == 0xEB &&
                    SourceData[3] == 0x92)
                {
                    //一体机数据
                    //BE BE 24 92 01 01 A2 59 11 11 11 11 20 17 09 11 15 13 10 68 00 01 00 01 00  00 00 00 12 69 21 74 09 09 75 70 00 33 46 6E 00 00 06 6F 00 00 00 00 EB EB
                    AlarmGateway1 alarm1 = new AlarmGateway1(SourceData);
                    return alarm1;
                }

                //可能的数据包括: 协议版本2 的网关的上报信息包
                if (SourceData[0] == 0xBE && SourceData[1] == 0xBE &&
                    SourceData[58] == 0xEB && SourceData[59] == 0xEB &&
                    SourceData[3] == 0x92)
                {
                    //一体机数据
                    //BE BE 24 92 01 01 A2 59 11 11 11 11 20 17 09 11 15 13 10 68 00 01 00 01 00  00 00 00 12 69 21 74 09 09 75 70 00 33 46 6E 00 00 06 6F 00 00 00 00 EB EB
                    AlarmGateway1 alarm1 = new AlarmGateway1(SourceData);
                    return alarm1;
                }
            }
            catch (Exception)
            {
                return null;
            }

            //处理从USB Gateway收到的传感器数据
            try
            {
                // 兼容Z协议的USB/UART接收
                if (SourceData[0] == 0xEA && SourceData[1] == 0x18 && SourceData.Length == 28)
                {
                    M1 sensorM1 = new M1(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }
                }

                //Socket1 M4
                if (SourceData[0] == 0xEC && SourceData[1] == 0x4D && SourceData[4] == 0x58 && SourceData.Length >= SourceData[1] + 5)
                {
                    Socket1 sensorSocket1 = new Socket1(SourceData);
                    if (sensorSocket1 != null)
                    {
                        return sensorSocket1;
                    }
                }

                // M1上电自检包，协议版本是0x02 
                if (SourceData[0] == 0xEC && SourceData[1] == 0x4D && SourceData.Length >= SourceData[1] + 5)
                {
                    M1 sensorM1 = new M1(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }
                }

                // M1上电自检包，协议版本是0x03  
                if (SourceData[0] == 0xEC && SourceData[1] == 0x55 && SourceData.Length >= 90)
                {      
                    M1 sensorM1 = new M1(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }
                }

                //M2
                if (SourceData[0] == 0xEC && SourceData[1] == 0x41 && SourceData[4] == 0x57)
                {
                    //发现Socket1 上电自检包
                    if (SourceData.Length == 0x46)
                    {
                        M2 m2 = new M2(SourceData);
                        if (m2 != null)
                        {
                            return m2;
                        }
                    }
                }

                //M30 GAS
                if (SourceData[0] == 0xEC && SourceData[1] == 0x67 && SourceData[4] == 0x7A)
                {
                    //发现M30 GAS上电自检包
                    if (SourceData.Length == 108) // 长度108
                    {
                        M30O2 m30 = new M30O2(SourceData);
                        if (m30 != null)
                        {
                            return m30;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            try
            {
                //1101 监控工具获得的数据
                for (int i = 0; i < SourceData.Length; i++)
                {
                    if (SourceData[i] == 0xEA)
                    {
                        //首先进行有效性验证，不通过直接返回null
                        //01.长度校验
                        int LengthCheck = SourceData[i + 1] + i;
                        if (SourceData.Length < LengthCheck + 4)
                            return null;
                        if (SourceData[LengthCheck + 4] != 0xAE)
                            return null;

                        //02.根据产品类型进行判断
                        switch (SourceData[i + 3])
                        {
                            case 0x51:
                                device = new M1(SourceData);
                                return device;
                            case 0x53:
                                device = new M1(SourceData);
                                return device;
                            case 0x57:
                                device = new M2(SourceData);
                                return device;                            
                            default:
                                return null;
                        }
                    }
                }

                // UartGateway 从Uart获得的数据
                for (int i = 0; i < SourceData.Length; i++)
                {
                    if (SourceData[i] == 0xAC && SourceData[i + 1] == 0xAC)
                    {
                        //UAERGateway(久通)  查询本地配置信息后获得的数据
                        //首先进行有效性验证，不通过直接返回null
                        //01.长度校验
                        int LengthCheck = SourceData[i + 2] + i;
                        if (SourceData.Length < LengthCheck + 6)
                            return null;
                        if (SourceData[LengthCheck + 5] != 0xCA && SourceData[LengthCheck + 6] == 0xCA)
                            return null;
                        //创建对应的对象
                        device = new UartGateway(SourceData);

                        return device;
                    }
                }

            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// 用户在连续的字节数组中，返回多个传感器信息
        /// </summary>
        /// <param name="SourceData"></param>
        /// <returns></returns>
        public static ObservableCollection<Device> CreateDevices(byte[] SrcData)
        {
            if (SrcData == null)
            {
                return null;
            }

            UInt16 SrcLen = (UInt16)SrcData.Length;

            // 创建设备列表
            ObservableCollection<Device> devices = new ObservableCollection<Device>();

            for (UInt16 iCnt = 0; iCnt < SrcLen; iCnt++)
            {
                try
                {
                    if (WP.isAdhocV1(SrcData, iCnt, true) >= 0)
                    {
                        WP wp = new WP(SrcData, iCnt, true);
                        if (wp != null)
                        {
                            devices.Add(wp);
                            continue;
                        }
                    }else if(WP.isAdhocDataUpV1(SrcData, iCnt, true) >= 0)
                    {
                        WP wp = new WP(SrcData, iCnt, true);
                        if (wp != null)
                        {
                            devices.Add(wp);
                            continue;
                        }
                    }
                    else if (M1.isNtpPktV1(SrcData, iCnt, true) >= 0)
                    {   // 授时
                        M1 m1 = new M1(SrcData, iCnt);
                        if (m1 != null)
                        {
                            devices.Add(m1);
                            continue;
                        }
                    }
                    else if (M1.isNtpRespondPktV1(SrcData, iCnt, true) >= 0)
                    {   // 授时反馈
                        M1 m1 = new M1(SrcData, iCnt);
                        if (m1 != null)
                        {
                            devices.Add(m1);
                            continue;
                        }
                    }
                    else if (M1.isSensorDataV1(SrcData, iCnt, true) >= 0 || M1.isSensorDataV3(SrcData, iCnt, true) >= 0 || M1.isSensorDataV4(SrcData, iCnt, true) >= 0 || M1.isSensorDataV1_MAX31855(SrcData, iCnt, true) >= 0)
                    {
                        M1 m1 = new M1(SrcData, iCnt);
                        if (m1 != null)
                        {
                            devices.Add(m1);
                            continue;
                        }
                    }
                    else if (M5.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        M5 m5 = new M5(SrcData, iCnt);
                        if (m5 != null)
                        {
                            devices.Add(m5);
                            continue;
                        }
                    }
                    else if (M20.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        M20 m20 = new M20(SrcData, iCnt);
                        if (m20 != null)
                        {
                            devices.Add(m20);
                            continue;
                        }
                    }
                    else if (M9.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        M9 m9 = new M9(SrcData, iCnt);
                        if (m9 != null)
                        {
                            devices.Add(m9);
                            continue;
                        }
                    }
                    else if (M40.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        M40 aM40 = new M40(SrcData, iCnt, Device.DataPktType.SensorFromSsToGw);
                        if (aM40 != null)
                        {
                            devices.Add(aM40);
                            continue;
                        }
                    }
                    else if (ACO2.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        ACO2 aCO2 = new ACO2(SrcData, iCnt);
                        if (aCO2 != null)
                        {
                            devices.Add(aCO2);
                            continue;
                        }
                    }
                    else if (SK.isSensorDataV2_and_V3(SrcData, iCnt, true) >= 0)
                    {
                        SK aSK = new SK(SrcData, iCnt);
                        if (aSK != null)
                        {
                            devices.Add(aSK);
                            continue;
                        }
                    }
                    else if (AO2.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        AO2 aO2 = new AO2(SrcData, iCnt);
                        if (aO2 != null)
                        {
                            devices.Add(aO2);
                            continue;
                        }
                    }
                    else if (L1.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        L1 aL1 = new L1(SrcData, iCnt);
                        if (aL1 != null)
                        {
                            devices.Add(aL1);
                            continue;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            return devices;
        }
    }
}
