using Qbs.Domain.Models;

namespace Qbs.Application.Ports;

public interface IAccountContext { AccountSession Session(); AntiforgeryToken Antiforgery(); }
