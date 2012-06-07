using System;

namespace SignalR.Server
{
    public class GlobalHost
    {
        private static Func<IDependencyResolver> _dependencyResolverAccessor;

        static GlobalHost()
        {
            SetDependencyResolverFactory(DefaultDependencyResolverFactory);
        }

        public static IDependencyResolver DependencyResolver
        {
            get { return _dependencyResolverAccessor.Invoke(); }
            set { _dependencyResolverAccessor = () => value; }
        }


        /// <summary>
        /// Gets the default <see cref="IConfigurationManager"/>
        /// </summary>
        public static IConfigurationManager Configuration
        {
            get
            {
                return DependencyResolver.Resolve<IConfigurationManager>();
            }
        }

        /// <summary>
        /// Gets the default <see cref="IConnectionManager"/>
        /// </summary>
        public static IConnectionManager ConnectionManager
        {
            get
            {
                return DependencyResolver.Resolve<IConnectionManager>();
            }
        }

        public static IDependencyResolver GetDependencyResolver()
        {
            return DependencyResolver;
        }

        public static void SetDependencyResolverFactory(Func<IDependencyResolver> resolverFactory)
        {
            _dependencyResolverAccessor =
                () =>
                {
                    var resolver = resolverFactory.Invoke();
                    _dependencyResolverAccessor = () => resolver;
                    return resolver;
                };
        }

        private static IDependencyResolver DefaultDependencyResolverFactory()
        {
            return new DefaultDependencyResolver();
        }
    }
}
