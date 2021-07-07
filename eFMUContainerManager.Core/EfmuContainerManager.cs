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

using eFMI.Misc;

namespace eFMI.ContainerManager
{
    public class EfmuContainerManager : IEfmuContainerManager
    {
        private EfmuCoreCallArguments CallArguments;

        private bool HasBeenBootedSuccessfully;

        private EfmuContainer Container;


        public static IEfmuContainerManager CreateContainerManager(EfmuCoreCallArguments callArguments)
        {
            return new EfmuContainerManager(callArguments);
        }

        private EfmuContainerManager(EfmuCoreCallArguments callArguments)
        {
            this.CallArguments = callArguments;

            this.HasBeenBootedSuccessfully = false;

            /* remaining members are initialized with null automatically */
        }

        public bool Boot()
        {
            bool success = !HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>>> EfmuContainerManager::Boot");
            CallArguments.DumpRelevantAttributes(EfmuConsoleWriter.WriteInfoLine);

            if (success)
            {
                /* Create instance of Container.
                 * => See comment at the beginning of the class
                 */
                Container = new EfmuContainer(CallArguments);
                success = Container.Boot();
            }

            HasBeenBootedSuccessfully = success;
            return success;
        }

        public bool Run()
        {
            bool success = HasBeenBootedSuccessfully;

            EfmuConsoleWriter.WriteDebugLine("\n>>> EfmuContainerManager::Run");

            if (success)
            {
                success = Container.Run();
            }

            return success;
        }

        public void Shutdown()
        {
            if (null != Container)
            {
                Container.Shutdown();
            }
        }
    }
}
