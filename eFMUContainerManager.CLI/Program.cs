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

using System;
using System.Collections;
using System.IO;
using CommandLine;
using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.Misc;

namespace eFMI.ContainerManager
{
    class Program
    {
        public const int ExitCodeSuccess = 0;
        public const int ExitCodeError = 1;

        static int Main(string[] args)
        {
            bool success = true;
            bool demandedHelpOrVersion = false;

            Options options = null;

            EfmuContainerOperations containerOperation = EfmuContainerOperations.UNKNOWN;

            EfmuConsoleWriter.PrintDisclaimer();

            ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed<Options>(o =>
                {
                    options = (Options) o.Clone();
                    EfmuConsoleWriter.EnableDebugOutput(options.Verbose);
                    success = CheckOptions(options,
                                           ref containerOperation);
                });
            result.WithNotParsed<Options>((errs) =>
                {
                    IEnumerator enumerator = errs.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        CommandLine.Error error = (CommandLine.Error) enumerator.Current;
                        //EfmuConsoleWriter.WriteInfoLine(error.ToString());
                        if ((error is HelpRequestedError)
                                || (error is VersionRequestedError))
                        {
                            demandedHelpOrVersion = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                });

            if (!success)
            {
                if (ParserResultType.NotParsed == result.Tag)
                {
                    EfmuConsoleWriter.WriteErrorLine("Parsing of command line parameters failed unexpectedly.");
                }
                Environment.Exit(ExitCodeError);
            }
            else if (demandedHelpOrVersion)
            {
                Environment.Exit(ExitCodeSuccess);
            }

            EfmuCoreCallArguments callArguments = new EfmuCoreCallArguments(containerOperation,
                                                                            options.Name,
                                                                            options.ContainerFilePath,
                                                                            options.InputDir,
                                                                            options.ManifestFileName,
                                                                            options.OutputPath,
                                                                            options.ForceOverwriting,
                                                                            !options.NoXmlValidation,
                                                                            !options.IgnoreChecksums);

            EfmuConsoleWriter.WriteDebugLine("Parsed command line options:");
            callArguments.DumpRawToDebug();

            /* run ContainerManager */
            var t = EfmuContainerManager.CreateContainerManager(callArguments);
            try
            {
                success = t.Boot();
                if (success)
                {
                    success = t.Run();
                }
            }
            catch (Exception e)
            {
                EfmuConsoleWriter.DumpException(e, "Caught exception");
                success = false;
            }
            t.Shutdown();

            int returnCode = !success ? ExitCodeError : ExitCodeSuccess;
            EfmuConsoleWriter.WriteDebugLine($"Exiting application with code {returnCode}");
            return returnCode;
        }

        private static bool DetermineContainerOperation(Options options,
                                                        ref EfmuContainerOperations containerOperation)
        {
            bool success = true;

            /* Determine container operation.
             * It is required that exactly ONE operation per call of the ContainerManager is demanded by the user.
             */
            int numDemandedContainerOperations = 0;
            numDemandedContainerOperations += options.CreateContainer ? 1 : 0;
            numDemandedContainerOperations += options.AddToContainer ? 1 : 0;
            numDemandedContainerOperations += options.ReplaceInContainer ? 1 : 0;
            numDemandedContainerOperations += options.DeleteFromContainer ? 1 : 0;
            numDemandedContainerOperations += options.ExtractFromContainer ? 1 : 0;
            numDemandedContainerOperations += options.ExtractSchemasFromContainer ? 1 : 0;
            numDemandedContainerOperations += options.UnpackFmu ? 1 : 0;
            numDemandedContainerOperations += options.TidyRoot ? 1 : 0;
            numDemandedContainerOperations += options.ListContainerContent ? 1 : 0;

            if (1 != numDemandedContainerOperations)
            {
                EfmuConsoleWriter.WriteErrorLine($"It is required that exactly ONE container operation is demanded, but you demanded {numDemandedContainerOperations}");
                success = false;
            }

            if (success)
            {
                if (options.CreateContainer)
                {
                    containerOperation = EfmuContainerOperations.CreateContainer;
                }
                else if (options.AddToContainer)
                {
                    containerOperation = EfmuContainerOperations.AddToContainer;
                }
                else if (options.ReplaceInContainer)
                {
                    containerOperation = EfmuContainerOperations.ReplaceInContainer;
                }
                else if (options.DeleteFromContainer)
                {
                    containerOperation = EfmuContainerOperations.DeleteFromContainer;
                }
                else if (options.ExtractFromContainer)
                {
                    containerOperation = EfmuContainerOperations.ExtractFromContainer;
                }
                else if (options.ExtractSchemasFromContainer)
                {
                    containerOperation = EfmuContainerOperations.ExtractSchemasFromContainer;
                }
                else if (options.UnpackFmu)
                {
                    containerOperation = EfmuContainerOperations.UnpackFmu;
                }
                else if (options.TidyRoot)
                {
                    containerOperation = EfmuContainerOperations.TidyRoot;
                }
                else if (options.ListContainerContent)
                {
                    containerOperation = EfmuContainerOperations.ListContainerContent;
                }
                /* else not needed by construction */
            }

            return success;
        }

        private static bool CheckDependentOptions(EfmuContainerOperations containerOperation,
                                                  Options options)
        {
            bool success = true;

            switch (containerOperation)
            {
                case EfmuContainerOperations.CreateContainer:
                {
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.InputDir,
                            "InputDir with schemas");
                    }
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.Name,
                            "Container name");
                    }

