using Qbs.Domain;

namespace Qbs.Api.Controllers;

public sealed record UploadInput(UploadEntry[] Files);
