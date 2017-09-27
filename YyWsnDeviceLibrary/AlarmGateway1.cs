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
            TransStrategy = SourceData[24];
            RAMCoutnt = SourceData[25];
            ROMCount = SourceData[26] * 65536 + SourceData[27] * 256 + SourceData[28];
            CSQ = SourceData[31];
            ReceivedSensorCount = SourceData[33];
            BindingSensorCount = SourceData[34];

            SimcardNum = CommArithmetic.DecodeMAC(SourceData, 36);

            LastTransforNumber = SourceData[41] * 65536 + SourceData[42] * 256 + SourceData[43];

            LastTransforStatus = SourceData[45] * 256 + SourceData[46];









        }

        public byte TransStrategy { get; set; }
    }
}
