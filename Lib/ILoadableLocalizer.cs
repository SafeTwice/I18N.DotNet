/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
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
        /// Loads a localization configuration from a file in XML format.
        /// </summary>
        /// <param name="filepath" > Path to the localization configuration file in XML format</param>
        /// <param name="language">Name, code or identifier for the target language of translations,
        ///                        or <c>null</c> to use the current UI language (obtained from <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>)</param>
        /// <param name="merge"> Replaces the current translations with the loaded ones when<c>false</c>,
        ///                      otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the input file cannot be parsed properly.</exception>
        void LoadXML( string filepath, string? language = null, bool merge = true );

        /// <summary>
        /// Loads a localization configuration from a stream in XML format.
        /// </summary>
        /// <param name="stream">Stream with the localization configuration in XML format</param>
        /// <param name="language">Name, code or identifier for the target language of translations,
        ///                        or <c>null</c> to use the current UI language (obtained from <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>)</param>
        /// <param name="merge"> Replaces the current translations with the loaded ones when<c>false</c>,
        ///                      otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the stream contents cannot be parsed properly.</exception>
        public void LoadXML( Stream stream, string? language = null, bool merge = true );

        /// <summary>
        /// Loads a localization configuration from a XML document.
        /// </summary>
        /// <param name="doc">XML document with the localization configuration</param>
        /// <param name="language">Name, code or identifier for the target language of translations,
        ///                        or <c>null</c> to use the current UI language (obtained from <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>)</param>
        /// <param name="merge"> Replaces the current translations with the loaded ones when<c>false</c>,
        ///                      otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the input document cannot be parsed properly.</exception>
        public void LoadXML( XDocument doc, string? language = null, bool merge = true );

        /// <summary>
        /// Loads a localization configuration from an XML text embedded as a resource in the given assembly.
        /// </summary>
        /// <param name="assembly">Assembly that contains the embedded XML text</param>
        /// <param name="resourceName">Name of the embedded resource for the XML text</param>
        /// <param name="language">Name, code or identifier for the target language of translations,
        ///                        or <c>null</c> to use the current UI language (obtained from <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>)</param>
        /// <param name="merge"> Replaces the current translations with the loaded ones when<c>false</c>,
        ///                      otherwise merges both (existing translations are overridden with loaded ones).</param>
        /// <exception cref="ParseException">Thrown when the embedded resource contents cannot be parsed properly.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the embedded resource could not be found in the given assembly</exception>
        public void LoadXML( Assembly assembly, string resourceName, string? language = null, bool merge = true );
    }
}
