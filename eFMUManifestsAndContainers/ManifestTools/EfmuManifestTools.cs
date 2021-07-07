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


using System.Xml.Linq;
using eFMI.ManifestsAndContainers.ManifestProperties;
using eFMI.Misc;

namespace eFMI.ManifestsAndContainers.ManifestTools
{
    public class EfmuManifestTools
    {
        public static string CreateBooleanAttributeValue(bool param)
        {
            if (param)
            {
                return EfmuCommonManifestProperties.BooleanTrueString;
            }
            else
            {
                return EfmuCommonManifestProperties.BooleanFalseString;
            }
        }

        public static bool ParseBoolean(string value, ref bool result)
        {
            bool success = true;

            if (value.Equals(EfmuCommonManifestProperties.BooleanTrueString)
                || value.Equals("1")) /* xs:boolean accepts 1 also! */
            {
                result = true;
            }
            else if (value.Equals(EfmuCommonManifestProperties.BooleanFalseString)
                || value.Equals("0")) /* xs:boolean accepts 0 also! */
            {
                result = false;
            }
            else
            {
                success = false;
            }

            return success;
        }

        public static bool GetBooleanAttributeValue(XElement elem,
                                                    string attributeName,
                                                    ref bool value)
        {
            string valueString = null;
            bool success = EfmuXmlTools.GetAttributeValue(elem, attributeName, ref valueString);
            if (success)
            {
                success = ParseBoolean(valueString, ref value);
            }

            return success;
        }

        public static bool GetOptionalBooleanAttributeValue(XElement elem,
                                                            string attributeName,
                                                            ref bool value,
                                                            out bool hasAttribute)
        {
            bool success = true;

            string valueString = null;
            EfmuXmlTools.GetOptionalAttributeValue(elem, attributeName, ref valueString, out hasAttribute);
            if (success && hasAttribute)
            {
                success = ParseBoolean(valueString, ref value);
            }

            return success;
        }
    }
}
