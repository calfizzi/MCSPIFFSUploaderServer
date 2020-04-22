using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace TcpEchoServer
{

  class LineBufferedClient
  {
    public LineBufferedClient(TcpClient client)
    {
      ReadBuffer = new byte[256];
      CurrentLine = new StringBuilder();
      Client = client;
    }

    public TcpClient Client { get; private set; }
    public Byte[] ReadBuffer { get; private set; }
    public StringBuilder CurrentLine { get; set; }
  }

  class ClientManager
  {
    List<LineBufferedClient> _clients = new List<LineBufferedClient>();

    public void Add(TcpClient tcpClient)
    {
      var client = new LineBufferedClient(tcpClient);

      var result = tcpClient.GetStream().BeginRead(client.ReadBuffer, 0, client.ReadBuffer.Length, DataReceived, client);

      _clients.Add(client);
    }

    private void HandleCompleteLine(LineBufferedClient client, string line)
    {
      Console.WriteLine(line);
      var buffer = Encoding.ASCII.GetBytes(line + "\n");
      _clients.ForEach((connectedClient) => { if (connectedClient != client) connectedClient.Client.GetStream().Write(buffer, 0, buffer.Length); });
    }

    private void DataReceived(IAsyncResult ar)
    {
      var client = ar.AsyncState as LineBufferedClient;

      var bytesRead = client.Client.GetStream().EndRead(ar);

      if (bytesRead > 0)
      {
        var readString = Encoding.UTF8.GetString(client.ReadBuffer, 0, bytesRead);

        while (readString.Contains("\n"))
        {
          var indexOfNewLine = readString.IndexOf('\n');
          var left = readString.Substring(0, indexOfNewLine);
          client.CurrentLine.Append(left);

          var line = client.CurrentLine.ToString();

          client.CurrentLine.Clear();
          if (indexOfNewLine != readString.Length - 1)
          {
            readString = readString.Substring(indexOfNewLine + 1);
          }
          else
          {
            readString = string.Empty;
          }
          client.Client.GetStream().Write(Encoding.UTF8.GetBytes(line + "\n"), 0, Encoding.UTF8.GetByteCount(line + "\n"));
          HandleCompleteLine(client, line);
        }

        if (!string.IsNullOrEmpty(readString))
        {
          client.CurrentLine.Append(readString);
        }

        client.Client.GetStream().BeginRead(client.ReadBuffer, 0, 256, DataReceived, client);

      }
      else
      {
        _clients.Remove(client);
      }
    }
  }

  class Server
  {
    CancellationTokenSource _cts = new CancellationTokenSource();
    private bool _shutdown = false;
    int _serverPort = 0;
    private Thread _listenerThread;
    private ClientManager _clientManager;

    public static IPAddress GetLocalIPAddress()
    {
      var host = Dns.GetHostEntry(Dns.GetHostName());
      foreach (var ip in host.AddressList)
      {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
          return ip;
        }
      }
      throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public Server(ClientManager clientManager)
    {
      _clientManager = clientManager;
    }

    public void Run(int serverPort)
    {
      _serverPort = serverPort;
      _listenerThread = new Thread(ListenLoop);
      _listenerThread.Start();
    }

    public void ListenLoop()
    {
      TcpListener listener = new TcpListener(new IPEndPoint(GetLocalIPAddress(), _serverPort));
      listener.Start();

      while (!_shutdown)
      {
        try
        {
          var acceptTask = listener.AcceptTcpClientAsync();

          acceptTask.Wait(_cts.Token);

          var newClient = acceptTask.Result;

          _clientManager.Add(newClient);
        }
        catch (OperationCanceledException)
        {
          // NOP - Shutting down
        }
      }
    }

    public void Stop()
    {
      _shutdown = true;
      _cts.Cancel();
      _listenerThread.Join();
    }
  }

  public class TcpEchoServer
  {
    public static void start()
    {
      Console.WriteLine("Starting echo server...");

      int port = 1234;
      TcpListener listener = new TcpListener(IPAddress.Loopback, port);
      listener.Start();

      TcpClient client = listener.AcceptTcpClient();
      NetworkStream stream = client.GetStream();
      StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
      StreamReader reader = new StreamReader(stream, Encoding.ASCII);

      while (true)
      {
        string inputLine = "";
        while (inputLine != null)
        {
          inputLine = reader.ReadLine();
          writer.WriteLine("Echoing string: " + inputLine);
          Console.WriteLine("Echoing string: " + inputLine);
        }
        Console.WriteLine("Server saw disconnect from client.");
      }
    }
  }
}