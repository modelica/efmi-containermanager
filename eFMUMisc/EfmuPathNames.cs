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
using System.Text.RegularExpressions;

namespace eFMI.Misc
{
    public class EfmuPathNames
    {
        public const string CurrentDirectory = ".";

        public const string RelativeUrlPrefix = CurrentDirectory + "/";
        public static string RelativePathPrefix;

        public const string RootUrl = "/";

        /* Artificial path for files which are not part of the container (yet)
         * like system header files e.g. dsfxp.h
         *
         * ATTENTION: After the decision of the Emphysis community
         * that paths should either start with "/" or "./",
         * this path is now allowed anymore.
         * But as there is no alternative currently
         * and this paths is only used for Autosar/System headers,
         * it should be no problem.
         * Usage of dsfxp.h has been avoided for saturation macros!
         */
        public const string ArtificialSystemPath = "..";


        static EfmuPathNames()
        {
            /* although only string concatenation, it is safer to perform it here */
            RelativePathPrefix = CurrentDirectory + Path.DirectorySeparatorChar;
        }


        public static bool CanExistanceOfFileBeIgnored(string path)
        {
            return path.Equals(EfmuPathNames.ArtificialSystemPath);
        }


        public static bool IsWellFormedDirName(string dirName)
        {
            bool result = true;

            string regexEscape = new string(Path.GetInvalidPathChars());
            string regexString = "[" + Regex.Escape(regexEscape) + "]";
            Regex containsABadCharacter = new Regex(regexString);
            result = !containsABadCharacter.IsMatch(dirName);

            return result;
        }

        public static bool IsWellFormedFileName(string fileName)
        {
            bool result = true;

            string dirName = Path.GetDirectoryName(fileName);

            if (!String.IsNullOrEmpty(dirName))
            {
                result = IsWellFormedDirName(dirName);
            }

            if (result)
            {
                string baseName = Path.GetFileName(fileName);
                string regexEscape = new string(Path.GetInvalidFileNameChars());
                string regexString = "[" + Regex.Escape(regexEscape) + "]";
                Regex containsABadCharacter = new Regex(regexString);
                result = !containsABadCharacter.IsMatch(baseName);
            }

            return result;
        }

        public static bool CheckForWellFormedDirName(string pathname,
                                                     string kind)
        {
            bool success = true;

            if (!IsWellFormedDirName(pathname))
            {
                EfmuConsoleWriter.WriteErrorLine($"Invalid path for '{kind}': {pathname}");
                success = false;
            }

            return success;
        }

        public static bool HasFileGivenExtension(string fileName,
                                                 string requiredExtension)
        {
            string extension = Path.GetExtension(fileName);
            return (null != extension && extension.Equals(requiredExtension));
        }

        public static bool CheckForRequiredDirectoryNameOrExtensionAndWellFormedFileName(string fileName,
                                                                                            string requiredDirName,
                                                                                            string requiredExtension,
                                                                                            string optionalKind = null)
        {
            bool success = true;

            if (!fileName.Equals(requiredDirName))
            {
                success = CheckForRequiredExtensionAndWellFormedFileName(fileName, requiredExtension, optionalKind);
            }

            return success;
        }

        public static bool CheckForRequiredExtensionAndWellFormedFileName(string fileName,
                                                                          string requiredExtension,
                                                                          string optionalKind = null)
        {
            bool success = true;

            optionalKind = (null != optionalKind) ? $"'{optionalKind}' " : "";
            if (!HasFileGivenExtension(fileName, requiredExtension))
            {
                EfmuConsoleWriter.WriteErrorLine($"Not a {optionalKind}{requiredExtension} file: {fileName}");
                success = false;
            }
            else if (!IsWellFormedFileName(fileName))
            {
                EfmuConsoleWriter.WriteErrorLine($"Invalid path for {optionalKind}{requiredExtension} file: {fileName}");
                success = false;
            }

            return success;
        }

        public static bool CheckForAllowedExtensionsAndWellFormedFileName(string fileName,
                                                                            string[] allowedExtensions,
                                                                            string optionalKind = null)
        {
            bool success = true;

            string allowedExtensionsDescr = String.Join("|", allowedExtensions);

            optionalKind = (null != optionalKind) ? $"'{optionalKind}' " : "";

            bool match = false;

            foreach (string allowedExtension in allowedExtensions)
            {
                if (HasFileGivenExtension(fileName, allowedExtension))
                {
                    match = true;
                    break;
                }
            }

            if (!match)
            {
                EfmuConsoleWriter.WriteErrorLine($"Not a {optionalKind}{allowedExtensionsDescr} file: {fileName}");
                success = false;
            }
            else if (!IsWellFormedFileName(fileName))
            {
                EfmuConsoleWriter.WriteErrorLine($"Invalid path for {optionalKind}{allowedExtensionsDescr} file: {fileName}");
                success = false;
            }

            return success;
        }


