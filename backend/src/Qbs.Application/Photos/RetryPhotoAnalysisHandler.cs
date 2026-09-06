using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class RetryPhotoAnalysisHandler(AnalysisWorkflows analysis) : IRequestHandler<RetryPhotoAnalysis, object>
{
    public Task<object> Handle(RetryPhotoAnalysis request, CancellationToken ct) =>
        analysis.Retry(request.Id, request.PhotoIds);
}
