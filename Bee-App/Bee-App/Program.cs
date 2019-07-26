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
            server = new Server(IPAddress.Any, 9999); 
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
    }
}