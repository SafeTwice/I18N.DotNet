/// @file
/// @copyright  Copyright (c) 2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;

#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace I18N.DotNet
{
    /// <summary>
    /// Implements a localized formattable string.
    /// </summary>
    internal class LocalizableFormat : Localizable
    {
        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="format">Base-language format string.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <param name="localizer">Localizer to translate the string.</param>
#if NET7_0_OR_GREATER
        public LocalizableFormat( [StringSyntax( "CompositeFormat" )] string format, object?[] args, ILocalizer localizer )
#else
        public LocalizableFormat( string format, object?[] args, ILocalizer localizer )
#endif
            : base( localizer )
        {
            m_format = format;
            m_args = args;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="formattableText">Base-language string.</param>
        /// <param name="localizer">Localizer to translate the string.</param>
        public LocalizableFormat( FormattableString formattableText, ILocalizer localizer ) : base( localizer )
        {
            m_format = formattableText.Format;
            m_args = formattableText.GetArguments();
        }


        //===========================================================================
        //                           PROTECTED PROPERTIES
        //===========================================================================

        /// <inheritdoc/>
        public override string UpdatedLocalization => Localizer.LocalizeFormat( m_format, m_args );

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private readonly string m_format;
        private readonly object?[] m_args;
    }
}
