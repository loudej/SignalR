using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Gate;

namespace SignalR.Server
{
    public class ServerRequest : IRequest
    {
        private readonly Request _request;
        private NameValueCollection _query;
        private NameValueCollection _headers;
        private NameValueCollection _form;

        internal ServerRequest(Request request)
        {
            _request = request;
        }

        public Uri Url
        {
            get
            {
                object portString;
                int port;
                if (_request.TryGetValue("server.SERVER_PORT", out portString))
                {
                    port = int.Parse(portString.ToString());
                }
                else
                {
                    port = _request.Scheme == "https" ? 433 : 80;
                }

                var uriBuilder = new UriBuilder(_request.Scheme, _request.Host, port, _request.PathBase + _request.Path);
                if (!string.IsNullOrEmpty(_request.QueryString))
                    uriBuilder.Query = _request.QueryString;
                return uriBuilder.Uri;
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                if (_query == null)
                {
                    var query = new NameValueCollection();
                    foreach (var kv in _request.Query)
                    {
                        query.Add(kv.Key, kv.Value);
                    }
                    Interlocked.CompareExchange(ref _query, query, null);
                }
                return _query;
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                if (_headers == null)
                {
                    var headers = new NameValueCollection();
                    foreach (var kv in _request.Headers)
                    {
                        foreach (var value in kv.Value)
                        {
                            headers.Add(kv.Key, value);
                        }
                    }
                    Interlocked.CompareExchange(ref _headers, headers, null);
                }
                return _headers;
            }
        }

        public NameValueCollection Form
        {
            get
            {
                if (_form == null)
                {
                    var form = new NameValueCollection();
                    foreach (var kv in _request.Post)
                    {
                        form.Add(kv.Key, kv.Value);
                    }
                    Interlocked.CompareExchange(ref _form, form, null);
                }
                return _form;
            }
        }

        public IRequestCookieCollection Cookies
        {
            get { return new ServerRequestCookies(_request.Cookies); }
        }

        public IPrincipal User
        {
            get
            {
                IPrincipal user = null;

                object value;
                if (_request.TryGetValue("host.User", out value))
                    user = value as IPrincipal;

                return user;
            }
        }

        public void AcceptWebSocketRequest(Func<IWebSocket, Task> callback)
        {
            throw new NotImplementedException();
        }
    }
}