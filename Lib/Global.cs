/**
 * @file
 * @copyright  Copyright (c) 2020 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System;

namespace I18N.Net
{
    /**
     * Utility class for convenient access to localization functions.
     */
    public static class Global
    {
        /*===========================================================================
         *                           PUBLIC PROPERTIES
         *===========================================================================*/

        /**
         * Global localizer.
         */
        public static Localizer Localizer { get; private set; } = new Localizer();

        /*===========================================================================
         *                            PUBLIC METHODS
         *===========================================================================*/

        /**
         * Converts the language-neutral string @p text to its corresponding language-specific localized value
         * using the global localizer.
         * 
         * @param [in] text Language-neutral string
         * @return Language-specific localized string if found, or @p text otherwise
         */
        public static string Localize( PlainString text )
        {
            return Localizer.Localize( text );
        }

        /**
         * Converts the composite format string of the language-neutral formattable string @p frmtText (e.g. an interpolated string) 
         * to its corresponding language-specific localized composite format value using the global localizer, and then generates the 
         * result by formatting the localized composite format value along with the @p frmtText arguments by using the formatting 
         * conventions of the current culture.
         * 
         * @param [in] frmtText Language-neutral formattable string
         * @return Formatted string generated from the language-specific localized format string if found, or generated from @p frmtText otherwise
         */
        public static string Localize( FormattableString frmtText )
        {
            return Localizer.Localize( frmtText );
        }

        /**
         * Converts the language-neutral format string @p format to its corresponding language-specific localized format value
         * using the global localizer, and then generates the result by formatting the localized format value along with the @p args 
         * arguments by using the formatting conventions of the current culture.
         * 
         * @param [in] format Language-neutral format string
         * @param [in] args Arguments for the format string
         * @return Formatted string generated from the language-specific localized format string if found, or generated from @p format otherwise
         */
        public static string LocalizeFormat( string format, params object[] args )
        {
            return Localizer.LocalizeFormat( format, args );
        }
    }
}
