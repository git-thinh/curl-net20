using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace curl
{
    class Program
    {
        static void Main(string[] args)
        {
            //http://127.0.0.1:8888/http_-_genk.vn/ai-nay-da-danh-bai-20-luat-su-hang-dau-nuoc-my-trong-linh-vuc-ma-ho-gioi-nhat-20180227012111793.chn?_format=text
            HttpServer Server = null;
            Server = new HttpProxyServer();
            Server.Start("http://127.0.0.1:8888/");
            //Server.Stop();



            Console.WriteLine("COMPLETE ........");
            Console.ReadLine();
        }
    }
}
