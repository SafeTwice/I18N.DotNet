# I18N.DotNet

[![Build](https://github.com/SafeTwice/I18N.DotNet/actions/workflows/Build.yml/badge.svg)](https://github.com/SafeTwice/I18N.DotNet/actions/workflows/Build.yml)
[![Coverage Status](https://coveralls.io/repos/github/SafeTwice/I18N.DotNet/badge.svg)](https://coveralls.io/github/SafeTwice/I18N.DotNet)

## About

I18N.DotNet is a .NET library written in C# to enable simple internationalization (I18N) / localization (L10N) (i.e. translation to different languages) of .NET applications and libraries.

The companion utility [I18N.DotNet Tool](https://github.com/SafeTwice/I18N.DotNet-Tool) is provided to ease management of translation files.

## Installation

The easiest way to install I18N.DotNet is using the NuGet package: https://www.nuget.org/packages/I18N.DotNet/

## Getting Started

To use the I18N.DotNet library, three steps must be followed:

1. Write/modify the source code to internationalize strings that must be translated (see [Writing/Adapting Source Code (I18N)](#writingadapting-source-code-i18n)).
2. Write translations for internationalized strings (see [Writing Translations (L10N)](#writing-translations-l10n)).
3. Embed the translations file in the executable (see [Embedding the Translations File](#embedding-the-translations-file)).

### Writing/Adapting Source Code (I18N)

When writing internationalized source code, the strings to be translated must be wrapped with a call to `I18N.DotNet.Global.Localize()`.

The easier and most convenient approach for writing internationalized software is to choose a language that will be used as the base language throughout the software development (e.g., English), and then write the software just as any non-internationalized source code, except that strings to be translated must be wrapped with calls to `Localize()`. This way the base language will act as the default language when translations are not available for the current target language.

Adapting exising non-internationalized source code is as easy as wrapping the existing strings to be translated with calls to `Localize()`.

###### Example (C#)
``` CS
using static I18N.DotNet.Global;
using System;
using System.IO;

public class Program
{
  static void Main( string[] args )
  {
    int i = 0x555;

    Console.WriteLine( Localize( "Plain string to be translated" ) );
    Console.WriteLine( Localize( $"Interpolated string to be translated with value {i:X4}" ) );
  }
}
```

### Writing Translations (L10N)

String translations must be stored in an XML file (the translations file) with root element `I18N`.

For each string than has been internationalized an `Entry` element under the root must be defined, with:

- A single `Key` child element which value is the internationalized string defined in the code (replacing for interpolated strings the interpolated expressions with their positional index).
- `Value`child elements with their attribute `lang` set to the target language of the translation and which value is the translated string.

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

> **NOTE**: The companion utility [I18N.DotNet Tool](https://github.com/SafeTwice/I18N.DotNet-Tool) can be used to ease the creation of the translations file by scanning source files and automatically generating entries for discovered internationalized strings.

### Embedding the Translations File

A very convenient way of distributing the translations for an application is to embedded the translations file in the executable assembly as an embedded resource identified by _Resources.I18N.xml_.

Using Visual Studio, the easiest way to achieve this is to name the translations file _"I18N.xml"_ and deploy it in a directory named  _"Resources"_ inside the VS project directory, and then configure the file in the VS project as an embedded resource (i.e., set its Build Action to "Embedded resource" in the IDE, or add `<EmbeddedResource Include="Resources\I18N.xml" />` to an `ItemGroup` in the project file).

###### Example (.csproj)
``` XML
<Project Sdk="Microsoft.NET.Sdk">
  ...
  <ItemGroup>
    <EmbeddedResource Include="Resources\I18N.xml" />
  </ItemGroup>
</Project>
```

> **NOTE**: The companion utility [I18N.DotNet Tool](https://github.com/SafeTwice/I18N.DotNet-Tool) can be used to generate translations files optimized for deployment from the separate translations files used during development and during the translation process.


## Advanced Usage

### Global Localizer

The static class `GlobalLocalizer` has the property `Localizer` which contains the global localizer. This instance is shared and can be conveniently used by all software components. In fact all the methods exposed by the `Global` class are just convenience wrappers that call the global localizer.

The instance of `Global.Localizer` is created automatically on first usage (if not having been set previously) with a default `Localizer` (see [Localizer Class](#localizer-class)). This default localizer has the target language set to the current UI language, and tries to load the translations file from an embedded resource identified by _Resources.I18N.xml_ inside the entry (application) assembly.

The default behavior is just right for most use cases, but `Global.Localizer` can be set with a different localizer during application startup or dynamically during execution (e.g., to use a different language than the current UI language, see [Specifying the Translation Target Language](#specifying-the-translation-target-language)).

Additionally, if the translations file is stored in an embedded resource with a different identifier, or in a separate file (e.g., installed alongside the application executable), the `LoadXML` method can be invoked on the global localizer to load it.

###### Non-Default usage Example (C#)
``` CS
void SetupI18N( string language, string directoryPath )
{
  Global.Localizer = new Localizer( language );
  Global.Localizer.LoadXML( directoryPath + "/I18N.xml" );
}
```

### Local Localizers

If necessary, additional instances of the `Localizer` class can be created (local localizers), loaded with string translations, and then passed to software components for being used instead of the global localizer.

For most cases using the global localizer (and optionally [Contexts](#contexts)) is just enough, but local localizers can be useful for example to implement report generation in different languages than the application UI language (see [Specifying the Translation Target Language](#specifying-the-translation-target-language)).

### Language Identifiers & Variants

Any arbitrary string can be used for identifying languages, although it is recommended to use identifiers formed by a ISO 639-1 alpha-2 language name (2-letter language codes, e.g., _"en"_, _"es"_), additionally followed by an hyphen and a ISO 3166-1 alpha-2 country/region name (e.g., _"en-US"_, _"es-ES"_).

Language identifiers are processed as case-insensitive (i.e., _"fr-FR"_ is equivalent to _"fr-fr"_).

When using language identifiers formed by a primary code and a variant code separated by an hyphen (e.g., _"en-us"_, _"es-es"_), if a localized conversion for the language variant is not found then a conversion for the primary (base) language is tried too.

For example, when loading the translations on a `Localizer` created for the _"en-gb"_ language, for each string to be translated a translation for the language _"en-gb"_ will be searched first, and if not found then a translation for the language _"en"_ will be searched next.

It is therefore recommended to:

- Use primary-variant code (e.g., _"en-us"_, _"es-es"_) as target language identifiers (i.e., as arguments to the `Localizer` constructor).
- Use primary code (e.g., _"en"_, _"fr"_) as translation language identifiers (i.e, as the `lang` attribute values of XML `I18N.Entry.Value` entries) for generic (non variant-specific) translations.
- Use primary code-variant (e.g., _"en-gb"_, _"es-ar"_) as translation language identifiers (i.e, as the `lang` attribute values of XML `I18N.Entry.Value` entries) for variant-specific translations.

### String Format

Calls to `String.Format()` where the format string has to be internationalized can be replaced by a call to `Global.LocalizeFormat()` or `ILocalizer.LocalizeFormat()`.

###### Example (C#)
``` CS
String.Format( Localize( "Format string to be translated with value {0}" ), myVar );
// is equivalent to
LocalizeFormat( "Format string to be translated with value {0}", myVar );
```

### Contexts

Sometimes the same source language string has different translations in different contexts (e.g., English _"OK"_ should be translated in Spanish to _"Aceptar"_ for a button label but to _"Correcto"_ for a successful outcome indication).

Since the source language key is the same in both cases, context partitioning must be used, which affects the source code side and the translations file side.

##### Context Partitioning in Source Code (I18N)

In source code, the context of the key can be explicitly indicated when the string is being internationalized by calling `Global.Context()` or `ILocalizer.Context()` and passing it the context identifier, and then calling the localization methods on the returned context `ILocalizer`.

Contexts can be nested. A chain of successively nested contexts can be identified by joining their identifiers using the dot character ('.') as a composite context identifier.

Translations in a context are searched hierarchically: if a translation is not found for the target language in is context (neither for the language variant nor the primary language), then a translation is searched again on its parent context (if it exists).

###### Example (C#)
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
    <Context id="Status">
      <Entry>
        <Key>OK</Key>
        <Value lang="es">Correcto</Value>
      </Entry>
    </Context>
  </Context>
</I18N>
```

### ILocalizer Interface

The `ILocalizer` interface represents classes which are responsible for providing localization functionality (i.e. perform string translations) for software components.

### Localizer Class

The `Localizer` class is a simple implementation of the `ILocalizer` [interface](#ilocalizer-interface), and is capable of loading string translations from a translations file for a single target language and then providing localization functionality.

#### Specifying the Translation Target Language

The default `Localizer` constructor sets the target language for translations to the current UI language (obtained from `System.Globalization.CultureInfo.CurrentUICulture`).

To specify a different target language use the `Localizer` constructor that accepts a language identifier as a parameter.

###### Example (C#)
``` CS
void SetupI18N( string language )
{
  Global.Localizer = new Localizer( language );
}
```

#### Loading the Translations File

The translations file can be loaded into a `Localizer` by different ways:

##### From an Embedded Resource

The easiest way of using translation files is to embed them into an executable assembly (application or library), then load them into a `Localizer` instance using the `LoadXML` method indicating the assembly to load the embedded resource from and its identifier.


> Note: The [global localizer](#global-localizer) will automatically try to load the translations file from an embedded resource identified by _Resources.I18N.xml_ in the entry assembly.

###### Example (C#)
``` CS
void SetupI18N()
{
  Global.Localizer.LoadXML( Assembly.GetExecutingAssembly(), "I18N.Translations.xml" );
}
```

##### From a Standalone File

If the translations file is stored as a separate file (e.g., installed alongside the application executable), the `LoadXML` method can be invoked on a `Localizer` instance passing the path to the file.

###### Example (C#)
``` CS
void SetupI18N()
{
  var programPath = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

  Global.Localizer.LoadXML( programPath + "/I18N.xml" );
}
```

##### From a Stream

When the translations file are neither stored as a file or embedded resource (e.g., downloading the translations from a remote server to local memory, obtaining the translations from a database), the `LoadXML` method can be invoked on a `Localizer` instance passing a `System.IO.Stream` object which must provide the file contents.


## Advanced Usage (Libraries)

### Library Localizers

The [global localizer](#global-localizer) is convenient for usage in applications (i.e., which are implemented in the entry assembly), but libraries should not use the [global localizer](#global-localizer) because they would depend on the application to load the translations for its internationalized strings or risk the application discarding the translations if trying to load them automatically.

For libraries the easiest solution is to define their own "global" localizer as a static property inside a static class, similar to the `Global` class but only intended for the scope of the library.

This library localizer can be initialized using an instance of `AutoLoadLocalizer`, which is a special localizer which automatically loads the translations file from an embedded resource (see [AutoLoadLocalizer Class](#autoloadlocalizer-class)).

The static class can be declared with `internal` scope, or with `public` scope to allow applications to extend or replace the library localizer (e.g., to add more translations, or to change them).

Finally, the translations file for the library must be embedded in the library assembly as an embedded resource identified by _Resources.I18N.xml_ (just like with an application), which the `AutoLoadLocalizer` instance will try to load by default.

###### Library Localizer Implementation Example (C#)
``` CS
using I18N.DotNet;
using System;

namespace ExampleLibrary
{
  public static class LibraryLocalizer
  {
    public static ILocalizer Localizer { get; set; } = new AutoLoadLocalizer();

    internal static string Localize( PlainString text ) => Localizer.Localize( text );
    internal static string Localize( FormattableString text ) => Localizer.Localize( text );
  }
}
```

###### Library Localizer Usage Example (C#)
``` CS
using static ExampleLibrary.LibraryLocalizer;
using System;

namespace ExampleLibrary
{
  public class ExampleClass
  {
    public void SomeMethod()
    {
      Console.WriteLine( Localize( "Plain string to be translated" ) );
      Console.WriteLine( Localize( $"Interpolated string to be translated with value {i:X4}" ) );
    }
  }
}
```

### AutoLoadLocalizer Class

The `AutoLoadLocalizer` class is an implementation of the `ILocalizer` [interface](#ilocalizer-interface) that loads automatically the translations file from an embedded resource in an assembly.

The default parameters for the `AutoLoadLocalizer` constructor make the created instance load the translations file from an embedded resource identified by _Resources.I18N.xml_ in the calling assembly (i.e., in the assembly that creates the instance).

A different resource identifier or assembly can be used as parameters to the `AutoLoadLocalizer` constructor if necessary.


## Full API Documentation

You can browse the full API documentation for:
 - [The last release (stable)](https://safetwice.github.io/I18N.DotNet/stable)
 - [Main branch (unstable)](https://safetwice.github.io/I18N.DotNet/main)
