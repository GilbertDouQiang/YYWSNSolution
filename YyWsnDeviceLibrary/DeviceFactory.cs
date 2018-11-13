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
                    SourceData[3]==0x92)
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
                if (SourceData[0]==0xEA && SourceData[1]==0x18 && SourceData.Length==28)
                {
                    M1 sensorM1 = new M1(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }
                }

                // 处理监测工具监听到的M1发出的温湿度数据包
                if (SourceData[0] == 0xEA && SourceData[1] == 0x22 && SourceData.Length == 40)
                {
                    M1 sensorM1 = new M1(SourceData);
                    if (sensorM1 != null)
                    {
                        return sensorM1;
                    }
                }

                if (SourceData[0]==0xEC && SourceData[1]==0x4D && SourceData[4]==0x51)
                {
                    //发现M1 上电自检包
                    if(SourceData.Length==82)
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
                if (SourceData[0] == 0xAC && SourceData[2] == 0x17 && SourceData[4] == 0x01) {
                    
                    M11 sensorM1 = new M11(SourceData);
                    if (sensorM1 != null) {
                        return sensorM1;
                    }

                }

                //
                if (SourceData[0] == 0xAC &&( SourceData[2] == 0x09|| SourceData[2] == 0x20) && SourceData[4] == 0x01) {

                    M11 sensorM1 = new M11(SourceData);
                    if (sensorM1 != null) {
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
                    if (SourceData[i] == 0xEA) {   // 起始位为0xEA

                        //首先进行有效性验证，不通过直接返回null
                        //01.长度校验
                        int LengthCheck = SourceData[i + 1] + i;
                        if (SourceData.Length < LengthCheck + 4) {
                            return null;
                        }
                        if (SourceData[LengthCheck + 4] != 0xAE) {
                            return null;
                        }

                        //02.根据产品类型进行判断
                        switch (SourceData[i + 3]) {
                            case 0x51: {
                                    device = new M1(SourceData);
                                    return device;
                                }
                            case 0x53: {
                                    device = new M1(SourceData);
                                    return device;
                                }
                            case 0x57: {
                                    device = new M2(SourceData);
                                    return device;
                                }
                            default: {
                                    return null;
                                }
                        }
                    }
                    else if (SourceData[i] == 0xAE) {
                        byte deviceType = SourceData[i + 3];
                        switch (deviceType) {
                            case 0x51: {
                                    device = new M1(SourceData);
                                    return device;
                                }
                            case 0x53: {
                                    device = new M1(SourceData);
                                    return device;
                                }
                            case 0x57: {
                                    device = new M2(SourceData);
                                    return device;
                                }
                            default: {
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
                        if (SourceData.Length < LengthCheck + 6) {
                            return null;
                        }
                        if (SourceData[LengthCheck + 5] != 0xCA && SourceData[LengthCheck + 6] == 0xCA) {
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
        public static ObservableCollection<Device> CreateDevices(byte[] SourceData)
        {
            if (SourceData == null)
            {
                return null;
            }

            ObservableCollection<Device> devices = new ObservableCollection<Device>();

            //临时应用，为兼容Z协议，只能处理单条，可能出现丢包
            if(SourceData.Length ==28 && SourceData[0]==0xEA && SourceData[1] ==0x18)
            {
                Device device = CreateDevice(SourceData);
                devices.Add(device);
                return devices;
            }
            
            //仅适合新协议，不兼容Z协议
           for (int i = 0; i < SourceData.Length; i++) {
                try {
                    if (SourceData[i] == 0xEA) {   // 起始位是0xEA
                        if (SourceData.Length <= i - 1) {
                            continue;
                        }

                        int LengthCheck = SourceData[i + 1] + i;        // 数据包长度，单位：Byte

                        //缺少长度校验
                        if (SourceData.Length >= LengthCheck + 4 && SourceData[LengthCheck + 4] == 0xAE) {
                            //复制部分数字
                            //创建设备
                            byte[] deviceBytes = new byte[LengthCheck + 6 - i];

                            for (int j = 0; j < deviceBytes.Length; j++) {
                                //TODO: 这里有错误，数组越界
                                if (SourceData.Length < i + j) {
                                    break;
                                }
                                deviceBytes[j] = SourceData[i + j];
                            }

                            Device device = CreateDevice(deviceBytes);
                            if (device != null) {
                                devices.Add(device);
                            }
                        }
                    } else if (SourceData[i] == 0xAE) {
                        // 起始位是0xAE
                        UInt16 StartIndex = (UInt16)i;
                        if (SourceData.Length - StartIndex < 6) {
                            continue;           // 起始位、长度位、CRC16、结束位、RSSI共占据了6个Byte
                        }

                        byte Len = SourceData[StartIndex + 1];      // 长度位
                        byte PktLen = (byte)(2 + Len + 2 + 2);      // 数据包的总长度，包含RSSI，单位：Byte
                        if (SourceData.Length < StartIndex + PktLen) {
                            continue;           // 长度位错误
                        }

                        UInt16 crc = (UInt16)(SourceData[StartIndex + 2 + Len] * 256 + SourceData[StartIndex + 2 + Len + 1]);
                        byte End = SourceData[StartIndex + 2 + Len + 2];
                        byte Rssi = SourceData[StartIndex + 2 + Len + 2 + 1];

                        if (End != 0xEA) {
                            continue;           // 结束位错误
                        }

                        // 单独摘出一个完整的数据包
                        byte[] deviceBytes = new byte[PktLen];
                        for (int j = 0; j < PktLen; j++) {
                            deviceBytes[j] = SourceData[StartIndex + j];
                        }

                        // 根据接收到数据包来创建对象
                        Device device = CreateDevice(deviceBytes);
                        if (device == null) {
                            continue;
                        }

                        // 已创建了对象，添加到Device数组里
                        devices.Add(device);
                    }
                }
                catch (Exception) {
                }
            }

            for (int i = 0; i < SourceData.Length; i++) {
                try {
                    if (SourceData[i] == 0xAC &&( SourceData[2]==0x20 || SourceData[2] == 0x09|| SourceData[2] == 0x17))
                 {  
                        if (SourceData.Length <= i - 1) {
                            continue;
                        }

                        int LengthCheck = SourceData[i + 2] + i;        // 数据包长度，单位：Byte

                        //缺少长度校验
                        if (SourceData.Length >= LengthCheck + 4 && SourceData[LengthCheck + 5] == 0xCA) 
                            {
                            //复制部分数字
                            //创建设备
                            byte[] deviceBytes = new byte[LengthCheck + 7 - i];

                            for (int j = 0; j < deviceBytes.Length; j++) {
                                //TODO: 这里有错误，数组越界
                                if (SourceData.Length < i + j) {
                                    break;
                                }
                                deviceBytes[j] = SourceData[i + j];
                            }

                            Device device = CreateDevice(deviceBytes);
                            if (device != null) {
                                devices.Add(device);
                                
                            }
                        }
                    }
                    
                }

                catch (Exception) {
                }
            }
            return devices;
        }
    }
}
