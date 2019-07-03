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
            }
            catch (Exception)
            {
                return null;
            }

            //处理从USB Gateway收到的传感器数据
            try
            {
                //兼容Z协议的USB/UART接收
                if (SourceData[0] == 0xEA && SourceData[1] == 0x18 && SourceData.Length == 28)
                {
                    M1 sensorM1 = new M1(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }
                }

                // 处理监测工具监听到的M1发出的温湿度数据包
                if (SourceData[0] == 0xEA && SourceData[1] == 0x22 && SourceData.Length >= 40)
                {
                    // 结束位
                    if (SourceData[4 + SourceData[1]] != 0xAE)
                    {
                        return null;
                    }

                    // CRC
                    UInt16 rx_crc = (UInt16)(SourceData[2 + SourceData[1]] * 256 + SourceData[3 + SourceData[1]]);

                    byte[] crc_data = new byte[SourceData[1]];
                    for (UInt16 iCnt = 0; iCnt < SourceData[1]; iCnt++)
                    {
                        crc_data[iCnt] = SourceData[2 + iCnt];
                    }
                    UInt16 crc_chk = CommArithmetic.CRC16(0x1021, 0, crc_data, (UInt16)crc_data.Length);

                    if (rx_crc != 0 && rx_crc != crc_chk)
                    {
                        return null;
                    }

                    M1 sensorM1 = new M1(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }
                }

                if (SourceData[0] == 0xEC && SourceData[1] == 0x4D && SourceData[4] == 0x51)
                {
                    //发现M1 上电自检包
                    if (SourceData.Length == 82)
                    {
                        M1 sensorM1 = new M1(SourceData);
                        if (sensorM1 != null)
                        {
                            return sensorM1;
                        }
                    }
                }

                if (SourceData[0] == 0xEC && SourceData[1] == 0x4D && SourceData[4] == 0x53)
                {
                    //发现M1 上电自检包
                    if (SourceData.Length == 82)
                    {
                        M1 sensorM1 = new M1(SourceData);
                        if (sensorM1 != null)
                        {
                            return sensorM1;
                        }
                    }
                }

                //Socket1 M4
                if (SourceData[0] == 0xEC && SourceData[1] == 0x4D && SourceData[4] == 0x58)
                {
                    //发现Socket1 上电自检包
                    if (SourceData.Length == 82)
                    {
                        Socket1 sensorSocket1 = new Socket1(SourceData);
                        if (sensorSocket1 != null)
                        {
                            return sensorSocket1;
                        }
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

                //beetch
                if (SourceData[0] == 0xAC && SourceData[2] == 0x17 && SourceData[4] == 0x01)
                {

                    M11 sensorM1 = new M11(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }

                }

                //
                if (SourceData[0] == 0xAC && (SourceData[2] == 0x09 || SourceData[2] == 0x20) && SourceData[4] == 0x01)
                {

                    M11 sensorM1 = new M11(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
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
                    {   // 起始位为0xEA

                        //首先进行有效性验证，不通过直接返回null
                        //01.长度校验
                        int LengthCheck = SourceData[i + 1] + i;
                        if (SourceData.Length < LengthCheck + 4)
                        {
                            return null;
                        }
                        if (SourceData[LengthCheck + 4] != 0xAE)
                        {
                            return null;
                        }

                        //02.根据产品类型进行判断
                        switch (SourceData[i + 3])
                        {
                            case 0x51:
                                {
                                    device = new M1(SourceData);
                                    return device;
                                }
                            case 0x53:
                                {
                                    device = new M1(SourceData);
                                    return device;
                                }
                            case 0x57:
                                {
                                    device = new M2(SourceData);
                                    return device;
                                }
                            default:
                                {
                                    return null;
                                }
                        }
                    }
                    else if (SourceData[i] == 0xAE)
                    {
                        // USB_Sniffer 无线监测工具接收到的数据包
                        byte deviceType = SourceData[i + 3];
                        switch (deviceType)
                        {
                            case 0x51:  // M1
                            case 0x53:  // S1+
                            case 0x5C:  // M1_NTC
                            case 0x5D:  // M1_Beetech
                                {
                                    device = new M1(SourceData);
                                    return device;
                                }
                            case 0x57:
                                {
                                    device = new M2(SourceData);
                                    return device;
                                }
                            default:
                                {
                                    return null;
                                }
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
                        {
                            return null;
                        }
                        if (SourceData[LengthCheck + 5] != 0xCA && SourceData[LengthCheck + 6] == 0xCA)
                        {
                            return null;
                        }
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
                    if (M1.isSensorDataV1(SrcData, iCnt, true) >= 0 || M1.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        M1 m1 = new M1(SrcData, iCnt);
                        if (m1 != null)
                        {
                            devices.Add(m1);
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
                    else if (ACO2.isSensorDataV3(SrcData, iCnt, true) >= 0)
                    {
                        ACO2 aCO2 = new ACO2(SrcData, iCnt);
                        if (aCO2 != null)
                        {
                            devices.Add(aCO2);
                            continue;
                        }
                    }
                    else if (SK.isSensorDataV2(SrcData, iCnt, true) >= 0)
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
                }
                catch (Exception)
                {

                }
            }

            return devices;
        }
    }
}
