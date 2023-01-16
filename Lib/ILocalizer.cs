/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections;
using System.Collections.Generic;

namespace I18N.Net
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
        /// Converts the composite format string of the language-neutral formattable string <paramref name = "frmtText" /> ( e.g.an interpolated string) 
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
        /// Converts the language-neutral format string <paramref name = "format" /> to its corresponding language-specific localized format value,
        /// and then generates the result by formatting the localized format value along with the<paramref name= "args" /> arguments by using the formatting
        /// conventions of the current culture.
        /// </remarks>
        /// <param name="format">Language-neutral format string</param>
        /// <param name="args">Arguments for the format string</param>
        /// <returns>Formatted string generated from the language-specific localized format string if found,
        ///          or generated from<paramref name="frmtText"/> otherwise</returns>
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
    }
}
