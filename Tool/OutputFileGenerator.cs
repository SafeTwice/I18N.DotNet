using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace I18N.Tool
{
    static class OutputFileGenerator
    {
        public static void GenerateFile( string filepath, bool preserveFoundingComments, Dictionary<string, List<string>> keyMatches )
        {
            XDocument doc = GetDocument( filepath, preserveFoundingComments );

            XElement root = doc.Root;            

            foreach( var key in keyMatches.Keys )
            {
                var entryElement = GetEntry( root, key );
                if( entryElement == null )
                {
                    entryElement = new XElement( ENTRY_TAG );
                    entryElement.Add( new XElement( KEY_TAG, key ) );
                    root.Add( entryElement );
                }

                foreach( var keyInfo in keyMatches[key] )
                {
                    AddCommentIfNeeded( entryElement, $" Found in: {keyInfo} " );
                }
            }

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;

            using( XmlWriter xw = XmlWriter.Create( filepath, xws ) )
            {
                doc.WriteTo( xw );
            }
        }

        private static XDocument GetDocument( string filepath, bool preserveFoundingComments )
        {
            XDocument doc = null;

            if( File.Exists( filepath ) )
            {
                try
                {
                    doc = XDocument.Load( filepath, LoadOptions.None );
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
            
            if( doc != null )
            {
                if( doc.Root.Name != ROOT_TAG )
                {
                    throw new ApplicationException( "Invalid XML root element in existing output file" );
                }

                if( !preserveFoundingComments )
                {
                    DeleteFoundingComments( doc.Root );
                }
            }
            else
            {
                doc = new XDocument( new XElement( ROOT_TAG ) );
            }

            return doc;
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
        }

        private static XElement GetEntry( XElement element, string key )
        {
            foreach( var entryElement in element.Elements( ENTRY_TAG ) )
            {
                var keyElement = entryElement.Element( KEY_TAG );
                if( keyElement.Value == key )
                {
                    return entryElement;
                }
            }

            return null;
        }

        private static void AddCommentIfNeeded( XElement entryElement, string comment )
        {
            foreach( var node in entryElement.Nodes() )
            {
                var commentNode = node as XComment;
                if( ( commentNode != null ) && commentNode.Value == comment )
                {
                    return;
                }
            }

            entryElement.Element( KEY_TAG ).AddBeforeSelf( new XComment( comment ) );
        }

        private const string ROOT_TAG = "I18N";
        private const string ENTRY_TAG = "Entry";
        private const string KEY_TAG = "Key";
    }
}
