using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class RetryPhotoAnalysisHandler(AnalysisWorkflows analysis) : IRequestHandler<RetryPhotoAnalysis, object>
{
    public Task<object> Handle(RetryPhotoAnalysis request, CancellationToken ct) =>
        analysis.Retry(request.Id, request.PhotoIds);
}
