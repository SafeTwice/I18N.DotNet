/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace I18N.Tool
{
    public class OutputFile
    {
        public OutputFile( string filepath )
        {
            m_doc = null;

            if( File.Exists( filepath ) )
            {
                try
                {
                    m_doc = XDocument.Load( filepath, LoadOptions.None );
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

            if( m_doc != null )
            {
                if( m_doc.Root.Name != ROOT_TAG )
                {
                    throw new ApplicationException( "Invalid XML root element in existing output file" );
                }

            }
            else
            {
                m_doc = new XDocument( new XElement( ROOT_TAG ) );
            }

            AnnotatePreexistingEntries( m_doc.Root );
        }

        public void DeleteFoundingComments()
        {
            DeleteFoundingComments( m_doc.Root );
        }

        public void CreateEntries( Context rootContext )
        {
            CreateEntries( m_doc.Root, rootContext );
        }

        public void CreateDeprecationComments()
        {
            CreateDeprecationComments( m_doc.Root );
        }

        public void WriteToFile( string filepath )
        {
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;

            using( XmlWriter xw = XmlWriter.Create( filepath, xws ) )
            {
                m_doc.WriteTo( xw );
            }
        }

        private static void CreateEntries( XElement parentElement, Context context )
        {
            foreach( var key in context.KeyMatches.Keys )
            {
                var entryElement = GetEntryElement( parentElement, key );

                foreach( var keyInfo in context.KeyMatches[ key ] )
                {
                    AddCommentIfNeeded( entryElement, $" Found in: {keyInfo} ", true );
                }
            }

            foreach( var nestedContext in context.NestedContexts )
            {
                var nestedContextElement = GetContextElement( parentElement, nestedContext.Key );

                CreateEntries( nestedContextElement, nestedContext.Value );
            }
        }

        private static void DeleteFoundingComments( XElement element )
        {
            List<XComment> commentsToRemove = new List<XComment>();

            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                foreach( var node in entryElement.Nodes() )
                {
                    var commentNode = node as XComment;
                    if( ( commentNode != null ) && commentNode.Value.StartsWith( " Found in:" ) )
                    {
                        commentsToRemove.Add( commentNode ); ;
                    }
                }
            }

            commentsToRemove.ForEach( xc => xc.Remove() );

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                DeleteFoundingComments( contextElement );
            }
        }

        private static XElement GetEntryElement( XElement parentElement, string key )
        {
            XElement lastEntryElement = null;

            foreach( var entryElement in parentElement.Elements( ENTRY_TAG ) )
            {
                var keyElement = entryElement.Element( KEY_TAG );
                if( ( keyElement != null ) && ( keyElement.Value == key ) ) 
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
                if( ( nameAttr != null ) && ( nameAttr.Value == contextName ) )
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
                entryElement.Element( KEY_TAG ).AddBeforeSelf( new XComment( comment ) );
            }
            else
            {
                entryElement.AddFirst( new XComment( comment ) );
            }
        }

        private class PreexistingEntryAnnotation
        {
        }

        private static PreexistingEntryAnnotation PREEXISTING_ENTRY_ANNOTATION = new PreexistingEntryAnnotation();

        private static void AnnotatePreexistingEntries( XElement element )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                entryElement.AddAnnotation( PREEXISTING_ENTRY_ANNOTATION );
            }

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                AnnotatePreexistingEntries( contextElement );
            }
        }

        private static void CreateDeprecationComments( XElement element )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                if( entryElement.Annotation<PreexistingEntryAnnotation>() != null )
                {
                    AddCommentIfNeeded( entryElement, " DEPRECATED ", false );
                }
            }

            foreach( var contextElement in element.Elements( CONTEXT_TAG ) )
            {
                CreateDeprecationComments( contextElement );
            }
        }

        private const string ROOT_TAG = "I18N";
        private const string ENTRY_TAG = "Entry";
        private const string KEY_TAG = "Key";
        private const string CONTEXT_TAG = "Context";
        private const string CONTEXT_ID_ATTR = "id";

        private XDocument m_doc;
    }
}
