using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class Gateway:Device
    {
        public int RAMCoutnt { get; set; }

        public int ROMCount { get; set; }

        public int FrontPoint { get; set; }

        public int RearPoint { get; set; }

        /// <summary>
        /// GSM 信号强度</br>
        /// 一般取值10 ~31，信号强度越大越好
        /// </summary>
        public byte CSQ { get; set; }

        /// <summary>
        /// 网关收到并转发传感器的数量
        /// </summary>
        public int ReceivedSensorCount { get; set; }


        /// <summary>
        /// 网关绑定传感器的数量
        /// </summary>
        public int BindingSensorCount { get; set; }


        /// <summary>
        /// Sim卡，最后8位数字
        /// </summary>
        public string SimcardNum { get; set; }

        /// <summary>
        /// 上次成功传输的数量
        /// </summary>
        public int LastTransforNumber { get; set; }

        /// <summary>
        /// 上次传输的状态，保留位
        /// </summary>
        public int LastTransforStatus { get; set; }




        
    }
}
