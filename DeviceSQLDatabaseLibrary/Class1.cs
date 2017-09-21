using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data; // State variables 
using System.Data.SqlClient; // Database 
using System.Globalization; // Date 

namespace DeviceSQLDatabaseLibrary
{
    public class Class1
    {
        public void ConnectDatabase(string ipaddress,string username ,string password)
        {
            SqlConnection myConn = new SqlConnection("Server=192.168.0.127;Network=dbmssocn;Database=master; Uid=sa; Pwd=M5m4v0e0");
            myConn.Open();

        }
    }
}
