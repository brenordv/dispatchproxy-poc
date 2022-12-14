
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;

Console.WriteLine("Method interception PoC - Tester");

const int quantityOfRequests = 50;
const string baseUrl = "https://localhost:7126";
var endpoints = new []
{
    $"{baseUrl}/pure",
    $"{baseUrl}/memento",
    $"{baseUrl}/memory",
    $"{baseUrl}/pure-sw-decorated",
    $"{baseUrl}/pure-cache-decorated",
    $"{baseUrl}/memento-decorated",
    $"{baseUrl}/memory-decorated"
};
var results = new Result[quantityOfRequests];

var client = new HttpClient
{
    Timeout = Timeout.InfiniteTimeSpan
};

foreach (var endpoint in endpoints)
{
    for (var requestIndex = 0; requestIndex < quantityOfRequests; requestIndex++)
    {
        var sw = new Stopwatch();
    
        sw.Start();

        Console.Write($"{requestIndex:0000}...\r");
        
        //I know, but this makes the Parallel.For easier.
        var response = client.GetAsync(endpoint).Result;
        
        sw.Stop();
    
        results[requestIndex] = new Result(requestIndex, sw.Elapsed, response.StatusCode);
    }

    results.Print(endpoint);

    var serializedResults = JsonConvert.SerializeObject(results, Formatting.Indented);
    
    File.WriteAllText($"{endpoint.Replace($"{baseUrl}/", "")}--{DateTime.Now.Ticks}.json", serializedResults);
}

public record Result(int RequestIndex, TimeSpan ElapsedTime, HttpStatusCode ResponseStatusCode);

public static class Extensions
{
    public static void Print(this Result[] requestResults, string suffix)
    {
        var groupedByStatusCode = requestResults.GroupBy(result => result.ResponseStatusCode);
        foreach (var grouping in groupedByStatusCode)
        {
            var statusCode = (int)grouping.Key;
            var quantityOfRequests = grouping.Count();
            var avgRequest = grouping.Average(result => result.ElapsedTime.TotalMilliseconds);
            var min = grouping.Min(result => result.ElapsedTime.TotalMilliseconds);
            var max = grouping.Max(result => result.ElapsedTime.TotalMilliseconds);
            Console.WriteLine($"[{statusCode}] {quantityOfRequests}/{requestResults.Length} | Min: {TimeSpan.FromMilliseconds(min)} | Max: {TimeSpan.FromMilliseconds(max)} | Avg Time: {TimeSpan.FromMilliseconds(avgRequest)} | {suffix}");
        }
    }
}