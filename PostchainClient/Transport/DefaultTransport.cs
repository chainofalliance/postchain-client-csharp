using System;
using System.Net.Http;
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
        public async Task<Buffer> Get(Uri uri)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(uri);
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }

            return await VerifyResponse(response);
        }

        /// <inheritdoc/>
        public async Task<Buffer> Post(Uri uri, Buffer content)
        {
            var bytes = new ByteArrayContent(content.Bytes);
            return await Post(uri, bytes);
        }

        /// <inheritdoc/>
        public async Task<Buffer> Post(Uri uri, string content)
        {
            var str = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            return await Post(uri, str);
        }

        /// <inheritdoc/>
        public async Task Delay(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        private async Task<Buffer> Post(Uri uri, HttpContent content)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync(uri, content);
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }

            return await VerifyResponse(response);
        }

        private static async Task<Buffer> VerifyResponse(HttpResponseMessage response)
        {
            try
            {
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();
                return Buffer.From(bytes);
            }
            catch (Exception e)
            {
                throw HandleException(e, (int)response.StatusCode);
            }
        }

        private static TransportException HandleException(Exception e, int statusCode = -1)
        {
            if (e is HttpRequestException h)
                return new TransportException(TransportException.ReasonCode.HttpError, h.Message, statusCode);
            else if (e is InvalidOperationException || e is UriFormatException)
                return new TransportException(TransportException.ReasonCode.MalformedUri, "malformed request uri");
            else if (e is TaskCanceledException)
                return new TransportException(TransportException.ReasonCode.Timeout, "request timed out");
            else
                return new TransportException(TransportException.ReasonCode.Unknown, e.Message);
        }
    }
}
