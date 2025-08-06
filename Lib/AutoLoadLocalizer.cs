/// @file
/// @copyright  Copyright (c) 2023-2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        //                           PUBLIC PROPERTIES
        //===========================================================================

        /// <inheritdoc/>
        public string TargetLanguage => m_internalLocalizer?.TargetLanguage ?? CultureInfo.CurrentUICulture.Name;

        /// <inheritdoc/>
        public CultureInfo TargetCulture => m_internalLocalizer?.TargetCulture ?? CultureInfo.CurrentUICulture;

        //===========================================================================
        //                           PUBLIC CONSTANTS
        //===========================================================================

        /// <summary>
        /// Default identifier for the embedded resource containing the translations.
        /// </summary>
        public const string DEFAULT_RESOURCE_NAME = "Resources.I18N.xml";

        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

#pragma warning disable S3427 // This constructor is intended only for public use

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// When the localization methods are called for the first time, the translations are automatically loaded from
        /// the embedded resource identified by <paramref name="resourceName"/> inside the given <paramref name="assembly"/>
        /// (if translations have not been previously loaded explicitly).
        /// </remarks>
        /// <param name="resourceName">Name of the embedded resource for the XML file.</param>
        /// <param name="assembly">Assembly that contains the embedded XML file (the calling assembly will be used if <c>null</c>).</param>
        public AutoLoadLocalizer( string resourceName = DEFAULT_RESOURCE_NAME, Assembly? assembly = null )
        {
            m_assembly = assembly ?? Assembly.GetCallingAssembly();
            m_resourceName = resourceName;
            m_ignoreIfNotExists = false;
        }

#pragma warning restore S3427

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <inheritdoc/>
        public string Localize( PlainString text ) => InternalLocalizer.Localize( text );

        /// <inheritdoc/>
        public string Localize( FormattableString text ) => InternalLocalizer.Localize( text );

        /// <inheritdoc/>
        public IEnumerable<string> Localize( IEnumerable<string> texts ) => InternalLocalizer.Localize( texts );

        /// <inheritdoc/>
#if NET7_0_OR_GREATER
        public string LocalizeFormat( [StringSyntax( "CompositeFormat" )] string format, params object?[] args )
#else
        public string LocalizeFormat( string format, params object?[] args )
#endif
        {
            return InternalLocalizer.LocalizeFormat( format, args );
        }

        /// <inheritdoc/>
        public ILocalizer Context( string contextId ) => InternalLocalizer.Context( contextId );

        /// <inheritdoc/>
        public ILocalizer Context( IEnumerable<string> splitContextIds ) => InternalLocalizer.Context( splitContextIds );

        /// <inheritdoc/>
        public void LoadXML( string filePath, CultureInfo? culture = null )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( filePath, culture );
        }

        /// <inheritdoc/>
        public void LoadXML( string filePath, string language )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( filePath, language );
        }

        /// <inheritdoc/>
        public void LoadXML( string filePath, bool merge )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( filePath, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, CultureInfo? culture = null )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( stream, culture );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, string language )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( stream, language );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, bool merge )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( stream, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, CultureInfo? culture = null )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( doc, culture );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, string language )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( doc, language );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, bool merge )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( doc, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, CultureInfo? culture = null )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( assembly, resourceName, culture );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, string language )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( assembly, resourceName, language );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, bool merge )
        {
            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( assembly, resourceName, merge );
        }

        /// <summary>
        /// Loads translations for the given <paramref name="culture"/> from the embedded resource specified when creating the instance.
        /// </summary>
        /// <param name="culture">Culture for the target language of translations,
        ///                       or <c>null</c> to use the current UI culture (obtained from <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>).</param>
        /// <exception cref="ILoadableLocalizer.ParseException">Thrown when the embedded resource contents cannot be parsed properly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the embedded resource could not be found.</exception>
        public void Load( CultureInfo? culture )
        {
            var assembly = CheckAssembly();

            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( assembly, m_resourceName, culture );
        }

        /// <summary>
        /// Loads translations for the given <paramref name="language"/> from the embedded resource specified when creating the instance.
        /// </summary>
        /// <param name="language">Name, code or identifier for the target language of translations.</param>
        /// <exception cref="ILoadableLocalizer.ParseException">Thrown when the embedded resource contents cannot be parsed properly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the embedded resource could not be found.</exception>
        public void Load( string language )
        {
            var assembly = CheckAssembly();

            m_internalLocalizer ??= new Localizer();
            m_internalLocalizer.LoadXML( assembly, m_resourceName, language );
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
                        m_internalLocalizer.LoadXML( m_assembly, m_resourceName, false, m_ignoreIfNotExists );
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
