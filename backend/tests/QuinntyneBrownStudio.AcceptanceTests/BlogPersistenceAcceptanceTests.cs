using Microsoft.EntityFrameworkCore;
using QuinntyneBrownStudio.Domain.Entities.Blog;

namespace QuinntyneBrownStudio.AcceptanceTests;

// AC-L2-070-05: run the additive migration against LocalDB and reopen the store.
public sealed class BlogPersistenceAcceptanceTests
{
    [LocalDbFact]
    public async Task Migrated_articles_survive_reopening_and_stale_writers_cannot_overwrite_them()
    {
        await using var database = new LocalDbTestDatabase();
        await database.Migrate();
        Guid id;
        await using (var first = database.Open())
        {
            var article = new Article
            {
                ArticleId = Guid.NewGuid(), Title = "Persisted portrait", Slug = "persisted-portrait",
                Abstract = "Stored in studio SQL", Body = "Portrait", BodyHtml = "<p>Portrait</p>",
                Published = true, DatePublished = DateTime.UtcNow
            };
            first.Articles.Add(article);
            await first.SaveChangesAsync();
            id = article.ArticleId;
        }
        await using var second = database.Open();
        await using var stale = database.Open();
        var reopened = await second.Articles.SingleAsync(a => a.ArticleId == id);
        var oldCopy = await stale.Articles.SingleAsync(a => a.ArticleId == id);
        Assert.True(reopened.Published);
        Assert.Equal("Persisted portrait", reopened.Title);
        reopened.Title = "Current edit";
        await second.SaveChangesAsync();
        oldCopy.Title = "Stale edit";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => stale.SaveChangesAsync());
        await using var final = database.Open();
        Assert.Equal("Current edit", (await final.Articles.SingleAsync(a => a.ArticleId == id)).Title);
    }
}
