using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YyWsnCommunicatonLibrary
{
    public class BinaryFile
    {
        public string SafeFileName { get; set; }

        public byte[] Content { get; set; }

        public int FileSize { get; set; }

        public int PacketSize { get; set; }

        public int PacketCount { get; set; }

        /// <summary>
        /// 打开图片文件，读取到字节数组中
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public int Open(string FileName)
        {
            Content = File.ReadAllBytes(FileName);
            FileSize = Content.Length;
            if (PacketSize != 0)
            {
                PacketCount = FileSize / PacketSize + 1;
            }

            return 0;
        }

        public void Save(string Path, byte[] FileBuf)
        {
            File.WriteAllBytes(Path, FileBuf);
        }

        /**************/
    }
}
