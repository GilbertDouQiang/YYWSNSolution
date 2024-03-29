﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class Sensor : Device
    {
        public int SensorSN { get; set; }

        public int PSensorSN { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public UInt16 Serial { get; set; }

        public int CSensorSN { get; set; }

        public byte Error { get; set; }

        public byte SendOK { get; set; }

        /// <summary>
        /// 采集时间
        /// </summary>
        public DateTime SensorCollectTime { get; set; }

        /// <summary>
        /// 传输时间
        /// </summary>
        public DateTime SensorTransforTime { get; set; }

        /// <summary>
        /// 网关时间
        /// </summary>
        public DateTime GWTime { get; set; }

        /// <summary>
        /// 系统时间
        /// </summary>
        public DateTime SystemTime { get; set; }

        /// <summary>
        /// 接收信号强度，单位：dBm
        /// </summary>
        public double RSSI { get; set; }

        /// <summary>
        /// 发射功率，有符号数，单位：dBm
        /// </summary>
        public Int16 TxPower { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        /// <summary>
        /// 采集/发送的倍数
        /// </summary>
        public byte SampleSend { get; set; }

        /// <summary>
        /// 频段，无符号数
        /// </summary>
        public byte Channel { get; set; }

        public string FlashID { get; set; }

        public UInt32 FlashFront { get; set; }

        public UInt32 FlashRear { get; set; }

        public UInt32 FlashQueueLength { get; set; }

        public byte MaxLength { get; set; }

        /// <summary>
        /// 片内温度
        /// </summary>
        public double ICTemperature { get; set; }

        public byte monTemp { get; set; }



        /// <summary>
        /// 设置发射功率
        /// </summary>
        /// <param name="txPower"></param>
        public void SetTxPower(byte txPower)
        {
            if (txPower >= 0x80)
            {
                TxPower = (Int16)(txPower - 256);
            }
            else
            {
                TxPower = (Int16)txPower;
            }
        }
    }
}
