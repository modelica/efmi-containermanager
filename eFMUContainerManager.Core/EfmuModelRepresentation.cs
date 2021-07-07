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

namespace eFMI.ContainerManager
{
    class EfmuModelRepresentation
    {
        /* Name of model representation, must be unique within EFMU container */
        public string Name { get; private set; }

        /* OptionalKind i.e. equation, algorithm, ... */
        public EfmuModelRepresentationKind Kind { get; private set; }

        /* Computed checksum for manifest */
        public string Checksum { get; private set; }

        /* Name of manifest file, must be relative to root of subtree,
         * i.e. ./manifest.xml
         */
        public string Manifest { get; private set; }

        /* Id from manifest file */
        public string ManifestId { get; private set; }

        /* Relative path to FMU.
         * Is valid if != null.
         */
        public string OptionalFmuReference { get; private set; }


        public EfmuModelRepresentation(string name,
                                        EfmuModelRepresentationKind kind,
                                        string manifest,
                                        string checksum,
                                        string manifestId,
                                        string optionalFmuReference = null)
        {
            this.Name = name;
            this.Kind = kind;
            this.Manifest = manifest;
            this.Checksum = checksum;
            this.ManifestId = manifestId;
            this.OptionalFmuReference = optionalFmuReference;
        }

        public bool HasFmuReference()
        {
            return null != OptionalFmuReference;
        }
    }
}
