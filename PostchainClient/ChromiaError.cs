using System;

namespace Chromia
{
    /// <summary>
    /// Base application exception for all Chromia related exceptions.
    /// </summary>
    public class ChromiaException : ApplicationException
    {
        /// <inheritdoc/>
        public ChromiaException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception raised for errors in transport layer.
    /// </summary>
    public class TransportException : ChromiaException
    {
        /// <summary>
        /// The reason why the request failed.
        /// </summary>
        public enum ReasonCode
        {
            /// <summary>
            /// Any other reason why the request failed. Check <see cref="Exception.Message"/> for details.
            /// </summary>
            Unknown,

            /// <summary>
            /// The node returned and http error code. Check <see cref="StatusCode"/> for details.
            /// </summary>
            HttpError,

            /// <summary>
            /// The given node url was faulty.
            /// </summary>
            MalformedUri,

            /// <summary>
            /// The request timed out.
            /// </summary>
            Timeout
        }

        /// <summary>
        /// Reason why the exception was thrown.
        /// </summary>
        public ReasonCode Reason { get; private set; }

        /// <summary>
        /// Http status code from the Reason.HttpError, -1 otherwise.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <inheritdoc/>
        public TransportException(ReasonCode reason, string message, int statusCode = -1) : base(message)
        {
            Reason = reason;
            StatusCode = statusCode;
        }
    }
}
