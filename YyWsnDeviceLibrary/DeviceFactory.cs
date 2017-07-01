using System;
using System.Collections.Generic;
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
            Device device =null;
            if (SourceData == null)
                return null;

            for (int i = 0; i < SourceData.Length; i++)
            {
                if (SourceData[i] == 0xEA)
                {
                    //首先进行有效性验证，不通过直接返回null
                    //01.长度校验
                    int LengthCheck = SourceData[i+1]+i;
                    if (SourceData.Length < LengthCheck + 4)
                        return null;
                    if (SourceData[LengthCheck + 4] != 0xAE)
                        return null;

                    //02.根据产品类型进行判断
                    switch (SourceData[i+3])
                    {
                        case 0x51:

                            device = new M1(SourceData);
                            return device;
                            

                        default:
                            return null;

                    }


                }

            }


            return null;
           
           
        }
    }
}
