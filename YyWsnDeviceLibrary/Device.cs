using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    /// <summary>
    /// 所有设备的父类，抽象类，不能直接实例化
    /// </summary>
    public abstract class Device
    {

        public int DisplayID { get; set; }

        /// <summary>
        /// 如果设备包含电池，代表电池的电压
        /// </summary>
        public double Volt { get; set; }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 设备的描述
        /// </summary>
        public string Descript { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public byte DeviceTypeB { get; set; }

        /// <summary>
        /// 设备类型，如M1 代号为51
        /// </summary>
        public String DeviceType { get; set; }        

        /// <summary>
        /// 设备的8位MAC地址
        /// </summary>
        public String DeviceMac { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String PrimaryMAC { get; set; }

        /// <summary>
        /// 设备的新的8位MAC地址
        /// </summary>
        public string DeviceNewMAC { get; set; }

        /// <summary>
        /// 设备原始数据
        /// </summary>
        public String SourceData { get; set; }

        public override string ToString()
        {
            return SourceData;
        }

        /// <summary>
        /// 客户识别码
        /// </summary>
        public String ClientID { get; set; }
        
        /// <summary>
        /// 协议版本
        /// </summary>
        public byte ProtocolVersion { get; set; }

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// 软件版本
        /// </summary>
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// Debug
        /// </summary>
       // public byte[] Debug { get; set; }
        public byte[] Debug { get; set; }

        /// <summary>
        /// Debug
        /// </summary>
        public UInt16 DebugT{ get; set; }

        /// <summary>
        /// 类
        /// </summary>
        public byte Category { get; set; }

        /// <summary>
        /// 时间间隔
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Calendar { get; set; }
        
        public byte Pattern { get; set; }
        public byte Bps { get; set; }
        /// <summary>
        /// Sensor的工作模式
        /// </summary>
        public byte WorkFunction { get; set; } //Pattern in protocol
        public byte STP { get; set; }
        /// <summary>
        /// 传输速率
        /// </summary>
        public byte SymbolRate { get; set; }

        /// <summary>
        /// 设备的最后传输日期和时间
        /// </summary>
        public DateTime LastTransforDate { get; set; }

        /// <summary>
        /// 最新数据/历史数据
        /// </summary>
        public byte LastHistory { get; set; }
    }
}
