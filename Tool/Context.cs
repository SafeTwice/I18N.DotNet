/// @file
/// @copyright  Copyright (c) 2022-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Collections.Generic;

namespace I18N.DotNet.Tool
{
    public class Context
    {
        public class KeyInfo
        {
            public string File { get; }
            public int Line { get; }

            public KeyInfo( string file, int line )
            {
                File = file;
                Line = line;
            }
        }

        public Dictionary<string, List<KeyInfo>> KeyMatches { get; } = new();

        public Dictionary<string, Context> NestedContexts { get; } = new();

        public void AddKey( string key, string file, int line )
        {
            if( !KeyMatches.TryGetValue( key, out var keyInfoList ) )
            {
                keyInfoList = new List<KeyInfo>();

                KeyMatches.Add( key, keyInfoList );
            }

            keyInfoList.Add( new KeyInfo( file, line ) );
        }

        public Context GetContext( List<string> contextStack )
        {
            return GetContext( contextStack, 0 );
        }

        private Context GetContext( List<string> contextStack, int index )
        {
            if( index >= contextStack.Count )
            {
                return this;
            }

            if( !NestedContexts.TryGetValue( contextStack[ index ], out var nestedContext ) )
            {
                nestedContext = new();
                NestedContexts.Add( contextStack[ index ], nestedContext );
            }

            return nestedContext.GetContext( contextStack, ( index + 1 ) );
        }
    }
}
