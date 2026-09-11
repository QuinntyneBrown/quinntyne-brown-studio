using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;

public sealed record GetBlogAssetQuery(string FileName, int? Width, string Accept, string IfNoneMatch) : IRequest<BlogAssetResult>;
