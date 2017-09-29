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

        /// <summary>
        /// 是否响应授时请求
        /// </summary>
        public static bool ResponseNTP { get; set; }

        /// <summary>
        /// 是否响应用户传感器数据
        /// </summary>
        public static bool ResponseSensorData { get; set; }

        /// <summary>
        /// 标记是否要存入数据库
        /// </summary>
        public static bool SaveToSQLServer { get; set; }

    }
}
