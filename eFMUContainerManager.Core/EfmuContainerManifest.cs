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

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using eFMI.ManifestsAndContainers;
using eFMI.ManifestsAndContainers.ManifestFileListing;
using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.ManifestsAndContainers.ManifestTools;
using eFMI.Misc;

namespace eFMI.ContainerManager
{
    /* This class manages a manifest of an EFMU container.
     * It is created for a certain temporary directory which is used
     * to create or read/evaluate or read/manipulate/write a container.
     * The constructor only initializes some properties which depend on the
     * location of the temporary directory.
     * The resulting instance of the class can be used as follows:
     * (1) read manifest XML file (file path already determined from temporary directory)
     * (2) write manifest XML file (dto.)
     * (3) evaluate or manipulate the model representations
     */
    class EfmuContainerManifest
    {
        private string Id;
        private string XsdVersion;
        private string EfmiVersion;
        private string Name;
        private string CreationDateAndTime;

        /* Name of model representation whose FMU is currently unpacked in the root directory of the container.
         * If no FMU is unpacked currently, the value is null.
         */
        private string ActiveFmu;

        private bool ValidateXmlTree;
        private bool ValidateChecksums;


        /* Temporary directory */
        private string TempContainerDir;

        /* eFMU directory in temporary directory */
        public string EfmuTempContainerDir { get; }

        /* Path to manifest file within temporary directory */
        private string ManifestFilePath;

        /* Relative URL to schema file (used for schema location) */
        private string SchemaFileRelUrl;


        /* name of model representation MR -> MR */
        private SortedDictionary<string, EfmuModelRepresentation> ModelRepresentations;


        public EfmuContainerManifest(string name,
                                     string tempContainerDir,
                                     bool validateXmlTree,
                                     bool validateChecksums)
        {
            this.Id = "{" + Guid.NewGuid().ToString() + "}";
            this.XsdVersion = EfmuContainerManifestProperties.ContainerManifestSchemaVersion;
            this.EfmiVersion = EfmuCommonManifestProperties.EfmiVersion;
            this.Name = name;
            DateTime dateTime = DateTime.Now;
            this.CreationDateAndTime = dateTime.ToString(EfmuCommonManifestProperties.GenerationDateAndTimeFormat);

            this.ValidateXmlTree = validateXmlTree;
            this.ValidateChecksums = validateChecksums;

            this.TempContainerDir = tempContainerDir;
            this.EfmuTempContainerDir = Path.Combine(TempContainerDir, EfmuContainerProperties.EfmuDirNameInContainer);

            /* Path to output file can already be determined here,
             * because we operate on a certain temporary container directory!
             */
            this.ManifestFilePath = Path.Combine(EfmuTempContainerDir,
                                                 EfmuContainerManifestProperties.ContainerManifestFileName);
            /* For paths which are written to XML, we always use "/" as path separator */
            this.SchemaFileRelUrl = EfmuContainerProperties.SchemaDirNameInContainer
                                                + "/" + EfmuContainerManifestProperties.ContainerManifestSchemaFile;

            this.ModelRepresentations = new SortedDictionary<string, EfmuModelRepresentation>();
        }

