using System.Collections.Generic;

namespace Mail.Contracts;

public record Message(string From, IEnumerable<string> Recipients, string Subject, string Body);