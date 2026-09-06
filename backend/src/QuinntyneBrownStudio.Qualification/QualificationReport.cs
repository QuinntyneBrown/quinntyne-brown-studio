namespace QuinntyneBrownStudio.Qualification;

public sealed record QualificationReport(string Gate, string Status, string Message, QualificationObservation[] Observations)
{
    public bool GateClosed => false;
    public DateTimeOffset RecordedAt { get; } = DateTimeOffset.UtcNow;
    public string RunId { get; } = Guid.NewGuid().ToString("N");
    public string Runtime { get; } = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
}