        /* Used by WriteToXmlFile to create XML tree */
        private bool BuildXmlTreeFromInternalData(ref XElement rootElem)
        {
            bool success = true;

            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteInfoLine($"> Creating XML tree from internal representation");

            /* Content */
            rootElem = new XElement("Content");

            rootElem.SetAttributeValue("id", Id);
            rootElem.SetAttributeValue("xsdVersion", XsdVersion);
            rootElem.SetAttributeValue("efmiVersion", EfmiVersion);
            rootElem.SetAttributeValue("name", Name);

            /* update creationDate set in constructor or read beforehand */
            DateTime dateTime = DateTime.Now;
            CreationDateAndTime = dateTime.ToString(EfmuCommonManifestProperties.GenerationDateAndTimeFormat);
            rootElem.SetAttributeValue(EfmuCommonManifestProperties.GenerationDateAndTime, CreationDateAndTime);

            /* optional attribute for unpacked FMU */
            if (HasActiveFmu())
            {
                rootElem.SetAttributeValue("activeFmu", ActiveFmu);
            }


            EfmuManifestValidation.SetXmlnsXsiAttribute(rootElem);
            EfmuManifestValidation.SetXsdNsAttribute(rootElem, SchemaFileRelUrl);

            /* Name */
            foreach (KeyValuePair<string, EfmuModelRepresentation> kvp in ModelRepresentations)
            {
                XElement modelReprElem = new XElement("ModelRepresentation");
                rootElem.Add(modelReprElem);

                EfmuModelRepresentation modelRepresentation = kvp.Value;
    
                /*
                 * name and manifest attribute must not contain paths.
                 */
                modelReprElem.SetAttributeValue("name", EfmuPathNames.GetFileOrDirName(modelRepresentation.Name));
                modelReprElem.SetAttributeValue("kind", modelRepresentation.Kind);

                success = EfmuPathNames.CheckThatPathIsRelative(modelRepresentation.Manifest);
                if (success)
                {
                    modelReprElem.SetAttributeValue("manifest",
                        EfmuPathNames.GetFileOrDirName(EfmuPathNames.ConvertRelativePathToUrlNotationWithPrefix(modelRepresentation.Manifest)));
                }
                else
                {
                    break;
                }

                /* Attention: Checksum MUST be computed when adding/replacing a model representation! */
                if (success)
                {
                    modelReprElem.SetAttributeValue("checksum", modelRepresentation.Checksum);
                }

                /* manifestRefId */
                if (success)
                {
                    modelReprElem.SetAttributeValue("manifestRefId", modelRepresentation.ManifestId);
                }
            }

            return success;
        }

        /* For a given model represesentation (i.e. "manifest" refers to "manifest of model representation"):
         * Validates (i) checksum and (ii) XML file.
         */
        private bool CheckAttributesOfModelRepresentation(int entryCounter,
                                                string name,
                                                string kindString,
                                                string manifest,
                                                ref string checksum,
                                                string manifestRefId,
                                                ref EfmuModelRepresentationKind modelRepresentationKind,
                                                ref string subtreePath,
                                                ref XDocument manifestDoc)
        {
            bool success = true;


            /* try to map kind string */
            if (success && !EfmuModelRepresentationKind.TryParse(kindString, out modelRepresentationKind))
            {
                EfmuConsoleWriter.WriteErrorLine($"The kind of the model representation #{entryCounter} is invalid: {kindString}");
                success = false;
            }

            /* check if subdirectory <name> exists */
            subtreePath = null;
            if (success)
            {
                subtreePath = BuildDirectoryPathForModelRepresentationName(name);
                if (!EfmuFilesystem.CheckForExistingDirectory(subtreePath, "model representation subdir"))
                {
                    success = false;
                }
            }

            /* check that manifest file exists */
            string manifestPath = null;
            if (success)
            {
                manifestPath = Path.Combine(subtreePath, manifest);
                success = EfmuFilesystem.CheckForExistingFile(manifestPath, "Manifest XML");
            }

            /* validate checksum */
            if (success)
            {
                EfmuConsoleWriter.WriteInfoLine(
                    $"> Validating checksum of manifest of model representation #{entryCounter}");
                string computedChecksum = EfmuChecksum.ComputeChecksumOfFile(manifestPath);
                if (!checksum.Equals(computedChecksum))
                {
                    if (ValidateChecksums)
                    {
                        EfmuConsoleWriter.WriteErrorLine(
                            $"The checksum of the manifest of the model representation #{entryCounter} is invalid");
                        EfmuConsoleWriter.WriteErrorLineNoPrefix($" subdirectory: {name}");
                        EfmuConsoleWriter.WriteErrorLineNoPrefix($" checksum from manifest: {checksum}");
                        EfmuConsoleWriter.WriteErrorLineNoPrefix($" checksum of file: {computedChecksum}");
                        success = false;
                    }
                    else
                    {
                        EfmuConsoleWriter.WriteWarningLine(
                            $"Ignored invalid checksum of the manifest of the model representation #{entryCounter}");
                        EfmuConsoleWriter.WriteWarningLineNoPrefix($" subdirectory: {name}");
                        EfmuConsoleWriter.WriteWarningLineNoPrefix($" checksum from manifest: {checksum}");
                        EfmuConsoleWriter.WriteWarningLineNoPrefix($" checksum of file: {computedChecksum}");

                        /* Fix checksum. It is used for building the ModelRepresentation object.
                         * Hence, if the container (manifest) is written back later,
                         * the checksum remains fixed :-)
                         */
                        checksum = computedChecksum;
                    }
                }
                else
                {
                    EfmuConsoleWriter.WriteDebugLine($" => checksum: {checksum}");
                }
            }

            /* validate subtree: validate manifest */
            if (success)
            {
                success = EfmuXmlTools.LoadToXDocument(manifestPath, ref manifestDoc);

                if (success && ValidateXmlTree)
                {
                    success = EfmuManifestValidation.PerformValidationOfManifestXmlDocument(manifestDoc,
                        null,
                        subtreePath,
                        "model representation manifest XML file");
                }
            }

            /* validate manifestRefId */
            if (success)
            {
                string manifestId = null;
                success = EfmuXmlTools.GetAttributeValue(manifestDoc.Root, "id", ref manifestId);
                if (success && !manifestId.Equals(manifestRefId))
                {
                    EfmuConsoleWriter.WriteErrorLine($"The manifest Ids of the model representation #{entryCounter} do not match:");
                    EfmuConsoleWriter.WriteErrorLineNoPrefix($" Id from container manifest: {manifestRefId}");
                    EfmuConsoleWriter.WriteErrorLineNoPrefix($" Id from model representation manifest: {manifestId}");
                    success = false;
                }
            }

            return success;
        }

