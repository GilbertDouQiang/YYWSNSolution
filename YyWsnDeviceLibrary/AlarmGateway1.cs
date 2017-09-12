using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    /// <summary>
    /// 一体机对象
    /// </summary>
    public class AlarmGateway1:Gateway
    {
        public AlarmGateway1()
        {

        }

        public AlarmGateway1(byte[] SourceData)
        {
            //有效性验证，确定是合理的数据包
            DeviceID = "59";
            Name = "AlarmGateway-1";
            DeviceMac = CommArithmetic.DecodeMAC(SourceData, 8);
            LastTransforDate = CommArithmetic.DecodeDateTime(SourceData, 12);
            Volt = Math.Round(Convert.ToDouble((SourceData[18] * 256 + SourceData[19])) / 1000, 2);
            SoftwareVersion = CommArithmetic.DecodeClientID(SourceData, 20);
            ClientID = CommArithmetic.DecodeClientID(SourceData, 22);
            RAMCoutnt = SourceData[24];
            ROMCount = SourceData[25] * 65536 + SourceData[26] * 256 + SourceData[27];
            CSQ = SourceData[30];
            ReceivedSensorCount = SourceData[32];
            BindingSensorCount = SourceData[33];

            SimcardNum = CommArithmetic.DecodeMAC(SourceData, 35);

            LastTransforNumber = SourceData[40] * 65536 + SourceData[41] * 256 + SourceData[42];

            LastTransforStatus = SourceData[44] * 256 + SourceData[45];









        }
    }
}
