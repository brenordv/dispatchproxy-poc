using Microsoft.Extensions.Caching.Memory;

namespace ServiceMethodInterceptorPoC.API.Caching;

public class DemoMemoryCache
{
    public MemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());
}