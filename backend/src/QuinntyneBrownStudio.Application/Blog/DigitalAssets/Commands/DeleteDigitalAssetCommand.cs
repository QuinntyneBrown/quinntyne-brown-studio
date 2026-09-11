using QuinntyneBrownStudio.Application.Ports.Blog;
using QuinntyneBrownStudio.Domain.Exceptions.Blog;
using MediatR;

namespace QuinntyneBrownStudio.Application.Blog.DigitalAssets.Commands;

public record DeleteDigitalAssetCommand(Guid Id) : IRequest;
