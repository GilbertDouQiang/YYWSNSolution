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



namespace SocketMonitorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        HyperWSNSocketServer server;

        IServerConfig m_Config;

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
                    ClearIdleSession= true,
                    ClearIdleSessionInterval=5
                };

                //保证启动时，前面AppServer对象已经释放
                if (server != null)
                {
                    server.Dispose();
                    server = null;
                }

                server = new HyperWSN.Socket.HyperWSNSocketServer();
                server.NewSessionConnected += Server_NewSessionConnected; ;
                server.NewRequestReceived += Server_NewRequestReceived; ;
                server.SessionClosed += Server_SessionClosed; ;


                //如果执行2次Setup , 会抛出错误
                server.Setup(new RootConfig(), m_Config);
                //AppServer对象的基础设置


                //server.

                server.Start();
                if (server.State == ServerState.Running)
                {
                    txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff")+ " :\tService Started \r\n" +txtConsole.Text;
                    

                    btnStartService.Content = "Stop Service";

                    if (chkLog.IsChecked == true)
                    {
                        Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Started ");
                    }

                }

            }
            else
            {
                if(server !=null)
                {
                    server.Stop();
                    server.Dispose();
                    server = null;
                    txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Stopped \r\n" + txtConsole.Text;
                    btnStartService.Content = "Start Service";
                    if (chkLog.IsChecked == true)
                    {
                        Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tService Stopped ");
                    }
                }
                else
                {
                    //
                    btnStartService.Content = "Start Service";
                }

            }

            
        }

        private void Server_SessionClosed(HyperWSNSession session, CloseReason value)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tClient Disconnect:\t " + session.RemoteEndPoint.Address.ToString() + " :"
                + session.RemoteEndPoint.Port.ToString() + "\t Reason:" + value.ToString() + " \r\n" + txtConsole.Text;

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
                txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tReceived:" + session.RemoteEndPoint.Address.ToString()+" :\t" 
                +CommArithmetic.ToHexString( requestInfo.Body)+" \r\n" + txtConsole.Text;
                if (chkLog.IsChecked == true)
                {
                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tReceived:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                + CommArithmetic.ToHexString(requestInfo.Body) + " ");
                }

            }));
        }

        private void Server_NewSessionConnected(HyperWSNSession session)
        {
            //throw new NotImplementedException();
            //客户端连接成功
            Dispatcher.BeginInvoke(new Action(delegate
            {
                txtConsole.Text = DateTime.Now.ToString("HH:mm:ss.fff") + " :\tClient Connect:      \t"+ session.RemoteEndPoint.Address.ToString() +" :" +session.RemoteEndPoint.Port.ToString()+ " \r\n" + txtConsole.Text;
                if (chkLog.IsChecked == true)
                {
                    Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :\tClient Connect:      \t" + session.RemoteEndPoint.Address.ToString() + " :" + session.RemoteEndPoint.Port.ToString() );
                }

            }));
        }
    }
}
