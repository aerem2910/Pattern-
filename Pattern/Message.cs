using System;
using System.Text.Json;

public class Message
{
    public string Sender { get; set; }
    public string Recipient { get; set; }
    public string Text { get; set; }
    public DateTime Stime { get; set; }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }

    public static Message? FromJson(string somemessage)
    {
        return JsonSerializer.Deserialize<Message>(somemessage);
    }

    public Message(string sender, string recipient, string text)
    {
        this.Sender = sender;
        this.Recipient = recipient;
        this.Text = text;
        this.Stime = DateTime.Now;
    }

    public Message() { }

    public override string ToString()
    {
        return $"Получено сообщение от {Sender} ({Stime.ToShortTimeString()}): \n {Text}";
    }
}
