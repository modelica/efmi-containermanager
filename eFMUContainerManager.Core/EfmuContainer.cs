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
using System.IO;
using System.IO.Compression;
using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.ManifestsAndContainers.ManifestTools;
using eFMI.Misc;

namespace eFMI.ContainerManager
{
    /* This class manages an EFMU container.
     * It is created for a certain temporary directory which is used
     * to create or read/evaluate or read/manipulate/write a container.
     * The constructor only initializes some properties which depend on the
     * location of the temporary directory.
     * The resulting instance of the class can be used as follows:
     * (1) read manifest XML file (file path already determined from temporary directory)
     * (2) write manifest XML file (dto.)
     * (3) evaluate or manipulate the model representations
     */
    class EfmuContainer
    {
        private EfmuCoreCallArguments CallArguments;

        private string Name;

        /* Directory which reflects the content of the container.
         * It is zipped to a container file, finally.
         *
         * This information is regarded as valid if != null.
         *
         * TODO:
         * The content may be prepared in a temporary directory within %TEMP%.
         */
        private string TempContainerDir;

        /* Corresponds to OutputPath or ContainerFilePath of CallArguments,
         * depending on ContainerOperation.
         */
        private string ContainerFilePath;


        private EfmuContainerManifest ContainerManifest;

        /* The following instances of worker classes are regarded as valid if != null */
        private EfmuSchemaCopier SchemaCopier;
        private EfmuModelRepresentationCopier ModelRepresentationCopier;

        private bool HasBeenBootedSuccessfully;


        public EfmuContainer(EfmuCoreCallArguments callArguments)
        {
            this.CallArguments = callArguments;

            this.HasBeenBootedSuccessfully = false;
        }

        private bool BootForAddOrReplaceOperation(bool addInsteadOfReplace)
        {
            bool success = true;

            if (addInsteadOfReplace)
            {
                if (ContainerManifest.HasModelRepresentation(CallArguments.Name))
                {
                    EfmuConsoleWriter.WriteErrorLine($"A model representation with name '{CallArguments.Name}' already exists");
                    success = false;
                }
            }
            else
            {
                if (!ContainerManifest.HasModelRepresentation(CallArguments.Name))
                {
                    EfmuConsoleWriter.WriteErrorLine($"A model representation with name '{CallArguments.Name}' does not exist");
                    success = false;
                }
            }

            if (success)
            {
                string outputDir = Path.Combine(TempContainerDir,
                                                EfmuContainerProperties.EfmuDirNameInContainer,
                                                CallArguments.Name);
                ModelRepresentationCopier = new EfmuModelRepresentationCopier(CallArguments.InputDir,
                                                                                outputDir,
                                                                                ContainerManifest,
                                                                                addInsteadOfReplace,
                                                                                CallArguments.Name,
                                                                                CallArguments.ManifestFileName,
                                                                                CallArguments.ValidateXmlTree,
                                                                                CallArguments.ValidateChecksums);
                success = EfmuContainerTools.PerformCallWithExceptionHandling(ModelRepresentationCopier.Boot);
            }

            return success;
        }

        private bool CheckFmuFile(string fmuPath)
        {
            bool success = true;

            ZipArchive archive = null;
            try
            {
                archive = ZipFile.OpenRead(fmuPath);
                string efmuDirName = EfmuContainerProperties.EfmuDirNameInContainer + "/";
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.StartsWith(efmuDirName))
                    {
                        EfmuConsoleWriter.WriteErrorLine($"FMU contains forbidden {EfmuContainerProperties.EfmuDirNameInContainer} directory");
                        EfmuConsoleWriter.WriteErrorLineNoPrefix("Unpacking the FMU would overwrite the EFMU parts of the container and is therefore skipped");
                        success = false;
                        break;
                    }
                }
                /* TODO:
                 * List entries of FMU to see if there are only files and not directories.
                 * If so, we have to check all entries whether they start with "eFMU/"
                 */
            }
            catch (Exception e)
            {
                EfmuConsoleWriter.DumpException(e, "Caught exception");
                EfmuConsoleWriter.WriteErrorLineNoPrefix(
                    $"=> The referenced FMU file is not a valid ZIP archive: {fmuPath}");
            }
            finally
            {
                if (null != archive)
                {
                    archive.Dispose();
                }
            }

