using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.Seo;

public sealed record GetBlogDocumentQuery(string Format) : IRequest<BlogDocument>;
