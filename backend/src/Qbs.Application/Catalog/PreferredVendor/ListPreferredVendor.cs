using MediatR;
using Qbs.Application.Ports;
using Qbs.Application.Catalog;
using Qbs.Domain.Entities;
using Qbs.Domain.Models;
using Qbs.Domain.ValueObjects;
using Qbs.Domain.Exceptions;

namespace Qbs.Application.Catalog.PreferredVendor;

public sealed record ListPreferredVendor() : IRequest<Qbs.Domain.Entities.PreferredVendor[]>;
