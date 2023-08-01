using System;
using device.network;
using Quobject.SocketIoClientDotNet.Client;
using device.util;
using Newtonsoft.Json.Linq;
using SimpleJSON;
using device.restful;
using System.Threading;
using System.Runtime.InteropServices;

namespace device
{
    class Program
    {
        public const int tcpPort = 21000;

        public static MainServer mainServer = ServerManager.Instance.mainServer;

        public static bool is_socket_open = false;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            ConfigSetting.api_prefix = @"/m-api/device/";
            try
            {
                Console.WriteLine("TagGW Exe");
                if (args.Length > 0)
                {
                    string title = "tagGW Exe";
                    Console.Title = title;
                    IntPtr hWnd = FindWindow(null, title);
                    if (hWnd != IntPtr.Zero)
                    {
                        ShowWindow(hWnd, 2); // Minimize the window
                    }
                    string serverIp = args[0];
                    ConfigSetting.server_address = args[0];
                    ConfigSetting.api_server_domain = @"http://" + ConfigSetting.server_address + ":3006";
                    ConfigSetting.socketServerUrl = @"http://" + ConfigSetting.server_address + ":3006";
                    Console.WriteLine("server_address :" + ConfigSetting.server_address);
                    ConfigSetting.devices = new DeivceInfo[(args.Length - 1) / 2];
                    for (int i = 1; i < args.Length; i++)
                    {
                        Console.WriteLine("ip : " + args[i]);
                        ConfigSetting.devices[(i - 1) / 2].ip = args[i];
                        Console.WriteLine("no : " + args[i + 1]);
                        ConfigSetting.devices[(i - 1) / 2].serial_number = int.Parse(args[i + 1]);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex);
            }

            try
            {
                Socket socket = IO.Socket(ConfigSetting.socketServerUrl);

                socket.On(Socket.EVENT_CONNECT, () =>
                {
                    try
                    {
                        if (is_socket_open)
                        {
                            return;
                        }
                        Console.WriteLine("Socket Connected!");
                        var UserInfo = new JObject();
                        socket.Emit("tagGWSetInfo", UserInfo);
                        //mainServer.Send_REQ_PING();
                        is_socket_open = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
                {
                    try
                    {
                        Console.WriteLine("Socket Connect failed : " + data.ToString());
                        //socket.Close();
                        is_socket_open = false;
                        //socket = IO.Socket(ConfigSetting.socketServerUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                socket.On(Socket.EVENT_DISCONNECT, (data) =>
                {
                    try
                    {
                        Console.WriteLine("Socket Disconnect : " + data.ToString());
                        //socket.Close();
                        is_socket_open = false;
                        //socket = IO.Socket(ConfigSetting.socketServerUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                mainServer.CreateServer(tcpPort);

                socket.On("tagVerifyResponse", (data) =>
                {
                    try
                    {
                        Console.WriteLine("tagVerifyResponse : " + data.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });

                socket.On("deviceTagLock", (data) =>
                {
                    try
                    {
                        Console.WriteLine("deviceTagLock : " + data.ToString());
                        JSONNode jsonNode = SimpleJSON.JSON.Parse(data.ToString());
                        int serial_number = jsonNode["tagGW_no"].AsInt;
                        int ch_value = jsonNode["ch_value"].AsInt;
                        int status = jsonNode["status"].AsInt;
                        mainServer.Send_REQ_SET_TAG_LOCK(serial_number, ch_value, status);
                        //mainServer.Send_REQ_PING(serial_number);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception : " + ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception : " + ex);
            }

            Console.ReadLine();
        }
    }
}
