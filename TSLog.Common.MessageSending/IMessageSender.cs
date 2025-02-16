namespace TSLog.Common.MessageSending;

public interface IMessageSender
{
    Task SendAsync(Message message);
}
