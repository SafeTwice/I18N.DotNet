/// @file
/// @copyright  Copyright (c) 2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;

namespace I18N.DotNet
{
    /// <summary>
    /// Represents a text expression that can be localized.
    /// </summary>
    /// <remarks>
    /// The localized value is automatically updated when the localizer is updated.
    /// </remarks>
    public abstract class Localizable : IDisposable
    {
        //===========================================================================
        //                           PUBLIC PROPERTIES
        //===========================================================================

        /// <summary>
        /// Localized text expression translated by the associated localizer.
        /// </summary>
        public string Localized
        {
            get
            {
                m_localizedText ??= UpdatedLocalization;
                return m_localizedText;
            }
        }

        /// <summary>
        /// Localizer used to translate the text expression.
        /// </summary>
        public ILocalizer Localizer { get; }

        //===========================================================================
        //                               FINALIZER
        //===========================================================================

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Localizable()
        {
            Dispose( false );
        }

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <summary>
        /// Implicit conversion to string.
        /// </summary>
        /// <param name="localizedString">Translated string.</param>
        public static implicit operator string( Localizable localizedString )
        {
            return localizedString.Localized;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <inheritdoc/>
        public override string ToString() => Localized;

        //===========================================================================
        //                           PROTECTED PROPERTIES
        //===========================================================================

        /// <summary>
        /// Implemented in derived classes to provide the updated localization.
        /// </summary>
        public abstract string UpdatedLocalization { get; }

        //===========================================================================
        //                          PROTECTED CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="localizer">Localizer to translate the expression.</param>
        protected Localizable( ILocalizer localizer )
        {
            Localizer = localizer;

            Localizer.LocalizationsUpdated += OnLocalizerUpdated;
        }

        //===========================================================================
        //                            PROTECTED METHODS
        //===========================================================================

        /// <summary>
        /// Releases the used resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called from Dispose, <c>false</c> if called from the finalizer.</param>
        protected virtual void Dispose( bool disposing )
        {
            Localizer.LocalizationsUpdated -= OnLocalizerUpdated;
        }

        //===========================================================================
        //                            PRIVATE METHODS
        //===========================================================================

        private void OnLocalizerUpdated()
        {
            m_localizedText = null;
        }

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private string? m_localizedText;
    }
}
