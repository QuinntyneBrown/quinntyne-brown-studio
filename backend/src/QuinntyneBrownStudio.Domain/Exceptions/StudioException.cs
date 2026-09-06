namespace QuinntyneBrownStudio.Domain.Exceptions;

public sealed class StudioException(int status, string message, string field = "request")
    : Exception(message)
{
    public int Status { get; } = status;
    public string Field { get; } = field;
}
