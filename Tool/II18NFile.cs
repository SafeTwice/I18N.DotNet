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
        void PrepareForDeployment();
        void CreateEntries( Context rootContext, bool reportLines );
        void CreateDeprecationComments();
        void WriteToFile( string filepath );

        // Analysis

        IEnumerable<(int line, string context, string? key)> GetDeprecatedEntries( IEnumerable<Regex> includeContexts, IEnumerable<Regex> excludeContexts );
        IEnumerable<(int line, string context, string? key)> GetNoTranslationEntries( IEnumerable<string> requiredLanguages, IEnumerable<Regex> includeContexts, IEnumerable<Regex> excludeContexts );
        IEnumerable<(int line, string message, bool isError)> GetFileIssues();
    }
}