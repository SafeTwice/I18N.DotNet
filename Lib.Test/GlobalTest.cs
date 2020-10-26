/**
 * @file
 * @copyright  Copyright (c) 2020 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using Xunit;

namespace I18N.Net.Test
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
        public void Context()
        {
            int i = 1234;

            Assert.NotNull( Global.Context( "Context 1" ) );
        }
    }
}
