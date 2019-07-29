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
                TcpClient tcpClient = await server.AcceptTcpClientAsync().ConfigureAwait(false); 
                listTcpClients.Add(tcpClient);
                var cw = new Client(tcpClient, listTcpClients); 
                cw.HandleIncomingClientAsync().NoWarning();  
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
            server.Stop();
        }
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
 
    public void stop()
    {
        serverSocket.Close();
    }

    
}