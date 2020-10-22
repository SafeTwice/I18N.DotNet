using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace I18N.Tool
{
    static class InputFileParser
    {
        public static void ParseFile( string filepath, IEnumerable<string> extraFunctions, Dictionary<string, List<string>> keyMatches )
        {
            var text = File.ReadAllText( filepath );

            var lineStartIndexes = GetLineStartIndexes( text );

            ParseText( filepath, text, lineStartIndexes, GetLocatorRegex( extraFunctions ), keyMatches );
        }

        private static void ParseText( string filepath, string text, List<int> lineStartIndexes, Regex locatorRegex, Dictionary<string, List<string>> keyMatches )
        {
            foreach( Match match in locatorRegex.Matches( text ) )
            {
                string key = match.Groups[2].Value;

                if( match.Groups[1].Length > 0 )
                {
                    key = ConvertToOrdinalFormat( key );
                }

                List<string> keyInfoList;
                if( !keyMatches.TryGetValue( key, out keyInfoList ) )
                {
                    keyInfoList = new List<string>();

                    keyMatches.Add( key, keyInfoList );
                }

                int line = GetLine( match.Index, lineStartIndexes );
                keyInfoList.Add( $"{filepath} @ {line}" );
            }
        }

        private static Regex GetLocatorRegex( IEnumerable<string> extraFunctions )
        {
            string functionExpr = "Localize(?:Format)?";
            if( extraFunctions != null )
            {
                foreach( var functionName in extraFunctions )
                {
                    functionExpr += "|" + Regex.Escape( functionName );
                }
            }

            return new Regex( $"(?:{functionExpr})\\s*\\(\\s*(\\$?)\"((?:[^\"\\\\]|\\\\.)*)\"", RegexOptions.Multiline );
        }

        private static List<int> GetLineStartIndexes( string text )
        {
            var lineStartIndexes = new List<int>();

            var newLineRegex = new Regex( @"\n", RegexOptions.Multiline );

            foreach( Match match in newLineRegex.Matches( text ) )
            {
                lineStartIndexes.Add( match.Index );
            }

            return lineStartIndexes;
        }

        private static int GetLine( int index, List<int> lineStartIndexes )
        {
            return lineStartIndexes.Count( lineStartIndex => ( lineStartIndex < index ) ) + 1;
        }

        private static string ConvertToOrdinalFormat( string format )
        {
            int placeholderIndex = 0;
            var placeholderRegex = new Regex( @"{[^:}]+(:[^}]+)?}" );

            var x = placeholderRegex.Matches( format );

            return placeholderRegex.Replace( format, new MatchEvaluator( new Func<Match, string>( m => $"{{{placeholderIndex++}{m.Groups[1]}}}" ) ) );
        }
    }
}
