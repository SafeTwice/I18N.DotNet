/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace I18N.DotNet
{
    /// <summary>
    /// Converter of strings from a language-neutral value to its corresponding language-specific localization.
    /// </summary>
    public class Localizer : ILoadableLocalizer
    {
        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// The target languange of translations is set to the current UI language (obtained from <see cref="CultureInfo.CurrentUICulture"/>).
        /// </remarks>
        public Localizer()
        {
        }

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <inheritdoc/>
        public string Localize( PlainString text )
        {
            if( m_localizations.TryGetValue( text.Value, out var localizedText ) )
            {
                return localizedText;
            }
            else if( m_parentContext != null )
            {
                return m_parentContext.Localize( text );
            }
            else
            {
                return text.Value;
            }
        }

        /// <inheritdoc/>
        public string Localize( FormattableString frmtText )
        {
            return LocalizeFormat( frmtText.Format, frmtText.GetArguments() );
        }

        /// <inheritdoc/>
        public string LocalizeFormat( string format, params object?[] args )
        {
            if( m_localizations.TryGetValue( format, out var localizedFormat ) )
            {
                return String.Format( localizedFormat, args );
            }
            else if( m_parentContext != null )
            {
                return m_parentContext.LocalizeFormat( format, args );
            }
            else
            {
                return String.Format( format, args );
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> Localize( IEnumerable<string> texts )
        {
            var result = new List<string>();

            foreach( var text in texts )
            {
                result.Add( Localize( text ) );
            }

            return result;
        }

        /// <inheritdoc/>
        ILocalizer ILocalizer.Context( string contextId )
        {
            return GetContext( contextId );
        }

        /// <inheritdoc/>
        ILocalizer ILocalizer.Context( IEnumerable<string> splitContextIds )
        {
            return GetContext( splitContextIds );
        }

        /// <inheritdoc cref="ILocalizer.Context(string)"/>
        public Localizer Context( string contextId )
        {
            return GetContext( contextId );
        }

        /// <inheritdoc cref="ILocalizer.Context(IEnumerable{string})"/>
        public Localizer Context( IEnumerable<string> splitContextIds )
        {
            return GetContext( splitContextIds );
        }

        /// <inheritdoc/>
        public void LoadXML( string filepath, string? language = null, bool merge = true )
        {
            LoadXML( XDocument.Load( filepath, LoadOptions.SetLineInfo ), language, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( Stream stream, string? language = null, bool merge = true )
        {
            LoadXML( XDocument.Load( stream, LoadOptions.SetLineInfo ), language, merge );
        }

        /// <inheritdoc/>
        public void LoadXML( XDocument doc, string? language = null, bool merge = true )
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

            Load( rootElement, new Language( language ?? CultureInfo.CurrentUICulture.Name ) );
        }

        /// <inheritdoc/>
        public void LoadXML( Assembly assembly, string resourceName, string? language = null, bool merge = true )
        {
            LoadXML( assembly, resourceName, language, merge, false );
        }

        //===========================================================================
        //                            INTERNAL METHODS
        //===========================================================================

        internal void LoadXML( Assembly assembly, string resourceName, string? language, bool merge, bool ignoreIfNotExists )
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

            LoadXML( stream, language, merge );
        }

        //===========================================================================
        //                          PRIVATE NESTED TYPES
        //===========================================================================

        private class Language
        {
            public string Full { get; }
            public string? Primary { get; }

            public Language( string language )
            {
                Full = language.ToLower();

                var splitLanguage = Full.Split( new char[] { '-' }, 2 );
                if( splitLanguage.Length > 1 )
                {
                    Primary = splitLanguage[ 0 ];
                }
            }
        }

        private enum EValueType
        {
            NO_MATCH,
            FULL,
            PRIMARY
        }

        //===========================================================================
        //                          PRIVATE CONSTRUCTORS
        //===========================================================================

        private Localizer( Localizer parent )
        {
            m_parentContext = parent;
        }

        //===========================================================================
        //                            PRIVATE METHODS
        //===========================================================================

        private void Load( XElement element, Language language )
        {
            foreach( var childElement in element.Elements() )
            {
                switch( childElement.Name.ToString() )
                {
                    case "Entry":
                        LoadEntry( childElement, language );
                        break;

                    case "Context":
                        LoadContext( childElement, language );
                        break;

                    default:
                        throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Invalid XML element" );
                }
            }
        }

        private void LoadEntry( XElement element, Language language )
        {
            string? key = null;
            string? valueFull = null;
            string? valuePrimary = null;

            foreach( var childElement in element.Elements() )
            {
                switch( childElement.Name.ToString() )
                {
                    case "Key":
                        if( key != null )
                        {
                            throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Too many child '{childElement.Name}' XML elements" );
                        }
                        key = UnescapeEscapeCodes( childElement.Value );
                        break;

                    case "Value":
                        string? loadedValue;
                        var loadedValueType = LoadValue( childElement, language, out loadedValue );
                        if( loadedValueType == EValueType.FULL )
                        {
                            if( valueFull != null )
                            {
                                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Too many child '{childElement.Name}' XML elements with the same 'lang' attribute" );
                            }
                            valueFull = loadedValue;
                        }
                        else if( loadedValueType == EValueType.PRIMARY )
                        {
                            if( valuePrimary != null )
                            {
                                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Too many child '{childElement.Name}' XML elements with the same 'lang' attribute" );
                            }
                            valuePrimary = loadedValue;
                        }
                        break;

                    default:
                        throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Invalid XML element" );
                }
            }

            if( key == null )
            {
                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing child 'Key' XML element" );
            }

            var value = ( valueFull ?? valuePrimary );

            if( value != null )
            {
                m_localizations[ key ] = value;
            }
        }

        private static EValueType LoadValue( XElement element, Language language, out string? value )
        {
            string? lang = element.Attribute( "lang" )?.Value.ToLower();
            if( lang == null )
            {
                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing attribute 'lang' in '{element.Name}' XML element" );
            }

            if( lang == language.Full )
            {
                value = UnescapeEscapeCodes( element.Value );
                return EValueType.FULL;
            }
            else if( lang == language.Primary )
            {
                value = UnescapeEscapeCodes( element.Value );
                return EValueType.PRIMARY;
            }
            else
            {
                value = null;
                return EValueType.NO_MATCH;
            }
        }

        private static string UnescapeEscapeCodes( string text )
        {
            return Regex.Replace( text, @"\\([nrftvb\\]|x[0-9A-Fa-f]{1,4}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})", m =>
            {
                var payload = m.Groups[ 1 ].Value;
                switch( payload )
                {
                    case "n":
                        return "\n";
                    case "r":
                        return "\r";
                    case "f":
                        return "\f";
                    case "t":
                        return "\t";
                    case "v":
                        return "\v";
                    case "b":
                        return "\b";
                    case "\\":
                        return "\\";
                    default:
                        int charNum = Convert.ToInt32( payload.Substring( 1 ), 16 );
                        return Convert.ToChar( charNum ).ToString();
                }
            } );
        }

        private void LoadContext( XElement element, Language language )
        {
            string? contextId = element.Attribute( "id" )?.Value;
            if( contextId == null )
            {
                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing attribute 'id' in '{element.Name}' XML element" );
            }

            GetContext( contextId ).Load( element, language );
        }

        private Localizer GetContext( string contextId )
        {
            return GetContext( contextId.Split( '.' ) );
        }

        private Localizer GetContext( IEnumerable<string> splitContextIds )
        {
            return GetContext( splitContextIds.GetEnumerator() );
        }

        private Localizer GetContext( IEnumerator<string> splitContextIds )
        {
            if( splitContextIds.MoveNext() )
            {
                string leftContextId = splitContextIds.Current.Trim();

                if( !m_nestedContexts.TryGetValue( leftContextId, out var localizer ) )
                {
                    localizer = new Localizer( this );
                    m_nestedContexts.Add( leftContextId, localizer );
                }

                return localizer.GetContext( splitContextIds );
            }
            else
            {
                return this;
            }
        }

        private void Clear()
        {
            m_localizations.Clear();

            foreach( var context in m_nestedContexts.Values )
            {
                context.Clear();
            }
        }

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private readonly Localizer? m_parentContext = null;

        private readonly Dictionary<string, string> m_localizations = new();
        private readonly Dictionary<string, Localizer> m_nestedContexts = new();
    }
}
