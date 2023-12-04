/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;

namespace I18N.DotNet
{
    /// <summary>
    /// Utility class for convenient access to localization functions.
    /// </summary>
    public static class Global
    {
        //===========================================================================
        //                           PUBLIC PROPERTIES
        //===========================================================================

        /// <value>
        /// Global localizer.
        /// </value>
        public static Localizer Localizer
        {
            get
            {
                g_localizer ??= new Localizer( CultureInfo.CurrentUICulture.Name );
                return g_localizer;
            }

            set
            {
                g_localizer = value;
            }
        }

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <summary>
        /// Localizes a string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Localize(PlainString)"/>
        /// <param name="text">Language-neutral string</param>
        /// <returns>Language-specific localized string if found, or <paramref name="text"/> otherwise</returns>
        public static string Localize( PlainString text )
        {
            return Localizer.Localize( text );
        }

        /// <summary>
        /// Localizes an interpolated string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Localize(FormattableString)"/>
        /// <param name="frmtText">Language-neutral formattable string</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found, 
        ///          or generated from <paramref name="frmtText"/> otherwise</returns>
        public static string Localize( FormattableString frmtText )
        {
            return Localizer.Localize( frmtText );
        }

        /// <summary>
        /// Localizes multiple strings.
        /// </summary>
        /// <seealso cref="ILocalizer.Localize(IEnumerable{string})"/>
        /// <param name="texts">Array of language-neutral strings</param>
        /// <returns>Array with the language-specific localized strings if found, or the language-neutral string otherwise</returns>
        public static IEnumerable<string> Localize( IEnumerable<string> texts )
        {
            return Localizer.Localize( texts );
        }

        /// <summary>
        /// Localizes and then formats a string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.LocalizeFormat(string, object[])"/>
        /// <param name="format">Language-neutral format string</param>
        /// <param name="args">Arguments for the format string</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found,
        ///          or generated from <paramref name="format"/> otherwise</returns>
        public static string LocalizeFormat( string format, params object[] args )
        {
            return Localizer.LocalizeFormat( format, args );
        }

        /// <summary>
        /// Gets a context in the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Context(string)"/>
        /// <param name="contextId">Identifier of the context</param>
        /// <returns>Localizer for the given context</returns>
        public static ILocalizer Context( string contextId )
        {
            return Localizer.Context( contextId );
        }

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private static Localizer? g_localizer;
    }
}
