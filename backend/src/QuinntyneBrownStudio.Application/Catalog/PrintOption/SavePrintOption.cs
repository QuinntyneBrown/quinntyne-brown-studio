using MediatR;
using DomainEntities = QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Catalog.PrintOption;

public sealed record SavePrintOption(DomainEntities.PrintOption Value, Guid? Id)
    : IRequest<DomainEntities.PrintOption>;
