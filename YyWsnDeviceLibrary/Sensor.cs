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
        public byte TXPower { get; set; }

        /// <summary>
        /// 采集和发送的倍数，最小为1，最大为8
        /// </summary>
        public byte TXTimers { get; set; }

        public byte Frequency { get; set; }

        public string FlashID { get; set; }

        public Int32 FlashFront { get; set; }
        public Int32 FlashRear { get; set; }
        public Int32 FlashQueueLength { get; set; }



       

        
    }
}
