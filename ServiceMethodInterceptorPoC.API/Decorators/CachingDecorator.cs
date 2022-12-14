using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using ServiceMethodInterceptorPoC.API.Caching;

namespace ServiceMethodInterceptorPoC.API.Decorators;

public class CachingDecorator<TDecorated> : DispatchProxy
{
    private TDecorated _decorated;
    private DemoMemoryCache _memoryCache;
    
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        var key = $"{targetMethod.Name}--{args[0]}";
        try
        {
            if (_memoryCache.Cache.TryGetValue(key, out var cachedResult))
                return cachedResult;
            
            var result = targetMethod.Invoke(_decorated, args);
            
            
            _memoryCache.Cache.Set(key, result);
            
            return result;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
    }

    /// <summary>
    /// Used dynamically during runtime.
    /// </summary>
    /// <remarks>Seems like dead code, but it's not.</remarks>
    /// <param name="decorated"></param>
    /// <returns></returns>
    public static TDecorated Create(TDecorated decorated, DemoMemoryCache memoryCache)
    {
        object proxy = Create<TDecorated, CachingDecorator<TDecorated>>();
        ((CachingDecorator<TDecorated>)proxy).SetParameters(decorated, memoryCache);

        return (TDecorated)proxy;
    }

    private void SetParameters(TDecorated decorated, DemoMemoryCache memoryCache)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _memoryCache = memoryCache;
    }
}