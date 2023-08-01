using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using device.util;

namespace device.network
{
    class TClient
    {
        public NetworkStream networkStream = null;
        public string _clientIP;
        public int gw_no = -1;
        public bool status = false;
        private byte[] receiveData = new byte[1024];          ////Receive Data for TCP callback
        private TcpClient _client;
        CommonHandler CommonHan = new CommonHandler();

        public TClient(TcpClient client)
        {
            status = true;
            _client = client;
            _clientIP = client.Client.RemoteEndPoint.ToString();
            _clientIP = _clientIP.Split(':')[0];
            receiveData = new byte[_client.ReceiveBufferSize];
            networkStream = client.GetStream();
            networkStream.BeginRead(receiveData, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), receiveTcpCallback, null);
            gw_no = ConfigSetting.getTagGWNo(_clientIP);
        }

        public void receiveTcpCallback(IAsyncResult result)
        {
            try
            {
                Console.WriteLine("[TCP][RECV]");
                int total_length = networkStream.EndRead(result);
                if (total_length < 1) return;

                int length = NetUtils.ToInt32(receiveData, 0);
                int opcode = NetUtils.ToInt32(receiveData, 4);
                long rsved1 = NetUtils.ToInt64(receiveData, 8);
                long rsved2 = NetUtils.ToInt64(receiveData, 16);
                int dataLength = length - PacketInfo.HeaderSize;
                //byte[] body = (dataLength == 0) ? null : new byte[dataLength];
                byte[] body = null;
                if (dataLength > 0)
                {
                    body = new byte[dataLength];
                    Array.Copy(receiveData, PacketInfo.HeaderSize, body, 0, dataLength);
                }
                else
                {
                    return;
                }
                Console.WriteLine("[TCP][RECV] Length : " + length);
                switch ((Opcode)opcode)
                {
                    case Opcode.RES_PING:
                        CommonHan.RES_PING(new PacketInfo(length, opcode, rsved1, rsved2, body), body);
                        break;
                    case Opcode.REQ_GET_TAG_INFO:
                        CommonHan.REQ_GET_TAG_INFO(networkStream, gw_no, new PacketInfo(length, opcode, rsved1, rsved2, body), body);
                        break;
                    case Opcode.RES_SET_TAG_LOCK:
                        CommonHan.RES_SET_TAG_LOCK(new PacketInfo(length, opcode, rsved1, rsved2, body), body);
                        break;
                    case Opcode.ERROR_MESSAGE:
                        CommonHan.ERROR_MESSAGE(new PacketInfo(length, opcode, rsved1, rsved2, body), body);
                        break;
                    default:
                        Console.WriteLine("[TCP][RECV] Wrong Opcode : 0x" + opcode.ToString("X8"));
                        break;
                }
                lock (_client.GetStream())
                {
                    networkStream.Flush();
                    networkStream.BeginRead(receiveData, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), receiveTcpCallback, null);
                }
            }
            catch (Exception e)
            {
                status = false;
            }
        }
    }
}
