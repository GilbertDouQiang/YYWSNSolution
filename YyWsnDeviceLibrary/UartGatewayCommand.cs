using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace YyWsnDeviceLibrary
{
    public class UartGatewayCommand
    {
        public byte[] ReadInfo()
        {

            return CommArithmetic.HexStringToByteArray("CA CA 05 01 00 00 00 00 00 00 AC AC");
        }

        public byte[] UpdateInfo(UartGateway device)
        {
            byte[] command = new byte[29];
            command[0] = 0xCA;
            command[1] = 0xCA;
            command[2] = 0x16;
            command[3] = 0x02;

            byte[] oldmac = CommArithmetic.HexStringToByteArray(device.DeviceMacS);
            command[4] = oldmac[0];
            command[5] = oldmac[1];
            command[6] = oldmac[2];
            command[7] = oldmac[3];

            byte[] newmac = CommArithmetic.HexStringToByteArray(device.DeviceNewMAC);
            command[8] = newmac[0];
            command[9] = newmac[1];
            command[10] = newmac[2];
            command[11] = newmac[3];

            byte[] hardVersion = CommArithmetic.HexStringToByteArray(device.HwVersionS);
            command[12] = hardVersion[0];
            command[13] = hardVersion[1];
            command[14] = hardVersion[2];
            command[15] = hardVersion[3];

            byte[] clientid = CommArithmetic.HexStringToByteArray(device.CustomerS);
            command[16] = clientid[0];
            command[17] = clientid[1];

            command[18] = (byte)(device.DebugV / 256);
            command[19] = (byte)(device.DebugV % 256);

            command[20] = device.Category;

            byte[] interval = CommArithmetic.Int16_2Bytes(device.Interval);
            command[21] = interval[0];
            command[22] = interval[1];

            command[23] = (byte)(device.WorkFunction);
            command[24] = (byte)(device.SymbolRate);

            command[27] = 0xAC;
            command[28] = 0xAC;

            //临时调试用
            string commandString = CommArithmetic.ToHexString(command);

            return command;

        }

        public byte[] DateTimeSyncchronization(UartGateway device)
        {

            byte[] command = new byte[21];
            command[0] = 0xCA;
            command[1] = 0xCA;
            command[2] = 0x0E;
            command[3] = 0x04;

            //byte[] oldmac = CommArithmetic.HexStringToByteArray(device.DeviceMacS);
            command[4] = 0x00;
            command[5] = 0x00;
            command[6] = 0x00;
            command[7] = 0x00;

            command[8] = 0x08;
            command[9] = 0x15;
            command[10] = 0x06;


            
            DateTime dt = DateTime.Now;
            byte[] decodeDate = CommArithmetic.EncodeDateTime(dt);
            command[11] = decodeDate[0];
            command[12] = decodeDate[1];
            command[13] = decodeDate[2];
            command[14] = decodeDate[3];
            command[15] = decodeDate[4];
            command[16] = decodeDate[5];



            command[17] = 0x00;
            command[18] = 0x00;
            command[19] = 0xAC;
            command[20] = 0xAC;



            return command;
        }

        public byte[] ReadData(int Timeout)
        {

            return CommArithmetic.HexStringToByteArray("CA CA 08 08 00 00 00 00 00 75 30 00 00 AC AC");
        }


    }
}
