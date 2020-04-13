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

using HyperWSN.Socket;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;

using YyWsnDeviceLibrary;

using YyWsnCommunicatonLibrary;

using SocketMonitorUI.BusinessLayer;
using System.Configuration;

namespace SocketMonitorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HyperWSNSocketServer ThisServer;

        IServerConfig m_Config;

        bool ServerInitSuc = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStartService_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartService.Content.ToString() == "Start Service")
            {
                m_Config = new ServerConfig
                {
                    Port = Convert.ToInt16(txtPort.Text.ToString()),
                    Ip = "Any",
                    MaxConnectionNumber = 200,
                    Mode = SocketMode.Tcp,
                    Name = "HyperWSNSocketServer",
                    IdleSessionTimeOut = 120,
                    ClearIdleSession = true,
                    ClearIdleSessionInterval = 5
                };

                // 保证启动时，前面AppServer对象已经释放
                if (ThisServer != null)
                {
                    ThisServer.Dispose();
                    ThisServer = null;
                }

                ThisServer = new HyperWSN.Socket.HyperWSNSocketServer();
                ThisServer.NewSessionConnected += Server_NewSessionConnected;
                //server.NewRequestReceived += Server_NewRequestReceived; 
                ThisServer.SessionClosed += Server_SessionClosed; ;


                // 如果执行2次Setup , 会抛出错误
                ServerInitSuc = ThisServer.Setup(new RootConfig(), m_Config);

                // AppServer对象的基础设置

                // 服务器状态设置
                ServiceStatus.ResponsePing = cbPing.IsChecked.Value;
                ServiceStatus.ResponseGatewayReport = cbGatewayReport.IsChecked.Value;
                ServiceStatus.ResponseNTP = cbNTP.IsChecked.Value;
                ServiceStatus.ResponseSensorData = cbSensorData.IsChecked.Value;
                ServiceStatus.SaveToSQLServer = chkDataBase.IsChecked.Value;

                ThisServer.Start();

                if (ThisServer.State == ServerState.Running)
                {
                    txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Started \r\n" + txtConsole.Text;

                    btnStartService.Content = "Stop Service";

                    if (chkLog.IsChecked == true)
                    {
                        Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Started ");
                    }
                }
            }
            else
            {
                if (ThisServer != null)
                {
                    ThisServer.Stop();
                    ThisServer.Dispose();
                    ThisServer = null;
                    txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Stopped \r\n" + txtConsole.Text;
                    btnStartService.Content = "Start Service";
                    if (chkLog.IsChecked == true)
                    {
                        Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Stopped ");
                    }
                }
                else
                {
                    btnStartService.Content = "Start Service";
                }
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="session"></param>
        /// <param name="value"></param>
        private void Server_SessionClosed(HyperWSNSession session, CloseReason value)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                try
                {
                    session.DisconnectQueue();
                    session.DisConnectSQLServer();
                }
                catch (Exception)
                {
                    throw;
                }

                if (cbLogConsole.IsChecked == true)
                {
                    txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tClient Disconnect:\t " + session.RemoteEndPoint.Address.ToString() + " :"
                    + session.RemoteEndPoint.Port.ToString() + "\t Reason:" + value.ToString() + " \r\n" + txtConsole.Text;
                }
                if (chkLog.IsChecked == true)
                {
                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tClient Disconnect:\t " + session.RemoteEndPoint.Address.ToString() + " :"
                + session.RemoteEndPoint.Port.ToString() + "\t Reason:" + value.ToString());
                }

            }));
        }

        private void Server_NewRequestReceived(HyperWSNSession session, SuperSocket.SocketBase.Protocol.BinaryRequestInfo requestInfo)
        {
            //throw new NotImplementedException();
            //客户端连接成功
            Dispatcher.BeginInvoke(new Action(delegate
            {
                session.Send(requestInfo.Body, 0, requestInfo.Body.Length);
                if (cbLogConsole.IsChecked == true)
                {
                    txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tReceived:" + session.RemoteEndPoint.Address.ToString() + " :\t"
               + CommArithmetic.ToHexString(requestInfo.Body) + " \r\n" + txtConsole.Text;
                }

                if (chkLog.IsChecked == true)
                {
                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tReceived:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                + CommArithmetic.ToHexString(requestInfo.Body) + " ");
                }
            }));
        }

        /// <summary>
        /// 客户端连接成功
        /// </summary>
        /// <param name="session"></param>
        private void Server_NewSessionConnected(HyperWSNSession session)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                //MQ 相关操作
                try
                {
                    //session.ConnectQueue("HyperWSNQueue",session.RemoteEndPoint.Address.ToString()+"."+session.RemoteEndPoint.Port.ToString());
                }
                catch (Exception)
                {

                }

                // connection to database

                // MQ 相关操作结束

                // 是否在Console显示日志
                if (cbLogConsole.IsChecked == true)
                {
                    txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tClient Connect:      \t" + session.RemoteEndPoint.Address.ToString() + " :" + session.RemoteEndPoint.Port.ToString() + " \r\n" + txtConsole.Text;
                }

                // 是否记录日志到文件中去
                if (chkLog.IsChecked == true)
                {
                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tClient Connect:      \t" + session.RemoteEndPoint.Address.ToString() + " :" + session.RemoteEndPoint.Port.ToString());
                }

                try
                {
                    session.ConnectSQLServer();
                }
                catch (Exception ex)
                {
                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tConnect Database Error" + ex.Message);
                }

            }));
        }

        private void cbPing_Click(object sender, RoutedEventArgs e)
        {
            ServiceStatus.ResponsePing = cbPing.IsChecked.Value;
        }

        private void cbGatewayReport_Click(object sender, RoutedEventArgs e)
        {
            ServiceStatus.ResponseGatewayReport = cbGatewayReport.IsChecked.Value;
        }

        private void cbNTP_Click(object sender, RoutedEventArgs e)
        {
            ServiceStatus.ResponseNTP = cbNTP.IsChecked.Value;
        }

        private void cbSensorData_Click(object sender, RoutedEventArgs e)
        {
            ServiceStatus.ResponseSensorData = cbSensorData.IsChecked.Value;
        }

        private void chkDataBase_Click(object sender, RoutedEventArgs e)
        {
            ServiceStatus.SaveToSQLServer = chkDataBase.IsChecked.Value;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //read setup from config
            try
            {
                string portStr = ConfigurationManager.AppSettings["port"];
                txtPort.Text = portStr;
            }
            catch (Exception)
            {

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["port"].Value = txtPort.Text;
            config.Save(ConfigurationSaveMode.Modified);
        }

        private void btnReadDeptCode_Click(object sender, RoutedEventArgs e)
        {
            byte[] ByteBufTmp = MyCustomFxn.HexStringToByteArray(tbxMacOfDesGateway.Text);
            if (ByteBufTmp == null || ByteBufTmp.Length < 4)
            {
                MessageBox.Show("网关MAC号错误！");
                return;
            }

            ServiceStatus.MacOfDesGateway = ((UInt32)ByteBufTmp[0] << 24) | ((UInt32)ByteBufTmp[1] << 16) | ((UInt32)ByteBufTmp[2] << 8) | ((UInt32)ByteBufTmp[3] << 0);
            ServiceStatus.ExeCmd = 0xA3;
            ServiceStatus.ExeResult = 0;
            ServiceStatus.Serial = 0;
            ServiceStatus.DeptCode = "";
        }

        private void btnSetDeptCode_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReadResult_Click(object sender, RoutedEventArgs e)
        {
            tbkRunning.Text = ServiceStatus.ExeResult.ToString();
            if(ServiceStatus.ExeResult == 2)
            {
                tbkDeptCode.Text = ServiceStatus.DeptCode;
            }
        }
    }
}
