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

        public Dictionary<string, List<KeyInfo>> KeyMatches = new();

        public Dictionary<string, Context> NestedContexts = new();

        public void AddKey( string key, string file, int line )
        {
            List<KeyInfo> keyInfoList;
            if( !KeyMatches.TryGetValue( key, out keyInfoList ) )
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

            Context nestedContext;

            if( !NestedContexts.TryGetValue( contextStack[ index ], out nestedContext ) )
            {
                nestedContext = new();
                NestedContexts.Add( contextStack[ index ], nestedContext );
            }

            return nestedContext.GetContext( contextStack, ( index + 1 ) );
        }
    }
}
