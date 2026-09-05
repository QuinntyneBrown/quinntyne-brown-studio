using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SavePreferredVendor(PreferredVendor Value, Guid? Id)
    : IRequest<PreferredVendor>;
