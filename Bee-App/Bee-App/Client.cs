using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.IO;

class Client
{
    TcpClient client;
    bool ownsClient;
    int clientId;
    public TrimMessage trimMsg;
    public Dictionary<int, string> clients;

    public Client(TcpClient client, bool ownsClient)
    {
        this.client = client;
        this.ownsClient = ownsClient;
        this.trimMsg = new TrimMessage();
        this.clients = new Dictionary<int, string>(); 
    }

    public async Task HandleIncomingClientAsync()
    {
        try
        {
            using (var stream = client.GetStream())
            { 
                using (var sr = new StreamReader(stream))
                using (var sw = new StreamWriter(stream))
                {
                    await sw.WriteLineAsync("Telnet Client" + Server.END_LINE + "User:").ConfigureAwait(false); 
                    await sw.FlushAsync().ConfigureAwait(false);
                    var data = default(string);
                    while (!((data = await sr.ReadLineAsync().ConfigureAwait(false)).Equals("exit", StringComparison.OrdinalIgnoreCase)))
                    { 
                        var newdata = trimMsg.CleanMessage(data); 
                        if (newdata != "")
                        { 
                            await sw.WriteLineAsync($"Message:" + data).ConfigureAwait(false);
                            await sw.FlushAsync().ConfigureAwait(false);
                            MessageReceived(client, data);
                        } 
                    }
                } 
            }
        }
        finally
        {
            if (client != null)
            {
                (client as IDisposable).Dispose();
                client = null;
            }
        }
    } 

    private void MessageReceived(TcpClient c, string message)
    {  
        if (!ownsClient)
        {
            HandleLogin(c, message);
            return;
        }

        var ip = ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString();
        string timeStamp = GetTimestamp(DateTime.Now);

        var user = clients[clientId];
        Console.WriteLine($"SENDER:{user} MESSAGE:{message} DATE:{timeStamp}"); 
    }
     
    private void HandleLogin(TcpClient c, string message)
    {
        var ip = ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString(); 
        this.ownsClient = true;
        this.clientId = clientId+ 1;
        this.clients.Add(clientId, message);
        Console.WriteLine($"USER {message} is now connected:" + Server.END_LINE);
    }

    public static String GetTimestamp(DateTime value)
    {
        return value.ToString("yyyyMMddHHmmssffff");
    }
}

static class TaskExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NoWarning(this Task t) { }
}
