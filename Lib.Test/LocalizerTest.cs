/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;

#pragma warning disable CA1861, CA1859

namespace I18N.DotNet.Test
{
    public class LocalizerTest
    {
        private static Stream CreateStream( string data )
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter( stream, Encoding.UTF8, 1024, true );

            writer.Write( data );
            writer.Flush();
            stream.Seek( 0, SeekOrigin.Begin );

            return stream;
        }

        private static Stream GetConfigA()
        {
            string config =
                "<I18N>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 1</Key>\n" +
                "    <Value lang='es-es'>Clave simple 1</Value>\n" +
                "    <Value lang='fr-fr'>Clef simple 1</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 2</Key>\n" +
                "    <Value lang='es-es'>Clave simple 2</Value>\n" +
                "    <Value lang='fr-fr'>Clef simple 2</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 3</Key>\n" +
                "    <Value lang='es-es'>Clave simple 3</Value>\n" +
                "    <Value lang='fr-fr'>Clef simple 3</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Simple Key 4</Key>\n" +
                "    <Value lang='es-es'>Clave simple 4</Value>\n" +
                "    <Value lang='fr-fr'>Clef simple 4</Value>\n" +
                "  </Entry>\n" +
                "  <Entry>\n" +
                "    <Key>Format Key: {0:X4}</Key>\n" +
                "    <Value lang='es-es'>Clave de formato: {0}</Value>\n" +
                "    <Value lang='fr'>Clef de format: {0}</Value>\n" +
                "  </Entry>\n" +
                "  <Context id='Level1.Level2'>\n" +
                "    <Entry>\n" +
                "      <Key>Simple Key 1</Key>\n" +
                "      <Value lang='es-es'>Clave simple 1 en contexto L2</Value>\n" +
                "      <Value lang='fr-fr'>Clef simple 1 en contexte L2</Value>\n" +
                "    </Entry>\n" +
                "  </Context>\n" +
                "  <Context id='Level1'>\n" +
                "    <Entry>\n" +
                "      <Key>Simple Key 2</Key>\n" +
                "      <Value lang='es-es'>Clave simple 2 en contexto L1</Value>\n" +
                "      <Value lang='fr-fr'>Clef simple 2 en contexte L1</Value>\n" +
                "    </Entry>\n" +
                "    <Context id='Level2'>\n" +
                "      <Entry>\n" +
                "        <Key>Simple Key 3</Key>\n" +
                "        <Value lang='fr'>Clef simple 3 en contexte L2</Value>\n" +
                "      </Entry>\n" +
                "    </Context>\n" +
                "  </Context>\n" +
                "  <Entry>\n" +
                "    <Key>Escaped:\\n\\r\\f&amp;\\t\\v\\b\\\\n\\xABC</Key>\n" +
                "    <Value lang='es-es'>Escapado:\\n\\r\\f&amp;\\t\\v\\b\\\\n\\xABC</Value>\n" +
                "  </Entry>\n" +
                "</I18N>";

            return CreateStream( config );
        }

