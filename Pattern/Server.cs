using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class Server
{
    private static CancellationTokenSource cts;
    private static List<string> connectedClients;
    private static UdpClient udpClient;
    private static IPEndPoint serverEndpoint;
    private static List<IMessageHandler> messageHandlers;

    public static async Task AcceptMsg(CancellationToken cancellationToken)
    {
        cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        connectedClients = new List<string>();
        udpClient = new UdpClient(5050);
        serverEndpoint = new IPEndPoint(IPAddress.Any, 0);

        messageHandlers = new List<IMessageHandler>
        {
            new RegistrationHandler(connectedClients),
            new BroadcastHandler(connectedClients, udpClient, serverEndpoint)
        };

        Console.WriteLine("Сервер ожидает сообщения. Для завершения нажмите клавишу...");

        Task exitTask = Task.Run(() =>
        {
            Console.ReadKey();
            RequestExit();
        });

        try
        {
            Task receiveTask = Task.Run(() => ReceiveMessages());

            while (!cts.Token.IsCancellationRequested)
            {
                UdpReceiveResult receiveResult;

                try
                {
                    receiveResult = await ReceiveWithCancellationAsync(udpClient, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                string data1 = Encoding.UTF8.GetString(receiveResult.Buffer);
                Message msg = Message.FromJson(data1);
                Console.WriteLine(msg.ToString());

                if (msg.Text.ToLower() == "exit")
                {
                    RequestExit();
                    break;
                }

                ProcessCommand(msg);
            }

            await receiveTask;
        }
        finally
        {
            udpClient.Close();
            await exitTask;
        }
    }

    private static void ProcessCommand(Message msg)
    {
        foreach (var handler in messageHandlers)
        {
            if (handler.CanHandle(msg))
            {
                handler.HandleAsync(msg).Wait(); // Ждем завершения обработки, чтобы сохранить порядок
                break; // Прерываем цепочку после первого обработчика
            }
        }
    }

    private static void SendToAllClients(Message msg)
    {
        foreach (var client in connectedClients)
        {
            if (client != msg.Sender)
            {
                Message responseMsg = new Message("Server", client, $"Broadcast: {msg.Text}");
                string responseMsgJs = responseMsg.ToJson();
                byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);
                udpClient.SendAsync(responseData, responseData.Length, serverEndpoint);
            }
        }
    }

    private static void SendToClient(Message msg)
    {
        if (connectedClients.Contains(msg.Recipient))
        {
            Message responseMsg = new Message("Server", msg.Recipient, $"Private message from {msg.Sender}: {msg.Text}");
            string responseMsgJs = responseMsg.ToJson();
            byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);
            udpClient.SendAsync(responseData, responseData.Length, serverEndpoint);
        }
        else
        {
            Console.WriteLine($"Клиент {msg.Recipient} не найден.");
        }
    }

    private static async Task<UdpReceiveResult> ReceiveWithCancellationAsync(UdpClient udpClient, CancellationToken cancellationToken)
    {
        var receiveTask = udpClient.ReceiveAsync();
        var completedTask = await Task.WhenAny(receiveTask, Task.Delay(-1, cancellationToken));

        if (completedTask == receiveTask)
        {
            return await receiveTask;
        }
        else
        {
            throw new OperationCanceledException(cancellationToken);
        }
    }

    private static void RequestExit()
    {
        if (!cts.Token.IsCancellationRequested)
        {
            cts.Cancel();
        }
    }

    private static async void ReceiveMessages()
    {
        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                UdpReceiveResult receiveResult = await udpClient.ReceiveAsync();
                byte[] answerData = receiveResult.Buffer;
                string answerMsgJs = Encoding.UTF8.GetString(answerData);
                Message answerMsg = Message.FromJson(answerMsgJs);
                Console.WriteLine(answerMsg.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при приеме сообщений: {ex.Message}");
        }
    }
}

