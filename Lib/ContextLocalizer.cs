/// @file
/// @copyright  Copyright (c) 2020-2024 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace I18N.DotNet
{
    /// <summary>
    /// Localizer that can provide translations and can store nested contexts.
    /// </summary>
    public class ContextLocalizer : ILocalizer
    {
        //===========================================================================
        //                           PUBLIC PROPERTIES
        //===========================================================================

        /// <inheritdoc/>
        public string TargetLanguage => Language.Full;

        /// <inheritdoc/>
        public CultureInfo TargetCulture => Language.Culture;

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
                return String.Format( Language.Culture, localizedFormat, args );
            }
            else if( m_parentContext != null )
            {
                return m_parentContext.LocalizeFormat( format, args );
            }
            else
            {
                return String.Format( Language.Culture, format, args );
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
        public ContextLocalizer Context( string contextId )
        {
            return GetContext( contextId );
        }

        /// <inheritdoc cref="ILocalizer.Context(IEnumerable{string})"/>
        public ContextLocalizer Context( IEnumerable<string> splitContextIds )
        {
            return GetContext( splitContextIds );
        }

        //===========================================================================
        //                           PROTECTED PROPERTIES
        //===========================================================================

        private protected Language Language
        {
            get
            {
                m_language ??= new Language( CultureInfo.CurrentUICulture );
                return m_language;
            }

            set
            {
                m_language = value;
            }
        }

        //===========================================================================
        //                          PROTECTED CONSTRUCTORS
        //===========================================================================

        private protected ContextLocalizer()
        {
        }

        //===========================================================================
        //                            PROTECTED METHODS
        //===========================================================================

        private protected void Clear()
        {
            m_localizations.Clear();

            foreach( var context in m_nestedContexts.Values )
            {
                context.m_language = m_language;
                context.Clear();
            }
        }

        private protected void Load( XElement element )
        {
            foreach( var childElement in element.Elements() )
            {
                switch( childElement.Name.ToString() )
                {
                    case "Entry":
                        LoadEntry( childElement );
                        break;

                    case "Context":
                        LoadContext( childElement );
                        break;

                    default:
                        throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Invalid XML element" );
                }
            }
        }

        //===========================================================================
        //                          PRIVATE NESTED TYPES
        //===========================================================================

        private enum EValueType
        {
            NO_MATCH,
            FULL,
            PRIMARY
        }

        //===========================================================================
        //                          PRIVATE CONSTRUCTORS
        //===========================================================================

        private ContextLocalizer( ContextLocalizer parent )
        {
            m_parentContext = parent;
            m_language = parent.m_language;
        }

        //===========================================================================
        //                            PRIVATE METHODS
        //===========================================================================

        private void LoadEntry( XElement element )
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
                        var loadedValueType = LoadValue( childElement, out loadedValue );
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

        private EValueType LoadValue( XElement element, out string? value )
        {
            string? lang = element.Attribute( "lang" )?.Value.ToLower();
            if( lang == null )
            {
                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing attribute 'lang' in '{element.Name}' XML element" );
            }

            if( lang == Language.Full )
            {
                value = UnescapeEscapeCodes( element.Value );
                return EValueType.FULL;
            }
            else if( lang == Language.Primary )
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
                if( ESCAPE_CODES.TryGetValue( payload, out var escapeCode ) )
                {
                    return escapeCode;
                }
                else
                {
                    int charNum = Convert.ToInt32( payload.Substring( 1 ), 16 );
                    return Convert.ToChar( charNum ).ToString();
                }
            } );
        }

        private void LoadContext( XElement element )
        {
            string? contextId = element.Attribute( "id" )?.Value;
            if( contextId == null )
            {
                throw new ILoadableLocalizer.ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing attribute 'id' in '{element.Name}' XML element" );
            }

            GetContext( contextId ).Load( element );
        }

        private ContextLocalizer GetContext( string contextId )
        {
            return GetContext( contextId.Split( '.' ) );
        }

        private ContextLocalizer GetContext( IEnumerable<string> splitContextIds )
        {
            return GetContext( splitContextIds.GetEnumerator() );
        }

        private ContextLocalizer GetContext( IEnumerator<string> splitContextIds )
        {
            if( splitContextIds.MoveNext() )
            {
                string leftContextId = splitContextIds.Current.Trim();

                if( !m_nestedContexts.TryGetValue( leftContextId, out var nestedContext ) )
                {
                    nestedContext = new ContextLocalizer( this );
                    m_nestedContexts.Add( leftContextId, nestedContext );
                }

                return nestedContext.GetContext( splitContextIds );
            }
            else
            {
                return this;
            }
        }

        //===========================================================================
        //                           PRIVATE CONSTANTS
        //===========================================================================

        private static readonly Dictionary<string, string> ESCAPE_CODES = new Dictionary<string, string>
        {
            { "n", "\n" },
            { "r", "\r" },
            { "f", "\f" },
            { "t", "\t" },
            { "v", "\v" },
            { "b", "\b" },
            { "\\", "\\" }
        };

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private readonly ContextLocalizer? m_parentContext = null;

        private readonly Dictionary<string, string> m_localizations = new();
        private readonly Dictionary<string, ContextLocalizer> m_nestedContexts = new();

        private Language? m_language;
    }
}
