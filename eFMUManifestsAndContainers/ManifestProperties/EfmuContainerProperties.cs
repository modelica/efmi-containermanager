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

namespace eFMI.ManifestsAndContainers.ManifestProperties
{
    public class EfmuContainerProperties
    {
        /* EFMU containers have the same file suffix like FMU containers.
         * They can only be distinguished by checking whether an "eFMU" directory exists or not.
         */
        public const string EfmuFileSuffix = ".fmu";
        public const string FmuFileSuffix = ".fmu";

        public const string EfmuDirNameInContainer = "eFMU";

        public const string SchemaDirNameInContainer = "schemas";
    }
}
