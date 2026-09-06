using MediatR;
using DomainEntities = Qbs.Domain.Entities;

namespace Qbs.Application.Catalog.PrintOption;

public sealed record SavePrintOption(DomainEntities.PrintOption Value, Guid? Id)
    : IRequest<DomainEntities.PrintOption>;
