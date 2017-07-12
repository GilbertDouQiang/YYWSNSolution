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
                if (SourceData[i] == 0xEA)
                {
                    int LengthCheck = SourceData[i + 1] + i;
                    //缺少长度校验
                    if (SourceData.Length>= LengthCheck+3 && SourceData[LengthCheck + 4] == 0xAE)
                    {
                        //复制部分数字
                        //创建设备
                        byte[] deviceBytes = new byte[LengthCheck + 6-i];

                        for (int j = 0; j < deviceBytes.Length; j++)
                        {
                            deviceBytes[j] = SourceData[i+j];
                        }

                        Device device = CreateDevice(deviceBytes);
                        if (device !=null)
                        {
                            devices.Add(device);

                        }




                    }
                }
            }



            return devices;
        }
    }
}
