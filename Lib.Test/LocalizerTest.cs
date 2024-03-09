/// @file
/// @copyright  Copyright (c) 2020-2024 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Xunit;
using static I18N.DotNet.Test.TestHelpers;

#pragma warning disable CA1861, CA1859

namespace I18N.DotNet.Test
{
    public class LocalizerTest
    {
        [Fact]
        public void DefaultConstructor()
        {
            // Execute

            var localizer = new Localizer();

            // Verify

            Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
            Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );

            Assert.Equal( "Simple Key 1", localizer.Localize( "Simple Key 1" ) );
        }

        [Fact]
        public void Localize_BaseLanguage_PlainString()
        {
            // Prepare

            const string targetLanguage = "en-us";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            string[] testData =
            {
                "Non-existant Key",
                "Simple Key 1"
            };

            foreach( var key in testData )
            {
                Assert.Equal( key, localizer.Localize( key ) );
            }
        }

        [Fact]
        public void Localize_SpecificLanguage_PlainString()
        {
            // Prepare

            const string targetLanguage = "fr";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            (string key, string value)[] testData =
{
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clef simple 1" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Localize( key ) );
            }
        }

        [Fact]
        public void Localize_PrimaryOrVariantLanguageMatch()
        {
            // Prepare

            const string targetLanguage = "es-AR";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            (string key, string value)[] testData =
{
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Che, viste, clave resimple 1. Obvio" ),
                ( "Simple Key 2", "Clave simple 2" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Localize( key ) );
            }
        }

