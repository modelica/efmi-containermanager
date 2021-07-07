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

using System.IO;

namespace eFMI.ManifestsAndContainers.ManifestProperties
{
    public class EfmuBehavModelManifestProperties
    {
        /* schema */
        public const string BehavModelManifestSchemaDirName = "BehavioralModel";
        public const string BehavModelManifestSchemaFileName = "efmiBehavioralModelManifest.xsd";
        public const string BehavModelManifestSchemaVersion = "0.7.0";


        /* element/attribute names used in manifest file */

        /* TODO: move those which are used by multiple classes/packages */


        public static readonly string BehavModelManifestSchemaRelPath;

        static EfmuBehavModelManifestProperties()
        {
            EfmuBehavModelManifestProperties.BehavModelManifestSchemaRelPath = Path.Combine(
                EfmuBehavModelManifestProperties.BehavModelManifestSchemaDirName,
                EfmuBehavModelManifestProperties.BehavModelManifestSchemaFileName);
        }
    }
}
