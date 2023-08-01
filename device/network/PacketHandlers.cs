using device.util;
using device.restful;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace device.network
{
    #region CommonHandler
    public class CommonHandler
    {
        TokenManager tm = TokenManager.Instance;
        public void REQ_PING(NetworkStream stream)
        {
            try
            {
                //// uint64 TIMESTAMP 본 패킷의 전송 시간
                //int date = NetUtils.ToInt32(requestInfo.body, 0);
                //int time = NetUtils.ToInt32(requestInfo.body, 4);
                DateTime now = DateTime.Now;
                Int32 length = PacketInfo.HeaderSize + 8;
                Int32 opcode = (int)Opcode.RES_PING;
                byte[] packet = new byte[length];
                Array.Copy(NetUtils.GetBytes(length), 0, packet, 0, 4);
                Array.Copy(NetUtils.GetBytes(opcode), 0, packet, 4, 4);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 8, 8);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 16, 8);
                int pos = 0;
                Array.Copy(NetUtils.GetBytes(NetUtils.ConvertDateTimeToNetDate(now)), 0, packet, PacketInfo.HeaderSize + pos, 4); pos += 4;
                Array.Copy(NetUtils.GetBytes(NetUtils.ConvertDateTimeToNetTime(now)), 0, packet, PacketInfo.HeaderSize + pos, 4); pos += 4;
                stream.Write(packet, 0, packet.Length);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void RES_PING(PacketInfo requestInfo, byte[] body)
        {
            try
            {
                // uint64 TIMESTAMP REQ_PING 패킷의 TIMESTAMP 값
                int date = NetUtils.ToInt32(body, 0);
                int time = NetUtils.ToInt32(body, 4);
                Console.WriteLine("Date : " + date + ", time : " + time);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void REQ_GET_TAG_INFO(NetworkStream stream, int gw_no, PacketInfo requestInfo, byte[] body)
        {
            try
            {
                // char[256] NFC_VALUE 확인 대상 NFC 값 문자열
                int ch_value = NetUtils.ToInt32(body, 0);
                byte[] tag_value = new byte[256];
                Array.Copy(body, 4, tag_value, 0, 256);
                string str_tag_value = NetUtils.ConvertByteArrayToStringASCII(tag_value).ToUpper();
                Console.WriteLine("[REQ_GET_TAG_INFO] :  ch_value : " + ch_value);
                Console.WriteLine("[REQ_GET_TAG_INFO] : tag data : ");
                for (int i = 0; i < tag_value.Length; i++)
                {
                    Console.WriteLine(tag_value[i] + " ");
                }
                Int32 length = PacketInfo.HeaderSize;
                Int32 opcode = (int)Opcode.RES_GET_TAG_INFO;
                byte[] packet = new byte[length];
                Array.Copy(NetUtils.GetBytes(length), 0, packet, 0, 4);
                Array.Copy(NetUtils.GetBytes(opcode), 0, packet, 4, 4);
                Array.Copy(NetUtils.GetBytes(requestInfo.reqid), 0, packet, 8, 8);
                Array.Copy(NetUtils.GetBytes(requestInfo.token), 0, packet, 16, 8);
                stream.Write(packet, 0, packet.Length);
                Console.WriteLine("[RES_GET_TAG_INFO] : " + str_tag_value);
                var res = ApiClient.Instance.TagVerifyFunc(ch_value, gw_no, str_tag_value);
                if (res.suc == 1)
                {
                    if (res.status == 1)
                    {
                        //TAG_LOCK
                        REQ_SET_TAG_LOCK(stream, gw_no, ch_value, 0);
                    }
                }
            }
            catch(Exception ex)
            {
                //Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void REQ_SET_TAG_LOCK(NetworkStream stream, int serial_number, int ch_value, int status)
        {
            try
            {
                Int32 length = PacketInfo.HeaderSize + 5;
                Int32 opcode = (int)Opcode.REQ_SET_TAG_LOCK;
                byte[] packet = new byte[length];
                Array.Copy(NetUtils.GetBytes(length), 0, packet, 0, 4);
                Array.Copy(NetUtils.GetBytes(opcode), 0, packet, 4, 4);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 8, 8);
                Array.Copy(NetUtils.GetBytes((long)0), 0, packet, 16, 8);
                Array.Copy(NetUtils.GetBytes(ch_value), 0, packet, 24, 4);
                packet[28] = (byte)status;
                stream.Write(packet, 0, packet.Length);
                Console.WriteLine("[REQ_SET_TAG_LOCK] : ch_value : " + ch_value + ", Status : " + status);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void RES_SET_TAG_LOCK(PacketInfo requestInfo, byte[] body)
        {
            try
            {
                int ch_value = NetUtils.ToInt32(body, 0);
                byte status = body[4];
                Console.WriteLine("[RES_SET_TAG_LOCK] : ch_value : " + ch_value + ", Status : " + status);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Exception : " + ex.ToString());
            }
        }

        public void ERROR_MESSAGE(PacketInfo requestInfo, byte[] body)
        {
            // uint32 OPCODE 에러가 발생한 요청의 OPCODE
            // uint32 ERROR_CODE 에러 코드
            // char[256] ERROR_MESSAGE 에러 메시지

            UInt32 opcode = NetUtils.ToUInt32(body, 0);
            UInt32 error_code = NetUtils.ToUInt32(body, 4);

            byte[] error_message = new byte[256];
            Array.Copy(requestInfo.body, 8, error_message, 0, 256);
            string str_error_message = NetUtils.ConvertByteArrayToStringASCII(error_message);

            Console.WriteLine("[ERROR_MESSAGE] " + opcode.ToString() + " : " + error_code.ToString());

            //session.Close();//disconnect
        }
    }
    #endregion
}
