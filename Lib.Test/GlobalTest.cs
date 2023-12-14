/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using Xunit;

namespace I18N.DotNet.Test
{
    public class GlobalTest
    {
        [Fact]
        public void Localizer()
        {
            // Verify

            Assert.NotNull( Global.Localizer );
        }

        [Fact]
        public void Localize_String()
        {
            Assert.Equal( "Test", Global.Localize( "Test" ) );
        }

        [Fact]
        public void Localize_Interpolated()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", Global.Localize( $"Test {i}" ) );
        }

        [Fact]
        public void Localize_Format()
        {
            int i = 1234;

            Assert.Equal( "Test 1234", Global.LocalizeFormat( "Test {0}", i ) );
        }

        [Fact]
        public void Localize_Multiple()
        {
            Assert.Equal( new string[] { "ABC", "123" }, Global.Localize( new string[] { "ABC", "123" } ) );
        }

        [Fact]
        public void Context()
        {
            Assert.NotNull( Global.Context( "Context 1" ) );
        }
    }
}
