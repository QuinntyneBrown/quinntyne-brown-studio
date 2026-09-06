namespace QuinntyneBrownStudio.Qualification;

public sealed record QualificationObservation(string Name, bool Passed, string Message, Dictionary<string, object?> Metrics);
