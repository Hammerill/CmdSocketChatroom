using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    class Program
    {
        private static void ListenServer(Socket serv)
        {
            int bytes;
            byte[] bytesReceived = new byte[1024];
            string data;

            while (true)
            {
                try
                {
                    bytes = serv.Receive(bytesReceived, bytesReceived.Length, 0);
                    data = Encoding.UTF8.GetString(bytesReceived, 0, bytes);

                    Console.WriteLine(data);
                }
                catch (SocketException)
                {
                    Console.WriteLine("\nСервер отключился.\nПерезапустите клиент, когда сервер будет в порядке.");
                    serv.Shutdown(SocketShutdown.Both);
                    serv.Close();
                    throw;
                }
            }
        }

        static void Main(string[] args)
        {
            bool key = true;
            Socket serv_connect = null;
            string nick, serv_ip, serv_port, inp;

            while (key)
            {
                Console.Write("Укажите ваш ник: ");
                nick = Console.ReadLine();

                Console.Write("Укажите IP-адрес сервера: ");
                serv_ip = Console.ReadLine();
                if (serv_ip == "") { serv_ip = "127.0.0.1"; }

                Console.Write("Укажите порт сервера\n(оставьте пустым для порта по умолчанию): ");
                serv_port = Console.ReadLine();
                if (serv_port == "") { serv_port = "7777"; }

                Console.WriteLine("\nСоединение с сервером...");

                try
                {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                    IPAddress ipAddress = ipHostInfo.AddressList[0];
                    serv_connect = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    IPEndPoint serverEndPoint = new(IPAddress.Parse(serv_ip), Convert.ToInt32(serv_port));
                    serv_connect.Connect(serverEndPoint);

                    serv_connect.Send(Encoding.UTF8.GetBytes(nick));
                    key = false;
                    Console.Clear();
                    Console.WriteLine("Соединение с сервером установлено!\n");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Не удалось соединиться с сервером. Ошибка: \"{e.Message}\".\nУбедитесь, что данные введены верно и вы находитесь в одной сети с сервером.\nВведите данные заново.\n\n");
                }
            }

            Thread t = new(() => ListenServer(serv_connect));
            t.Start();

            while (true)
            {
                inp = Console.ReadLine();

                if (inp == "/q")
                {
                    break;
                }

                try
                {
                    serv_connect.Send(Encoding.UTF8.GetBytes(inp));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nОшибка сети...\nНе удаётся отправить сообщение из-за ошибки: \"{e.Message}\".\nУбедитесь, что интернет и сервер в порядке.\n");
                }
            }

            serv_connect.Close();
            serv_connect.Shutdown(SocketShutdown.Both);
        }
    }
}
