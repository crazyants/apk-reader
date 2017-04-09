// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System;
using System.Globalization;

namespace ApkReader
{
    /// <devdoc>
    ///     <para>The exception that is thrown when using invalid arguments that are enumerators.</para>
    /// </devdoc>
    public class InvalidEnumArgumentException : ArgumentException
    {
        /// <devdoc>
        ///     <para>
        ///         Initializes a new instance of the <see cref='InvalidEnumArgumentException' /> class
        ///         without a message.
        ///     </para>
        /// </devdoc>
        public InvalidEnumArgumentException() : this(null)
        {
        }

        /// <devdoc>
        ///     <para>
        ///         Initializes a new instance of the <see cref='InvalidEnumArgumentException' /> class with
        ///         the specified message.
        ///     </para>
        /// </devdoc>
        public InvalidEnumArgumentException(string message)
            : base(message)
        {
        }

        /// <devdoc>
        ///     Initializes a new instance of the Exception class with a specified error message and a
        ///     reference to the inner exception that is the cause of this exception.
        ///     FxCop CA1032: Multiple constructors are required to correctly implement a custom exception.
        /// </devdoc>
        public InvalidEnumArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <devdoc>
        ///     <para>
        ///         Initializes a new instance of the <see cref='InvalidEnumArgumentException' /> class with
        ///         a
        ///         message generated from the argument, invalid value, and enumeration
        ///         class.
        ///     </para>
        /// </devdoc>
        public InvalidEnumArgumentException(string argumentName, int invalidValue, Type enumClass)
            : base(string.Format("The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.",
                argumentName,
                invalidValue.ToString(CultureInfo.CurrentCulture),
                enumClass.Name), argumentName)
        {
        }
    }
}