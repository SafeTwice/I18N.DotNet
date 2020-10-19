using System;

namespace I18N.Net
{
    /**
     * Represents just a string. This class is used to allow interpolated strings to preferably be passed as FormattableString 
     * instead of string to methods that overload both types.
     */
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

        public static implicit operator PlainString( FormattableString arg )
        {
            throw new InvalidOperationException();
        }
    }
}
