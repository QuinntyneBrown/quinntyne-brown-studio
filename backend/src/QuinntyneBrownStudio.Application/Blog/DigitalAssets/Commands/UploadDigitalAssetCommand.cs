using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using QuinntyneBrownStudio.Application.Blog.Services;
using QuinntyneBrownStudio.Domain.Entities.Blog;
using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Application.Blog.DigitalAssets.Queries;
using MediatR;
using SixLabors.ImageSharp;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Commands;

public record UploadDigitalAssetCommand(BlogUploadFile File, Guid UserId) : IRequest<DigitalAssetDto>;
