// See https://aka.ms/new-console-template for more information
using Gilmartin.Unity.HttpClientFactory;
using System.Diagnostics;
using System.Net.Http.Json;

Console.WriteLine("Hello, World!");


UnityHttpClientFactory.OnHandlerCreated += (factoryName, handler) => {
    Console.WriteLine($"Handler created for factory {factoryName}");
    //handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
};



var client = UnityHttpClientFactory.CreateClient("test");

var timer = Stopwatch.StartNew();

var slow1 = CreateTask(client, "Slow 1", "10");
var slow2 = CreateTask(client, "Slow 2", "10");
var slow3 = CreateTask(client, "Slow 3", "10");

bool useDifferentFactoryForFastClients = false;

HttpClient fastClient;
if (useDifferentFactoryForFastClients) {
    fastClient = UnityHttpClientFactory.CreateClient("fast");
}
else {
    fastClient = client;
}

var fast1 = CreateTask(client, "Fast 1", "0.1");
var fast2 = CreateTask(client, "Fast 2", "0.1");
var fast3 = CreateTask(client, "Fast 3", "0.1");



var allSlowTask = Task.WhenAll(slow1, slow2, slow3).ContinueWith(task => {
    Console.WriteLine($"All Slow requests finished: Total time {timer.ElapsedMilliseconds}");
});

var allFastTask = Task.WhenAll(fast1, fast2, fast3).ContinueWith(task => {
    Console.WriteLine($"All Fast requests finished: Total time {timer.ElapsedMilliseconds}");
});

Console.WriteLine("Waiting for Client tasks to finish");

await Task.WhenAll(allSlowTask, allFastTask);

Console.WriteLine($"** Total time at finish: Total time {timer.ElapsedMilliseconds}");

Console.WriteLine("Finished");
Console.ReadKey();


Task CreateTask(HttpClient client, string name, string delaySecs) {
    return Task.Run(async () => {
        var timer = Stopwatch.StartNew();
        Console.WriteLine($"{name} Started");
        await client.GetAsync($"https://httpbin.org/delay/{delaySecs}");
        Console.WriteLine($"{name} Finished: {timer.ElapsedMilliseconds}");
    });
}