        /* Assumes that directory paths have no trailing "\" */
        public static bool IsDirectoryPrefixOfSecondOne(string dirPath1, string dirPath2)
        {
            if (!String.IsNullOrWhiteSpace(dirPath1) && !Path.IsPathRooted(dirPath1))
            {
                dirPath1 = Path.GetFullPath(dirPath1);
            }
            if (!String.IsNullOrWhiteSpace(dirPath2) && !Path.IsPathRooted(dirPath2))
            {
                dirPath2 = Path.GetFullPath(dirPath2);
            }

            /* to avoid that a directory NAME is part of another */
            dirPath1 += Path.DirectorySeparatorChar;
            dirPath2 += Path.DirectorySeparatorChar;

            return dirPath2.StartsWith(dirPath1);
        }

        public static bool CheckThatDirectoryIsNotPrefixOfSecondOne(string dirPath1,
            string kind1,
            string dirPath2,
            string kind2)
        {
            bool success = true;

            if (IsDirectoryPrefixOfSecondOne(dirPath1, dirPath2))
            {
                EfmuConsoleWriter.WriteErrorLine("The first directory is equal to / parent of the second one:");
                EfmuConsoleWriter.WriteErrorLineNoPrefix($" {kind1}: {dirPath1}");
                EfmuConsoleWriter.WriteErrorLineNoPrefix($" {kind2}: {dirPath2}");
                success = false;
            }

            return success;
        }


        public static bool IsFileName(string pathName)
        {
            if (Path.IsPathRooted(pathName))
            {
                //EfmuConsoleWriter.WriteDebugLine($"Path is absolute path: {pathName}");
                return false;
            }

            string directory = Path.GetDirectoryName(pathName);
            if (!String.IsNullOrEmpty(directory) && !directory.Equals(CurrentDirectory))
            {
                //EfmuConsoleWriter.WriteDebugLine($"Path {pathName} is relative path with directory part {directory}");
                return false;
            }

            return true;
        }

        /* strict = not a file name
         * If you want !IsAbsolutePath, use the corresponding method.
         */
        public static bool IsStrictRelativePath(string pathName)
        {
            return !Path.IsPathRooted(pathName) && !IsFileName(pathName);
        }

        public static bool IsAbsolutePath(string pathName)
        {
            return Path.IsPathRooted(pathName);
        }

        public static bool CheckThatPathIsFileName(string pathName)
        {
            bool success = true;

            if (!IsFileName(pathName))
            {
                EfmuConsoleWriter.WriteErrorLine($"The path {pathName} is not a file name, but an absolute/relative path");
                success = false;
            }

            return success;
        }

        public static bool CheckThatPathIsRelative(string pathName)
        {
            bool success = true;

            if (IsAbsolutePath(pathName))
            {
                EfmuConsoleWriter.WriteErrorLine($"The path {pathName} is not a relative path");
                success = false;
            }

            return success;
        }

        public static bool EqualsExceptOptionalRelativeUrlOrPathPrefix(string name,
                                                                        string refName)
        {
            if (name.Equals(refName))
            {
                return true;
            }
            else
            {
                return name.Equals(EfmuPathNames.RelativePathPrefix + refName) || name.Equals(EfmuPathNames.RelativeUrlPrefix + refName);
            }
        }

        public static string GetFileOrDirName(string qualifier)
        {
            string result = System.IO.Path.GetFileName(qualifier);
            if ((string.Empty == result) || (null == result))
            {
                result = System.IO.Path.GetDirectoryName(qualifier);
            }

            return result;
        }

        /* Assumes that the given path is a relative one.
         * Converts directory separators to '/'.
         * Prepends "./" if necessary.
         */
        public static string ConvertRelativePathToUrlNotationWithPrefix(string pathName)
        {
            string url = pathName.Replace(Path.DirectorySeparatorChar, '/');
            if (!url.Equals(CurrentDirectory) && !url.StartsWith(RelativeUrlPrefix) && !url.Equals(ArtificialSystemPath))
            {
                url = RelativeUrlPrefix + url;
            }
            if (url.EndsWith("/"))
            {
                url = url.Remove(url.Length - 1, 1);
            }
            return url;
        }

        public static string ConvertRelativePathToUrlNotationWithPrefixAndRemoveIntermediateDot(string pathName)
        {
            string url = ConvertRelativePathToUrlNotationWithPrefix(pathName);
            url = url.Replace("/./", "/");
            return url;
        }
    }
}
