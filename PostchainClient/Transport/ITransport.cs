using System;
using System.Threading.Tasks;

namespace Chromia.Transport
{
    /// <summary>
    /// Interface to define a transport layer that can be used by ChromiaClient.
    /// </summary>
    public interface ITransport
    {
        /// <summary>
        /// Sends a GET request to the uri.
        /// </summary>
        /// <param name="uri">The uri to send the request to.</param>
        /// <returns>The response content.</returns>
        /// <exception cref="TransportException"></exception>
        Task<Buffer> Get(Uri uri);

        /// <summary>
        /// Sends a POST request to the uri with Buffer content.
        /// </summary>
        /// <param name="uri">The uri to send the request to.</param>
        /// <param name="content">The content to post.</param>
        /// <returns>The response content.</returns>
        /// <exception cref="TransportException"></exception>
        Task<Buffer> Post(Uri uri, Buffer content);

        /// <summary>
        /// Sends a POST request to the uri with Buffer content.
        /// </summary>
        /// <param name="uri">The uri to send the request to.</param>
        /// <param name="content">The content to post.</param>
        /// <returns>The response content.</returns>
        /// <exception cref="TransportException"></exception>
        Task<Buffer> Post(Uri uri, string content);

        /// <summary>
        /// Delays the task for the given amount of milliseconds.
        /// </summary>
        /// <param name="milliseconds">The milliseconds to delay.</param>
        Task Delay(int milliseconds);
    }
}
