/// @file
/// @copyright  Copyright (c) 2023-2024 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace I18N.DotNet
{
    /// <summary>
    /// Localizer which translations can be loaded from different sources.
    /// </summary>
    public interface ILoadableLocalizer : ILocalizer
    {
        //===========================================================================
        //                          PUBLIC NESTED TYPES
        //===========================================================================

        /// <summary>
        /// Exception thrown when a localization file cannot be parsed properly. 
        /// </summary>
        public class ParseException : Exception
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="message">A message that describes the error.</param>
            public ParseException( string message ) : base( message ) { }
        }

        //===========================================================================
        //                                  METHODS
        //===========================================================================

        /// <summary>
        /// Loads translations for the given <paramref name="culture"/> from a localization configuration file in XML format.
        /// </summary>
        /// <remarks>
        /// All the translations loaded previously in the localizer are discarded and replaced with the new ones.
        /// </remarks>
        /// <param name="filepath">Path to the localization configuration file</param>
        /// <param name="culture">Culture for the target language of translations,
        ///                       or <c>null</c> to use the current UI culture (obtained from <see cref="CultureInfo.CurrentUICulture"/>)</param>
        /// <exception cref="ParseException">Thrown when the input file cannot be parsed properly.</exception>
        void LoadXML( string filepath, CultureInfo? culture = null );

        /// <summary>
        /// Loads translations for the given <paramref name="language"/> from a localization configuration file in XML format.
        /// </summary>
        /// <remarks>
        /// <para>All the translations loaded previously in the localizer are discarded and replaced with the new ones.</para>
        /// <para>If the system does not support a culture for <paramref name="language"/>, then <see cref="CultureInfo.InvariantCulture"/>
        /// will be used as the culture for formatting operations.</para>
        /// </remarks>
        /// <param name="filepath">Path to the localization configuration file</param>
        /// <param name="language">Name, code or identifier for the target language of translations</param>
        /// <exception cref="ParseException">Thrown when the input file cannot be parsed properly.</exception>
        void LoadXML( string filepath, string language );

        /// <summary>
        /// Loads translations for the current localizer language from a localization configuration file in XML format.
        /// </summary>
        /// <param name="filepath">Path to the localization configuration file</param>
        /// <param name="merge">Replaces the current translations with the loaded ones when<c>false</c>,
        ///                     otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the input file cannot be parsed properly.</exception>
        void LoadXML( string filepath, bool merge );

        /// <summary>
        /// Loads translations for the given <paramref name="culture"/> from a localization configuration file in XML format obtained from a stream.
        /// </summary>
        /// <remarks>
        /// All the translations loaded previously in the localizer are discarded and replaced with the new ones.
        /// </remarks>
        /// <param name="stream">Stream with the localization configuration</param>
        /// <param name="culture">Culture for the target language of translations,
        ///                       or <c>null</c> to use the current UI culture (obtained from <see cref="CultureInfo.CurrentUICulture"/>)</param>
        /// <exception cref="ParseException">Thrown when the stream contents cannot be parsed properly.</exception>
        void LoadXML( Stream stream, CultureInfo? culture = null );

        /// <summary>
        /// Loads translations for the given <paramref name="language"/> from a localization configuration file obtained in XML format from a stream.
        /// </summary>
        /// <remarks>
        /// <para>All the translations loaded previously in the localizer are discarded and replaced with the new ones.</para>
        /// <para>If the system does not support a culture for <paramref name="language"/>, then <see cref="CultureInfo.InvariantCulture"/>
        /// will be used as the culture for formatting operations.</para>
        /// </remarks>
        /// <param name="stream">Stream with the localization configuration</param>
        /// <param name="language">Name, code or identifier for the target language of translations</param>
        /// <exception cref="ParseException">Thrown when the stream contents cannot be parsed properly.</exception>
        void LoadXML( Stream stream, string language );

        /// <summary>
        /// Loads translations for the current localizer language from a localization configuration file in XML format obtained from a stream.
        /// </summary>
        /// <param name="stream">Stream with the localization configuration</param>
        /// <param name="merge">Replaces the current translations with the loaded ones when<c>false</c>,
        ///                     otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the stream contents cannot be parsed properly.</exception>
        void LoadXML( Stream stream, bool merge );

        /// <summary>
        /// Loads translations for the given <paramref name="culture"/> from a localization configuration in an XML document.
        /// </summary>
        /// <remarks>
        /// All the translations loaded previously in the localizer are discarded and replaced with the new ones.
        /// </remarks>
        /// <param name="doc">XML document with the localization configuration</param>
        /// <param name="culture">Culture for the target language of translations,
        ///                       or <c>null</c> to use the current UI culture (obtained from <see cref="CultureInfo.CurrentUICulture"/>)</param>
        /// <exception cref="ParseException">Thrown when the input document cannot be parsed properly.</exception>
        void LoadXML( XDocument doc, CultureInfo? culture = null );

        /// <summary>
        /// Loads translations for the given <paramref name="language"/> from a localization configuration in an XML document.
        /// </summary>
        /// <remarks>
        /// <para>All the translations loaded previously in the localizer are discarded and replaced with the new ones.</para>
        /// <para>If the system does not support a culture for <paramref name="language"/>, then <see cref="CultureInfo.InvariantCulture"/>
        /// will be used as the culture for formatting operations.</para>
        /// </remarks>
        /// <param name="doc">XML document with the localization configuration</param>
        /// <param name="language">Name, code or identifier for the target language of translations</param>
        /// <exception cref="ParseException">Thrown when the input document cannot be parsed properly.</exception>
        void LoadXML( XDocument doc, string language );

        /// <summary>
        /// Loads translations for the current localizer language from a localization configuration in an XML document.
        /// </summary>
        /// <param name="doc">XML document with the localization configuration</param>
        /// <param name="merge">Replaces the current translations with the loaded ones when<c>false</c>,
        ///                     otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the stream contents cannot be parsed properly.</exception>
        void LoadXML( XDocument doc, bool merge );

        /// <summary>
        /// Loads translations for the given <paramref name="culture"/> from a localization configuration file in XML format obtained from an embedded resource in the given assembly.
        /// </summary>
        /// <remarks>
        /// All the translations loaded previously in the localizer are discarded and replaced with the new ones.
        /// </remarks>
        /// <param name="assembly">Assembly that contains the embedded XML file</param>
        /// <param name="resourceName">Name of the embedded resource for the XML file</param>
        /// <param name="culture">Culture for the target language of translations,
        ///                       or <c>null</c> to use the current UI culture (obtained from <see cref="CultureInfo.CurrentUICulture"/>)</param>
        /// <exception cref="ParseException">Thrown when the embedded resource contents cannot be parsed properly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the embedded resource could not be found in the given assembly.</exception>
        void LoadXML( Assembly assembly, string resourceName, CultureInfo? culture = null );

        /// <summary>
        /// Loads translations for the given <paramref name="language"/> from a localization configuration file in XML format obtained from an embedded resource in the given assembly.
        /// </summary>
        /// <remarks>
        /// <para>All the translations loaded previously in the localizer are discarded and replaced with the new ones.</para>
        /// <para>If the system does not support a culture for <paramref name="language"/>, then <see cref="CultureInfo.InvariantCulture"/>
        /// will be used as the culture for formatting operations.</para>
        /// </remarks>
        /// <param name="assembly">Assembly that contains the embedded XML file</param>
        /// <param name="resourceName">Name of the embedded resource for the XML file</param>
        /// <param name="language">Name, code or identifier for the target language of translations</param>
        /// <exception cref="ParseException">Thrown when the embedded resource contents cannot be parsed properly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the embedded resource could not be found in the given assembly.</exception>
        void LoadXML( Assembly assembly, string resourceName, string language );

        /// <summary>
        /// Loads translations for the current localizer language from a localization configuration file in XML format obtained from an embedded resource in the given assembly.
        /// </summary>
        /// <param name="assembly">Assembly that contains the embedded XML file</param>
        /// <param name="resourceName">Name of the embedded resource for the XML file</param>
        /// <param name="merge">Replaces the current translations with the loaded ones when<c>false</c>,
        ///                     otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the stream contents cannot be parsed properly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the embedded resource could not be found in the given assembly.</exception>
        void LoadXML( Assembly assembly, string resourceName, bool merge );
    }
}
