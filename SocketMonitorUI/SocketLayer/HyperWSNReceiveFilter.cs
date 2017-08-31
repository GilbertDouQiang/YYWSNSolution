using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SuperSocket.Common;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;

namespace HyperWSN.Socket
{
    public class HyperWSNReceiveFilter : BeginEndMarkReceiveFilter<BinaryRequestInfo>
    {
        private readonly static byte[] BeginMark = new byte[] { 0xBE, 0xBE };
        private readonly static byte[] EndMark = new byte[] { 0xEB, 0xEB };

        public HyperWSNReceiveFilter()
            : base(BeginMark, EndMark)
        {

        }

        protected override BinaryRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            return new BinaryRequestInfo(BitConverter.ToString(readBuffer, offset + 3, 1), readBuffer.CloneRange(offset, length));
        }
    }
}
