/// @file
/// @copyright  Copyright (c) 2023 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Collections.Generic;
using System.Reflection;

namespace I18N.DotNet
{
    /// <summary>
    /// Implementation of a localizer which configuration is automatically loaded from an embedded resource.
    /// </summary>
    public class AutoLoadLocalizer : ILocalizer
    {
        //===========================================================================
        //                          PUBLIC CONSTRUCTORS
        //===========================================================================

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resourceName">Name of the embedded resource for the XML text</param>
        /// <param name="assembly">Assembly that contains the embedded XML text (the calling assembly will be used if <c>null</c>)</param>
        public AutoLoadLocalizer( string resourceName = "Resources.I18N.xml", Assembly? assembly = null )
        {
            m_assembly = assembly ?? Assembly.GetCallingAssembly();

            m_resourceName = resourceName;
        }

        //===========================================================================
        //                            PUBLIC METHODS
        //===========================================================================

        /// <inheritdoc/>
        public string Localize( PlainString text ) => InternalLocalizer.Localize( text );

        /// <inheritdoc/>
        public string Localize( FormattableString frmtText ) => InternalLocalizer.Localize( frmtText );

        /// <inheritdoc/>
        public IEnumerable<string> Localize( IEnumerable<string> texts ) => InternalLocalizer.Localize( texts );

        /// <inheritdoc/>
        public string LocalizeFormat( string format, params object[] args ) => InternalLocalizer.LocalizeFormat( format, args );

        /// <inheritdoc/>
        public ILocalizer Context( string contextId ) => InternalLocalizer.Context( contextId );

        /// <inheritdoc/>
        public ILocalizer Context( IEnumerable<string> splitContextIds ) => InternalLocalizer.Context( splitContextIds );

        //===========================================================================
        //                           PRIVATE PROPERTIES
        //===========================================================================

        private Localizer InternalLocalizer
        {
            get
            {
                if( m_internalLocalizer == null )
                {
                    m_internalLocalizer = new Localizer();
                    m_internalLocalizer.LoadXML( m_assembly, m_resourceName );
                }
                return m_internalLocalizer;
            }
        }

        //===========================================================================
        //                           PRIVATE ATTRIBUTES
        //===========================================================================

        private readonly Assembly m_assembly;
        private readonly string m_resourceName;

        private Localizer? m_internalLocalizer;
    }
}
