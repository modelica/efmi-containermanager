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
using System.IO;

namespace eFMI.Misc
{
    public class EfmuConsoleWriter
    {
        /* This class is intended as an interface to output methods
         * to simplify extension and refactoring
         * compared to using standard Console.Write... methods from .NET lib.
         *
         * TODO:
         * - convert this class to a singleton or use factory method
         * - use Tracing service implemented during the development of PropMan NG.
         * - if necessary, extended by further output methods according to the trace levels
         *   in TraceLevel.cs of the TraceService.
         */

        public delegate void DelegateWriteLine(string value); /* and default "NoLine" */
        public delegate void DelegateWriteLineOptionalPrefixTrue(string value,
                                                                 bool optionalPrintPrefix = true);
        public delegate void DelegateWriteNoLineOptionalPrefixFalse(string value,
                                                                    bool optionalPrintPrefix = false);

        public static void PrintDisclaimer()
        {
            /*
            EfmuConsoleWriter.WriteInfoLine("    ##################################################################################");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    # BSD 3 - Clause License                                                         #");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    # Copyright(c) 2021, dSPACE GmbH                                                 #");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    # Redistribution and use in source and binary forms, with or without             #");
            EfmuConsoleWriter.WriteInfoLine("    # modification, are permitted provided that the following conditions are met:    #");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    # 1. Redistributions of source code must retain the above copyright notice, this #");
            EfmuConsoleWriter.WriteInfoLine("    #    list of conditions and the following disclaimer.                            #");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    # 2. Redistributions in binary form must reproduce the above copyright notice,   #");
            EfmuConsoleWriter.WriteInfoLine("    #    this list of conditions and the following disclaimer in the documentation   #");
            EfmuConsoleWriter.WriteInfoLine("    #    and / or other materials provided with the distribution.                    #");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    # 3. Neither the name of the copyright holder nor the names of its               #");
            EfmuConsoleWriter.WriteInfoLine("    #    contributors may be used to endorse or promote products derived from        #");
            EfmuConsoleWriter.WriteInfoLine("    #    this software without specific prior written permission.                    #");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    # THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS \"AS IS\"    #");
            EfmuConsoleWriter.WriteInfoLine("    # AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE      #");
            EfmuConsoleWriter.WriteInfoLine("    # IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE #");
            EfmuConsoleWriter.WriteInfoLine("    # DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE   #");
            EfmuConsoleWriter.WriteInfoLine("    # FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL     #");
            EfmuConsoleWriter.WriteInfoLine("    # DAMAGES(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR      #");
            EfmuConsoleWriter.WriteInfoLine("    # SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER     #");
            EfmuConsoleWriter.WriteInfoLine("    # CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,  #");
            EfmuConsoleWriter.WriteInfoLine("    # OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE   #");
            EfmuConsoleWriter.WriteInfoLine("    # OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.           #");
            EfmuConsoleWriter.WriteInfoLine("    #                                                                                #");
            EfmuConsoleWriter.WriteInfoLine("    ##################################################################################");
            */
            /* Uncomment part above if needed in binary redistribution.
             */
            }

        /* - test logging - */

        private static string TestLogFile = null;

        public static void EnableTestLog(string filename)
        {
            TestLogFile = filename;
            if (null != TestLogFile)
            {
                using (StreamWriter w = File.CreateText(TestLogFile))
                {
                }
            }
        }

        public static bool IsTestLogEnabled()
        {
            return null != TestLogFile;
        }

        private static void PrintToTestLog(string value)
        {
            if (null != TestLogFile)
            {
                using (StreamWriter w = File.AppendText(TestLogFile))
                {
                    w.Write(value);
                }
            }
        }

        public static void WriteTestLogLine(string value)
        {
            PrintToTestLog(value + Environment.NewLine);
        }


            /* - proxy methods - */


        private static void ProxyConsoleWrite(string value,
                                              bool errorInsteadOfOutput)
        {
            if (errorInsteadOfOutput)
            {
                Console.Error.Write(value);
                /* TODO:
                 * Control printing to test log more fine-grained:
                 * - Write<kind>(No)Line methods should pass <kind> to this method
                 * - Logging for each <kind> should be enabled or disabled separately
                 */
                PrintToTestLog(value);
            }
            else
            {
                Console.Write(value);
            }
        }