        private bool CheckFmuFileEntry(EfmuFileListingEntry fmuFileEntry,
                                            string subtreePath,
                                            ref string fmuReference)
        {
            bool success = EfmuProdCodeManifestTools.CheckFmuReference(fmuFileEntry.UniqueName, "referenced FMU");

            string fmuPath = null;

            if (success)
            {
                fmuReference = fmuFileEntry.CombineNameAndPath();
                EfmuConsoleWriter.WriteDebugLine($"Found reference to FMU: {fmuReference}");

                fmuPath = Path.Combine(subtreePath, fmuReference);
                if (success && !EfmuFilesystem.CheckForExistingFileOrDirectory(fmuPath,
                                                                                "referenced FMU"))
                {
                    success = false;
                }
            }

            bool isFileInsteadOfFolder = false;

            if (success)
            {
                success = EfmuProdCodeManifestTools.CategorizeFmuReference(fmuFileEntry.UniqueName, ref isFileInsteadOfFolder);
            }

            if (success && isFileInsteadOfFolder)
            {
                ZipArchive archive = null;
                try
                {
                    archive = ZipFile.OpenRead(fmuPath);
                }
                catch (Exception e)
                {
                    EfmuConsoleWriter.DumpException(e, "Caught exception");
                    EfmuConsoleWriter.WriteErrorLineNoPrefix(
                        $"=> The referenced FMU file is not a valid ZIP archive: {fmuReference}");
                    success = false;
                }
                finally
                {
                    if (null != archive)
                    {
                        archive.Dispose();
                    }
                }
            }

            return success;
        }

        private bool CheckFmuContainerFileReference(string subtreePath,
                                                    EfmuManifestFileListing fileListing,
                                                    ref string fmuReference)
        {
            bool success = true;

            fmuReference = null;

            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteDebugLine("> Checking for FMU reference");

            EfmuFileListingEntry fmuFileEntry = null;
            if (success)
            {
                EfmuConsoleWriter.WriteDebugLine("before TryGetFmuEntry");
                bool dummyResult = fileListing.TryGetFmuEntry(out fmuFileEntry);
            }

            if (success && null != fmuFileEntry)
            {
                success = CheckFmuFileEntry(fmuFileEntry,
                                            subtreePath,
                                            ref fmuReference);
            }

            return success;
        }

