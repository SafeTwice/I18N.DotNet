/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace I18N.Tool
{
    public class SourceFileParser : ISourceFileParser
    {
        public void ParseFile( string filepath, IEnumerable<string> extraFunctions, Context rootContext, ITextConsole textConsole )
        {
            var text = File.ReadAllText( filepath );

            var syntaxTree = CSharpSyntaxTree.ParseText( text );

            var localizerRegex = GetLocalizerRegex( extraFunctions );

            var relativeFilePath = AbsoluteToRelativePath( filepath );

            ParseSyntaxTree( syntaxTree, relativeFilePath, localizerRegex, rootContext, textConsole );
        }

        private static void ParseSyntaxTree( SyntaxTree tree, string filepath, Regex localizerRegex, Context rootContext, ITextConsole textConsole )
        {
            var root = tree.GetCompilationUnitRoot();

            var localizerMatches = from localizerCall in root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                   where ( localizerCall.Expression is IdentifierNameSyntax id && localizerRegex.IsMatch( id.Identifier.ValueText ) ) ||
                                         ( localizerCall.Expression is MemberAccessExpressionSyntax method && localizerRegex.IsMatch( method.Name.Identifier.ValueText ) )
                                   where ( localizerCall.ArgumentList.Arguments.Count > 0 ) &&
                                         ( localizerCall.ArgumentList.Arguments.First().Expression != null )
                                   select localizerCall;

            foreach( var match in localizerMatches )
            {
                var firstArgument = match.ArgumentList.Arguments.First().Expression;

                string key;

                int line = ( match.Expression.GetLocation().GetLineSpan().StartLinePosition.Line + 1 );

                if( firstArgument.IsKind( SyntaxKind.InterpolatedStringExpression ) )
                {
                    var interpolatedString = firstArgument as InterpolatedStringExpressionSyntax;
                    key = ConvertToKey( interpolatedString );
                }
                else if( firstArgument.IsKind( SyntaxKind.StringLiteralExpression ) )
                {
                    var stringExpr = firstArgument as LiteralExpressionSyntax;
                    key = stringExpr.Token.ValueText;
                }
                else
                {
                    textConsole.WriteLine( $"WARNING: Non-literal localization method argument at line {line} of file '{filepath}'" );
                    continue;
                }

                key = EscapeString( key );

                var context = GetContext( match, rootContext );
                context.AddKey( key, $"{filepath} @ {line}" );
            }
        }

        private static Context GetContext( InvocationExpressionSyntax localizerCall, Context rootContext )
        {
            List<string> contextStack = new();

            UpdateContextStack( localizerCall, contextStack );

            return rootContext.GetContext( contextStack );
        }

        private static void UpdateContextStack( InvocationExpressionSyntax invocation, List<string> contextStack )
        {
            if( invocation.Expression is MemberAccessExpressionSyntax candidateContextObject )
            {
                if( candidateContextObject.Expression is InvocationExpressionSyntax candidateContextInvocation )
                {
                    string calledMethodName = null;
                    bool isNested = false;

                    if( candidateContextInvocation.Expression is IdentifierNameSyntax calledObjectId )
                    {
                        calledMethodName = calledObjectId.Identifier.ValueText;
                    }
                    else if( candidateContextInvocation.Expression is MemberAccessExpressionSyntax calledObjectMethod )
                    {
                        calledMethodName = calledObjectMethod.Name.Identifier.ValueText;
                        isNested = true;
                    }

                    if( ( calledMethodName == "Context" ) &&
                        ( candidateContextInvocation.ArgumentList.Arguments.Count == 1 ) )
                    {
                        var argument = candidateContextInvocation.ArgumentList.Arguments.First().Expression;
                        if( argument.IsKind( SyntaxKind.StringLiteralExpression ) )
                        {
                            if( isNested )
                            {
                                UpdateContextStack( candidateContextInvocation, contextStack );
                            }

                            var contextName = ( argument as LiteralExpressionSyntax ).Token.ValueText;

                            foreach( var splitContextName in contextName.Split( '.' ) )
                            {
                                contextStack.Add( splitContextName );
                            }
                        }
                    }
                }
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

        private static string ConvertToKey( InterpolatedStringExpressionSyntax interpolatedString )
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
                string result = payload;
                switch( payload )
                {
                    case "\n":
                        result = "\\n";
                        break;

                    case "\r":
                        result = "\\r";
                        break;

                    case "\f":
                        result = "\\f";
                        break;

                    case "\t":
                        result = "\\t";
                        break;

                    case "\v":
                        result = "\\v";
                        break;

                    case "\b":
                        result = "\\b";
                        break;

                    case "\\":
                        result = "\\\\";
                        break;
                }
                return result;
            } );
        }
    }
}
