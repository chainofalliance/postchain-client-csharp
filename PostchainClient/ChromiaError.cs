using System;

namespace Chromia
{
    /// <summary>
    /// Base application exception for all Chromia related exceptions.
    /// </summary>
    class ChromiaException : ApplicationException
    {
        public ChromiaException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception raised for errors in transport layer.
    /// </summary>
    class TransportException : ChromiaException
    {
        public enum ReasonCode
        {
            Unknown,
            HttpError,
            MalformedUri,
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

        public TransportException(ReasonCode reason, string message, int statusCode = -1) : base(message)
        {
            Reason = reason;
            StatusCode = statusCode;
        }
    }
}
