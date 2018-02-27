using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace curl
{
    public class HttpProxyServer : HttpServer
    {
        protected override void ProcessRequest(System.Net.HttpListenerContext Context)
        {
            HttpListenerRequest Request = Context.Request;
            HttpListenerResponse Response = Context.Response;

            string url = Request.RawUrl;

            string htm = "";
            switch (url)
            {
                case "/":
                case "/favicon.ico":
                    break;
                default:
                    var uri = new Uri(url.Substring(1).Replace("_-_", "://"));

                    var requestBytes = Encoding.UTF8.GetBytes(
        @"GET " + uri.PathAndQuery + @" HTTP/1.1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36
Host: " + uri.Host + @"

");
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect("genk.vn", 80);
                    if (socket.Connected)
                    {
                        socket.Send(requestBytes);
                        var responseBytes = new byte[socket.ReceiveBufferSize];
                        socket.Receive(responseBytes);
                        htm = Encoding.UTF8.GetString(responseBytes);
                    }
                    break;
            }

            string _type = "text/html; charset=utf-8";
            if (htm != "")
            {
                if (htm.IndexOf('<') != -1)
                    htm = htm.Substring(htm.IndexOf('<'));

                string _format = Request.QueryString["_format"];
                switch (_format)
                {
                    case "text":
                        htm = new HtmlToText().ConvertHtml(htm);
                        _type = "text/plain; charset=utf-8";
                        break;
                    case "body":
                        htm = new Regex(@"<script[^>]*>[\s\S]*?</script>").Replace(htm, string.Empty);
                        break;
                    case "link":  
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(htm);

                        DocumentWithLinks nwl = new DocumentWithLinks(doc);
                        Console.WriteLine("Linked urls:");
                        for (int i = 0; i < nwl.Links.Count; i++)
                        {
                            Console.WriteLine(nwl.Links[i]);
                        }

                        Console.WriteLine("Referenced urls:");
                        for (int i = 0; i < nwl.References.Count; i++)
                        {
                            Console.WriteLine(nwl.References[i]);
                        } 
                        break;
                    default:
                        break;
                }
            }

            byte[] bOutput = System.Text.Encoding.UTF8.GetBytes(htm);
            Response.ContentType = _type;
            Response.ContentLength64 = bOutput.Length;

            //Response.ContentEncoding = Encoding.UTF8; 
            Stream OutputStream = Response.OutputStream;
            OutputStream.Write(bOutput, 0, bOutput.Length);
            OutputStream.Close();
        }
    }
}
