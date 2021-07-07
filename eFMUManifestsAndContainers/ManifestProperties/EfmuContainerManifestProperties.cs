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
    public class EfmuContainerManifestProperties
    {
        /* schema */
        public const string ContainerManifestSchemaFile = "efmiContainerManifest.xsd";
        public const string ContainerManifestSchemaVersion = "0.9.0";

        /* manifest file */
        public const string ContainerManifestFileName = "__content.xml";

        /* element/attribute names used in manifest file */

        /* TODO: move those which are used by multiple classes/packages */


        public static readonly string EfmuAndSchemaDirNamesInContainer;

        static EfmuContainerManifestProperties()
        {
            EfmuAndSchemaDirNamesInContainer = Path.Combine(EfmuContainerProperties.EfmuDirNameInContainer,
                EfmuContainerProperties.SchemaDirNameInContainer);
        }
    }
}
