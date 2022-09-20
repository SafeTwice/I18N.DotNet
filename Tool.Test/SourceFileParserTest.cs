/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace I18N.Tool.Test
{
    public class SourceFileParserTest : IDisposable
    {
        private string m_tempFile;

        private const string TEMP_FILE_NAME = "Temp.cs";

        public SourceFileParserTest()
        {
            m_tempFile = Path.GetTempPath() + $"\\{TEMP_FILE_NAME}";
        }

        public void Dispose()
        {
            File.Delete( m_tempFile );
        }

        private void WriteTempFile( string contents )
        {
            File.WriteAllText( m_tempFile, contents );
        }

        private void CreateInputFile()
        {
            string contents =
                "/* First line */\n" +
                "// This Localize should not be matched\n" +
                "Localize ( \"Plain String\" );\n" +
                "  \n" +
                "  Localize( $\"Interpolated String {Whatever:F3} {SomethingElse} {Foo,5:X4}\" );\n" +
                "\n" +
                "CustomFunction(\"Plain String\");\n" +
                "CustomFunction(\"Another String \\t\");\n" +
                "  myLocalizer.LocalizeFormat( \"Format String {0:X4}\", param1 );\n" +
                "\tLocalize(\"Plain String\"); // Again\n" +
                "Localize(); // Ignored\n" +
                "/* Last line */";

            WriteTempFile( contents );
        }

        [Fact]
        public void ParseFile_Default()
        {
            // Prepare

            CreateInputFile();

            var rootContext = new Context();

            // Execute

            SourceFileParser.ParseFile( m_tempFile, null, rootContext );

            // Verify

            string plainStringKey = "Plain String";
            string interpolatedStringKey = "Interpolated String {0:F3} {1} {2,5:X4}";
            string formatStringKey = "Format String {0:X4}";

            Assert.Equal( 3, rootContext.KeyMatches.Count );
            Assert.Empty( rootContext.NestedContexts );

            Assert.True( rootContext.KeyMatches.ContainsKey( plainStringKey ) );
            Assert.Equal( 2, rootContext.KeyMatches[plainStringKey].Count );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 3", rootContext.KeyMatches[plainStringKey][0] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 10", rootContext.KeyMatches[plainStringKey][1] );

            Assert.True( rootContext.KeyMatches.ContainsKey( interpolatedStringKey ) );
            Assert.Single( rootContext.KeyMatches[interpolatedStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 5", rootContext.KeyMatches[interpolatedStringKey][0] );

            Assert.True( rootContext.KeyMatches.ContainsKey( formatStringKey ) );
            Assert.Single( rootContext.KeyMatches[formatStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 9", rootContext.KeyMatches[formatStringKey][0] );
        }

        [Fact]
        public void ParseFile_NonDefault()
        {
            // Prepare

            CreateInputFile();

            var rootContext = new Context();

            // Execute

            SourceFileParser.ParseFile( m_tempFile, new List<string>() { "CustomFunction" }, rootContext );

            // Verify

            string plainStringKey = "Plain String";
            string interpolatedStringKey = "Interpolated String {0:F3} {1} {2,5:X4}";
            string formatStringKey = "Format String {0:X4}";
            string anotherStringKey = "Another String \\t";

            Assert.Equal( 4, rootContext.KeyMatches.Count );
            Assert.Empty( rootContext.NestedContexts );

            Assert.True( rootContext.KeyMatches.ContainsKey( plainStringKey ) );
            Assert.Equal( 3, rootContext.KeyMatches[plainStringKey].Count );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 3", rootContext.KeyMatches[plainStringKey][0] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 7", rootContext.KeyMatches[plainStringKey][1] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 10", rootContext.KeyMatches[plainStringKey][2] );

            Assert.True( rootContext.KeyMatches.ContainsKey( interpolatedStringKey ) );
            Assert.Single( rootContext.KeyMatches[interpolatedStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 5", rootContext.KeyMatches[interpolatedStringKey][0] );

            Assert.True( rootContext.KeyMatches.ContainsKey( formatStringKey ) );
            Assert.Single( rootContext.KeyMatches[formatStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 9", rootContext.KeyMatches[formatStringKey][0] );

            Assert.True( rootContext.KeyMatches.ContainsKey( anotherStringKey ) );
            Assert.Single( rootContext.KeyMatches[anotherStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 8", rootContext.KeyMatches[anotherStringKey][0] );
        }

        [Fact]
        public void ParseFile_EscapeCodes()
        {
            // Prepare

            string contents =
                "Localize(\"\\t\\r\\n\\v\\f\\b\\\\n\")";
            WriteTempFile( contents );

            var rootContext = new Context();

            // Execute

            SourceFileParser.ParseFile( m_tempFile, null, rootContext );

            // Verify

            string key = "\\t\\r\\n\\v\\f\\b\\\\n";
            Assert.Single( rootContext.KeyMatches );
            Assert.Empty( rootContext.NestedContexts );

            Assert.True( rootContext.KeyMatches.ContainsKey( key ) );
            Assert.Single( rootContext.KeyMatches[ key ] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 1", rootContext.KeyMatches[ key ][ 0 ] );
        }

        private void CreateInputFileWithContext()
        {
            string contents =
                "Localize(\"Plain String 1\");\n" +
                "Context(\"Context 1\").Localize(\"Plain String 2\");\n" +
                "Context(\"Context 1\").Context(\"Context 1_1\").Localize(\"Plain String 3\");\n" +
                "Context(\"Context 2.Context 2_2\").Localize(\"Plain String 4\");\n" +
                "AnotherMethod(\"Don't Care\").Localize(\"Plain String 5\");\n" +
                "Context(\"Context Ignored\", 2).Localize(\"Plain String 5\");\n" +
                "Context().Localize(\"Plain String 5\"); // Context Ignored";

            WriteTempFile( contents );
        }

        [Fact]
        public void ParseFile_Context()
        {
            // Prepare

            CreateInputFileWithContext();

            var rootContext = new Context();

            // Execute

            SourceFileParser.ParseFile( m_tempFile, null, rootContext );

            // Verify

            string plainString1Key = "Plain String 1";
            string plainString2Key = "Plain String 2";
            string plainString3Key = "Plain String 3";
            string plainString4Key = "Plain String 4";
            string plainString5Key = "Plain String 5";

            Assert.Equal( 2, rootContext.KeyMatches.Count );

            Assert.True( rootContext.KeyMatches.ContainsKey( plainString1Key ) );
            Assert.Single( rootContext.KeyMatches[ plainString1Key ] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 1", rootContext.KeyMatches[ plainString1Key ][ 0 ] );

            Assert.True( rootContext.KeyMatches.ContainsKey( plainString5Key ) );
            Assert.Equal( 3, rootContext.KeyMatches[ plainString5Key ].Count );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 5", rootContext.KeyMatches[ plainString5Key ][ 0 ] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 6", rootContext.KeyMatches[ plainString5Key ][ 1 ] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 7", rootContext.KeyMatches[ plainString5Key ][ 2 ] );

            Assert.Equal( 2, rootContext.NestedContexts.Count );

            Context context1 = rootContext.NestedContexts[ "Context 1" ];
            Assert.NotNull( context1 );

            Assert.True( context1.KeyMatches.ContainsKey( plainString2Key ) );
            Assert.Single( context1.KeyMatches[ plainString2Key ] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 2", context1.KeyMatches[ plainString2Key ][ 0 ] );

            Assert.Single( context1.NestedContexts );

            Context context1_1 = context1.NestedContexts[ "Context 1_1" ];
            Assert.NotNull( context1_1 );

            Assert.True( context1_1.KeyMatches.ContainsKey( plainString3Key ) );
            Assert.Single( context1_1.KeyMatches[ plainString3Key ] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 3", context1_1.KeyMatches[ plainString3Key ][ 0 ] );

            Assert.Empty( context1_1.NestedContexts );

            Context context2 = rootContext.NestedContexts[ "Context 2" ];
            Assert.NotNull( context2 );

            Assert.Empty( context2.KeyMatches );
            Assert.Single( context1.NestedContexts );

            Context context2_2 = context2.NestedContexts[ "Context 2_2" ];
            Assert.NotNull( context2_2 );

            Assert.True( context2_2.KeyMatches.ContainsKey( plainString4Key ) );
            Assert.Single( context2_2.KeyMatches[ plainString4Key ] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 4", context2_2.KeyMatches[ plainString4Key ][ 0 ] );

            Assert.Empty( context2_2.NestedContexts );
        }
    }
}
