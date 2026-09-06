using MediatR;
using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Application.Catalog;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Models;
using QuinntyneBrownStudio.Domain.ValueObjects;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Photos;

public sealed class GetPhotoAnalysisHandler(AnalysisWorkflows analysis) : IRequestHandler<GetPhotoAnalysis, object>
{
    public Task<object> Handle(GetPhotoAnalysis request, CancellationToken ct) =>
        analysis.Status(request.Id);
}
