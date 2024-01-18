public interface IMessageHandler
{
    bool CanHandle(Message message);
    Task HandleAsync(Message message);
}
