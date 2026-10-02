using DeskShare.Domain;
using DeskShare.Domain.Desks;
using Microsoft.EntityFrameworkCore;

namespace DeskShare.Infrastructure.Persistence;

internal static class SampleDataSeeder
{
    public static async Task SeedAsync(DeskShareDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Desks.AnyAsync(cancellationToken))
            return;

        dbContext.Desks.AddRange(
            Desk.Create("A-101", 1, DeskFeatures.Monitor | DeskFeatures.Window),
            Desk.Create("A-102", 1, DeskFeatures.Monitor),
            Desk.Create("A-103", 1, DeskFeatures.Standing),
            Desk.Create("B-201", 2, DeskFeatures.Monitor | DeskFeatures.Standing | DeskFeatures.Window),
            Desk.Create("B-202", 2, DeskFeatures.None),
            Desk.Create("B-203", 2, DeskFeatures.Window));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
