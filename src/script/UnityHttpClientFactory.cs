#nullable enable
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Gilmartin.Unity.HttpClientFactory
{
    /// <summary>
    /// Simple replacement for HttpClientFactory which is not available in Unity, and which doesn't
    /// rely on IServiceCollection. Achieves nearly the same effect by sharing a single HttpClientHandler 
    /// between multiple HttpClients. Allowing HttpClient to be short-lived.
    /// 
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#httpclient-lifetime-management"/>
    /// </summary>
    public class UnityHttpClientFactory
    {
        private const int handlerExpirySeconds = 120;

        private static readonly ConcurrentDictionary<string, UnityHttpClientFactory> factories =
                new ConcurrentDictionary<string, UnityHttpClientFactory>();

        public static event Action<string, HttpClientHandler>? OnHandlerCreated;

        /// <summary>
        /// Create a new HttpClient from a named UnityHttpClientFactory, sharing a handler with other clients of the same factory.
        /// The HttpClient created from this factory should be disposed as soon as it is no longer needed.
        /// 
        /// Each factory has its own HttpClientHandler, so each factory has its own set of connections.
        /// </summary>
        /// <param name="name">Descriptive name of the factory.</param>
        /// <returns>HttpClient which should be disposed when finished with it.</returns>
        public static HttpClient CreateClient(string name) {
            var factory = factories.GetOrAdd(name, name => new UnityHttpClientFactory(name));
            return factory.CreateNewHttpClient();
        }

        /// <summary>
        /// Create a new HttpClient with the default factory, sharing a handler with other clients of the default factory.
        /// The HttpClient created from this factory should be disposed as soon as it is no longer needed.
        /// </summary>
        /// <returns>HttpClient which should be disposed when finished with it</returns>
        public static HttpClient CreateClient() => CreateClient(string.Empty);


        private HttpClientHandler? _currentHandler = null;
        private readonly string _name;
        private readonly Stopwatch _handlerTimer = new Stopwatch();
        private readonly object _handlerLock = new object();

        private UnityHttpClientFactory(string name) 
        {
            _name = name;
        }

        private HttpClient CreateNewHttpClient() => new HttpClient(GetHandler(), disposeHandler: false);

        private HttpClientHandler GetHandler() {
            lock (_handlerLock) {
                if (_currentHandler == null || 
                    _handlerTimer.Elapsed.TotalSeconds > handlerExpirySeconds) {
                    // The handler has expired (or one wasn't yet created), so create a new one, but DO NOT Dispose() the old one.
                    // The GC will do that when all outstanding requests have completed and no HttpClients still reference it.
                    _currentHandler = new HttpClientHandler();

                    // Allows the handler to be modified before it is used.
                    OnHandlerCreated?.Invoke(_name, _currentHandler);

                    _handlerTimer.Restart();
                }
                return _currentHandler;
            }
        }
    }
}