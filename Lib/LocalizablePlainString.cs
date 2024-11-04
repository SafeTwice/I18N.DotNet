/// @file
/// @copyright  Copyright (c) 2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

namespace I18N.DotNet
{
    /// <summary>
    /// Implements a localized plain string.
    /// </summary>
    internal class LocalizablePlainString : Localizable
    {
        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text">Base-language string.</param>
        /// <param name="localizer">Localizer to translate the string.</param>
        public LocalizablePlainString( string text, ILocalizer localizer ) : base( localizer )
        {
            m_source = text;
        }

        //===========================================================================
        //                           PROTECTED PROPERTIES
        //===========================================================================

        /// <inheritdoc/>
        public override string UpdatedLocalization => Localizer.Localize( m_source );

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private readonly string m_source;
    }
}
