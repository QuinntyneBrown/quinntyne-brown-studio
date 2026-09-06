namespace QuinntyneBrownStudio.Domain.Models;

public sealed record PrintPreview(long InputRevision, PrintLine[] Lines, decimal Total);
