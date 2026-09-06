using MediatR;
using Qbs.Domain.Models;

namespace Qbs.Application.Clients;

public sealed record PreviewPrintRequest(Guid Client, PrintPreviewInput Value) : IRequest<PrintPreview>;
