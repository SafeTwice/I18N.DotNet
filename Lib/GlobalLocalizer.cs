/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;

namespace I18N.DotNet
{
    /// <summary>
    /// Utility class for convenient access to localization functions.
    /// </summary>
    public static class GlobalLocalizer
    {
        //===========================================================================
        //                           PUBLIC PROPERTIES
        //===========================================================================

        /// <summary>
        /// Global localizer.
        /// </summary>
        public static AutoLoadLocalizer Localizer { get; } = new AutoLoadLocalizer();

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <summary>
        /// Localizes a string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Localize(PlainString)"/>
        /// <param name="text">Base-language string.</param>
        /// <returns>Language-specific localized string if found, or <paramref name="text"/> otherwise.</returns>
        public static string Localize( PlainString text )
        {
            return Localizer.Localize( text );
        }

        /// <summary>
        /// Localizes an interpolated string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Localize(FormattableString)"/>
        /// <param name="frmtText">Base-language formattable string.</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found, 
        ///          or generated from <paramref name="frmtText"/> otherwise.</returns>
        public static string Localize( FormattableString frmtText )
        {
            return Localizer.Localize( frmtText );
        }

        /// <summary>
        /// Localizes multiple strings using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Localize(IEnumerable{string})"/>
        /// <param name="texts">Base-language strings.</param>
        /// <returns>Language-specific localized strings if found, or the base-language string otherwise.</returns>
        public static IEnumerable<string> Localize( IEnumerable<string> texts )
        {
            return Localizer.Localize( texts );
        }

        /// <summary>
        /// Localizes and then formats a string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.LocalizeFormat(string, object[])"/>
        /// <param name="format">Base-language format string.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found,
        ///          or generated from <paramref name="format"/> otherwise.</returns>
        public static string LocalizeFormat( string format, params object[] args )
        {
            return Localizer.LocalizeFormat( format, args );
        }

        /// <summary>
        /// Gets a context in the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Context(string)"/>
        /// <param name="contextId">Identifier of the context.</param>
        /// <returns>Localizer for the given context.</returns>
        public static ILocalizer Context( string contextId )
        {
            return Localizer.Context( contextId );
        }

        /// <summary>
        /// Gets a context in the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.Context(IEnumerable{string})"/>
        /// <param name="splitContextIds">Chain of context identifiers in split form.</param>
        /// <returns>Localizer for the given context.</returns>
        public static ILocalizer Context( IEnumerable<string> splitContextIds )
        {
            return Localizer.Context( splitContextIds );
        }
    }
}
