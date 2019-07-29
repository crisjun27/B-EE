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
    string currentUser;
    public TrimMessage trimMsg;
    public Dictionary<int, string> clients = new Dictionary<int, string>();
    List<TcpClient> listTcpClients;
    public const string END_LINE = "\r\n";
    public const string CURSOR = " > ";

    public Client(TcpClient client,Object obj)
    {
        this.client = client;
        this.ownsClient = false;
        this.trimMsg = new TrimMessage();
        this.clients = new Dictionary<int, string>(); 
        this.listTcpClients =  (List<TcpClient>)obj;
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
                            if(MessageReceived(client, data)){ 
                                SendToAllClients(newdata); 
                            } 
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

    public void SendToAllClients(string message)
    {
        var user = GetUser();
        string timeStamp = GetTimestamp(DateTime.Now);
        var composemessge = $"USER: {user} MESSAGE: {message} {timeStamp}" + END_LINE;
          
        Byte[] reply = System.Text.Encoding.ASCII.GetBytes(composemessge);
        listTcpClients.ForEach(cl =>
        {
            Task.Run(async () => 
                await cl.GetStream().WriteAsync(reply, 0, reply.Length)
                );
        }); 
    } 

    private bool MessageReceived(TcpClient c, string message)
    {  
        if (!ownsClient)
        {
            HandleLogin(c, message);
            return false;
        }

        var ip = ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString();
        string timeStamp = GetTimestamp(DateTime.Now);
        var user = GetUser();

        Console.WriteLine($"SENDER:{user} MESSAGE:{message} DATE:{timeStamp}"); 
        return true;
    }
     
    private void HandleLogin(TcpClient c, string message)
    {
        this.ownsClient = true;
        this.currentUser = message;
        this.clientId = clientId + 1;
        this.clients.Add(clientId, message);

        var ip = ((IPEndPoint)c.Client.RemoteEndPoint).Address.ToString(); 
        Console.WriteLine($"USER {message} is now connected:" + Server.END_LINE);
    }

    public static String GetTimestamp(DateTime value)
    {
        return value.ToString("yyyyMMddHHmmssffff");
    }

    public string GetUser()
    {
        return this.clients[clientId];
    }
}

static class TaskExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NoWarning(this Task t) { }
}
