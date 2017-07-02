using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnCommunicatonLibrary
{
    public class SerialPortEventArgs:EventArgs
    {


        public SerialPortEventArgs()
        {
            
        }

        public byte[] ReceivedBytes { get; set; }

    }
}
