/// @file
/// @copyright  Copyright (c) 2022-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Diagnostics.CodeAnalysis;

namespace I18N.DotNet.Tool
{
    [ExcludeFromCodeCoverage]
    public class TextConsole : ITextConsole
    {
        public void WriteLine( string text, bool error = false )
        {
            var writer = error ? Console.Error : Console.Out;
            writer.WriteLine( text );
        }
    }
}
