/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace I18N.Tool
{
    [Verb( "generate", HelpText = "Generate an I18N file." )]
    class GenerateOptions
    {
        [Option( 'I', Required = true, HelpText = "Input directories paths." )]
        public IEnumerable<string> Directories { get; set; }

        [Option( 'p', Default = "*.cs", HelpText = "Input files name pattern." )]
        public string Pattern { get; set; }

        [Option( 'r', Default = false, HelpText = "Scan in input directories recursively." )]
        public bool Recursive { get; set; }

        [Option( 'k', Default = false, HelpText = "Preserve founding comments in output file." )]
        public bool PreserveFoundingComments { get; set; }

        [Option( 'd', Default = false, HelpText = "Mark deprecated entries." )]
        public bool MarkDeprecated { get; set; }

        [Option( 'e', HelpText = "Extra methods to be parsed for strings to be localized." )]
        public IEnumerable<string> ExtraFunctions { get; set; }

        [Option( 'o', Required = true, HelpText = "Output file path." )]
        public string OutputFile { get; set; }
    }

    [Verb( "analyze", HelpText = "Analyze an I18N file." )]
    class AnalyzeOptions
    {
        [Option( 'i', "input", Required = true, HelpText = "Input file path." )]
        public string InputFile { get; set; }

        [Option( 'd', "check-deprecated", HelpText = "Check presence of deprecated entries." )]
        public bool CheckDeprecated { get; set; }

        [Option( 'l', "check-language", HelpText = "Check for entries without translation for one or more languages ('*' for any)." )]
        public IEnumerable<string> CheckTranslationForLanguages { get; set; }

        [Option( 'c', "include-context", HelpText = "Context to include in analysis." )]
        public IEnumerable<string> IncludeContexts { get; set; }

        [Option( 'e', "exclude-context", HelpText = "Context to exclude from analysis." )]
        public IEnumerable<string> ExcludeContexts { get; set; }
    }

    class Program
    {
        static void Main( string[] args )
        {
            var parserResult = Parser.Default.ParseArguments<GenerateOptions, AnalyzeOptions>( args );

            parserResult.MapResult(
                ( GenerateOptions opts ) => Generate( opts, new I18NFile(), new SourceFileParser() ),
                ( AnalyzeOptions opts ) => Analyze( opts, new I18NFile() ),
                errs => 1
                );
        }

        internal static int Generate( GenerateOptions options, II18NFile outputFile, ISourceFileParser sourceFileParser )
        {
            try
            {
                var rootContext = new Context();

                foreach( var directory in options.Directories )
                {
                    var dirInfo = new DirectoryInfo( directory );

                    ParseFilesInDirectory( sourceFileParser, dirInfo, options.Pattern, options.Recursive, options.ExtraFunctions, rootContext );
                }

                outputFile.Load( options.OutputFile );

                if( !options.PreserveFoundingComments )
                {
                    outputFile.DeleteFoundingComments();
                }

                outputFile.CreateEntries( rootContext );

                if( options.MarkDeprecated )
                {
                    outputFile.CreateDeprecationComments();
                }

                outputFile.WriteToFile( options.OutputFile );

                Console.WriteLine( $"File generated successfully" );

                return 0;
            }
            catch( ApplicationException e )
            {
                Console.WriteLine( $"ERROR: {e.Message}" );
            }
            catch( Exception e )
            {
                Console.WriteLine( $"UNEXPECTED ERROR: {e}" );
            }

            return 1;
        }

        internal static int Analyze( AnalyzeOptions options, II18NFile inputFile )
        {
            try
            {
                inputFile.Load( options.InputFile );

                var includeContexts = options.IncludeContexts.Select( s => ContextSpecToRegex( s ) ).ToArray();
                var excludeContexts = options.ExcludeContexts.Select( s => ContextSpecToRegex( s ) ).ToArray();

                var languagesToCheck = options.CheckTranslationForLanguages.ToArray();
                bool checkLanguages = ( languagesToCheck.Length > 0 );

                if( !options.CheckDeprecated && !checkLanguages )
                {
                    // No check options => use default check options
                    options.CheckDeprecated = true;
                    checkLanguages = true;
                }

                if( options.CheckDeprecated )
                {
                    foreach( (int line, string context, string key) in inputFile.GetDeprecatedEntries( includeContexts, excludeContexts ) )
                    {
                        if( key != null )
                        {
                            Console.WriteLine( $"WARNING: Deprecated entry at line {line} (Context = {context}, Key = '{key}')" );
                        }
                        else
                        {
                            Console.WriteLine( $"WARNING: Deprecated entry at line {line} (Context = {context}, No key)" );
                        }
                    }
                }

                if( checkLanguages )
                {
                    if( languagesToCheck.Contains( "*" ) )
                    {
                        languagesToCheck = new string[ 0 ];
                    }

                    foreach( (int line, string context, string key) in inputFile.GetNoTranslationEntries( languagesToCheck, includeContexts, excludeContexts ) )
                    {
                        if( key != null )
                        {
                            Console.WriteLine( $"WARNING: Entry without translation at line {line} (Context = {context}, Key = '{key}')" );
                        }
                        else
                        {
                            Console.WriteLine( $"WARNING: Entry without translation at line {line} (Context = {context}, No key)" );
                        }
                    }
                }

                return 0;
            }
            catch( ApplicationException e )
            {
                Console.WriteLine( $"ERROR: {e.Message}" );
            }
            catch( Exception e )
            {
                Console.WriteLine( $"UNEXPECTED ERROR: {e}" );
            }

            return 1;
        }

        private static void ParseFilesInDirectory( ISourceFileParser sourceFileParser, DirectoryInfo dirInfo, string pattern, bool recursive, IEnumerable<string> extraFunctions, Context rootContext )
        {
            foreach( var fileInfo in dirInfo.GetFiles( pattern ) )
            {
                sourceFileParser.ParseFile( fileInfo.FullName, extraFunctions, rootContext );
            }

            if( recursive )
            {
                foreach( var childDirInfo in dirInfo.GetDirectories() )
                {
                    ParseFilesInDirectory( sourceFileParser, childDirInfo, pattern, true, extraFunctions, rootContext );
                }
            }
        }

        private static Regex ContextSpecToRegex( string contextSpec )
        {
            if( contextSpec.StartsWith( '@' ) )
            {
                return new Regex( contextSpec.Substring( 1 ) );
            }
            else
            {
                var escapedSpec = Regex.Escape( contextSpec );
                var transformedSpec = WILDCARD_REGEX.Replace( escapedSpec, ".*" );
                return new Regex( $"^{transformedSpec}/?$" );
            }
        }

        private static readonly Regex WILDCARD_REGEX = new Regex( @"(?<!\\)\\\*" );
    }
}
