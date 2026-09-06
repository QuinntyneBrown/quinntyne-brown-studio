using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Presentation;

public sealed class GetPublishedPresentationHandler(Presentation presentation) : IRequestHandler<GetPublishedPresentation, object>
{
    public Task<object> Handle(GetPublishedPresentation request, CancellationToken ct) =>
        presentation.Public(request.Kind, request.Key);
}
