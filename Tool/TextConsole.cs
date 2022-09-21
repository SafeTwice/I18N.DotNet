/// @file
/// @copyright  Copyright (c) 2022 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;

namespace I18N.Tool
{
    public class TextConsole : ITextConsole
    {
        public void WriteLine( string text )
        {
            Console.WriteLine( text );
        }
    }
}
