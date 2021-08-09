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

using eFMI.Misc;

namespace eFMI.ContainerManager
{
    public class EfmuCoreCallArguments
    {
        public EfmuContainerOperations ContainerOperation { get;  }

        public string Name { get; }

        public string ContainerFilePath { get; }
        public string InputDir { get; }
        public string ManifestFileName { get; }
        public string OutputPath { get; }

        public bool ForceOverwriting { get; }

        public bool ValidateXmlTree { get; }
        public bool ValidateChecksums { get; }

        /* Some arguments may be null.
         * The command line handling of the .CLI assembly ensures
         * that the arguments are consistent.
         */
        public EfmuCoreCallArguments(EfmuContainerOperations containerOperation,
                                     string name,
                                     string containerFilePath,
                                     string inputDir,
                                     string manifestFileName,
                                     string outputPath,
                                     bool forceOverwriting,
                                     bool validateXmlTree,
                                     bool validateChecksums)
        {
            this.ContainerOperation = containerOperation;
            this.Name = name;
            this.ContainerFilePath = containerFilePath;
            this.InputDir = inputDir;
            this.ManifestFileName = manifestFileName;
            this.OutputPath = outputPath;
            this.ForceOverwriting = forceOverwriting;
            this.ValidateXmlTree = validateXmlTree;
            this.ValidateChecksums = validateChecksums;
        }

        public void DumpRawToDebug()
        {
            EfmuConsoleWriter.WriteDebugLine("All arguments:");
            EfmuConsoleWriter.WriteDebugLine($" ContainerOperation: {ContainerOperation}");
            EfmuConsoleWriter.WriteDebugLine($" Name: {Name}");
            EfmuConsoleWriter.WriteDebugLine($" ContainerFilePath: {ContainerFilePath}");
            EfmuConsoleWriter.WriteDebugLine($" InputDir: {InputDir}");
            EfmuConsoleWriter.WriteDebugLine($" ManifestFileName: {ManifestFileName}");
            EfmuConsoleWriter.WriteDebugLine($" OutputPath: {OutputPath}");
            EfmuConsoleWriter.WriteDebugLine($" ForceOverwriting: {ForceOverwriting}");
            EfmuConsoleWriter.WriteDebugLine($" ValidateXmlTree: {ValidateXmlTree}");
            EfmuConsoleWriter.WriteDebugLine($" ValidateChecksums: {ValidateChecksums}");
            EfmuConsoleWriter.WriteDebugLine("");
        }

        public void DumpRelevantAttributes(EfmuConsoleWriter.DelegateWriteLine handler)
        {
            handler("Call arguments:");
            handler($" ContainerOperation: {ContainerOperation}");
            switch (ContainerOperation)
            {
                case EfmuContainerOperations.CreateContainer:
                {
                    handler($" InputDir: {InputDir}");
                    handler($" OutputPath: {OutputPath}");
                    handler($" ForceOverwriting: {ForceOverwriting}");
                }
                    break;
                case EfmuContainerOperations.AddToContainer:
                {
                    handler($" ContainerFilePath: {ContainerFilePath}");
                    handler($" Name: {Name}");
                    handler($" InputDir: {InputDir}");
                    handler($" ManifestFileName: {ManifestFileName}");
                }
                    break;
                case EfmuContainerOperations.ReplaceInContainer:
                {
                    handler($" ContainerFilePath: {ContainerFilePath}");
                    handler($" Name: {Name}");
                    handler($" InputDir: {InputDir}");
                    handler($" ManifestFileName: {ManifestFileName}");
                }
                    break;
                case EfmuContainerOperations.DeleteFromContainer:
                {
                    handler($" ContainerFilePath: {ContainerFilePath}");
                    handler($" Name: {Name}");
                }
                    break;
                case EfmuContainerOperations.ExtractFromContainer:
                {
                    handler($" ContainerFilePath: {ContainerFilePath}");
                    handler($" Name: {Name}");
                    handler($" OutputPath: {OutputPath}");
                    handler($" ForceOverwriting: {ForceOverwriting}");
                }
                    break;
                case EfmuContainerOperations.ExtractSchemasFromContainer:
                {
                    handler($" ContainerFilePath: {ContainerFilePath}");
                    handler($" OutputPath: {OutputPath}");
                    handler($" ForceOverwriting: {ForceOverwriting}");
                }
                    break;
                case EfmuContainerOperations.UnpackFmu:
                {
                    handler($" ContainerFilePath: {ContainerFilePath}");
                    handler($" Name: {Name}");
                }
                    break;
                case EfmuContainerOperations.ListContainerContent:
                {
                    handler($" ContainerFilePath: {ContainerFilePath}");
                }
                    break;
                case EfmuContainerOperations.UNKNOWN:
                {
                    handler("--- never happens :-) ---");
                }
                    break;
            }

            //handler($" Verbose: {Verbose}");

            handler("");
        }
    }
}
