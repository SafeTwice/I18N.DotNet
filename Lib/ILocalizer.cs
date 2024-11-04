/// @file
/// @copyright  Copyright (c) 2023-2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;

#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace I18N.DotNet
{
    /// <summary>
    /// Converter of strings from a base-language value to its corresponding language-specific localization.
    /// </summary>
    public interface ILocalizer
    {
        //===========================================================================
        //                                PROPERTIES
        //===========================================================================

        /// <summary>
        /// Target language of the localizer.
        /// </summary>
        public string TargetLanguage { get; }

        /// <summary>
        /// Target culture of the localizer.
        /// </summary>
        public CultureInfo TargetCulture { get; }

        //===========================================================================
        //                                  EVENTS
        //===========================================================================

        /// <summary>
        /// Event triggered when the localizations are updated.
        /// </summary>
        public event Action LocalizationsUpdated;

        //===========================================================================
        //                                  METHODS
        //===========================================================================

        /// <summary>
        /// Localizes a string.
        /// </summary>
        /// <remarks>
        /// Converts the base-language string <paramref name="text"/> to its corresponding language-specific localized value.
        /// </remarks>
        /// <param name="text">Base-language string.</param>
        /// <returns>Language-specific localized string if found, or <paramref name="text"/> otherwise.</returns>
        string Localize( PlainString text );

        /// <summary>
        /// Localizes an interpolated string.
        /// </summary>
        /// <remarks>
        /// Converts the composite format string of the base-language formattable string <paramref name="text"/> (e.g. an interpolated string) 
        /// to its corresponding language-specific localized composite format value, and then generates the result by formatting the 
        /// localized composite format value along with the <paramref name="text"/> arguments by using the formatting conventions of the localizer culture.
        /// </remarks>
        /// <param name="text">Base-language formattable string.</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found,
        ///          or generated from <paramref name="text"/> otherwise.</returns>
        /// <exception cref="FormatException">Thrown when the localized format value of <paramref name="text"/> is invalid.</exception>
        string Localize( FormattableString text );

        /// <summary>
        /// Localizes multiple strings.
        /// </summary>
        /// <remarks>
        /// Converts the base-language strings in <paramref name="texts"/> to their corresponding language-specific localized values.
        /// </remarks>
        /// <param name="texts">Base-language strings.</param>
        /// <returns>Language-specific localized strings if found, or the base-language string otherwise.</returns>
        IEnumerable<string> Localize( IEnumerable<string> texts );

        /// <summary>
        /// Localizes and then formats a string.
        /// </summary>
        /// <remarks>
        /// Converts the base-language format string <paramref name="format"/> to its corresponding language-specific localized format value,
        /// and then generates the result by formatting the localized format value along with the <paramref name= "args" /> arguments by using the formatting
        /// conventions of the localizer culture.
        /// </remarks>
        /// <param name="format">Base-language format string.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found,
        ///          or generated from <paramref name="format"/> otherwise.</returns>
        /// <exception cref="FormatException">Thrown when <paramref name="format"/> or its localized format value is invalid.</exception>
#if NET7_0_OR_GREATER
        string LocalizeFormat( [StringSyntax( "CompositeFormat" )] string format, params object?[] args );
#else
        string LocalizeFormat( string format, params object?[] args );
#endif

        /// <summary>
        /// Gets a localizable expression for a string.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="Localizable"/> instance will act as a delegated call to <see cref="Localize(PlainString)"/>.
        /// </remarks>
        /// <param name="text">Base-language string.</param>
        /// <returns><see cref="Localizable"/> instance that can localize the string.</returns>
        Localizable GetLocalizable( PlainString text );

        /// <summary>
        /// Gets a localizable expression for an interpolated string.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="Localizable"/> instance will act as a delegated call to <see cref="Localize(FormattableString)"/>.
        /// </remarks>
        /// <param name="formattableText">Base-language formattable string.</param>
        /// <returns><see cref="Localizable"/> instance that can localize the interpolated string.</returns>
        Localizable GetLocalizable( FormattableString formattableText );

        /// <summary>
        /// Gets localizable expressions for multiple strings.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="Localizable"/> instances will act as a delegated calls to <see cref="Localize(PlainString)"/>.
        /// </remarks>
        /// <param name="texts">Base-language strings.</param>
        /// <returns><see cref="Localizable"/> instances that can localize the input strings.</returns>
        IEnumerable<Localizable> GetLocalizables( IEnumerable<string> texts );

        /// <summary>
        /// Gets a localizable expression for string formatting operation.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="Localizable"/> instance will act as a delegated call to <see cref="LocalizeFormat(string, object[])"/>.
        /// </remarks>
        /// <param name="format">Base-language format string.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns><see cref="Localizable"/> instance that can localize then execute the format operation.</returns>
#if NET7_0_OR_GREATER
        Localizable GetLocalizableFormat( [StringSyntax( "CompositeFormat" )] string format, params object?[] args );
#else
        Localizable GetLocalizableFormat( string format, params object?[] args );
#endif

        /// <summary>
        /// Gets the localizer for a context in the current localizer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Contexts are used to disambiguate the conversion of the same base-language string to different
        /// language-specific strings depending on the context where the conversion is performed.
        /// </para>
        /// <para>
        /// Contexts can be nested. The context identifier can identify a chain of nested contexts by separating
        /// their identifiers with the '.' character (left = outermost / right = innermost).
        /// </para>
        /// </remarks>
        /// <param name="contextId">Identifier of the context.</param>
        /// <returns>Localizer for the given context.</returns>
        ILocalizer Context( string contextId );

        /// <summary>
        /// Gets the localizer for a context in the current localizer.
        /// </summary>
        /// <remarks>
        /// Contexts are used to disambiguate the conversion of the same base-language string to different
        /// language-specific strings depending on the context where the conversion is performed.
        /// </remarks>
        /// <param name="splitContextIds">Chain of context identifiers in split form.</param>
        /// <returns>Localizer for the given context.</returns>
        ILocalizer Context( IEnumerable<string> splitContextIds );
    }
}
