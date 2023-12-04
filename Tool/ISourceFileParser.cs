/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Collections.Generic;

namespace I18N.DotNet.Tool
{
    public interface ISourceFileParser
    {
        void ParseFile( string filepath, IEnumerable<string> extraFunctions, Context rootContext );
    }
}