using QuinntyneBrownStudio.Application.Ports;
using QuinntyneBrownStudio.Domain.Entities;
using QuinntyneBrownStudio.Domain.Enums;
using QuinntyneBrownStudio.Domain.Exceptions;

namespace QuinntyneBrownStudio.Application.Clients;

public static class PhotoAccess
{
    public static bool SessionAllowed(Session s, Guid client, DateTimeOffset now) =>
        s.ClientIds.Contains(client)
        && s.ExpiresAt > now
        && s.RetentionState is not "Deleted" and not "DeletionPending";

    public static async Task<SessionPhoto> Require(
        IStudioTransaction tx,
        Guid id,
        Guid client,
        DateTimeOffset now
    )
    {
        var photo = await tx.Get<SessionPhoto>(id);
        var session = photo == null ? null : await tx.Get<Session>(photo.SessionId);
        if (
            photo?.State != PhotoState.Ready
            || session == null
            || !SessionAllowed(session, client, now)
        )
            throw new StudioException(404, "Photo not found.");
        return photo;
    }
}
