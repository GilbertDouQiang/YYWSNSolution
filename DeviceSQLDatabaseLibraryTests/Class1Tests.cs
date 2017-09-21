using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceSQLDatabaseLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceSQLDatabaseLibrary.Tests
{
    [TestClass()]
    public class Class1Tests
    {
        [TestMethod()]
        public void ConnectDatabaseTest()
        {
            Class1 database = new Class1();
            database.ConnectDatabase("192.168.0.127", "sa", "M5m4v0e0");

            //Assert.Fail();
        }

        [TestMethod()]
        public void ConnectDatabaseTest1()
        {
            Class1 sqlserver = new Class1();
            sqlserver.ConnectDatabase(null, null, null);
            
            //Assert.Fail();
        }
    }
}