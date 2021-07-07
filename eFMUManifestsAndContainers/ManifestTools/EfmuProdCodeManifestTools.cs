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

using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.Misc;

namespace eFMI.ManifestsAndContainers.ManifestTools
{
    public class EfmuProdCodeManifestTools
    {
        public static bool CheckFmuReference(string fmuReference,
                                                string optionalKind = null)
        {
            return EfmuPathNames.CheckForRequiredDirectoryNameOrExtensionAndWellFormedFileName(fmuReference,
                EfmuProdCodeManifestProperties.FmuDirName,
                EfmuContainerProperties.FmuFileSuffix);
        }

        public static bool CategorizeFmuReference(string fmuReference,
                                                    ref bool isFileInsteadOfFolder)
        {
            bool success = true;

            if (fmuReference.EndsWith(EfmuContainerProperties.FmuFileSuffix))
            {
                isFileInsteadOfFolder = true;
            }
            else if (EfmuPathNames.EqualsExceptOptionalRelativeUrlOrPathPrefix(fmuReference,
                EfmuProdCodeManifestProperties.FmuDirName))
            {
                isFileInsteadOfFolder = false;
            }
            else
            {
                success = false;
            }

            return success;
        }
    }
}
