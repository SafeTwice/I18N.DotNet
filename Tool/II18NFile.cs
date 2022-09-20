/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace I18N.Tool
{
    public interface II18NFile
    {
        // Common
        public void Load( string filepath );

        // Generation

        void DeleteFoundingComments();
        void CreateEntries( Context rootContext );
        void CreateDeprecationComments();
        void WriteToFile( string filepath );

        // Analysis

        IEnumerable<(int line, string context, string key)> GetDeprecatedEntries( Regex[] includeContexts, Regex[] excludeContexts );
        IEnumerable<(int line, string context, string key)> GetNoTranslationEntries( string[] languages, Regex[] includeContexts, Regex[] excludeContexts );
    }
}