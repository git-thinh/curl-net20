using HtmlAgilityPack;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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
            byte[] bOutput;
            Stream OutputStream = Response.OutputStream;

            switch (Request.HttpMethod)
            {
                case "POST":
                    #region
                    htm = "{}";
                    StreamReader stream = new StreamReader(Request.InputStream);
                    string data = stream.ReadToEnd();
                    data = HttpUtility.UrlDecode(data);

                    htm = rest.Query(data);

                    bOutput = System.Text.Encoding.UTF8.GetBytes(htm);
                    Response.ContentType = "application/json; charset=utf-8";
                    Response.ContentLength64 = bOutput.Length;
                    OutputStream.Write(bOutput, 0, bOutput.Length);
                    OutputStream.Close();
                    break;
                #endregion
                case "GET":
                    string _type = "text/html; charset=utf-8";
                    string _action = Request.QueryString["action"];
                    string _input = string.Empty, _file = string.Empty, _id = string.Empty, _model = string.Empty;
                    int _skip = 0, _limit = 10;
                    JObject _jobject;

                    #region 

                    switch (_action)
                    {
                        case "restore-file":
                            #region
                            _type = "application/json; charset=utf-8";
                            _model = Request.QueryString["model"];
                            _file = Request.QueryString["file"];

                            htm = JsonConvert.SerializeObject(new { ok = false, output = "The file is invalid" });
                            if (!string.IsNullOrEmpty(_file))
                            {
                                _file = HttpUtility.UrlDecode(_file);
                                if (File.Exists(_file))
                                {

                                    _input = JsonConvert.SerializeObject(new { file = _file });
                                    _jobject = JsonConvert.DeserializeObject<JObject>(_input);
                                    htm = rest.Query(new message[] { new message() { action = _action, model = _model, input = _input, jobject = _jobject } });
                                }
                            }
                            break;
                            #endregion
                        case "restore-uri":
                            #region
                            _type = "application/json; charset=utf-8";
                            _model = Request.QueryString["model"];
                            _file = Request.QueryString["file"];

                            htm = JsonConvert.SerializeObject(new { ok = false, output = "The file is invalid" });
                            if (!string.IsNullOrEmpty(_file))
                            {
                                _file = HttpUtility.UrlDecode(_file);
                                if (File.Exists(_file))
                                {

                                    _input = JsonConvert.SerializeObject(new { file = _file });
                                    _jobject = JsonConvert.DeserializeObject<JObject>(_input);
                                    htm = rest.Query(new message[] { new message() { action = _action, model = _model, input = _input, jobject = _jobject } });
                                }
                            }
                            break;
                            #endregion
                        case "import-file":
                            #region
                            _type = "application/json; charset=utf-8";
                            _model = Request.QueryString["model"];
                            _file = Request.QueryString["file"];

                            htm = JsonConvert.SerializeObject(new { ok = false, output = "The file is invalid" });
                            if (!string.IsNullOrEmpty(_file))
                            {
                                _file = HttpUtility.UrlDecode(_file);
                                if (File.Exists(_file))
                                {

                                    _input = JsonConvert.SerializeObject(new { file = _file });
                                    _jobject = JsonConvert.DeserializeObject<JObject>(_input);
                                    htm = rest.Query(new message[] { new message() { action = _action, model = _model, input = _input, jobject = _jobject } });
                                }
                            }
                            break;
                            #endregion
                        case "import-uri":
                            #region
                            _type = "application/json; charset=utf-8";
                            _model = Request.QueryString["model"];
                            _file = Request.QueryString["file"];

                            htm = JsonConvert.SerializeObject(new { ok = false, output = "The file is invalid" });
                            if (!string.IsNullOrEmpty(_file))
                            {
                                _file = HttpUtility.UrlDecode(_file);
                                if (File.Exists(_file))
                                {

                                    _input = JsonConvert.SerializeObject(new { file = _file });
                                    _jobject = JsonConvert.DeserializeObject<JObject>(_input);
                                    htm = rest.Query(new message[] { new message() { action = _action, model = _model, input = _input, jobject = _jobject } });
                                }
                            }
                            break;
                            #endregion
                        case "fetch":
                            #region
                            _type = "application/json; charset=utf-8";
                            _model = Request.QueryString["model"];

                            string skip = Request.QueryString["skip"];
                            string limit = Request.QueryString["limit"];

                            int.TryParse(skip, out _skip);
                            int.TryParse(limit, out _limit);

                            if (_skip < 0) _skip = 0;
                            if (_limit <= 0) _limit = 10;

                            _input = JsonConvert.SerializeObject(new { skip = _skip, limit = _limit });
                            _jobject = JsonConvert.DeserializeObject<JObject>(_input);
                            htm = rest.Query(new message[] { new message() { action = _action, model = _model, input = _input, jobject = _jobject } });
                            break;
                        #endregion
                        case "getbyid":
                            #region
                            _type = "application/json; charset=utf-8";
                            _id = Request.QueryString[_LITEDB_CONST.FIELD_ID];
                            _model = Request.QueryString["model"];
                            if (string.IsNullOrEmpty(_id) || string.IsNullOrEmpty(_model))
                            {
                                htm = @"{""ok"":false, ""output"":""The fields " + _LITEDB_CONST.FIELD_ID + @" or model be not NULL""}";
                            }
                            else
                            {
                                _input = @"{""" + _LITEDB_CONST.FIELD_ID + @""":" + _id + "}";
                                _jobject = JsonConvert.DeserializeObject<JObject>(_input);
                                htm = rest.Query(new message[] { new message() { action = _action, model = _model, input = _input, jobject = _jobject } });
                            }
                            break;
                        #endregion
                        default:
                            #region

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


                            break;
                            #endregion
                    }

                    bOutput = System.Text.Encoding.UTF8.GetBytes(htm);
                    Response.ContentType = _type;
                    Response.ContentLength64 = bOutput.Length;
                    OutputStream.Write(bOutput, 0, bOutput.Length);
                    OutputStream.Close();
                    break;
                    #endregion
            }


        }
    }
}
