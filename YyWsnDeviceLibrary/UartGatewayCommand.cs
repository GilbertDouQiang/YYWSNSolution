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
    }
}
