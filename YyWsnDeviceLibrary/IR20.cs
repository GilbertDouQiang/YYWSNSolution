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

            buf[0] = 0x00;      // ASCII码， '0'

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

        /// <summary>
        /// 读取设备信息
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 身份认证
        /// </summary>
        /// <returns></returns>
        public byte[] TxPkt_Authenticate(byte[] encrypt, string Key)
        {
            byte[] buf = null;
            UInt16 len = 0;

            if (encrypt == null)
            {
                buf = new byte[14];
            }
            else
            {
                buf = new byte[16];
            }

            // 起始位
            buf[len++] = 0xFD;

            // 长度位
            buf[len++] = 0x00;

            // 命令位
            buf[len++] = 0x05;

            // 协议版本
            buf[len++] = 0x01;

            if (encrypt == null)
            {   // 步骤
                buf[len++] = 0x01;

                // 日期和时间
                DateTime ThisCalendar = System.DateTime.Now;
                byte[] ByteBufTmp = MyCustomFxn.DataTimeToByteArray(ThisCalendar);
                buf[len++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[0]);
                buf[len++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[1]);
                buf[len++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[2]);
                buf[len++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[3]);
                buf[len++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[4]);
                buf[len++] = MyCustomFxn.DecimalToBcd(ByteBufTmp[5]);
            }
            else
            {   // 步骤
                buf[len++] = 0x02;

                // 密文
                MD5 aMD5 = new MD5(Key);

                byte[] decrypt = aMD5.Calc(encrypt, (UInt32)encrypt.Length);        // 密文
                if (decrypt == null || decrypt.Length != 16)
                {
                    buf[len++] = 0x00;
                    buf[len++] = 0x00;
                    buf[len++] = 0x00;
                    buf[len++] = 0x00;
                    buf[len++] = 0x00;
                    buf[len++] = 0x00;
                    buf[len++] = 0x00;
                    buf[len++] = 0x00;
                }
                else
                {
                    buf[len++] = decrypt[8];
                    buf[len++] = decrypt[9];
                    buf[len++] = decrypt[10];
                    buf[len++] = decrypt[11];
                    buf[len++] = decrypt[12];
                    buf[len++] = decrypt[13];
                    buf[len++] = decrypt[14];
                    buf[len++] = decrypt[15];
                }
            }          

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

        /// <summary>
        /// 测量一次体温
        /// </summary>
        /// <param name="PassCode"> 认证码 </param>
        /// <param name="tempThrLow"> 正常体温下限，单位：℃ </param>
        /// <param name="tempThrHigh"> 正常体温上限，单位：℃ </param>
        /// <returns></returns>
        public byte[] TxPkt_GetBodyTemp(UInt32 PassCode, double tempThrLow, double tempThrHigh)
        {
            byte[] buf = new byte[17];
            UInt16 len = 0;

            // 起始位
            buf[len++] = 0xFD;

            // 长度位
            buf[len++] = 0x00;

            // 命令位
            buf[len++] = 0x06;

            // 协议版本
            buf[len++] = 0x01;

            // 认证码
            buf[len++] = (byte)((PassCode & 0xFF000000) >> 24);
            buf[len++] = (byte)((PassCode & 0x00FF0000) >> 16);
            buf[len++] = (byte)((PassCode & 0x0000FF00) >> 8);
            buf[len++] = (byte)((PassCode & 0x000000FF) >> 0);

            // 正常体温下限
            Int16 tempThrLow_i = (Int16) Math.Round(tempThrLow * 100.0f);
            buf[len++] = (byte)((tempThrLow_i & 0xFF00) >> 8);
            buf[len++] = (byte)((tempThrLow_i & 0x00FF) >> 0);

            // 正常体温上限
            Int16 tempThrHigh_i = (Int16)Math.Round(tempThrHigh * 100.0f);
            buf[len++] = (byte)((tempThrHigh_i & 0xFF00) >> 8);
            buf[len++] = (byte)((tempThrHigh_i & 0x00FF) >> 0);

            // 保留
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

        /// <summary>
        /// 测量一次温度
        /// </summary>
        /// <param name="PassCode"></param>
        /// <returns></returns>
        public byte[] TxPkt_GetAllTemp(UInt32 PassCode)
        {
            byte[] buf = new byte[15];
            UInt16 len = 0;

            // 起始位
            buf[len++] = 0xFD;

            // 长度位
            buf[len++] = 0x00;

            // 命令位
            buf[len++] = 0x08;

            // 协议版本
            buf[len++] = 0x01;

            // 认证码
            buf[len++] = (byte)((PassCode & 0xFF000000) >> 24);
            buf[len++] = (byte)((PassCode & 0x00FF0000) >> 16);
            buf[len++] = (byte)((PassCode & 0x0000FF00) >> 8);
            buf[len++] = (byte)((PassCode & 0x000000FF) >> 0);

            // 保留
            buf[len++] = 0x00;
            buf[len++] = 0x00;
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

        /// <summary>
        /// 重启设备
        /// </summary>
        /// <returns></returns>
        public byte[] TxPkt_Reset()
        {
            byte[] buf = new byte[15];
            UInt16 len = 0;

            // 起始位
            buf[len++] = 0xFD;

            // 长度位
            buf[len++] = 0x00;

            // 命令位
            buf[len++] = 0x36;

            // 协议版本
            buf[len++] = 0x01;

            // 认证码
            UInt32 PassCode = 0xD6D8C6F4;
            buf[len++] = (byte)((PassCode & 0xFF000000) >> 24);
            buf[len++] = (byte)((PassCode & 0x00FF0000) >> 16);
            buf[len++] = (byte)((PassCode & 0x0000FF00) >> 8);
            buf[len++] = (byte)((PassCode & 0x000000FF) >> 0);

            // 保留
            buf[len++] = 0x00;
            buf[len++] = 0x00;
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

        /// <summary>
        /// 修改设备的配置
        /// </summary>
        /// <param name="NewDeviceMac"></param>
        /// <param name="NewHwRevision"></param>
        /// <param name="Mode"></param>
        /// <param name="objTempCom"></param>
        /// <returns></returns>
        public byte[] TxPkt_WriteCfg(UInt32 NewDeviceMac, UInt32 NewHwRevision, byte Mode, double objTempCom)
        {
            byte[] buf = new byte[22];
            UInt16 len = 0;

            // 起始位
            buf[len++] = 0xFD;

            // 长度位
            buf[len++] = 0x00;

            // 命令位
            buf[len++] = 0x37;

            // 协议版本
            buf[len++] = 0x01;

            // 认证码
            UInt32 PassCode = 0xC5E4D6C3;
            buf[len++] = (byte)((PassCode & 0xFF000000) >> 24);
            buf[len++] = (byte)((PassCode & 0x00FF0000) >> 16);
            buf[len++] = (byte)((PassCode & 0x0000FF00) >> 8);
            buf[len++] = (byte)((PassCode & 0x000000FF) >> 0);

            // Device Mac
            buf[len++] = (byte)((NewDeviceMac & 0xFF000000) >> 24);
            buf[len++] = (byte)((NewDeviceMac & 0x00FF0000) >> 16);
            buf[len++] = (byte)((NewDeviceMac & 0x0000FF00) >> 8);
            buf[len++] = (byte)((NewDeviceMac & 0x000000FF) >> 0);

            // Hardware Revision
            buf[len++] = (byte)((NewHwRevision & 0xFF000000) >> 24);
            buf[len++] = (byte)((NewHwRevision & 0x00FF0000) >> 16);
            buf[len++] = (byte)((NewHwRevision & 0x0000FF00) >> 8);
            buf[len++] = (byte)((NewHwRevision & 0x000000FF) >> 0);

            // Mode
            buf[len++] = Mode;

            // object温度补偿
            Int16 objTempCom_i = (Int16)Math.Round(objTempCom * 100.0f);
            buf[len++] = (byte)((objTempCom_i & 0xFF00) >> 8);
            buf[len++] = (byte)((objTempCom_i & 0x00FF) >> 0);

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

        /// <summary>
        /// 修改密钥
        /// </summary>
        /// <param name="Key"> 长度是16的字节数组 </param>
        /// <returns></returns>
        public byte[] TxPkt_WriteKey(byte[] Key)
        {
            if(Key == null)
            {
                return null;
            }

            if(Key.Length < 16)
            {
                return null;
            }

            byte[] buf = new byte[29];
            UInt16 len = 0;

            // 起始位
            buf[len++] = 0xFD;

            // 长度位
            buf[len++] = 0x00;

            // 命令位
            buf[len++] = 0x49;

            // 协议版本
            buf[len++] = 0x01;

            // 认证码
            UInt32 PassCode = 0xC3DCD4BF;
            buf[len++] = (byte)((PassCode & 0xFF000000) >> 24);
            buf[len++] = (byte)((PassCode & 0x00FF0000) >> 16);
            buf[len++] = (byte)((PassCode & 0x0000FF00) >> 8);
            buf[len++] = (byte)((PassCode & 0x000000FF) >> 0);

            // 密钥
            for(int iX = 0; iX < 16; iX++)
            {
                buf[len++] = Key[iX];
            }

            // 保留
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
