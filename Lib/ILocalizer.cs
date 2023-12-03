/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;

namespace I18N.DotNet
{
    /// <summary>
    /// Converter of strings from a language-neutral value to its corresponding language-specific localization.
    /// </summary>
    public interface ILocalizer
    {
        //===========================================================================
        //                                  METHODS
        //===========================================================================

        /// <summary>
        /// Localizes a string.
        /// </summary>
        /// <remarks>
        /// Converts the language-neutral string <paramref name="text"/> to its corresponding language-specific localized value.
        /// </remarks>
        /// <param name="text">Language-neutral string</param>
        /// <returns>Language-specific localized string if found, or <paramref name="text"/> otherwise</returns>
        string Localize( PlainString text );

        /// <summary>
        /// Localizes an interpolated string.
        /// </summary>
        /// <remarks>
        /// Converts the composite format string of the language-neutral formattable string <paramref name="frmtText"/> ( e.g.an interpolated string) 
        /// to its corresponding language-specific localized composite format value, and then generates the result by formatting the 
        /// localized composite format value along with the<paramref name="frmtText"/> arguments by using the formatting conventions of the current culture.
        /// </remarks>
        /// <param name="frmtText">Language-neutral formattable string</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found,
        ///          or generated from<paramref name="frmtText"/> otherwise</returns>
        string Localize( FormattableString frmtText );

        /// <summary>
        /// Localizes and then formats a string.
        /// </summary>
        /// <remarks>
        /// Converts the language-neutral format string <paramref name="format"/> to its corresponding language-specific localized format value,
        /// and then generates the result by formatting the localized format value along with the<paramref name= "args" /> arguments by using the formatting
        /// conventions of the current culture.
        /// </remarks>
        /// <param name="format">Language-neutral format string</param>
        /// <param name="args">Arguments for the format string</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found,
        ///          or generated from<paramref name="format"/> otherwise</returns>
        string LocalizeFormat( string format, params object[] args );

        /// <summary>
        /// Localizes multiple strings.
        /// </summary>
        /// <remarks>
        /// Converts the language-neutral strings in <paramref name="texts"/> to their corresponding language-specific localized values.
        /// </remarks>
        /// <param name="texts">Language-neutral strings</param>
        /// <returns></returns>
        IEnumerable<string> Localize( IEnumerable<string> texts );

        /// <summary>
        /// Gets the localizer for a context in the current localizer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Contexts are used to disambiguate the conversion of the same language-neutral string to different
        /// language-specific strings depending on the context where the conversion is performed.
        /// </para>
        /// <para>
        /// Contexts can be nested. The context identifier can identify a chain of nested contexts by separating
        /// their identifiers with the '.' character (left = outermost / right = innermost).
        /// </para>
        /// </remarks>
        /// <param name="contextId">Identifier of the context</param>
        /// <returns>Localizer for the given context</returns>
        ILocalizer Context( string contextId );

        /// <summary>
        /// Gets the localizer for a context in the current localizer.
        /// </summary>
        /// <remarks>
        /// Contexts are used to disambiguate the conversion of the same language-neutral string to different
        /// language-specific strings depending on the context where the conversion is performed.
        /// </remarks>
        /// <param name="splitContextIds">Chain of context identifiers in split form</param>
        /// <returns>Localizer for the given context</returns>
        ILocalizer Context( IEnumerable<string> splitContextIds );
    }
}
