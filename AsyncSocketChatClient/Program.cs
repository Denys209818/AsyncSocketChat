using AsyncSocketChatClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSocketChatClient
{
    class Program
    {
        public static AutoResetEvent resetEvent = new AutoResetEvent(false);
        public static AutoResetEvent receiveStop { get; set; } = new AutoResetEvent(false);
        public static string address { get; set; } = "127.0.0.1";
        public static int port { get; set; }
        static void Main()
        {   
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2895);

            port = new Random().Next(1000, 9999);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(address), port);
            
            Socket socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        
                socketClient.Bind(endPoint);
                try
                {
                    socketClient.BeginConnect(server, new AsyncCallback(ConnectEnd), socketClient);
                      
                    resetEvent.WaitOne();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                socketClient.Shutdown(SocketShutdown.Both);
                socketClient.Close();
        }

        static void ConnectEnd(IAsyncResult ar) 
        {
                Task.Run(() =>
                {
                    DataModel model = new DataModel();
                    Socket socket = ar.AsyncState as Socket;
                    model.socket = socket;
                    model.socket.EndConnect(ar);
                    while (true)
                    {
                        string message = String.Empty;
                        Task.Run(() =>
                        {
                            while (string.IsNullOrEmpty(message)) 
                            {
                                socket.BeginReceive(model.bytes, 0, model.bytes.Length, 0,
                                new AsyncCallback(ReceiveEnd), model);

                                receiveStop.WaitOne();
                            }
                        });
                        Console.Write("Ведіть повідомлення: ");
                        message = Console.ReadLine();
                        if (!string.IsNullOrEmpty(message))
                        {
                            var data = Encoding.UTF8.GetBytes(message);
                            socket.Send(data);
                            Console.WriteLine("Повідомлення відправлено!");
                        }
                        else
                        {
                            resetEvent.Set();
                            return;
                        }
                    }

                });
        }

        static void ReceiveEnd(IAsyncResult ar) 
        {
            DataModel model = ar.AsyncState as DataModel;
            lock (model) 
            {
            model.countBytes = model.socket.EndReceive(ar);
            model.builder.Append( Encoding.UTF8.GetString(model.bytes).ToCharArray().Take(model.countBytes).ToArray());
            if (model.socket.Available > 0) 
            {
                model.socket.BeginReceive(model.bytes, 0, model.bytes.Length, 0, new AsyncCallback(ReceiveEnd), model);
            }

            Console.WriteLine("\n" + model.socket.RemoteEndPoint.ToString() + ": " + 
                model.builder.ToString() + "\n");
            model.builder.Clear();
            }
            receiveStop.Set();
        }

    }
}
