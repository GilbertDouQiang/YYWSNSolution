﻿using System;
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
        /// 设备代号，如M1 代号为51
        /// </summary>
        public String DeviceID { get; set; }

        /// <summary>
        /// 设备的8位MAC地址
        /// </summary>
        public String DeviceMac { get; set; }

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

        public byte ProtocolVersion { get; set; }

    }
}
