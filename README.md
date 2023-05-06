# UnityHttpClientFactory
Script file to properly use HttpClient in Unity applications


## Background
This script was heavily influenced by this blog post: https://ticehurst.com/2021/08/27/unity-httpclient.html by Bill Ticehurst.

I've made significant changes and additions. I also verified that it works similarly to how the HttpClientFactory works, 
which is holding onto an HttpClientHandler, and recycling it every so often. Currently this code recycles it every 2 minutes,
but it is up to the user of this script to determine how often the Handler should be recycled.

If DNS changes are not expected for the endpoints, a longer recycle time could be used.

This site section of the Microsoft documentation is applicable: 
- https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#httpclient-lifetimes
- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-7.0

However, everything related to typed clients and `IServiceCollection` are not applicable to this script, as it is just the bear minimum to properly use HttpClient without all of the
drawbacks listed in Bill's post.