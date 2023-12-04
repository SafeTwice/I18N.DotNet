/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace I18N.DotNet.Tool
{
    public interface II18NFile
    {
        // Common
        public void LoadFromFile( string filepath );

        // Generation

        void DeleteFoundingComments();
        void CreateEntries( Context rootContext );
        void CreateDeprecationComments();
        void WriteToFile( string filepath );

        // Analysis

        IEnumerable<(int line, string context, string? key)> GetDeprecatedEntries( Regex[] includeContexts, Regex[] excludeContexts );
        IEnumerable<(int line, string context, string? key)> GetNoTranslationEntries( string[] languages, Regex[] includeContexts, Regex[] excludeContexts );
    }
}