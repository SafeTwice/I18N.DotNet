I18N.DotNet
========

## About

I18N.DotNet is a .NET library written in C# to enable simple internationalization (I18N) / localization (L10N) (i.e. translation to different languages) of .NET applications and libraries.

## Getting Started

### Adapting Source Code (I18N)

Source code must be adapted following two simple steps:

1) The first step consists in adding a couple of calls during initialization of the program (before any translated string is used):

  1. Call `I18N.DotNet.Global.Localizer.SetTargetLanguage()` to set the language to which strings will be translated.
  2. Call `I18N.DotNet.Global.Localizer.LoadXML()` to load the file that contains the translations.

2) The second step consists in adapting the source code in order to wrap the strings to be translated with a call to `I18N.DotNet.Global.Localize()`.

###### Example
``` CS
using static I18N.DotNet.Global;
using System;
using System.IO;
using System.Reflection;

public class Program
{
  static void Main( string[] args )
  {
    var programPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
    int i = 0x555;

    Localizer.SetTargetLanguage( CultureInfo.CurrentUICulture.Name ).LoadXML( programPath + "/I18N.xml" );

    Console.WriteLine( Localize( "Plain string to be translated" ) );
    Console.WriteLine( Localize( $"Interpolated string to be translated with value {i:X4}" ) );
  }
}
```

### Writing Translations (L10N)

String translations must be stored in an XML file with root element `I18N`.

For each string than has been internationalized an `Entry` element under the root must be defined, with:

- A single `Key` child element which value is the internationalized string defined in the code (replacing for interpolated strings the interpolated expressions with their positional index).
- `Value`child elements with their attribute `lang` set to the target language of the translation and which value is the translated string.

