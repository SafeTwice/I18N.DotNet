using System;
using System.Diagnostics.CodeAnalysis;

namespace I18N.Net
{
    /// <summary>
    /// Represents just a string. This class is used to allow interpolated strings to preferably be passed as FormattableString 
    /// instead of string to methods that overload both types.
    /// </summary>
    public class PlainString
    {
        /// <value>
        /// Value of the string.
        /// </value>
        public string Value { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PlainString( string value )
        {
            Value = value;
        }

        /// <summary>
        /// Converts a string value to a PlainString.
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator PlainString( string value )
        {
            return new PlainString( value );
        }

        /// <summary>
        /// Converts a FormattableString value to a PlainString.
        /// </summary>
        /// <remarks>
        /// This implicit operator is needed to avoid FormattableString values to be automatically
        /// converted to string and then to PlainString when resolving parameter overloads.
        /// </remarks>
        /// <param arg="value">Value</param>
        /// <exception cref="InvalidOperationException">Always thrown</exception>
        [ExcludeFromCodeCoverage]
        public static implicit operator PlainString( FormattableString arg )
        {
            throw new InvalidOperationException();
        }
    }
}
