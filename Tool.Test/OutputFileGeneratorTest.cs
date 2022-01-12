/**
 * @file
 * @copyright  Copyright (c) 2020 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;

namespace I18N.Tool.Test
{
    public class OutputFileGeneratorTest : IDisposable
    {
        private string m_tempFile;

        public OutputFileGeneratorTest()
        {
            m_tempFile = Path.GetTempPath() + "\\Temp.xml";
        }

        public void Dispose()
        {
            File.Delete( m_tempFile );
        }

        private void WriteTempFile( string contents )
        {
            File.WriteAllText( m_tempFile, contents );
        }

        private XDocument ReadTempFileAsXML()
        {
            return XDocument.Load( m_tempFile );
        }

        private void CreateInitialOutputFile()
        {
            string contents =
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 -->\n" +
                "    <!-- Found in: Match 2 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "  </Entry>\n" +
                "</I18N>";
            WriteTempFile( contents );
        }

        private Dictionary<string, List<string>> CreateKeyMatches()
        {
            Dictionary<string, List<string>> keyMatches = new Dictionary<string, List<string>>();

            keyMatches.Add( "Key 1", new List<string> { "Match 1", "Match A" } );
            keyMatches.Add( "Key A", new List<string> { "Match B" } );

            return keyMatches;
        }

        private void Check_GenerateFile_New()
        {
            // Prepare

            var keyMatches = CreateKeyMatches();

            // Execute

            OutputFileGenerator.GenerateFile( m_tempFile, false, keyMatches );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            var keys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 2, keys.Count() );
            Assert.All( keys, element => Assert.Matches( "^Key [1A]$", element.Value ) );

            var key1Comments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key 1']/comment()" ) );
            Assert.Equal( 2, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match [1A] $", comment.Value ) );

            var keyAComments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key A']/comment()" ) );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match B $", comment.Value ) );
        }

        [Fact]
        public void GenerateFile_New()
        {
            // Prepare

            File.Delete( m_tempFile );

            // Execute & Verify

            Check_GenerateFile_New();
        }

        [Fact]
        public void GenerateFile_Update_PreserveFoundingComments()
        {
            // Prepare

            var keyMatches = CreateKeyMatches();

            CreateInitialOutputFile();

            // Execute

            OutputFileGenerator.GenerateFile( m_tempFile, true, keyMatches );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            var keys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 3, keys.Count() );
            Assert.All( keys, element => Assert.Matches( "^Key [12A]$", element.Value ) );

            var key1Comments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key 1']/comment()" ) );
            Assert.Equal( 3, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match [12A] $", comment.Value ) );

            var key2Comments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key 2']/comment()" ) );
            Assert.Single( key2Comments );
            Assert.All( key2Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var keyAComments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key A']/comment()" ) );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match B $", comment.Value ) );
        }

        [Fact]
        public void GenerateFile_Update_DeleteFoundingComments()
        {
            // Prepare

            var keyMatches = CreateKeyMatches();

            CreateInitialOutputFile();

            // Execute

            OutputFileGenerator.GenerateFile( m_tempFile, false, keyMatches );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            var keys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 3, keys.Count() );
            Assert.All( keys, element => Assert.Matches( "^Key [12A]$", element.Value ) );

            var key1Comments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key 1']/comment()" ) );
            Assert.Equal( 2, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match [1A] $", comment.Value ) );

            var key2Comments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key 2']/comment()" ) );
            Assert.Single( key2Comments );
            Assert.All( key2Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var keyAComments = Enumerable.Cast<XComment>( (IEnumerable<object>) doc.XPathEvaluate( "I18N/Entry[Key/text()='Key A']/comment()" ) );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match B $", comment.Value ) );
        }

        [Fact]
        public void GenerateFile_InitialFile_WrongRoot()
        {
            // Prepare

            var keyMatches = CreateKeyMatches();

            WriteTempFile( "<L10N></L10N>" );

            // Execute & Verify

            var exception = Assert.Throws<ApplicationException>( () => OutputFileGenerator.GenerateFile( m_tempFile, false, keyMatches ) );

            Assert.Contains( "Invalid XML root element in existing output file", exception.Message );
        }

        [Fact]
        public void GenerateFile_InitialFile_InvalidXML()
        {
            // Prepare

            var keyMatches = CreateKeyMatches();

            WriteTempFile( "Whatever" );

            // Execute & Verify

            var exception = Assert.Throws<ApplicationException>( () => OutputFileGenerator.GenerateFile( m_tempFile, false, keyMatches ) );

            Assert.Contains( "Invalid XML format in existing output file", exception.Message );
        }

        [Fact]
        public void GenerateFile_InitialFile_Blank()
        {
            // Prepare

            WriteTempFile( "" );

            // Execute & Verify

            Check_GenerateFile_New();
        }

        [Fact]
        public void GenerateFile_InitialFile_OnlyWhiteSpace()
        {
            // Prepare

            WriteTempFile( " \n" );

            // Execute & Verify

            Check_GenerateFile_New();
        }
    }
}
