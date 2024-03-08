/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace I18N.DotNet
{
    /// <summary>
    /// Implementation of a localizer which configuration is automatically loaded from an embedded resource.
    /// </summary>
    public class AutoLoadLocalizer : ILoadableLocalizer
    {
        //===========================================================================
        //                           PUBLIC CONSTANTS
        //===========================================================================

        /// <value>
        /// Default identifier for the embedded resource containing the translations.
        /// </value>
        public const string DEFAULT_RESOURCE_NAME = "Resources.I18N.xml";

        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourceName">Name of the embedded resource for the XML text</param>
        /// <param name="assembly">Assembly that contains the embedded XML text (the calling assembly will be used if <c>null</c>)</param>
        public AutoLoadLocalizer( string resourceName = DEFAULT_RESOURCE_NAME, Assembly? assembly = null )
        {
            m_assembly = assembly ?? Assembly.GetCallingAssembly();
            m_resourceName = resourceName;
            m_ignoreIfNotExists = false;
        }

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <inheritdoc/>
        public string Localize( PlainString text ) => InternalLocalizer.Localize( text );

        /// <inheritdoc/>
        public string Localize( FormattableString frmtText ) => InternalLocalizer.Localize( frmtText );

        /// <inheritdoc/>
        public IEnumerable<string> Localize( IEnumerable<string> texts ) => InternalLocalizer.Localize( texts );

        /// <inheritdoc/>
        public string LocalizeFormat( string format, params object[] args ) => InternalLocalizer.LocalizeFormat( format, args );

        /// <inheritdoc/>
        public ILocalizer Context( string contextId ) => InternalLocalizer.Context( contextId );

        /// <inheritdoc/>
        public ILocalizer Context( IEnumerable<string> splitContextIds ) => InternalLocalizer.Context( splitContextIds );

        /// <inheritdoc/>
        public void LoadXML( string filepath, string? language = null, bool merge = false )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( filepath, language, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, string? language = null, bool merge = false )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( stream, language, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, string? language = null, bool merge = false )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( doc, language, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, string? language = null, bool merge = false )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( assembly, resourceName, language, merge );
        }

        /// <summary>
        /// Loads the localization configuration from the embedded resource using the given language.
        /// </summary>
        /// <remarks>
        /// If this method is not called explicitly, the translations are automatically loaded from the embedded resource using the
        /// current UI language when a localization method is called for the first time.
        /// </remarks>
        /// <param name="language">Name, code or identifier for the target language of translations,
        ///                        or <c>null</c> to use the current UI language (obtained from <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>)</param>
        /// <param name="merge"> Replaces the current translations with the loaded ones when<c>false</c>,
        ///                      otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ILoadableLocalizer.ParseException">Thrown when the embedded resource contents cannot be parsed properly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the embedded resource could not be found</exception>
        public void Load( string? language, bool merge = false )
        {
            var assembly = CheckAssembly();

            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( assembly, m_resourceName, language, merge );
        }

        //===========================================================================
        //                          INTERNAL CONSTRUCTORS
        //===========================================================================

        internal AutoLoadLocalizer()
        {
            m_assembly = Assembly.GetEntryAssembly();
            m_resourceName = DEFAULT_RESOURCE_NAME;
            m_ignoreIfNotExists = true;
        }

        //===========================================================================
        //                           PRIVATE PROPERTIES
        //===========================================================================

        private Localizer InternalLocalizer
        {
            get
            {
                if( m_internalLocalizer == null )
                {
                    m_internalLocalizer = new Localizer();
                    if( m_assembly != null )
                    {
                        m_internalLocalizer.LoadXML( m_assembly, m_resourceName, null, true, m_ignoreIfNotExists );
                    }
                }
                return m_internalLocalizer;
            }
        }

        //===========================================================================
        //                            PRIVATE METHODS
        //===========================================================================

        [ExcludeFromCodeCoverage]
        private Assembly CheckAssembly()
        {
            if( m_assembly == null )
            {
                throw new InvalidOperationException( "Invalid entry assembly" );
            }
            return m_assembly;
        }

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private readonly Assembly? m_assembly;
        private readonly string m_resourceName;
        private readonly bool m_ignoreIfNotExists;

        private Localizer? m_internalLocalizer;
    }
}
