using MediatR;
using Qbs.Domain;

namespace Qbs.Application;

public sealed record SavePublicGallery(PublicGallery Value, Guid? Id) : IRequest<PublicGallery>;
