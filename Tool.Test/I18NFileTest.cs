/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace I18N.DotNet.Tool.Test
{
    public class I18NFileTest : IDisposable
    {
        private readonly string m_tempFile;

        public I18NFileTest()
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

        private string ReadTempFile()
        {
            return File.ReadAllText( m_tempFile ).Replace( "\r\n", "\n" );
        }

        private void CreateExistingOutputFile()
        {
            string contents =
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 @ 1 -->\n" +
                "    <!-- Found in: Match 1 @ 11 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "    <Value lang='de'>Schlüssel 1</Value>\n" +
                "    <Value lang='fr'>Clef 1</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Value lang='es'>Clave 2</Value>\n" +
                "    <Value lang='fr'>Clef 2</Value>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 @ 33 -->\n" +
                "      <!-- Found in: Match 3 @ 1 -->\n" +
                "      <Key lang=\"es,it\">Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 10\">\n" +
                "      <Entry>\n" +
                "        <!-- DEPRECATED -->\n" +
                "        <Key>Key 5</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9 @ 99 -->\n" +
                "        <Key lang=\"ca\">Key 9</Key>\n" +
                "        <Value lang='es'>Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <!-- DEPRECATED -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "</I18N>";
            WriteTempFile( contents );
        }

        private static Context CreateRootContext()
        {
            var rootContext = new Context();

            rootContext.KeyMatches.Add( "Key 1", new List<Context.KeyInfo> { new( "Match 1", 1 ), new( "Match 1", 2 ) } );
            rootContext.KeyMatches.Add( "Key 6", new List<Context.KeyInfo> { new( "Match 6", 0 ) } );
            rootContext.KeyMatches.Add( "Key A", new List<Context.KeyInfo> { new( "Match A", 0 ) } );

            var nestedContext1 = new Context();
            rootContext.NestedContexts.Add( "Context 1", nestedContext1 );

            nestedContext1.KeyMatches.Add( "Key 3", new List<Context.KeyInfo> { new( "Match 3", 1 ), new( "Match 3", 2 ) } );

            var nestedContext2 = new Context();
            rootContext.NestedContexts.Add( "Context 2", nestedContext2 );

            var nestedContext22 = new Context();
            nestedContext2.NestedContexts.Add( "Context 22", nestedContext22 );

            nestedContext22.KeyMatches.Add( "Key 4", new List<Context.KeyInfo> { new( "Match 4", 0 ) } );

            return rootContext;
        }

        private void Check_Parse_NewFile_WithLines()
        {
            // Prepare

            var rootContext = CreateRootContext();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            i18nFile.CreateEntries( rootContext, true );
            i18nFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 @ 1 -->\n" +
                "    <!-- Found in: Match 1 @ 2 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 6 @ 0 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A @ 0 -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 @ 1 -->\n" +
                "      <!-- Found in: Match 3 @ 2 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "  </Context>\n" +
                "  <Context id=\"Context 2\">\n" +
                "    <Context id=\"Context 22\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 4 @ 0 -->\n" +
                "        <Key>Key 4</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";

            string actualContents = ReadTempFile();
            Assert.Equal( expectedContents, actualContents );
        }

        [Fact]
        public void Parse_NewFile_WithLines()
        {
            // Prepare

            File.Delete( m_tempFile );

            // Execute & Verify

            Check_Parse_NewFile_WithLines();
        }

        [Fact]
        public void Parse_NewFile_WithoutLines()
        {
            // Prepare

            File.Delete( m_tempFile );

            var rootContext = CreateRootContext();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            i18nFile.CreateEntries( rootContext, false );
            i18nFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 6 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "  </Context>\n" +
                "  <Context id=\"Context 2\">\n" +
                "    <Context id=\"Context 22\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 4 -->\n" +
                "        <Key>Key 4</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";

            string actualContents = ReadTempFile();
            Assert.Equal( expectedContents, actualContents );
        }

        [Fact]
        public void Parse_UpdateFile_PreserveFoundingComments()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            i18nFile.CreateEntries( rootContext, true );
            i18nFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 @ 1 -->\n" +
                "    <!-- Found in: Match 1 @ 11 -->\n" +
                "    <!-- Found in: Match 1 @ 2 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "    <Value lang=\"de\">Schlüssel 1</Value>\n" +
                "    <Value lang=\"fr\">Clef 1</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Value lang=\"es\">Clave 2</Value>\n" +
                "    <Value lang=\"fr\">Clef 2</Value>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 @ 33 -->\n" +
                "      <!-- Found in: Match 3 @ 1 -->\n" +
                "      <!-- Found in: Match 3 @ 2 -->\n" +
                "      <Key lang=\"es,it\">Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 10\">\n" +
                "      <Entry>\n" +
                "        <!-- DEPRECATED -->\n" +
                "        <Key>Key 5</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9 @ 99 -->\n" +
                "        <Key lang=\"ca\">Key 9</Key>\n" +
                "        <Value lang=\"es\">Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <!-- DEPRECATED -->\n" +
                "    <!-- Found in: Match 6 @ 0 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A @ 0 -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 2\">\n" +
                "    <Context id=\"Context 22\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 4 @ 0 -->\n" +
                "        <Key>Key 4</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";

            string actualContents = ReadTempFile();
            Assert.Equal( expectedContents, actualContents );
        }

        [Fact]
        public void Parse_UpdateFile_DeleteFoundingComments()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            i18nFile.DeleteFoundingComments();
            i18nFile.CreateEntries( rootContext, true );
            i18nFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 @ 1 -->\n" +
                "    <!-- Found in: Match 1 @ 2 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "    <Value lang=\"de\">Schlüssel 1</Value>\n" +
                "    <Value lang=\"fr\">Clef 1</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Value lang=\"es\">Clave 2</Value>\n" +
                "    <Value lang=\"fr\">Clef 2</Value>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 @ 1 -->\n" +
                "      <!-- Found in: Match 3 @ 2 -->\n" +
                "      <Key lang=\"es,it\">Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 10\">\n" +
                "      <Entry>\n" +
                "        <!-- DEPRECATED -->\n" +
                "        <Key>Key 5</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <Key lang=\"ca\">Key 9</Key>\n" +
                "        <Value lang=\"es\">Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <!-- DEPRECATED -->\n" +
                "    <!-- Found in: Match 6 @ 0 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A @ 0 -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 2\">\n" +
                "    <Context id=\"Context 22\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 4 @ 0 -->\n" +
                "        <Key>Key 4</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";

            string actualContents = ReadTempFile();
            Assert.Equal( expectedContents, actualContents );
        }

        [Fact]
        public void Parse_UpdateFile_MarkDeprecated()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            i18nFile.DeleteFoundingComments();
            i18nFile.CreateEntries( rootContext, true );
            i18nFile.CreateDeprecationComments();
            i18nFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 @ 1 -->\n" +
                "    <!-- Found in: Match 1 @ 2 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "    <Value lang=\"de\">Schlüssel 1</Value>\n" +
                "    <Value lang=\"fr\">Clef 1</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- DEPRECATED -->\n" +
                "    <Key>Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Value lang=\"es\">Clave 2</Value>\n" +
                "    <Value lang=\"fr\">Clef 2</Value>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 @ 1 -->\n" +
                "      <!-- Found in: Match 3 @ 2 -->\n" +
                "      <Key lang=\"es,it\">Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 10\">\n" +
                "      <Entry>\n" +
                "        <!-- DEPRECATED -->\n" +
                "        <Key>Key 5</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- DEPRECATED -->\n" +
                "        <Key lang=\"ca\">Key 9</Key>\n" +
                "        <Value lang=\"es\">Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 6 @ 0 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A @ 0 -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 2\">\n" +
                "    <Context id=\"Context 22\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 4 @ 0 -->\n" +
                "        <Key>Key 4</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";

            string actualContents = ReadTempFile();
            Assert.Equal( expectedContents, actualContents );
        }

        [Fact]
        public void Parse_UpdateFile_WrongRoot()
        {
            // Prepare

            var rootContext = CreateRootContext();

            WriteTempFile( "<L10N></L10N>" );

            // Execute & Verify

            var i18nFile = new I18NFile();
            var exception = Assert.Throws<ApplicationException>( () => i18nFile.LoadFromFile( m_tempFile ) );

            Assert.Contains( "Invalid XML root element in existing output file", exception.Message );
        }

        [Fact]
        public void Parse_UpdateFile_InvalidXML()
        {
            // Prepare

            var rootContext = CreateRootContext();

            WriteTempFile( "Whatever" );

            // Execute & Verify

            var i18nFile = new I18NFile();
            var exception = Assert.Throws<ApplicationException>( () => i18nFile.LoadFromFile( m_tempFile ) );

            Assert.Contains( "Invalid XML format in existing output file", exception.Message );
        }

        [Fact]
        public void Parse_UpdateFile_Blank()
        {
            // Prepare

            WriteTempFile( "" );

            // Execute & Verify

            Check_Parse_NewFile_WithLines();
        }

        [Fact]
        public void Parse_UpdateFile_OnlyWhiteSpace()
        {
            // Prepare

            WriteTempFile( " \n" );

            // Execute & Verify

            Check_Parse_NewFile_WithLines();
        }

        private void CreateExistingOutputFileMalformed()
        {
            string contents =
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 @ 1 -->\n" +
                "    <!-- Found in: Match 1 @ 11 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key lang=\"*\">Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 @ 33 -->\n" +
                "      <!-- Found in: Match 3 @ 1 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "      <Value>Value without language</Value>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9 @ 99 -->\n" +
                "        <Key>Key 9</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key/>\n" +
                "    <Value lang='zh'></Value>\n" +
                "    <Value lang='zh'>Other value</Value>\n" +
                "  </Entry>\n" +
                "  <Context>\n" +
                "    <Entry>\n" +
                "      <Key>Key 77</Key>\n" +
                "      <Key>Key 78</Key>\n" +
                "      <Value lang=''>Value with empty language</Value>\n" +
                "    </Entry>\n" +
                "    <Context id=''>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";
            WriteTempFile( contents );
        }

        [Fact]
        public void Parse_UpdateFile_Malformed()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFileMalformed();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            i18nFile.CreateEntries( rootContext, true );
            i18nFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1 @ 1 -->\n" +
                "    <!-- Found in: Match 1 @ 11 -->\n" +
                "    <!-- Found in: Match 1 @ 2 -->\n" +
                "    <Key>Key 1</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key lang=\"*\">Key 2</Key>\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <!-- Non-Erasable Comment -->\n" +
                "    <Entry>\n" +
                "      <!-- Found in: Match 3 @ 33 -->\n" +
                "      <!-- Found in: Match 3 @ 1 -->\n" +
                "      <!-- Found in: Match 3 @ 2 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "      <Value>Value without language</Value>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9 @ 99 -->\n" +
                "        <Key>Key 9</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry></Entry>\n" +
                "  <Entry>\n" +
                "    <Key />\n" +
                "    <Value lang=\"zh\"></Value>\n" +
                "    <Value lang=\"zh\">Other value</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 6 @ 0 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A @ 0 -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
                "  <Context>\n" +
                "    <Entry>\n" +
                "      <Key>Key 77</Key>\n" +
                "      <Key>Key 78</Key>\n" +
                "      <Value lang=\"\">Value with empty language</Value>\n" +
                "    </Entry>\n" +
                "    <Context id=\"\"></Context>\n" +
                "  </Context>\n" +
                "  <Context id=\"Context 2\">\n" +
                "    <Context id=\"Context 22\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 4 @ 0 -->\n" +
                "        <Key>Key 4</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";

            string actualContents = ReadTempFile();
            Assert.Equal( expectedContents, actualContents );
        }

        [Fact]
        public void Analyze_FileIssues()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetFileIssues().ToArray();

            // Verify

            var expectedResults = Array.Empty<(int line, string message, bool isError)>();

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_FileIssues_Malformed()
        {
            // Prepare

            CreateExistingOutputFileMalformed();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetFileIssues().ToArray();

            // Verify

            var expectedResults = new (int line, string message, bool isError)[]
            {
                ( 26, "'Entry' element does not have a 'Key' element", true ),
                ( 29, "'Key' element is empty", true ),
                ( 30, "'Value' element is empty", false ),
                ( 31, "Translation for language 'zh' has already been defined at line 30", false ),
                ( 17, "'Value' element attribute 'lang' is missing", true ),
                ( 33, "'Context' element attribute 'id' is missing", true ),
                ( 34, "'Entry' element has more than one 'Key' element", true ),
                ( 37, "'Value' element attribute 'lang' is empty", true ),
                ( 39, "'Context' element attribute 'id' is empty", true ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_DeprecatedEntries()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetDeprecatedEntries( Array.Empty<Regex>(), Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 9, "/", "Key 2" ),
                ( 36, "/", "Key 6" ),
                ( 23, "/Context 1/Context 10/", "Key 5" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_DeprecatedEntries_IncludeContext()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetDeprecatedEntries( new Regex[] { new( "^/Context.*$" ) }, Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 23, "/Context 1/Context 10/", "Key 5" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_DeprecatedEntries_ExcludeContext()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetDeprecatedEntries( Array.Empty<Regex>(), new Regex[] { new( "^/Context.*$" ) } ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 9, "/", "Key 2" ),
                ( 36, "/", "Key 6" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_DeprecatedEntries_Malformed()
        {
            // Prepare

            CreateExistingOutputFileMalformed();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetDeprecatedEntries( Array.Empty<Regex>(), Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 7, "/", "Key 2" ),
                ( 26, "/", null ),
                ( 28, "/", "" ),
                ( 34, "//", "Key 77" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_AnyLanguage()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetNoTranslationEntries( Array.Empty<string>(), Array.Empty<Regex>(), Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 36, "/", "Key 6" ),
                ( 23, "/Context 1/Context 10/", "Key 5" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_OneLanguage()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetNoTranslationEntries( new string[] { "fr" }, Array.Empty<Regex>(), Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 36, "/", "Key 6" ),
                ( 17, "/Context 1/", "Key 3" ),
                ( 23, "/Context 1/Context 10/", "Key 5" ),
                ( 29, "/Context 1/Context 11/", "Key 9" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_MultipleLanguages()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetNoTranslationEntries( new string[] { "fr", "es" }, Array.Empty<Regex>(), Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 2, "/", "Key 1" ),
                ( 36, "/", "Key 6" ),
                ( 17, "/Context 1/", "Key 3" ),
                ( 23, "/Context 1/Context 10/", "Key 5" ),
                ( 29, "/Context 1/Context 11/", "Key 9" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_AnyLanguage_Malformed()
        {
            // Prepare

            CreateExistingOutputFileMalformed();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetNoTranslationEntries( Array.Empty<string>(), Array.Empty<Regex>(), Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 2, "/", "Key 1" ),
                ( 26, "/", null ),
                ( 13, "/Context 1/", "Key 3" ),
                ( 20, "/Context 1/Context 11/", "Key 9" ),
                ( 34, "//", "Key 77" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_OneLanguage_Malformed()
        {
            // Prepare

            CreateExistingOutputFileMalformed();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            var actualResults = i18nFile.GetNoTranslationEntries( new string[] { "fr" }, Array.Empty<Regex>(), Array.Empty<Regex>() ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string? key)[]
            {
                ( 2, "/", "Key 1" ),
                ( 26, "/", null ),
                ( 28, "/", "" ),
                ( 13, "/Context 1/", "Key 3" ),
                ( 20, "/Context 1/Context 11/", "Key 9" ),
                ( 34, "//", "Key 77" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Deploy()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var i18nFile = new I18NFile();
            i18nFile.LoadFromFile( m_tempFile );
            i18nFile.PrepareForDeployment();
            i18nFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <Key>Key 1</Key>\n" +
                "    <Value lang=\"de\">Schlüssel 1</Value>\n" +
                "    <Value lang=\"fr\">Clef 1</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Key 2</Key>\n" +
                "    <Value lang=\"es\">Clave 2</Value>\n" +
                "    <Value lang=\"fr\">Clef 2</Value>\n" +
                "  </Entry>\n" +
                "  <Context id=\"Context 1\">\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <Key>Key 9</Key>\n" +
                "        <Value lang=\"es\">Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "</I18N>";

            string actualContents = ReadTempFile();
            Assert.Equal( expectedContents, actualContents );
        }

        [Fact]
        public void Exception_NotInitialized()
        {
            // Prepare

            var i18nFile = new I18NFile();

            // Execute & Verify

            Assert.Throws<InvalidOperationException>( () => i18nFile.WriteToFile( m_tempFile ) );
        }
    }
}
