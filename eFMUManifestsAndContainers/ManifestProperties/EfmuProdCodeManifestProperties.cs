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
    public class EfmuProdCodeManifestProperties
    {
        /* schema */
        public const string ProdCodeManifestSchemaDirName = "ProductionCode";
        public const string ProdCodeManifestSchemaFileName = "efmiProductionCodeManifest.xsd";
        public const string ProdCodeManifestSchemaVersion = "0.17.0";

        public const string ProdCodeManifestCreatorToolName = "eFMUProdCodeManifestCreator";

        /* element/attribute names used in manifest file */

        public const string FmuDirName = "FMU";

        /* TODO: move those which are used by multiple classes/packages */


        public static readonly string ProdCodeManifestSchemaRelPath;

        static EfmuProdCodeManifestProperties()
        {
            EfmuProdCodeManifestProperties.ProdCodeManifestSchemaRelPath = Path.Combine(EfmuProdCodeManifestProperties.ProdCodeManifestSchemaDirName,
                EfmuProdCodeManifestProperties.ProdCodeManifestSchemaFileName);
        }
    }
}