        private static void ProxyConsoleWriteLine(string value,
                                                  bool errorInsteadOfOutput)
        {
            ProxyConsoleWrite(value + Environment.NewLine, errorInsteadOfOutput);
        }


            /* - debug - */

        private static bool IsDebugOutputEnabled = false;

        public static void EnableDebugOutput(bool status)
        {
            IsDebugOutputEnabled = status;
        }


        public static void WriteDebugLine(string value)
        {
            if (IsDebugOutputEnabled)
            {
                ProxyConsoleWriteLine(value, false);
            }
        }

        public static void WriteDebugNoLine(string value)
        {
            if (IsDebugOutputEnabled)
            {
                ProxyConsoleWrite(value, false);
            }
        }


            /* - temp debug (enabled here) - */

        private static bool IsTempDebugOutputEnabled = false;

        public static void WriteTempDebugLine(string value)
        {
            if (IsTempDebugOutputEnabled)
            {
                ProxyConsoleWriteLine(value, false);
            }
        }

        public static void WriteTempDebugNoLine(string value)
        {
            if (IsTempDebugOutputEnabled)
            {
                ProxyConsoleWrite(value, false);
            }
        }


            /* - info - */

        public static void WriteInfoLine(string value)
        {
            ProxyConsoleWriteLine(value, false);
        }

        public static void WriteInfoNoLine(string value)
        {
            ProxyConsoleWrite(value, false);
        }


            /* - warning - */

        public static void WriteWarningLine(string value,
                                            bool optionalPrintPrefix = true)
        {
            if (optionalPrintPrefix)
            {
                ProxyConsoleWriteLine("Warning: " + value, true);
            }
            else
            {
                ProxyConsoleWriteLine(value, true);
            }
        }

        /* shortcut to support refactoring */
        public static void WriteWarningLineNoPrefix(string value)
        {
            WriteWarningLine(value, false);
        }

        public static void WriteWarningNoLine(string value,
                                              bool optionalPrintPrefix = false)
        {
            if (optionalPrintPrefix)
            {
                ProxyConsoleWrite("Warning: " + value, true);
            }
            else
            {
                ProxyConsoleWrite(value, true);
            }
        }


            /* - error - */

        public static void WriteErrorLine(string value,
                                          bool optionalPrintPrefix = true)
        {
            if (optionalPrintPrefix)
            {
                ProxyConsoleWriteLine("Error: " + value, true);
            }
            else
            {
                ProxyConsoleWriteLine(value, true);
            }
        }

        /* shortcut to support refactoring */
        public static void WriteErrorLineNoPrefix(string value)
        {
            WriteErrorLine(value, false);
        }

        public static void WriteErrorNoLine(string value,
                                            bool optionalPrintPrefix = false)
        {
            if (optionalPrintPrefix)
            {
                ProxyConsoleWrite("Error: " + value, true);
            }
            else
            {
                ProxyConsoleWrite(value, true);
            }
        }


        public static void DumpException(Exception e,
                                            string titleMessage)
        {
            if (IsDebugOutputEnabled)
            {
                EfmuConsoleWriter.WriteErrorLine($"{titleMessage}");
                EfmuConsoleWriter.WriteErrorLineNoPrefix(e.ToString());
                EfmuConsoleWriter.WriteErrorLineNoPrefix("");
            }
            else
            {
                EfmuConsoleWriter.WriteErrorLine($"{titleMessage}");
                EfmuConsoleWriter.WriteErrorLineNoPrefix($"Message: {e.Message}");
                EfmuConsoleWriter.WriteErrorLineNoPrefix(" (Provide -v to see exception details)");
            }
        }

        public static void DumpExitException(Exception e)
        {
            if (IsDebugOutputEnabled)
            {
                EfmuConsoleWriter.WriteErrorLine(e.ToString());
                EfmuConsoleWriter.WriteErrorLineNoPrefix("");
            }
            else
            {
                EfmuConsoleWriter.WriteErrorLine($"{e.Message}");
                EfmuConsoleWriter.WriteErrorLineNoPrefix(" (Provide -v to see exception details)");
            }
        }

    }
}
