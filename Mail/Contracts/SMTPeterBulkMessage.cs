using System.Collections.Generic;

namespace Mail.Contracts;

public class SmtPeterBulkMessage : BulkMessage
{
    public string To => "{$toAddress}";
    public required Dictionary<string, SmtPeterBulkRecipient> Recipients { get; init; }
}

public class SmtPeterBulkRecipient
{
    public required string ToAddress { get; init; }
}