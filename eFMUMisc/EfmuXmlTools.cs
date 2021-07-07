/*
 * Copyright 2021 dSPACE GmbH
 *
 * Licensed under the 3-Clause BSD license(the \"License\");    
 * you may not use this software except in compliance with    
 * the License.                                               
 *                                                            
 * This software is not fully developed or tested.            
 *                                                            
 * THE SOFTWARE IS PROVIDED \"as is\", in the hope that it may  
 * be useful to other users, without any warranty of any      
 * kind, either express or implied.                           
 *                                                            
 * See the License for the specific language governing        
 * permissions and limitations under the License.             
 */

using System.Xml;
using System.Xml.Linq;

namespace eFMI.Misc
{
    public class EfmuXmlTools
    {
        public static bool LoadToXDocument(string filePath, ref XDocument doc)
        {
            bool success = true;

            try
            {
                doc = XDocument.Load(filePath, LoadOptions.SetLineInfo);
            }
            catch (XmlException e)
            {
                EfmuConsoleWriter.DumpException(e, $"Caught exception during loading XML document '{filePath}'");
                throw;
            }

            return success;
        }

        public static void PrintErrorDetailsForElement(XElement elem)
        {
            EfmuConsoleWriter.WriteErrorLineNoPrefix($" Element in line {((IXmlLineInfo)elem).LineNumber}:");
            EfmuConsoleWriter.WriteErrorLineNoPrefix(" " + elem.ToString());
        }

        public static bool GetAttributeValue(XElement elem,
                                             string attributeName,
                                             ref string value)
        {
            XAttribute attribute = elem.Attribute(attributeName);
            if (null != attribute)
            {
                value = attribute.Value;
                return true;
            }
            else
            {
                EfmuConsoleWriter.WriteErrorLine($"Could not find XML attribute '{attributeName}'");
                PrintErrorDetailsForElement(elem);
                return false;
            }
        }

        /* Not used currently.
         * For parsing boolean attributes of manifests use the specific methods!
         */
        /*public static bool GetBooleanAttributeValue(XElement elem,
                                                    string attributeName,
                                                    ref bool value)
        {
            string valueString = null;
            bool success = GetAttributeValue(elem, attributeName, ref valueString);
            if (success)
            {
                success = EfmuStringToValueParser.ParseBoolean(valueString, out value);
            }

            return success;
        }*/

        public static bool GetLongIntegerAttributeValue(XElement elem,
                                                        string attributeName,
                                                        ref long value)
        {
            string valueString = null;
            bool success = GetAttributeValue(elem, attributeName, ref valueString);
            if (success)
            {
                success = EfmuStringToValueParser.ParseLongInteger(valueString, out value);
            }

            return success;
        }

        public static bool GetDoubleAttributeValue(XElement elem,
                                                    string attributeName,
                                                    ref double value)
        {
            string valueString = null;
            bool success = GetAttributeValue(elem, attributeName, ref valueString);
            if (success)
            {
                success = EfmuStringToValueParser.ParseDouble(valueString, out value);
            }

            return success;
        }

        /* Results of GetOptional... methods:
         * - hasAttribute: signals whether attribute exists,
         *      i.e. hasAttribute may be set to true, but function result may be false, because value parsing failed.
         */

        public static void GetOptionalAttributeValue(XElement elem,
                                                       string attributeName,
                                                       ref string value,
                                                       out bool hasAttribute)
        {
            XAttribute attribute = elem.Attribute(attributeName);
            if (null != attribute)
            {
                value = attribute.Value;
                hasAttribute = true;
            }
            else
            {
                hasAttribute = false;
            }
        }

        /* Like GetOptionalAttributeValue, but sets "value" to null
         * if the attribute does not exist.
         */
        public static void GetOptionalAttributeValueForce(XElement elem,
                                                            string attributeName,
                                                            out string value)
        {
            XAttribute attribute = elem.Attribute(attributeName);
            if (null != attribute)
            {
                value = attribute.Value;
            }
            else
            {
                value = null;
            }
        }

        /* Not used currently.
         * For parsing boolean attributes of manifests use the specific methods!
         */
        /*public static bool GetOptionalBooleanAttributeValue(XElement elem,
                                                            string attributeName,
                                                            ref bool value,
                                                            out bool hasAttribute)
        {
            bool success = true;

            string valueString = null;
            GetOptionalAttributeValue(elem, attributeName, ref valueString, out hasAttribute);
            if (success && hasAttribute)
            {
                success = EfmuStringToValueParser.ParseBoolean(valueString, out value);
            }

            return success;
        }*/

        public static bool GetOptionalLongIntegerAttributeValue(XElement elem,
                                                                string attributeName,
                                                                ref long value,
                                                                out bool hasAttribute)
        {
            bool success = true;

            string valueString = null;
            GetOptionalAttributeValue(elem, attributeName, ref valueString, out hasAttribute);
            if (success && hasAttribute)
            {
                success = EfmuStringToValueParser.ParseLongInteger(valueString, out value);
            }

            return success;
        }

        public static bool GetOptionalDoubleAttributeValue(XElement elem,
                                                            string attributeName,
                                                            ref double value,
                                                            out bool hasAttribute)
        {
            bool success = true;

            string valueString = null;
            GetOptionalAttributeValue(elem, attributeName, ref valueString, out hasAttribute);
            if (success && hasAttribute)
            {
                success = EfmuStringToValueParser.ParseDouble(valueString, out value);
            }

            return success;
        }

        /* Only used rarely currently, but makes code more readable */
        public static bool GetOptionalChildElement(XElement elem,
                                                    string childName,
                                                    ref XElement childElem)
        {
            childElem = elem.Element(childName);
            return null != childElem;
        }
    }
}
