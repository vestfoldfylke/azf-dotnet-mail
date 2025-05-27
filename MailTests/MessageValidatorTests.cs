using FluentValidation.Results;
using Mail.Contracts;
using Mail.Validation;

namespace MailTests;

public class MessageValidatorTests
{
    [Fact]
    public void Invalid_Message()
    {
        Message message = CreateMessage();

        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(6, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName == "From");
        Assert.Contains(results.Errors, error => error.PropertyName == "To");
        Assert.Contains(results.Errors, error => error.PropertyName == "Subject");
        Assert.Contains(results.Errors, error => error.PropertyName == "Html");
        Assert.Contains(results.Errors, error => error.PropertyName == "Text");
        Assert.Contains(results.Errors, error => error.PropertyName == "Template");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("foo")]
    [InlineData("Heisann sveisann <>")]
    public void Invalid_From(string from)
    {
        string[] recipients = ["bar@biz.foo"];
        var message = CreateMessage(
            from,
            to: [..recipients],
            subject: "Test",
            text: "Test",
            html: "<p>Test</p>");
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(from == "" ? 2 : 1, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName.Contains("From"));
    }
    
    [Fact]
    public void Invalid_To()
    {
        string[] recipients = [""];
        var message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            subject: "Test",
            text: "Test",
            html: "<p>Test</p>");
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(2, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName.Contains("To"));
    }

    [Theory]
    [InlineData("SGVpc2FubiBzdmVpc2Fubg==", null, null!, "text/plain")]
    [InlineData("SGVpc2FubiBzdmVpc2Fubg==", null, "Test", null!)]
    [InlineData("S==", null, "Test", "text/plain")]
    [InlineData(null, "https://example.com/test.txt", null!, "text/plain")]
    [InlineData(null, "https://example.com/test.txt", "Test2", null!)]
    [InlineData(null, "example.com", "Test2", "text/plain")]
    public void Invalid_Attachments(string? data, string? url, string name, string type)
    {
        string[] recipients = ["bar@biz.foo"];
        var message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            subject: "Test",
            text: "Test",
            html: "<p>Test</p>",
            attachments:
            [
                new Attachment
                {
                    Data = data,
                    Url = url,
                    Name = name,
                    Type = type
                }
            ]);
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Single(results.Errors);
    }
    
    [Fact]
    public void Invalid_Cc()
    {
        string[] recipients = ["bar@biz.foo"];
        var message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            subject: "Test",
            text: "Test",
            html: "<p>Test</p>",
            cc: [""]);
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(2, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName.Contains("Cc"));
    }
    
    [Fact]
    public void Invalid_Extra()
    {
        string[] recipients = ["bar@biz.foo"];
        var message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            subject: "Test",
            text: "Test",
            html: "<p>Test</p>",
            extra: new Dictionary<string, string>
            {
                {"foo", "bar"},
                {"x-", "foo"}
            });
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(2, results.Errors.Count);
    }
    
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Minimal_Valid_Message(bool useHtml, bool useText)
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            subject: "Test",
            html: useHtml ? "<p>Test</p>" : null,
            text: useText ? "Test" : null);

        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.True(results.IsValid);

        Assert.Empty(results.Errors);
    }
    
    [Fact]
    public void Minimal_Valid_Message_With_Template()
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            subject: "Test",
            template: new Template
            {
                TemplateName = "vestfoldfylke",
                TemplateData = new TemplateData
                {
                    Body = "<p>Test</p>",
                    Signature = new Signature
                    {
                        Name = "Test"
                    }
                }
            });

        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        var smtPeterMessage = message.GenerateSmtPeterMessage();
        
        Assert.True(results.IsValid);

        Assert.Empty(results.Errors);
        
        Assert.Equal(message.To.Count(), smtPeterMessage.Recipients.Count());
        Assert.Equal(message.From, smtPeterMessage.From);
        Assert.Equal(message.Subject, smtPeterMessage.Subject);
        Assert.NotEqual(message.Template!.TemplateData.Body, smtPeterMessage.Html);
        Assert.Contains(message.Template!.TemplateData.Body, smtPeterMessage.Html);
        Assert.Contains("Vestfold fylkeskommune", smtPeterMessage.Html);
    }
    
    [Fact]
    public void Maximum_Valid_Message()
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            cc: [..recipients],
            bcc: [..recipients],
            "Test",
            "<p>Test</p>",
            "Test",
            attachments:
            [
                new Attachment
                {
                    Url = "https://example.com/test.txt",
                    Name = "Test",
                    Type = "text/plain"
                },
                new Attachment
                {
                    Data = "SGVpc2FubiBzdmVpc2Fubg==",
                    Name = "Test",
                    Type = "text/plain"
                }
            ],
            extra: new Dictionary<string, string>
            {
                { "x-foo", "bar"},
                { "x-bar", "foo"}
            });

        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.True(results.IsValid);

        Assert.Empty(results.Errors);
    }
    
    [Fact]
    public void Maximum_Valid_Message_With_Template()
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            to: [..recipients],
            cc: [..recipients],
            bcc: [..recipients],
            "Test",
            attachments:
            [
                new Attachment
                {
                    Url = "https://example.com/test.txt",
                    Name = "Test",
                    Type = "text/plain"
                },
                new Attachment
                {
                    Data = "SGVpc2FubiBzdmVpc2Fubg==",
                    Name = "Test",
                    Type = "text/plain"
                }
            ],
            extra: new Dictionary<string, string>
            {
                { "x-foo", "bar"},
                { "x-bar", "foo"}
            },
            template: new Template
            {
                TemplateName = "telemarkfylke",
                TemplateData = new TemplateData
                {
                    Body = "<p>Test</p>",
                    Signature = new Signature
                    {
                        Name = "Test",
                        Title = "Test",
                        Department = "Test",
                        Company = "Test",
                        Phone = "12345678",
                        Mobile = "87654321",
                        Webpage = "https://example.com"
                    }
                }
            });

        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        var smtPeterMessage = message.GenerateSmtPeterMessage();
        
        Assert.True(results.IsValid);

        Assert.Empty(results.Errors);
        
        Assert.Equal(recipients.Length * 3, smtPeterMessage.Recipients.Count());
        Assert.Equal(message.From, smtPeterMessage.From);
        Assert.Equal(message.Subject, smtPeterMessage.Subject);
        Assert.NotEqual(message.Template!.TemplateData.Body, smtPeterMessage.Html);
        Assert.Contains(message.Template!.TemplateData.Body, smtPeterMessage.Html);
        Assert.Contains(message.Template!.TemplateData.Signature!.Phone!, smtPeterMessage.Html);
        Assert.Contains(message.Template!.TemplateData.Signature!.Mobile!, smtPeterMessage.Html);
        Assert.Contains(message.Template!.TemplateData.Signature!.Webpage!, smtPeterMessage.Html);
        Assert.Contains("Telemark fylkeskommune", smtPeterMessage.Html);
    }
    
    // TODO: Create tests for Template validation when implemented
    
    private static Message CreateMessage(string from = null!, IEnumerable<string> to = null!, IEnumerable<string>? cc = null, IEnumerable<string>? bcc = null,
        string subject = null!, string? html = null, string? text = null, Template? template = null, IEnumerable<Attachment>? attachments = null,
        Dictionary<string, string>? extra = null) => new Message
        {
            From = from,
            To = to,
            Cc = cc,
            Bcc = bcc,
            Subject = subject,
            Html = html,
            Text = text,
            Template = template,
            Attachments = attachments,
            Extra = extra
        };
}