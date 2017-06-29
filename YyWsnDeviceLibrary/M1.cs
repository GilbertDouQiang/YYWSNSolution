using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M1:Sensor
    {
        public M1(byte[] SourceData)
        {
            //将收到的数据填充到属性
            Name = "M1";
            DeviceID = SourceData[3];
            DeviceMac = CommArithmetic.DecodeMAC(SourceData,6);
            ClientID = CommArithmetic.DecodeClientID(SourceData, 4);
            WorkFunction = SourceData[2];

            SensorSN = SourceData[12] * 256 + SourceData[13];
            //传感器信息
            ICTemperature = SourceData[15]; //todo : 有符号整形数
            Volt =Math.Round( Convert.ToDouble((SourceData[17] * 256 + SourceData[18]) ) / 1000,2);
            Temperature = Math.Round(Convert.ToDouble((SourceData[20] * 256 + SourceData[21])) / 100,2);
            Humidity = Math.Round(Convert.ToDouble((SourceData[23] * 256 + SourceData[24]) )/ 100,2);
            //广播模式，补充采集和传输时间
            SensorCollectTime = System.DateTime.Now;
            SensorTransforTime = System.DateTime.Now;



        }

        
        public double ICTemperature{ get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }




    }
}
