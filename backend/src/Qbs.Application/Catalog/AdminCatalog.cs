using System.Net.Mail;
using System.Text.RegularExpressions;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Domain.Enums;
using Qbs.Domain.Exceptions;
using Qbs.Domain.Policies;
using Qbs.Domain.ValueObjects;
using DomainEntities = Qbs.Domain.Entities;
using SchedulingService = Qbs.Application.Scheduling.Scheduling;

namespace Qbs.Application.Catalog;

public sealed class AdminCatalog(IStudioStore store)
{
    public static readonly Guid ConfigurationId = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );

    public Task<T[]> List<T>()
        where T : Entity => store.Run(typeof(T).Name, tx => tx.List<T>());

    public Task<T?> Get<T>(Guid id)
        where T : Entity => store.Run(typeof(T).Name, tx => tx.Get<T>(id));

    public Task<T> Save<T>(T value, Guid? id = null)
        where T : Entity =>
        store.Run(
            Key(value),
            async tx =>
            {
                value.Id = id ?? Guid.NewGuid();
                var old = await tx.Get<T>(value.Id);
                if (
                    id != null
                    && old == null
                    && typeof(T) != typeof(DomainEntities.RateConfiguration)
                    && typeof(T) != typeof(DomainEntities.DiscountConfiguration)
                )
                    throw new StudioException(404, "Record not found.");
                var expected = id == null ? 0 : value.ExpectedVersion;
                await Validate(tx, value, old);
                await tx.Save(value, expected);
                if (value is DomainEntities.RateConfiguration or DomainEntities.DiscountConfiguration or DomainEntities.Studio)
                {
                    var revision =
                        await tx.Get<ConfigurationRevision>(ConfigurationId)
                        ?? new() { Id = ConfigurationId };
                    revision.Revision++;
                    await tx.Save(revision, revision.Version);
                }
                return value;
            }
        );

    private static string Key(Entity e) =>
        e switch
        {
            DomainEntities.RateConfiguration or DomainEntities.DiscountConfiguration or DomainEntities.Studio => "pricing",
            DomainEntities.Session s => "photographer:" + s.PhotographerId,
            DomainEntities.PublicGallery => "media",
            _ => e.GetType().Name,
        };

    private static void Nonnegative(decimal? number, string field) =>
        Rules.Require(number == null || number >= 0, "Must be nonnegative.", field);

    private async Task Validate<T>(IStudioTransaction tx, T value, T? old)
        where T : Entity
    {
        switch (value)
        {
            case DomainEntities.Equipment e:
                Rules.Text(e.Name, "name");
                Rules.Require(e.Quantity >= 0, "Quantity must be nonnegative.", "quantity");
                Nonnegative(e.ReferenceRentalRate, "referenceRentalRate");
                break;
            case DomainEntities.PreferredVendor v:
                Rules.Text(v.Name, "name");
                Rules.Require(
                    v.Roles.Length > 0 && v.Roles.All(Enum.IsDefined),
                    "Select supported vendor roles.",
                    "roles"
                );
                Rules.Require(
                    !string.IsNullOrWhiteSpace(v.Email) || !string.IsNullOrWhiteSpace(v.Phone),
                    "Provide a contact method.",
                    "email"
                );
                if (!string.IsNullOrWhiteSpace(v.Email))
                    Rules.Require(
                        MailAddress.TryCreate(v.Email, out var mail) && mail.Address == v.Email,
                        "Enter a valid email.",
                        "email"
                    );
                if (!string.IsNullOrWhiteSpace(v.Phone))
                    Rules.Require(
                        Regex.IsMatch(v.Phone, @"^[+0-9 ()-]{7,30}$"),
                        "Enter a valid phone number.",
                        "phone"
                    );
                v.Roles = v.Roles.Distinct().ToArray();
                break;
            case DomainEntities.Promotion p:
                Rules.Text(p.Title, "title");
                Rules.Text(p.Description, "description", 10000);
                Nonnegative(p.IndicativePrice, "indicativePrice");
                break;
            case DomainEntities.PrintOption p:
                Rules.Text(p.Name, "name");
                Rules.Text(p.Dimensions, "dimensions");
                Rules.Text(p.Finish, "finish");
                Nonnegative(p.UnitPrice, "unitPrice");
                p.UnitPrice = Rules.Round(p.UnitPrice);
                break;
            case DomainEntities.Photographer p:
                Rules.Text(p.Name, "name");
                break;
            case DomainEntities.Studio s:
                Rules.Text(s.Name, "name");
                ValidateLocation(s.ResolvedAddress);
                Nonnegative(s.HourlyFee, "hourlyFee");
                if (s.IsBase)
                    foreach (
                        var previous in (await tx.List<DomainEntities.Studio>()).Where(x =>
                            x.IsBase && x.Id != s.Id
                        )
                    )
                    {
                        previous.IsBase = false;
                        await tx.Save(previous, previous.Version);
                    }
                break;
            case DomainEntities.RateConfiguration r:
                Rules.Require(r.ServiceRates.Keys.All(Enum.IsDefined), "Unknown service.");
                Rules.Require(
                    r.CostRates.Keys.All(x =>
                        new[] { "travel", "equipment", "lunch", "assistant" }.Contains(x)
                    ),
                    "Unknown cost rate."
                );
                foreach (var rate in r.ServiceRates.Values.Concat(r.CostRates.Values))
                    Nonnegative(rate, "rates");
                break;
            case DomainEntities.DiscountConfiguration d:
                var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var rule in d.CodeRules.Append(d.AdvanceRule).Append(d.WeekdayRule))
                {
                    Rules.Require(
                        rule.Percentage >= 0 && rule.Percentage <= 100,
                        "Percentage must be between 0 and 100.",
                        "percentage"
                    );
                    Rules.Require(
                        rule.Threshold >= 0,
                        "Threshold must be nonnegative.",
                        "threshold"
                    );
                    Rules.Require(
                        rule.ValidFrom == null
                            || rule.ValidTo == null
                            || rule.ValidFrom <= rule.ValidTo,
                        "Invalid validity dates."
                    );
                    Rules.Require(rule.Weekdays.All(Enum.IsDefined), "Invalid weekday.");
                }
                foreach (var code in d.CodeRules)
                {
                    code.Code = code.Code?.Trim().ToUpperInvariant();
                    Rules.Text(code.Code, "code", 100);
                    Rules.Require(codes.Add(code.Code!), "Codes must be unique.", "code");
                }
                break;
            case DomainEntities.Session s:
                Rules.Text(s.Name, "name");
                Rules.Require(Enum.IsDefined(s.Service), "Select a valid service.");
                Rules.Interval(s.StartsAt, s.EndsAt);
                var prior = old as DomainEntities.Session;
                s.ClientIds = prior?.ClientIds ?? [];
                s.ExpiresAt = prior?.ExpiresAt;
                s.RetentionMonths = prior?.RetentionMonths ?? 12;
                s.ExpiryRevision = prior?.ExpiryRevision ?? 0;
                s.NoticeRevision = prior?.NoticeRevision ?? 0;
                s.RetentionState = prior?.RetentionState ?? "Active";
                if (s.PhotographerId is Guid photographer)
                {
                    var available = await SchedulingService.Availability(
                        tx,
                        s.StartsAt,
                        s.EndsAt,
                        photographer,
                        s.Id
                    );
                    if (!available.Available)
                        throw new StudioException(
                            409,
                            "Photographer cannot accommodate this interval.",
                            "photographerId"
                        );
                }
                break;
            case DomainEntities.PublicGallery g:
                Rules.Text(g.Title, "title");
                Rules.Require(
                    Regex.IsMatch(g.Slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"),
                    "Use a lowercase URL slug.",
                    "slug"
                );
                Rules.Require(
                    g.PhotoIds.Distinct().Count() == g.PhotoIds.Length,
                    "Photos must be unique."
                );
                if ((await tx.List<DomainEntities.PublicGallery>()).Any(x => x.Id != g.Id && x.Slug == g.Slug))
                    throw new StudioException(409, "Slug already exists.");
                foreach (var photoId in g.PhotoIds)
                {
                    var photo = await tx.Get<SessionPhoto>(photoId);
                    Rules.Require(
                        photo?.State == PhotoState.Ready,
                        "Only ready photos can be published."
                    );
                    var session = await tx.Get<DomainEntities.Session>(photo!.SessionId);
                    Rules.Require(
                        session?.RetentionState != "DeletionPending"
                            && session?.RetentionState != "Deleted",
                        "Session is being deleted."
                    );
                }
                break;
        }
    }

    public static void ValidateLocation(ResolvedLocation l)
    {
        Rules.Text(l.Label, "address");
        Rules.Require(
            l.Latitude >= -90 && l.Latitude <= 90 && l.Longitude >= -180 && l.Longitude <= 180,
            "Select a resolved address.",
            "address"
        );
    }
}
