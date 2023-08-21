I18N.DotNet
========

[![Build](https://github.com/SafeTwice/I18N.DotNet/actions/workflows/Build.yml/badge.svg)](https://github.com/SafeTwice/I18N.DotNet/actions/workflows/Build.yml)

## About

I18N.DotNet is a .NET library written in C# to enable simple internationalization (I18N) / localization (L10N) (i.e. translation to different languages) of .NET applications and libraries.

A companion utility [I18N.DotNet Tool](https://github.com/SafeTwice/I18N.DotNet/tree/main/Tool) is provided to ease management of translation files.

## Installation

The easiest way to install I18N.DotNet is using the NuGet package: https://www.nuget.org/packages/I18N.DotNet/

## Getting Started

### Adapting Source Code (I18N)

Source code must be adapted following two simple steps:

- The first step consists in adding a couple of calls during initialization of the program (before any translated string is used):
  - Call `I18N.DotNet.Global.Localizer.SetTargetLanguage()` to set the language to which strings will be translated.
  - Call `I18N.DotNet.Global.Localizer.LoadXML()` to load the file that contains the translations.
- The second step consists in adapting the source code in order to wrap the strings to be translated with a call to `I18N.DotNet.Global.Localize()`.

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

The companion utility [I18N.DotNet Tool](Tool/) can be used to ease the creation of the translations file by scanning source files and automatically generating entries for discovered internationalized strings.

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
