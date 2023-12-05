/// @file
/// @copyright  Copyright (c) 2022 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace System.Xml.XPath
{
    public static class ExtraExtensions
    {
        /// <summary>
        /// Selects a collection of attributes using an XPath expression.
        /// </summary>
        /// <param name="node">The System.Xml.Linq.XNode on which to evaluate the XPath expression.</param>
        /// <param name="expression">A System.String that contains an XPath expression.</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 of System.Xml.Linq.XElement that contains the selected attributes.</returns>
        public static IEnumerable<XAttribute> XPathSelectAttributes( this XNode node, string expression )
        {
            return ( (IEnumerable<object>) node.XPathEvaluate( expression ) ).OfType<XAttribute>();
        }

        /// <summary>
        /// Selects a collection of comments using an XPath expression.
        /// </summary>
        /// <param name="node">The System.Xml.Linq.XNode on which to evaluate the XPath expression.</param>
        /// <param name="expression">A System.String that contains an XPath expression.</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 of System.Xml.Linq.XElement that contains the selected comments.</returns>
        public static IEnumerable<XComment> XPathSelectComments( this XNode node, string expression )
        {
            return ( (IEnumerable<object>) node.XPathEvaluate( expression ) ).OfType<XComment>();
        }
    }
}
