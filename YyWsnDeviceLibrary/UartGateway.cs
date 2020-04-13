using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class UartGateway : Gateway
    {
        public UartGateway()
        {

        }

        public UartGateway(byte[] SourceData)
        {
            Name = "UartGateway";
            //DeviceID = SourceData[3].ToString("X2");
            DeviceMacS = CommArithmetic.DecodeMAC(SourceData, 4);
            CustomerS = CommArithmetic.DecodeClientID(SourceData, 14);
            Pattern = SourceData[27];

            HwRevisionS = CommArithmetic.DecodeMAC(SourceData, 8);
            SwRevisionS = CommArithmetic.DecodeClientID(SourceData, 12);
            DebugV = (UInt16)(SourceData[16] * 256 + SourceData[17]);
            Category = SourceData[18];
            Interval = (UInt16) (SourceData[19] * 256 + SourceData[20]);
            Bps = SourceData[28];
            Calendar = CommArithmetic.DecodeDateTime(SourceData, 21);
            FrontPoint = SourceData[30] * 256 + SourceData[31];
            RearPoint = SourceData[32] * 256 + SourceData[32];
            RAMCoutnt = SourceData[29];
            ROMCount = SourceData[34] * 256 + SourceData[35];
        }
    }
}
