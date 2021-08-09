/*
 * Copyright (c) 2021, dSPACE GmbH, Modelica Association and contributors
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

using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.Misc;

namespace eFMI.ContainerManager
{
    class EfmuSchemaCopier : EfmuAbstractCopier
    {
        public EfmuSchemaCopier(string inputDir,
                                string outputDir) :
                                    base(inputDir, outputDir)
        {
        }

        public override bool Boot()
        {
            bool success = !HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>> EfmuSchemaCopier:Boot");

            if (success)
            {
                success = PerformCommonBootChecks();
            }

            /* This file is referenced by behavioral model manifest file. */
            if (success && !EfmuFilesystem.CheckForExistingFile(InputDir,
                                                                EfmuBehavModelManifestProperties.BehavModelManifestSchemaRelPath,
                                                                "Schema"))
            {
                success = false;
            }


            /* This file is referenced by equation code manifest file. */
            /*
             
              Temporarily disabled until Equation Code will be part of specification again.
            
            
            if (success && !EfmuFilesystem.CheckForExistingFile(InputDir,
                                                                EfmuEquCodeManifestProperties.EquCodeManifestSchemaRelPath,
                                                                "Schema"))
            {
                success = false;
            }
            */

            /* This file is referenced by algorithm code manifest file. */
            if (success && !EfmuFilesystem.CheckForExistingFile(InputDir,
                                                                EfmuAlgoCodeManifestProperties.AlgoCodeManifestSchemaRelPath,
                                                                "Schema"))
            {
                success = false;
            }

            /* This file is referenced by production code manifest file. */
            if (success && !EfmuFilesystem.CheckForExistingFile(InputDir,
                                                                EfmuProdCodeManifestProperties.ProdCodeManifestSchemaRelPath,
                                                                "Schema"))
            {
                success = false;
            }

            /* This file is referenced by binary code manifest file. */
            if (success && !EfmuFilesystem.CheckForExistingFile(InputDir,
                                                                EfmuBinCodeManifestProperties.BinCodeManifestSchemaRelPath,
                                                                "Schema"))
            {
                success = false;
            }

            HasBeenBootedSuccessfully = success;
            return success;
        }

        public override bool Run()
        {
            bool success = HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>> EfmuSchemaCopier:Run");
            EfmuConsoleWriter.WriteDebugLine("");
            EfmuConsoleWriter.WriteInfoLine(">> Copying schema files");

            EfmuConsoleWriter.WriteDebugLine($"Source directory: {InputDir}");
            EfmuConsoleWriter.WriteDebugLine($"Destination directory: {OutputDir}");
            EfmuFilesystem.CopyDirectory(InputDir, OutputDir, true, EfmuKnownFileExtensions.SchemaFileSuffix, true);

            return success;
        }
    }
}
