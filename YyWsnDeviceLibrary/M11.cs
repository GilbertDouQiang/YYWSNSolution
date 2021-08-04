using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class M11 : Sensor//, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        public M11()
        {

        }

        public M11(byte[] SrcData)
        {
            //2017版协议 v3.5
            //判断第三位 ，01 代表从传感器发出的正常数据，长度不是固定值

            //上电自检数据
            if (SrcData.Length == 82)
            {
                SetDeviceName(SrcData[4]);
                ProtocolVersion = SrcData[5];
                SetDevicePrimaryMac(SrcData, 6);
                SetDeviceMac(SrcData, 10);
                HwRevisionS = CommArithmetic.DecodeMAC(SrcData, 14);
                SwRevisionS = CommArithmetic.DecodeClientID(SrcData, 18);
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 20);
                DebugV = (UInt16)(SrcData[22] * 256 + SrcData[23]);
                Category = SrcData[24];
                Interval = (UInt16)(SrcData[25] * 256 + SrcData[26]);
                Calendar = CommArithmetic.DecodeDateTime(SrcData, 27);

                Pattern = SrcData[33];
                Bps = SrcData[34];
                SetTxPower(SrcData[35]);
                SampleSend = SrcData[36];
                Channel = SrcData[37];

                TemperatureInfoHigh = CommArithmetic.DecodeTemperature(SrcData, 38);
                TemperatureInfoLow = CommArithmetic.DecodeTemperature(SrcData, 40);

                HumidityInfoHigh = CommArithmetic.DecodeHumidity(SrcData, 42);
                HumidityInfoLow = CommArithmetic.DecodeHumidity(SrcData, 44);

                TemperatureWarnHigh = CommArithmetic.DecodeTemperature(SrcData, 46);
                TemperatureWarnLow = CommArithmetic.DecodeTemperature(SrcData, 48);

                HumidityWarnHigh = CommArithmetic.DecodeHumidity(SrcData, 50);
                HumidityWarnLow = CommArithmetic.DecodeHumidity(SrcData, 52);

                TemperatureCompensation = CommArithmetic.DecodeTemperature(SrcData, 54);
                HumidityCompensation = CommArithmetic.DecodeHumidity(SrcData, 56);

                ICTemperature = SrcData[58];
                voltF = Math.Round(Convert.ToDouble((SrcData[59] * 256 + SrcData[60])) / 1000, 2);

                MaxLength = SrcData[63];

                Temperature = CommArithmetic.DecodeTemperature(SrcData, 73);
                Humidity = CommArithmetic.DecodeHumidity(SrcData, 75);
                RSSI = SrcData[78] - 256;

                //Falsh
                FlashID = CommArithmetic.DecodeClientID(SrcData, 61);
                FlashFront = (UInt32)(SrcData[64] * 256 * 256 + SrcData[65] * 256 + SrcData[66]);
                FlashRear = (UInt32)(SrcData[67] * 256 * 256 + SrcData[68] * 256 + SrcData[69]);
                FlashQueueLength = (UInt32)(SrcData[70] * 256 * 256 + SrcData[71] * 256 + SrcData[72]);
            }

            // 处理监测工具监听到的M1发出的温湿度数据包
            STP = SrcData[0];
            if (SrcData[0] == 0xAC && SrcData[1] == 0xAC && SrcData[3] == 0x0d && (SrcData[5] == 0x51 || SrcData[5] == 0x5C || SrcData[5] == 0x5D))
            {
                // 将收到的数据填充到属性

                // Cmd
                Pattern = SrcData[3];

                // Device type                
                SetDeviceName(SrcData[5]);

                // Last/History
                LastHistory = SrcData[2];

                // protocol
                ProtocolVersion = SrcData[4];

                //PrimaryMacS

                // Sensor ID
                SetDeviceMac(SrcData, 6);

                //Error
                Error = SrcData[10];

                //开始时间
                StartTime = CommArithmetic.DecodeDateTime(SrcData, 14);

                //结束时间
                FinishTime = CommArithmetic.DecodeDateTime(SrcData, 20);

                // 传输时间                
                SensorTransforTime = System.DateTime.Now;

                //可能收到没有RSSI的数据               

                this.SourceData = CommArithmetic.ToHexString(SrcData);
            }

            if (SrcData[0] == 0xAC && SrcData[1] == 0xAC && SrcData[2] == 0x20 && (SrcData[5] == 0x51 || SrcData[5] == 0x5C || SrcData[5] == 0x5D))
            {
                // 将收到的数据填充到属性

                // Cmd
                Pattern = SrcData[3];

                // Device type  
                SetDeviceName(SrcData[5]);

                // Last/History
                LastHistory = SrcData[2];

                // protocol
                ProtocolVersion = SrcData[4];

                //PrimaryMacS
                // PrimaryMacS = CommArithmetic.DecodePrimaryMAC(SrcData, 5);

                // Sensor ID
                DeviceMacS = CommArithmetic.DecodeMAC(SrcData, 6);

                //过程
                //Progrom = SrcData[10];

                //Error
                Error = SrcData[11];

                //SendOK
                SendOK = SrcData[15];

                //状态state
                State = SrcData[16];

                //报警项
                AlarmItem = SrcData[17];

                //传输序列号
                PSensorSN = SrcData[18] * 256 + SrcData[19];

                //采集时间
                SensorCollectTime = CommArithmetic.DecodeDateTime(SrcData, 20);

                //片内温度
                ICTemperature = SrcData[22];

                //电压
                voltF = Math.Round(Convert.ToDouble((SrcData[27] * 256 + SrcData[28])) / 1000, 2);

                //采集序列号
                CSensorSN = SrcData[29] * 256 + SrcData[30];

                // temp
                int tempCalc = SrcData[31] * 256 + SrcData[32];
                if (tempCalc >= 0x8000)
                {
                    tempCalc -= 65536;
                }
                Temperature = Math.Round((Convert.ToDouble(tempCalc) / 100), 2);

                // hum
                Humidity = Math.Round(Convert.ToDouble((SrcData[33] * 256 + SrcData[34])) / 100, 2);

                // 传输时间                
                SensorTransforTime = System.DateTime.Now;

                //可能收到没有RSSI的数据

                this.SourceData = CommArithmetic.ToHexString(SrcData);
            }

            //兼容模式，兼容Z模式
            if (SrcData.Length == 28)
            {
                SetDeviceName(0x51);
                DeviceMacS = CommArithmetic.DecodeMAC(SrcData, 5);
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 3);
                Pattern = SrcData[2];
                ProtocolVersion = 0x00;

                SensorSN = SrcData[12] * 256 + SrcData[13];

                //传感器信息
                ICTemperature = 0; //老协议中没有IC 温度

                voltF = CommArithmetic.SHT20Voltage(SrcData[24], SrcData[25]);

                Temperature = CommArithmetic.SHT20Temperature(SrcData[16], SrcData[17]);

                Humidity = CommArithmetic.SHT20Humidity(SrcData[20], SrcData[21]);
                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;
                //RSSI = SrcData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SrcData.Length == 28)
                {
                    RSSI = SrcData[27] - 256;
                }
                this.SourceData = CommArithmetic.ToHexString(SrcData);
            }


            //模式1 正常传输的数据，
            if (SrcData.Length == 31)
            {
                //将收到的数据填充到属性
                SetDeviceName(SrcData[3]);

                DeviceMacS = CommArithmetic.DecodeMAC(SrcData, 7);
                CustomerS = CommArithmetic.DecodeClientID(SrcData, 5);
                Pattern = SrcData[2];
                ProtocolVersion = SrcData[4];

                SensorSN = SrcData[13] * 256 + SrcData[14];

                //传感器信息
                ICTemperature = SrcData[16]; //todo : 有符号整形数
                if (ICTemperature >= 128)
                {
                    ICTemperature -= 256;
                }

                voltF = Math.Round(Convert.ToDouble((SrcData[18] * 256 + SrcData[19])) / 1000, 2);
                int tempCalc = SrcData[21] * 256 + SrcData[22];
                if (tempCalc >= 0x8000)
                {
                    tempCalc -= 65536;
                }
                Temperature = Math.Round((Convert.ToDouble(tempCalc) / 100), 2);

                Humidity = Math.Round(Convert.ToDouble((SrcData[24] * 256 + SrcData[25])) / 100, 2);

                //广播模式，补充采集和传输时间
                SensorCollectTime = System.DateTime.Now;
                SensorTransforTime = System.DateTime.Now;

                //RSSI = SrcData[30] / 2 - 138; //1101 方案
                //可能收到没有RSSI的数据
                if (SrcData.Length == 31)
                {
                    RSSI = SrcData[30] - 256;
                }

                this.SourceData = CommArithmetic.ToHexString(SrcData);
            }
        }

        public byte[] UpdateFactory()
        {
            byte[] updateBytes = new byte[21];

            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x10;
            updateBytes[2] = 0xA1;
            updateBytes[3] = 0x01;

            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
            {
                updateBytes[4] = 0x51;
            }
            else if (DeviceTypeS == "53")
            {
                updateBytes[4] = 0x53;
            }
            else if (DeviceTypeS == "57")
            {
                updateBytes[4] = 0x57;
            }

            updateBytes[5] = 0x02;

            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacNewS);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];
            updateBytes[12] = deviceMacBytes[2];
            updateBytes[13] = deviceMacBytes[3];

            deviceMacBytes = CommArithmetic.HexStringToByteArray(HwRevisionS);
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
            if (DeviceTypeS == "57")
            {
                updateBytes[1] = 0x14;
            }
            else
            {
                updateBytes[1] = 0x16;
            }

            updateBytes[2] = 0xA2;
            updateBytes[3] = 0x01;
            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
            {
                updateBytes[4] = 0x51;
            }
            else if (DeviceTypeS == "53")
            {
                updateBytes[4] = 0x53;
            }
            else if (DeviceTypeS == "57")
            {
                updateBytes[4] = 0x57;
            }

            updateBytes[5] = 0x02;

            //DeviceMacV
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];

            //clientID
            deviceMacBytes = CommArithmetic.HexStringToByteArray(CustomerS);
            updateBytes[10] = deviceMacBytes[0];
            updateBytes[11] = deviceMacBytes[1];

            //Debug
            updateBytes[12] = (byte)(DebugV / 256);
            updateBytes[13] = (byte)(DebugV % 256);

            //category
            updateBytes[14] = Category;
            //Pattern
            updateBytes[15] = Pattern;
            //Bps
            updateBytes[16] = Bps;
            //TxPower
            updateBytes[17] = (byte)TxPower;
            //Channel
            updateBytes[18] = Channel;

            //温度补偿：TODO 暂未实现
            updateBytes[19] = 0x00;
            updateBytes[20] = 0x00;

            if (DeviceTypeS == "57")
            {
                //湿度补偿：TODO 暂未实现,M2 没有湿度补偿

                updateBytes[21] = MaxLength;

                //CRC：TODO 暂未实现
                updateBytes[22] = 0x00;
                updateBytes[23] = 0x00;

                updateBytes[24] = 0xEC;
            }
            else
            {
                updateBytes[21] = 0x00;
                updateBytes[22] = 0x00;

                updateBytes[23] = MaxLength;

                //CRC：TODO 暂未实现
                updateBytes[24] = 0x00;
                updateBytes[25] = 0x00;

                updateBytes[26] = 0xEC;
            }

            //updateBytes[0] = 0xCE;

            return updateBytes;
        }


        public byte[] UpdateApplicationConfig()
        {
            byte[] updateBytes = new byte[38];

            updateBytes[0] = 0xCE;

            if (DeviceTypeS != "57")
            {
                updateBytes[1] = 0x21;
            }
            else
            {
                updateBytes[1] = 0x19;
            }

            updateBytes[2] = 0xA3;
            updateBytes[3] = 0x01;

            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
            {
                updateBytes[4] = 0x51;
            }
            else if (DeviceTypeS == "53")
            {
                updateBytes[4] = 0x53;
            }
            else if (DeviceTypeS == "57")
            {
                updateBytes[4] = 0x57;
            }                

            updateBytes[5] = 0x02;

            //DeviceMacV
            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
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

            updateBytes[18] = SampleSend;

            if (DeviceTypeS == "57")
            {
                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoHigh);
                updateBytes[19] = deviceMacBytes[0];
                updateBytes[20] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoLow);
                updateBytes[21] = deviceMacBytes[0];
                updateBytes[22] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnHigh);
                updateBytes[23] = deviceMacBytes[0];
                updateBytes[24] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnLow);
                updateBytes[25] = deviceMacBytes[0];
                updateBytes[26] = deviceMacBytes[1];

                //CRC：TODO 暂未实现
                updateBytes[27] = 0x00;
                updateBytes[28] = 0x00;

                updateBytes[29] = 0xEC;
            }
            else
            {
                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoHigh);
                updateBytes[19] = deviceMacBytes[0];
                updateBytes[20] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureInfoLow);
                updateBytes[21] = deviceMacBytes[0];
                updateBytes[22] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityInfoHigh);
                updateBytes[23] = deviceMacBytes[0];
                updateBytes[24] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityInfoLow);
                updateBytes[25] = deviceMacBytes[0];
                updateBytes[26] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnHigh);
                updateBytes[27] = deviceMacBytes[0];
                updateBytes[28] = deviceMacBytes[1];


                deviceMacBytes = CommArithmetic.Double_2Bytes(TemperatureWarnLow);
                updateBytes[29] = deviceMacBytes[0];
                updateBytes[30] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityWarnHigh);
                updateBytes[31] = deviceMacBytes[0];
                updateBytes[32] = deviceMacBytes[1];

                deviceMacBytes = CommArithmetic.Double_2Bytes(HumidityWarnLow);
                updateBytes[33] = deviceMacBytes[0];
                updateBytes[34] = deviceMacBytes[1];

                //CRC：TODO 暂未实现
                updateBytes[35] = 0x00;
                updateBytes[36] = 0x00;

                updateBytes[37] = 0xEC;
            }

            //updateBytes[0] = 0xCE;

            return updateBytes;
        }

        public byte[] DeleteData()
        {
            byte[] updateBytes = new byte[19];

            updateBytes[0] = 0xCE;
            updateBytes[1] = 0x0E;
            updateBytes[2] = 0xA4;
            updateBytes[3] = 0x01;

            //兼容M1 和 M1P
            if (DeviceTypeS == "51")
            {
                updateBytes[4] = 0x51;
            }
            else if (DeviceTypeS == "53")
            {
                updateBytes[4] = 0x53;
            }
            else if (DeviceTypeS == "57")
            {
                updateBytes[4] = 0x57;
            }

            updateBytes[5] = 0x02;

            byte[] deviceMacBytes = CommArithmetic.HexStringToByteArray(DeviceMacS);
            updateBytes[6] = deviceMacBytes[0];
            updateBytes[7] = deviceMacBytes[1];
            updateBytes[8] = deviceMacBytes[2];
            updateBytes[9] = deviceMacBytes[3];

            updateBytes[18] = 0xEC;

            return updateBytes;
        }

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
    }
}
