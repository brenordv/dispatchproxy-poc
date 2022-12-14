using ServiceMethodInterceptorPoC.API.Caching;
using ServiceMethodInterceptorPoC.API.Extensions;
using ServiceMethodInterceptorPoC.API.Interfaces;
using ServiceMethodInterceptorPoC.API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DemoMemoryCache>();


/*
 * Adding the services in their most simple form, just for reference.
 */

// "Pure" service.
builder.Services.AddSingleton<INumberServicePure, NumberServicePure>();

// Service with memento.
builder.Services.AddSingleton<INumberServiceMemento, NumberServiceMemento>();

// Service with memory cache in service.
builder.Services.AddSingleton<INumberServiceMemoryCache, NumberServiceMemoryCache>();

/*
 * Adding StopWatch Decorator/interceptor.
 */
// Mode 1: In separate steps
// Adding service
builder.Services.AddSingleton<INumberServiceMementoDecorated, NumberServiceMemento>();

// Decorating the service with the StopWatch.
builder.Services.DecorateWithStopWatch<INumberServiceMementoDecorated>();

// Mode 2: In a single step (or at least it looks like it).
builder.Services.AddSingletonWithStopWatch<INumberServicePureDecoratedStopWatch, NumberServicePure>();
builder.Services.AddSingletonWithStopWatch<INumberServiceMemoryCacheDecorated, NumberServiceMemoryCache>();
builder.Services.AddSingletonWithMemoryCache<INumberServicePureDecoratedMemoryCache, NumberServicePure>();

var app = builder.Build();

var countPure = 0;
var countMemento = 0;
var countMemory = 0;
var countPureSw = 0;
var countPureCache = 0;
var countMementoDecorated = 0;
var countMemoryDecorated = 0;


app.MapGet("/pure", (INumberServicePure numberService) =>
{
    //This route uses the "Pure" service. No decorators, no caching, no nothing...
    countPure++;
    var fibonacci = numberService.NFibonacci(countPure);
    var prime = numberService.NPrime(countPure);
    return $"Calculating {countPure}-nth: Fibonacci: {fibonacci} // Prime: {prime}";
});

app.MapGet("/memento", (INumberServiceMemento numberService) =>
{
    //This route uses the service that has a memento pattern implemented (in a very basic and naive way)...
    countMemento++;
    var fibonacci = numberService.NFibonacci(countMemento);
    var prime = numberService.NPrime(countMemento);
    return $"Calculating {countMemento}-nth: Fibonacci: {fibonacci} // Prime: {prime}";
});

app.MapGet("/memory", (INumberServiceMemoryCache numberService) =>
{
    //This route uses the service that has a very naive memory pattern implemented...
    countMemory++;
    var fibonacci = numberService.NFibonacci(countMemory);
    var prime = numberService.NPrime(countMemory);
    return $"Calculating {countMemory}-nth: Fibonacci: {fibonacci} // Prime: {prime}";
});

app.MapGet("/pure-sw-decorated", (INumberServicePureDecoratedStopWatch numberService) =>
{
    //This route uses the pure service implementation, but with the stopwatch decorator.
    countPureSw++;
    var fibonacci = numberService.NFibonacci(countPureSw);
    var prime = numberService.NPrime(countPureSw);
    return $"Calculating {countPureSw}-nth: Fibonacci: {fibonacci} // Prime: {prime}";
});

app.MapGet("/pure-cache-decorated", (INumberServicePureDecoratedMemoryCache numberService) =>
{
    //This route uses the pure service implementation, but with the stopwatch decorator.
    /*
     * This, however, will not work as intended. The reason is that the logic inside the method is
     * recursive and that type of logic will not "covered" by the dispatch logic.
     */
    countPureCache++;
    var fibonacci = numberService.NFibonacci(countPureCache);
    var prime = numberService.NPrime(countPureCache);
    return $"Calculating {countPureCache}-nth: Fibonacci: {fibonacci} // Prime: {prime}";
});

app.MapGet("/memento-decorated", (INumberServiceMementoDecorated numberService) =>
{
    //This route uses the service that has a memento pattern implemented + stopwatch...
    countMementoDecorated++;
    var fibonacci = numberService.NFibonacci(countMementoDecorated);
    var prime = numberService.NPrime(countMementoDecorated);
    return $"Calculating {countMementoDecorated}-nth: Fibonacci: {fibonacci} // Prime: {prime}";
});

app.MapGet("/memory-decorated", (INumberServiceMemoryCacheDecorated numberService) =>
{
    //This route uses the service that has a very naive memory pattern implemented + stopwatch decorator...
    countMemoryDecorated++;
    var fibonacci = numberService.NFibonacci(countMemoryDecorated);
    var prime = numberService.NPrime(countMemoryDecorated);
    return $"Calculating {countMemoryDecorated}-nth: Fibonacci: {fibonacci} // Prime: {prime}";
});

app.Run();