using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Photos;

public sealed class GetPhotoAnalysisHandler(AnalysisWorkflows analysis) : IRequestHandler<GetPhotoAnalysis, object>
{
    public Task<object> Handle(GetPhotoAnalysis request, CancellationToken ct) =>
        analysis.Status(request.Id);
}
