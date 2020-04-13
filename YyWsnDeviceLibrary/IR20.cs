using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class IR20 : Sensor
    {
        /**************************************
         * 属性
         * ************************************/

        /// <summary>
        /// 数据包的类型
        /// </summary>
        private DataPktType dataPktType { get; set; }       // 外部只读

        /// <summary>
        /// 体温，两位小数，单位：℃
        /// </summary>
        public double TempBody { get; set; }

        /// <summary>
        /// 物体表面温度，两位小数，单位：℃
        /// </summary>
        public double TempObj { get; set; }

        /// <summary>
        /// 环境温度，两位小数，单位：℃
        /// </summary>
        public double TempAmb { get; set; }


        /**************************************
         * 方法
         * ************************************/

        /// <summary>
        /// 获取设备类型编码
        /// </summary>
        /// <returns></returns>
        static public byte GetDeviceType()
        {
            return (byte)DeviceType.IR20;
        }

        /// <summary>
        /// 判断输入的设备类型是否与本设备类型一致
        /// </summary>
        /// <param name="iDeviceType"></param>
        /// <returns></returns>
        static public bool isDeviceType(byte iDeviceType)
        {
            if (iDeviceType == GetDeviceType())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取数据包的类型
        /// </summary>
        /// <returns></returns>
        public DataPktType GetDataPktType()
        {
            return dataPktType;
        }

        public IR20() { }

        public byte[] TxPkt_Ping()
        {
            byte[] buf = new byte[1];

            buf[0] = 0x30;      // ASCII码， '0'

            return buf;
        }

        public byte[] TxPkt_SwitchCmdMode(bool StringCmdMode)
        {
            byte[] buf = null;

            string StrCmd = "HyperWSN_Debug";
            string HexCmd = "HyperWSN_Release";

            if (StringCmdMode == false)
            {
                buf = Encoding.UTF8.GetBytes(HexCmd);
            }
            else
            {
                buf = Encoding.UTF8.GetBytes(StrCmd);
            }

            return buf;
        }

        public byte[] TxPkt_ReadCfg()
        {
            byte[] buf = new byte[9];
            UInt16 len = 0;

            // 起始位
            buf[len++] = 0xFD;

            // 长度位
            buf[len++] = 0x00;

            // 命令位
            buf[len++] = 0x01;

            // 协议版本
            buf[len++] = 0x01;

            // 保留位
            buf[len++] = 0x00;
            buf[len++] = 0x00;

            // CRC16
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, buf, 2, (UInt16)(len - 2));
            buf[len++] = (byte)((crc & 0xFF00) >> 8);
            buf[len++] = (byte)((crc & 0x00FF) >> 0);

            // 结束位
            buf[len++] = 0xDF;

            // 重写长度位
            buf[1] = (byte)(len - 5);

            return buf;
        }

    }
}
