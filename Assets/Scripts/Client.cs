using System;
using System.Text;
using Newtonsoft.Json;
using System.Net.Sockets;
using UnityEngine;

public class DancerClient
{
    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private string _serverIp;
    private int _serverPort;

    public bool Connected { get; private set; }

    public void Connect(string serverIp, int serverPort)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;

        try
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(_serverIp, _serverPort);
            _networkStream = _tcpClient.GetStream();
            Connected = true;
            //Console.WriteLine("Conectado ao servidor.");
        }
        catch (Exception ex)
        {
            Debug.Log($"Erro ao conectar ao servidor: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        if (!Connected) return;

        try
        {
            _networkStream.Close();
            _tcpClient.Close();
            Connected = false;
            //Console.WriteLine("Desconectado do servidor.");
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Erro ao desconectar: {ex.Message}");
        }
    }

    public void SendMessage(string message)
    {
        if (!Connected) return;

        try
        {
            var fixedMessage = message;
            while (fixedMessage.Length != 150)
            {
                fixedMessage += "*";
            }
            var data = Encoding.UTF8.GetBytes(fixedMessage);
            _networkStream.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Erro ao enviar mensagem: {ex.Message}");
        }
    }

    public string ReceiveMessage()
    {
        if (!Connected) return string.Empty;

        try
        {
            var buffer = new byte[1024];
            var bytesRead = _networkStream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }

    public NetworkData? ReceiveNetworkData()
    {
        var message = ReceiveMessage();
        if (string.IsNullOrEmpty(message)) return null;

        try
        {
            return JsonConvert.DeserializeObject<NetworkData>(message);
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Erro ao desserializar dados: {ex.Message}");
            return null;
        }
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
