using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using YyWsnDeviceLibrary;

namespace YyWsnCommunicatonLibrary
{
    public class FileHelper
    {
        public static ObservableCollection<Device> ReadFile(string FileName)
        {
            if (FileName == null)
                return null;

            FileInfo file = new FileInfo(FileName);
            if (!file.Exists)
            {
                return null;

            }

            StreamReader reader = new StreamReader(file.FullName);


            ObservableCollection<Device> devices = new ObservableCollection<Device>();

            string currentLine;

            currentLine = reader.ReadLine();
            while (currentLine != null)
            {
                byte[] SourceByte = CommArithmetic.HexStringToByteArray(currentLine, 27);


                Device singleDevice = DeviceFactory.CreateDevice(SourceByte);
                if (singleDevice != null)
                {
                    devices.Add(singleDevice);

                }
                currentLine = reader.ReadLine();
            }




            return devices;
        }
    }
}
