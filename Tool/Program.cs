/// @file
/// @copyright  Copyright (c) 2020-2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable IDE0042

namespace I18N.DotNet.Tool
{
    [Verb( "parse", HelpText = "Parse source files and generate a I18N development file." )]
    class ParseSourcesOptions
    {
        private const string DEFAULT_SOURCE_FILE_PATTERN = "*.cs";

        [ Option( 'S', "sources", Required = true, HelpText = "Sources directory paths." )]
        public IEnumerable<string> SourcesDirectories { get; set; } = new List<string>();

        [Option( 'o', "output", Required = true, HelpText = "Output file path." )]
        public string OutputFile { get; set; } = string.Empty;

        [Option( 'p', "file-pattern", Default = DEFAULT_SOURCE_FILE_PATTERN, HelpText = "Source files name pattern." )]
        public string SourceFilesPattern { get; set; } = DEFAULT_SOURCE_FILE_PATTERN;

        [Option( 'r', "recursive", Default = false, HelpText = "Scan in source directories recursively." )]
        public bool Recursive { get; set; }

        [Option( 'k', "preserve-foundings", Default = false, HelpText = "Preserve founding comments in output file." )]
        public bool PreserveFoundingComments { get; set; }

        [Option( 'd', "mark-deprecated", Default = false, HelpText = "Mark deprecated entries." )]
        public bool MarkDeprecated { get; set; }

        [Option( 'l', "with-lines", Default = false, HelpText = "Include line numbers in founding comments." )]
        public bool LineIndicationInFoundingComments { get; set; }

        [Option( 'E', "extra-methods", HelpText = "Extra methods to be parsed for strings to be localized." )]
        public IEnumerable<string> ExtraLocalizationFunctions { get; set; } = new List<string>();
    }

    [Verb( "analyze", HelpText = "Analyze an I18N file." )]
    class AnalyzeOptions
    {
        [Option( 'i', "input", Required = true, HelpText = "Input file path." )]
        public string InputFile { get; set; } = string.Empty;

        [Option( 'd', "check-deprecated", HelpText = "Check presence of deprecated entries." )]
        public bool CheckDeprecated { get; set; }

        [Option( 'L', "check-language", HelpText = "Check for entries without translation for one or more languages ('*' for any)." )]
        public IEnumerable<string> CheckTranslationForLanguages { get; set; } = new List<string>();

        [Option( 'C', "include-context", HelpText = "Context to include in analysis." )]
        public IEnumerable<string> IncludeContexts { get; set; } = new List<string>();

        [Option( 'E', "exclude-context", HelpText = "Context to exclude from analysis." )]
        public IEnumerable<string> ExcludeContexts { get; set; } = new List<string>();
    }

    [Verb( "deploy", HelpText = "Generate a I18N deployment file." )]
    class DeployOptions
    {
        [Option( 'i', "input", Required = true, HelpText = "Input file path." )]
        public string InputFile { get; set; } = string.Empty;

        [Option( 'o', "output", Required = true, HelpText = "Output file path." )]
        public string OutputFile { get; set; } = string.Empty;
    }

    class Program
    {
        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        [ExcludeFromCodeCoverage]
        public static void Main( string[] args )
        {
            var parserResult = Parser.Default.ParseArguments<ParseSourcesOptions, AnalyzeOptions, DeployOptions>( args );

            parserResult.MapResult(
                ( ParseSourcesOptions opts ) => ParseSources( opts, new I18NFile(), new SourceFileParser(), new TextConsole() ),
                ( AnalyzeOptions opts ) => Analyze( opts, new I18NFile(), new TextConsole() ),
                ( DeployOptions opts ) => Deploy( opts, new I18NFile(), new TextConsole() ),
                errs => EXIT_CODE_ERROR
                );
        }

        //===========================================================================
        //                            INTERNAL METHODS
        //===========================================================================

        internal static int ParseSources( ParseSourcesOptions options, II18NFile outputFile, ISourceFileParser sourceFileParser, ITextConsole textConsole )
        {
            try
            {
                CheckFileDirectoryExists( options.OutputFile );

                var rootContext = new Context();

                foreach( var directory in options.SourcesDirectories )
                {
                    var dirInfo = new DirectoryInfo( directory );

                    ParseSourcesInDirectory( sourceFileParser, dirInfo, options.SourceFilesPattern, options.Recursive, options.ExtraLocalizationFunctions, rootContext );
                }

                outputFile.LoadFromFile( options.OutputFile );

                if( !options.PreserveFoundingComments )
                {
                    outputFile.DeleteFoundingComments();
                }

                outputFile.CreateEntries( rootContext, options.LineIndicationInFoundingComments );

                if( options.MarkDeprecated )
                {
                    outputFile.CreateDeprecationComments();
                }

                outputFile.WriteToFile( options.OutputFile );

                textConsole.WriteLine( $"I18N development file generated successfully" );

                return EXIT_CODE_SUCCESS;
            }
            catch( ApplicationException e )
            {
                textConsole.WriteLine( $"ERROR: {e.Message}", true );
            }
            catch( Exception e )
            {
                textConsole.WriteLine( $"UNEXPECTED ERROR: {e}", true );
            }

            return EXIT_CODE_ERROR;
        }

