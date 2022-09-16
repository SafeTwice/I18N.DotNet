/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-Z -->\n" +
                "    <Key>Key 1</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3-Z -->\n" +
                "      <!-- Found in: Match 3-1 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9-Z -->\n" +
                "        <Key>Key 9</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";
            WriteTempFile( contents );
        }

        private void CreateInitialOutputFileMalformed()
        {
            string contents =
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-Z -->\n" +
                "    <Key>Key 1</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3-Z -->\n" +
                "      <!-- Found in: Match 3-1 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9-Z -->\n" +
                "        <Key>Key 9</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "  </Entry>\n" +
                "  <Context>\n" +
                "  </Context>\n" +
                "</I18N>";
            WriteTempFile( contents );
        }

        private Context CreateRootContext()
        {
            var rootContext = new Context();

            rootContext.KeyMatches.Add( "Key 1", new List<string> { "Match 1-1", "Match 1-2" } );
            rootContext.KeyMatches.Add( "Key A", new List<string> { "Match A" } );

            var nestedContext1 = new Context();
            rootContext.NestedContexts.Add( "Context 1", nestedContext1 );

            nestedContext1.KeyMatches.Add( "Key 3", new List<string> { "Match 3-1", "Match 3-2" } );

            var nestedContext2 = new Context();
            rootContext.NestedContexts.Add( "Context 2", nestedContext2 );

            var nestedContext22 = new Context();
            nestedContext2.NestedContexts.Add( "Context 22", nestedContext22 );

            nestedContext22.KeyMatches.Add( "Key 4", new List<string> { "Match 4" } );

            return rootContext;
        }

        private void Check_GenerateFile_New()
        {
            // Prepare

            var rootContext = CreateRootContext();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            // Root Context

            var rootKeys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 2, rootKeys.Count() );
            Assert.All( rootKeys, element => Assert.Matches( "^Key [1A]$", element.Value ) );

            var key1Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 1']/comment()" );
            Assert.Equal( 2, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match 1-[12] $", comment.Value ) );

            var keyAComments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key A']/comment()" );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match A $", comment.Value ) );

            var rootContexts = doc.XPathSelectElements( "I18N/Context" );
            Assert.Equal( 2, rootContexts.Count() );
            Assert.All( rootContexts, element => Assert.Matches( "^Context [12]$", element.Attribute( "id" ).Value ) );

            // Context 1

            var context1Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 1']/comment()" );
            Assert.Empty( context1Comments );

            var context1Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Entry/Key" );
            Assert.Single( context1Keys );
            Assert.All( context1Keys, element => Assert.Matches( "^Key 3$", element.Value ) );

            var key3Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Entry[Key/text()='Key 3']/comment()" );
            Assert.Equal( 2, key3Comments.Count() );
            Assert.All( key3Comments, comment => Assert.Matches( "^ Found in: Match 3-[12] $", comment.Value ) );

            var context1Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context" );
            Assert.Empty( context1Contexts );

            // Context 2

            var context2Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/comment()" );
            Assert.Empty( context2Comments );

            var context2Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Entry/Key" );
            Assert.Empty( context2Keys );

            var context2Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context" );
            Assert.Single( context2Contexts );
            Assert.All( context2Contexts, element => Assert.Matches( "^Context 22$", element.Attribute( "id" ).Value ) );

            // Context 22

            var context22Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/Context[@id='Context 22']/comment()" );
            Assert.Empty( context22Comments );

            var context22Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry/Key" );
            Assert.Single( context22Keys );
            Assert.All( context22Keys, element => Assert.Matches( "^Key 4$", element.Value ) );

            var key4Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry[Key/text()='Key 4']/comment()" );
            Assert.Single( key4Comments );
            Assert.All( key4Comments, comment => Assert.Matches( "^ Found in: Match 4 $", comment.Value ) );

            var context22Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Context" );
            Assert.Empty( context22Contexts );
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

            var rootContext = CreateRootContext();

            CreateInitialOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            // Root Context

            var rootKeys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 3, rootKeys.Count() );
            Assert.All( rootKeys, element => Assert.Matches( "^Key [12A]$", element.Value ) );

            var rootComments = doc.XPathSelectComments( "I18N/comment()" );
            Assert.Empty( rootComments );

            var key1Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 1']/comment()" );
            Assert.Equal( 3, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match 1-[12Z] $", comment.Value ) );

            var key2Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 2']/comment()" );
            Assert.Single( key2Comments );
            Assert.All( key2Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var keyAComments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key A']/comment()" );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match A $", comment.Value ) );

            var rootContexts = doc.XPathSelectElements( "I18N/Context" );
            Assert.Equal( 2, rootContexts.Count() );
            Assert.All( rootContexts, element => Assert.Matches( "^Context [12]$", element.Attribute( "id" ).Value ) );

            // Context 1

            var context1Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/comment()" );
            Assert.Single( context1Comments );
            Assert.All( context1Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var context1Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Entry/Key" );
            Assert.Single( context1Keys );
            Assert.All( context1Keys, element => Assert.Matches( "^Key 3$", element.Value ) );

            var key3Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Entry[Key/text()='Key 3']/comment()" );
            Assert.Equal( 3, key3Comments.Count() );
            Assert.All( key3Comments, comment => Assert.Matches( "^ Found in: Match 3-[12Z] $", comment.Value ) );

            var context1Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context" );
            Assert.Single( context1Contexts );
            Assert.All( context1Contexts, element => Assert.Matches( "^Context 11$", element.Attribute( "id" ).Value ) );

            // Context 11

            var context11Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 1']/Context[@id='Context 11']/comment()" );
            Assert.Empty( context11Comments );

            var context11Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry/Key" );
            Assert.Single( context11Keys );
            Assert.All( context11Keys, element => Assert.Matches( "^Key 9$", element.Value ) );

            var key9Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry[Key/text()='Key 9']/comment()" );
            Assert.Single( key9Comments );
            Assert.All( key9Comments, comment => Assert.Matches( "^ Found in: Match 9-Z $", comment.Value ) );

            var context11Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Context" );
            Assert.Empty( context11Contexts );

            // Context 2

            var context2Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/comment()" );
            Assert.Empty( context2Comments );

            var context2Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Entry/Key" );
            Assert.Empty( context2Keys );

            var context2Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context" );
            Assert.Single( context2Contexts );
            Assert.All( context2Contexts, element => Assert.Matches( "^Context 22$", element.Attribute( "id" ).Value ) );

            // Context 22

            var context22Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/Context[@id='Context 22']/comment()" );
            Assert.Empty( context22Comments );

            var context22Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry/Key" );
            Assert.Single( context22Keys );
            Assert.All( context22Keys, element => Assert.Matches( "^Key 4$", element.Value ) );

            var key4Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry[Key/text()='Key 4']/comment()" );
            Assert.Single( key4Comments );
            Assert.All( key4Comments, comment => Assert.Matches( "^ Found in: Match 4 $", comment.Value ) );

            var context22Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Context" );
            Assert.Empty( context22Contexts );
        }

        [Fact]
        public void GenerateFile_Update_DeleteFoundingComments()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateInitialOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.DeleteFoundingComments();
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            // Root Context

            var rootKeys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 3, rootKeys.Count() );
            Assert.All( rootKeys, element => Assert.Matches( "^Key [12A]$", element.Value ) );

            var rootComments = doc.XPathSelectComments( "I18N/comment()" );
            Assert.Empty( rootComments );

            var key1Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 1']/comment()" );
            Assert.Equal( 2, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match 1-[12] $", comment.Value ) );

            var key2Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 2']/comment()" );
            Assert.Single( key2Comments );
            Assert.All( key2Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var keyAComments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key A']/comment()" );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match A $", comment.Value ) );

            var rootContexts = doc.XPathSelectElements( "I18N/Context" );
            Assert.Equal( 2, rootContexts.Count() );
            Assert.All( rootContexts, element => Assert.Matches( "^Context [12]$", element.Attribute( "id" ).Value ) );

            // Context 1

            var context1Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/comment()" );
            Assert.Single( context1Comments );
            Assert.All( context1Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var context1Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Entry/Key" );
            Assert.Single( context1Keys );
            Assert.Equal( "Key 3", context1Keys.First().Value );

            var key3Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Entry[Key/text()='Key 3']/comment()" );
            Assert.Equal( 2, key3Comments.Count() );
            Assert.All( key3Comments, comment => Assert.Matches( "^ Found in: Match 3-[12] $", comment.Value ) );

            var context1Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context" );
            Assert.Single( context1Contexts );
            Assert.All( context1Contexts, element => Assert.Matches( "^Context 11$", element.Attribute( "id" ).Value ) );

            // Context 11

            var context11Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 1']/Context[@id='Context 11']/comment()" );
            Assert.Empty( context11Comments );

            var context11Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry/Key" );
            Assert.Single( context11Keys );
            Assert.All( context11Keys, element => Assert.Matches( "^Key 9$", element.Value ) );

            var key9Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry[Key/text()='Key 9']/comment()" );
            Assert.Empty( key9Comments );

            var context11Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Context" );
            Assert.Empty( context11Contexts );

            // Context 2

            var context2Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/comment()" );
            Assert.Empty( context2Comments );

            var context2Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Entry/Key" );
            Assert.Empty( context2Keys );

            var context2Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context" );
            Assert.Single( context2Contexts );
            Assert.Equal( "Context 22", context2Contexts.First().Attribute( "id" ).Value );

            // Context 22

            var context22Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/Context[@id='Context 22']/comment()" );
            Assert.Empty( context22Comments );

            var context22Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry/Key" );
            Assert.Single( context22Keys );
            Assert.Equal( "Key 3", context1Keys.First().Value );

            var key4Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry[Key/text()='Key 4']/comment()" );
            Assert.Single( key4Comments );
            Assert.Equal( " Found in: Match 4 ", key4Comments.First().Value );

            var context22Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Context" );
            Assert.Empty( context22Contexts );
        }


        [Fact]
        public void GenerateFile_Update_MarkDeprecated()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateInitialOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.DeleteFoundingComments();
            outputFile.CreateEntries( rootContext );
            outputFile.CreateDeprecationComments();
            outputFile.WriteToFile( m_tempFile );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            // Root Context

            var rootKeys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 3, rootKeys.Count() );
            Assert.All( rootKeys, element => Assert.Matches( "^Key [12A]$", element.Value ) );

            var rootComments = doc.XPathSelectComments( "I18N/comment()" );
            Assert.Empty( rootComments );

            var key1Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 1']/comment()" );
            Assert.Equal( 2, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match 1-[12] $", comment.Value ) );

            var key2Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 2']/comment()" );
            Assert.Equal( 2, key2Comments.Count() );
            Assert.All( key2Comments, comment => Assert.Matches( "^ (Non-Erasable Comment|DEPRECATED) $", comment.Value ) );

            var keyAComments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key A']/comment()" );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match A $", comment.Value ) );

            var rootContexts = doc.XPathSelectElements( "I18N/Context" );
            Assert.Equal( 2, rootContexts.Count() );
            Assert.All( rootContexts, element => Assert.Matches( "^Context [12]$", element.Attribute( "id" ).Value ) );

            // Context 1

            var context1Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/comment()" );
            Assert.Single( context1Comments );
            Assert.All( context1Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var context1Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Entry/Key" );
            Assert.Single( context1Keys );
            Assert.Equal( "Key 3", context1Keys.First().Value );

            var key3Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Entry[Key/text()='Key 3']/comment()" );
            Assert.Equal( 2, key3Comments.Count() );
            Assert.All( key3Comments, comment => Assert.Matches( "^ Found in: Match 3-[12] $", comment.Value ) );

            var context1Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context" );
            Assert.Single( context1Contexts );
            Assert.All( context1Contexts, element => Assert.Matches( "^Context 11$", element.Attribute( "id" ).Value ) );

            // Context 11

            var context11Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 1']/Context[@id='Context 11']/comment()" );
            Assert.Empty( context11Comments );

            var context11Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry/Key" );
            Assert.Single( context11Keys );
            Assert.All( context11Keys, element => Assert.Matches( "^Key 9$", element.Value ) );

            var key9Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry[Key/text()='Key 9']/comment()" );
            Assert.Single( key9Comments );
            Assert.Equal( " DEPRECATED ", key9Comments.First().Value );

            var context11Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Context" );
            Assert.Empty( context11Contexts );

            // Context 2

            var context2Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/comment()" );
            Assert.Empty( context2Comments );

            var context2Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Entry/Key" );
            Assert.Empty( context2Keys );

            var context2Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context" );
            Assert.Single( context2Contexts );
            Assert.Equal( "Context 22", context2Contexts.First().Attribute( "id" ).Value );

            // Context 22

            var context22Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/Context[@id='Context 22']/comment()" );
            Assert.Empty( context22Comments );

            var context22Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry/Key" );
            Assert.Single( context22Keys );
            Assert.Equal( "Key 3", context1Keys.First().Value );

            var key4Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry[Key/text()='Key 4']/comment()" );
            Assert.Single( key4Comments );
            Assert.Equal( " Found in: Match 4 ", key4Comments.First().Value );

            var context22Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Context" );
            Assert.Empty( context22Contexts );
        }

        [Fact]
        public void GenerateFile_InitialFile_WrongRoot()
        {
            // Prepare

            var rootContext = CreateRootContext();

            WriteTempFile( "<L10N></L10N>" );

            // Execute & Verify

            var exception = Assert.Throws<ApplicationException>( () => new OutputFile( m_tempFile ) );

            Assert.Contains( "Invalid XML root element in existing output file", exception.Message );
        }

        [Fact]
        public void GenerateFile_InitialFile_InvalidXML()
        {
            // Prepare

            var rootContext = CreateRootContext();

            WriteTempFile( "Whatever" );

            // Execute & Verify

            var exception = Assert.Throws<ApplicationException>( () => new OutputFile( m_tempFile ) );

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

        [Fact]
        public void GenerateFile_Update_Malformed()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateInitialOutputFileMalformed();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            Assert.True( File.Exists( m_tempFile ) );

            var doc = ReadTempFileAsXML();

            // Root Context

            var rootKeys = doc.XPathSelectElements( "I18N/Entry/Key" );
            Assert.Equal( 3, rootKeys.Count() );
            Assert.All( rootKeys, element => Assert.Matches( "^Key [12A]$", element.Value ) );

            var rootComments = doc.XPathSelectComments( "I18N/comment()" );
            Assert.Empty( rootComments );

            var key1Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 1']/comment()" );
            Assert.Equal( 3, key1Comments.Count() );
            Assert.All( key1Comments, comment => Assert.Matches( "^ Found in: Match 1-[12Z] $", comment.Value ) );

            var key2Comments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key 2']/comment()" );
            Assert.Single( key2Comments );
            Assert.All( key2Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var keyAComments = doc.XPathSelectComments( "I18N/Entry[Key/text()='Key A']/comment()" );
            Assert.Single( keyAComments );
            Assert.All( keyAComments, comment => Assert.Matches( "^ Found in: Match A $", comment.Value ) );

            var rootContexts = doc.XPathSelectElements( "I18N/Context" );
            Assert.Equal( 3, rootContexts.Count() );

            var rootContextsNames = doc.XPathSelectAttributes( "I18N/Context/@id" );
            Assert.Equal( 2, rootContextsNames.Count() );
            Assert.All( rootContextsNames, attribute => Assert.Matches( "^Context [12]$", attribute.Value ) );

            // Context 1

            var context1Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/comment()" );
            Assert.Single( context1Comments );
            Assert.All( context1Comments, comment => Assert.Matches( "^ Non-Erasable Comment $", comment.Value ) );

            var context1Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Entry/Key" );
            Assert.Single( context1Keys );
            Assert.All( context1Keys, element => Assert.Matches( "^Key 3$", element.Value ) );

            var key3Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Entry[Key/text()='Key 3']/comment()" );
            Assert.Equal( 3, key3Comments.Count() );
            Assert.All( key3Comments, comment => Assert.Matches( "^ Found in: Match 3-[12Z] $", comment.Value ) );

            var context1Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context" );
            Assert.Single( context1Contexts );
            Assert.All( context1Contexts, element => Assert.Matches( "^Context 11$", element.Attribute( "id" ).Value ) );

            // Context 11

            var context11Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 1']/Context[@id='Context 11']/comment()" );
            Assert.Empty( context11Comments );

            var context11Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry/Key" );
            Assert.Single( context11Keys );
            Assert.All( context11Keys, element => Assert.Matches( "^Key 9$", element.Value ) );

            var key9Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Entry[Key/text()='Key 9']/comment()" );
            Assert.Single( key9Comments );
            Assert.All( key9Comments, comment => Assert.Matches( "^ Found in: Match 9-Z $", comment.Value ) );

            var context11Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 1']/Context[@id='Context 11']/Context" );
            Assert.Empty( context11Contexts );

            // Context 2

            var context2Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/comment()" );
            Assert.Empty( context2Comments );

            var context2Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Entry/Key" );
            Assert.Empty( context2Keys );

            var context2Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context" );
            Assert.Single( context2Contexts );
            Assert.All( context2Contexts, element => Assert.Matches( "^Context 22$", element.Attribute( "id" ).Value ) );

            // Context 22

            var context22Comments = doc.XPathSelectComments( "I18N//Context[@id='Context 2']/Context[@id='Context 22']/comment()" );
            Assert.Empty( context22Comments );

            var context22Keys = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry/Key" );
            Assert.Single( context22Keys );
            Assert.All( context22Keys, element => Assert.Matches( "^Key 4$", element.Value ) );

            var key4Comments = doc.XPathSelectComments( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Entry[Key/text()='Key 4']/comment()" );
            Assert.Single( key4Comments );
            Assert.All( key4Comments, comment => Assert.Matches( "^ Found in: Match 4 $", comment.Value ) );

            var context22Contexts = doc.XPathSelectElements( "I18N/Context[@id='Context 2']/Context[@id='Context 22']/Context" );
            Assert.Empty( context22Contexts );
        }
    }
}
