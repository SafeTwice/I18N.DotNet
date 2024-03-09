/// @file
/// @copyright  Copyright (c) 2020-2024 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace I18N.DotNet
{
    /// <summary>
    /// Simple loadable localizer.
    /// </summary>
    public class Localizer : ContextLocalizer, ILoadableLocalizer
    {
        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Localizer()
        {
        }

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <inheritdoc/>
        public void LoadXML( string filepath, CultureInfo? culture = null )
        {
            LoadXML( XDocument.Load( filepath, LoadOptions.SetLineInfo ), culture );
        }

        /// <inheritdoc/>
        public void LoadXML( string filepath, string language )
        {
            LoadXML( XDocument.Load( filepath, LoadOptions.SetLineInfo ), language );
        }

        /// <inheritdoc/>
        public void LoadXML( string filepath, bool merge )
        {
            LoadXML( XDocument.Load( filepath, LoadOptions.SetLineInfo ), merge );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, CultureInfo? culture = null )
        {
            LoadXML( XDocument.Load( stream, LoadOptions.SetLineInfo ), culture );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, string language )
        {
            LoadXML( XDocument.Load( stream, LoadOptions.SetLineInfo ), language );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, bool merge )
        {
            LoadXML( XDocument.Load( stream, LoadOptions.SetLineInfo ), merge );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, CultureInfo? culture = null )
        {
            Language = new Language( culture ?? CultureInfo.CurrentUICulture );

            LoadXML( doc, false );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, string language )
        {
            Language = new Language( language );

            LoadXML( doc, false );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, bool merge )
        {
            if( !merge )
            {
                Clear();
            }

            var rootElement = doc.Root ?? throw new ILoadableLocalizer.ParseException( $"XML has no root element" );

            if( rootElement.Name != "I18N" )
            {
                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) rootElement ).LineNumber}: Invalid XML root element" );
            }

            Load( rootElement );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, CultureInfo? culture = null )
        {
            Language = new Language( culture ?? CultureInfo.CurrentUICulture );

            LoadXML( assembly, resourceName, false );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, string language )
        {
            Language = new Language( language );

            LoadXML( assembly, resourceName, false );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, bool merge )
        {
            LoadXML( assembly, resourceName, merge, false );
        }

        //===========================================================================
        //                            INTERNAL METHODS
        //===========================================================================

        internal void LoadXML( Assembly assembly, string resourceName, bool merge, bool ignoreIfNotExists )
        {
            var assemblyName = assembly.GetName().Name;
            string usedResourceName;

            if( ( assemblyName != null ) && !resourceName.StartsWith( assemblyName ) )
            {
                usedResourceName = assemblyName + "." + resourceName;
            }
            else
            {
                usedResourceName = resourceName;
            }

            using var stream = assembly.GetManifestResourceStream( usedResourceName );

            if( stream == null )
            {
                if( ignoreIfNotExists )
                {
                    return;
                }

                throw new InvalidOperationException( $"Cannot find resource '{usedResourceName}'" );
            }

            LoadXML( stream, merge );
        }
    }
}
