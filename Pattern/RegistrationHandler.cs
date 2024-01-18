// RegistrationHandler.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RegistrationHandler : IMessageHandler
{
    private readonly List<string> connectedClients;

    public RegistrationHandler(List<string> connectedClients)
    {
        this.connectedClients = connectedClients;
    }

    public bool CanHandle(Message message)
    {
        return message.Text.ToLower() == "register" || message.Text.ToLower() == "delete";
    }

    public Task HandleAsync(Message message)
    {
        switch (message.Text.ToLower())
        {
            case "register":
                if (!connectedClients.Contains(message.Sender))
                {
                    connectedClients.Add(message.Sender);
                    Console.WriteLine($"Клиент {message.Sender} зарегистрирован.");
                }
                break;

            case "delete":
                if (connectedClients.Contains(message.Sender))
                {
                    connectedClients.Remove(message.Sender);
                    Console.WriteLine($"Клиент {message.Sender} удален.");
                }
                break;
        }

        return Task.CompletedTask;
    }
}
