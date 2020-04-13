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

        //======================================================
        // DQ
        //======================================================

        /// <summary>
        /// 目的网关的MAC
        /// </summary>
        public static UInt32 MacOfDesGateway { get; set; }

        /// <summary>
        /// 需要执行的指令
        /// </summary>
        public static byte ExeCmd { get; set; }

        /// <summary>
        /// 执行的过程和结果
        /// </summary>
        public static Int16 ExeResult { get; set; }

        /// <summary>
        /// 传输序列号
        /// </summary>
        public static UInt16 Serial { get; set; }

        /// <summary>
        /// DeptCode
        /// </summary>
        public static string DeptCode { get; set; }

    }
}
