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

using System;

namespace eFMI.Misc
{
    public class EfmuCommandLine
    {
        public static bool HasStringCommandLineOptionBeenGiven(string optionValue)
        {
            return !String.IsNullOrEmpty(optionValue);
        }

        public static bool EnsureThatStringCommandLineOptionHasBeenGiven(string optionValue,
                                                                         string optionName)
        {
            if (HasStringCommandLineOptionBeenGiven(optionValue))
            {
                return true;
            }
            else
            {
                EfmuConsoleWriter.WriteErrorLine($"Required dependent option '{optionName}' has not been specified");
                return false;
            }
        }
    }
}
