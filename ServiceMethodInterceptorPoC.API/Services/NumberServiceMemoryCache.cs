using Microsoft.Extensions.Caching.Memory;
using ServiceMethodInterceptorPoC.API.Caching;
using ServiceMethodInterceptorPoC.API.Interfaces;

namespace ServiceMethodInterceptorPoC.API.Services;

public class NumberServiceMemoryCache : BaseService, INumberServiceMemoryCache, INumberServiceMemoryCacheDecorated
{
    private readonly DemoMemoryCache _memoryCache;

    public NumberServiceMemoryCache(DemoMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public ulong NFibonacci(int n)
    {
        var key = $"Fibonacci-{n}";
        if (_memoryCache.Cache.TryGetValue(key, out var cachedResult))
            return (ulong)cachedResult;

        ulong result = n switch
        {
            0 => 0,
            1 => 1,
            _ => NFibonacci(n - 1) + NFibonacci(n - 2)
        };
        
        _memoryCache.Cache.Set(key, result);

        return result;
    }

    public ulong NPrime(int n)
    {
        var key = $"Prime-{n}";
        if (_memoryCache.Cache.TryGetValue(key, out var cachedResult))
            return (ulong)cachedResult;

        ulong result = 2;

        while (n > 0)
        {
            if (IsPrime(result))
                n--;

            result++;
        }

        result -= 1;
        
        _memoryCache.Cache.Set(key, result);
        
        return result;
    }
}