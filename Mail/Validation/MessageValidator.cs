using System;
using System.Buffers.Text;
using FluentValidation;
using Mail.Contracts;

namespace Mail.Validation;

public class MessageValidator : AbstractValidator<Message>
{
    public MessageValidator()
    {
        RuleFor(message => message.From)
            .NotEmpty()
            .WithMessage("{PropertyName} field is required")
            .EmailAddress()
            .WithMessage("{PropertyName} has invalid email address format");

        RuleFor(message => message.Subject)
            .NotEmpty()
            .WithMessage("Subject field is required");
        
        RuleFor(message => message.Html)
            .NotEmpty()
            .When(message => string.IsNullOrEmpty(message.Text))
            .WithMessage("{PropertyName} field is required since Text field is not provided");
        
        RuleFor(message => message.Text)
            .NotEmpty()
            .When(message => string.IsNullOrEmpty(message.Html))
            .WithMessage("{PropertyName} field is required since Html field is not provided");

        RuleForEach(message => message.Attachments)
            .ChildRules(messageAttachment =>
            {
                messageAttachment.RuleFor(attachment => attachment.Name)
                    .NotEmpty()
                    .WithMessage("Attachment.{PropertyName} field is required");
                
                messageAttachment.RuleFor(attachment => attachment.Type)
                    .NotEmpty()
                    .WithMessage("Attachment.{PropertyName} field is required");

                messageAttachment.RuleFor(attachment => attachment.Data)
                    .NotEmpty()
                    .When(attachment => string.IsNullOrEmpty(attachment.Url))
                    .WithMessage("Attachment.{PropertyName} field is required since Url field is not provided");

                messageAttachment.RuleFor(attachment => attachment.Data)
                    .Must(data => Base64.IsValid(data))
                    .When(attachment => !string.IsNullOrEmpty(attachment.Data))
                    .WithMessage("Attachment.{PropertyName} field is not a valid Base64 encoded string");

                messageAttachment.RuleFor(attachment => attachment.Url)
                    .NotEmpty()
                    .When(attachment => string.IsNullOrEmpty(attachment.Data))
                    .WithMessage("Attachment.{PropertyName} field is required since Data field is not provided");
                
                messageAttachment.RuleFor(attachment => attachment.Url)
                    .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .When(attachment => !string.IsNullOrEmpty(attachment.Url))
                    .WithMessage("Attachment.{PropertyName} field is not a valid URL");
            });
        
        RuleForEach(message => message.Cc)
            .NotEmpty()
            .WithMessage("{PropertyName} can not have empty values")
            .EmailAddress()
            .WithMessage("{PropertyName} with {PropertyValue} has invalid email address format");
        
        RuleForEach(message => message.Bcc)
            .NotEmpty()
            .WithMessage("{PropertyName} can not have empty values")
            .EmailAddress()
            .WithMessage("{PropertyName} with {PropertyValue} has invalid email address format");
        
        RuleFor(message => message.To)
            .NotEmpty()
            .WithMessage("{PropertyName} field is required to have at least one recipient");
        
        RuleForEach(message => message.To)
            .NotEmpty()
            .WithMessage("{PropertyName} can not have empty values")
            .EmailAddress()
            .WithMessage("{PropertyName} with {PropertyValue} has invalid email address format");
        
        RuleForEach(message => message.Extra)
            .ChildRules(extra =>
            {
                extra.RuleFor(x => x.Key)
                    .NotEmpty()
                    .WithMessage("{PropertyName} field is required")
                    .Must(x => x.StartsWith("x-") && x.Length > 2)
                    .WithMessage("Extra['{PropertyValue}'] must be minimum 3 characters long and start with 'x-'");
                
                extra.RuleFor(x => x.Value)
                    .NotEmpty()
                    .WithMessage("{PropertyName} field is required");
            });
    }
}