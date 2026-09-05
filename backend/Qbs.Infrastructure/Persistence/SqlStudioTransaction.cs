using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qbs.Application;
using Qbs.Domain;

namespace Qbs.Infrastructure;

public sealed class SqlStudioTransaction(StudioDbContext db) : IStudioTransaction
{
    public async Task<T?> Get<T>(Guid id)
        where T : Entity
    {
        var row = await db.Records.FindAsync(typeof(T).Name, id);
        return row == null ? null : JsonSerializer.Deserialize<T>(row.Payload, StudioJson.Options);
    }

    public async Task<T[]> List<T>()
        where T : Entity
    {
        var rows = await db.Records.Where(x => x.Kind == typeof(T).Name).ToArrayAsync();
        return rows.Select(x => JsonSerializer.Deserialize<T>(x.Payload, StudioJson.Options)!)
            .ToArray();
    }

    public async Task Save<T>(T value, long expectedVersion)
        where T : Entity
    {
        var row = await db.Records.FindAsync(typeof(T).Name, value.Id);
        if ((row?.Version ?? 0) != expectedVersion)
            throw new StudioException(409, "This record changed. Reload before saving.");
        if (row == null)
        {
            row = new() { Id = value.Id, Kind = typeof(T).Name };
            db.Records.Add(row);
        }
        value.Version = expectedVersion + 1;
        value.ExpectedVersion = 0;
        row.Version = value.Version;
        row.UniqueKey = value switch
        {
            PrintRequest p => $"{p.ClientId}:{p.IdempotencyKey}",
            PublicGallery g => g.Slug,
            AccountToken t => t.Digest,
            _ => null,
        };
        row.Payload = JsonSerializer.Serialize(value, StudioJson.Options);
    }
}