                    if (success)
                    {
                        success = EfmuPathNames.CheckForWellFormedDirName(options.InputDir, "InputDir with schemas");
                    }
                    if (success)
                    {
                        if (EfmuCommandLine.HasStringCommandLineOptionBeenGiven(options.OutputPath))
                        {
                            success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.OutputPath,
                                EfmuContainerProperties.EfmuFileSuffix);
                        }
                        else
                        {
                            options.OutputPath = "container" + EfmuContainerProperties.EfmuFileSuffix;
                        }
                    }
                }
                    break;
                case EfmuContainerOperations.AddToContainer:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.Name,
                            "Name of model representation");
                    }
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.InputDir,
                            "InputDir");
                    }
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.ManifestFileName,
                            "ManifestFileName");
                    }

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                                                                                        EfmuContainerProperties.EfmuFileSuffix);
                    }
                    if (success)
                    {
                        success = EfmuPathNames.CheckForWellFormedDirName(options.InputDir, "InputDir");
                    }
                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ManifestFileName,
                                                                                        EfmuKnownFileExtensions.ManifestFileSuffix);
                        if (success)
                        {
                            success = EfmuPathNames.CheckThatPathIsFileName(options.ManifestFileName);
                        }
                    }
                }
                    break;
                case EfmuContainerOperations.ReplaceInContainer:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.Name,
                            "Name of model representation");
                        }
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.InputDir,
                            "InputDir");
                    }
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.ManifestFileName,
                            "ManifestFileName");
                    }

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                                                                                        EfmuContainerProperties.EfmuFileSuffix);
                    }
                    if (success)
                    {
                        success = EfmuPathNames.CheckForWellFormedDirName(options.InputDir, "InputDir");
                    }
                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ManifestFileName,
                                                                                        EfmuKnownFileExtensions.ManifestFileSuffix);
                        if (success)
                        {
                            success = EfmuPathNames.CheckThatPathIsFileName(options.ManifestFileName);
                        }
                    }
                }
                    break;
                case EfmuContainerOperations.DeleteFromContainer:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.Name,
                            "Name of model representation");
                        }

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                                                                                        EfmuContainerProperties.EfmuFileSuffix);
                    }
                }
                    break;
                case EfmuContainerOperations.ExtractFromContainer:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.Name,
                            "Name of model representation");
                        }
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.OutputPath,
                            "OutputPath");
                    }

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                                                                                        EfmuContainerProperties.EfmuFileSuffix);
                    }
                    if (success)
                    {
                        success = EfmuPathNames.CheckForWellFormedDirName(options.OutputPath, "OutputPath");
                    }
                }
                    break;
                case EfmuContainerOperations.ExtractSchemasFromContainer:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.OutputPath,
                            "OutputPath");
                    }

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                            EfmuContainerProperties.EfmuFileSuffix);
                    }
                    if (success)
                    {
                        success = EfmuPathNames.CheckForWellFormedDirName(options.OutputPath, "OutputPath");
                    }
                }
                    break;
                case EfmuContainerOperations.UnpackFmu:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");
                    if (success)
                    {
                        success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(
                            options.Name,
                            "Name of model representation");
                        }

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                                                                                        EfmuContainerProperties.EfmuFileSuffix);
                    }
                }
                    break;
                case EfmuContainerOperations.TidyRoot:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                            EfmuContainerProperties.EfmuFileSuffix);
                    }
                }
                    break;
                case EfmuContainerOperations.ListContainerContent:
                {
                    success = EfmuCommandLine.EnsureThatStringCommandLineOptionHasBeenGiven(options.ContainerFilePath,
                        "ContainerFilePath");

                    if (success)
                    {
                        success = EfmuPathNames.CheckForRequiredExtensionAndWellFormedFileName(options.ContainerFilePath,
                                                                                        EfmuContainerProperties.EfmuFileSuffix);
                    }
                }
                    break;
                case EfmuContainerOperations.UNKNOWN:
                {
                    /* should never happen */
                    success = false;
                }
                    break;
            }

            return success;
        }

        private static bool CheckDisjointnessOfDirectories(Options options,
                                                           EfmuContainerOperations containerOperation)
        {
            bool success = true;

            string inputDir = options.InputDir;
            string containerDir = (null != options.ContainerFilePath)
                                        ? Path.GetDirectoryName(options.ContainerFilePath)
                                        : null;
            string outputDir = null;
            if (EfmuContainerOperations.CreateContainer == containerOperation)
            {
                outputDir = Path.GetDirectoryName(options.OutputPath);
            }
            else if (EfmuContainerOperations.ExtractFromContainer == containerOperation
                        || EfmuContainerOperations.ExtractSchemasFromContainer == containerOperation)
            {
                outputDir = options.OutputPath;
            }

            if (success && null != inputDir && null != outputDir)
            {
                /* "create" operation:
                 * A container must not be created in the input directory (schemas).
                 */
                success = EfmuPathNames.CheckThatDirectoryIsNotPrefixOfSecondOne(inputDir, "Input", outputDir, "Output");
            }
            else if (success && null != inputDir && null != containerDir)
            {
                /* "add", "replace", "schemaupdate" operations :
                 * Means that EFMU containers must not be stored within the input directories.
                 */
                success = EfmuPathNames.CheckThatDirectoryIsNotPrefixOfSecondOne(inputDir, "Input", containerDir, "EFMU container");
            }
            else if (success && null != containerDir && null != outputDir)
            {
                /* "extract" operation:
                 * It is legal to create a directory in the parent directory of the container.
                 */
                //success = EfmuPathNames.CheckThatDirectoryIsNotPrefixOfSecondOne(containerDir, "EFMU container", outputDir, "Output");
            }

            return success;
        }

        private static bool CheckOptions(Options options,
                                         ref EfmuContainerOperations containerOperation)
        {
            bool success = true;

            success = DetermineContainerOperation(options,
                                                    ref containerOperation);

            if (success)
            {
                EfmuConsoleWriter.WriteDebugLine($"Selected container operation: {containerOperation}");
                success = CheckDependentOptions(containerOperation,
                                                options);
            }

            if (success)
            {
                success = CheckDisjointnessOfDirectories(options, containerOperation);
            }

            return success;
        }
    }
}
