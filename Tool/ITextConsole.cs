/// @file
/// @copyright  Copyright (c) 2022-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

namespace I18N.DotNet.Tool
{
    public interface ITextConsole
    {
        void WriteLine( string text, bool error = false );
    }
}