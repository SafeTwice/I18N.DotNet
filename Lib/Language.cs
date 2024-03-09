/// @file
/// @copyright  Copyright (c) 2023-2024 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Globalization;

namespace I18N.DotNet
{
    /// <summary>
    /// Represents a language for localization purposes.
    /// </summary>
    internal class Language
    {
        //===========================================================================
        //                           PUBLIC PROPERTIES
        //===========================================================================

        /// <summary>
        /// Full name of the language.
        /// </summary>
        public string Full { get; }

        /// <summary>
        /// Primary name of the language.
        /// </summary>
        public string? Primary { get; }

        /// <summary>
        /// Format provider for the language.
        /// </summary>
        public IFormatProvider FormatProvider { get; }

        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="language">Name, code or identifier for the language</param>
        public Language( string language )
        {
            (Full, Primary) = SplitLanguage( language );

            try
            {
                FormatProvider = CultureInfo.GetCultureInfo( language );
            }
            catch( CultureNotFoundException )
            {
                FormatProvider = CultureInfo.InvariantCulture;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="culture">Culture for the language</param>
        public Language( CultureInfo culture )
        {
            (Full, Primary) = SplitLanguage( culture.Name );
            FormatProvider = culture;
        }

        //===========================================================================
        //                            PRIVATE METHODS
        //===========================================================================

        private static (string full, string? primary) SplitLanguage( string language )
        {
            var full = language.ToLower();

            var splitLanguage = full.Split( LANG_SEPARATOR, 2 );
            if( splitLanguage.Length > 1 )
            {
                return (full, splitLanguage[ 0 ]);
            }
            else
            {
                return (full, null);
            }
        }

        //===========================================================================
        //                           PRIVATE CONSTANTS
        //===========================================================================

        private static readonly char[] LANG_SEPARATOR = { '-' };
    }
}