        internal static int Analyze( AnalyzeOptions options, II18NFile inputFile, ITextConsole textConsole )
        {
            try
            {
                inputFile.LoadFromFile( options.InputFile );

                var includeContexts = options.IncludeContexts.Select( s => ContextSpecToRegex( s ) );
                var excludeContexts = options.ExcludeContexts.Select( s => ContextSpecToRegex( s ) );

                var languagesToCheck = options.CheckTranslationForLanguages.ToArray();
                bool checkLanguages = ( languagesToCheck.Length > 0 );

                if( !options.CheckDeprecated && !checkLanguages )
                {
                    // No check options => use default check options
                    options.CheckDeprecated = true;
                    checkLanguages = true;
                }

                bool hasWarnings = false;
                bool hasErrors = false;

                var issues = inputFile.GetFileIssues();

                foreach( var issue in issues )
                {
                    if( issue.isError )
                    {
                        hasErrors = true;
                        textConsole.WriteLine( $"ERROR: [Line {issue.line}] {issue.message}", true );
                    }
                    else
                    {
                        hasWarnings = true;
                        textConsole.WriteLine( $"WARNING: [Line {issue.line}] {issue.message}", true );
                    }
                }

                if( options.CheckDeprecated )
                {
                    var results = inputFile.GetDeprecatedEntries( includeContexts, excludeContexts );

                    if( results.Any() )
                    {
                        hasWarnings = true;
                    }

                    foreach( var result in results )
                    {
                        if( result.key != null )
                        {
                            textConsole.WriteLine( $"WARNING: [Line {result.line}] Deprecated entry (Context = {result.context}, Key = '{result.key}')", true );
                        }
                        else
                        {
                            textConsole.WriteLine( $"WARNING: [Line {result.line}] Deprecated entry (Context = {result.context}, No key)", true );
                        }
                    }
                }

                if( checkLanguages )
                {
                    if( languagesToCheck.Contains( "*" ) )
                    {
                        languagesToCheck = Array.Empty<string>();
                    }

                    var results = inputFile.GetNoTranslationEntries( languagesToCheck, includeContexts, excludeContexts );

                    if( results.Any() )
                    {
                        hasWarnings = true;
                    }

                    foreach( var result in results )
                    {
                        if( result.key != null )
                        {
                            textConsole.WriteLine( $"WARNING: [Line {result.line}] Entry without translation (Context = {result.context}, Key = '{result.key}')", true );
                        }
                        else
                        {
                            textConsole.WriteLine( $"WARNING: [Line {result.line}] Entry without translation (Context = {result.context}, No key)", true );
                        }
                    }
                }

                if( hasWarnings || hasErrors )
                {
                    textConsole.WriteLine( string.Empty );
                }

                textConsole.WriteLine( $"I18N file analysis finished" );

                return hasErrors ? EXIT_CODE_ERROR : ( hasWarnings ? EXIT_CODE_WARNING : EXIT_CODE_SUCCESS );
            }
            catch( ApplicationException e )
            {
                textConsole.WriteLine( $"ERROR: {e.Message}", true );
            }
            catch( Exception e )
            {
                textConsole.WriteLine( $"UNEXPECTED ERROR: {e}", true );
            }

            return EXIT_CODE_ERROR;
        }

        internal static int Deploy( DeployOptions options, II18NFile outputFile, ITextConsole textConsole )
        {
            try
            {
                CheckFileDirectoryExists( options.OutputFile );

                outputFile.LoadFromFile( options.InputFile );

                outputFile.DeleteAllComments();

                outputFile.WriteToFile( options.OutputFile );

                textConsole.WriteLine( $"I18N deployment file generated successfully" );

                return EXIT_CODE_SUCCESS;
            }
            catch( ApplicationException e )
            {
                textConsole.WriteLine( $"ERROR: {e.Message}", true );
            }
            catch( Exception e )
            {
                textConsole.WriteLine( $"UNEXPECTED ERROR: {e}", true );
            }

            return EXIT_CODE_ERROR;
        }

        //===========================================================================
        //                            PRIVATE METHODS
        //===========================================================================

        private static void ParseSourcesInDirectory( ISourceFileParser sourceFileParser, DirectoryInfo dirInfo, string pattern, bool recursive, IEnumerable<string> extraFunctions, Context rootContext )
        {
            foreach( var fileInfo in dirInfo.GetFiles( pattern ) )
            {
                sourceFileParser.ParseFile( fileInfo.FullName, extraFunctions, rootContext );
            }

            if( recursive )
            {
                foreach( var childDirInfo in dirInfo.GetDirectories() )
                {
                    ParseSourcesInDirectory( sourceFileParser, childDirInfo, pattern, true, extraFunctions, rootContext );
                }
            }
        }

        private static Regex ContextSpecToRegex( string contextSpec )
        {
            if( contextSpec.StartsWith( '@' ) )
            {
                return new Regex( contextSpec[ 1.. ] );
            }
            else
            {
                var escapedSpec = Regex.Escape( contextSpec );
                var transformedSpec = WILDCARD_REGEX.Replace( escapedSpec, ".*" );
                return new Regex( $"^/?{transformedSpec}/?$" );
            }
        }

        private static void CheckFileDirectoryExists( string filepath )
        {
            var directory = Path.GetDirectoryName( filepath ) ?? string.Empty;

            if( directory.Length == 0 )
            {
                directory = ".";
            }

            if( !Directory.Exists( directory ) )
            {
                throw new ApplicationException( $"Output directory '{directory}' does not exist" );
            }
        }

        //===========================================================================
        //                           PRIVATE CONSTANTS
        //===========================================================================

        private static readonly Regex WILDCARD_REGEX = new( @"(?<!\\)\\\*" );

        private const int EXIT_CODE_SUCCESS = 0;
        private const int EXIT_CODE_ERROR = 1;
        private const int EXIT_CODE_WARNING = 2;

    }
}
