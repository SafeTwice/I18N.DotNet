/// @file
/// @copyright  Copyright (c) 2020-2025 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Linq;
using Xunit;

namespace I18N.DotNet.Test
{
    public class GlobalLocalizerTest
    {
        [Fact]
        public void Localizer()
        {
            // Verify

            Assert.NotNull( GlobalLocalizer.Localizer );
        }

        [Fact]
        public void Localize_PlainString()
        {
            Assert.Equal( "Test", GlobalLocalizer.Localize( "Test" ) );
        }

        [Fact]
        public void Localize_InterpolatedString()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", GlobalLocalizer.Localize( $"Test {i}" ) );
        }

        [Fact]
        public void Localize_MultipleStrings()
        {
            Assert.Equal( new string[] { "ABC", "123" }, GlobalLocalizer.Localize( new string[] { "ABC", "123" } ) );
        }

        [Fact]
        public void LocalizeFormat()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", GlobalLocalizer.LocalizeFormat( "Test {0}", i ) );
        }

        [Fact]
        public void GetLocalizable_PlainString()
        {
            Assert.Equal( "Test", GlobalLocalizer.GetLocalizable( "Test" ) );
        }

        [Fact]
        public void GetLocalizable_InterpolatedString()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", GlobalLocalizer.GetLocalizable( $"Test {i}" ) );
        }

        [Fact]
        public void GetLocalizables()
        {
            Assert.Equal( new string[] { "ABC", "123" }, GlobalLocalizer.GetLocalizables( new[] { "ABC", "123" } ).Select( l => l.Localized ) );
        }

        [Fact]
        public void GetLocalizableFormat()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", GlobalLocalizer.GetLocalizableFormat( "Test {0}", i ) );
        }

        [Fact]
        public void Context_Single()
        {
            Assert.NotNull( GlobalLocalizer.Context( "Context 1" ) );
        }

        [Fact]
        public void Context_Split()
        {
            Assert.NotNull( GlobalLocalizer.Context( new string[] { "Context 1", "Context2" } ) );
        }
    }
}