        /* Assumes that parameter is != null. */
        private bool CheckActiveFmu(string activeFmu)
        {
            bool success = true;

            if (!HasModelRepresentation(activeFmu))
            {
                EfmuConsoleWriter.WriteErrorLine($"Active model representation '{activeFmu}' does not exist");
                success = false;
            }

            if (success)
            {
                EfmuModelRepresentationKind kind = EfmuModelRepresentationKind.ProductionCode;
                // result is true by construction
                GetModelRepresentationKind(activeFmu,
                                           ref kind);
                if (EfmuModelRepresentationKind.ProductionCode != kind)
                {
                    EfmuConsoleWriter.WriteErrorLine($"The active model representation does not have kind '{EfmuModelRepresentationKind.ProductionCode}' but '{kind}'");
                    success = false;
                }
            }

            return success;
        }

        private bool DetermineModelRepresentationFromXmlTree(XElement modelReprElem,
                                                             int entryCounter)
        {
            bool success = true;

            string name = null;
            string kindString = null;
            string manifest = null;
            string checksum = null;
            string manifestRefId = null;

            EfmuModelRepresentationKind modelRepresentationKind = EfmuModelRepresentationKind.ProductionCode;

            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(modelReprElem, "name", ref name);
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(modelReprElem, "kind", ref kindString);
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(modelReprElem, "manifest", ref manifest);
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(modelReprElem, "checksum", ref checksum);
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(modelReprElem, "manifestRefId", ref manifestRefId);
            }

            if (success)
            {
                EfmuConsoleWriter.WriteDebugLine($"Name:");
                EfmuConsoleWriter.WriteDebugLine($" name: {name}");
                EfmuConsoleWriter.WriteDebugLine($" kindString: {kindString}");
                EfmuConsoleWriter.WriteDebugLine($" manifest: {manifest}");
                EfmuConsoleWriter.WriteDebugLine($" checksum: {checksum}");
                EfmuConsoleWriter.WriteDebugLine($" manifestRefId: {manifestRefId}");
            }

            if (success)
            {
                success = EfmuPathNames.CheckThatPathIsFileName(manifest);
            }

            /* check if model representation with <name> already exists */
            XDocument manifestDoc = null;
            if (success && HasModelRepresentation(name))
            {
                EfmuConsoleWriter.WriteErrorLine($"A model representation with name '{name}' already exists");
                success = false;
            }

            string subtreePath = null;
            if (success)
            {
                success = CheckAttributesOfModelRepresentation(entryCounter,
                                                                        name,
                                                                        kindString,
                                                                        manifest,
                                                                        ref checksum,
                                                                        manifestRefId,
                                                                        ref modelRepresentationKind,
                                                                        ref subtreePath,
                                                                        ref manifestDoc);
            }

            EfmuManifestFileListing fileListing = null;
            if (success)
            {
                success = EfmuFileListingFromXml.DetermineManifestFileListing(manifestDoc,
                                                                                subtreePath,
                                                                                /* TODO: Fix checksums on-the-fly */
                                                                                ValidateChecksums,
                                                                                ref fileListing);
            }

            string fmuReference = null;
            if (success && EfmuModelRepresentationKind.ProductionCode == modelRepresentationKind)
            {
                success = CheckFmuContainerFileReference(subtreePath,
                                                            fileListing,
                                                            ref fmuReference);
            }

            if (success)
            {
                /* At this point, we can assume the consistency of the model representation. */
                EfmuModelRepresentation modelRepresentation = new EfmuModelRepresentation(name,
                                                                                            modelRepresentationKind,
                                                                                            manifest,
                                                                                            checksum,
                                                                                            manifestRefId,
                                                                                            fmuReference);
                success = AddModelRepresentation(modelRepresentation);
            }

            return success;
        }

