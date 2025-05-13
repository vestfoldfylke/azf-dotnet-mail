using System.Collections.Generic;
using System.ComponentModel;

namespace Mail.Contracts;

public class Message
{
    [Description("Sender email address")]
    public required string From { get; init; }
    
    [Description("Email addresses of the recipients")]
    public required IEnumerable<string> Recipients { get; init; }
    
    [Description("Subject of the email")]
    public required string Subject { get; init; }
    
    [Description("Html body of the email")]
    public string? Html { get; init; }
    
    [Description("Text body of the email (used if HTML is not supported on the client or when the receiver prefers text mode)")]
    public string? Text { get; init; }
    
    [Description("List of attachments to be sent with the email")]
    public IEnumerable<Attachment>? Attachments { get; init; }
    
    [Description("Email addresses of the Cc recipients as shown in the email")]
    public IEnumerable<string>? Cc { get; init; }
    
    [Description("Email addresses of the ReplyTo recipient (if not given, the recipient will be used)")]
    public IEnumerable<string>? ReplyTo { get; init; }
    
    [Description("Email addresses of the recipients as shown in the email")]
    public IEnumerable<string>? To { get; init; }
    
    [Description("Extra 'x-*' custom headers")]
    public Dictionary<string, string>? Extra { get; init; }
}