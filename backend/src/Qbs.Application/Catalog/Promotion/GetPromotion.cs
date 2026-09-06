using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.Promotion;

public sealed record GetPromotion(Guid Id) : IRequest<Qbs.Domain.Entities.Promotion>;
