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
            // TODO: 2021-01-05 为了方便调试CC1310的串口升级，暂时将此测试用例注释掉。

            // string filePath = @"E:\Visual_Workspace\dq\VS2015\YYWSNSolution\YyWsnCommunicatonLibraryTests\bin\Debug\out\SGX_YYWSN_MSP432.txt";
            // BootLoader BSL = new BootLoader();

            // BSL.MyOpen(filePath, 256 * 1024);

            //Assert.Fail();
        }
    }
}