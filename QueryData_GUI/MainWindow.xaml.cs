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


        }

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {
            QueryService.queryserviceSoapClient service = new QueryService.queryserviceSoapClient();
            txtHelloWorld.Text= service.HelloWorld();


        }

        

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            QueryService.queryserviceSoapClient service = new QueryService.queryserviceSoapClient();
            string queryMac = txtMac.Text.Trim().ToString();
            string startDate = dateStart.CurrentDateTimeText;
            string endDate = dateEnd.CurrentDateTimeText;
            DataTable dt =  service.QueryNTP(queryMac,startDate,endDate);
            int x = dt.Rows.Count;
        }
    }
}
