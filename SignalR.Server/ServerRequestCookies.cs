using System.Collections.Generic;

namespace SignalR.Server
{
    public class ServerRequestCookies : IRequestCookieCollection
    {
        private readonly IDictionary<string, string> _cookies;

        public ServerRequestCookies(IDictionary<string, string> cookies)
        {
            _cookies = cookies;
        }

        public Cookie this[string name]
        {
            get
            {
                string value;
                return _cookies.TryGetValue(name, out value) ? new Cookie(name, value) : null;
            }
        }

        public int Count
        {
            get { return _cookies.Count; }
        }
    }
}