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
        Assert.Contains(results.Errors, error => error.PropertyName == "Recipients");
        Assert.Contains(results.Errors, error => error.PropertyName == "Subject");
        Assert.Contains(results.Errors, error => error.PropertyName == "Html");
        Assert.Contains(results.Errors, error => error.PropertyName == "Text");
        Assert.Contains(results.Errors, error => error.PropertyName == "To");
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("foo")]
    [InlineData("Heisann sveisann <>")]
    public void Invalid_From(string from)
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            from,
            [..recipients],
            "Test",
            "<p>Test</p>",
            to: [..recipients]);
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(from == "" ? 2 : 1, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName.Contains("From"));
    }
    
    [Fact]
    public void Invalid_Recipients()
    {
        string[] recipients = [""];
        Message message = CreateMessage(
            "foo@bar.biz",
            [..recipients],
            "Test",
            "<p>Test</p>",
            to: [..recipients]);
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(4, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName.Contains("Recipients"));
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
        Message message = CreateMessage(
            "foo@bar.biz",
            [..recipients],
            "Test",
            "<p>Test</p>",
            attachments:
            [
                new Attachment
                {
                    Data = data,
                    Url = url,
                    Name = name,
                    Type = type
                }
            ],
            to: [..recipients]);
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Single(results.Errors);
    }
    
    [Fact]
    public void Invalid_Cc()
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            [..recipients],
            "Test",
            "<p>Test</p>",
            cc: [""],
            to: [..recipients]);
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(2, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName.Contains("Cc"));
    }
    
    [Fact]
    public void Invalid_ReplyTo()
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            [..recipients],
            "Test",
            "<p>Test</p>",
            replyTo: [""],
            to: [..recipients]);
        
        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.False(results.IsValid);

        Assert.Equal(2, results.Errors.Count);
        Assert.Contains(results.Errors, error => error.PropertyName.Contains("ReplyTo"));
    }
    
    [Fact]
    public void Invalid_Extra()
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            [..recipients],
            "Test",
            "<p>Test</p>",
            to: [..recipients],
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
            [..recipients],
            "Test",
            useHtml ? "<p>Test</p>" : null,
            useText ? "Test" : null,
            to: [..recipients]);

        MessageValidator validator = new MessageValidator();

        ValidationResult results = validator.Validate(message);
        
        Assert.True(results.IsValid);

        Assert.Empty(results.Errors);
    }
    
    [Fact]
    public void Maximum_Valid_Message()
    {
        string[] recipients = ["bar@biz.foo"];
        Message message = CreateMessage(
            "foo@bar.biz",
            [..recipients],
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
            cc: [..recipients],
            replyTo: [..recipients],
            to: [..recipients],
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
    
    private static Message CreateMessage(string from = null!, IEnumerable<string> recipients = null!, string subject = null!,
        string? html = null, string? text = null, IEnumerable<Attachment>? attachments = null,
        IEnumerable<string>? cc = null, IEnumerable<string>? replyTo = null, IEnumerable<string>? to = null,
        Dictionary<string, string>? extra = null) => new Message
        {
            From = from,
            Recipients = recipients,
            Subject = subject,
            Html = html,
            Text = text,
            Attachments = attachments,
            Cc = cc,
            ReplyTo = replyTo,
            To = to,
            Extra = extra
        };
}