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
    public class EfmuStringToValueParser
    {
        /* Not used currently.
         * For parsing boolean attributes of manifests use the specific methods!
         */
        /*public static bool ParseBoolean(string valueString, out bool result)
        {
            return Boolean.TryParse(valueString, out result);
        }*/

        public static bool ParseInteger(string valueString, out int result)
        {
            return Int32.TryParse(valueString, out result);
        }
        public static bool ParseUnsignedInteger(string valueString, out uint result)
        {
            /* not CLS compliant */
            return UInt32.TryParse(valueString, out result);
        }

        public static bool ParseLongInteger(string valueString, out long result)
        {
            return Int64.TryParse(valueString, out result);
        }
        public static bool ParseUnsignedLongInteger(string valueString, out ulong result)
        {
            return UInt64.TryParse(valueString, out result);
        }

        public static bool ParseDouble(string valueString, out double result)
        {
            return EfmuFloatingPointNumbers.ParseDouble(valueString, out result);
        }
    }
}
