/**
 * @file
 * @copyright  Copyright (c) 2020 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace I18N.Tool.Test
{
    public class InputFileParserTest : IDisposable
    {
        private string m_tempFile;

        private const string TEMP_FILE_NAME = "Temp.cs";

        public InputFileParserTest()
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
                "  Localize( $\"Interpolated String {Whatever:F3} {SomethingElse} {Foo:X4}\" );\n" +
                "\n" +
                "CustomFunction(\"Plain String\");\n" +
                "CustomFunction(\"Another String \\t\");\n" +
                "  myLocalizer.LocalizeFormat( \"Format String {0:X4}\", param1 );\n" +
                "\tLocalize(\"Plain String\"); // Again\n" +
                "/* Last line */";

            WriteTempFile( contents );
        }

        [Fact]
        public void ParseFile_Default()
        {
            // Prepare

            CreateInputFile();

            Dictionary<string, List<string>> keyMatches = new Dictionary<string, List<string>>();

            // Execute

            InputFileParser.ParseFile( m_tempFile, null, keyMatches );

            // Verify

            string plainStringKey = "Plain String";
            string interpolatedStringKey = "Interpolated String {0:F3} {1} {2:X4}";
            string formatStringKey = "Format String {0:X4}";

            Assert.Equal( 3, keyMatches.Count );

            Assert.True( keyMatches.ContainsKey( plainStringKey ) );
            Assert.Equal( 2, keyMatches[plainStringKey].Count );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 3", keyMatches[plainStringKey][0] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 10", keyMatches[plainStringKey][1] );

            Assert.True( keyMatches.ContainsKey( interpolatedStringKey ) );
            Assert.Single( keyMatches[interpolatedStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 5", keyMatches[interpolatedStringKey][0] );

            Assert.True( keyMatches.ContainsKey( formatStringKey ) );
            Assert.Single( keyMatches[formatStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 9", keyMatches[formatStringKey][0] );
        }

        [Fact]
        public void ParseFile_NonDefault()
        {
            // Prepare

            CreateInputFile();

            Dictionary<string, List<string>> keyMatches = new Dictionary<string, List<string>>();

            // Execute

            InputFileParser.ParseFile( m_tempFile, new List<string>() { "CustomFunction" }, keyMatches );

            // Verify

            string plainStringKey = "Plain String";
            string interpolatedStringKey = "Interpolated String {0:F3} {1} {2:X4}";
            string formatStringKey = "Format String {0:X4}";
            string anotherStringKey = "Another String \\t";

            Assert.Equal( 4, keyMatches.Count );

            Assert.True( keyMatches.ContainsKey( plainStringKey ) );
            Assert.Equal( 3, keyMatches[plainStringKey].Count );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 3", keyMatches[plainStringKey][0] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 7", keyMatches[plainStringKey][1] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 10", keyMatches[plainStringKey][2] );

            Assert.True( keyMatches.ContainsKey( interpolatedStringKey ) );
            Assert.Single( keyMatches[interpolatedStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 5", keyMatches[interpolatedStringKey][0] );

            Assert.True( keyMatches.ContainsKey( formatStringKey ) );
            Assert.Single( keyMatches[formatStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 9", keyMatches[formatStringKey][0] );

            Assert.True( keyMatches.ContainsKey( anotherStringKey ) );
            Assert.Single( keyMatches[anotherStringKey] );
            Assert.Contains( $"{TEMP_FILE_NAME} @ 8", keyMatches[anotherStringKey][0] );
        }
    }
}
