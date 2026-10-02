using AutoMapper;
using DeskShare.Application;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeskShare.UnitTests.Fakes;

internal static class TestMapper
{
    public static readonly MapperConfiguration Configuration =
        new(config => config.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);

    public static readonly IMapper Instance = Configuration.CreateMapper();
}
