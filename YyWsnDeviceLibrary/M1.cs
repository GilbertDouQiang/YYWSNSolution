using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M1:Sensor//, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        public M1(byte[] SourceData)
        {


            //将收到的数据填充到属性
            Name = "M1";
            DeviceID = SourceData[3].ToString("X2");
            DeviceMac = CommArithmetic.DecodeMAC(SourceData,6);
            ClientID = CommArithmetic.DecodeClientID(SourceData, 4);
            WorkFunction = SourceData[2];

            SensorSN = SourceData[12] * 256 + SourceData[13];
            //传感器信息
            ICTemperature = SourceData[15]; //todo : 有符号整形数
            if (ICTemperature >= 128)
                ICTemperature -= 256;

            Volt =Math.Round( Convert.ToDouble((SourceData[17] * 256 + SourceData[18]) ) / 1000,2);
            int tempCalc = SourceData[20] * 256 + SourceData[21];
            if (tempCalc >= 0x8000)
                tempCalc -= 65536;
            Temperature = Math.Round((Convert.ToDouble( tempCalc)/ 100),2);
            

            Humidity = Math.Round(Convert.ToDouble((SourceData[23] * 256 + SourceData[24]) )/ 100,2);
            //广播模式，补充采集和传输时间
            SensorCollectTime = System.DateTime.Now;
            SensorTransforTime = System.DateTime.Now;
            RSSI = SourceData[29] / 2 - 138;

            this.SourceData = CommArithmetic.ToHexString(SourceData);




        }

        
        public double ICTemperature{ get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }

        /*
        public void OnPropertyChanged(String strProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(strProperty));
            }
        }
        */
        
    }
}
