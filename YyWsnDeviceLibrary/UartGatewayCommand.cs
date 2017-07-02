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

            byte[] oldmac =CommArithmetic.HexStringToByteArray(  device.DeviceMac);
            command[4] = oldmac[0];
            command[5] = oldmac[1];
            command[6] = oldmac[2];
            command[7] = oldmac[3];

            byte[] newmac = CommArithmetic.HexStringToByteArray(device.DeviceNewMAC);
            command[8] = newmac[0];
            command[9] = newmac[1];
            command[10] = newmac[2];
            command[11] = newmac[3];

            byte[] hardVersion = CommArithmetic.HexStringToByteArray(device.HardwareVersion);
            command[12] = hardVersion[0];
            command[13] = hardVersion[1];
            command[14] = hardVersion[2];
            command[15] = hardVersion[3];

            byte[] clientid = CommArithmetic.HexStringToByteArray(device.ClientID);
            command[16] = clientid[0];
            command[17] = clientid[1];

            command[18] = device.Debug[0];
            command[19] = device.Debug[1];

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
    }
}
