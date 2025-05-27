namespace Mail.Contracts;

public class Template
{
    public required string TemplateName { get; init; }
    public required TemplateData TemplateData { get; init; }
}

public class TemplateData
{
    public required string Body { get; init; }
    public Signature? Signature { get; init; }
}

public class Signature
{
    public string? Name { get; init; }
    public string? Title { get; init; }
    public string? Department { get; init; }
    public string? Company { get; init; }
    public string? Phone { get; init; }
    public string? Mobile { get; init; }
    public string? Webpage { get; init; }
}