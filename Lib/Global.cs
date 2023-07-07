/// @file
/// @copyright  Copyright (c) 2022-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;

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
        public static Localizer Localizer { get; private set; } = new Localizer();

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <summary>
        /// Localizes a string using the global localizer.
        /// </summary>
        /// <seealso cref="Localizer.Localize(PlainString)"/>
        /// <param name="text">Language-neutral string</param>
        /// <returns>Language-specific localized string if found, or <paramref name="text"/> otherwise</returns>
        public static string Localize( PlainString text )
        {
            return Localizer.Localize( text );
        }

        /// <summary>
        /// Localizes an interpolated string using the global localizer.
        /// </summary>
        /// <seealso cref="Localizer.Localize(FormattableString)"/>
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
        /// <remarks>
        /// Converts the language-neutral strings in <paramref name="texts"/> to their corresponding language-specific localized values.
        /// </remarks>
        /// <param name="texts">Array of language-neutral strings</param>
        /// <returns>Array with the language-specific localized strings if found, or the language-neutral string otherwise</returns>
        public static IEnumerable<string> Localize( IEnumerable<string> texts )
        {
            return Localizer.Localize( texts );
        }

        /// <summary>
        /// Localizes and then formats a string using the global localizer.
        /// </summary>
        /// <seealso cref="Localizer.LocalizeFormat(string, object[])"/>
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
        /// <seealso cref="Localizer.Context(string)"/>
        /// <param name="contextId">Identifier of the context</param>
        /// <returns>Localizer for the given context</returns>
        public static Localizer Context( string contextId )
        {
            return Localizer.Context( contextId );
        }

    }
}
