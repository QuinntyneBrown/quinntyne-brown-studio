using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class StartPhotoAnalysisHandler(AnalysisWorkflows analysis) : IRequestHandler<StartPhotoAnalysis, object>
{
    public Task<object> Handle(StartPhotoAnalysis request, CancellationToken ct) =>
        analysis.Request(request.Id, request.PhotoIds);
}
