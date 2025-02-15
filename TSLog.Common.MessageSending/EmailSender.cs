using MailKit.Net.Smtp;
using MimeKit;

namespace TSLog.Common.MessageSending;

public class EmailSender : IMessageSender
{
    private readonly EmailConfiguration _emailConfig;

    public EmailSender(EmailConfiguration emailConfig)
    {
        _emailConfig = emailConfig;
    }

    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        try
        {
            emailMessage.From.Add(MailboxAddress.Parse(_emailConfig.From));
            foreach (var recipient in message.To)
            {
                if (MailboxAddress.TryParse(recipient, out var mailboxAddress))
                {
                    emailMessage.To.Add(mailboxAddress);
                }
                else
                {
                    continue;
                }
            }
        }
        catch (FormatException)
        {
            throw;
        }

        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Body };
        return emailMessage;
    }

    public async Task SendAsync(Message message)
    {
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
            // Remove OAuth2 authentication to use traditional login.
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
            var emailMessage = CreateEmailMessage(message);
            await client.SendAsync(emailMessage);
        }
        catch
        {
            throw;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}
