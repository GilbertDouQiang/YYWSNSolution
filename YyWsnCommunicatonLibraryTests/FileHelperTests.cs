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
            string filePath = @"E:\Visual_Workspace\dq\VS2015\YYWSNSolution\YyWsnCommunicatonLibraryTests\bin\Debug\out\SGX_YYWSN_MSP432.txt";
            BootLoader BSL = new BootLoader();

            BSL.MyOpen(filePath, 256 * 1024);

            //Assert.Fail();
        }
    }
}