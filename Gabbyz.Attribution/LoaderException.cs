using System;
using System.Collections.Generic;
using System.Text;

namespace Gabbyz.Attribution
{
    /// <summary>
    /// A loader exception.
    /// </summary>
    public class LoaderException : Exception
    {
        /// <summary>
        /// Creates a new loader exception.
        /// </summary>
        public LoaderException()
        {
        }

        /// <summary>
        /// Creates a new loader exception with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public LoaderException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new loader exception with a message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public LoaderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
