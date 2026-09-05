using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SavePrintOption(PrintOption Value, Guid? Id) : IRequest<PrintOption>;
