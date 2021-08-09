/*
 * Copyright (c) 2021, dSPACE GmbH, Modelica Association and contributors
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
using System.Globalization;

namespace eFMI.Misc
{
    public class EfmuFloatingPointNumbers
    {
        private static readonly NumberStyles Style;
        private static readonly CultureInfo Culture;

        static EfmuFloatingPointNumbers()
        {
            Style = NumberStyles.Number | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;
            //NumberStyles.AllowLeadingSign
            Culture = CultureInfo.CreateSpecificCulture("en-US");
        }

        public static string DoubleToString(double value)
        {
            return value.ToString(Culture);
        }

        public static bool HasNoDecimalPlaces(double value)
        {
            return Math.Abs(value % 1) <= Double.Epsilon;
        }

        public static string DoubleToStringForType(double value, bool isFloatingPointType)
        {
            if (isFloatingPointType)
            {
                return DoubleToStringWithAtLeastOneDecimal(value);
            }
            else
            {
                return DoubleToString(value);
            }
        }

        public static string DoubleToStringWithAtLeastOneDecimal(double value)
        {
            if (HasNoDecimalPlaces(value))
            {
                return value.ToString("0.0###############", Culture);
            }
            else
            {
                return DoubleToString(value);
            }
        }

        public static bool ParseDouble(string valueString, out double result)
        {
            return Double.TryParse(valueString, Style, Culture, out result);
        }
    }
}
