/// @file
/// @copyright  Copyright (c) 2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Linq;
using Xunit;
using static I18N.DotNet.Test.TestHelpers;

namespace I18N.DotNet.Test
{
    public class LocalizableTest
    {
        [Fact]
        public void Localizable_CreationAndDisposal()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "foo" );

            // Act

            var localizable = localizer.GetLocalizable( "Non-existant Key" );

            // Assert

            Assert.Same( localizable.Localizer, localizer );

            // Act

            localizable.Dispose();
        }

        [Fact]
        public void Localizable_PlainString()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "fr" );

            // Act

            var localizable1 = localizer.GetLocalizable( "Non-existant Key" );
            var localizable2 = localizer.GetLocalizable( "Simple Key 1" );

            // Act & Assert

            Assert.Equal( "Non-existant Key", localizable1.Localized );
            Assert.Equal( "Clef simple 1", localizable2.Localized );

            // Arrange

            localizer.LoadXML( GetI18NConfig(), "es" );

            // Act & Assert

            Assert.Equal( "Non-existant Key", localizable1.Localized );
            Assert.Equal( "Clave simple 1", localizable2.Localized );
        }

        [Fact]
        public void Localizable_InterpolatedString()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "fr" );

            var i = 884.2398878;

            // Act

            var localizable1 = localizer.GetLocalizable( $"Non-existent Format: {i}" );
            var localizable2 = localizer.GetLocalizable( $"Format Key: {i:F3}" );

            // Act & Assert

            Assert.Equal( "Non-existent Format: 884,2398878", localizable1.Localized );
            Assert.Equal( "Clef de format: 884,2", localizable2.Localized );

            // Arrange

            localizer.LoadXML( GetI18NConfig(), "es" );

            // Act & Assert

            Assert.Equal( "Non-existent Format: 884,2398878", localizable1.Localized );
            Assert.Equal( "Clave de formato: 884,2399", localizable2.Localized );
        }

        [Fact]
        public void Localizable_Format()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "fr" );

            var i = 884.2398878;

            // Act

            var localizable1 = localizer.GetLocalizableFormat( "Non-existent Format: {0}", i );
            var localizable2 = localizer.GetLocalizableFormat( "Format Key: {0:F3}", i );

            // Act & Assert

            Assert.Equal( "Non-existent Format: 884,2398878", localizable1.Localized );
            Assert.Equal( "Clef de format: 884,2", localizable2.Localized );

            // Arrange

            localizer.LoadXML( GetI18NConfig(), "es" );

            // Act & Assert

            Assert.Equal( "Non-existent Format: 884,2398878", localizable1.Localized );
            Assert.Equal( "Clave de formato: 884,2399", localizable2.Localized );
        }

        [Fact]
        public void Localizable_MultipleStrings()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "fr" );

            // Act

            var localizables = localizer.GetLocalizables( new[] { "Non-existant Key", "Simple Key 1" } ).ToArray();

            // Act & Assert

            Assert.Equal( "Non-existant Key", localizables[ 0 ].Localized );
            Assert.Equal( "Clef simple 1", localizables[ 1 ].Localized );

            // Arrange

            localizer.LoadXML( GetI18NConfig(), "es" );

            // Act & Assert

            Assert.Equal( "Non-existant Key", localizables[ 0 ].Localized );
            Assert.Equal( "Clave simple 1", localizables[ 1 ].Localized );
        }

        [Fact]
        public void Localizable_ImplicitConversionToString()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "es" );

            // Act

            var localizable = localizer.GetLocalizable( "Simple Key 1" );

            // Act & Assert

            Assert.Equal( "Clave simple 1", localizable );
        }

        [Fact]
        public void Localizable_ToString()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "es" );

            // Act

            var localizable = localizer.GetLocalizable( "Simple Key 1" );

            // Act & Assert

            Assert.Equal( "Clave simple 1", localizable.ToString() );
        }

        [Fact]
        public void Localizable_NotifyPropertyChanged()
        {
            // Arrange

            var localizer = new Localizer();
            localizer.LoadXML( GetI18NConfig(), "fr" );

            // Act

            var localizable = localizer.GetLocalizable( "Simple Key 1" );

            // Act & Assert

            Assert.Equal( "Clef simple 1", localizable.Localized );

            // Arrange

            string? localizableUpdatedProperty = null;
            localizable.PropertyChanged += ( _, e ) => localizableUpdatedProperty = e.PropertyName;

            Assert.Null( localizableUpdatedProperty );

            localizer.LoadXML( GetI18NConfig(), "es" );

            // Act & Assert

            Assert.Equal( nameof( Localizable.Localized ), localizableUpdatedProperty );

            Assert.Equal( "Clave simple 1", localizable.Localized );
        }
    }
}
