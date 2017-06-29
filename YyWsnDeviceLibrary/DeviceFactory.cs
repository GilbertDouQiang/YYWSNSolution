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
            if (SourceData[0] == 0xEA)
            {
                //首先进行有效性验证，不通过直接返回null
                //01.长度校验
                int LengthCheck = SourceData[1];
                if (SourceData.Length < LengthCheck + 4)
                    return null;
                if (SourceData[LengthCheck + 4] != 0xAE)
                    return null;

                //02.根据产品类型进行判断
                switch (SourceData[3])
                {
                    case 0x51:
                        device = new M1(SourceData);
                        break;

                    default:
                        return null;
                        
                }


            }

          
           
            return device;
        }
    }
}
