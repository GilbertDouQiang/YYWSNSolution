using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class CommArithmetic
    {
        public static int Byte2Int(byte[] Sourcebytes,int Position,int Length)
        {
            try
            {
                if (Length == 2)
                {
                    return Sourcebytes[Position] * 256 + Sourcebytes[Position + 1];
                }
                else if (Length == 3)
                {
                    return Sourcebytes[Position] * 65536 + Sourcebytes[Position + 1] * 256 + Sourcebytes[Position + 2];
                }
            }
            catch (Exception)
            {

                
            }
            
            return 0;

        }

        public static byte[] HexStringToByteArray(string s)
        {
            try
            {
                s = s.Replace(" ", "");

                byte[] buffer = new byte[s.Length / 2];

                for (int i = 0; i < s.Length; i += 2)
                {
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                }

                return buffer;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        public static byte[] HexStringToByteArray(string s, int start)
        {
            try
            {
                s = s.Substring(start);
                s = s.Replace(" ", "");

                byte[] buffer = new byte[s.Length / 2];

                for (int i = 0; i < s.Length; i += 2)
                {
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                }

                return buffer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 将数组中的MAC地址解析出来
        /// </summary>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static string DecodeMAC(byte[] source, int start)
        {
            byte[] mac = new byte[4];
            if (source != null && source.Length > start + 3)
            {
                mac[0] = source[start];
                mac[1] = source[start + 1];
                mac[2] = source[start + 2];
                mac[3] = source[start + 3];
                return ToHexString(mac);
            }

            return null;
        }

        public static UInt32 ByteBuf_to_UInt32(byte[] source, int start)
        {
            return ((UInt32)source[start] << 24) | ((UInt32)source[start + 1] << 16) | ((UInt32)source[start + 2] << 8) | ((UInt32)source[start + 3] << 0);
        }

        public static UInt16 ByteBuf_to_UInt16(byte[] source, int start)
        {
            return (UInt16)(((UInt16)source[start] << 8) | ((UInt16)source[start + 1] << 0));
        }

        /// <summary>
        /// 将一个字节按照有符号数来解析
        /// </summary>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static Int16 ByteBuf_to_Int8(byte[] source, int start)
        {
            Int16 val = (Int16)source[start];

            if (val >= 0x80)
            {
                val -= 0x100;
            }

            return val;
        }

        public static string ByteBuf_to_HexStr(byte[] source, int start, int num)
        {
            if (num == 0 || start >= source.Length || (start + num) > source.Length)
            {
                return string.Empty;
            }

            string hexStr = string.Empty;

            for (int iX = 0; iX < num; iX++)
            {
                hexStr += source[start + iX].ToString("X2") + " ";
            }

            hexStr = hexStr.TrimEnd();      // 删除末尾的空格

            return hexStr;
        }

        /// <summary>
        /// 将数组中的LastHistory解析出来
        /// </summary>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static byte DecodeLastHistory(byte[] source, int start) {

            if(source == null || source.Length < start) {
                return 0;
            }

            return source[start];
        }
       
        public static string DecodeClientID(byte[] source, int start)
        {
            byte[] clientID = new byte[2];
            if (source != null && source.Length > start + 1)
            {
                clientID[0] = source[start];
                clientID[1] = source[start + 1];
                
                return ToHexString(clientID);
            }

            return null;
        }

        public static string DecodeFlashID(byte[] source, int start) {
            byte[] flashid = new byte[2];

            if (source != null && source.Length > start + 1)
            {
                flashid[0] = source[start];
                flashid[1] = source[start + 1];

                return ToHexString(flashid);
            }

            return null;
        }

        public static string ToHexString(byte[] bytes) 
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2") + " ");
                }

                hexString = strB.ToString();

                hexString = hexString.TrimEnd();
            }

            return hexString;
        }

        public static string ToHexString(byte[] bytes, int StartIndex, int length) 
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < length; i++)
                {
                    strB.Append(bytes[StartIndex + i].ToString("X2") + " ");
                }

                hexString = strB.ToString();

                hexString = hexString.TrimEnd();
            }

            return hexString;
        }

        public static DateTime DecodeDateTime(byte[] source, int start)
        {
            DateTime dt;

            string dtStr = ByteBuf_to_HexStr(source, start, 6);

            try
            {
                if (dtStr == string.Empty || dtStr == "00 00 00 00 00 00")
                {
                    dt = DateTime.ParseExact("20010101", "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                }
                else
                {
                    dt = DateTime.ParseExact(dtStr, "yy MM dd HH mm ss", System.Globalization.CultureInfo.CurrentCulture);
                }
            }
            catch (Exception)
            {
                dt = DateTime.ParseExact("20010101", "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            }

            return dt;
        }

        /// <summary>
        /// 将2个字节的数字转换成字节数组</br>
        /// 适用于Interval的结算
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] Int16_2Bytes(int source)
        {
            byte[] result = new byte[2];
            if (source < 256)
            {
                result[0] = 0;
                result[1] = (byte)source;
            }
            else
            {
                result[0] = (byte)(source / 256);
                result[1] =  (byte)(source-result[0]*256);
            }

            return result;
        }

        /// <summary>
        /// 将2个字节的浮点数转换成字节数组</br>
        /// 适用于温湿度各种阈值的反相的解算
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] Double_2Bytes(double source)
        {
            source = source * 100;
            if (source<0)
            {
                source += 65536;
            }

            byte[] result = new byte[2];
            if (source < 256)
            {
                result[0] = 0;
                result[1] = (byte)source;
            }
            else
            {
                result[0] = (byte)(source / 256);
                result[1] = (byte)(source - result[0] * 256);

            }


            return result;
        }

        /// <summary>
        /// 将温度16进制字节数组，转换为浮点数
        /// </summary>
        /// <param name="SourceData"></param>
        /// <param name="Start"></param>
        /// <returns></returns>
        public static double DecodeTemperature(byte[] SourceData,int Start)
        {
            int tempCalc = SourceData[Start] * 256 + SourceData[Start+1];

            if (tempCalc >= 0x8000)
            {
                tempCalc -= 65536;
            }
               
            return  Math.Round((Convert.ToDouble(tempCalc) / 100), 2);
        }

        public static double DecodeHumidity(byte[] SourceData, int Start)
        {
            return Math.Round(Convert.ToDouble((SourceData[Start] * 256 + SourceData[Start+1])) / 100, 2);
        }

        public static double DecodeVoltage(byte[] SourceData, int Start)
        {
            UInt16 voltInt = ByteBuf_to_UInt16(SourceData, Start);

            if (voltInt >= 0x8000)
            {   // 接入了充电器              
                voltInt &= 0x7FFF;
            }

            return Math.Round((double)voltInt / 1000.0f, 2);
        }


        public static double DecodeSensorVoltage(byte[] SourceData, int Start)
        {
            return DecodeVoltage(SourceData, Start);
        }

        public static  byte[] EncodeDateTime(DateTime dateTime)
        {            
            string dateString = (dateTime.Year - 2000).ToString() ;
            if (dateTime.Month < 10)
            {
                dateString += " 0" + dateTime.Month;
            }
            else
            {
                dateString += " " + dateTime.Month;
            }

            if (dateTime.Day < 10)
            {
                dateString += " 0" + dateTime.Day;
            }
            else
            {
                dateString += " " + dateTime.Day;
            }


            if (dateTime.Hour < 10)
            {
                dateString += " 0" + dateTime.Hour;
            }
            else
            {
                dateString += " " + dateTime.Hour;
            }

            if (dateTime.Minute < 10)
            {
                dateString += " 0" + dateTime.Minute;
            }
            else
            {
                dateString += " " + dateTime.Minute;
            }

            if (dateTime.Second < 10)
            {
                dateString += " 0" + dateTime.Second;
            }
            else
            {
                dateString += " " + dateTime.Second;
            }

            byte[] datetimeByte = CommArithmetic.HexStringToByteArray(dateString);
            
            return datetimeByte;
        }

        public static double SHT20Temperature(byte a, byte b)
        {
            //double a = ((Convert.ToInt32(buf[bufRef + 5].ToString("X2"), 16) * 256 + Convert.ToInt32(buf[bufRef + 6].ToString("X2"), 16)) / 65536) * 175.72 - 46.85;
            double c = Math.Round(( -46.85 + 175.72 * (a * 256 + b) / 65536),2);

            return c;
        }

        public static double SHT20Humidity(byte a, byte b)
        {
            //double a = ((Convert.ToInt32(buf[bufRef + 5].ToString("X2"), 16) * 256 + Convert.ToInt32(buf[bufRef + 6].ToString("X2"), 16)) / 65536) * 175.72 - 46.85;
            double c = Math.Round((0 - 6.0 + 125.0 * (a * 256.0 + b) / 65536),2);

            return c;
        }

        public static double SHT20Voltage(byte a, byte b)
        {
            double c = Math.Round(((a * 256 + b) * 4 / (double)1023),2);
            return c;
        }

        public static byte DecodeACPower(byte a)
        {
            if (a >=0x80)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static string FormatIPAddress(string IPString)
        {
            string[] part = IPString.Split('.');
            string result="";
            foreach (var item in part)
            {
                result += item.ToString().PadLeft(3,'0')+".";

            }

            return result;
        }

    }
}
