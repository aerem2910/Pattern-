
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class BroadcastHandler : IMessageHandler
{
    private readonly List<string> connectedClients;
    private readonly UdpClient udpClient;
    private readonly IPEndPoint serverEndpoint;

    public BroadcastHandler(List<string> connectedClients, UdpClient udpClient, IPEndPoint serverEndpoint)
    {
        this.connectedClients = connectedClients;
        this.udpClient = udpClient;
        this.serverEndpoint = serverEndpoint;
    }

    public bool CanHandle(Message message)
    {
        return string.IsNullOrEmpty(message.Recipient);
    }

    public async Task HandleAsync(Message message)
    {
        foreach (var client in connectedClients)
        {
            if (client != message.Sender)
            {
                Message responseMsg = new Message("Server", client, $"Broadcast: {message.Text}");
                string responseMsgJs = responseMsg.ToJson();
                byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);
                await udpClient.SendAsync(responseData, responseData.Length, serverEndpoint);
            }
        }
    }
}