        private bool DetermineInternalDataFromXmlTree(XElement rootElem)
        {
            bool success = true;

            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteInfoLine("> Extracting data from container manifest");

            /* reset internal data */
            this.Id = null;
            this.XsdVersion = null;
            this.EfmiVersion = null;
            this.Name = null;
            this.CreationDateAndTime = null;

            this.ModelRepresentations.Clear();

            /* Content */
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(rootElem, "id", ref this.Id);
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(rootElem, "xsdVersion", ref this.XsdVersion);
                if (success
                    && EfmuContainerManifestProperties.ContainerManifestSchemaVersion != this.XsdVersion)
                {
                    EfmuConsoleWriter.WriteErrorLine($"Supported schema version {EfmuContainerManifestProperties.ContainerManifestSchemaVersion} is unequal to schema version {this.XsdVersion} of container manifest");
                    success = false;
                }
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(rootElem, "efmiVersion", ref this.EfmiVersion);
                if (success
                    && EfmuCommonManifestProperties.EfmiVersion != this.EfmiVersion)
                {
                    EfmuConsoleWriter.WriteErrorLine($"Supported efmi version {EfmuCommonManifestProperties.EfmiVersion} is unequal to efmi version {this.EfmiVersion} of container manifest");
                    success = false;
                }
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(rootElem, "name", ref this.Name);
            }
            if (success)
            {
                success = EfmuXmlTools.GetAttributeValue(rootElem, EfmuCommonManifestProperties.GenerationDateAndTime, ref this.CreationDateAndTime);
            }

            /* activeFmu */
            string activeFmu = null;
            if (success)
            {
                bool dummyHasAttribute = false;
                EfmuXmlTools.GetOptionalAttributeValue(rootElem,
                                                       "activeFmu",
                                                       ref activeFmu,
                                                       out dummyHasAttribute);
                /* result is checked later when model representations are known */
            }

            if (success)
            {
                EfmuConsoleWriter.WriteDebugLine($" id: {this.Id}");
                EfmuConsoleWriter.WriteDebugLine($" xsdVersion: {this.XsdVersion}");
                EfmuConsoleWriter.WriteDebugLine($" efmiVersion: {this.EfmiVersion}");
                EfmuConsoleWriter.WriteDebugLine($" name: {this.Name}");
                EfmuConsoleWriter.WriteDebugLine($" creationDate: {this.CreationDateAndTime}");
                if (null != activeFmu)
                {
                    EfmuConsoleWriter.WriteDebugLine(
                        $" activeFmu (not verified yet): {activeFmu}");
                }
            }

