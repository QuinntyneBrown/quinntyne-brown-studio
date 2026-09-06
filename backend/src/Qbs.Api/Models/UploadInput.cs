using Qbs.Domain.Models;

namespace Qbs.Api.Models;

public sealed record UploadInput(UploadEntry[] Files);
