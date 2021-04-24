using AsyncSocketChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSocketChat
{
    class Program
    {
        public static List<DataModel> _sokets { get; set; } = new List<DataModel>();
        public static AutoResetEvent resetEvent = new AutoResetEvent(false);
        public static string address = "127.0.0.1";
        public static int port = 2895;
        static void Main()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            IPEndPoint server = new IPEndPoint(IPAddress.Parse(address), port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(server);

            socket.Listen(10);
            Console.WriteLine("Сервер запущений. Очікую підключення...");
            while (true)
            {
                socket.BeginAccept(new AsyncCallback(AcceptEnd), socket);

                resetEvent.WaitOne();
            }
        }

        public static void AcceptEnd(IAsyncResult ar)
        {
            DataModel model = new DataModel();
            
            var socket = ar.AsyncState as Socket;
            Socket newSocket = socket.EndAccept(ar);
            model.currentSocket = newSocket;
            _sokets.Add(model);
            resetEvent.Set();
            Task.Run(() =>
            {
               try
                {
                    while (model.currentSocket.Connected)
                    {
                        newSocket.BeginReceive(model.elements, 0, model.elements.Length, 0, new AsyncCallback(ReceiveEnd), model);

                        model.resetEvent.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public static void ReceiveEnd(IAsyncResult ar)
        {
                Task.Run(() =>
                {
                    lock (_sokets)
                    {
                    
                    DataModel model = ar.AsyncState as DataModel;

                    try
                    {

                        model.countBytes = model.currentSocket.EndReceive(ar);
                        if (model.countBytes == 0) { throw new Exception(); } 
                        model.builder.Append(Encoding.UTF8.GetString(model.elements)
                            .ToCharArray().Take(model.countBytes).ToArray());


                        if (model.currentSocket.Available > 0)
                        {
                            model.currentSocket
                            .BeginReceive(model.elements, 0, model.elements.Length, 0, new AsyncCallback(ReceiveEnd), model);

                            model.resetEvent.WaitOne();
                        }


                        Console.WriteLine(model.builder.ToString());
                        foreach (var item in _sokets.ToList().Where(x => x.currentSocket.Connected).ToList())
                        {
                            item.currentSocket.Send(Encoding.UTF8.GetBytes(model.builder.ToString()));
                        }
                        model.builder.Clear();
                        model.resetEvent.Set();

                    }
                    catch
                    {
                        model.currentSocket.Shutdown(SocketShutdown.Both);
                        model.currentSocket.Close();
                        model.resetEvent.Set();
                    }

                  }
                });
        }
    }
}
