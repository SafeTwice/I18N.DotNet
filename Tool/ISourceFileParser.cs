/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System.Collections.Generic;

namespace I18N.DotNet.Tool
{
    public interface ISourceFileParser
    {
        void ParseFile( string filepath, IEnumerable<string> extraFunctions, Context rootContext );
    }
}