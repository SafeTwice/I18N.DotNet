/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Xunit;
using static I18N.DotNet.Test.TestHelpers;

namespace I18N.DotNet.Test
{
    public class AutoLoadLocalizerTest
    {
        [Fact]
        public void Constructor_PublicDefault()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "es-ES" );

            // Execute

            var localizer = new AutoLoadLocalizer( assembly: null );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clave de formato: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
            Assert.Equal( new string[] { "Clave simple 1", "Clave simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
            Assert.Equal( "Clave de formato: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

            Assert.Equal( "Clave simple 3 en contexto L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
            Assert.Equal( "Clave simple 3 en contexto L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }

        [Fact]
        public void Constructor_PublicNonDefaultParameters()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "fr-FR" );

            // Execute

            var localizer = new AutoLoadLocalizer( "I18N.DotNet.Test.Resources.I18N.xml", typeof( AutoLoadLocalizerTest ).Assembly );

            // Verify

            Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clef de format: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
            Assert.Equal( new string[] { "Clef simple 1", "Clef simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
            Assert.Equal( "Clef de format: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }

        [Fact]
        public void Constructor_Internal()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "es-ES" );

            // Execute

            var localizer = new AutoLoadLocalizer();

            // Verify

            Assert.Equal( "Simple Key 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Format Key: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
            Assert.Equal( new string[] { "Simple Key 1", "Simple Key 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
            Assert.Equal( "Format Key: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

            Assert.Equal( "Simple Key 3", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
            Assert.Equal( "Simple Key 3", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }

        [Fact]
        public void Load()
        {
            // Prepare

            var localizer = new AutoLoadLocalizer( assembly: null );

            // Execute

            localizer.Load( "fr" );

            // Verify

            Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clef de format: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
            Assert.Equal( new string[] { "Clef simple 1", "Clef simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
            Assert.Equal( "Clef de format: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );
        }

        [Fact]
        public void LoadXML_FromStream()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "es-ES" );

            var localizer = new AutoLoadLocalizer();

            // Execute

            localizer.LoadXML( GetI18NConfig(), "es" );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }

        [Fact]
        public void LoadXML_FromFile()
        {
            // Prepare

            var tempFileName = Path.GetTempFileName();

            try
            {
                using( var tempFile = File.Create( tempFileName ) )
                {
                    GetI18NConfig().CopyTo( tempFile );
                }

                var localizer = new AutoLoadLocalizer();

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
        public void LoadXML_FromXDocument()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "es-ES" );

            var localizer = new AutoLoadLocalizer();

            var doc = XDocument.Load( GetI18NConfig() );

            // Execute

            localizer.LoadXML( doc, "es" );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }

        [Fact]
        public void LoadXML_FromEmbeddedResource()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "es-ES" );

            var localizer = new AutoLoadLocalizer();

            // Execute

            localizer.LoadXML( typeof( AutoLoadLocalizerTest ).Assembly, "Resources.I18N.xml", "es" );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }
    }
}
