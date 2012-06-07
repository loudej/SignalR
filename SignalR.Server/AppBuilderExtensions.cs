using System;
using System.Diagnostics;
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

        public static IAppBuilder UsePersistentConnection<T>(this IAppBuilder builder, IDependencyResolver resolver) where T : PersistentConnection
        {
            return Use(builder, () => resolver, CreatePersistentConnection<T>);
        }

        public static IAppBuilder UsePersistentConnection(this IAppBuilder builder, Type type)
        {
            return Use(builder, GlobalHost.GetDependencyResolver, CreatePersistentConnection(type));
        }

        public static IAppBuilder UsePersistentConnection(this IAppBuilder builder, Type type, IDependencyResolver resolver)
        {
            return Use(builder, () => resolver, CreatePersistentConnection(type));
        }


        static IAppBuilder Use(
            IAppBuilder builder,
            Func<IDependencyResolver> resolver,
            Func<IDependencyResolver, Request, PersistentConnection> createConnection)
        {
            return builder.Use(TraceCalls).Use(Call, resolver, createConnection);
        }

        private static AppDelegate TraceCalls(AppDelegate app)
        {
            return
                (env, result, fault) =>
                {
                    Trace.WriteLine(string.Format(
                        "{0} {1}{2} {3}",
                        env["owin.RequestMethod"],
                        env["owin.RequestPathBase"],
                        env["owin.RequestPath"],
                        env["owin.RequestQueryString"]));

                    app(
                        env,
                        (status, headers, body) =>
                        {
                            Trace.WriteLine(string.Format("{0} {1}{2}", status, env["owin.RequestPathBase"],
                                                          env["owin.RequestPath"]));
                            result(status, headers, body);
                        },
                        ex =>
                        {
                            Trace.WriteLine(string.Format("{0} {1}{2}", ex.Message, env["owin.RequestPathBase"],
                                                          env["owin.RequestPath"]));
                            fault(ex);
                        });
                };
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
                    var response = new Response(result);

                    var serverRequest = new ServerRequest(request);
                    var serverResponse = new ServerResponse(request, response);
                    var hostContext = new HostContext(serverRequest, serverResponse);

                    var connection = createPersistentConnection(resolver.Invoke(), request);
                    connection
                        .ProcessRequestAsync(hostContext)
                        .Then(() => response.End());
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

        static Func<IDependencyResolver, Request, PersistentConnection> CreatePersistentConnection(Type type)
        {
            return
                (resolver, request) =>
                {
                    var factory = new PersistentConnectionFactory(resolver);
                    var persistentConnection = factory.CreateInstance(type);
                    persistentConnection.Initialize(resolver);
                    return persistentConnection;
                };
        }
    }
}
