using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;

namespace QueryData_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            dateStart.DateTimeText = System.DateTime.Now.AddHours(-4).ToString();
            dateEnd.DateTimeText = System.DateTime.Now.AddHours(1).ToString();

            dateGatewayStaticStart.DateTimeText = System.DateTime.Now.AddHours(-4).ToString();
            dateGatewayStaticEnd.DateTimeText = System.DateTime.Now.AddHours(1).ToString();

            dateM1Start.DateTimeText = System.DateTime.Now.AddHours(-4).ToString();
            dateM1End.DateTimeText = System.DateTime.Now.AddHours(1).ToString();


        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {
            QueryService.queryserviceSoapClient service = new QueryService.queryserviceSoapClient();
            txtHelloWorld.Text= service.HelloWorld();
        }


        DataTable DTNTP;
        DataTable DTGatewayStatus;
        DataTable DTM1;
        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryService.queryserviceSoapClient service = new QueryService.queryserviceSoapClient();
            string queryMac = txtMac.Text.Trim().ToString();
            string startDate = dateStart.CurrentDateTimeText;
            string endDate = dateEnd.CurrentDateTimeText;
            DTNTP =  service.QueryNTP(queryMac,startDate,endDate);
            int x = DTNTP.Rows.Count;
            gridNTP.ItemsSource = DTNTP.DefaultView;
            
        }

        private void btnGatewayStaticQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryService.queryserviceSoapClient service = new QueryService.queryserviceSoapClient();
            string queryMac = txtGatewayStaticMac.Text.Trim().ToString();
            string startDate = dateGatewayStaticStart.CurrentDateTimeText;
            string endDate = dateGatewayStaticEnd.CurrentDateTimeText;
            DTGatewayStatus = service.QueryGatewayStatus(queryMac, startDate, endDate);
            
            gridGatewayStatic.ItemsSource = DTGatewayStatus.DefaultView;
        }

        private void btnM1Query_Click(object sender, RoutedEventArgs e)
        {
            QueryService.queryserviceSoapClient service = new QueryService.queryserviceSoapClient();
            string queryMac = txtM1Mac.Text.Trim().ToString();
            string startDate = dateM1Start.CurrentDateTimeText;
            string endDate = dateM1End.CurrentDateTimeText;
            DTM1 = service.QueryM1Status(queryMac, startDate, endDate);
            int x = DTM1.Rows.Count;
            gridM1.ItemsSource = DTM1.DefaultView;
        }
    }
}
