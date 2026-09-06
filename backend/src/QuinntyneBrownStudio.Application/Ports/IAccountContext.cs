using QuinntyneBrownStudio.Domain.Models;

namespace QuinntyneBrownStudio.Application.Ports;

public interface IAccountContext { AccountSession Session(); AntiforgeryToken Antiforgery(); }
