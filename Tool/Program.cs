/**
 * @file
 * @copyright  Copyright (c) 2020-2022 SafeTwice S.L. All rights reserved.
 * @license    MIT (https://opensource.org/licenses/MIT)
 */

using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;

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

        [Option( 'l', "language", SetName = "language", HelpText = "(Default: *) Warn on entries without translation for a language." )]
        public string Language { get; set; }

        [Option( "ignore-deprecated", HelpText = "Skip warning on deprecated entries." )]
        public bool IgnoreDeprecated { get; set; }

        [Option( "ignore-no-translation", SetName = "no-language", HelpText = "Skip warning on empty without translation." )]
        public bool IgnoreNoTranslation { get; set; }
    }

    class Program
    {
        static void Main( string[] args )
        {
            var parserResult = Parser.Default.ParseArguments<GenerateOptions, AnalyzeOptions>( args );

            parserResult.MapResult(
                ( GenerateOptions opts ) => Generate( opts ),
                ( AnalyzeOptions opts ) => Analyze( opts ),
                errs => 1
                );
        }

        private static int Generate( GenerateOptions options )
        {
            try
            {
                var rootContext = new Context();

                foreach( var directory in options.Directories )
                {
                    var dirInfo = new DirectoryInfo( directory );

                    ParseFilesInDirectory( dirInfo, options.Pattern, options.Recursive, options.ExtraFunctions, rootContext );
                }

                var outputFile = new OutputFile( options.OutputFile );

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

        private static int Analyze( AnalyzeOptions options )
        {
            try
            {
                var inputFile = new OutputFile( options.InputFile );

                if( !options.IgnoreDeprecated )
                {
                    foreach( (int line, string context, string key) in inputFile.GetDeprecatedEntries() )
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

                if( !options.IgnoreNoTranslation )
                {
                    foreach( (int line, string context, string key) in inputFile.GetNoTranslationEntries( options.Language ?? "*" ) )
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

        private static void ParseFilesInDirectory( DirectoryInfo dirInfo, string pattern, bool recursive, IEnumerable<string> extraFunctions, Context rootContext )
        {
            foreach( var fileInfo in dirInfo.GetFiles( pattern ) )
            {
                InputFileParser.ParseFile( fileInfo.FullName, extraFunctions, rootContext );
            }

            if( recursive )
            {
                foreach( var childDirInfo in dirInfo.GetDirectories() )
                {
                    ParseFilesInDirectory( childDirInfo, pattern, true, extraFunctions, rootContext );
                }
            }
        }
    }
}
