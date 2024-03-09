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
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "es-ES" );

            try
            {
                // Execute

                var localizer = new AutoLoadLocalizer( assembly: null );

                // Verify

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );
                Assert.Equal( "Clave de formato: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
                Assert.Equal( new string[] { "Clave simple 1", "Clave simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
                Assert.Equal( "Clave de formato: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

                Assert.Equal( "Clave simple 3 en contexto L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
                Assert.Equal( "Clave simple 3 en contexto L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );
            }
            finally
            {
                // Cleanup

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }

        [Fact]
        public void Constructor_PublicNonDefaultParameters()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr-FR" );

            try
            {
                // Execute

                var localizer = new AutoLoadLocalizer( "I18N.DotNet.Test.Resources.I18N.xml", typeof( AutoLoadLocalizerTest ).Assembly );

                // Verify

                Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
                Assert.Equal( "Clef de format: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
                Assert.Equal( new string[] { "Clef simple 1", "Clef simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
                Assert.Equal( "Clef de format: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

                Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
                Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );
            }
            finally
            {
                // Cleanup

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }

        [Fact]
        public void Constructor_Internal()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "es-ES" );

            try
            {
                // Execute

                var localizer = new AutoLoadLocalizer();

                // Verify

                Assert.Equal( "Simple Key 1", localizer.Localize( "Simple Key 1" ) );
                Assert.Equal( "Format Key: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
                Assert.Equal( new string[] { "Simple Key 1", "Simple Key 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
                Assert.Equal( "Format Key: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

                Assert.Equal( "Simple Key 3", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
                Assert.Equal( "Simple Key 3", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );
            }
            finally
            {
                // Cleanup

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }

        [Fact]
        public void Load_Language()
        {
            // Prepare

            const string targetLanguage = "fr";

            var localizer = new AutoLoadLocalizer( assembly: null );

            // Execute

            localizer.Load( targetLanguage );

            // Verify

            Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clef de format: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
            Assert.Equal( new string[] { "Clef simple 1", "Clef simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
            Assert.Equal( "Clef de format: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );
        }

        [Fact]
        public void Load_Culture()
        {
            // Prepare

            var targetCulture = CultureInfo.GetCultureInfo( "fr-FR" );

            var localizer = new AutoLoadLocalizer( assembly: null );

            // Execute

            localizer.Load( targetCulture );

            // Verify

            Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
            Assert.Equal( targetCulture, localizer.TargetCulture );

            Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clef de format: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
            Assert.Equal( new string[] { "Clef simple 1", "Clef simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
            Assert.Equal( "Clef de format: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );
        }

        [Fact]
        public void LoadXML_FromStream_Language()
        {
            // Prepare

            const string targetLanguage = "es";

            var localizer = new AutoLoadLocalizer();

            // Execute

            localizer.LoadXML( GetI18NConfig(), targetLanguage );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );
        }

        [Fact]
        public void LoadXML_FromStream_Culture()
        {
            // Prepare

            var targetCulture = CultureInfo.GetCultureInfo( "es-AR" );

            var localizer = new AutoLoadLocalizer();

            // Execute

            localizer.LoadXML( GetI18NConfig(), targetCulture );

            // Verify

            Assert.Equal( "Che, viste, clave resimple 1. Obvio", localizer.Localize( "Simple Key 1" ) );

            Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
            Assert.Equal( targetCulture, localizer.TargetCulture );
        }

        [Fact]
        public void LoadXML_FromStream_Merge()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "fr" );

            try
            {
                var localizer = new AutoLoadLocalizer();

                // Execute

                localizer.LoadXML( GetI18NConfig(), true );

                // Verify

                Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );
            }
            finally
            {
                // Cleanup

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }

        [Fact]
        public void LoadXML_FromFile_Language()
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

                var localizer = new AutoLoadLocalizer();

                // Execute

                localizer.LoadXML( tempFileName, targetLanguage );

                // Verify

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

                Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
                Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );
            }
            finally
            {
                // Cleanup

                File.Delete( tempFileName );
            }
        }

        [Fact]
        public void LoadXML_FromFile_Culture()
        {
            // Prepare

            var tempFileName = Path.GetTempFileName();

            try
            {
                var targetCulture = CultureInfo.GetCultureInfo( "es" );

                using( var tempFile = File.Create( tempFileName ) )
                {
                    GetI18NConfig().CopyTo( tempFile );
                }

                var localizer = new AutoLoadLocalizer();

                // Execute

                localizer.LoadXML( tempFileName, targetCulture );

                // Verify

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

                Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( targetCulture, localizer.TargetCulture );
            }
            finally
            {
                // Cleanup

                File.Delete( tempFileName );
            }
        }

        [Fact]
        public void LoadXML_FromFile_Merge()
        {
            // Prepare

            var tempFileName = Path.GetTempFileName();

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "es-ES" );

            try
            {
                using( var tempFile = File.Create( tempFileName ) )
                {
                    GetI18NConfig().CopyTo( tempFile );
                }

                var localizer = new AutoLoadLocalizer();

                // Execute

                localizer.LoadXML( tempFileName, true );

                // Verify

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );
            }
            finally
            {
                // Cleanup

                File.Delete( tempFileName );

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }

        [Fact]
        public void LoadXML_FromXDocument_Language()
        {
            // Prepare

            const string targetLanguage = "es";

            var localizer = new AutoLoadLocalizer();

            var doc = XDocument.Load( GetI18NConfig() );

            // Execute

            localizer.LoadXML( doc, targetLanguage );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );
        }

        [Fact]
        public void LoadXML_FromXDocument_Culture()
        {
            // Prepare

            var targetCulture = CultureInfo.GetCultureInfo( "es" );

            var localizer = new AutoLoadLocalizer();

            var doc = XDocument.Load( GetI18NConfig() );

            // Execute

            localizer.LoadXML( doc, targetCulture );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
            Assert.Equal( targetCulture, localizer.TargetCulture );
        }

        [Fact]
        public void LoadXML_FromXDocument_Merge()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "es-ES" );

            try
            {
                var localizer = new AutoLoadLocalizer();

                var doc = XDocument.Load( GetI18NConfig() );

                // Execute

                localizer.LoadXML( doc, false );

                // Verify

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );
            }
            finally
            {
                // Cleanup

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }

        [Fact]
        public void LoadXML_FromEmbeddedResource_Language()
        {
            // Prepare

            const string targetLanguage = "es";

            var localizer = new AutoLoadLocalizer();

            // Execute

            localizer.LoadXML( typeof( AutoLoadLocalizerTest ).Assembly, "Resources.I18N.xml", targetLanguage );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            Assert.Equal( targetLanguage, localizer.TargetLanguage, true );
            Assert.Equal( targetLanguage, localizer.TargetCulture.Name, true );
        }

        [Fact]
        public void LoadXML_FromEmbeddedResource_Culture()
        {
            // Prepare

            var targetCulture = CultureInfo.GetCultureInfo( "es" );

            var localizer = new AutoLoadLocalizer();

            // Execute

            localizer.LoadXML( typeof( AutoLoadLocalizerTest ).Assembly, "Resources.I18N.xml", targetCulture );

            // Verify

            Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

            Assert.Equal( targetCulture.Name, localizer.TargetLanguage, true );
            Assert.Equal( targetCulture, localizer.TargetCulture );
        }

        [Fact]
        public void LoadXML_FromEmbeddedResource_Merge()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo( "es-ES" );

            try
            {
                var localizer = new AutoLoadLocalizer();

                // Execute

                localizer.LoadXML( typeof( AutoLoadLocalizerTest ).Assembly, "Resources.I18N.xml", true );

                // Verify

                Assert.Equal( "Clave simple 1", localizer.Localize( "Simple Key 1" ) );

                Assert.Equal( CultureInfo.CurrentUICulture.Name, localizer.TargetLanguage, true );
                Assert.Equal( CultureInfo.CurrentUICulture, localizer.TargetCulture );
            }
            finally
            {
                // Cleanup

                CultureInfo.CurrentUICulture = oldUICulture;
            }
        }
    }
}
