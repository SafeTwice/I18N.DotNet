using CommandLine;
using System.Collections.Generic;
using System.IO;

namespace I18N.Tool
{
    class Options
    {
        [Option( 'I', Required = true, HelpText = "Input directory" )]
        public IEnumerable<string> Directories { get; set; }

        [Option( 'p', Default = "*.cs", HelpText = "Input files pattern" )]
        public string Pattern { get; set; }

        [Option( 'r', Default = false, HelpText = "Search input files in directories recursively" )]
        public bool Recursive { get; set; }

        [Option( 'z', Default = false, HelpText = "Reset founding comments in output file" )]
        public bool ResetFoundings { get; set; }

        [Option( 'e', HelpText = "Extra function to be parsed for strings to be localized" )]
        public IEnumerable<string> ExtraFunctions { get; set; }

        [Option( 'o', Required = true, HelpText = "Output file" )]
        public string OutputFile { get; set; }
    }

    class Program
    {
        static void Main( string[] args )
        {
            var parserResult = Parser.Default.ParseArguments<Options>( args );

            parserResult.MapResult(
                opts => Run( opts ),
                errs => 1
                );
        }

        static int Run( Options options )
        {
            Dictionary<string, List<string>> keyMatches = new Dictionary<string, List<string>>();

            foreach( var directory in options.Directories )
            {
                var dirInfo = new DirectoryInfo( directory );

                ParseFilesInDirectory( dirInfo, options.Pattern, options.Recursive, options.ExtraFunctions, keyMatches );
            }

            OutputFileGenerator.GenerateFile( options.OutputFile, options.ResetFoundings, keyMatches );

            return 0;
        }

        static void ParseFilesInDirectory( DirectoryInfo dirInfo, string pattern, bool recursive, IEnumerable<string> extraFunctions, Dictionary<string, List<string>> keyMatches )
        {
            foreach( var fileInfo in dirInfo.GetFiles( pattern ) )
            {
                InputFileParser.ParseFile( fileInfo.FullName, extraFunctions, keyMatches );
            }

            if( recursive )
            {
                foreach( var childDirInfo in dirInfo.GetDirectories() )
                {
                    ParseFilesInDirectory( childDirInfo, pattern, true, extraFunctions, keyMatches );
                }
            }
        }
    }
}
