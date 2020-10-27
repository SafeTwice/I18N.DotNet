using CommandLine;
using System.Collections.Generic;
using System.IO;

namespace I18N.Tool
{
    class Options
    {
        [Option( 'I', Required = true, HelpText = "Input directories paths" )]
        public IEnumerable<string> Directories { get; set; }

        [Option( 'p', Default = "*.cs", HelpText = "Input files name pattern" )]
        public string Pattern { get; set; }

        [Option( 'r', Default = false, HelpText = "Scan in input directories recursively" )]
        public bool Recursive { get; set; }

        [Option( 'k', Default = false, HelpText = "Preserve founding comments in output file" )]
        public bool PreserveFoundingComments { get; set; }

        [Option( 'e', HelpText = "Extra methods to be parsed for strings to be localized" )]
        public IEnumerable<string> ExtraFunctions { get; set; }

        [Option( 'o', Required = true, HelpText = "Output file path" )]
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

        private static int Run( Options options )
        {
            Dictionary<string, List<string>> keyMatches = new Dictionary<string, List<string>>();

            foreach( var directory in options.Directories )
            {
                var dirInfo = new DirectoryInfo( directory );

                ParseFilesInDirectory( dirInfo, options.Pattern, options.Recursive, options.ExtraFunctions, keyMatches );
            }

            OutputFileGenerator.GenerateFile( options.OutputFile, options.PreserveFoundingComments, keyMatches );

            return 0;
        }

        private static void ParseFilesInDirectory( DirectoryInfo dirInfo, string pattern, bool recursive, IEnumerable<string> extraFunctions, Dictionary<string, List<string>> keyMatches )
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