The companion utility [I18N Tool](#i18n-tool) can be used to ease the creation of the translations file by scanning source files and automatically generating entries for discovered internationalized strings.

###### Example
``` XML
<?xml version="1.0" encoding="utf-8"?>
<I18N>
  <Entry>
    <Key>Plain string to be translated</Key>
    <Value lang="es">String simple a traducir</Value>
    <Value lang="fr">String simple à traduire</Value>
  </Entry>
  <Entry>
    <Key>Interpolated string to be translated with value {0:X4}</Key>
    <Value lang="es">String interpolado a traducir con valor {0:X4}</Value>
    <Value lang="fr">String interpolé à traduire avec valeur {0:X4}</Value>
  </Entry>
</I18N>
```

## Advanced Usage

### Language Identifiers & Variants

Any arbitrary string can be used for identifying languages, and they are processed as case-insensitive.

When using language identifiers formed by a primary code and a variant code separated by an hyphen (e.g., _"en-us"_, _"es-es"_), if a localized conversion for the language variant is not found then a conversion for the primary (base) language is tried too.

For example, if `"en-gb"` is passed to `Localizer.SetTargetLanguage()`, then for each string to be translated a translation for the language _"en-gb"_ will be searched first, and if not found then a translation for the language _"en"_ will be searched next.

It is therefore recommended to:

- Use primary-variant code (e.g., _"en-us"_, _"es-es"_) as target language identifiers (i.e., as arguments to `Localizer.SetTargetLanguage()`).
- Use primary code (e.g., _"en"_, _"fr"_) as translation language identifiers (i.e, as the `lang` attribute values of XML `I18N.Entry.Value` entries) for generic (non variant-specific) translations.
- Use primary code-variant (e.g., _"en-gb"_, _"es-ar"_) as translation language identifiers (i.e, as the `lang` attribute values of XML `I18N.Entry.Value` entries) for variant-specific translations.

### String Format

Calls to `String.Format()` where the format string has to be internationalized can be replaced by a call to `I18N.DotNet.Global.LocalizeFormat()` (or `Localizer.LocalizeFormat()`, see [Global and Local Localizers](#global-and-local-localizers)).

###### Example
``` CS
String.Format( Localize( "Format string to be translated with value {0}" ), myVar );
// is equivalent to
LocalizeFormat( "Format string to be translated with value {0}", myVar );
```

### Global and Local Localizers

Instances of the `Localizer` class are responsible for loading string translations and then providing localization functionality (i.e. perform string translations) for software components.

The static class `I18N.DotNet.Global` has the property `Localizer` which contains the global localizer. This instance is shared and can be conveniently used by all software components. In fact all the methods exposed by the `I18N.DotNet.Global` class are just convenience wrappers that call the global localizer.

If necessary, additional instances of the `Localizer` class can be created (local localizers), loaded with string translations, and then passed to software components for being used instead of the global localizer. Nevertheless, for most cases using the global localizer is just enough.

### Contexts

Sometimes the same source language string has different translations in different contexts (e.g., English _"OK"_ should be translated in Spanish to _"Aceptar"_ for a button label but to _"Correcto"_ for a successful outcome indication).

Since the source language key is the same in both cases, context partitioning must be used, which affects the source code side and the translations file side.

##### Context Partitioning in Source Code (I18N)

In source code, the context of the key can be explicitly indicated when the string is being internationalized by calling `I18N.DotNet.Global.Context()` (or `Localizer.Context()`, see [Global and Local Localizers](#global-and-local-localizers)) and passing it the context identifier, and then calling the localization methods on the returned context `Localizer`.

Contexts can be nested. A chain of successively nested contexts can be identified by joining their identifiers using the dot character ('.') as a composite context identifier.

Translations in a context are searched hierarchically: if a translation is not found for the target language in is context (neither for the language variant nor the primary language), then a translation is searched again on its parent context (if it exists).

###### Example
``` CS
Button.Label = Context( "GUI.Button" ).Localize( "OK" );
// ...
TextBox.Text = Context( "GUI" ).Context( "Status" ).Localize( "OK" );
```

##### Context Partitioning in the Translation File (L10N)

Context partitioning is performed in the translations XML file using `Context` elements as children of the root element or nested within other `Context` elements. These elements must have an `id` attribute to indicate the context identifier (which can be a composite context identifier), and are containers for the `Entry` elements that define the translations for that context.

###### Example
``` XML
<?xml version="1.0" encoding="utf-8"?>
<I18N>
  <Entry>
    <Key>OK</Key>
    <Value lang="fr">O.K.</Value>
  </Entry>
  <Context id="GUI">
    <Context id="Button">
      <Entry>
        <Key>OK</Key>
        <Value lang="es">Aceptar</Value>
      </Entry>
    </Context>
    <Context id="Button">
      <Entry>
        <Key>OK</Key>
        <Value lang="es">Correcto</Value>
      </Entry>
    </Context>
  </Context>
</I18N>
```

### Embedding the Translations File

Instead of using translation files installed on the filesystem during the installation procedure for the application, these files can be embedded inside an executable assembly. Embedded resource files can then be accessed as `Stream` objects which are passed to `Localizer.LoadXML`.

## I18N Tool

### Usage

``` powershell
I18N.Tool.exe <command> [COMMAND-OPTIONS...]
```

This tool accepts two different commands:

| Command                       | Description                              |
| -                             | -                                        |
| [generate](#generate-command) | Generates or updates a translations file |
| [analyze](#analyze-command)   | Analyzes a translations file             |

### Generate Command

This command extracts translation keys from source code by scanning source code files and, for each discovered internationalized string, it generates in the output file an `Entry` element with a `Key` element which value is set to the discovered internationalized string (if such entry does not already exist). Localization can be then performed by adding `Value` elements for each translation of the entries' keys to different languages.

To discover internationalized strings the tool searches for plain strings and interpolated strings that are used as the first argument to methods named `Localize` or `LocalizeFormat`.

Generated entries are decorated with "founding" comments indicating the source file and line where the internationalized string was found, to allow obtaining the context in which the string appears in order to improve translations. This also eases the task of introducing context partitions (see [Contexts](#contexts)).

If the output file already exists, the tool preserves the existing XML elements (except "founding" comments), i.e., it does not delete any existing entries even if the entry's key is not found anymore in the source code.

##### Command Options

| Option                                     | Description                                            |
| -                                          | -                                                      |
| -I &lt;input-dir> _[&lt;input-dir-2 ...>]_ | Input directories                                      |
| -o &lt;output-file>                        | Output file path                                       |
| -p &lt;input-files-pattern>                | Input files name pattern (default: *.cs)               |
| -r                                         | Scan in input directories recursively                  |
| -k                                         | Preserve founding comments in output file              |
| -d                                         | Mark deprecated entries                                |
| -E &lt;func-name> _[&lt;func-name-2 ...>]_ | Extra methods to be parsed for strings to be localized |

At least one input directory must be passed using the `-I` option, and the output file must be specified using the `-o` option.

Input directories are by default not scanned recursively navigating into nested directories. Use the `-r` option to perform recursive scan on the input directories.

Internationalized strings are by default located by searching for plain strings and interpolated strings that are used as the first argument to methods named `Localize` or `LocalizeFormat`. If you define your own classes that define methods that wrap internationalization functionality (i.e., which internally call `Localizer` methods), then these additional methods can be also parsed using the `-E` option (as long as these methods take the strings to be localized as their first parameter).

Existing "founding" comments in the output file that indicate where a key was found in the source code are not preserved by default. To avoid this behavior, use the option `-k` to keep all "founding" comments.

Using the option `-d` makes the tool add a comment indicating that the entry is deprecated to previously existing entries in the output file which keys do not correspond to a key found in the source code.

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

The `-L` options makes the tool to check for the presence of entries with do not have translations defined for any of the languages passed. Pass `*` to check for entries which do not have a tranalation for any language.

Not passing neither `-d` nor `-L` is equivalent to `-d -L *`.

The `-C` option is used to indicate the contexts to include in analysis, and the `-E` options is used to indicate the context to exclude from analysis. Leading and trailing `/` context delimiters are options. The `*` character may be used as a wildcard. Alternatively, if the context begins with `@` then the following expression will be used as a regular expression to match contexts.

##### Analysis Examples

Check for deprecated entries and entries without translations for any language in all contexts:
``` powershell
I18N.Tool.exe analyze -i myfile.xml
```

Check for deprecated entries in context */Context 1/* and its nested contexts except for */Context 1/Context2/*:
``` powershell
I18N.Tool.exe analyze -i myfile.xml -d -C "Context 1/*" -E "Context 1/Context2"
```

Check for entries without translations for languages *es* or *fr* in nested contexts of */Context 1/* or */Context 2/*:
``` powershell
I18N.Tool.exe analyze -i myfile.xml -L es fr -C "@^/Context [12]/.+$"
```

