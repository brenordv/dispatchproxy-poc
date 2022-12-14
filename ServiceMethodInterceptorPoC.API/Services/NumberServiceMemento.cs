using ServiceMethodInterceptorPoC.API.Interfaces;

namespace ServiceMethodInterceptorPoC.API.Services;

public class NumberServiceMemento: BaseService, INumberServiceMemento, INumberServiceMementoDecorated
{
    //Memento pattern - https://refactoring.guru/design-patterns/memento
    private readonly IDictionary<int, ulong> _historyFibonacci;
    private readonly IDictionary<int, ulong> _historyPrime;

    public NumberServiceMemento()
    {
        _historyFibonacci = new Dictionary<int, ulong>();
        _historyPrime = new Dictionary<int, ulong>();
    }
    
    public ulong NFibonacci(int n)
    {
        if (_historyFibonacci.ContainsKey(n))
            return _historyFibonacci[n];

        ulong result = n switch
        {
            0 => 0,
            1 => 1,
            _ => NFibonacci(n - 1) + NFibonacci(n - 2)
        };

        _historyFibonacci.Add(n, result);
        
        return result;
    }

    public ulong NPrime(int n)
    {
        var key = n;
        if (_historyPrime.ContainsKey(key))
            return _historyPrime[key];
        
        ulong result = 2;

        while (n > 0)
        {
            if (IsPrime(result))
                n--;

            result++;
        }

        result -= 1;
        
        _historyPrime.Add(key, result);
        
        return result;
    }
}