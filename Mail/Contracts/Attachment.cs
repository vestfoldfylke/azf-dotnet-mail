using System.ComponentModel;

namespace Mail.Contracts;

public class Attachment
{
    [Description("The content of the attachment. This is a base64 encoded string")]
    public string? Data { get; init; }
    
    [Description("A URL to the attachment")]
    public string? Url { get; init; }
    
    [Description("The name of the attachment")]
    public required string Name { get; init; }
    
    [Description("The MIME type of the attachment")]
    public required string Type { get; init; }
}