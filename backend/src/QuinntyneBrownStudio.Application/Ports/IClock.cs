namespace QuinntyneBrownStudio.Application.Ports;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
