using System;
using System.Security.Permissions;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LiteDB;
using Fleck2;
using Fleck2.Interfaces;

namespace curl
{
    class Program
    {
        static void Main(string[] args)
        {
            app.Start();
        }
    }

    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class app
    {
        private static int m_Port = 0;
        static app()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                Assembly asm = null;
                string comName = ev.Name.Split(',')[0];
                string resourceName = @"DLL\" + comName + ".dll";
                var assembly = Assembly.GetExecutingAssembly();
                resourceName = typeof(app).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                ms.Write(buffer, 0, read);
                            buffer = ms.ToArray();
                        }
                        asm = Assembly.Load(buffer);
                    }
                }
                return asm;
            };
        }


        private static HttpServer Server = null;
        public static void Start()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            m_Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            m_Port = 8888;
            string uri = string.Format("http://127.0.0.1:{0}/", m_Port);
            Console.Title = m_Port.ToString();

            dbi.Init();

            //////http://127.0.0.1:8888/http_-_genk.vn/ai-nay-da-danh-bai-20-luat-su-hang-dau-nuoc-my-trong-linh-vuc-ma-ho-gioi-nhat-20180227012111793.chn?_format=text
            //////HttpServer Server = null;
            ////Server = new HttpProxyServer();
            ////Server.Start(uri);
            //////Server.Stop();
            //////Console.WriteLine(uri);
            ////Console.ReadLine();


            //FleckLog.Level = LogLevel.Debug;
            FleckLog.Level = LogLevel.Info;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://localhost:"+ m_Port.ToString());
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                    allSockets.ForEach(s => s.Send("Echo: " + message));
                };
            });

            //Process.Start("client.html");

            var input = Console.ReadLine();
            while (input != "exit")
            {
                foreach (var socket in allSockets)
                {
                    socket.Send(input);
                }
                input = Console.ReadLine();
            }






        }

        public static void Stop()
        {
            Server.Stop();
            dbi.CloseAll();
        }
    }
}
