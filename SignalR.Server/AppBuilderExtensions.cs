using System;
using Owin;
using Gate;
using SignalR.Hubs;

namespace SignalR.Server
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseHubDispatcher(this IAppBuilder builder)
        {
            return Use(builder, GlobalHost.GetDependencyResolver, CreateHubDispatcher);
        }

        public static IAppBuilder UseHubDispatcher(this IAppBuilder builder, IDependencyResolver resolver)
        {
            return Use(builder, () => resolver, CreateHubDispatcher);
        }

        public static IAppBuilder UsePersistentConnection<T>(this IAppBuilder builder) where T : PersistentConnection
        {
            return Use(builder, GlobalHost.GetDependencyResolver, CreatePersistentConnection<T>);
        }

        public static IAppBuilder UsePersistentConnection<T>(this IAppBuilder builder, IDependencyResolver resolver)
        {
            return Use(builder, () => resolver, CreatePersistentConnection<T>);
        }


        static IAppBuilder Use(
            IAppBuilder builder,
            Func<IDependencyResolver> resolver,
            Func<IDependencyResolver, Request, PersistentConnection> createConnection)
        {
            return builder.Use(Call, resolver, createConnection);
        }

        static AppDelegate Call(
            AppDelegate app,
            Func<IDependencyResolver> resolver,
            Func<IDependencyResolver, Request, PersistentConnection> createPersistentConnection)
        {
            return
                (env, result, fault) =>
                {
                    var request = new Request(env);

                    var serverRequest = new ServerRequest(request);
                    var serverResponse = new ServerResponse(new Response(result), request.CallDisposed);
                    var hostContext = new HostContext(serverRequest, serverResponse);

                    var connection = createPersistentConnection(resolver.Invoke(), request);
                    connection.ProcessRequestAsync(hostContext);
                };
        }

        static PersistentConnection CreateHubDispatcher(IDependencyResolver resolver, Request request)
        {
            var hubDispatcher = new HubDispatcher(request.PathBase);
            hubDispatcher.Initialize(resolver);
            return hubDispatcher;
        }

        static PersistentConnection CreatePersistentConnection<T>(IDependencyResolver resolver, Request request)
        {
            var factory = new PersistentConnectionFactory(resolver);
            var persistentConnection = factory.CreateInstance(typeof(T));
            persistentConnection.Initialize(resolver);
            return persistentConnection;
        }
    }
}
