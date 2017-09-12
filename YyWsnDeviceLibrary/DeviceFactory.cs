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
                return null;

            //处理从互联网端接收到的数据
            try
            {
                //可能的数据包括: 网关的上报信息包
                if (SourceData[0] == 0xBE && SourceData[1] == 0xBE && 
                    SourceData[48] == 0xEB && SourceData[49] == 0xEB &&
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
            catch (Exception )
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
                return null;

            ObservableCollection<Device> devices = new ObservableCollection<Device>();

            for (int i = 0; i < SourceData.Length; i++)
            {
                try
                {
                    if (SourceData[i] == 0xEA)
                    {
                        //if(SourceData[i+1] == null)
                        if (SourceData.Length <= i - 1)
                        {
                            continue;
                        }
                        int LengthCheck = SourceData[i + 1] + i;
                        //缺少长度校验
                        if (SourceData.Length >= LengthCheck + 4 && SourceData[LengthCheck + 4] == 0xAE)
                        {
                            //复制部分数字
                            //创建设备
                            byte[] deviceBytes = new byte[LengthCheck + 6 - i];

                            for (int j = 0; j < deviceBytes.Length; j++)
                            {
                                //TODO: 这里有错误，数组越界
                                if (SourceData.Length < i + j)
                                {
                                    break;
                                }
                                deviceBytes[j] = SourceData[i + j];
                            }

                            Device device = CreateDevice(deviceBytes);
                            if (device != null)
                            {
                                devices.Add(device);

                            }




                        }
                    }



                }
                catch (Exception)
                {

                   // throw;
                }


            }



            return devices;
        }
    }
}
