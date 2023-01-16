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
        public string Value { get; }

        public PlainString( string value )
        {
            Value = value;
        }

        public static implicit operator PlainString( string value )
        {
            return new PlainString( value );
        }

        [ExcludeFromCodeCoverage]
        public static implicit operator PlainString( FormattableString arg )
        {
            throw new InvalidOperationException();
        }
    }
}
