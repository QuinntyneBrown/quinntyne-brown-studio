using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class StartPhotoAnalysisHandler(AnalysisWorkflows analysis) : IRequestHandler<StartPhotoAnalysis, object>
{
    public Task<object> Handle(StartPhotoAnalysis request, CancellationToken ct) =>
        analysis.Request(request.Id, request.PhotoIds);
}
