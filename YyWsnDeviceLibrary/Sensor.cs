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
        public int TXPower { get; set; }

        /// <summary>
        /// 采集和发送的倍数，最小为1，最大为8
        /// </summary>
        public int TXTimers { get; set; }

        public int Frequency { get; set; }



       

        
    }
}
