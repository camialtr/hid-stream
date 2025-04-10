using System;
using System.Text;
using System.Net.Sockets;

// ReSharper disable EmptyGeneralCatchClause

public class Client
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _networkStream;

    public bool Connected { get; private set; }

    public Client(string serverIp, int serverPort)
    {
        try
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(serverIp, serverPort);
            _networkStream = _tcpClient.GetStream();
            Connected = true;
        }
        catch (Exception) { }
    }

    public void Disconnect()
    {
        if (!Connected) return;

        try
        {
            _networkStream.Close();
            _tcpClient.Close();
            Connected = false;
        }
        catch (Exception) { }
    }

    public void SendMessage(string message)
    {
        if (!Connected) return;
        
        try
        {
            var data = Encoding.UTF8.GetBytes(message);
            _networkStream.Write(data, 0, data.Length);
        }
        catch (Exception) { }
    }
}

public class NetworkData
{
    public float AccelX { get; set; }
    public float AccelY { get; set; }
    public float AccelZ { get; set; }
    public float GyroX { get; set; }
    public float GyroY{ get; set; }
    public float GyroZ{ get; set; }
}
