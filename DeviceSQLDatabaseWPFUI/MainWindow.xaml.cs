using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DeviceSQLDatabaseLibrary;
using System.Threading;

namespace DeviceSQLDatabaseWPFUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DatabaseService service;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStartService_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartService.Content.ToString() == "Start Service")
            {
                if (service != null)
                {
                    service.StartService();
                    service = null;
                    Thread.Sleep(500);
                }

                service = new DatabaseService();
                service.ConnectionString = txtConnectionString.Text;//"Server = 192.168.0.127; Database = HyperWSN_DB; Uid = sa; Pwd = M5m4v0e0";
                //service.StopService();
                service.SaveInterval = 2000;
                service.QueueName = "HyperWSNQueue";
                
                service.StartService();

                btnStartService.Content = "Stop Service";
            }
            else
            {

                if (service != null)
                {
                    service.StopService();
                    //service.Dispose();
                    service = null;
                    //txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Stopped \r\n" + txtConsole.Text;
                    btnStartService.Content = "Start Service";
                    //if (chkLog.IsChecked == true)
                    //{
                    //    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Stopped ");
                    //}
                }
                else
                {
                    //
                    btnStartService.Content = "Start Service";
                }

            }
        }

        

    }
}
