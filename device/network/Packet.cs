using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Facility.Protocol;

namespace device.network
{
    public enum Errorcode : int
    {
        ERR_VERSION = 0x00000000,

        ERR_UNKNOWN,
        ERR_INVALID_OPCODE,
        ERR_NOT_AUTH,
        ERR_SERVER_INTERNAL,
        ERR_INVALID_SERIAL,
        ERR_ALREADY_AUTH,

        ERR_VERSION_END
    }
    public enum Opcode : int
    {
        OP_VERSION = 0x10000000,

        REQ_GET_DEVICE_INFO,
        RES_GET_DEVICE_INFO,
        REQ_SET_DEVICE_INFO,
        RES_SET_DEVICE_INFO,
        REQ_SET_DEVICE_REBOOT,
        RES_SET_DEVICE_REBOOT,
        REQ_PING,
        RES_PING,
        REQ_GET_TAG_INFO,
        RES_GET_TAG_INFO,
        REQ_SET_TAG_LOCK,
        RES_SET_TAG_LOCK,
        REQ_SET_VALVE_CTRL,
        RES_SET_VALVE_CTRL,
        REQ_GET_DEVICE_STATUS,
        RES_GET_DEVICE_STATUS,
        REQ_SET_DEVICE_STATUS,
        RES_SET_DEVICE_STATUS,
        REQ_SET_FLOWMETER_START,
        RES_SET_FLOWMETER_START,
        REQ_SET_FLOWMETER_VALUE,
        RES_SET_FLOWMETER_VALUE,
        REQ_SET_FLOWMETER_FINISH,
        RES_SET_FLOWMETER_FINISH,

        ERROR_MESSAGE,

        OP_VERSION_END
    }

    public class PacketInfo : BinaryRequestInfo
    {
        public Int32 length { get; private set; }
        public Int32 opcode { get; private set; }
        public Int64 reqid { get; private set; }
        public Int64 token { get; private set; }

        public byte[] body { get; private set; }

        public PacketInfo(int length, int opcode, long reqid, long token, byte[] body)
            :base(null, body)
        {
            this.length = length;
            this.opcode = opcode;
            this.reqid = reqid;
            this.token = token;
        }

        public static int HeaderSize
        {
            get {
                return 24;
            }
        }
    }
}
