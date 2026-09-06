using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Domain.Exceptions;
using Qbs.Domain.Policies;

namespace Qbs.Infrastructure.Identity;

public sealed class IdentityAccounts(
    UserManager<IdentityUser<Guid>> users,
    SignInManager<IdentityUser<Guid>> signIn,
    IStudioStore store,
    IClock clock,
    IDataProtectionProvider protection,
    IConfiguration configuration
) : IIdentityAccounts
{
    public async Task<object> Login(string email, string password)
    {
        var result = await signIn.PasswordSignInAsync(email, password, false, true);
        if (!result.Succeeded)
            throw new StudioException(
                401,
                "Unable to sign in. Check your credentials or try again later."
            );
        return new { authenticated = true };
    }

    public Task Logout() => signIn.SignOutAsync();

    public async Task<object> Clients() =>
        (await users.GetUsersInRoleAsync("Client")).Select(x => new { x.Id, x.Email }).ToArray();

    public async Task RequireClients(Guid[] ids)
    {
        foreach (var id in ids)
        {
            var user = await users.FindByIdAsync(id.ToString());
            Rules.Require(
                user != null && await users.IsInRoleAsync(user, "Client"),
                "Assignments must identify client accounts.",
                "clientIds"
            );
        }
    }

    public async Task<Guid> Invite(string email)
    {
        Rules.Require(
            System.Net.Mail.MailAddress.TryCreate(email, out var address)
                && address.Address == email,
            "Enter a valid email.",
            "email"
        );
        var user = await users.FindByEmailAsync(email);
        if (user != null && user.EmailConfirmed)
            throw new StudioException(409, "Account already exists.");
        if (user == null)
        {
            user = new()
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
            };
            Check(await users.CreateAsync(user));
            Check(await users.AddToRoleAsync(user, "Client"));
        }
        return await Token(user, "invitation", TimeSpan.FromHours(24));
    }

    public async Task Recover(string email)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is { EmailConfirmed: true })
            await Token(user, "recovery", TimeSpan.FromHours(1));
    }

    private Task<Guid> Token(IdentityUser<Guid> user, string purpose, TimeSpan lifetime) =>
        store.Run(
            "account:" + user.Id,
            async tx =>
            {
                var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
                var token = new AccountToken
                {
                    AccountId = user.Id,
                    Purpose = purpose,
                    Digest = Digest(raw),
                    ExpiresAt = clock.UtcNow + lifetime,
                };
                await tx.Save(token, 0);
                var origin = configuration["PublicOrigin"] ?? "https://localhost:7443";
                var path = purpose == "invitation" ? "accept-invitation" : "reset-password";
                var payload = JsonSerializer.Serialize(
                    new
                    {
                        recipient = user.Email,
                        subject = "Quinntyne Brown Studio account",
                        body = $"Open {origin}/client/{path}?token={raw} to continue. This link expires.",
                    }
                );
                var job = new BackgroundJob
                {
                    Kind = "Email",
                    ResourceId = token.Id,
                    AvailableAt = clock.UtcNow,
                    Payload = protection.CreateProtector("qbs-email-v1").Protect(payload),
                };
                await tx.Save(job, 0);
                return token.Id;
            }
        );

    public async Task<object> Accept(string token, string password, string purpose)
    {
        IdentityUser<Guid>? account = null;
        await store.Run(
            "token:" + Digest(token),
            async tx =>
            {
                var entry = (await tx.List<AccountToken>()).SingleOrDefault(x =>
                    x.Digest == Digest(token) && x.Purpose == purpose
                );
                if (entry == null || entry.Used || entry.ExpiresAt <= clock.UtcNow)
                    throw new StudioException(400, "This link is invalid or expired.");
                account =
                    await users.FindByIdAsync(entry.AccountId.ToString())
                    ?? throw new StudioException(400, "This link is invalid or expired.");
                if (purpose == "invitation")
                {
                    if (account.EmailConfirmed)
                        throw new StudioException(400, "This invitation has already been used.");
                    Check(await users.AddPasswordAsync(account, password));
                    account.EmailConfirmed = true;
                    Check(await users.UpdateAsync(account));
                }
                else
                {
                    var reset = await users.GeneratePasswordResetTokenAsync(account);
                    Check(await users.ResetPasswordAsync(account, reset, password));
                }
                entry.Used = true;
                await tx.Save(entry, entry.Version);
                return true;
            }
        );
        if (purpose == "invitation")
            await signIn.SignInAsync(account!, false);
        return new { success = true };
    }

    private static string Digest(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new StudioException(
                400,
                string.Join(" ", result.Errors.Select(x => x.Description))
            );
    }
}
