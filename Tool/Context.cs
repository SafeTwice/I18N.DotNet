/**
 * @file
 * @copyright  Copyright (c) 2022 SafeTwice S.L. All rights reserved.
 * @license    See LICENSE.txt
 */

using System.Collections.Generic;

namespace I18N.DotNet.Tool
{
    public class Context
    {
        public Dictionary<string, List<string>> KeyMatches = new();

        public Dictionary<string, Context> NestedContexts = new();

        public void AddKey( string key, string keyInfo )
        {
            List<string> keyInfoList;
            if( !KeyMatches.TryGetValue( key, out keyInfoList ) )
            {
                keyInfoList = new List<string>();

                KeyMatches.Add( key, keyInfoList );
            }

            keyInfoList.Add( keyInfo );
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
