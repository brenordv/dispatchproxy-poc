using ServiceMethodInterceptorPoC.API.Interfaces;

namespace ServiceMethodInterceptorPoC.API.Services;

public class NumberServicePure : BaseService, INumberServicePure, INumberServicePureDecoratedStopWatch,
    INumberServicePureDecoratedMemoryCache
{
    public ulong NFibonacci(int n)
    {
        ulong result = n switch
        {
            0 => 0,
            1 => 1,
            _ => NFibonacci(n - 1) + NFibonacci(n - 2)
        };

        return result;
    }

    public ulong NPrime(int n)
    {
        ulong result = 2;

        while (n > 0)
        {
            if (IsPrime(result))
                n--;

            result++;
        }

        result -= 1;

        return result;
    }
}