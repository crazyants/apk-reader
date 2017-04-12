using System;

namespace ApkReader
{
#if !NETSTANDARD1_3

    [Serializable]
#endif
    public class ApkReaderException : Exception
    {
        /// <devdoc>
        ///     <para>
        ///         Initializes a new instance of the <see cref='ApkReaderException' /> class
        ///         without a message.
        ///     </para>
        /// </devdoc>
        public ApkReaderException() : this(null)
        {
        }

        /// <devdoc>
        ///     <para>
        ///         Initializes a new instance of the <see cref='ApkReaderException' /> class with
        ///         the specified message.
        ///     </para>
        /// </devdoc>
        public ApkReaderException(string message)
            : base(message)
        {
        }
    }
}