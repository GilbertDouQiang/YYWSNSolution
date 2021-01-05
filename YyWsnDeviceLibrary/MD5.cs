using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary
{
    public class MD5
    {

        byte[] PADDING = { 0x80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                   0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                   0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                   0, 0, 0, 0, 0 };

        UInt32[] count = { 0, 0 };
        UInt32[] state = { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476 };
        byte[] buffer = new byte[64];

        /// <summary>
        /// 使用默认密钥：67452301EFCDAB8998BADCFE10325476
        /// </summary>
        public MD5()
        {
            state[0] = 0x67452301;
            state[1] = 0xEFCDAB89;
            state[2] = 0x98BADCFE;
            state[3] = 0x10325476;
        }

        /// <summary>
        /// 使用指定密钥；内部自动去掉空格、逗号、回车和换行；
        /// </summary>
        /// <param name="keyStr">16进制数的字符串，长度是32个字符，编码：ASCII；</param>
        public MD5(string keyStr)
        {
            byte[] keyByte = MyCustomFxn.HexStringToByteArray(keyStr);
            if (keyByte == null || keyByte.Length < 16)
            {
                return;         // 指定密钥格式错误，使用默认密钥
            }
            else
            {
                state[0] = ((UInt32)keyByte[0] << 24) | ((UInt32)keyByte[1] << 16) | ((UInt32)keyByte[2] << 8) | ((UInt32)keyByte[3] << 0);
                state[1] = ((UInt32)keyByte[4] << 24) | ((UInt32)keyByte[5] << 16) | ((UInt32)keyByte[6] << 8) | ((UInt32)keyByte[7] << 0);
                state[2] = ((UInt32)keyByte[8] << 24) | ((UInt32)keyByte[9] << 16) | ((UInt32)keyByte[10] << 8) | ((UInt32)keyByte[11] << 0);
                state[3] = ((UInt32)keyByte[12] << 24) | ((UInt32)keyByte[13] << 16) | ((UInt32)keyByte[14] << 8) | ((UInt32)keyByte[15] << 0);
            }           
        }

        private UInt32 F(UInt32 x, UInt32 y, UInt32 z)
        {
            return ((x & y) | (~x & z));
        }

        private UInt32 G(UInt32 x, UInt32 y, UInt32 z)
        {
            return ((x & z) | (y & ~z));
        }

        private UInt32 H(UInt32 x, UInt32 y, UInt32 z)
        {
            return (x ^ y ^ z);
        }

        private UInt32 I(UInt32 x, UInt32 y, UInt32 z)
        {
            return (y ^ (x | ~z));
        }

        private UInt32 ROTATE_LEFT(UInt32 x, UInt32 n)
        {
            return ((x << (int)n) | (x >> (int)(32 - n)));
        }

        private void FF(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, UInt32 s, UInt32 ac)
        {
            a += F(b, c, d) + x + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        private void GG(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, UInt32 s, UInt32 ac)
        {
            a += G(b, c, d) + x + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        private void HH(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, UInt32 s, UInt32 ac)
        {
            a += H(b, c, d) + x + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        private void II(ref UInt32 a, UInt32 b, UInt32 c, UInt32 d, UInt32 x, UInt32 s, UInt32 ac)
        {
            a += I(b, c, d) + x + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="input"></param>
        /// <param name="len"></param>
        private void MD5Decode(UInt32[] output, UInt32 outputIndex, byte[] input, UInt32 inputIndex, UInt32 len)
        {
            UInt32 i = 0, j = 0;
            while (j < len)
            {
                output[outputIndex + i] = (UInt32)((input[inputIndex + j]) | (input[inputIndex + j + 1] << 8) | (input[inputIndex + j + 2] << 16)
                        | (input[inputIndex + j + 3] << 24));
                i++;
                j += 4;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stateIndex"></param>
        /// <param name="block">长度是64个字节</param>
        /// <param name="blockIndex"></param>
        private void MD5Transform(UInt32[] state, UInt32 stateIndex, byte[] block, UInt32 blockIndex)
        {
            UInt32 a = state[0];
            UInt32 b = state[1];
            UInt32 c = state[2];
            UInt32 d = state[3];
            UInt32[] x = new UInt32[64];

            MD5Decode(x, 0, block, 0, 64);
            FF(ref a, b, c, d, x[0], 7, 0xd76aa478); /* 1 */
            FF(ref d, a, b, c, x[1], 12, 0xe8c7b756); /* 2 */
            FF(ref c, d, a, b, x[2], 17, 0x242070db); /* 3 */
            FF(ref b, c, d, a, x[3], 22, 0xc1bdceee); /* 4 */
            FF(ref a, b, c, d, x[4], 7, 0xf57c0faf); /* 5 */
            FF(ref d, a, b, c, x[5], 12, 0x4787c62a); /* 6 */
            FF(ref c, d, a, b, x[6], 17, 0xa8304613); /* 7 */
            FF(ref b, c, d, a, x[7], 22, 0xfd469501); /* 8 */
            FF(ref a, b, c, d, x[8], 7, 0x698098d8); /* 9 */
            FF(ref d, a, b, c, x[9], 12, 0x8b44f7af); /* 10 */
            FF(ref c, d, a, b, x[10], 17, 0xffff5bb1); /* 11 */
            FF(ref b, c, d, a, x[11], 22, 0x895cd7be); /* 12 */
            FF(ref a, b, c, d, x[12], 7, 0x6b901122); /* 13 */
            FF(ref d, a, b, c, x[13], 12, 0xfd987193); /* 14 */
            FF(ref c, d, a, b, x[14], 17, 0xa679438e); /* 15 */
            FF(ref b, c, d, a, x[15], 22, 0x49b40821); /* 16 *//* Round 2 */
            GG(ref a, b, c, d, x[1], 5, 0xf61e2562); /* 17 */
            GG(ref d, a, b, c, x[6], 9, 0xc040b340); /* 18 */
            GG(ref c, d, a, b, x[11], 14, 0x265e5a51); /* 19 */
            GG(ref b, c, d, a, x[0], 20, 0xe9b6c7aa); /* 20 */
            GG(ref a, b, c, d, x[5], 5, 0xd62f105d); /* 21 */
            GG(ref d, a, b, c, x[10], 9, 0x2441453); /* 22 */
            GG(ref c, d, a, b, x[15], 14, 0xd8a1e681); /* 23 */
            GG(ref b, c, d, a, x[4], 20, 0xe7d3fbc8); /* 24 */
            GG(ref a, b, c, d, x[9], 5, 0x21e1cde6); /* 25 */
            GG(ref d, a, b, c, x[14], 9, 0xc33707d6); /* 26 */
            GG(ref c, d, a, b, x[3], 14, 0xf4d50d87); /* 27 */
            GG(ref b, c, d, a, x[8], 20, 0x455a14ed); /* 28 */
            GG(ref a, b, c, d, x[13], 5, 0xa9e3e905); /* 29 */
            GG(ref d, a, b, c, x[2], 9, 0xfcefa3f8); /* 30 */
            GG(ref c, d, a, b, x[7], 14, 0x676f02d9); /* 31 */
            GG(ref b, c, d, a, x[12], 20, 0x8d2a4c8a); /* 32 *//* Round 3 */
            HH(ref a, b, c, d, x[5], 4, 0xfffa3942); /* 33 */
            HH(ref d, a, b, c, x[8], 11, 0x8771f681); /* 34 */
            HH(ref c, d, a, b, x[11], 16, 0x6d9d6122); /* 35 */
            HH(ref b, c, d, a, x[14], 23, 0xfde5380c); /* 36 */
            HH(ref a, b, c, d, x[1], 4, 0xa4beea44); /* 37 */
            HH(ref d, a, b, c, x[4], 11, 0x4bdecfa9); /* 38 */
            HH(ref c, d, a, b, x[7], 16, 0xf6bb4b60); /* 39 */
            HH(ref b, c, d, a, x[10], 23, 0xbebfbc70); /* 40 */
            HH(ref a, b, c, d, x[13], 4, 0x289b7ec6); /* 41 */
            HH(ref d, a, b, c, x[0], 11, 0xeaa127fa); /* 42 */
            HH(ref c, d, a, b, x[3], 16, 0xd4ef3085); /* 43 */
            HH(ref b, c, d, a, x[6], 23, 0x4881d05); /* 44 */
            HH(ref a, b, c, d, x[9], 4, 0xd9d4d039); /* 45 */
            HH(ref d, a, b, c, x[12], 11, 0xe6db99e5); /* 46 */
            HH(ref c, d, a, b, x[15], 16, 0x1fa27cf8); /* 47 */
            HH(ref b, c, d, a, x[2], 23, 0xc4ac5665); /* 48 *//* Round 4 */
            II(ref a, b, c, d, x[0], 6, 0xf4292244); /* 49 */
            II(ref d, a, b, c, x[7], 10, 0x432aff97); /* 50 */
            II(ref c, d, a, b, x[14], 15, 0xab9423a7); /* 51 */
            II(ref b, c, d, a, x[5], 21, 0xfc93a039); /* 52 */
            II(ref a, b, c, d, x[12], 6, 0x655b59c3); /* 53 */
            II(ref d, a, b, c, x[3], 10, 0x8f0ccc92); /* 54 */
            II(ref c, d, a, b, x[10], 15, 0xffeff47d); /* 55 */
            II(ref b, c, d, a, x[1], 21, 0x85845dd1); /* 56 */
            II(ref a, b, c, d, x[8], 6, 0x6fa87e4f); /* 57 */
            II(ref d, a, b, c, x[15], 10, 0xfe2ce6e0); /* 58 */
            II(ref c, d, a, b, x[6], 15, 0xa3014314); /* 59 */
            II(ref b, c, d, a, x[13], 21, 0x4e0811a1); /* 60 */
            II(ref a, b, c, d, x[4], 6, 0xf7537e82); /* 61 */
            II(ref d, a, b, c, x[11], 10, 0xbd3af235); /* 62 */
            II(ref c, d, a, b, x[2], 15, 0x2ad7d2bb); /* 63 */
            II(ref b, c, d, a, x[9], 21, 0xeb86d391); /* 64 */
            state[0] += a;
            state[1] += b;
            state[2] += c;
            state[3] += d;
        }

        private void MD5Update(byte[] input, UInt32 inputlen)
        {
            UInt32 i = 0, index = 0, partlen = 0;

            index = (count[0] >> 3) & 0x3F;                 // index是一个小于64的整型数

            partlen = 64 - index;                           // partlen是一个小于64的整型数
            count[0] += inputlen << 3;

            if (count[0] < (inputlen << 3))
            {
                count[1]++;
            }

            count[1] += inputlen >> 29;

            if (inputlen >= partlen)
            {
                Array.Copy(input, 0, buffer, index, partlen);
                MD5Transform(state, 0, buffer, 0);

                for (i = partlen; i + 64 <= inputlen; i += 64)
                {
                    MD5Transform(state, 0, input, i);
                }

                index = 0;
            }
            else
            {
                i = 0;
            }

            Array.Copy(input, i, buffer, index, inputlen - i);
        }
                
        private void MD5Encode(byte[] output, UInt32 outputIndex, UInt32[] input, UInt32 inputIndex, UInt32 len)
        {
            UInt32 i = 0, j = 0;
            while (j < len)
            {
                output[j] = (byte)(input[inputIndex + i] & 0xFF);
                output[j + 1] = (byte)((input[inputIndex + i] >> 8) & 0xFF);
                output[j + 2] = (byte)((input[inputIndex + i] >> 16) & 0xFF);
                output[j + 3] = (byte)((input[inputIndex + i] >> 24) & 0xFF);
                i++;
                j += 4;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="digest"></param>
        private void MD5Final(byte[] digest)
        {
            UInt32 index = 0, padlen = 0;
            byte[] bits = new byte[8];

            index = (count[0] >> 3) & 0x3F;
            padlen = (index < 56) ? (56 - index) : (120 - index);

            MD5Encode(bits, 0, count, 0, 8);
            MD5Update(PADDING, padlen);
            MD5Update(bits, 8);
            MD5Encode(digest, 0, state, 0, 16);
        }

        public byte[] Calc(byte[] encrypt, UInt32 encryptLen)
        {
            byte[] decrypt = new byte[16];

            MD5Update(encrypt, encryptLen);

            MD5Final(decrypt);

            return decrypt;
        }
        
    }
}
