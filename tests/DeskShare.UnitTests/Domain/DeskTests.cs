using DeskShare.Domain;
using DeskShare.Domain.Desks;

namespace DeskShare.UnitTests.Domain;

public sealed class DeskTests
{
    [Fact]
    public void Create_NormalizesCode()
    {
        var desk = Desk.Create("  a-101 ", 1, DeskFeatures.Monitor);

        Assert.Equal("A-101", desk.Code);
    }
}
