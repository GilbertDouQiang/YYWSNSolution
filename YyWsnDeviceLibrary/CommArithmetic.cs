using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class CommArithmetic
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string s)
        {
            try
            {
                s = s.Replace(" ", "");
                byte[] buffer = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                return buffer;
            }
            catch (Exception)
            {

                return null;
            }
            
        }

        public static byte[] HexStringToByteArray(string s,int start)
        {
            try
            {
                s = s.Substring(start);
                s = s.Replace(" ", "");
                byte[] buffer = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
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

        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
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
            }
            return hexString;
        }



        public static DateTime DecodeDateTime(byte[] source, int start)
        {
            DateTime dt;
            byte[] tempDate = new byte[6];

            if (source != null && source.Length > start + 5)
            {
                tempDate[0] = source[start];
                tempDate[1] = source[start + 1];
                tempDate[2] = source[start + 2];
                tempDate[3] = source[start + 3];
                tempDate[4] = source[start + 4];
                tempDate[5] = source[start + 5];
            }


            string strDate = ToHexString(tempDate);




            try
            {
                dt = DateTime.ParseExact(strDate, "yy MM dd HH mm ss ", System.Globalization.CultureInfo.CurrentCulture);

            }
            catch (Exception)
            {
                dt = DateTime.ParseExact("20010101", "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
            }


            return dt;

        }

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
                result[1] = 1;

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
                tempCalc -= 65536;
            return  Math.Round((Convert.ToDouble(tempCalc) / 100), 2);

        }

        public static double DecodeHumidity(byte[] SourceData, int Start)
        {
            return Math.Round(Convert.ToDouble((SourceData[Start] * 256 + SourceData[Start+1])) / 100, 2);

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
    }
}
