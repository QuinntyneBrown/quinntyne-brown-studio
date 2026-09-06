using MediatR;
namespace QuinntyneBrownStudio.Application.Diagnostics;
public sealed record ReceiveDevelopmentUpload(string Key, string Operation, string? BlockId, Stream Body) : IRequest;
