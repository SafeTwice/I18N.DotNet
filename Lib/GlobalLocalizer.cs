/// @file
/// @copyright  Copyright (c) 2020-2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;

#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

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
        /// <param name="text">Base-language formattable string.</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found, 
        ///          or generated from <paramref name="text"/> otherwise.</returns>
        public static string Localize( FormattableString text )
        {
            return Localizer.Localize( text );
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
#if NET7_0_OR_GREATER
        public static string LocalizeFormat( [StringSyntax( "CompositeFormat" )] string format, params object?[] args )
#else
        public static string LocalizeFormat( string format, params object?[] args )
#endif
        {
            return Localizer.LocalizeFormat( format, args );
        }

        /// <summary>
        /// Gets a localizable expression for a string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.GetLocalizable(PlainString)"/>
        /// <param name="text">Base-language string.</param>
        /// <returns><see cref="Localizable"/> instance that can localize the string.</returns>
        public static Localizable GetLocalizable( PlainString text )
        {
            return Localizer.GetLocalizable( text );
        }

        /// <summary>
        /// Gets a localizable expression for an interpolated string using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.GetLocalizable(FormattableString)"/>
        /// <param name="formattableText">Base-language formattable string.</param>
        /// <returns><see cref="Localizable"/> instance that can localize the interpolated string.</returns>
        public static Localizable GetLocalizable( FormattableString formattableText )
        {
            return Localizer.GetLocalizable( formattableText );
        }

        /// <summary>
        /// Gets localizable expressions for multiple strings using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.GetLocalizables(IEnumerable{string})"/>
        /// <param name="texts">Base-language strings.</param>
        /// <returns><see cref="Localizable"/> instances that can localize the input strings.</returns>
        public static IEnumerable<Localizable> GetLocalizables( IEnumerable<string> texts )
        {
            return Localizer.GetLocalizables( texts );
        }

        /// <summary>
        /// Gets a localizable expression for string formatting operation using the global localizer.
        /// </summary>
        /// <seealso cref="ILocalizer.GetLocalizableFormat(string, object[])"/>
        /// <param name="format">Base-language format string.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns><see cref="Localizable"/> instance that can localize then execute the format operation.</returns>
#if NET7_0_OR_GREATER
        public static Localizable GetLocalizableFormat( [StringSyntax( "CompositeFormat" )] string format, params object?[] args )
#else
        public static Localizable GetLocalizableFormat( string format, params object?[] args )
#endif
        {
            return Localizer.GetLocalizableFormat( format, args );
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
