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

        public M1()
        {

        }

        public M1(byte[] SourceData)
        {
            //上电自检数据
            if (SourceData.Length==82)
            {
                Name = "M1";
                DeviceID = SourceData[4].ToString("X2");
                ProtocolVersion = SourceData[5];
                PrimaryMAC = CommArithmetic.DecodeMAC(SourceData, 6);
                DeviceMac = CommArithmetic.DecodeMAC(SourceData, 10);
                HardwareVersion = CommArithmetic.DecodeMAC(SourceData, 14);
                SoftwareVersion = CommArithmetic.DecodeClientID(SourceData,18);
                ClientID = CommArithmetic.DecodeClientID(SourceData, 20);
                byte[] debugBytes = new byte[2];
                debugBytes[0] = SourceData[22];
                debugBytes[1] = SourceData[23];

                DebugString = CommArithmetic.DecodeClientID(SourceData, 22);

                Debug = debugBytes;
                Category = SourceData[24];
                Interval = SourceData[25] * 256 + SourceData[26];
                Calendar = CommArithmetic.DecodeDateTime(SourceData, 27);


                WorkFunction = SourceData[33];
                SymbolRate = SourceData[34];
                TXPower = SourceData[35];
                TXTimers = SourceData[36];
                Frequency = SourceData[37];

                TemperatureInfoHigh = CommArithmetic.DecodeTemperature(SourceData, 38);
                TemperatureInfoLow = CommArithmetic.DecodeTemperature(SourceData, 40);

                HumidityInfoHigh = CommArithmetic.DecodeHumidity(SourceData, 42);
                HumidityInfoLow = CommArithmetic.DecodeHumidity(SourceData, 44);

                TemperatureWarnHigh = CommArithmetic.DecodeTemperature(SourceData, 46);
                TemperatureWarnLow = CommArithmetic.DecodeTemperature(SourceData, 48);

                HumidityWarnHigh = CommArithmetic.DecodeHumidity(SourceData, 50);
                HumidityWarnLow = CommArithmetic.DecodeHumidity(SourceData, 52);

                TemperatureCompensation = CommArithmetic.DecodeTemperature(SourceData, 54);
                HumidityCompensation = CommArithmetic.DecodeHumidity(SourceData, 56);

                ICTemperature = SourceData[57];
                Volt = Math.Round(Convert.ToDouble((SourceData[59] * 256 + SourceData[60])) / 1000, 2);

                MaxLength = SourceData[63];

                Temperature = CommArithmetic.DecodeTemperature(SourceData, 73);
                Humidity = CommArithmetic.DecodeHumidity(SourceData, 75);
                RSSI = SourceData[30] - 256;









                



            }


            //模式1 正常传输的数据，兼容原Z版本
            if (SourceData.Length==31)
            {
                
                //将收到的数据填充到属性
                Name = "M1";
                DeviceID = SourceData[3].ToString("X2");
                DeviceMac = CommArithmetic.DecodeMAC(SourceData, 7);
                ClientID = CommArithmetic.DecodeClientID(SourceData, 5);
                WorkFunction = SourceData[2];
                ProtocolVersion = SourceData[4];

                SensorSN = SourceData[13] * 256 + SourceData[14];
                //传感器信息
                ICTemperature = SourceData[16]; //todo : 有符号整形数
                if (ICTemperature >= 128)
                    ICTemperature -= 256;

                Volt = Math.Round(Convert.ToDouble((SourceData[18] * 256 + SourceData[19])) / 1000, 2);
                int tempCalc = SourceData[21] * 256 + SourceData[22];
                if (tempCalc >= 0x8000)
                    tempCalc -= 65536;
                Temperature = Math.Round((Convert.ToDouble(tempCalc) / 100), 2);


                Humidity = Math.Round(Convert.ToDouble((SourceData[24] * 256 + SourceData[25])) / 100, 2);
                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SourceData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SourceData.Length == 31)
                {
                    RSSI = SourceData[30] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SourceData);


            }




        }

        public byte[] UpdateFactory()
        {
            byte[] updateBytes = new byte[21];
            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x10;
            updateBytes[2] = 0xA1;
            updateBytes[3] = 0x01;
            updateBytes[4] = 0x51;
            updateBytes[5] = 0x02;

            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMac);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceNewMAC);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];
            updateBytes[12] = deviceMacBytes[2];
            updateBytes[13] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(HardwareVersion);
            updateBytes[14] = deviceMacBytes[0];
            updateBytes[15] = deviceMacBytes[1];
            updateBytes[16] = deviceMacBytes[2];
            updateBytes[17] = deviceMacBytes[3];


            updateBytes[18] = 0x00;
            updateBytes[19] = 0x00;
            updateBytes[20] = 0xEC;

            //updateBytes[0] = 0xCE;




            return updateBytes;
        }

        public byte[] UpdateUserConfig()
        {
            byte[] updateBytes = new byte[27];
            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x16;
            updateBytes[2] = 0xA2;
            updateBytes[3] = 0x01;
            updateBytes[4] = 0x51;
            updateBytes[5] = 0x02;
            //Mac
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMac);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];
            //clientID
            deviceMacBytes = CommArithmetic.HexStringToByteArray(ClientID);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];

            //Debug
            deviceMacBytes = CommArithmetic.HexStringToByteArray(DebugString);          
            updateBytes[12] = deviceMacBytes[0];
            updateBytes[13] = deviceMacBytes[1];

            //category
            updateBytes[14] = Category;
            //WorkFunction
            updateBytes[15] = WorkFunction;
            //SymbolRate
            updateBytes[16] = SymbolRate;
            //TXPower
            updateBytes[17] = TXPower;
            //Frequency
            updateBytes[18] = Frequency;

            //温度补偿：TODO 暂未实现
            updateBytes[19] = 0x00;
            updateBytes[20] = 0x00;

            //湿度补偿：TODO 暂未实现
            updateBytes[21] = 0x00;
            updateBytes[22] = 0x00;

            updateBytes[23] = MaxLength;

            //CRC：TODO 暂未实现
            updateBytes[24] = 0x00;
            updateBytes[25] = 0x00;


            updateBytes[26] = 0xEC;

            //updateBytes[0] = 0xCE;




            return updateBytes;
        }


        public byte[] UpdateApplicationConfig()
        {
            byte[] updateBytes = new byte[38];
            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x21;
            updateBytes[2] = 0xA3;
            updateBytes[3] = 0x01;
            updateBytes[4] = 0x51;
            updateBytes[5] = 0x02;
            //Mac
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMac);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];
            //Interval
            deviceMacBytes = CommArithmetic.Int16_2Bytes(Interval);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];

            //Calendar
            deviceMacBytes = CommArithmetic.EncodeDateTime(Calendar);
            updateBytes[12] = deviceMacBytes[0];
            updateBytes[13] = deviceMacBytes[1];
            updateBytes[14] = deviceMacBytes[2];
            updateBytes[15] = deviceMacBytes[3];
            updateBytes[16] = deviceMacBytes[4];
            updateBytes[17] = deviceMacBytes[5];

            updateBytes[18] = TXTimers;


            //CRC：TODO 暂未实现
            updateBytes[35] = 0x00;
            updateBytes[36] = 0x00;


            updateBytes[37] = 0xEC;

            //updateBytes[0] = 0xCE;




            return updateBytes;
        }


        public double ICTemperature{ get; set; }

        public double Temperature { get; set; }

        public double Humidity { get; set; }

        /// <summary>
        /// 温度预警上限
        /// </summary>
        public double TemperatureInfoHigh { get; set; }
        /// <summary>
        /// 温度预警下限
        /// </summary>
        public double TemperatureInfoLow { get; set; }

        /// <summary>
        /// 温度报警上限
        /// </summary>
        public double TemperatureWarnHigh { get; set; }

        /// <summary>
        /// 温度报警下限
        /// </summary>
        public double TemperatureWarnLow { get; set; }

        /// <summary>
        /// 湿度预警上限
        /// </summary>
        public double HumidityInfoHigh { get; set; }
        /// <summary>
        /// 湿度预警下限
        /// </summary>
        public double HumidityInfoLow { get; set; }

        /// <summary>
        /// 湿度报警上限
        /// </summary>
        public double HumidityWarnHigh { get; set; }

        /// <summary>
        /// 湿度报警下限
        /// </summary>
        public double HumidityWarnLow { get; set; }

        /// <summary>
        /// 温度补偿
        /// </summary>
        public double TemperatureCompensation { get; set; }

        /// <summary>
        /// 湿度补偿
        /// </summary>
        public double HumidityCompensation { get; set; }

        public byte MaxLength { get; set; }

        public string DebugString { get; set; }




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
