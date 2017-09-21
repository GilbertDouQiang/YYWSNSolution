using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeviceSQLDatabaseLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceSQLDatabaseLibrary.Tests
{
    [TestClass()]
    public class DatabaseServiceTests
    {
        [TestMethod()]
        public void StartServiceTest()
        {
            DatabaseService service = new DatabaseService();
            service.ConnectionString = "Server = 192.168.0.127; Database = HyperWSN_DB; Uid = sa; Pwd = M5m4v0e0";
            //service.StopService();
            service.SaveInterval = 1000;
            service.QueueName = "HyperWSNQueue";
            service.ClientID = "Receive1";
            service.StartService();
            System.Threading.Thread.Sleep(60000);
            //"Server=192.168.0.127;Network=dbmssocn;Database=master; Uid=sa; Pwd=M5m4v0e0"


        }
    }
}