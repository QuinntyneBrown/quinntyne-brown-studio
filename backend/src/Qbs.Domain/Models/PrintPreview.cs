namespace Qbs.Domain.Models;

public sealed record PrintPreview(long InputRevision, PrintLine[] Lines, decimal Total);
