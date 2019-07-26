using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using TestAppC;

class Server
{
    public TcpListener server = null;
    private IPAddress ip;
    private int port;
    private int dataSize;
    private byte[] data;
    public const string END_LINE = "\r\n";
    public const string CURSOR = " > ";

    TrimMessage trimMsg;
    List<TcpClient> listTcpClients = new List<TcpClient>();
    private Socket serverSocket;

    public delegate void ConnectionEventHandler(TcpClient c);
    public event ConnectionEventHandler ClientConnected;
    public delegate void MessageReceivedEventHandler(TcpClient c, string message);
    public event MessageReceivedEventHandler MessageReceived;

    public Server(IPAddress ip, int port)
    {
        this.ip = ip;
        this.port = port;
        this.dataSize = 1024;
        this.data = new byte[dataSize];
        this.trimMsg = new TrimMessage();

        this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Start()
    {
        IPAddress localAddr = this.ip;
        server = new TcpListener(localAddr, this.port);
        server.Start();
        StartListener();
    }

    public async void StartListener()
    {
        try
        {
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                TcpClient tcpClient = await server.AcceptTcpClientAsync();
                listTcpClients.Add(tcpClient);
                ClientConnected(tcpClient);
                HandleIncomingClient(tcpClient);

            }
        }
        catch (SocketException e)
        {
            Console.WriteLine("SocketException: {0}", e);
            server.Stop();
        }
    }

    public void HandleIncomingClient(Object obj)
    {
        TcpClient client = (TcpClient)obj;
        var stream = client.GetStream();
        var ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        string data = null;
        byte[] bytes = new Byte[256];
        int? i;
        try
        {
            var initialMsg = "";
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                short hex = BitConverter.ToInt16(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, i.Value);
                initialMsg = string.Concat(initialMsg, data);

                if (data == ";" || hex == 2573)
                {
                    var message = trimMsg.CleanMessage(initialMsg);
                    if (message != "")
                    {
                        MessageReceived(client, message);
                    }
                    i = null;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e.ToString());
            client.Close();
        }
    }

    public void sendMessageToAllClient(string message)
    {
        Byte[] reply = System.Text.Encoding.ASCII.GetBytes("MESSAGE:" + message);
        listTcpClients.ForEach(cl => {
            Task.Run(async () => await cl.GetStream().WriteAsync(reply, 0, reply.Length));
        });
    }

    public void sendMessageToClient(TcpClient c, string message)
    {
        var data = Encoding.ASCII.GetBytes(message);
        var stream = c.GetStream();
        stream.Write(data, 0, data.Length);
    }

    public void MessageAllClientList()
    {
        foreach (TcpClient client in listTcpClients)
        {
            try
            {
                Console.WriteLine("Client List:" + Server.END_LINE + ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());
            }
            catch
            {
                // clients.Remove(s);
            }
        }
    }


    public String GetTimestamp(DateTime value)
    {
        return value.ToString("yyyyMMddHHmmssffff");
    }

    public void stop()
    {
        serverSocket.Close();
    }

}