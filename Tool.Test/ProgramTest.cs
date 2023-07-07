/// @file
/// @copyright  Copyright (c) 2022 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace I18N.DotNet.Tool.Test
{
    public class ProgramTest : IDisposable
    {
        private bool m_deleteTempFiles = false;

        private static readonly string[] TEMP_FILE_NAMES = 
        {
            @"TempDir1\TempFile1.foo",
            @"TempDir1\TempFile2.bar",
            @"TempDir2\TempFile3.foo",
            @"TempDir2\TempFile4.foo",
            @"TempDir2\TempFile5.bar",
            @"TempDir2\TempDir22\TempFile6.foo",
            @"TempDir2\TempDir22\TempFile7.bar",
        };

        private static readonly string[] TEMP_DIR_NAMES =
        {
            @"TempDir1",
            @"TempDir2",
        };

        public void Dispose()
        {
            if( m_deleteTempFiles )
            {
                foreach( var relDirname in TEMP_DIR_NAMES )
                {
                    var absDirname = Path.GetTempPath() + relDirname;
                    Directory.Delete( absDirname, true );
                }

                m_deleteTempFiles = false;
            }
        }

        private void CreateTempFiles()
        {
            m_deleteTempFiles = true;

            foreach( var relFilename in TEMP_FILE_NAMES )
            {
                var absFilename = Path.GetTempPath() + relFilename;
                Directory.CreateDirectory( Path.GetDirectoryName( absFilename ) );
                using( var file = File.Create( absFilename ) )
                {
                    file.Close();
                }
            }
        }

        [Fact]
        public void Generate_Default()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = false;
            options.InputFilesPattern = "*.foo";
            options.MarkDeprecated = false;
            options.PreserveFoundingComments = false;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[ 0 ];

            Context internalContext = null;

            string[] expectedParsedFiles =
            {
                @"TempDir1\TempFile1.foo",
                @"TempDir2\TempFile3.foo",
                @"TempDir2\TempFile4.foo",
            };

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.InSequence( callSequence ).Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) )
                .Callback<string, IEnumerable<string>, Context>( ( _, _, context ) => { if( internalContext == null ) { internalContext = context; } } );

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( options.OutputFile ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.DeleteFoundingComments() );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateEntries( It.Is<Context>( ctx => ctx == internalContext ) ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.WriteToFile( options.OutputFile ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            foreach( var relParsedFileName in expectedParsedFiles )
            {
                var absParsedFileName = Path.GetTempPath() + relParsedFileName;
                sourceFileParserMock.Verify( sfp => sfp.ParseFile( absParsedFileName, options.ExtraLocalizationFunctions, internalContext ), Times.Once );
            }

            i18nFileMock.Verify( f => f.Load( options.OutputFile ), Times.Once );
            i18nFileMock.Verify( f => f.DeleteFoundingComments(), Times.Once );
            i18nFileMock.Verify( f => f.CreateEntries( internalContext ), Times.Once );
            i18nFileMock.Verify( f => f.CreateDeprecationComments(), Times.Never );
            i18nFileMock.Verify( f => f.WriteToFile( options.OutputFile ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "success" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Generate_Recursive()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = true;
            options.InputFilesPattern = "*.foo";
            options.MarkDeprecated = false;
            options.PreserveFoundingComments = false;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[ 0 ];

            Context internalContext = null;

            string[] expectedParsedFiles =
            {
                @"TempDir1\TempFile1.foo",
                @"TempDir2\TempFile3.foo",
                @"TempDir2\TempFile4.foo",
                @"TempDir2\TempDir22\TempFile6.foo",
            };

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.InSequence( callSequence ).Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) )
                .Callback<string, IEnumerable<string>, Context>( ( _, _, context ) => { if( internalContext == null ) { internalContext = context; } } );

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( options.OutputFile ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.DeleteFoundingComments() );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateEntries( It.Is<Context>( ctx => ctx == internalContext ) ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.WriteToFile( options.OutputFile ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            foreach( var relParsedFileName in expectedParsedFiles )
            {
                var absParsedFileName = Path.GetTempPath() + relParsedFileName;
                sourceFileParserMock.Verify( sfp => sfp.ParseFile( absParsedFileName, options.ExtraLocalizationFunctions, internalContext ), Times.Once );
            }

            i18nFileMock.Verify( f => f.Load( options.OutputFile ), Times.Once );
            i18nFileMock.Verify( f => f.DeleteFoundingComments(), Times.Once );
            i18nFileMock.Verify( f => f.CreateEntries( internalContext ), Times.Once );
            i18nFileMock.Verify( f => f.CreateDeprecationComments(), Times.Never );
            i18nFileMock.Verify( f => f.WriteToFile( options.OutputFile ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "success" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Generate_InputFilesPattern()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = false;
            options.InputFilesPattern = "*.bar";
            options.MarkDeprecated = false;
            options.PreserveFoundingComments = false;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[ 0 ];

            Context internalContext = null;

            string[] expectedParsedFiles =
            {
                @"TempDir1\TempFile2.bar",
                @"TempDir2\TempFile5.bar",
            };

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.InSequence( callSequence ).Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) )
                .Callback<string, IEnumerable<string>, Context>( ( _, _, context ) => { if( internalContext == null ) { internalContext = context; } } );

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( options.OutputFile ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.DeleteFoundingComments() );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateEntries( It.Is<Context>( ctx => ctx == internalContext ) ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.WriteToFile( options.OutputFile ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            foreach( var relParsedFileName in expectedParsedFiles )
            {
                var absParsedFileName = Path.GetTempPath() + relParsedFileName;
                sourceFileParserMock.Verify( sfp => sfp.ParseFile( absParsedFileName, options.ExtraLocalizationFunctions, internalContext ), Times.Once );
            }

            i18nFileMock.Verify( f => f.Load( options.OutputFile ), Times.Once );
            i18nFileMock.Verify( f => f.DeleteFoundingComments(), Times.Once );
            i18nFileMock.Verify( f => f.CreateEntries( internalContext ), Times.Once );
            i18nFileMock.Verify( f => f.CreateDeprecationComments(), Times.Never );
            i18nFileMock.Verify( f => f.WriteToFile( options.OutputFile ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "success" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Generate_MarkDeprecated()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = false;
            options.InputFilesPattern = "*.foo";
            options.MarkDeprecated = true;
            options.PreserveFoundingComments = false;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[ 0 ];

            Context internalContext = null;

            string[] expectedParsedFiles =
            {
                @"TempDir1\TempFile1.foo",
                @"TempDir2\TempFile3.foo",
                @"TempDir2\TempFile4.foo",
            };

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.InSequence( callSequence ).Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) )
                .Callback<string, IEnumerable<string>, Context>( ( _, _, context ) => { if( internalContext == null ) { internalContext = context; } } );

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( options.OutputFile ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.DeleteFoundingComments() );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateEntries( It.Is<Context>( ctx => ctx == internalContext ) ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateDeprecationComments() );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.WriteToFile( options.OutputFile ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            foreach( var relParsedFileName in expectedParsedFiles )
            {
                var absParsedFileName = Path.GetTempPath() + relParsedFileName;
                sourceFileParserMock.Verify( sfp => sfp.ParseFile( absParsedFileName, options.ExtraLocalizationFunctions, internalContext ), Times.Once );
            }

            i18nFileMock.Verify( f => f.Load( options.OutputFile ), Times.Once );
            i18nFileMock.Verify( f => f.DeleteFoundingComments(), Times.Once );
            i18nFileMock.Verify( f => f.CreateEntries( internalContext ), Times.Once );
            i18nFileMock.Verify( f => f.CreateDeprecationComments(), Times.Once );
            i18nFileMock.Verify( f => f.WriteToFile( options.OutputFile ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "success" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Generate_PreserveFoundingComments()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = false;
            options.InputFilesPattern = "*.foo";
            options.MarkDeprecated = false;
            options.PreserveFoundingComments = true;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[ 0 ];

            Context internalContext = null;

            string[] expectedParsedFiles =
            {
                @"TempDir1\TempFile1.foo",
                @"TempDir2\TempFile3.foo",
                @"TempDir2\TempFile4.foo",
            };

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.InSequence( callSequence ).Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) )
                .Callback<string, IEnumerable<string>, Context>( ( _, _, context ) => { if( internalContext == null ) { internalContext = context; } } );

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( options.OutputFile ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateEntries( It.Is<Context>( ctx => ctx == internalContext ) ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.WriteToFile( options.OutputFile ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            foreach( var relParsedFileName in expectedParsedFiles )
            {
                var absParsedFileName = Path.GetTempPath() + relParsedFileName;
                sourceFileParserMock.Verify( sfp => sfp.ParseFile( absParsedFileName, options.ExtraLocalizationFunctions, internalContext ), Times.Once );
            }

            i18nFileMock.Verify( f => f.Load( options.OutputFile ), Times.Once );
            i18nFileMock.Verify( f => f.DeleteFoundingComments(), Times.Never );
            i18nFileMock.Verify( f => f.CreateEntries( internalContext ), Times.Once );
            i18nFileMock.Verify( f => f.CreateDeprecationComments(), Times.Never );
            i18nFileMock.Verify( f => f.WriteToFile( options.OutputFile ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "success" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Generate_ExtraLocalizationFunctions()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = false;
            options.InputFilesPattern = "*.foo";
            options.MarkDeprecated = false;
            options.PreserveFoundingComments = false;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[] { "foo", "bar" };

            Context internalContext = null;

            string[] expectedParsedFiles =
            {
                @"TempDir1\TempFile1.foo",
                @"TempDir2\TempFile3.foo",
                @"TempDir2\TempFile4.foo",
            };

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.InSequence( callSequence ).Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) )
                .Callback<string, IEnumerable<string>, Context>( ( _, _, context ) => { if( internalContext == null ) { internalContext = context; } } );

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( options.OutputFile ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.DeleteFoundingComments() );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateEntries( It.Is<Context>( ctx => ctx == internalContext ) ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.CreateDeprecationComments() );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.WriteToFile( options.OutputFile ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            foreach( var relParsedFileName in expectedParsedFiles )
            {
                var absParsedFileName = Path.GetTempPath() + relParsedFileName;
                sourceFileParserMock.Verify( sfp => sfp.ParseFile( absParsedFileName, options.ExtraLocalizationFunctions, internalContext ), Times.Once );
            }

            i18nFileMock.Verify( f => f.Load( options.OutputFile ), Times.Once );
            i18nFileMock.Verify( f => f.DeleteFoundingComments(), Times.Once );
            i18nFileMock.Verify( f => f.CreateEntries( internalContext ), Times.Once );
            i18nFileMock.Verify( f => f.CreateDeprecationComments(), Times.Never );
            i18nFileMock.Verify( f => f.WriteToFile( options.OutputFile ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "success" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Generate_ApplicationError()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = false;
            options.InputFilesPattern = "*.foo";
            options.MarkDeprecated = false;
            options.PreserveFoundingComments = false;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[ 0 ];

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) ).Throws( new ApplicationException( "foo" ) );

            var i18nFileMock = new Mock<II18NFile>();

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 1, ret );

            sourceFileParserMock.Verify( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "ERROR" ) && s.Contains( "foo" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Generate_UnexpectedError()
        {
            // Prepare

            CreateTempFiles();

            var options = new GenerateOptions();
            options.InputDirectories = new string[] { Path.GetTempPath() + @"\TempDir1", Path.GetTempPath() + @"\TempDir2" };
            options.Recursive = false;
            options.InputFilesPattern = "*.foo";
            options.MarkDeprecated = false;
            options.PreserveFoundingComments = false;
            options.OutputFile = "bar.xml";
            options.ExtraLocalizationFunctions = new string[ 0 ];

            var callSequence = new MockSequence();

            var sourceFileParserMock = new Mock<ISourceFileParser>();
            sourceFileParserMock.InSequence( callSequence ).Setup( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ) )
                .Throws( new Exception( "foo" ) );

            var i18nFileMock = new Mock<II18NFile>();

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Generate( options, i18nFileMock.Object, sourceFileParserMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 1, ret );

            sourceFileParserMock.Verify( sfp => sfp.ParseFile( It.IsAny<string>(), options.ExtraLocalizationFunctions, It.IsAny<Context>() ), Times.Once );

            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "UNEXPECTED ERROR" ) && s.Contains( "foo" ) ) ), Times.Once );

            sourceFileParserMock.VerifyNoOtherCalls();
            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_Deprecated()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = true;
            options.CheckTranslationForLanguages = new string[ 0 ];
            options.IncludeContexts = new string[ 0 ];
            options.ExcludeContexts = new string[ 0 ];

            var includeContext = new Regex[ 0 ];
            var excludeContext = new Regex[ 0 ];

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 99, "Context1", "Key1" ),
                ( 888, "Context2", "Key2" ),
            };

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetDeprecatedEntries( includeContext, excludeContext ) ).Returns( expectedResults );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            i18nFileMock.Verify( f => f.GetDeprecatedEntries( includeContext, excludeContext ), Times.Once );
            foreach( var result in expectedResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) && s.Contains( result.key ) ) ), Times.Once );
            }

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_NoTranslations_SpecificLanguages()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = false;
            options.CheckTranslationForLanguages = new string[] { "es", "en", "fr" };
            options.IncludeContexts = new string[ 0 ];
            options.ExcludeContexts = new string[ 0 ];

            var includeContext = new Regex[ 0 ];
            var excludeContext = new Regex[ 0 ];

            var expectedResults = new (int line, string context, string key)[]
            {
                ( 99, "Context1", "Key1" ),
                ( 888, "Context2", "Key2" ),
            };
            var expectedLanguages = options.CheckTranslationForLanguages.ToArray();

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ) ).Returns( expectedResults );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            i18nFileMock.Verify( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ), Times.Once );
            foreach( var result in expectedResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) && s.Contains( result.key ) ) ), Times.Once );
            }

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_NoTranslations_AnyLanguage()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = false;
            options.CheckTranslationForLanguages = new string[] { "*", "en", "fr" };
            options.IncludeContexts = new string[ 0 ];
            options.ExcludeContexts = new string[ 0 ];

            var includeContext = new Regex[ 0 ];
            var excludeContext = new Regex[ 0 ];

            var expectedResults = new (int line, string context, string key)[ 0 ];
            var expectedLanguages = new string[ 0 ];

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ) ).Returns( expectedResults );

            var textConsoleMock = new Mock<ITextConsole>();

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            i18nFileMock.Verify( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ), Times.Once );
            textConsoleMock.Verify( c => c.WriteLine( It.IsAny<string>() ), Times.Never );

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_Default()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = false;
            options.CheckTranslationForLanguages = new string[ 0 ];
            options.IncludeContexts = new string[ 0 ];
            options.ExcludeContexts = new string[ 0 ];

            var includeContext = new Regex[ 0 ];
            var excludeContext = new Regex[ 0 ];

            var expectedDeprecatedResults = new (int line, string context, string key)[]
            {
                ( 99, "Context1", "Key1" ),
                ( 888, "Context2", "Key2" ),
            };
            var expectedNoTranslationResults = new (int line, string context, string key)[]
            {
                ( 9999, "Context 22", "Key 345" ),
                ( 123445, "Fooo Bar", "Key 3456" ),
            };
            var expectedLanguages = new string[ 0 ];

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetDeprecatedEntries( includeContext, excludeContext ) ).Returns( expectedDeprecatedResults );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ) ).Returns( expectedNoTranslationResults );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            i18nFileMock.Verify( f => f.GetDeprecatedEntries( includeContext, excludeContext ), Times.Once );
            i18nFileMock.Verify( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ), Times.Once );
            foreach( var result in expectedDeprecatedResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) && s.Contains( result.key ) ) ), Times.Once );
            }
            foreach( var result in expectedNoTranslationResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) && s.Contains( result.key ) ) ), Times.Once );
            }

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_ContextFilter()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = true;
            options.CheckTranslationForLanguages = new string[] { "es", "en", "fr" };
            options.IncludeContexts = new string[] { "Context 1", "/Context 2/", "@FooRegex" };
            options.ExcludeContexts = new string[] { "Context 4/*" };

            var includeContext = new Regex[] { new Regex( @"^/?Context 1/?$" ), new Regex( @"^/?/Context 2//?$" ), new Regex( @"FooRegex" ) };
            var excludeContext = new Regex[] { new Regex( @"^/?Context 4/.*/?$" ) };

            var expectedDeprecatedResults = new (int line, string context, string key)[]
            {
                ( 99, "Context1", "Key1" ),
                ( 888, "Context2", "Key2" ),
            };
            var expectedNoTranslationResults = new (int line, string context, string key)[]
            {
                ( 9999, "Context 22", "Key 345" ),
                ( 123445, "Fooo Bar", "Key 3456" ),
            };
            var expectedLanguages = options.CheckTranslationForLanguages.ToArray();

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetDeprecatedEntries( It.IsAny<Regex[]>(), It.IsAny<Regex[]>() ) ).Returns( expectedDeprecatedResults );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetNoTranslationEntries( expectedLanguages, It.IsAny<Regex[]>(), It.IsAny<Regex[]>() ) ).Returns( expectedNoTranslationResults );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            i18nFileMock.Verify( f => f.GetDeprecatedEntries( It.Is<Regex[]>( a => a.ToString() == includeContext.ToString() ),
                                                              It.Is<Regex[]>( a => a.ToString() == excludeContext.ToString() ) ),
                                 Times.Once );
            i18nFileMock.Verify( f => f.GetNoTranslationEntries( expectedLanguages, 
                                                                 It.Is<Regex[]>( a => a.ToString() == includeContext.ToString() ),
                                                                 It.Is<Regex[]>( a => a.ToString() == excludeContext.ToString() ) ),
                                 Times.Once );
            foreach( var result in expectedDeprecatedResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) && s.Contains( result.key ) ) ), Times.Once );
            }
            foreach( var result in expectedNoTranslationResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) && s.Contains( result.key ) ) ), Times.Once );
            }

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_NoKeys()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = false;
            options.CheckTranslationForLanguages = new string[ 0 ];
            options.IncludeContexts = new string[ 0 ];
            options.ExcludeContexts = new string[ 0 ];

            var includeContext = new Regex[ 0 ];
            var excludeContext = new Regex[ 0 ];

            var expectedDeprecatedResults = new (int line, string context, string key)[]
            {
                ( 99, "Context1", null),
                ( 888, "Context2", null ),
            };
            var expectedNoTranslationResults = new (int line, string context, string key)[]
            {
                ( 9999, "Context 22", null ),
                ( 123445, "Fooo Bar", null ),
            };
            var expectedLanguages = new string[ 0 ];

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetDeprecatedEntries( includeContext, excludeContext ) ).Returns( expectedDeprecatedResults );
            i18nFileMock.InSequence( callSequence ).Setup( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ) ).Returns( expectedNoTranslationResults );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 0, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            i18nFileMock.Verify( f => f.GetDeprecatedEntries( includeContext, excludeContext ), Times.Once );
            i18nFileMock.Verify( f => f.GetNoTranslationEntries( expectedLanguages, includeContext, excludeContext ), Times.Once );
            foreach( var result in expectedDeprecatedResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) ) ), Times.Once );
            }
            foreach( var result in expectedNoTranslationResults )
            {
                textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( result.line.ToString() ) && s.Contains( result.context ) ) ), Times.Once );
            }

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_ApplicationError()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = true;
            options.CheckTranslationForLanguages = new string[ 0 ];
            options.IncludeContexts = new string[ 0 ];
            options.ExcludeContexts = new string[ 0 ];

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) ).Throws( new ApplicationException( "foo" ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 1, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "ERROR" ) && s.Contains( "foo" ) ) ), Times.Once );

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Analize_UnexpectedError()
        {
            // Prepare

            string inputFilePath = @"C:\Foo.xml";

            var options = new AnalyzeOptions();
            options.InputFile = inputFilePath;
            options.CheckDeprecated = true;
            options.CheckTranslationForLanguages = new string[ 0 ];
            options.IncludeContexts = new string[ 0 ];
            options.ExcludeContexts = new string[ 0 ];

            var callSequence = new MockSequence();

            var i18nFileMock = new Mock<II18NFile>();
            i18nFileMock.InSequence( callSequence ).Setup( f => f.Load( inputFilePath ) ).Throws( new Exception( "foo" ) );

            var textConsoleMock = new Mock<ITextConsole>();
            textConsoleMock.InSequence( callSequence ).Setup( c => c.WriteLine( It.IsAny<string>() ) );

            // Execute

            int ret = Program.Analyze( options, i18nFileMock.Object, textConsoleMock.Object );

            // Verify

            Assert.Equal( 1, ret );

            i18nFileMock.Verify( o => o.Load( inputFilePath ), Times.Once );
            textConsoleMock.Verify( c => c.WriteLine( It.Is<string>( s => s.Contains( "UNEXPECTED ERROR" ) && s.Contains( "foo" ) ) ), Times.Once );

            i18nFileMock.VerifyNoOtherCalls();
            textConsoleMock.VerifyNoOtherCalls();
        }
    }
}
