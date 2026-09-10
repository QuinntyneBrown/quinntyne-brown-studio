using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public record GetDigitalAssetByIdQuery(Guid Id) : IRequest<DigitalAssetDto>;
