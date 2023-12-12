/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Globalization;
using Xunit;

namespace I18N.DotNet.Test
{
    public class AutoLoadLocalizerTest
    {
        [Fact]
        public void Default()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "es-ES" );

            var localizer = new AutoLoadLocalizer();

            // Execute & Verify

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
        public void ExplicitConstructorParameters()
        {
            // Prepare

            var oldUICulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture( "fr-FR" );

            var localizer = new AutoLoadLocalizer( "I18N.DotNet.Test.Resources.I18N.xml", typeof( AutoLoadLocalizerTest ).Assembly );

            // Execute & Verify

            Assert.Equal( "Clef simple 1", localizer.Localize( "Simple Key 1" ) );
            Assert.Equal( "Clef de format: 0159", localizer.Localize( $"Format Key: {345:X4}" ) );
            Assert.Equal( new string[] { "Clef simple 1", "Clef simple 2" }, localizer.Localize( new string[] { "Simple Key 1", "Simple Key 2" } ) );
            Assert.Equal( "Clef de format: 002F", localizer.LocalizeFormat( "Format Key: {0:X4}", 47 ) );

            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( "Level1.Level2" ).Localize( "Simple Key 3" ) );
            Assert.Equal( "Clef simple 3 en contexte L2", localizer.Context( new string[] { "Level1", "Level2" } ).Localize( "Simple Key 3" ) );

            // Cleanup

            CultureInfo.CurrentUICulture = oldUICulture;
        }
    }
}
