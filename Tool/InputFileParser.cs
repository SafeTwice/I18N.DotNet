using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

            var syntaxTree = CSharpSyntaxTree.ParseText( text );

            var localizerRegex = GetLocalizerRegex( extraFunctions );

            var relativeFilePath = AbsoluteToRelativePath( filepath );

            ParseSyntaxTree( syntaxTree, relativeFilePath, localizerRegex, keyMatches );
        }

        private static void ParseSyntaxTree( SyntaxTree tree, string filepath, Regex localizerRegex, Dictionary<string, List<string>> keyMatches )
        {
            var root = tree.GetCompilationUnitRoot();

            var localizerMatches = from localizerCall in root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                   where ( localizerCall.Expression is IdentifierNameSyntax id && localizerRegex.IsMatch( id.Identifier.ValueText ) ) ||
                                         ( localizerCall.Expression is MemberAccessExpressionSyntax method && localizerRegex.IsMatch( method.Name.Identifier.ValueText ) )
                                   let firstArgument = localizerCall.ArgumentList.Arguments.First()?.Expression
                                   where firstArgument != null
                                   where firstArgument.IsKind( SyntaxKind.StringLiteralExpression ) || 
                                         firstArgument.IsKind( SyntaxKind.InterpolatedStringExpression )
                                   select localizerCall;

            foreach( var match in localizerMatches )
            {
                var firstArgument = match.ArgumentList.Arguments.First().Expression;

                string key;

                if( firstArgument.IsKind( SyntaxKind.InterpolatedStringExpression ) )
                {
                    var interpolatedString = firstArgument as InterpolatedStringExpressionSyntax;
                    key = ConvertToOrdinalFormat( interpolatedString );
                }
                else
                {
                    var stringExpr = firstArgument as LiteralExpressionSyntax;
                    key = stringExpr.Token.ValueText;
                }

                key = EscapeString( key );

                List<string> keyInfoList;
                if( !keyMatches.TryGetValue( key, out keyInfoList ) )
                {
                    keyInfoList = new List<string>();

                    keyMatches.Add( key, keyInfoList );
                }

                int line = ( match.Expression.GetLocation().GetLineSpan().StartLinePosition.Line + 1 );
                keyInfoList.Add( $"{filepath} @ {line}" );
            }
        }

        private static string AbsoluteToRelativePath( string filePath )
        {
            return Path.GetRelativePath( Directory.GetCurrentDirectory() + "\\", filePath );
        }

        private static Regex GetLocalizerRegex( IEnumerable<string> extraFunctions )
        {
            string functionExpr = "Localize(?:Format)?";
            if( extraFunctions != null )
            {
                foreach( var functionName in extraFunctions )
                {
                    functionExpr += "|" + Regex.Escape( functionName );
                }
            }

            return new Regex( functionExpr, RegexOptions.Singleline );
        }

        private static string ConvertToOrdinalFormat( InterpolatedStringExpressionSyntax interpolatedString )
        {
            string result = "";
            int placeholderIndex = 0;

            foreach( var item in interpolatedString.Contents )
            {
                if( item.IsKind( SyntaxKind.InterpolatedStringText ) )
                {
                    var text = item as InterpolatedStringTextSyntax;
                    result += text.TextToken.ValueText;
                }
                else
                {
                    var interpolation = item as InterpolationSyntax;
                    result += $"{{{placeholderIndex}{interpolation.AlignmentClause?.ToString()}{interpolation.FormatClause?.ToString()}}}";
                    placeholderIndex++;
                }
            }

            return result;
        }

        private static string EscapeString( string text )
        {
            return Regex.Replace( text, @"([\n\r\f\t\v\b\\])", m =>
            {
                var payload = m.Groups[ 1 ].Value;
                switch( payload )
                {
                    case "\n":
                        return "\\n";
                    case "\r":
                        return "\\r";
                    case "\f":
                        return "\\f";
                    case "\t":
                        return "\\t";
                    case "\v":
                        return "\\v";
                    case "\b":
                        return "\\b";
                    case "\\":
                        return "\\\\";
                    default:
                        return payload;
                }
            } );
        }
    }
}
