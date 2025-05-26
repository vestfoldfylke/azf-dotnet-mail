using System.Collections.Generic;

namespace Mail.Contracts;

public class SmtPeterMessage : Message
{
    public required IEnumerable<string> Recipients { get; init; }
}