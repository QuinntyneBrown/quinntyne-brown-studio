using System.Text.Json;
using Qbs.Application.Ports;
using Qbs.Domain.Entities;
using Qbs.Domain.Exceptions;
using Qbs.Infrastructure.Serialization;

namespace Qbs.AcceptanceTests;

public sealed class MemoryStudioTransaction(Dictionary<(Type, Guid), string> data)
    : IStudioTransaction
{
    public Dictionary<(Type, Guid), string> Data { get; } = data;

    public Task<T?> Get<T>(Guid id)
        where T : Entity =>
        Task.FromResult(
            Data.TryGetValue((typeof(T), id), out var json)
                ? JsonSerializer.Deserialize<T>(json, StudioJson.Options)
                : null
        );

    public Task<T[]> List<T>()
        where T : Entity =>
        Task.FromResult(
            Data.Where(x => x.Key.Item1 == typeof(T))
                .Select(x => JsonSerializer.Deserialize<T>(x.Value, StudioJson.Options)!)
                .ToArray()
        );

    public async Task Save<T>(T value, long expectedVersion)
        where T : Entity
    {
        var old = await Get<T>(value.Id);
        if ((old?.Version ?? 0) != expectedVersion)
            throw new StudioException(409, "This record changed. Reload before saving.");
        value.Version = expectedVersion + 1;
        value.ExpectedVersion = 0;
        Data[(typeof(T), value.Id)] = JsonSerializer.Serialize(value, StudioJson.Options);
    }
}
