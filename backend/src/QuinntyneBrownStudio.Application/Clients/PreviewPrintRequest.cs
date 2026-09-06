using MediatR;
using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Clients;

public sealed record PreviewPrintRequest(Guid Client, PrintPreviewInput Value) : IRequest<PrintPreview>;
