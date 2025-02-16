namespace TSLog.Common.MessageSending;

public enum MessageType
{
    Email,
    Telegram,
    WhatsApp
}

public static class MessageSenderFactory
{
    private static readonly EmailConfiguration DefaultEmailConfiguration = new()
    {
        From = "infowms@tslog.ru",
        SmtpServer = "smtp.yandex.ru",
        Port = 465,
        UserName = "infowms@tslog.ru",
        Password = "InfoWMS$0"
    };

    public static IMessageSender CreateSender(MessageType type)
    {
        switch (type)
        {
            case MessageType.Email:
                return new EmailSender(DefaultEmailConfiguration);
            default:
                throw new ArgumentException("Unsupported sender type", nameof(type));
        }
    }
}