            return success;
        }

        private bool CheckFmuFolder(string fmuPath)
        {
            bool success = true;

            DirectoryInfo dir = new DirectoryInfo(fmuPath);
            FileSystemInfo[] fileSystemInfos = dir.GetFileSystemInfos(EfmuContainerProperties.EfmuDirNameInContainer);
            if (null != fileSystemInfos && 0 != fileSystemInfos.Length)
            {
                EfmuConsoleWriter.WriteErrorLine($"FMU contains forbidden {EfmuContainerProperties.EfmuDirNameInContainer} directory");
                EfmuConsoleWriter.WriteErrorLineNoPrefix("Unpacking the FMU would overwrite the EFMU parts of the container and is therefore skipped");
                success = false;
            }

            return success;
        }

        private bool BootForUnpackFmuOperation()
        {
            bool success = true;

            if (!ContainerManifest.HasModelRepresentation(CallArguments.Name))
            {
                EfmuConsoleWriter.WriteErrorLine($"A model representation with name '{CallArguments.Name}' does not exist");
                success = false;
            }

            EfmuModelRepresentation modelRepresentation = null;
            if (success)
            {
                // result is true by construction
                ContainerManifest.GetModelRepresentation(CallArguments.Name, ref modelRepresentation);
                if (!modelRepresentation.HasFmuReference())
                {
                    EfmuConsoleWriter.WriteErrorLine($"The selected model representation does not have a FMU");
                    success = false;
                }
            }

            bool isFileInsteadOfFolder = false;

            if (success)
            {
                success = EfmuProdCodeManifestTools.CategorizeFmuReference(modelRepresentation.OptionalFmuReference,
                    ref isFileInsteadOfFolder);
            }

            if (success)
            {
                string subtreePath =
                    ContainerManifest.BuildDirectoryPathForModelRepresentationName(CallArguments.Name);
                string fmuPath = Path.Combine(subtreePath, modelRepresentation.OptionalFmuReference);

                if (isFileInsteadOfFolder)
                {
                    success = CheckFmuFile(fmuPath);
                }
                else
                {
                    success = CheckFmuFolder(fmuPath);
                }
            }

            return success;
        }

        public bool Boot()
        {
            bool success = !HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>>> EfmuContainer:Boot");

            if (success)
            {
                if (EfmuContainerOperations.CreateContainer == CallArguments.ContainerOperation)
                {
                    /* Check if output file already exists. */
                    if (EfmuFilesystem.DoesFileExist(CallArguments.OutputPath) && !CallArguments.ForceOverwriting)
                    {
                        /* An existing EFMU file is removed just before creating a new one */
                        EfmuConsoleWriter.WriteErrorLine($"Output file '{CallArguments.OutputPath}' already exists.");
                        success = false;
                    }
                }
                else if (EfmuContainerTools.IsExtractFromContainerOperation(CallArguments.ContainerOperation))
                {
                    /* Check if output dirctory already exists. */
                    if (EfmuFilesystem.DoesDirectoryExist(CallArguments.OutputPath) && !CallArguments.ForceOverwriting)
                    {
                        /* An existing directory is removed just before creating a new one */
                        EfmuConsoleWriter.WriteErrorLine($"Output directory '{CallArguments.OutputPath}' already exists.");
                        success = false;
                    }
                }
            }

            if (success)
            {
                if (EfmuContainerTools.DoesContainerOperationRequireInitialRead(CallArguments.ContainerOperation))
                {
                    ContainerFilePath = CallArguments.ContainerFilePath;
                    if (!EfmuFilesystem.CheckForExistingFile(ContainerFilePath, "EFMU"))
                    {
                        success = false;
                    }
                }
                else if (EfmuContainerTools.DoesContainerOperationImplyFinalWrite(CallArguments.ContainerOperation))
                {
                    ContainerFilePath = CallArguments.OutputPath;
                }
                else
                {
                    EfmuConsoleWriter.WriteErrorLine("Reached inconsistent state: Selected container operation implies neither initial read nor final write of container");
                    success = false;
                }
            }

            if (success)
            {
                /* Determine and create temporary directory for the container. */
                TempContainerDir = EfmuFilesystem.GetTempDirectoryName();

                EfmuConsoleWriter.WriteInfoLine($"Container will be managed in temporary directory '{TempContainerDir}'");
            }

            if (success)
            {
                /* Create instance of ContainerManifest */
                this.Name = CallArguments.Name; /* != null if needed */

                this.ContainerManifest = new EfmuContainerManifest(Name,
                    TempContainerDir,
                    CallArguments.ValidateXmlTree,
                    CallArguments.ValidateChecksums);
            }

            if (success)
            {
                if (EfmuContainerOperations.CreateContainer == CallArguments.ContainerOperation)
                {
                    /* Create eFMU directory */
                    string efmuDirPath = Path.Combine(TempContainerDir, EfmuContainerProperties.EfmuDirNameInContainer);
                    success = EfmuContainerTools.PerformCallWithExceptionHandling(EfmuFilesystem.CreateDirectory, efmuDirPath);
                }
                else if (EfmuContainerTools.DoesContainerOperationRequireInitialRead(CallArguments.ContainerOperation))
                {
                    /* includes reading ContainerManifest from file */
                    success = ReadContainerFile();
                }
            }

            /* INVARIANT: If success=true at this point, we have ...
             * - either created an empty container
             * - or a read (non-)empty container which has been validated completely
             * => The ModelRepresentations are known => they can be queried
             */

            if (success)
            {
                switch (CallArguments.ContainerOperation)
                {
                    case EfmuContainerOperations.UNKNOWN:
                    {
                        success = false;
                    }
                        break;
                    case EfmuContainerOperations.CreateContainer:
                    {
                        string outputDir = Path.Combine(TempContainerDir, EfmuContainerManifestProperties.EfmuAndSchemaDirNamesInContainer);
                        SchemaCopier = new EfmuSchemaCopier(CallArguments.InputDir, outputDir);
                        success = EfmuContainerTools.PerformCallWithExceptionHandling(SchemaCopier.Boot);
                    }
                        break;
                    case EfmuContainerOperations.AddToContainer:
                    case EfmuContainerOperations.ReplaceInContainer:
                    {
                        success = BootForAddOrReplaceOperation(EfmuContainerOperations.AddToContainer == CallArguments.ContainerOperation);
                    }
                        break;
                    case EfmuContainerOperations.DeleteFromContainer:
                    case EfmuContainerOperations.ExtractFromContainer:
                    {
                        if (!ContainerManifest.HasModelRepresentation(CallArguments.Name))
                        {
                            EfmuConsoleWriter.WriteErrorLine($"A model representation with name '{CallArguments.Name}' does not exist");
                            success = false;
                        }
                    }
                        break;
                    case EfmuContainerOperations.UnpackFmu:
                    {
                        success = BootForUnpackFmuOperation();
                    }
                        break;
                    case EfmuContainerOperations.TidyRoot:
                    case EfmuContainerOperations.ListContainerContent:
                    {
                        /* nothing to do */
                    }
                        break;
                }
            }

            HasBeenBootedSuccessfully = success;
            return success;
        }

        public bool Run()
        {
            bool success = HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>>> EfmuContainer::Run");

            if (success)
            {
                switch (CallArguments.ContainerOperation)
                {
                    case EfmuContainerOperations.UNKNOWN:
                    {
                        success = false;
                    }
                        break;
                    case EfmuContainerOperations.CreateContainer:
                    {
                        success = EfmuContainerTools.PerformCallWithExceptionHandling(SchemaCopier.Run);
                    }
                        break;
                    case EfmuContainerOperations.AddToContainer:
                    case EfmuContainerOperations.ReplaceInContainer:
                    {
                        success = AddToOrReplaceInContainer(EfmuContainerOperations.AddToContainer ==
                                                            CallArguments.ContainerOperation);
                    }
                        break;
                    case EfmuContainerOperations.DeleteFromContainer:
                    {
                        success = EfmuContainerTools.PerformCallWithExceptionHandling(DeleteFromContainer);
                    }
                        break;
                    case EfmuContainerOperations.ExtractFromContainer:
                    {
                        success = EfmuContainerTools.PerformCallWithExceptionHandling(ExtractFromContainer);
                    }
                        break;
                    case EfmuContainerOperations.ExtractSchemasFromContainer:
                    {
                        success = EfmuContainerTools.PerformCallWithExceptionHandling(ExtractSchemasFromContainer);
                    }
                        break;
                    case EfmuContainerOperations.UnpackFmu:
                    {
                        success = EfmuContainerTools.PerformCallWithExceptionHandling(UnpackFmu);
                    }
                        break;
                    case EfmuContainerOperations.TidyRoot:
                    {
                        success = EfmuContainerTools.PerformCallWithExceptionHandling(ClearContainerRoot);
                    }
                        break;
                    case EfmuContainerOperations.ListContainerContent:
                    {
                        success = ListContainerContent();
                    }
                        break;
                }
            }


            if (success && EfmuContainerTools.DoesContainerOperationImplyFinalWrite(CallArguments.ContainerOperation))
            {
                /* list final content */
                success = ListContainerContent();

                /* includes writing ContainerManifest to file */
                success = WriteContainerFile();
            }

            return success;
        }

        public void Shutdown()
        {
            if (EfmuFilesystem.DoesDirectoryExist(TempContainerDir))
            {
                /* remove temporary directory */
                bool dummyResult = EfmuContainerTools.PerformCallWithExceptionHandling(EfmuFilesystem.RemoveDirectory, TempContainerDir);
            }
        }

        private bool AddToOrReplaceInContainer(bool addInsteadOfReplace)
        {
            bool success = true;

            success = EfmuContainerTools.PerformCallWithExceptionHandling(ModelRepresentationCopier.Run);

            return success;
        }

        private bool DeleteFromContainer()
        {
            bool success = true;

            EfmuConsoleWriter.WriteInfoLine($">> Removing model representation '{CallArguments.Name}'");
            success = ContainerManifest.RemoveModelRepresentation(CallArguments.Name);
            if (success)
            {
                string subtreePath =
                    ContainerManifest.BuildDirectoryPathForModelRepresentationName(CallArguments.Name);
                bool dummyResult = EfmuContainerTools.PerformCallWithExceptionHandling(EfmuFilesystem.RemoveDirectory, subtreePath);
                EfmuConsoleWriter.WriteInfoLine("=> Model representation has been removed successfully");
            }
            else
            {
                EfmuConsoleWriter.WriteErrorLine("=> Model representation could not be removed from Container Manifest");
            }

            if (success && ContainerManifest.HasActiveFmu())
            {
                string activeFmu = ContainerManifest.GetActiveFmu();
                if (activeFmu.Equals(CallArguments.Name))
                {
                    success = ClearContainerRoot();
                }
            }

            return success;
        }

        private bool ExtractFromContainer()
        {
            bool success = true;

            EfmuConsoleWriter.WriteInfoLine($">> Extracting model representation '{CallArguments.Name}'");

            string sourcePath =
                ContainerManifest.BuildDirectoryPathForModelRepresentationName(CallArguments.Name);
            string destPath = CallArguments.OutputPath;

            if (EfmuFilesystem.DoesDirectoryExist(destPath))
            {
                bool dummyResult = EfmuContainerTools.PerformCallWithExceptionHandling(EfmuFilesystem.RemoveDirectory, destPath);
            }
            EfmuFilesystem.CopyDirectory(sourcePath, destPath);
            EfmuConsoleWriter.WriteInfoLine("=> Model representation has been extracted successfully");

            return success;
        }

        private bool ExtractSchemasFromContainer()
        {
            bool success = true;

            EfmuConsoleWriter.WriteInfoLine($">> Extracting schemas");

            string sourcePath = Path.Combine(TempContainerDir, EfmuContainerManifestProperties.EfmuAndSchemaDirNamesInContainer);
            string destPath = CallArguments.OutputPath;

            if (EfmuFilesystem.DoesDirectoryExist(destPath))
            {
                bool dummyResult = EfmuContainerTools.PerformCallWithExceptionHandling(EfmuFilesystem.RemoveDirectory, destPath);
            }
            EfmuFilesystem.CopyDirectory(sourcePath, destPath);
            EfmuConsoleWriter.WriteInfoLine("=> Schemas have been extracted successfully");

            return success;
        }

        /* Used by ClearContainerRoot. */
        private bool RemoveAllNonEfmuEntitiesFromContainerRoot()
        {
            bool success = true;

            DirectoryInfo dirInfo = new DirectoryInfo(TempContainerDir);

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete(); 
            }
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                /* NOTE: Checking "!dir.Name.Equals..." did not work, because of missing ToString() */
                string dirName = dir.Name;
                if (!dirName.Equals(EfmuContainerProperties.EfmuDirNameInContainer))
                {
                    dir.Delete(true);
                }
            }

            return success;
        }

        private bool ClearContainerRoot()
        {
            EfmuConsoleWriter.WriteInfoLine($">> Removing non-EFMU entities from container root");
            ContainerManifest.ResetActiveFmu();
            return EfmuContainerTools.PerformCallWithExceptionHandling(RemoveAllNonEfmuEntitiesFromContainerRoot);
        }

        /* Used by UnpackFmu. */
        private bool UnpackFmuOfModelRepresentation()
        {
            bool success = true;

            EfmuModelRepresentation modelRepresentation = null;
            // result always true by construction
            ContainerManifest.GetModelRepresentation(CallArguments.Name, ref modelRepresentation);
            string subtreePath =
                ContainerManifest.BuildDirectoryPathForModelRepresentationName(CallArguments.Name);
            string fmuPath = Path.Combine(subtreePath, modelRepresentation.OptionalFmuReference);


            bool isFileInsteadOfFolder = false;

            if (success)
            {
                success = EfmuProdCodeManifestTools.CategorizeFmuReference(modelRepresentation.OptionalFmuReference, ref isFileInsteadOfFolder);
            }

            if (success)
            {
                if (isFileInsteadOfFolder)
                {
                    ZipFile.ExtractToDirectory(fmuPath, TempContainerDir);
                }
                else
                {
                    EfmuFilesystem.CopyDirectory(fmuPath, TempContainerDir, true);
                }
            }

            return success;
        }

        private bool UnpackFmu()
        {
            bool success = true;

            success = ClearContainerRoot();
            if (success)
            {
                EfmuConsoleWriter.WriteInfoLine($">> Unpacking FMU of model representation '{CallArguments.Name}'");
                success = EfmuContainerTools.PerformCallWithExceptionHandling(UnpackFmuOfModelRepresentation);
                if (success)
                {
                    ContainerManifest.SetActiveFmu(CallArguments.Name);
                }
            }

            return success;
        }

        private bool ListContainerContent()
        {
            bool success = true;

            EfmuConsoleWriter.WriteInfoLine("");
            EfmuConsoleWriter.WriteInfoLine("=== Dump of content of EFMU container ===");

            success = ContainerManifest.DumpContainerManifest();

            EfmuConsoleWriter.WriteInfoLine("=== END of dump ===");
            EfmuConsoleWriter.WriteInfoLine("");

            return success;
        }

        private bool ReadManifestFromXmlFile()
        {
            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteInfoLine(">> Reading container manifest");

            bool success = ContainerManifest.ReadFromXmlFile();
            if (success)
            {
                EfmuConsoleWriter.WriteInfoLine("=> Container manifest has been read successfully");
            }
            return success;
        }

        private bool WriteManifestToXmlFile()
        {
            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteInfoLine(">> Writing container manifest");

            bool success = ContainerManifest.WriteToXmlFile();
            if (success)
            {
                EfmuConsoleWriter.WriteInfoLine("=> Container manifest has been written successfully");
            }
            return success;
        }

        private bool ReadContainerFile()
        {
            bool success = true;

            /* Unzip container to temporary directory */
            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteInfoLine($">> Reading zipped EFMU container: {CallArguments.ContainerFilePath}");
            success = EfmuContainerTools.PerformCallWithExceptionHandling(PerformUnzippingOfContainerFile);

            if (success)
            {
                string efmuDirPath = Path.Combine(TempContainerDir, EfmuContainerProperties.EfmuDirNameInContainer);
                if (!EfmuFilesystem.DoesDirectoryExist(efmuDirPath))
                {
                    EfmuConsoleWriter.WriteErrorLine($"Container has no {EfmuContainerProperties.EfmuDirNameInContainer} directory, i.e. is no EFMU container");
                    success = false;
                }
            }
            if (success)
            {
                success = ReadManifestFromXmlFile();
            }

            return success;
        }

        private bool WriteContainerFile()
        {
            bool success = WriteManifestToXmlFile();

            if (success)
            {
                /* Zip temporary directory to container */
                EfmuConsoleWriter.WriteDebugLine("");
                EfmuConsoleWriter.WriteInfoLine($">> Writing zipped EFMU container: {ContainerFilePath}");
                success = EfmuContainerTools.PerformCallWithExceptionHandling(PerformZippingOfTempContainerDir);

                if (success)
                {
                    EfmuConsoleWriter.WriteInfoLine($"=> Successfully written EFMU container: {ContainerFilePath}");
                }
            }

            return success;
        }

        private bool PerformUnzippingOfContainerFile()
        {
            bool success = true;

            /* It is assumed that target directory already exists */
            ZipFile.ExtractToDirectory(CallArguments.ContainerFilePath, TempContainerDir);

            return success;
        }

        private bool PerformZippingOfTempContainerDir()
        {
            bool success = EfmuFilesystem.CreateDirectoryOfFileIfNotExisting(ContainerFilePath);

            if (success)
            {
                /* Remove EFMU file to avoid that zipping raises exception
                 * because it already exists (create mode).
                 * Removing works even if file does not exist, already.
                 * If update mode is chosen, an existing archive would be updated,
                 * but not overwritten.
                 */
                File.Delete(ContainerFilePath);

                /* Zip contents of directory to file. */
                ZipFile.CreateFromDirectory(TempContainerDir, ContainerFilePath);
            }

            return success;
        }
    }
}
