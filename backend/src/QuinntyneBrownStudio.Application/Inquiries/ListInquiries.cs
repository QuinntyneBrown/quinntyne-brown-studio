using MediatR;
using QuinntyneBrownStudio.Domain.Entities;

namespace QuinntyneBrownStudio.Application.Inquiries;

/// <summary>The administrator inbox, newest first, optionally limited to one state.</summary>
public sealed record ListInquiries(string? State) : IRequest<Inquiry[]>;
