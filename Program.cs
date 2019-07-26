using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace TestAppC
{

    class Program
    {
        private static Server server;
        public static Dictionary<string, string> clients = new Dictionary<string, string>();
        static void Main(string[] args)
        {
            //var t = new Thread(delegate ()
            //{
            //    var myServer = new Server("0.0.0.0", 9999);
            //}); 
            //t.Start();

            server = new Server(IPAddress.Any, 9999);
            server.ClientConnected += ClientConnected;
            server.MessageReceived += MessageReceived;
            server.Start();

            Console.WriteLine("SERVER STARTED: " + DateTime.Now);
            char read = Console.ReadKey(true).KeyChar;

            do
            {
                if (read == '1')
                {
                    server.MessageAllClientList();
                }
            } while ((read = Console.ReadKey(true).KeyChar) != '\u001b');
            server.stop();
        }

        private static void handleLogin(TcpClient c, string message)
        {
            var ip = ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString();
            clients.Add(ip, message);
            Console.WriteLine($"USER {message} is now connected:" + Server.END_LINE);
        }

        private static void ClientConnected(TcpClient c)
        {
            if (clients.Count == 0)
            {
                server.sendMessageToClient(c, "Telnet Client" + Server.END_LINE + "User: ");
            }
        }

        private static void MessageReceived(TcpClient c, string message)
        {
            if (clients.Count == 0)
            {
                handleLogin(c, message);
                return;
            }

            var ip = ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString();
            String timeStamp = server.GetTimestamp(DateTime.Now);

            var user = clients[ip];
            Console.WriteLine($"SENDER: {user} MESSAGE:{message} DATE:{timeStamp}");
            server.sendMessageToAllClient(message + Server.END_LINE + Server.CURSOR);
        }
    }
}
