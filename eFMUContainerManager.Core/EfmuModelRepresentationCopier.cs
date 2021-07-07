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
using System.Xml.Linq;
using eFMI.ManifestsAndContainers;
using eFMI.ManifestsAndContainers.ManifestFileListing;
using eFMI.Misc;

namespace eFMI.ContainerManager
{
    class EfmuModelRepresentationCopier : EfmuAbstractCopier
    {
        private EfmuContainerManifest ContainerManifest;

        private bool AddInsteadOfReplace;

        private string ModelRepresentationName;

        private string ManifestFileName;

        private bool ValidateXmlTree;
        private bool ValidateChecksums;


        /* Determined by Boot method. */
        private string ManifestPath;
        private XDocument manifestDoc;

        /* Determined by Boot method. */
        private EfmuModelRepresentationKind ModelRepresentationKind;


        /* optionalModelRepresentationName needed for "replace" */
        public EfmuModelRepresentationCopier(string inputDir,
                                             string outputDir,
                                             EfmuContainerManifest containerManifest,
                                             bool addInsteadOfReplace,
                                             string modelRepresentationName,
                                             string manifestFileName,
                                             bool validateXmlTree,
                                             bool validateChecksums) :
                                                    base(inputDir, outputDir)
        {
            this.ContainerManifest = containerManifest;
            this.AddInsteadOfReplace = addInsteadOfReplace;
            this.ModelRepresentationName = modelRepresentationName;
            this.ManifestFileName = manifestFileName;
            this.ValidateXmlTree = validateXmlTree;
            this.ValidateChecksums = validateChecksums;
        }

        public override bool Boot()
        {
            bool success = !HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>> EfmuModelRepresentationCopier::Boot");

            if (success)
            {
                success = PerformCommonBootChecks();
            }

            if (success)
            {
                success = EfmuPathNames.CheckThatPathIsFileName(ManifestFileName);
            }

            if (success)
            {
                ManifestPath = Path.Combine(InputDir, ManifestFileName);
                success = EfmuFilesystem.CheckForExistingFile(ManifestPath, "Manifest XML");
            }

            if (success)
            {
                success = EfmuXmlTools.LoadToXDocument(ManifestPath, ref manifestDoc);

                if (success && ValidateXmlTree)
                {
                    /* Perform validation as if the manifest file would already be located
                     * in the output directory!
                     * The relative path to the schema file is extracted from the manifest file
                     * as already done for other validation runs!
                     */
                    success = EfmuManifestValidation.PerformValidationOfManifestXmlDocument(manifestDoc,
                        null,
                        OutputDir,
                        "input model representation manifest XML file");
                }

                if (success)
                {
                    /* Determine manifest file listing from manifest file,
                     * because this implies that existance and checksums of file entries are checked
                     * implicitly when creating the file entries.
                     */
                    EfmuManifestFileListing fileListing = null;
                    success = EfmuFileListingFromXml.DetermineManifestFileListing(manifestDoc,
                        InputDir,
                        ValidateChecksums, /* TODO: Fix checksums on-the-fly */
                        ref fileListing);
                }
            }

            if (success)
            {
                string kindString = null;
                success = EfmuXmlTools.GetAttributeValue(manifestDoc.Root, "kind", ref kindString);
                
                if (success && !EfmuModelRepresentationKind.TryParse(kindString, out ModelRepresentationKind))
                {
                    EfmuConsoleWriter.WriteErrorLine($"Invalid kind for input model representation: {kindString}");
                    success = false;
                }

                if (success)
                {
                    EfmuConsoleWriter.WriteDebugLine($"Recognized kind of input model representation: {ModelRepresentationKind}");
                }
            }

            HasBeenBootedSuccessfully = success;
            return success;
        }

        public override bool Run()
        {
            bool success = HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>> EfmuModelRepresentationCopier::Run");

            if (success)
            {
                EfmuConsoleWriter.WriteDebugLine("");
                if (AddInsteadOfReplace)
                {
                    EfmuConsoleWriter.WriteInfoLine(">> Copying model representation subtree");
                }
                else
                {
                    EfmuConsoleWriter.WriteInfoLine(">> Replacing model representation subtree");
                    bool dummyResult = EfmuContainerTools.PerformCallWithExceptionHandling(EfmuFilesystem.RemoveDirectory, OutputDir);
                    ContainerManifest.RemoveModelRepresentation(ModelRepresentationName);
                }
            }

            if (success)
            {
                EfmuConsoleWriter.WriteInfoLine($"InputDir: {InputDir}");
                EfmuConsoleWriter.WriteInfoLine($"OutputDir: {OutputDir}");
                EfmuFilesystem.CopyDirectory(InputDir, OutputDir, true);
                    //, EfmuContainerProperties.EfmuFileSuffix, false);
            }

            string checksum = null;
            if (success)
            {
                /* Compute checksum of manifest and add model representation to container manifest */
                EfmuConsoleWriter.WriteDebugLine("");
                EfmuConsoleWriter.WriteInfoLine($"> Computing checksum of manifest");
                checksum = EfmuChecksum.ComputeChecksumOfFile(ManifestPath);
                EfmuConsoleWriter.WriteDebugLine($" => checksum: {checksum}");
            }

            string manifestId = null;
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(manifestDoc.Root, "id", ref manifestId);
            }

            if (success)
            {
                EfmuModelRepresentation modelRepresentation = new EfmuModelRepresentation(
                                                                        ModelRepresentationName,
                                                                        ModelRepresentationKind,
                                                                        ManifestFileName,
                                                                        checksum,
                                                                        manifestId);
                if (success)
                {
                    success = ContainerManifest.AddModelRepresentation(modelRepresentation);
                }
            }

            return success;
        }
    }
}
