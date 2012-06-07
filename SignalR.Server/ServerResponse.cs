using System;
using System.Threading;
using System.Threading.Tasks;
using Gate;

namespace SignalR.Server
{
    public class ServerResponse : IResponse
    {
        private readonly Request _request;
        private readonly Response _response;
        private readonly CancellationToken _cancellationToken;

        internal ServerResponse(Request request, Response response)
        {
            _request = request;
            _response = response;
            _cancellationToken = request.CallDisposed;
        }

        public bool IsClientConnected
        {
            get { return !_cancellationToken.IsCancellationRequested; }
        }

        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        public Task WriteAsync(ArraySegment<byte> data)
        {
            return WriteAsync(data, disableBuffering: true);
        }

        public Task EndAsync(ArraySegment<byte> data)
        {
            return WriteAsync(data, disableBuffering: false);
        }

        private Task WriteAsync(ArraySegment<byte> data, bool disableBuffering)
        {
            if (disableBuffering && _response.Buffer)
            {
                _response.Buffer = false;

                object value;
                if (_request.TryGetValue("host.DisableResponseBuffering", out value) && value is Action)
                {
                    ((Action)value).Invoke();
                }
            }

            if (!IsClientConnected)
            {
                return TaskAsyncHelper.Empty;
            }

            try
            {
                return _response.WriteAsync(data)
                                .Then(response => response.FlushAsync(), _response);
            }
            catch (Exception ex)
            {
                return TaskAsyncHelper.FromError(ex);
            }
        }
    }
}