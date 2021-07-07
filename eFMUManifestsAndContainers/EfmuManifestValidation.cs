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

using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.Misc;

namespace eFMI.ManifestsAndContainers
{
    public class EfmuManifestValidation
    {
        private static readonly XNamespace XsdNamespaceUrl = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XName XsdNamespaceAttribName = XsdNamespaceUrl + "noNamespaceSchemaLocation";

        /* This method must be called by classes creating an XML document associated with a schema.
         * It must be called on the root element.
         */
        public static void SetXmlnsXsiAttribute(XElement rootElem)
        {
            rootElem.SetAttributeValue(XNamespace.Xmlns + "xsi", XsdNamespaceUrl.ToString());
        }

        /* This method must be called by classes creating an XML document associated with a schema.
         * It must be called on the root element.
         */
        public static void SetXsdNsAttribute(XElement rootElem, string schemaFileRelPath)
        {
            rootElem.SetAttributeValue(XsdNamespaceAttribName, schemaFileRelPath);
        }


        /* Used by PerformValidationOfManifestXmlDocument */
        private static bool GetAndCheckValueOfNamespaceAttribute(XElement rootElem,
                                                                    ref string schemaFileRelUrl)
        {
            bool success = true;

            XAttribute nsAttribute = rootElem.Attribute(XsdNamespaceAttribName);
            if (null == nsAttribute)
            {
                EfmuConsoleWriter.WriteErrorLine($"Attribute for XsdNamespace is missing: {XsdNamespaceAttribName}");
                success = false;
            }
            else
            {
                schemaFileRelUrl = nsAttribute.Value;
            }

            if (success)
            {
                string schemaDirNameInContainer = EfmuContainerProperties.SchemaDirNameInContainer;
                if (!schemaFileRelUrl.StartsWith(schemaDirNameInContainer)
                    && !schemaFileRelUrl.StartsWith("./" + schemaDirNameInContainer)
                    && !schemaFileRelUrl.StartsWith("../" + schemaDirNameInContainer)
                )
                {
                    EfmuConsoleWriter.WriteErrorLine($"Link to schema file does not refer to schema file of EFMU container: {schemaFileRelUrl}");
                    EfmuConsoleWriter.WriteErrorLine($" Attribute: {XsdNamespaceAttribName}");
                    success = false;
                }
            }

            return success;
        }

        /* Perform validation of the given manifest XML document using one of the following schema files:
         * (1) optionalSchemaFilePath != null: use this file
         * (2) otherwise optionalRootDirInTempContainerDir != null:
         *          combine it with value of attribute "xsi:noNamespaceSchemaLocation" in doc's root element
         *
         * It is assumed that the manifest XML document becomes or is part of an EFMU container,
         * i.e. the value of the attribute <XsdNamespaceAttribName> must be a relative path
         * to a schema file in the "schemas" directory.
         */
        public static bool PerformValidationOfManifestXmlDocument(XDocument doc,
                                                                    string optionalSchemaFilePath = null,
                                                                    string optionalRootDirInTempContainerDir = null,
                                                                    string optionalDescr = null)
        {
            bool success = true;

            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteInfoLine("> Validating XML tree against schema" + ((null != optionalDescr) ? $" ({optionalDescr})" : ""));

            XElement rootElem = doc.Root;
            string schemaFilePath = null;
            if (null != optionalSchemaFilePath)
            {
                string dummyString = null;
                success = GetAndCheckValueOfNamespaceAttribute(rootElem, ref dummyString);
                if (success)
                {
                    schemaFilePath = optionalSchemaFilePath;
                }
            }
            else if (null != optionalRootDirInTempContainerDir)
            {
                string schemaFileRelUrl = null;
                success = GetAndCheckValueOfNamespaceAttribute(rootElem, ref schemaFileRelUrl);

                if (success)
                {
                    EfmuConsoleWriter.WriteDebugLine($"schemaFileRelUrl: {schemaFileRelUrl}");
                    string schemaFileRelPath = schemaFileRelUrl.Replace('/', Path.DirectorySeparatorChar);
                    EfmuConsoleWriter.WriteDebugLine($"schemaFileRelPath: {schemaFileRelPath}");

                    string schemaDirNameInContainer = EfmuContainerProperties.SchemaDirNameInContainer;
                    if (schemaFileRelPath.StartsWith(".." + Path.DirectorySeparatorChar + schemaDirNameInContainer))
                    {
                        schemaFileRelPath = schemaFileRelPath.Substring(3);
                        optionalRootDirInTempContainerDir = Path.GetDirectoryName(optionalRootDirInTempContainerDir);
                        schemaFilePath = Path.Combine(optionalRootDirInTempContainerDir, schemaFileRelPath);
                    }
                    else if (schemaFileRelPath.StartsWith("." + Path.DirectorySeparatorChar + schemaDirNameInContainer))
                    {
                        schemaFileRelPath = schemaFileRelPath.Substring(2);
                        schemaFilePath = Path.Combine(optionalRootDirInTempContainerDir, schemaFileRelPath);
                    }
                    else
                    {
                        schemaFilePath = Path.Combine(optionalRootDirInTempContainerDir, schemaFileRelPath);
                    }
                }
            }
            else
            {
                EfmuConsoleWriter.WriteErrorLine("No XML schema file can be used");
                success = false;
            }

            if (success)
            {
                EfmuConsoleWriter.WriteDebugLine($"schemaFilePath: {schemaFilePath}");
                if (!EfmuFilesystem.DoesFileExist(schemaFilePath))
                {
                    EfmuConsoleWriter.WriteErrorLine(
                        $"The XML schema file to be validated against does not exist: {schemaFilePath}");
                    success = false;
                }
            }

            if (success)
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", schemaFilePath);

                bool hasLineInfos = ((IXmlLineInfo) rootElem).HasLineInfo();

                uint validationErrors = 0;
                doc.Validate(schemas, (sender, e) =>
                {
                    int linenumber = e.Exception.LineNumber;
                    if (hasLineInfos)
                    {
                        EfmuConsoleWriter.WriteErrorLine($"Validation failed in line {linenumber}: " + e.Message);
                    }
                    else
                    {
                        EfmuConsoleWriter.WriteErrorLine($"Validation failed in unknown line: " + e.Message);
                    }
                    ++validationErrors;
                });
                if (0 == validationErrors)
                {
                    EfmuConsoleWriter.WriteInfoLine("=> XML tree has been validated successfully");
                }
                else
                {
                    success = false;
                    EfmuConsoleWriter.WriteErrorLineNoPrefix("(The error message(s) stem from from XDocument.Validate(..) )");
                    EfmuConsoleWriter.WriteErrorLineNoPrefix($"=> Validation encountered {validationErrors} error(s)");
                }
            }

            return success;
        }
    }
}
