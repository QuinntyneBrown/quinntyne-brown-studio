namespace QuinntyneBrownStudio.Application.Ports;

public interface IJobQueue
{
    Task Send(Guid id, CancellationToken ct);
    Task<(Guid Id, string Receipt)?> Receive(CancellationToken ct);
    Task Complete(string receipt, CancellationToken ct);
}
