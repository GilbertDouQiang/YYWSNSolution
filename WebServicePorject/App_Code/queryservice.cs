using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using YyWsnDeviceLibrary;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;



/// <summary>
/// Summary description for queryservice
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class queryservice : System.Web.Services.WebService
{

    public queryservice()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    public string HelloWorld()
    {
        return "Hello World";
    }



    [WebMethod]
    public DataTable QueryNTP(string mac ,string startdate,string edndate)
    {
        string connStr = ConfigurationManager.AppSettings["ConnectionString"];
        SqlConnection connection = new SqlConnection(connStr);
        connection.Open();

        SqlCommand cmd = new SqlCommand();

        cmd.Connection = connection;

        cmd.CommandType = CommandType.Text;
        //cmd.CommandText = "select * from ntpstatus where DeviceMac=@DeviceMac ";

        cmd.CommandText = "select * from ntpstatus where DeviceMac=@DeviceMac and SystemDate>=@startDate and SystemDate<=@endDate";

        cmd.Parameters.Add("@DeviceMAC", SqlDbType.VarChar);
        cmd.Parameters.Add("@startDate", SqlDbType.DateTime);
        cmd.Parameters.Add("@endDate", SqlDbType.DateTime);

        cmd.Parameters["@DeviceMAC"].Value = mac;
        cmd.Parameters["@startDate"].Value = startdate;
        cmd.Parameters["@endDate"].Value = edndate;
        

        DataTable dt = new DataTable();
        dt.TableName = "NTP";

        SqlDataAdapter adapter = new SqlDataAdapter();

        adapter.SelectCommand = cmd;

        adapter.Fill(dt);





        return dt;
    }

}