            if (success)
            {
                /* Model Representations */
                IEnumerable<XElement> modelRepresentationElems = rootElem.Elements();
                EfmuConsoleWriter.WriteInfoLine($"- Found {modelRepresentationElems.Count()} model representation(s)");
                int entryCounter = 0;
                foreach (XElement modelReprElem in modelRepresentationElems)
                {
                    success = DetermineModelRepresentationFromXmlTree(modelReprElem, entryCounter);
                    if (success)
                    {
                        ++entryCounter;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (success && null != activeFmu)
            {
                success = CheckActiveFmu(activeFmu);
                if (success)
                {
                    SetActiveFmu(activeFmu);
                    EfmuConsoleWriter.WriteInfoLine($"Container has active FMU from model representation {ActiveFmu}");
                }
            }

            return success;
        }

        public bool ReadFromXmlFile()
        {
            bool success = EfmuFilesystem.CheckForExistingFile(ManifestFilePath, "Container Manifest");

            XDocument doc = null;

            if (success)
            {
                success = EfmuXmlTools.LoadToXDocument(ManifestFilePath, ref doc);
                if (success && ValidateXmlTree)
                {
                    success = EfmuManifestValidation.PerformValidationOfManifestXmlDocument(doc,
                        null,
                        EfmuTempContainerDir,
                        "read container manifest XML file");
                }
            }

            if (success)
            {
                success = DetermineInternalDataFromXmlTree(doc.Root);
            }

            return success;
        }

        public bool WriteToXmlFile()
        {
            bool success = true;

            XElement rootElem = null;

            if (success)
            {
                success = BuildXmlTreeFromInternalData(ref rootElem);
            }
            if (success && ValidateXmlTree)
            {
                XDocument doc = new XDocument(rootElem);
                success = EfmuManifestValidation.PerformValidationOfManifestXmlDocument(doc,
                                                                                null,
                                                                                EfmuTempContainerDir,
                                                                                "write container manifest XML file");
            }
            if (success)
            {
                EfmuConsoleWriter.WriteDebugLine("");
                EfmuConsoleWriter.WriteInfoLine("> Writing XML tree to file");
                rootElem.Save(ManifestFilePath);
            }

            return success;
        }

        /* The path is only build, i.e. it is not checked whether the model representation exists. */
        public string BuildDirectoryPathForModelRepresentationName(string name)
        {
            return Path.Combine(EfmuTempContainerDir, name);
        }

        public bool HasModelRepresentation(string name)
        {
            return ModelRepresentations.ContainsKey(name);
        }

        public bool GetModelRepresentation(string name,
                                           ref EfmuModelRepresentation modelRepresentation)
        {
            if (HasModelRepresentation(name))
            {
                modelRepresentation = ModelRepresentations[name];
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetModelRepresentationKind(string name,
                                               ref EfmuModelRepresentationKind kind)
        {
            EfmuModelRepresentation modelRepresentation = null;
            if (GetModelRepresentation(name, ref modelRepresentation))
            {
                kind = modelRepresentation.Kind;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveModelRepresentation(string name)
        {
            if (HasModelRepresentation(name))
            {
                ModelRepresentations.Remove(name);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddModelRepresentation(EfmuModelRepresentation modelRepresentation)
        {
            string name = modelRepresentation.Name;
            if (HasModelRepresentation(name))
            {
                return false;
            }
            else
            {
                ModelRepresentations[name] = modelRepresentation;
                return true;
            }
        }

        public void SetActiveFmu(string activeFmu)
        {
            ActiveFmu = activeFmu;
        }

        public void ResetActiveFmu()
        {
            ActiveFmu = null;
        }

        public bool HasActiveFmu()
        {
            return null != ActiveFmu;
        }

        public string GetActiveFmu()
        {
            return ActiveFmu;
        }

        public bool DumpContainerManifest()
        {
            bool success = true;

            EfmuConsoleWriter.WriteInfoLine($" id: {this.Id}");
            EfmuConsoleWriter.WriteInfoLine($" xsdVersion: {this.XsdVersion}");
            EfmuConsoleWriter.WriteInfoLine($" efmiVersion: {this.EfmiVersion}");
            EfmuConsoleWriter.WriteInfoLine($" name: {this.Name}");
            EfmuConsoleWriter.WriteInfoLine($" creationDate: {this.CreationDateAndTime}");
            if (HasActiveFmu())
            {
                EfmuConsoleWriter.WriteInfoLine($" activeFmu: {this.ActiveFmu}");
            }

            int index = 0;
            foreach (KeyValuePair<string, EfmuModelRepresentation> kvp in ModelRepresentations)
            {
                EfmuModelRepresentation modelRepresentation = kvp.Value;
    
                EfmuConsoleWriter.WriteInfoLine($"Model representation #{index}");
                EfmuConsoleWriter.WriteInfoLine($" name: {modelRepresentation.Name}");
                EfmuConsoleWriter.WriteInfoLine($" kind: {modelRepresentation.Kind}");
                EfmuConsoleWriter.WriteInfoLine($" manifest: {modelRepresentation.Manifest}");
                EfmuConsoleWriter.WriteInfoLine($" checksum: {modelRepresentation.Checksum}");
                EfmuConsoleWriter.WriteInfoLine($" manifestRefId: {modelRepresentation.ManifestId}");
                if (null != modelRepresentation.OptionalFmuReference)
                {
                    bool isFileInsteadOfFolder = false;

                    if (success)
                    {
                        success = EfmuProdCodeManifestTools.CategorizeFmuReference(modelRepresentation.OptionalFmuReference, ref isFileInsteadOfFolder);
                    }
                    if (success)
                    {
                        if (isFileInsteadOfFolder)
                        {
                            EfmuConsoleWriter.WriteInfoLine($" FMU: {modelRepresentation.OptionalFmuReference}");
                        }
                        else
                        {
                            EfmuConsoleWriter.WriteInfoLine($" FMUFolder: {modelRepresentation.OptionalFmuReference}");
                        }
                    }
                }

                if (success)
                {
                    ++index;
                }
                else
                {
                    break;
                }
            }

            return success;
        }
    }
}
