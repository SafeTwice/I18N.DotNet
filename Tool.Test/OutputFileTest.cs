/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Xunit;

namespace I18N.Tool.Test
{
    public class OutputFileTest : IDisposable
    {
        private string m_tempFile;

        public OutputFileTest()
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
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-Z -->\n" +
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
                "      <!-- Found in: Match 3-Z -->\n" +
                "      <!-- Found in: Match 3-1 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Entry>\n" +
                "      <!-- DEPRECATED -->\n" +
                "      <Key>Key 5</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9-Z -->\n" +
                "        <Key>Key 9</Key>\n" +
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

        private Context CreateRootContext()
        {
            var rootContext = new Context();

            rootContext.KeyMatches.Add( "Key 1", new List<string> { "Match 1-1", "Match 1-2" } );
            rootContext.KeyMatches.Add( "Key 6", new List<string> { "Match 6" } );
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

        private void Check_Generate_NewFile()
        {
            // Prepare

            var rootContext = CreateRootContext();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-2 -->\n" +
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
                "      <!-- Found in: Match 3-1 -->\n" +
                "      <!-- Found in: Match 3-2 -->\n" +
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
        public void Generate_NewFile()
        {
            // Prepare

            File.Delete( m_tempFile );

            // Execute & Verify

            Check_Generate_NewFile();
        }

        [Fact]
        public void Generate_UpdateFile_PreserveFoundingComments()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-Z -->\n" +
                "    <!-- Found in: Match 1-2 -->\n" +
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
                "      <!-- Found in: Match 3-Z -->\n" +
                "      <!-- Found in: Match 3-1 -->\n" +
                "      <!-- Found in: Match 3-2 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Entry>\n" +
                "      <!-- DEPRECATED -->\n" +
                "      <Key>Key 5</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9-Z -->\n" +
                "        <Key>Key 9</Key>\n" +
                "        <Value lang=\"es\">Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <!-- DEPRECATED -->\n" +
                "    <!-- Found in: Match 6 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
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
        public void Generate_UpdateFile_DeleteFoundingComments()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.DeleteFoundingComments();
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-2 -->\n" +
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
                "      <!-- Found in: Match 3-1 -->\n" +
                "      <!-- Found in: Match 3-2 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Entry>\n" +
                "      <!-- DEPRECATED -->\n" +
                "      <Key>Key 5</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <Key>Key 9</Key>\n" +
                "        <Value lang=\"es\">Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <!-- DEPRECATED -->\n" +
                "    <!-- Found in: Match 6 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
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
        public void Generate_UpdateFile_MarkDeprecated()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.DeleteFoundingComments();
            outputFile.CreateEntries( rootContext );
            outputFile.CreateDeprecationComments();
            outputFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-2 -->\n" +
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
                "      <!-- Found in: Match 3-1 -->\n" +
                "      <!-- Found in: Match 3-2 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Entry>\n" +
                "      <!-- DEPRECATED -->\n" +
                "      <Key>Key 5</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- DEPRECATED -->\n" +
                "        <Key>Key 9</Key>\n" +
                "        <Value lang=\"es\">Clave 9</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 6 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
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
        public void Generate_UpdateFile_WrongRoot()
        {
            // Prepare

            var rootContext = CreateRootContext();

            WriteTempFile( "<L10N></L10N>" );

            // Execute & Verify

            var exception = Assert.Throws<ApplicationException>( () => new OutputFile( m_tempFile ) );

            Assert.Contains( "Invalid XML root element in existing output file", exception.Message );
        }

        [Fact]
        public void Generate_UpdateFile_InvalidXML()
        {
            // Prepare

            var rootContext = CreateRootContext();

            WriteTempFile( "Whatever" );

            // Execute & Verify

            var exception = Assert.Throws<ApplicationException>( () => new OutputFile( m_tempFile ) );

            Assert.Contains( "Invalid XML format in existing output file", exception.Message );
        }

        [Fact]
        public void Generate_UpdateFile_Blank()
        {
            // Prepare

            WriteTempFile( "" );

            // Execute & Verify

            Check_Generate_NewFile();
        }

        [Fact]
        public void Generate_UpdateFile_OnlyWhiteSpace()
        {
            // Prepare

            WriteTempFile( " \n" );

            // Execute & Verify

            Check_Generate_NewFile();
        }

        private void CreateExistingOutputFileMalformed()
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

        [Fact]
        public void Generate_UpdateFile_Malformed()
        {
            // Prepare

            var rootContext = CreateRootContext();

            CreateExistingOutputFileMalformed();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            outputFile.CreateEntries( rootContext );
            outputFile.WriteToFile( m_tempFile );

            // Verify

            string expectedContents =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 1-1 -->\n" +
                "    <!-- Found in: Match 1-Z -->\n" +
                "    <!-- Found in: Match 1-2 -->\n" +
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
                "      <!-- Found in: Match 3-2 -->\n" +
                "      <Key>Key 3</Key>\n" +
                "    </Entry>\n" +
                "    <Context id=\"Context 11\">\n" +
                "      <Entry>\n" +
                "        <!-- Found in: Match 9-Z -->\n" +
                "        <Key>Key 9</Key>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry></Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match 6 -->\n" +
                "    <Key>Key 6</Key>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <!-- Found in: Match A -->\n" +
                "    <Key>Key A</Key>\n" +
                "  </Entry>\n" +
                "  <Context></Context>\n" +
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
        public void Analyze_DeprecatedEntries()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            var actualResults = outputFile.GetDeprecatedEntries( new Regex[ 0 ], new Regex[ 0 ] ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 34, "/", "Key 6" ),
                ( 22, "/Context 1/", "Key 5" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_DeprecatedEntries_IncludeContext()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            var actualResults = outputFile.GetDeprecatedEntries( new Regex[] { new Regex( "^/Context.*$" ) }, new Regex[ 0 ] ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 22, "/Context 1/", "Key 5" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_DeprecatedEntries_ExcludeContext()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            var actualResults = outputFile.GetDeprecatedEntries( new Regex[ 0 ], new Regex[] { new Regex( "^/Context.*$" ) } ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 34, "/", "Key 6" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_AnyLanguage()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            var actualResults = outputFile.GetNoTranslationEntries( new string[ 0 ], new Regex[ 0 ], new Regex[ 0 ] ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 34, "/", "Key 6" ),
                ( 17, "/Context 1/", "Key 3" ),
                ( 22, "/Context 1/", "Key 5" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_OneLanguage()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            var actualResults = outputFile.GetNoTranslationEntries( new string[] { "fr" }, new Regex[ 0 ], new Regex[ 0 ] ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 34, "/", "Key 6" ),
                ( 17, "/Context 1/", "Key 3" ),
                ( 22, "/Context 1/", "Key 5" ),
                ( 27, "/Context 1/Context 11/", "Key 9" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }

        [Fact]
        public void Analyze_NoTranslations_MultipleLanguages()
        {
            // Prepare

            CreateExistingOutputFile();

            // Execute

            var outputFile = new OutputFile( m_tempFile );
            var actualResults = outputFile.GetNoTranslationEntries( new string[] { "fr", "es" }, new Regex[ 0 ], new Regex[ 0 ] ).ToArray();

            // Verify

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 2, "/", "Key 1" ),
                ( 34, "/", "Key 6" ),
                ( 17, "/Context 1/", "Key 3" ),
                ( 22, "/Context 1/", "Key 5" ),
                ( 27, "/Context 1/Context 11/", "Key 9" ),
            };

            Assert.Equal( expectedResults, actualResults );
        }
    }
}
