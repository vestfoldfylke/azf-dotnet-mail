using System;
using System.Collections.Generic;
using System.ComponentModel;
using HandlebarsDotNet;

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
    
    [Description("Data to be used in the email template")]
    public Template? Template { get; init; }

    [Description("Whether to track clicks in the email")]
    public bool Trackclicks => false;
    
    [Description("Whether to track opens in the email. If set to true, this will rewrite image urls to track opens.")]
    public bool Trackopens => false;

    public SmtPeterMessage GenerateSmtPeterMessage()
    {
        List<string> recipients = [
            ..To,
            ..Cc ?? new List<string>(),
            ..Bcc ?? new List<string>()
        ];

        string? templateBody = null;
        if (Template != null)
        {
            if (!Templates.AllTemplates.TryGetValue(Template.TemplateName, out string? template))
            {
                throw new InvalidOperationException($"Template '{Template.TemplateName}' not found.");
            }

            var templateGenerator = Handlebars.Compile(template);
            templateBody = templateGenerator(Template.TemplateData);
        }
        
        return new SmtPeterMessage
        {
            From = From,
            To = To,
            Recipients = recipients,
            Cc = Cc,
            Bcc = Bcc,
            Subject = Subject,
            Html = templateBody ?? Html,
            Text = Text,
            Attachments = Attachments,
            Extra = Extra
        };
    }
}