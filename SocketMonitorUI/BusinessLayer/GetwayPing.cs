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
    public class GetwayPing : CommandBase<HyperWSNSession, BinaryRequestInfo>
    {
        public override string Name
        {
            get
            {
                return "90";
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

            if (ServiceStatus.ResponsePing == false)
            {
                return;             // 不需要响应
            }

            // 发送反馈
            byte[] TxBuf = new byte[11];

            // 起始位
            TxBuf[0] = 0xEB;
            TxBuf[1] = 0xEB;

            // 长度位
            TxBuf[2] = 0x04;

            // 命令位
            TxBuf[3] = RxBuf.Body[3];

            // 协议版本
            TxBuf[4] = RxBuf.Body[4];

            // 序列号
            TxBuf[5] = RxBuf.Body[5];
            TxBuf[6] = RxBuf.Body[6];

            // CRC
            UInt16 crc = MyCustomFxn.CRC16(MyCustomFxn.GetItuPolynomialOfCrc16(), 0, TxBuf, 3, TxBuf[2]);
            TxBuf[7] = (byte)((crc & 0xFF00) >> 8);
            TxBuf[8] = (byte)((crc & 0x00FF) >> 0);

            // 结束位
            TxBuf[9] = 0xBE;
            TxBuf[10] = 0xBE;

            session.Send(TxBuf, 0, TxBuf.Length);

            // 记录日志
            Logger.AddLog(DateTime.Now.ToString("HH:mm:ss.fff") + " :SendData:" + session.RemoteEndPoint.Address.ToString() + " :\t"
               + CommArithmetic.ToHexString(TxBuf) + " ");
        }
    }
}
