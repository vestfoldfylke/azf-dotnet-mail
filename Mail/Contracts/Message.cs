using System.Collections.Generic;
using System.ComponentModel;

namespace Mail.Contracts;

public class Message
{
    [Description("Sender email address")]
    public required string From { get; init; }
    
    [Description("Email addresses of the recipients")]
    public required IEnumerable<string> To { get; init; }
    
    [Description("Email addresses of the Cc recipients")]
    public IEnumerable<string>? Cc { get; init; }
    
    [Description("Email addresses of the Bcc recipients")]
    public IEnumerable<string>? Bcc { get; init; }
    
    [Description("Subject of the email")]
    public required string Subject { get; init; }
    
    [Description("Html body of the email")]
    public string? Html { get; init; }
    
    [Description("Text body of the email (used if HTML is not supported on the client or when the receiver prefers text mode)")]
    public string? Text { get; init; }
    
    [Description("List of attachments to be sent with the email")]
    public IEnumerable<Attachment>? Attachments { get; init; }
    
    [Description("Extra 'x-*' custom headers")]
    public Dictionary<string, string>? Extra { get; init; }

    public SmtPeterMessage GenerateSmtPeterMessage()
    {
        List<string> recipients = [
            ..To
        ];
        
        if (Cc != null)
        {
            recipients.AddRange(Cc);
        }
        
        if (Bcc != null)
        {
            recipients.AddRange(Bcc);
        }
        
        return new SmtPeterMessage
        {
            From = From,
            To = To,
            Recipients = recipients,
            Cc = Cc ?? new List<string>(),
            Bcc = Bcc ?? new List<string>(),
            Subject = Subject,
            Html = Html,
            Text = Text,
            Attachments = Attachments ?? new List<Attachment>(),
            Extra = Extra ?? new Dictionary<string, string>()
        };
    }
}