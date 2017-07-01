using Microsoft.VisualStudio.TestTools.UnitTesting;
using YyWsnCommunicatonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YyWsnDeviceLibrary;
using System.Collections.ObjectModel;

namespace YyWsnCommunicatonLibrary.Tests
{
    [TestClass()]
    public class FileHelperTests
    {
        [TestMethod()]
        public void ReadFileTest()
        {
            string fileName = @"D:\00.YYTech\21.Program\01.YYWSNSolution\YYWSNSolution\SnifferGUI\bin\Debug\Data.txt";
            ObservableCollection<Device> devices = FileHelper.ReadFile(fileName);

            //Assert.Fail();
        }
    }
}