        [Fact]
        public void DefaultConstructor()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "es-ES" );

            // Execute & Verify

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA() );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }

        [Theory]
        [InlineData( "Non-existant Key" )]
        [InlineData( "Simple Key 1" )]
        public void Localize_NeutralLanguage_Simple( string key )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "en-us" );

            // Execute & Verify

            Assert.Equal( key, localizer.Localize( key ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clef simple 1" )]
        public void Localize_SpecificLanguage_Simple( string key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "fr-fr" );

            // Execute & Verify

            Assert.Equal( value, localizer.Localize( key ) );
        }

        public static IEnumerable<object[]> GetNeutralLanguageInterpolatedData()
        {
            int i = 1234;
            yield return new object[] { (FormattableString) $"Format Key: {i:X4}", $"Format Key: {i:X4}" };
            yield return new object[] { (FormattableString) $"Non-existent Format: {i}", $"Non-existent Format: {i}" };
        }

        [Theory]
        [MemberData( nameof( GetNeutralLanguageInterpolatedData ) )]
        public void Localize_NeutralLanguage_Interpolated( FormattableString format, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "en-us" );

            // Execute & Verify

            Assert.Equal( value, localizer.Localize( format ) );
        }

        public static IEnumerable<object[]> GetSpecificLanguageInterpolatedData()
        {
            int i = 1234;
            yield return new object[] { (FormattableString) $"Format Key: {i:X4}", $"Clave de formato: {i}" };
            yield return new object[] { (FormattableString) $"Non-existent Format: {i}", $"Non-existent Format: {i}" };
        }

        [Theory]
        [MemberData( nameof( GetSpecificLanguageInterpolatedData ) )]
        public void Localize_SpecificLanguage_Interpolated( FormattableString format, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "es-es" );

            // Execute & Verify

            Assert.Equal( value, localizer.Localize( format ) );
        }

        [Fact]
        public void Localize_EscapedChars()
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "es-es" );

            // Execute & Verify

            Assert.Equal( "Escapado:\n\r\f&\t\v\b\\n\xABC", localizer.Localize( "Escaped:\n\r\f&\t\v\b\\n\xABC" ) );
        }

        [Fact]
        public void Localize_MultipleKeys()
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "fr-fr" );

            // Execute & Verify

            Assert.Equal( new string[] { "Non-existant Key", "Clef simple 1" }, localizer.Localize( new string[] { "Non-existant Key", "Simple Key 1" } ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clef simple 1" )]
        [InlineData( "Simple Key 2", "Clef simple 2 en contexte L1" )]
        [InlineData( "Simple Key 3", "Clef simple 3" )]
        [InlineData( "Simple Key 4", "Clef simple 4" )]
        public void Context_ExistingContextL1( string key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "fr-fr" );

            // Execute & Verify

            Assert.Equal( value, localizer.Context( "Level1" ).Localize( key ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clef simple 1 en contexte L2" )]
        [InlineData( "Simple Key 2", "Clef simple 2 en contexte L1" )]
        [InlineData( "Simple Key 3", "Clef simple 3 en contexte L2" )]
        [InlineData( "Simple Key 4", "Clef simple 4" )]
        public void Context_ExistingContextL2A( string key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "fr-fr" );

            // Execute & Verify

            Assert.Equal( value, localizer.Context( "Level1.Level2" ).Localize( key ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clave simple 1 en contexto L2" )]
        [InlineData( "Simple Key 2", "Clave simple 2 en contexto L1" )]
        [InlineData( "Simple Key 3", "Clave simple 3" )]
        [InlineData( "Simple Key 4", "Clave simple 4" )]
        public void Context_ExistingContextL2B( string key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "es-es" );

            // Execute & Verify

            Assert.Equal( value, localizer.Context( "Level1.Level2" ).Localize( key ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clave simple 1 en contexto L2" )]
        [InlineData( "Simple Key 2", "Clave simple 2 en contexto L1" )]
        [InlineData( "Simple Key 3", "Clave simple 3" )]
        [InlineData( "Simple Key 4", "Clave simple 4" )]
        public void Context_ExistingContextL2B_Split( string key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "es-es" );

            // Execute & Verify

            Assert.Equal( value, localizer.Context( new string[] { "Level1", "Level2" } ).Localize( key ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clef simple 1" )]
        [InlineData( "Simple Key 2", "Clef simple 2" )]
        [InlineData( "Simple Key 3", "Clef simple 3" )]
        [InlineData( "Simple Key 4", "Clef simple 4" )]
        public void Context_NonExistingContext( string key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "fr-fr" );

            // Execute & Verify

            Assert.Equal( value, localizer.Context( "LevelX" ).Localize( key ) );
        }

        [Theory]
        [MemberData( nameof( GetSpecificLanguageInterpolatedData ) )]
        public void Context_Interpolated( FormattableString key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "es-es" );

            // Execute & Verify

            Assert.Equal( value, localizer.Context( "Level1" ).Localize( key ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clef simple 1" )]
        [InlineData( "Simple Key 2", "Clef simple 2 en contexte L1" )]
        [InlineData( "Simple Key 3", "Clef simple 3" )]
        [InlineData( "Simple Key 4", "Clef simple 4" )]
        public void ILocalizer_Context_Single( string key, string value )
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetConfigA(), "fr-fr" );

            ILocalizer ilocalizer = localizer;

            // Execute & Verify

            Assert.Equal( value, ilocalizer.Context( "Level1" ).Localize( key ) );
        }

        [Theory]
        [InlineData( "Non-existant Key", "Non-existant Key" )]
        [InlineData( "Simple Key 1", "Clave simple 1 en contexto L2" )]
        [InlineData( "Simple Key 2", "Clave simple 2 en contexto L1" )]
        [InlineData( "Simple Key 3", "Clave simple 3" )]
        [InlineData( "Simple Key 4", "Clave simple 4" )]
        public void ILocalizer_Context_Split( string key, string value )
        {
            // Prepare

            var localizer = new Localizer( );
            localizer.LoadXML( GetConfigA(), "es-es" );

            ILocalizer ilocalizer = localizer;

            // Execute & Verify

            Assert.Equal( value, ilocalizer.Context( new string[] { "Level1", "Level2" } ).Localize( key ) );
        }

        [Fact]
        public void LoadXML_InvalidFormat()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<System.Xml.XmlException>( () => localizer.LoadXML( data ) );
        }

        [Fact]
        public void LoadXML_Root_InvalidRootNode()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<L10N></L10N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Invalid XML root element", exception.Message );
        }

        [Fact]
        public void LoadXML_Root_InvalidChildElement()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><X/></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Invalid XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_NoKey()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Value lang='x'>X</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data,  "x"  ) );

            Assert.Contains( "Missing child 'Key' XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_TooManyKeys()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value lang='x'>X</Value><Key>Y</Key></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data, "x" ) );

            Assert.Contains( "Too many child 'Key' XML elements", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_TooManyValuesForSameLanguage()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value lang='x'>X</Value><Value lang='x'>Y</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data, "x" ) );

            Assert.Contains( "Too many child 'Value' XML elements with the same 'lang' attribute", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_TooManyValuesForSamePrimaryLanguage()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value lang='en'>X</Value><Value lang='en'>Y</Value><Value lang='en-gb'>Z</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data, "en-GB" ) );

            Assert.Contains( "Too many child 'Value' XML elements with the same 'lang' attribute", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_ValuesWithoutLanguage()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value>X</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Missing attribute 'lang' in 'Value' XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_InvalidChildElement()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Kay>X</Kay><Value lang='x'>X</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data, "x" ) );

            Assert.Contains( "Invalid XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Context_NoId()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Context></Context></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<Localizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Missing attribute 'id' in 'Context' XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Merge()
        {
            // Prepare

            var localizer = new Localizer();
            var configB = CreateStream( "<I18N><Entry><Key>Simple Key 1</Key><Value lang='es'>XYZ</Value></Entry></I18N>" );
            localizer.LoadXML( GetConfigA(), "es-es" );

            // Execute

            localizer.LoadXML( configB, "es-es" );

            // Verify

            Assert.Equal( "XYZ", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clave simple 2", localizer.Localize( "Simple Key 2" ) );
        }

        [Fact]
        public void LoadXML_NoMerge()
        {
            // Prepare

            var localizer = new Localizer();
            var configB = CreateStream( "<I18N><Entry><Key>Simple Key 1</Key><Value lang='es'>XYZ</Value></Entry></I18N>" );
            localizer.LoadXML( GetConfigA(), "es-es" );

            // Execute

            localizer.LoadXML( configB, "es-es", false );

            // Verify

            Assert.Equal( "XYZ", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Simple Key 2", localizer.Localize( "Simple Key 2" ) );
        }

        [Fact]
        public void LoadXML_File()
        {
            // Prepare

            var tempFileName = Path.GetTempFileName();

            try
            {
                using( var tempFile = File.Create( tempFileName ) )
                {
                    GetConfigA().CopyTo( tempFile );
                }

                var localizer = new Localizer();

                // Execute

                localizer.LoadXML( tempFileName, "es-es" );

                // Verify

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
            }
            finally
            {
                // Cleanup

                File.Delete( tempFileName );
            }
        }

        [Fact]
        public void LoadXML_EmbeddedResource_PartialName()
        {
            // Prepare

            var localizer = new Localizer();

            // Execute

            localizer.LoadXML( typeof( LocalizerTest ).Assembly, "Resources.I18N.xml", "es-es" );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
        }

        [Fact]
        public void LoadXML_EmbeddedResource_FullName()
        {
            // Prepare

            var localizer = new Localizer();

            // Execute

            localizer.LoadXML( typeof( LocalizerTest ).Assembly, "I18N.DotNet.Test.Resources.I18N.xml", "es-es" );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
        }

        [Fact]
        public void LoadXML_EmbeddedResource_NotExisting()
        {
            // Prepare

            var localizer = new Localizer();

            // Execute

            var ex = Assert.Throws<InvalidOperationException>( () => localizer.LoadXML( typeof( LocalizerTest ).Assembly, "NotExisting.xml" ) );

            // Verify

            Assert.Contains( "Cannot find resource", ex.Message );
        }
    }
}
