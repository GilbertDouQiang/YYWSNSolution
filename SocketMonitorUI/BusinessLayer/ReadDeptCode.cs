using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//user define namaspace
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using HyperWSN.Socket;
using YyWsnCommunicatonLibrary;
using YyWsnDeviceLibrary;

namespace SocketMonitorUI.BusinessLayer
{
    /// <summary>
    /// Hyper WSN Internet Protocol 1.0 </br>
    /// Ping 
    /// </summary>
    public class ReadDeptCode : CommandBase<HyperWSNSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "A3";
            }
        }

        public override void ExecuteCommand(HyperWSNSession session, BinaryRequestInfo RxBuf)
        {
            //记录到日志,收到数据
            Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :Received:" + session.RemoteEndPoint.Address.ToString() + " :\t"
                + CommArithmetic.ToHexString(RxBuf.Body) + " ");

            Int16 error = Device.IsPktFromGatewayToServer(RxBuf.Body);

            if (error < 0)
            {
                return;             // 格式错误
            }

            if (ServiceStatus.ExeCmd != 0xA3 || ServiceStatus.ExeResult != 1)
            {
                return;             // 
            }

            UInt16 Serial = (UInt16)(((UInt16)RxBuf.Body[5] << 8) | ((UInt16)RxBuf.Body[6] << 0));
            if (Serial != ServiceStatus.Serial)
            {
                return;             // 序列号不一致
            }

            UInt32 Sid = (UInt32)(((UInt32)RxBuf.Body[7] << 24) | ((UInt32)RxBuf.Body[8] << 16) | ((UInt32)RxBuf.Body[9] << 8) | ((UInt32)RxBuf.Body[10] << 0));
            if (Sid != ServiceStatus.MacOfDesGateway)
            {
                return;
            }

            Int16 Error = RxBuf.Body[11];
            if (Error >= 0x80)
            {
                Error -= 0x100;
            }

            if (Error >= 0)
            {   // 读取成功
                ServiceStatus.ExeResult = 2;
                ServiceStatus.DeptCode = System.Text.Encoding.UTF8.GetString(RxBuf.Body, 12, 10);
            }
            else
            {   // 读取失败
                ServiceStatus.ExeResult = Error;
            }

        }
    }
}
