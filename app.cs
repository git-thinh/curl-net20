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
using Newtonsoft.Json;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace curl
{


    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class app
    {
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

        public static void test()
        {
            string[] a = File.ReadAllLines("demo.txt").Where(x => x.Trim() != "").ToArray();
            Paragraph p;
            List<Paragraph> ls = new List<Paragraph>() { new Paragraph(0, a[0]) };
            string si = string.Empty, _code = string.Empty, _ul = string.Empty;
            bool _isCode = false, _isLI = false;
            int _id = 0;
            for (int i = 1; i < a.Length; i++)
            {
                si = a[i];
                if (si == EL._TAG_CODE_CHAR_BEGIN || _isCode)
                {
                    if (si != EL._TAG_CODE_CHAR_BEGIN) _id = i;

                    _isCode = true;
                    if (si != EL._TAG_CODE_CHAR_BEGIN && si != EL._TAG_CODE_CHAR_END)
                        _code += Environment.NewLine + si;

                    if (i == a.Length - 1 || si == EL._TAG_CODE_CHAR_END)
                    {
                        _isCode = false;
                        p = new Paragraph() { id = _id, text = _code, type = SENTENCE.CODE, html = string.Format("<{0}>{1}</{0}>",EL.TAG_CODE, _code) };
                        ls.Add(p);
                    }
                }
                else
                {
                    if (si[0] == '#')
                    {
                        si = si.Substring(1).Trim();
                        if (_isLI == false)
                        {
                            _id = i;
                            _isLI = true;
                            _ul = "<ul><li>" + si + "</li>";
                        }
                        else
                            _ul += "<li>" + si + "</li>";

                        if (i == a.Length - 1)
                        {
                            _ul += "</ul>";
                            _isLI = false;
                            p = new Paragraph() { id = _id, text = _ul, type = SENTENCE.UL_LI, html = _ul };
                            ls.Add(p);
                        }
                    }
                    else
                    {
                        if (_isLI)
                        {
                            _ul += "</ul>";
                            _isLI = false;
                            p = new Paragraph() { id = _id, text = _ul, type = SENTENCE.UL_LI, html = _ul };
                            ls.Add(p);
                        }

                        p = new Paragraph(i, si);
                        ls.Add(p);
                    }
                }
            }
            string htm = string.Join(Environment.NewLine, ls.Select(x => x.html).ToArray());
            htm = string.Format("<{0}>{1}</{0}>", EL.TAG_ARTICLE, htm);

            string ss = Translator.TranslateText("hello", "en|vi");
            Console.WriteLine("{0} = {1}", "hello", ss);
            Console.ReadLine();
        }



        #region

        // Initialize a new instance of the SpeechSynthesizer.
        private static SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        private static HttpServer Server = null;
        private static int m_Port = 0;
        private static void DoMethod(object obj)
        {
            IWebSocketConnection socket = (IWebSocketConnection)obj;
            while (true)
            {
                try
                {
                    socket.Send(JsonConvert.SerializeObject(new { action = "NOTI_TEST", msg = Guid.NewGuid().ToString() }));
                }
                catch
                {
                    break;
                    Console.WriteLine("CLOSE .....");
                }
                Thread.Sleep(5000);
            }
        }
        // Write to the console when the SpeakAsync operation has been cancelled.
        static void synth_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            Console.WriteLine("\nThe SpeakAsync operation was cancelled!!");
        }

        // When it changes, write the state of the SpeechSynthesizer to the console.
        static void synth_StateChanged(object sender, StateChangedEventArgs e)
        {
            Console.WriteLine("\nSynthesizer State: {0}    Previous State: {1}\n", e.State, e.PreviousState);
        }

        // Write the text being spoken by the SpeechSynthesizer to the console.
        static void synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            //Console.WriteLine(e.Text);
        }

        public static void Start()
        {
            #region
            // Configure the audio output. 
            speechSynthesizer.SetOutputToDefaultAudioDevice();
            // Subscribe to the StateChanged event.
            speechSynthesizer.StateChanged += new EventHandler<StateChangedEventArgs>(synth_StateChanged);
            // Subscribe to the SpeakProgress event.
            speechSynthesizer.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synth_SpeakProgress);
            // Subscribe to the SpeakCompleted event.
            speechSynthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(synth_SpeakCompleted);

            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            m_Port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            m_Port = 8888;
            string uri = string.Format("http://127.0.0.1:{0}/", m_Port);
            Console.Title = m_Port.ToString();

            dbi.Init();

            msgProcess.Init();
            //FleckLog.Level = LogLevel.Debug;
            FleckLog.Level = LogLevel.Info;
            var allSockets = new List<IWebSocketConnection>();
            var server = new WebSocketServer("ws://localhost:8889");
            #endregion

            server.Start(socket =>
            {
                socket.OnMessage = message =>
                {
                    if (message.Length > 0)
                    {
                        switch (message[0])
                        {
                            case '#':
                                try
                                {
                                    message = message.Substring(1).Trim();
                                    speechSynthesizer.Speak(message);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                                break;
                            case '!':
                                // Cancel the SpeakAsync operation and wait one second.
                                speechSynthesizer.SpeakAsyncCancelAll();
                                break;
                            default:
                                long id = msgProcess.Push(socket.ConnectionInfo.Id.ToString(), message);
                                //socket.Send(id.ToString());
                                break;
                        }
                    }
                };
                socket.OnOpen = () =>
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(DoMethod));
                    thread.Start(socket);

                    msgProcess.Join(socket);
                    //socket.Send("ID=" + socket.ConnectionInfo.Id.ToString());
                };
                socket.OnClose = () =>
                {
                    //Console.WriteLine("Close!");
                    //allSockets.Remove(socket);
                };
            });

            //Process.Start("client.html");

            //http://127.0.0.1:8888/http_-_genk.vn/ai-nay-da-danh-bai-20-luat-su-hang-dau-nuoc-my-trong-linh-vuc-ma-ho-gioi-nhat-20180227012111793.chn?_format=text
            //HttpServer Server = null;
            Server = new HttpProxyServer();
            Server.Start(uri);
            //Server.Stop();
            //Console.WriteLine(uri);
            ////Console.ReadLine();


            var input = Console.ReadLine();
            while (input != "exit")
            {
                if (input == "cls")
                    Console.Clear();
                else
                {
                    //foreach (var socket in allSockets)
                    //{
                    //    socket.Send(input);
                    //}

                    switch (input[0])
                    {
                        case '!':
                            // Cancel the SpeakAsync operation and wait one second.
                            speechSynthesizer.SpeakAsyncCancelAll();
                            break;
                        default:
                            try
                            {
                                speechSynthesizer.Speak(input);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            break;
                    }
                }
                input = Console.ReadLine();
            }






        }

        public static void Stop()
        {
            Server.Stop();
            dbi.CloseAll();
        }
        #endregion
    }
    class Program
    {
        static void Main(string[] args)
        {
            //app.Start();
            app.test();
        }
    }

}

