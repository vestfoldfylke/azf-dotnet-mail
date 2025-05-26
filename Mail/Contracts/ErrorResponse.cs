namespace Mail.Contracts;

public record ErrorResponse(string Message, string? ExceptionMessage = null);