        [Fact]
        public void Localize_BaseLanguage_Interpolated()
        {
            // Prepare

            const string targetLanguage = "en-us";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            var i = 12.34;

            (FormattableString key, string value)[] testData =
{
                ( $"Format Key: {i:F3}", "Format Key: 12.340" ),
                ( $"Non-existent Format: {i}", "Non-existent Format: 12.34" ),
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Localize( key ) );
            }
        }

        [Fact]
        public void Localize_SpecificLanguage_Interpolated()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            var i = 884.2398878;

            (FormattableString key, string value)[] testData =
{
                ( $"Format Key: {i:F3}", "Clave de formato: 884,2399" ),
                ( $"Non-existent Format: {i}", "Non-existent Format: 884,2398878" ),
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Localize( key ) );
            }
        }

        [Fact]
        public void Localize_EscapedChars()
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "es-es" );

            // Execute & Verify

            Assert.Equal( "Escapado:\n\r\f&\t\v\b\\n\xABC", localizer.Localize( "Escaped:\n\r\f&\t\v\b\\n\xABC" ) );
        }

        [Fact]
        public void Localize_MultipleKeys()
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "fr-fr" );

            // Execute & Verify

            Assert.Equal( new string[] { "Non-existant Key", "Clef simple 1" }, localizer.Localize( new string[] { "Non-existant Key", "Simple Key 1" } ) );
        }

        [Fact]
        public void Context_ExistingContextL1()
        {
            // Prepare

            const string targetLanguage = "fr-fr";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.Context( "Level1" ).TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.Context( "Level1" ).TargetCulture.Name, true );

            (string key, string value)[] testData =
            {
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clef simple 1" ),
                ( "Simple Key 2", "Clef simple 2 en contexte L1" ),
                ( "Simple Key 3", "Clef simple 3" ),
                ( "Simple Key 4", "Clef simple 4" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Context( "Level1" ).Localize( key ) );
            }
        }

        [Fact]
        public void Context_ExistingContextL2A()
        {
            // Prepare

            const string targetLanguage = "fr-fr";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.Context( "Level1.Level2" ).TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.Context( "Level1.Level2" ).TargetCulture.Name, true );

            (string key, string value)[] testData =
            {
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clef simple 1 en contexte L2" ),
                ( "Simple Key 2", "Clef simple 2 en contexte L1" ),
                ( "Simple Key 3", "Clef simple 3 en contexte L2" ),
                ( "Simple Key 4", "Clef simple 4" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Context( "Level1.Level2" ).Localize( key ) );
            }
        }

        [Fact]
        public void Context_ExistingContextL2B()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.Context( "Level1.Level2" ).TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.Context( "Level1.Level2" ).TargetCulture.Name, true );

            (string key, string value)[] testData =
            {
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clave simple 1 en contexto L2" ),
                ( "Simple Key 2", "Clave simple 2 en contexto L1" ),
                ( "Simple Key 3", "Clave simple 3" ),
                ( "Simple Key 4", "Clave simple 4" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Context( "Level1.Level2" ).Localize( key ) );
            }
        }

        [Fact]
        public void Context_ExistingContextL2B_Split()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            var contextId = new string[] { "Level1", "Level2" };

            Assert.Equal( targetLanguage, localizer.Context( contextId ).TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.Context( contextId ).TargetCulture.Name, true );

            (string key, string value)[] testData =
            {
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clave simple 1 en contexto L2" ),
                ( "Simple Key 2", "Clave simple 2 en contexto L1" ),
                ( "Simple Key 3", "Clave simple 3" ),
                ( "Simple Key 4", "Clave simple 4" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Context( contextId ).Localize( key ) );
            }
        }

        [Fact]
        public void Context_NonExistingContext()
        {
            // Prepare

            const string targetLanguage = "fr-fr";

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute & Verify

            Assert.Equal( targetLanguage, localizer.Context( "LevelX" ).TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.Context( "LevelX" ).TargetCulture.Name, true );

            (string key, string value)[] testData =
            {
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clef simple 1" ),
                ( "Simple Key 2", "Clef simple 2" ),
                ( "Simple Key 3", "Clef simple 3" ),
                ( "Simple Key 4", "Clef simple 4" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Context( "LevelX" ).Localize( key ) );
            }
        }

        [Fact]
        public void Context_Interpolated()
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "es-es" );

            // Execute & Verify

            int i = 1234;
            (FormattableString key, string value)[] testData =
{
                ( $"Format Key: {i:F3}", "Clave de formato: 1234,0000" ),
                ( $"Non-existent Format: {i}", "Non-existent Format: 1234" ),
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, localizer.Context( "Level1" ).Localize( key ) );
            }
        }

        [Fact]
        public void ILocalizer_Context_Single()
        {
            // Prepare

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "fr-fr" );

            ILocalizer ilocalizer = localizer;

            // Execute & Verify

            (string key, string value)[] testData =
            {
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clef simple 1" ),
                ( "Simple Key 2", "Clef simple 2 en contexte L1" ),
                ( "Simple Key 3", "Clef simple 3" ),
                ( "Simple Key 4", "Clef simple 4" )
            };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, ilocalizer.Context( "Level1" ).Localize( key ) );
            }
        }

        [Fact]
        public void ILocalizer_Context_Split()
        {
            // Prepare

            var localizer = new Localizer( );
            localizer.LoadXML( GetI18NConfig(), "es-es" );

            ILocalizer ilocalizer = localizer;

            // Execute & Verify

            (string key, string value)[] testData =
            {
                ( "Non-existant Key", "Non-existant Key" ),
                ( "Simple Key 1", "Clave simple 1 en contexto L2" ),
                ( "Simple Key 2", "Clave simple 2 en contexto L1" ),
                ( "Simple Key 3", "Clave simple 3" ),
                ( "Simple Key 4", "Clave simple 4" )
            };

            var context = new string[] { "Level1", "Level2" };

            foreach( var (key, value) in testData )
            {
                Assert.Equal( value, ilocalizer.Context( context ).Localize( key ) );
            }
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

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Invalid XML root element", exception.Message );
        }

        [Fact]
        public void LoadXML_Root_InvalidChildElement()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><X/></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Invalid XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_NoKey()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Value lang='x'>X</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data, "x" ) );

            Assert.Contains( "Missing child 'Key' XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_TooManyKeys()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value lang='x'>X</Value><Key>Y</Key></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data, "x" ) );

            Assert.Contains( "Too many child 'Key' XML elements", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_TooManyValuesForSameLanguage()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value lang='x'>X</Value><Value lang='x'>Y</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data, "x" ) );

            Assert.Contains( "Too many child 'Value' XML elements with the same 'lang' attribute", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_TooManyValuesForSamePrimaryLanguage()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value lang='en'>X</Value><Value lang='en'>Y</Value><Value lang='en-gb'>Z</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data, "en-GB" ) );

            Assert.Contains( "Too many child 'Value' XML elements with the same 'lang' attribute", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_ValuesWithoutLanguage()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Key>X</Key><Value>X</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Missing attribute 'lang' in 'Value' XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Entry_InvalidChildElement()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Entry><Kay>X</Kay><Value lang='x'>X</Value></Entry></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data, "x" ) );

            Assert.Contains( "Invalid XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Context_NoId()
        {
            // Prepare

            var localizer = new Localizer();
            var data = CreateStream( "<I18N><Context></Context></I18N>" );

            // Execute & Verify

            var exception = Assert.Throws<ILoadableLocalizer.ParseException>( () => localizer.LoadXML( data ) );

            Assert.Contains( "Missing attribute 'id' in 'Context' XML element", exception.Message );
        }

        [Fact]
        public void LoadXML_Stream_Language()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var localizer = new Localizer();

            // Execute

            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            var formatArg = 12.5;

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clave simple 2", localizer.Localize( "Simple Key 2" ) );
            Assert.Equal( "Clave de formato: 12,5000", localizer.Localize( $"Format Key: {formatArg:F3}" ) );
        }

        [Fact]
        public void LoadXML_Stream_Culture()
        {
            // Prepare

            var targetCulture = CultureInfo.GetCultureInfo( "es" );

            var localizer = new Localizer();

            // Execute

            localizer.LoadXML( GetI18NConfig(), targetCulture );

            // Verify

            Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
            Assert.Equal( targetCulture, localizer.TargetCulture );

            var formatArg = 12.5;

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clave simple 2", localizer.Localize( "Simple Key 2" ) );
            Assert.Equal( "Clave de formato: 12,5000", localizer.Localize( $"Format Key: {formatArg:F3}" ) );
        }

        [Fact]
        public void LoadXML_Stream_DefaultCulture()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );

            try
            {
                var localizer = new Localizer();

                // Execute

                localizer.LoadXML( GetI18NConfig() );

                // Verify

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );

                var formatArg = 789.566;

                Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
                Assert.Equal( "Clef simple 2", localizer.Localize( "Simple Key 2" ) );
                Assert.Equal( "Clef de format: 789,6", localizer.Localize( $"Format Key: {formatArg:F3}" ) );
            }
            finally
            {
                // Cleanup

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }

        [Fact]
        public void LoadXML_Stream_Merge()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var localizer = new Localizer();
            var configB = CreateStream( "<I18N><Entry><Key>Simple Key 1</Key><Value lang='es'>XYZ</Value></Entry></I18N>" );
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Execute

            localizer.LoadXML( configB, true );

            // Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            Assert.Equal( "XYZ", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clave simple 2", localizer.Localize( "Simple Key 2" ) );
        }

        [Fact]
        public void LoadXML_Stream_NoMerge()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var localizer = new Localizer();
            var configB = CreateStream( "<I18N><Entry><Key>Simple Key 1</Key><Value lang='es'>XYZ</Value></Entry>" +
                                        "<Context id='Level1'><Entry><Key>Simple Key 2</Key><Value lang='es'>ABC</Value></Entry></Context></I18N>" );
            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            var contextL1 = localizer.Context( "Level1" );

            // Execute

            localizer.LoadXML( configB, false );

            // Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            Assert.Equal( "XYZ", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Simple Key 2", localizer.Localize( "Simple Key 2" ) );
            Assert.Equal( "ABC", contextL1.Localize( "Simple Key 2" ) );
        }

        [Fact]
        public void LoadXML_File_Language()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var tempFileName = Path.GetTempFileName();

            try
            {
                using( var tempFile = File.Create( tempFileName ) )
                {
                    GetI18NConfig().CopyTo( tempFile );
                }

                var localizer = new Localizer();

                // Execute

                localizer.LoadXML( tempFileName, targetLanguage );

                // Verify

                Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
                Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
            }
            finally
            {
                // Cleanup

                File.Delete( tempFileName );
            }
        }

        [Fact]
        public void LoadXML_File_Culture()
        {
            // Prepare

            var tempFileName = Path.GetTempFileName();

            try
            {
                var targetCulture = CultureInfo.GetCultureInfo( "es-es" );

                using( var tempFile = File.Create( tempFileName ) )
                {
                    GetI18NConfig().CopyTo( tempFile );
                }

                var localizer = new Localizer();

                // Execute

                localizer.LoadXML( tempFileName, targetCulture );

                // Verify

                Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( targetCulture, localizer.TargetCulture );

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
            }
            finally
            {
                // Cleanup

                File.Delete( tempFileName );
            }
        }

        [Fact]
        public void LoadXML_File_Merge()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var tempFileName = Path.GetTempFileName();

            try
            {
                using( var tempFile = File.Create( tempFileName ) )
                using( var writer = new StreamWriter( tempFile ) )
                {
                    writer.Write( "<I18N><Entry><Key>Simple Key 1</Key><Value lang='es'>XYZ</Value></Entry></I18N>" );
                }

                var localizer = new Localizer();
                localizer.LoadXML( GetI18NConfig(), targetLanguage );

                // Execute

                localizer.LoadXML( tempFileName, true );

                // Verify

                Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
                Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

                Assert.Equal( "XYZ", localizer.Localize( "Simple Key 1" ) );
                Assert.Equal( "Clave simple 2", localizer.Localize( "Simple Key 2" ) );
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

            const string targetLanguage = "es-es";

            var localizer = new Localizer();

            // Execute

            localizer.LoadXML( typeof( LocalizerTest ).Assembly, "Resources.I18N.xml", targetLanguage );

            // Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
        }

        [Fact]
        public void LoadXML_EmbeddedResource_FullName()
        {
            // Prepare

            const string targetLanguage = "es-es";

            var localizer = new Localizer();

            // Execute

            localizer.LoadXML( typeof( LocalizerTest ).Assembly, "I18N.DotNet.Test.Resources.I18N.xml", targetLanguage );

            // Verify

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
        }

        [Fact]
        public void LoadXML_EmbeddedResource_Culture()
        {
            // Prepare

            var targetCulture = CultureInfo.GetCultureInfo( "fr-CA" );

            var localizer = new Localizer();

            // Execute

            localizer.LoadXML( typeof( LocalizerTest ).Assembly, "Resources.I18N.xml", targetCulture );

            // Verify

            Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
            Assert.Equal( targetCulture, localizer.TargetCulture );

            Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
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
