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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.ManifestsAndContainers.ManifestTools;
using eFMI.Misc;

namespace eFMI.ManifestsAndContainers.ManifestFileListing
{
    public class EfmuFileListingFromXml
    {
        private static bool DetermineFilesElement(XDocument manifestDoc, ref XElement filesElem)
        {
            bool result = true;

            XElement rootElem = manifestDoc.Root;
            IEnumerable<XElement> elems =
                from elem in rootElem.Elements()
                where elem.Name.ToString().Equals(EfmuCommonManifestProperties.FilesElementName)
                select elem;
            if (0 == elems.Count())
            {
                result = false;
            }
            else
            {
                filesElem = elems.First();
            }

            return result;
        }


        /* Used by DetermineManifestFileListingForFilesElem. */
        private static bool AugmentFileListingByFileEntries(IEnumerable<XElement> fileElems,
                                                                string subtreePath,
                                                                bool validateChecksums,
                                                                EfmuManifestFileListing fileListing)
        {
            bool success = true;

            bool overAllSuccess = true;

            foreach (XElement fileElem in fileElems)
            {
                string id = null;
                string name = null;
                string path = null;
                bool needsChecksum = false;
                string checksum = null;
                EfmuFileEntryRole role = EfmuFileEntryRole.other;

                if (success)
                {
                    success = EfmuXmlTools.GetAttributeValue(fileElem, "id", ref id);
                }

                if (success)
                {
                    success = EfmuXmlTools.GetAttributeValue(fileElem, "name", ref name);
                }

                if (success)
                {
                    success = EfmuXmlTools.GetAttributeValue(fileElem, "path", ref path);
                }

                if (success)
                {
                    success = EfmuManifestTools.GetBooleanAttributeValue(fileElem,
                                                                            "needsChecksum",
                                                                            ref needsChecksum);
                }

                if (success)
                {
                    bool dummyHasAttribute = false;
                    EfmuXmlTools.GetOptionalAttributeValue(fileElem,
                                                           "checksum",
                                                           ref checksum,
                                                           out dummyHasAttribute);
                }

                if (success)
                {
                    string roleString = null;
                    success = EfmuXmlTools.GetAttributeValue(fileElem, "role", ref roleString);
                    if (success)
                    {
                        success = EfmuFileEntryRole.TryParse(roleString, out role);
                    }
                }

                if (success)
                {
                    EfmuFileListingEntry fileEntry = null;
                    success = EfmuFileListingEntry.CreateFileListingEntryFromXml(subtreePath,
                                                                            name,
                                                                            path,
                                                                            needsChecksum,
                                                                            checksum,
                                                                            role,
                                                                            null,
                                                                            validateChecksums,
                                                                            ref fileEntry);
                    if (success)
                    {
                        fileEntry.SetId(id);
                        success = fileListing.AddFileEntry(fileEntry);
                    }
                    else
                    {
                        // Allow for checking all files and compute checksums.
                        overAllSuccess = false;
                        success = true;
                    }
                }

                if (!success)
                {
                    break;
                }
            }

            return (success && overAllSuccess);
        }

        private static bool DetermineManifestFileListingForFilesElem(XElement filesElem,
                                                                        string subtreePath,
                                                                        bool validateChecksums,
                                                                        ref EfmuManifestFileListing fileListing)
        {
            bool success = true;

            IEnumerable<XElement> elems =
                from elem in filesElem.Elements()
                where elem.Name.ToString().Equals(EfmuCommonManifestProperties.FileElementName)
                select elem;
            if (0 == elems.Count())
            {
                /* According to the schema "efmiFiles.xsd",
                 * there msut be at least one (non-foreign) file!
                 */
                EfmuConsoleWriter.WriteErrorLine("File listing does not contain any files");
                success = false;
            }

            if (success)
            {
                fileListing = new EfmuManifestFileListing();
                success = AugmentFileListingByFileEntries(elems,
                    subtreePath,
                    validateChecksums,
                    fileListing);
            }

            if (!success)
            {
                EfmuConsoleWriter.WriteErrorLine("Could not determine file listing from manifest");
            }

            return success;
        }


        public static bool DetermineManifestFileListing(XDocument manifestDoc,
                                                        string subtreePath,
                                                        bool validateChecksums,
                                                        ref EfmuManifestFileListing fileListing)
        {
            bool success = true;

            XElement filesElem = null;
            if (EfmuFileListingFromXml.DetermineFilesElement(manifestDoc, ref filesElem))
            {
                fileListing = null;
                success = EfmuFileListingFromXml.DetermineManifestFileListingForFilesElem(filesElem,
                    subtreePath,
                    validateChecksums,
                    ref fileListing);
            }
            else
            {
                fileListing = new EfmuManifestFileListing();
            }

            return success;
        }
    }
}
