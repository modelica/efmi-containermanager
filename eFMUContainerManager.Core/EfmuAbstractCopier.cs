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
    abstract class EfmuAbstractCopier
    {
        protected string InputDir;
        protected string OutputDir;

        protected bool HasBeenBootedSuccessfully;

        /* inputDir: input directory, corresponds to directory to be copied
         *              or represents base directory for a source directory,
         *              which is determined by inheriting class
         * outputDir: output directory, like inputDir
         */
        public EfmuAbstractCopier(string inputDir,
                                  string outputDir)
        {
            this.InputDir = inputDir;
            this.OutputDir = outputDir;

            this.HasBeenBootedSuccessfully = false;
        }

        /* Checks if everything is OK for copying files like:
         * - source directory exists
         * - certain required files exist in source directory
         */
        public abstract bool Boot();

        /* Performs the actual copy operations. */
        public abstract bool Run();


        /* Common checks during Boot. */
        protected bool PerformCommonBootChecks()
        {
            return EfmuFilesystem.CheckForExistingDirectory(InputDir, "Input");
        }
    }
}
