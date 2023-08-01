using device.util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace device.network
{
    public class MainServer
    {
        CommonHandler CommonHan = new CommonHandler();
        TcpListener tcpServer = null;
        int tcp_port;
        Thread waitClient;
        List<TClient> tclients = new List<TClient>();

        public MainServer()
        {
        }

        public void CreateServer(int port)
        {
            try
            {
                tcp_port = port;
                tcpServer = new TcpListener(IPAddress.Any, port);
                tcpServer.Start();
                waitClient = new Thread(new ThreadStart(ReceiveWait));
                waitClient.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Tag GW 장치 연결 실패!") + ex.ToString());
                Thread.Sleep(5000);
                CreateServer(port);
            }
        }

        private void ReceiveWait()
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection... ");
                TClient client = new TClient(tcpServer.AcceptTcpClient());
                tclients.Add(client);
                Console.WriteLine("New Client Connected! ip:" + client._clientIP + ", no:" + client.gw_no);
                //Thread checkStatus = new Thread(new ThreadStart(checkClientStatus));
                //checkStatus.Start();
            }
        }

        private void checkClientStatus()
        {
            TClient tc = null;
            try
            {
                tc = tclients[tclients.Count - 1];
            }
            catch (Exception ex)
            {
                return;
            }
            while (tc != null)
            {
                try
                {
                    if (!tc.status)
                    {
                        tclients.Remove(tc);
                        break;
                    }
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        public void Send_REQ_PING(int gw_no)
        {
            for (int i = 0; i < tclients.Count; i++)
            {
                if (gw_no == ConfigSetting.getTagGWNo(tclients[i]._clientIP))
                {
                    CommonHan.REQ_PING(tclients[i].networkStream);
                    tclients[i].networkStream.Flush();
                }
            }
        }

        public void Send_REQ_SET_TAG_LOCK(int gw_no, int ch_value, int status)
        {
            for (int i = 0; i < tclients.Count; i++)
            {
                if (gw_no == ConfigSetting.getTagGWNo(tclients[i]._clientIP))
                {
                    CommonHan.REQ_SET_TAG_LOCK(tclients[i].networkStream, gw_no, ch_value, status);
                    tclients[i].networkStream.Flush();
                }
            }
        }
    }
}
