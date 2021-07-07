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

namespace eFMI.ManifestsAndContainers.ManifestFileListing
{
    /* NOTE: The string representation of the enum values MUST be consistent with the schema
     * in order to allow automatic parsing by C#
     */
    public enum EfmuFileEntryRole
    {
        Code,

        /* NOTE: The manifest file itself is NOT listed,
         * although this has been specified, because ..
         * (1) it does not make sense as it is listed in the container manifest
         * (2) it cannot be checksummed without modifying it ...
         */
        //Manifest,

        FMU,

        FMUFolder,

        /* No special support yet but not forbidden explictly */
        ReferenceData,

        other
    }
}
