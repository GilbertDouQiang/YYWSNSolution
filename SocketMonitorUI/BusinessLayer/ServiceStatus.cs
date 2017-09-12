using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketMonitorUI.BusinessLayer
{
    public class ServiceStatus
    {
        /// <summary>
        /// 是否响应Ping
        /// </summary>
        public static bool ResponsePing { get; set; }

        /// <summary>
        /// 是否响应网关上报的状态信息
        /// </summary>
        public static bool ResponseGatewayReport { get; set; }

    }
}
