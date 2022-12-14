# Method interception PoC
For this PoC, I used the `DispatchProxy` from C#.

## Why use DispatchProxy?
There are several use cases where it can be useful to make use of the DispatchProxy class in C#. Some of the best use cases for DispatchProxy include:

1. Adding logging or debugging functionality to an existing object without modifying its code.
2. Creating a performance monitoring proxy that tracks the time taken for each method call on the target object.
3. Implementing cross-cutting concerns such as caching, security, or transaction management without modifying the code of the target object.
4. Creating a mocking proxy for use in unit tests, allowing you to easily mock the behavior of the target object without modifying its code.
5. Wrapping an existing object with a proxy that can automatically retry failed method calls or provide automatic fallback behavior in case of failure.

These are just a few examples of the many ways that DispatchProxy can be used to add custom behavior to an existing object in C#.

## What is this DispatchProxy?
The DispatchProxy class is a special class in the .NET framework that allows developers to create a dynamic proxy for a target object. This means that it can intercept calls to the target object and handle them in a custom way. This is often used in scenarios where it is necessary to add additional behavior to an existing object without modifying its code.

To use the DispatchProxy class, you first create a new class that derives from DispatchProxy and overrides the Invoke method. This method allows you to define custom behavior for how the proxy should handle calls to the target object. Then, you can create an instance of your proxy class and use it to wrap the target object. From then on, any calls to the target object will be intercepted by the proxy and handled according to the logic you defined in the Invoke method.

For example, you could use a DispatchProxy to create a logging proxy that logs every method call made to the target object, or to create a performance monitoring proxy that tracks the time taken for each method call. These are just a few of the many ways that DispatchProxy can be used to add custom behavior to an existing object.

### DispatchProxy method: Create
The Create method is a static factory method that is used to create a new instance of a DispatchProxy-derived class. It takes as arguments the type of the target object that the proxy will be used with, and an instance of the target object. It then creates a new instance of the DispatchProxy-derived class and sets the target object as the object that the proxy will intercept method calls for. The Create method returns the newly created proxy object, which can then be used to invoke methods on the target object.

Here is an example of how the Create method might be used (without dependency injection):

```csharp
class LoggingProxy : DispatchProxy
{
    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        Console.WriteLine($"Invoking method {targetMethod.Name} on target object");
        var result = targetMethod.Invoke(targetObject, args);
        Console.WriteLine($"Method {targetMethod.Name} returned with value {result}");
        return result;
    }
}

// Create an instance of the target object
var target = new SomeClass();

// Create a proxy for the target object
var proxy = DispatchProxy.Create<ISomeInterface, LoggingProxy>();

// Invoke a method on the target object through the proxy
proxy.SomeMethod("hello");
```

In this example, the Create method is used to create a new instance of the LoggingProxy class and set the target object as the object that the proxy will intercept method calls for. Then, the SomeMethod method is invoked on the proxy, which causes the Invoke method on the LoggingProxy class to be called, allowing the proxy to log the method call and its result.

### DispatchProxy method: SetParameters
The SetParameters method is a method on the DispatchProxy class that allows you to set the parameters that will be passed to the Invoke method when a method on the target object is called. This is useful in scenarios where the proxy needs to have access to certain information or objects in order to handle the method call correctly.

Here is an example of how the SetParameters method might be used:
```csharp
class LoggingProxy : DispatchProxy
{
    private ILogger logger;

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        logger.Log($"Invoking method {targetMethod.Name} on target object");
        var result = targetMethod.Invoke(targetObject, args);
        logger.Log($"Method {targetMethod.Name} returned with value {result}");
        return result;
    }
}

// Create an instance of the target object
var target = new SomeClass();

// Create a proxy for the target object
var proxy = DispatchProxy.Create<ISomeInterface, LoggingProxy>();

// Set the logger to be used by the proxy
proxy.SetParameters(new ConsoleLogger());

// Invoke a method on the target object through the proxy
proxy.SomeMethod("hello");

```

In this example, the SetParameters method is used to set the logger field on the LoggingProxy class to an instance of the ConsoleLogger class. Then, when a method on the target object is invoked through the proxy, the Invoke method on the LoggingProxy class can use the logger object to log the method call and its result.


## Proof of Concept
Well, some context is required for this PoC. I wanted to see if performance would be heavily affected if I added a
`DispatchProxy` to a service. Also I wanted to check, for this specific use case, if a memento or a memory cache 
would make a difference. 
My goal was not to create a pier reviewed study about `DispatchProxy` and it's affect in performance. I wanted to test 
a basic concept to know if I the whole idea is feasible.

So, the first thing I did was create a service that does the following thing:
1. Calculates the Nth position in the Fibonacci sequence;
2. Calculates the Nth prime number;

Then I created an API with a bunch of endpoints: one for each theory I wanted  to test.

1. `/pure`: The service in it's pure form. No logs, no decorators, just the operation;
2. `/memento`: Same basic functionality as the pure + a memento pattern applied;
3. `/memory`: Same basic functionality as the pure + a memory cache applied;
4. `/pure-sw-decorated`: Same as the `/pure` route, but with a `DispatchProxy` that logs the elapsed time of each method.
5. `/pure-cache-decorated`: Same as the `/pure` route, but with a `DispatchProxy` that uses memory cache before even calling the method. 
This type of decoration **does not work as intended** because the methods were built to do the logic recursively and that breaks the DispatchProxy. If no recursion was used, this would work. I left it just because I might try to do somemore testing later.
6. `/memento-decorated`: Same as the `/memento` route, but with a `DispatchProxy` that logs the elapsed time of each method.
7. `/memory-decorated`: Same as the `/memory` route, but with a `DispatchProxy` that logs the elapsed time of each method.

In general, all endpoints will return the Nth Fibonacci and Prime numbers and with every request, it will get the next one. 
For instance: The 1st request will get the 1st Fibonacci and Prime numbers. The 2nd will get the 2nd Fibonacci and Prime and so on and so forth...

To put all that to the test, I created the `ServiceMethodInterceptorPoC.Cli.API.Tester` to make consecutive requests to each endpoint of the API and measure
the elapsed time. I made 50 requests to each of the endpoints.

> Note that in the endpoint `/pure-cache-decorated`, the decorator don't work as intended (reasons explained above) 

| Min                | Max              | Avg              | Url                                         |
|--------------------|------------------|------------------|---------------------------------------------|
| 00:00:00.0002486   | 00:01:26.1175579 | 00:00:04.4979094 | https://localhost:7126/pure                 |
| 00:00:00.0002220   | 00:01:26.6493252 | 00:00:04.5327115 | https://localhost:7126/pure-sw-decorated    |
| 00:00:00.0002138   | 00:01:23.8874709 | 00:00:04.4329748 | https://localhost:7126/pure-cache-decorated |
| 00:00:00.0002184   | 00:00:00.0046571 | 00:00:00.0004284 | https://localhost:7126/memento              |
| 00:00:00.0002026   | 00:00:00.0020083 | 00:00:00.0003799 | https://localhost:7126/memento-decorated    |
| 00:00:00.0002077   | 00:00:00.0043240 | 00:00:00.0003713 | https://localhost:7126/memory               |
| 00:00:00.0001922   | 00:00:00.0021753 | 00:00:00.0003183 | https://localhost:7126/memory-decorated     |


## Conclusion
From the results above, I'm concluding that the `DispatchProxy` won't harm performance - as long as you don't do anything complex in it.
Now I'm a bit more at ease to use this without worrying that this will impact performance in any significant way.
(Hmm... I wonder how many `DispatchProxy` classes i need to nest until it starts to weigh down the system...)