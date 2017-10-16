using Microsoft.VisualStudio.TestTools.UnitTesting;
using YyWsnDeviceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YyWsnDeviceLibrary.Tests
{
    [TestClass()]
    public class CommArithmeticTests
    {
        [TestMethod()]
        public void FormatIPAddressTest()
        {
            string x = CommArithmetic.FormatIPAddress("192.16.18.1");
            Assert.Fail();
        }
    }
}