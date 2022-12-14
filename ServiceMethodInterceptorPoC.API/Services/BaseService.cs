using ServiceMethodInterceptorPoC.API.Interfaces;

namespace ServiceMethodInterceptorPoC.API.Services;

public abstract class BaseService
{

    
    protected static bool IsPrime(ulong k)
    {
        switch (k)
        {
            case <= 1:
                return false;
            case 2:
            case 3:
                return true;
        }

        if (k % 2 == 0 || k % 3 == 0)
            return false;

        for (ulong i = 5; i * i <= k; i = i + 6)
            if (k % i == 0 || k % (i + 2) == 0)
                return false;

        return true;
    }
}