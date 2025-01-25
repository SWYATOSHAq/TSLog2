namespace TSLog.Common.MessageSending;

public class Message
{
    public List<string> To { get; set; } = new List<string>();
    public required string Subject { get; set; }
    public required string Body { get; set; }
}
