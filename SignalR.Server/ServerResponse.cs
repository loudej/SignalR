using System;
using System.Threading;
using System.Threading.Tasks;
using Gate;

namespace SignalR.Server
{
    public class ServerResponse : IResponse
    {
        private readonly Response _response;
        private readonly CancellationToken _cancellationToken;

        internal ServerResponse(Response response, CancellationToken cancellationToken)
        {
            _response = response;
            _cancellationToken = cancellationToken;
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

        public Task EndAsync(ArraySegment<byte> data)
        {
            if (!IsClientConnected)
            {
                return TaskAsyncHelper.Empty;
            }

            try
            {
                return _response.WriteAsync(data)
                                .Then(response => response.End(), _response);
            }
            catch (Exception ex)
            {
                return TaskAsyncHelper.FromError(ex);
            }
        }
    }
}