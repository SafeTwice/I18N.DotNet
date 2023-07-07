/// @file
/// @copyright  Copyright (c) 2022-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace I18N.DotNet
{
    /// <summary>
    /// Converter of strings from a language-neutral value to its corresponding language-specific localization.
    /// </summary>
    public class Localizer : ILocalizer
    {
        //===========================================================================
        //                          PUBLIC NESTED TYPES
        //===========================================================================

        /// <summary>
        /// Exception thrown when a localization file cannot be parsed properly. 
        /// </summary>
        public class ParseException : ApplicationException
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="message">A message that describes the error.</param>
            public ParseException( string message ) : base( message ) { }
        }

        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Localizer()
        {
        }

        /*===========================================================================
         *                            PUBLIC METHODS
         *===========================================================================*/

        /// <inheritdoc/>
        public string Localize( PlainString text )
        {
            string localizedText;
            if( m_localizations.TryGetValue( text.Value, out localizedText ) )
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
        public string LocalizeFormat( string format, params object[] args )
        {
            string localizedFormat;
            if( m_localizations.TryGetValue( format, out localizedFormat ) )
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

        /// <summary>
        /// Gets the localizer for a context in the current localizer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Contexts are used to disambiguate the conversion of the same language-neutral string to different
        /// language-specific strings depending on the context where the conversion is performed.
        /// </para>
        /// <para>
        /// Contexts can be nested.The context identifier can identify a chain of nested contexts by separating
        /// their identifiers with the '.' character (left = outermost / right = innermost ).
        /// </para>
        /// </remarks>
        /// <param name="contextId">Identifier of the context</param>
        /// <returns>Localizer for the given context</returns>
        public Localizer Context( string contextId )
        {
            var contextEnumerator = ((IEnumerable<string>) contextId.Split( '.' )).GetEnumerator();
            contextEnumerator.MoveNext();
            return Context( contextEnumerator );
        }

        /// <summary>
        /// Gets the localizer for a context in the current localizer.
        /// </summary>
        /// <remarks>
        /// Contexts are used to disambiguate the conversion of the same language-neutral string to different
        /// language-specific strings depending on the context where the conversion is performed.
        /// </remarks>
        /// <param name="splitContextIds">Chain of context identifiers in split form</param>
        /// <returns>Localizer for the given context</returns>
        public Localizer Context( IEnumerator<string> splitContextIds )
        {
            string leftContextId = splitContextIds.Current.Trim();

            Localizer localizer;
            if( !m_nestedContexts.TryGetValue( leftContextId, out localizer ) )
            {
                localizer = new Localizer( this, m_targetLanguageFull, m_targetLanguagePrimary );
                m_nestedContexts.Add( leftContextId, localizer );
            }

            if( splitContextIds.MoveNext() )
            {
                return localizer.Context( splitContextIds );
            }
            else
            {
                return localizer;
            }
        }

        /// <summary>
        /// Sets the localized language to which conversion will be performed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Language matching is case-insensitive.
        /// </para>
        /// <para>
        /// Any arbitrary string can be used for identifying languages, but when using language identifiers formed
        /// by a primary code and a variant code separated by an hyphen( e.g., "en-us") if a localized conversion
        /// for the "full" language is not found then a conversion for the primary(base) language is tried too.
        /// </para>
        /// </remarks>
        /// <param name="language">Name, code or identifier for the language</param>
        /// <returns></returns>
        public Localizer SetTargetLanguage( string language )
        {
            m_targetLanguageFull = language.ToLower();

            var splitLanguage = m_targetLanguageFull.Split( new char[] { '-' }, 2 );
            if( splitLanguage.Length > 1 )
            {
                m_targetLanguagePrimary = splitLanguage[0];
            }
            else
            {
                m_targetLanguagePrimary = null;
            }

            return this;
        }

        /// <summary>
        /// Loads a localization configuration from a file in XML format.
        /// </summary>
        /// <remarks>
        /// Precondition: The language must be set before calling this method.
        /// </remarks>
        /// <param name="filepath" > Path to the localization configuration file in XML format</param>
        /// <param name="merge"> Replaces the current localization mapping with the loaded one when<c>false</c>,
        ///                      otherwise merges both (existing mappings are overridden with loaded ones ).</param>
        /// <exception cref="InvalidOperationException">Thrown when the language is not set.</exception>
        /// <exception cref="ParseException">Thrown when the input file cannot be parsed properly.</exception>
        public void LoadXML( string filepath, bool merge = true )
        {
            LoadXML( XDocument.Load( filepath, LoadOptions.SetLineInfo ), merge );
        }

        /// <summary>
        /// Loads a localization configuration from a stream in XML format.
        /// </summary>
        /// <remarks>
        /// Precondition: The language must be set before calling this method.
        /// </remarks>
        /// <param name="stream">Stream with the localization configuration in XML format</param>
        /// <param name="merge"> Replaces the current localization mapping with the loaded one when<c>false</c>,
        ///                      otherwise merges both (existing mappings are overridden with loaded ones ).</param>
        /// <exception cref="InvalidOperationException">Thrown when the language is not set.</exception>
        /// <exception cref="ParseException">Thrown when the input file cannot be parsed properly.</exception>
        public void LoadXML( Stream stream , bool merge = true )
        {
            LoadXML( XDocument.Load( stream, LoadOptions.SetLineInfo ), merge );
        }

        /// <summary>
        /// Loads a localization configuration from a XML document.
        /// </summary>
        /// <remarks>
        /// Precondition: The language must be set before calling this method.
        /// </remarks>
        /// <param name="doc">XML document with the localization configuration</param>
        /// <param name="merge"> Replaces the current localization mapping with the loaded one when<c>false</c>,
        ///                       otherwise merges both (existing mappings are overridden with loaded ones ).</param>
        /// <exception cref="InvalidOperationException">Thrown when the language is not set.</exception>
        /// <exception cref="ParseException">Thrown when the input file cannot be parsed properly.</exception>
        public void LoadXML( XDocument doc, bool merge = true )
        {
            if( m_targetLanguageFull == null )
            {
                throw new InvalidOperationException( "Language must be set before loading localization files" );
            }

            if( !merge )
            {
                m_localizations.Clear();
                m_nestedContexts.Clear();
            }

            XElement rootElement = doc.Root;

            if( rootElement.Name != "I18N" )
            {
                throw new ParseException( $"Line {( (IXmlLineInfo) rootElement ).LineNumber}: Invalid XML root element" );
            }

            Load( rootElement );
        }

        //===========================================================================
        //                          PRIVATE CONSTRUCTORS
        //===========================================================================

        private Localizer( Localizer parent, string targetLanguageFull, string targetLanguagePrimary )
        {
            m_parentContext = parent;

            m_targetLanguageFull = targetLanguageFull;
            m_targetLanguagePrimary = targetLanguagePrimary;
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
        //                            PRIVATE METHODS
        //===========================================================================

        private void Load( XElement element )
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
                        throw new ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Invalid XML element" );
                }
            }
        }

        private void LoadEntry( XElement element )
        {
            string key = null;
            string valueFull = null;
            string valuePrimary = null;

            foreach( var childElement in element.Elements() )
            {
                switch( childElement.Name.ToString() )
                {
                    case "Key":
                        if( key != null )
                        {
                            throw new ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Too many child '{childElement.Name}' XML elements" );
                        }
                        key = UnescapeEscapeCodes( childElement.Value );
                        break;

                    case "Value":
                        string loadedValue;
                        var loadedValueType = LoadValue( childElement, out loadedValue );
                        if( loadedValueType == EValueType.FULL )
                        {
                            if( valueFull != null )
                            {
                                throw new ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Too many child '{childElement.Name}' XML elements with the same 'lang' attribute" );
                            }
                            valueFull = loadedValue;
                        }
                        else if( loadedValueType == EValueType.PRIMARY )
                        {
                            if( valuePrimary != null )
                            {
                                throw new ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Too many child '{childElement.Name}' XML elements with the same 'lang' attribute" );
                            }
                            valuePrimary = loadedValue;
                        }
                        break;

                    default:
                        throw new ParseException( $"Line {( (IXmlLineInfo) childElement ).LineNumber}: Invalid XML element" );
                }
            }

            if( key == null )
            {
                throw new ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing child 'Key' XML element" );
            }

            string value = ( valueFull ?? valuePrimary );

            if( value != null )
            {
                m_localizations[key] = value;
            }
        }

        private EValueType LoadValue( XElement element, out string value )
        {
            string lang = element.Attribute( "lang" )?.Value.ToLower();
            if( lang == null )
            {
                throw new ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing attribute 'lang' in '{element.Name}' XML element" );
            }

            if( lang == m_targetLanguageFull )
            {
                value = UnescapeEscapeCodes( element.Value );
                return EValueType.FULL;
            }
            else if( lang == m_targetLanguagePrimary )
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
                var payload = m.Groups[1].Value;
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

        private void LoadContext( XElement element )
        {
            string contextId = element.Attribute( "id" )?.Value;
            if( contextId == null )
            {
                throw new ParseException( $"Line {( (IXmlLineInfo) element ).LineNumber}: Missing attribute 'id' in '{element.Name}' XML element" );
            }

            Context( contextId ).Load( element );
        }

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private Localizer m_parentContext = null;
        private string m_targetLanguageFull = null;
        private string m_targetLanguagePrimary = null;
        private Dictionary<string, string> m_localizations = new Dictionary<string, string>();
        private Dictionary<string, Localizer> m_nestedContexts = new Dictionary<string, Localizer>();
    }
}
