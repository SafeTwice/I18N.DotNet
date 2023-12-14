/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

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
        public void Localize_String()
        {
            Assert.Equal( "Test", GlobalLocalizer.Localize( "Test" ) );
        }

        [Fact]
        public void Localize_Interpolated()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", GlobalLocalizer.Localize( $"Test {i}" ) );
        }

        [Fact]
        public void Localize_Format()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", GlobalLocalizer.LocalizeFormat( "Test {0}", i ) );
        }

        [Fact]
        public void Localize_Multiple()
        {
            Assert.Equal( new string[] { "ABC", "123" }, GlobalLocalizer.Localize( new string[] { "ABC", "123" } ) );
        }

        [Fact]
        public void Context()
        {
            Assert.NotNull( GlobalLocalizer.Context( "Context 1" ) );
        }
    }
}
