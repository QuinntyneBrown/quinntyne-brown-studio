using QuinntyneBrownStudio.Application.Ports.Blog;

using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public record GetDigitalAssetsQuery(Guid UserId) : IRequest<List<DigitalAssetDto>>;
