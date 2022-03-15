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
    public class EfmuCommonManifestProperties
    {
        public const string EfmiVersion = "1.0.0";

        /* Manifest element and other element with tool/date+time information */
        public const string GenerationTool = "generationTool";
        public const string GenerationDateAndTime = "generationDateAndTime";
        public const string GenerationDateAndTimeFormat = "yyyy-MM-ddTHH:mm:ssZ";

        /* manifest file listing */
        public const string FilesElementName = "Files";
        public const string FileElementName = "File";

        /* values of boolean attributes in manifest files */
        public const string BooleanTrueString = "true";
        public const string BooleanFalseString = "false";
    }
}
