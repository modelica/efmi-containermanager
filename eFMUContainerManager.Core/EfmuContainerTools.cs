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
using System.IO;
using eFMI.Misc;

namespace eFMI.ContainerManager
{
    class EfmuContainerTools
    {
        public delegate bool BoolReturnMethod();

        public delegate bool BoolReturnStringParamMethod(string param);

        /* TODO: refactoring + different solution */
        public static bool PerformCallWithExceptionHandling(BoolReturnMethod handler)
        {
            bool success = true;

            try
            {
                success = handler();
            }
            catch (IOException e)
            {
                EfmuConsoleWriter.DumpException(e, "Caught IO exception");
                success = false;
            }
            catch (Exception e)
            {
                EfmuConsoleWriter.DumpException(e, "Caught exception");
                success = false;
            }

            return success;
        }

        /* TODO: refactoring + different solution */
        public static bool PerformCallWithExceptionHandling(BoolReturnStringParamMethod handler,
                                                            string handlerParam)
        {
            bool success = true;

            try
            {
                success = handler(handlerParam);
            }
            catch (IOException e)
            {
                EfmuConsoleWriter.DumpException(e, "Caught IO exception");
                success = false;
            }
            catch (Exception e)
            {
                EfmuConsoleWriter.DumpException(e, "Caught exception");
                success = false;
            }

            return success;
        }

        public static bool DoesContainerOperationImplyFinalWrite(EfmuContainerOperations containerOperation)
        {
            switch (containerOperation)
            {
                case EfmuContainerOperations.CreateContainer:
                case EfmuContainerOperations.AddToContainer:
                case EfmuContainerOperations.ReplaceInContainer:
                case EfmuContainerOperations.DeleteFromContainer:
                case EfmuContainerOperations.UnpackFmu:
                case EfmuContainerOperations.TidyRoot:
                {
                    return true;
                }
            }

            return false;
        }

        public static bool DoesContainerOperationRequireInitialRead(EfmuContainerOperations containerOperation)
        {
            switch (containerOperation)
            {
                case EfmuContainerOperations.AddToContainer:
                case EfmuContainerOperations.ReplaceInContainer:
                case EfmuContainerOperations.DeleteFromContainer:
                case EfmuContainerOperations.ExtractFromContainer:
                case EfmuContainerOperations.ExtractSchemasFromContainer:
                case EfmuContainerOperations.UnpackFmu:
                case EfmuContainerOperations.TidyRoot:
                case EfmuContainerOperations.ListContainerContent:
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsContainerOperationReadyOnly(EfmuContainerOperations containerOperation)
        {
            return DoesContainerOperationRequireInitialRead(containerOperation)
                   && !DoesContainerOperationImplyFinalWrite(containerOperation);
        }

        public static bool IsExtractFromContainerOperation(EfmuContainerOperations containerOperation)
        {
            return (EfmuContainerOperations.ExtractFromContainer == containerOperation
                    || EfmuContainerOperations.ExtractSchemasFromContainer == containerOperation);
        }
    }
}
