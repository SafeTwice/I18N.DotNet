I18N.DotNet Tool
================

## About

The I18N.DotNet Tool utility can be used to generate, update, analyze and deploy translation files for the [I18N.DotNet](../README.md) library.

## Installation

The easiest way to install I18N.DotNet Tool is using the NuGet package: https://www.nuget.org/packages/I18N.DotNet.Tool/


## Usage

When installed from the [NuGet package](https://www.nuget.org/packages/I18N.DotNet.Tool/):
``` powershell
dotnet i18n-tool <command> [COMMAND-OPTIONS...]
```

When executed from the tool compiled using Visual Studio:
``` powershell
I18N.DotNet.Tool.exe <command> [COMMAND-OPTIONS...]
```

This tool accepts two different commands:

| Command                       | Description                                                      |
| -                             | -                                                                |
| [parse](#parse-command)       | Parses source files and generates or updates a translations file |
| [analyze](#analyze-command)   | Analyzes a translations file                                     |
| [deploy](#deploy-command)     | Generates a translations file suitable for deployment            |

### Parse Command

This command extracts translation keys from source code by scanning source code files and, for each discovered internationalized string, it generates in the output file an `Entry` element with a `Key` element which value is set to the discovered internationalized string (if such entry does not already exist). Localization can be then performed by adding `Value` elements for each translation of the entries' keys to different languages.

To discover internationalized strings the tool searches for plain strings and interpolated strings that are used as the first argument to methods named `Localize` or `LocalizeFormat`.

Generated entries are decorated with "founding" comments indicating the source file and optionally the line where the internationalized string was found, to allow obtaining the context in which the string appears in order to improve translations. This also eases the task of introducing context partitions (see [Contexts](#contexts)).

If the output file already exists, the tool preserves the existing XML elements (except "founding" comments), i.e., it does not delete any existing entries even if the entry's key is not found anymore in the source code.

##### Command Options

| Option                                         | Description                                            |
| -                                              | -                                                      |
| -S &lt;sources-dir> _[&lt;sources-dir-2 ...>]_ | Source directory paths                                 |
| -o &lt;output-file>                            | Output file path                                       |
| -p &lt;input-files-pattern>                    | Input files name pattern (default: *.cs)               |
| -r                                             | Scan in input directories recursively                  |
| -k                                             | Preserve founding comments in output file              |
| -d                                             | Mark deprecated entries                                |
| -l                                             | Include line numbers in founding comments              |
| -E &lt;func-name> _[&lt;func-name-2 ...>]_     | Extra methods to be parsed for strings to be localized |

At least one source files directory path must be passed using the `-S` option, and the output file must be specified using the `-o` option.

Input directories are by default not scanned recursively navigating into nested directories. Use the `-r` option to perform recursive scan on the input directories.

Internationalized strings are by default located by searching for plain strings and interpolated strings that are used as the first argument to methods named `Localize` or `LocalizeFormat`. If you define your own classes that define methods that wrap internationalization functionality (i.e., which internally call `Localizer` methods), then these additional methods can be also parsed using the `-E` option (as long as these methods take the strings to be localized as their first parameter).

Existing "founding" comments in the output file that indicate where a key was found in the source code are not preserved by default. To avoid this behavior, use the option `-k` to keep all "founding" comments.

Using the option `-d` makes the tool add a comment indicating that the entry is deprecated to previously existing entries in the output file which keys do not correspond to a key found in the source code.

By default "founding" comments just indicate the file where a key was found to avoid having too many changes in the translations file during development (even when no new translation keys are introduced). Using the option `-l` makes the tool include the line number where a key was found to "founding" comments.

##### Parsing Examples

Generate (or update) the translations file from all the sources found recursively in the _Sources_ folder:
``` powershell
dotnet i18n-tool parse -o MyApp.I18N.xml -r -d -S Sources\
```
### Analyze Command

This command analyzes a translations file to indicate the presence of deprecated entries and/or entries without translations for any, one or several languages.

##### Command Options

| Option                                     | Description                                            |
| -                                          | -                                                      |
| -i &lt;output-file>                        | Input file path                                        |
| -d                                         | Check presence deprecated entries                      |
| -L &lt;language> _[&lt;language-2 ...>]_   | Check for entries without translation for one or more languages ('*' for any) |
| -C &lt;context> _[&lt;context-2 ...>]_     | Contexts to include in analysis (default: all)         |
| -E &lt;context> _[&lt;context-2 ...>]_     | Contexts to exclude from analysis (default: none)      |

At least one input file path must be specified using the `-i` option.

The `-d` option makes the tool to check for the presence of deprecated entries (i.e., entries with no foundings).

The `-L` options makes the tool to check for the presence of entries with do not have translations defined for any of the languages passed. Pass `*` to check for entries which do not have a translation for any language.

Not passing neither `-d` nor `-L` is equivalent to `-d -L *`.

The `-C` option is used to indicate the contexts to include in analysis, and the `-E` options is used to indicate the context to exclude from analysis. Leading and trailing `/` context delimiters are options. The `*` character may be used as a wildcard. Alternatively, if the context begins with `@` then the following expression will be used as a regular expression to match contexts.

##### Analysis Examples

Check for deprecated entries and entries without translations for any language in all contexts:
``` powershell
dotnet i18n-tool analyze -i MyApp.I18N.xml
```

Check for deprecated entries in context */Context 1/* and its nested contexts except for */Context 1/Context2/*:
``` powershell
dotnet i18n-tool analyze -i MyApp.I18N.xml -d -C "Context 1/*" -E "Context 1/Context2"
```

Check for entries without translations for languages *es* or *fr* in nested contexts of */Context 1/* or */Context 2/*:
``` powershell
dotnet i18n-tool analyze -i MyApp.I18N.xml -L es fr -C "@^/Context [12]/.+$"
```
### Deploy Command

This command deploys a translations file ready for deployment generated from a "development" translations file.

Translations files generated by the `parse` command contain information useful for development (e.g., writing translations, checking missing and deprecated translations, etc.), but may not be suitable for deployment because they contain comments and development information that may not have to be disclosed. This command takes a "development" translations file and generates a translations file stripped out from comments suitable for deployment .

If the output file already exists, the tool overwrites its contents.

##### Command Options

| Option                                         | Description                                            |
| -                                              | -                                                      |
| -i &lt;input-file>                             | Input file path                                        |
| -o &lt;output-file>                            | Output file path                                       |

Both the input and output file must be specified using the  `-i` and `-o` options.

##### Deployment Examples

Generate the deployment translation file from a development translations file:
``` powershell
dotnet i18n-tool deploy -i Sources/MyApp.I18N.xml -o Resources/MyApp.I18N.xml
```
