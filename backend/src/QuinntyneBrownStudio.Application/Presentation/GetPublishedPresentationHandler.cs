using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Presentation;

public sealed class GetPublishedPresentationHandler(Presentation presentation) : IRequestHandler<GetPublishedPresentation, object>
{
    public Task<object> Handle(GetPublishedPresentation request, CancellationToken ct) =>
        presentation.Public(request.Kind, request.Key);
}
