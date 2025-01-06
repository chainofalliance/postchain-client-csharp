using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Chromia.Transport
{
    /// <summary>
    /// Default .NET implementation of the transport.
    /// </summary>
    public class DefaultTransport : ITransport
    {
        private readonly HttpClient _httpClient = new HttpClient();

        /// <inheritdoc/>
        public async Task<Buffer> Get(Uri uri, CancellationToken ct)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(uri, ct);
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }

            return await VerifyResponse(response);
        }

        /// <inheritdoc/>
        public async Task<Buffer> Post(Uri uri, Buffer content, CancellationToken ct)
        {
            var bytes = new ByteArrayContent(content.Bytes);
            return await Post(uri, bytes, ct);
        }

        /// <inheritdoc/>
        public async Task<Buffer> Post(Uri uri, string content, CancellationToken ct)
        {
            var str = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            return await Post(uri, str, ct);
        }

        /// <inheritdoc/>
        public Task Delay(int milliseconds, CancellationToken ct)
        {
            return Task.Delay(milliseconds, ct);
        }

        private async Task<Buffer> Post(Uri uri, HttpContent content, CancellationToken ct)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(uri, content, ct);
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }

            return await VerifyResponse(response);
        }

        private static async Task<Buffer> VerifyResponse(HttpResponseMessage response)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync();
            try
            {
                response.EnsureSuccessStatusCode();
                return Buffer.From(bytes);
            }
            catch (Exception e)
            {
                throw HandleException(e, (int)response.StatusCode, System.Text.Encoding.UTF8.GetString(bytes));
            }
        }

        private static TransportException HandleException(
            Exception e,
            int statusCode = -1,
            string content = ""
        )
        {
            if (e is HttpRequestException h)
                return new TransportException(TransportException.ReasonCode.HttpError, content, statusCode);
            else if (e is InvalidOperationException || e is UriFormatException)
                return new TransportException(TransportException.ReasonCode.MalformedUri, $"malformed request uri");
            else if (e is TaskCanceledException)
                return new TransportException(TransportException.ReasonCode.Timeout, $"request timed out");
            else
                return new TransportException(TransportException.ReasonCode.Unknown, $"{e.Message}\n{content}");
        }
    }
}
