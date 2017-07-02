using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class Sensor:Device
    {
        public int SensorSN { get; set; }

        public DateTime SensorCollectTime { get; set; }
       
        public DateTime SensorTransforTime { get; set; }

        public double RSSI { get; set; }


       

        
    }
}
