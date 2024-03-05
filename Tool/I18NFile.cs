/// @file
/// @copyright  Copyright (c) 2022-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace I18N.DotNet.Tool
{
    public class I18NFile : II18NFile
    {
        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        public void LoadFromFile( string filepath )
        {
            m_doc = null;

            if( File.Exists( filepath ) )
            {
                try
                {
                    m_doc = XDocument.Load( filepath, LoadOptions.SetLineInfo );
                }
                catch( Exception )
                {
                    var text = File.ReadAllText( filepath );

                    if( text.Trim().Length > 0 )
                    {
                        throw new ApplicationException( "Invalid XML format in existing output file" );
                    }
                }
            }

            if( m_doc == null )
            {
                m_doc = new XDocument( new XElement( ROOT_TAG ) );
            }
            else if( m_doc.Root!.Name != ROOT_TAG )
            {
                throw new ApplicationException( "Invalid XML root element in existing output file" );
            }

            PreparePreexistingEntries( Root );
        }

        public void DeleteFoundingComments()
        {
            DeleteFoundingComments( Root );
        }

        public void PrepareForDeployment()
        {
            PrepareForDeployment( Doc );
        }

        public void CreateEntries( Context rootContext, bool reportLines )
        {
            CreateEntries( Root, rootContext, reportLines );
        }

        public void CreateDeprecationComments()
        {
            CreateDeprecationComments( Root );
        }

        public void WriteToFile( string filepath )
        {
            var xws = new XmlWriterSettings() { Indent = true };

            using XmlWriter xw = XmlWriter.Create( filepath, xws );

            Doc.WriteTo( xw );
        }

        public IEnumerable<(int line, string context, string? key)> GetDeprecatedEntries( IEnumerable<Regex> includeContexts, IEnumerable<Regex> excludeContexts )
        {
            foreach( var element in GetDeprecatedEntries( Root ) )
            {
                var line = ( (IXmlLineInfo) element ).LineNumber;
                var key = element.Element( KEY_TAG )?.Value;
                var context = GetContext( element );
                if( MatchesContexts( context, includeContexts, excludeContexts ) )
                {
                    yield return (line, context, key);
                }
            }
        }

        public IEnumerable<(int line, string context, string? key)> GetNoTranslationEntries( IEnumerable<string> requiredLanguages, IEnumerable<Regex> includeContexts, IEnumerable<Regex> excludeContexts )
        {
            foreach( var element in GetNoTranslationEntries( Root, requiredLanguages ) )
            {
                var line = ( (IXmlLineInfo) element ).LineNumber;
                var key = element.Element( KEY_TAG )?.Value;
                var context = GetContext( element );
                if( MatchesContexts( context, includeContexts, excludeContexts ) )
                {
                    yield return (line, context, key);
                }
            }
        }

        public IEnumerable<(int line, string message, bool isError)> GetFileIssues()
        {
            foreach( var issueInfo in GetFileIssues( Root ) )
            {
                var line = ( (IXmlLineInfo) issueInfo.element ).LineNumber;
                yield return (line, issueInfo.message, issueInfo.isError);
            }
        }

        //===========================================================================
        //                          PRIVATE NESTED TYPES
        //===========================================================================

        private class PreexistingEntryAnnotation
        {
        }

        //===========================================================================
        //                           PRIVATE PROPERTIES
        //===========================================================================

        private XDocument Doc => m_doc ?? throw new InvalidOperationException( "Not initialized" );

        private XElement Root => Doc.Root!;

        //===========================================================================
        //                            PRIVATE METHODS
        //===========================================================================

        private static bool MatchesContexts( string context, IEnumerable<Regex> includeContexts, IEnumerable<Regex> excludeContexts )
        {
            bool includeMatch = !includeContexts.Any();

            foreach( var includeContext in includeContexts )
            {
                if( includeContext.IsMatch( context ) )
                {
                    includeMatch = true;
                    break;
                }
            }

            if( !includeMatch )
            {
                return false;
            }

            foreach( var excludeMatch in excludeContexts )
            {
                if( excludeMatch.IsMatch( context ) )
                {
                    return false;
                }
            }

            return true;
        }

        private static void CreateEntries( XElement parentElement, Context context, bool reportLines )
        {
            foreach( var key in context.KeyMatches.Keys )
            {
                var entryElement = GetEntryElement( parentElement, key );

                foreach( var keyInfo in context.KeyMatches[ key ] )
                {
                    var comment = $"{FOUNDING_HEADING} {keyInfo.File} ";
                    if( reportLines )
                    {
                        comment += $"@ {keyInfo.Line} ";
                    }
                    AddCommentIfNeeded( entryElement, comment, true );
                }
            }

            foreach( var nestedContext in context.NestedContexts )
            {
                var nestedContextElement = GetContextElement( parentElement, nestedContext.Key );

                CreateEntries( nestedContextElement, nestedContext.Value, reportLines );
            }
        }

        private static void DeleteFoundingComments( XElement element )
        {
            var commentsToRemove = new List<XComment>();

            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                foreach( var node in entryElement.Nodes() )
                {
                    if( ( node is XComment comment ) && ( comment.Value.StartsWith( FOUNDING_HEADING ) ) )
                    {
                        commentsToRemove.Add( comment );
                    }
                }
            }

            commentsToRemove.ForEach( xc => xc.Remove() );

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                DeleteFoundingComments( contextElement );
            }
        }

        private static void PrepareForDeployment( XContainer container )
        {
            DeleteAllComments( container );
            DeleteEmptyEntries( container );

            foreach( var childNode in container.Nodes() )
            {
                if( childNode is XContainer childContainer )
                {
                    PrepareForDeployment( childContainer );
                }
            }

            DeleteEmptyContexts( container );
        }

        private static void DeleteAllComments( XContainer container )
        {
            var commentsToRemove = new List<XComment>();

            foreach( var childNode in container.Nodes() )
            {
                if( childNode is XComment comment )
                {
                    commentsToRemove.Add( comment );
                }
            }

            commentsToRemove.ForEach( xc => xc.Remove() );
        }

        private static void DeleteEmptyEntries( XContainer container )
        {
            var elementsToRemove = new List<XElement>();

            foreach( var childNode in container.Nodes() )
            {
                if( childNode is XElement element )
                {
                    if( element.Name == ENTRY_TAG )
                    {
                        if( !element.Elements( VALUE_TAG ).Any() )
                        {
                            elementsToRemove.Add( element );
                        }
                        else
                        {
                            element.Element( KEY_TAG )?.Attribute( LANG_ATTR )?.Remove();
                        }
                    }
                }
            }

            elementsToRemove.ForEach( xc => xc.Remove() );
        }

        private static void DeleteEmptyContexts( XContainer container )
        {
            var elementsToRemove = new List<XElement>();

            foreach( var childNode in container.Nodes() )
            {
                if( childNode is XElement element )
                {
                    if( element.Name == CONTEXT_TAG )
                    {
                        if( !element.Elements().Any() )
                        {
                            elementsToRemove.Add( element );
                        }
                    }
                }
            }

            elementsToRemove.ForEach( xc => xc.Remove() );
        }

        private static void DeleteDeprecatedComments( XElement element )
        {
            var commentsToRemove = new List<XComment>();

            foreach( var node in element.Nodes() )
            {
                var commentNode = node as XComment;
                if( commentNode?.Value == DEPRECATED_COMMENT )
                {
                    commentsToRemove.Add( commentNode );
                }
            }

            commentsToRemove.ForEach( xc => xc.Remove() );
        }

        private static XElement GetEntryElement( XElement parentElement, string key )
        {
            XElement? lastEntryElement = null;

            foreach( var entryElement in parentElement.Elements( ENTRY_TAG ) )
            {
                var keyElement = entryElement.Element( KEY_TAG );
                if( keyElement?.Value == key )
                {
                    entryElement.RemoveAnnotations<PreexistingEntryAnnotation>();
                    return entryElement;
                }

                lastEntryElement = entryElement;
            }

            var newEntryElement = new XElement( ENTRY_TAG );
            newEntryElement.Add( new XElement( KEY_TAG, key ) );

            if( lastEntryElement != null )
            {
                lastEntryElement.AddAfterSelf( newEntryElement );
            }
            else
            {
                parentElement.AddFirst( newEntryElement );
            }

            return newEntryElement;
        }

        private static XElement GetContextElement( XElement parentElement, string contextName )
        {
            foreach( var contextElement in parentElement.Elements( CONTEXT_TAG ) )
            {
                var nameAttr = contextElement.Attribute( CONTEXT_ID_ATTR );
                if( nameAttr?.Value == contextName )
                {
                    return contextElement;
                }
            }

            var newContextElement = new XElement( CONTEXT_TAG );
            newContextElement.Add( new XAttribute( CONTEXT_ID_ATTR, contextName ) );
            parentElement.Add( newContextElement );

            return newContextElement;
        }

        private static void AddCommentIfNeeded( XElement entryElement, string comment, bool beforeKey )
        {
            foreach( var node in entryElement.Nodes() )
            {
                var commentNode = node as XComment;
                if( commentNode?.Value == comment )
                {
                    return;
                }
            }

            if( beforeKey )
            {
                entryElement.Element( KEY_TAG )!.AddBeforeSelf( new XComment( comment ) );
            }
            else
            {
                entryElement.AddFirst( new XComment( comment ) );
            }
        }

        private static void PreparePreexistingEntries( XElement element )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                entryElement.AddAnnotation( PREEXISTING_ENTRY_ANNOTATION );
            }

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                PreparePreexistingEntries( contextElement );
            }
        }

        private static void CreateDeprecationComments( XElement element )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                DeleteDeprecatedComments( entryElement );

                if( entryElement.Annotation<PreexistingEntryAnnotation>() != null )
                {
                    AddCommentIfNeeded( entryElement, DEPRECATED_COMMENT, false );
                }
            }

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                CreateDeprecationComments( contextElement );
            }
        }

        private static IEnumerable<XElement> GetDeprecatedEntries( XElement element )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                bool deprecated = false;
                bool hasFoundings = false;

                foreach( var node in entryElement.Nodes() )
                {
                    if( node is XComment commentNode )
                    {
                        var commentValue = commentNode.Value;
                        if( commentValue == DEPRECATED_COMMENT )
                        {
                            deprecated = true;
                            break;
                        }
                        else if( commentValue.StartsWith( FOUNDING_HEADING ) )
                        {
                            hasFoundings = true;
                        }
                    }
                }

                if( deprecated || !hasFoundings )
                {
                    yield return entryElement;
                }
            }

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                foreach( var childEntry in GetDeprecatedEntries( contextElement ) )
                {
                    yield return childEntry;
                }
            }
        }

        private static IEnumerable<XElement> GetNoTranslationEntries( XElement element, IEnumerable<string> requiredLanguages )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                var keyElement = entryElement.Element( KEY_TAG );

                var omittedLanguagesValue = keyElement?.Attribute( LANG_ATTR )?.Value.Trim() ?? string.Empty;

                if( omittedLanguagesValue == "*" )
                {
                    continue;
                }

                if( !requiredLanguages.Any() )
                {
                    if( omittedLanguagesValue.Length > 0 )
                    {
                        continue;
                    }

                    if( !entryElement.Elements( VALUE_TAG ).Any( value => ( value.Attribute( LANG_ATTR )?.Value?.Length ?? 0 ) > 0 ) )
                    {
                        yield return entryElement;
                    }
                }
                else
                {
                    var omittedLanguages = omittedLanguagesValue.Split( ',' ).Select( v => v.Trim() );

                    var expectedLanguages = requiredLanguages.Except( omittedLanguages ).ToList();

                    if( expectedLanguages.Any() )
                    {
                        foreach( var valueElement in entryElement.Elements( VALUE_TAG ) )
                        {
                            var valueLanguage = valueElement.Attribute( LANG_ATTR )?.Value;
                            if( valueLanguage != null )
                            {
                                if( expectedLanguages.Contains( valueLanguage ) )
                                {
                                    expectedLanguages.Remove( valueLanguage );
                                }

                                if( !expectedLanguages.Any() )
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if( expectedLanguages.Any() )
                    {
                        yield return entryElement;
                    }
                }
            }

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                foreach( var childEntry in GetNoTranslationEntries( contextElement, requiredLanguages ) )
                {
                    yield return childEntry;
                }
            }
        }

        private static IEnumerable<(XElement element, string message, bool isError)> GetFileIssues( XElement element )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                var keyElements = entryElement.Elements( KEY_TAG );

                var keyElementsCount = keyElements.Count();

                if( keyElementsCount == 0 )
                {
                    yield return (entryElement, $"'{ENTRY_TAG}' element does not have a '{KEY_TAG}' element", true);
                }
                else if( keyElementsCount > 1 )
                {
                    yield return (entryElement, $"'{ENTRY_TAG}' element has more than one '{KEY_TAG}' element", true);
                }

                foreach( var keyElement in keyElements )
                {
                    var key = keyElement.Value;

                    if( key.Length == 0 )
                    {
                        yield return (keyElement, $"'{KEY_TAG}' element is empty", true);
                    }
                }

                var languages = new Dictionary<string, int>();

                var valueElements = entryElement.Elements( VALUE_TAG );

                foreach( var valueElement in valueElements )
                {
                    var language = valueElement.Attribute( LANG_ATTR )?.Value;

                    if( language == null )
                    {
                        yield return (valueElement, $"'{VALUE_TAG}' element attribute '{LANG_ATTR}' is missing", true);
                    }
                    else if( language.Length == 0 )
                    {
                        yield return (valueElement, $"'{VALUE_TAG}' element attribute '{LANG_ATTR}' is empty", true);
                    }
                    else if( languages.ContainsKey( language ) )
                    {
                        yield return (valueElement, $"Translation for language '{language}' has already been defined at line {languages[ language ]}", false);
                    }
                    else
                    {
                        languages.Add( language, ( (IXmlLineInfo) valueElement ).LineNumber );
                    }

                    var value = valueElement.Value;

                    if( value.Length == 0 )
                    {
                        yield return (valueElement, $"'{VALUE_TAG}' element is empty", false);
                    }
                }
            }

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                var id = contextElement.Attribute( CONTEXT_ID_ATTR )?.Value;

                if( id == null )
                {
                    yield return (contextElement, $"'{CONTEXT_TAG}' element attribute '{CONTEXT_ID_ATTR}' is missing", true);
                }
                else if( id.Length == 0 )
                {
                    yield return (contextElement, $"'{CONTEXT_TAG}' element attribute '{CONTEXT_ID_ATTR}' is empty", true);
                }

                foreach( var contextIssue in GetFileIssues( contextElement ) )
                {
                    yield return contextIssue;
                }
            }
        }

        private static string GetContext( XElement element )
        {
            string context = "/";
            var currentElement = element;

            while( currentElement != null )
            {
                if( currentElement.Name == CONTEXT_TAG )
                {
                    context = $"/{currentElement.Attribute( CONTEXT_ID_ATTR )?.Value}{context}";
                }

                currentElement = currentElement.Parent;
            }

            return context;
        }

        //===========================================================================
        //                           PRIVATE CONSTANTS
        //===========================================================================

        private static readonly PreexistingEntryAnnotation PREEXISTING_ENTRY_ANNOTATION = new();

        private const string ROOT_TAG = "I18N";
        private const string ENTRY_TAG = "Entry";
        private const string KEY_TAG = "Key";
        private const string VALUE_TAG = "Value";
        private const string CONTEXT_TAG = "Context";
        private const string CONTEXT_ID_ATTR = "id";
        private const string LANG_ATTR = "lang";

        private const string DEPRECATED_COMMENT = " DEPRECATED ";

        private const string FOUNDING_HEADING = " Found in:";

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private XDocument? m_doc;
    }
}
