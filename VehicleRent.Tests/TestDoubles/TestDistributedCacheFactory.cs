using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace VehicleRent.Tests.TestDoubles;

internal static class TestDistributedCacheFactory
{
    public static IDistributedCache Create()
    {
        return new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
    }
}
