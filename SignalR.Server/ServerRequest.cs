using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Threading.Tasks;
using Gate;

namespace SignalR.Server
{
    public class ServerRequest : IRequest
    {
        private readonly Request _request;

        internal ServerRequest(Request request)
        {
            _request = request;
        }

        public Uri Url
        {
            get { throw new NotImplementedException(); }
        }

        public NameValueCollection QueryString
        {
            get { throw new NotImplementedException(); }
        }

        public NameValueCollection Headers
        {
            get { throw new NotImplementedException(); }
        }

        public NameValueCollection Form
        {
            get { throw new NotImplementedException(); }
        }

        public IRequestCookieCollection Cookies
        {
            get { throw new NotImplementedException(); }
        }

        public IPrincipal User
        {
            get { throw new NotImplementedException(); }
        }

        public void AcceptWebSocketRequest(Func<IWebSocket, Task> callback)
        {
            throw new NotImplementedException();
        }
    }
}