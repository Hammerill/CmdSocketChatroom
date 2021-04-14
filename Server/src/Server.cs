using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    class Server
    {
        private List<Socket> Clients { get; set; }
        private Socket Serv { get; set; }

        public Server()
        {
            bool key = true;
            string serv_port;

            Console.Clear();

            Clients = new();

            while (key)
            {
                Console.Write("Укажите порт сервера\n(оставьте пустым для порта по умолчанию): ");
                serv_port = Console.ReadLine();
                if (serv_port == "") { serv_port = "7777"; }

                try
                {
                    IPEndPoint ipEndPoint = new(IPAddress.Any, Convert.ToInt32(serv_port));

                    Serv = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    Serv.Bind(ipEndPoint);
                    Serv.Listen();
                    key = false;

                    Console.WriteLine($"Сервер инициализирован на порту {serv_port}.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Не удалось запустить сервер на порту {serv_port}, ошибка: \"{e.Message}\".\nВведите данные повторно.\n");
                }
            }
        }

        public void Start()
        {
            Socket client;
            while (true)
            {
                try
                {
                    client = Serv.Accept();
                    Clients.Add(client);

                    Console.WriteLine($"Клиент {GetAddress(client)} подключается к серверу...");
                    Thread t = new(() => ListenClient(client));
                    t.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Произошла неизвестная ошибка: \"{e.Message}\".");
                }
            }
        }

        private void SendAll(string data, Socket exceptClient = null)
        {
            foreach (Socket cl in Clients)
            {
                if (cl != exceptClient)
                {
                    cl.Send(Encoding.UTF8.GetBytes(data));
                }
            }
        }
        private void ListenClient(Socket client)
        {
            int bytes;
            byte[] bytesReceived = new byte[1024];
            string data, client_nick, toSend;

            try
            {
                bytes = client.Receive(bytesReceived, bytesReceived.Length, 0);
                client_nick = Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                Console.WriteLine($"Клиент {client_nick} ({GetAddress(client)}) подключился к серверу.");
                SendAll($"{client_nick} подключился.", client);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Клиент {GetAddress(client)} не смог подключиться к серверу из-за ошибки \"{e.Message}\".");
                Clients.Remove(client);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                return;
            }

            while (true)
            {
                try
                {
                    bytes = client.Receive(bytesReceived, bytesReceived.Length, 0);
                    data = Encoding.UTF8.GetString(bytesReceived, 0, bytes);

                    toSend = $"{client_nick}: {data}";
                    Console.WriteLine(toSend);
                    SendAll(toSend, client);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Клиент {client_nick} ({GetAddress(client)}) отключился от сервера.");
                    Clients.Remove(client);
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    SendAll($"{client_nick} отключился.");
                    return;
                }
            }
        }
        private static string GetAddress(Socket socket)
        {
            return $"{IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Address.ToString())}:{IPAddress.Parse(((IPEndPoint)socket.RemoteEndPoint).Port.ToString())}";
        }
    }
}
