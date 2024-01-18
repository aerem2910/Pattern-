
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class Client
{
    private static UdpClient udpClient;
    private static IPEndPoint serverEndpoint;

    public static async Task SendMsg(string name, int localPort, CancellationToken cancellationToken)
    {
        udpClient = new UdpClient(localPort);
        serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5050);

        try
        {
            Task receiveTask = Task.Run(() => ReceiveMessages(cancellationToken));

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("Введите сообщение: ");
                string text = Console.ReadLine();

                if (text.ToLower() == "exit")
                    break;

                Console.Write("Введите имя получателя : ");
                string recipient = Console.ReadLine();

                Message msg = new Message(name, recipient, text);
                string responseMsgJs = msg.ToJson();
                byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);

                await udpClient.SendAsync(responseData, responseData.Length, serverEndpoint);
            }
        }
        finally
        {
            udpClient.Close();
        }
    }

    private static async void ReceiveMessages(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult receiveResult = await udpClient.ReceiveAsync();
                byte[] answerData = receiveResult.Buffer;
                string answerMsgJs = Encoding.UTF8.GetString(answerData);
                Message answerMsg = Message.FromJson(answerMsgJs);
                Console.WriteLine(answerMsg.ToString());
            }
        }
        catch (ObjectDisposedException)
        {
            // Исключение произойдет, когда UdpClient закрыт.
            Console.WriteLine("Прием завершен.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при приеме сообщений: {ex.Message}");
        }
    }
}
