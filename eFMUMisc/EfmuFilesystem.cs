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
using System.IO;

namespace eFMI.Misc
{
    public class EfmuFilesystem
    {
        public static bool DoesDirectoryExist(string pathName)
        {
            return Directory.Exists(pathName);
        }

        public static bool DoesFileExist(string pathName)
        {
            return File.Exists(pathName);
        }

        public static bool DoesFileOrDirectoryExist(string pathName,
                                                    ref bool isFileInsteadOfDirectory)
        {
            bool result = false;

            if (DoesFileExist(pathName))
            {
                isFileInsteadOfDirectory = true;
                result = true;
            }
            else if (DoesDirectoryExist(pathName))
            {
                isFileInsteadOfDirectory = false;
                result = true;
            }

            return result;
        }

        public static bool CheckForExistingDirectory(string pathName,
                                                     string kind,
                                                     bool optionalIsRequiredInsteadOfOptional = true)
        {
            bool success = true;

            if (!DoesDirectoryExist(pathName))
            {
                if (optionalIsRequiredInsteadOfOptional)
                {
                    EfmuConsoleWriter.WriteErrorLine($"The required '{kind}' directory does not exist: {pathName}");
                }
                else
                {
                    EfmuConsoleWriter.WriteWarningLine($"The optional '{kind}' directory does not exist: {pathName}");
                }
                success = false;
            }

            return success;
        }

        public static bool CheckForExistingFile(string baseDir,
                                                string relativePath,
                                                string kind,
                                                bool optionalIsRequiredInsteadOfOptional = true)
        {
            string pathName = Path.Combine(baseDir, relativePath);
            return CheckForExistingFile(pathName, kind, optionalIsRequiredInsteadOfOptional);
        }

        public static bool CheckForExistingFile(string pathName,
                                                string kind,
                                                bool optionalIsRequiredInsteadOfOptional = true)
        {
            bool success = true;

            if (!DoesFileExist(pathName))
            {
                if (optionalIsRequiredInsteadOfOptional)
                {
                    EfmuConsoleWriter.WriteErrorLine($"The following required '{kind}' file does not exist: {pathName}");
                }
                else
                {
                    EfmuConsoleWriter.WriteWarningLine($"The following optional '{kind}' file does not exist: {pathName}");
                }
                success = false;
            }

            return success;
        }

        public static bool CheckForExistingFileOrDirectory(string pathName,
                                                            string kind,
                                                            bool optionalIsRequiredInsteadOfOptional = true)
        {
            bool success = true;

            bool dummy = false;
            if (!DoesFileOrDirectoryExist(pathName, ref dummy))
            {
                if (optionalIsRequiredInsteadOfOptional)
                {
                    EfmuConsoleWriter.WriteErrorLine($"The following required '{kind}' file/directory does not exist: {pathName}");
                }
                else
                {
                    EfmuConsoleWriter.WriteWarningLine($"The following optional '{kind}' file/directory does not exist: {pathName}");
                }
                success = false;
            }

            return success;
        }

        /* Determines random name of a non-existing directory within windows temporary directory.
         * The directory is created and the absolute path is returned.
         */
        public static string GetTempDirectoryName()
        {
            string tempRoot = Path.GetTempPath();
            string dirName = null;
            string dirPath = null;

            do
            {
                dirName = Path.GetRandomFileName();
                dirPath = Path.Combine(tempRoot, dirName);
            } while (EfmuFilesystem.DoesDirectoryExist(dirPath));

            EfmuFilesystem.CreateDirectory(dirPath);
            return dirPath;
        }


        public static bool CreateDirectory(string pathName)
        {
            /* C# method works recursive */
            Directory.CreateDirectory(pathName);
            return true;
        }

        public static bool CreateDirectoryOfFileIfNotExisting(string pathName)
        {
            pathName = Path.GetDirectoryName(pathName);
            return CreateDirectoryIfNotExisting(pathName);
        }

        public static bool CreateDirectoryIfNotExisting(string pathName)
        {
            if (String.IsNullOrEmpty(pathName) || DoesDirectoryExist(pathName))
            {
                return true;
            }
            else
            {
                return CreateDirectory(pathName);
            }
        }


        /* Used by CopyDirectory method. */
        private static bool MustFileBeCopied(string pathName,
                                                  string optionalRestrictOrExcludeFileExtension = null,
                                                  bool optionalRestrictInsteadOfExclude = true)
        {
            if (String.IsNullOrEmpty(optionalRestrictOrExcludeFileExtension))
            {
                return true;
            }
            else
            {
                bool hasGivenExtension = EfmuPathNames.HasFileGivenExtension(pathName, optionalRestrictOrExcludeFileExtension);
                if (!(optionalRestrictInsteadOfExclude ^ hasGivenExtension))
                {
                    return true;
                }
            }

            return false;
        }

        /* Used by CopyDirectory method. */
        private static bool MustDirBeCopied(string pathName,
                                            string optionalRestrictOrExcludeFileExtension = null,
                                            bool optionalRestrictInsteadOfExclude = true)
        {
            if (String.IsNullOrEmpty(optionalRestrictOrExcludeFileExtension))
            {
                return true;
            }
            else
            {
                bool hasGivenExtension = EfmuPathNames.HasFileGivenExtension(pathName, optionalRestrictOrExcludeFileExtension);
                if (optionalRestrictInsteadOfExclude /* "restrict" not relevant for directories */
                    || (!optionalRestrictInsteadOfExclude && !hasGivenExtension))
                {
                    return true;
                }
            }

            return false;
        }

        /* Creates a copy of the given source directory at the specified destination.
         * It is assumed that the source directory exists.
         * Creates the destination directory if necessary.
         * Overwrites existing files (and directories if recursive copy is enabled).
         *
         * If the optional parameter "optionalRestrictOrExcludeFileExtension"
         * is set to a value != null, the copy operation considers the extensions
         * of files to be copied: The optional parameter "optionalRestrictInsteadOfExclude" selects between
         * "restrict to copying certain files" or "exclude certain files from being copied".
         * For directories, the behaviour is similar except that "restrict" mode implies that all directories are copied.
         *
         * Creates destination directory using CreateDirectory,
         * i.e. missing subdirectories are created recursively.
         */
        public static void CopyDirectory(string sourceDirName,
                                         string destDirName,
                                         bool optionalRecursive = true,
                                         string optionalRestrictOrExcludeFileExtension = null,
                                         bool optionalRestrictInsteadOfExclude = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!EfmuFilesystem.DoesDirectoryExist(destDirName))
            {
                EfmuFilesystem.CreateDirectory(destDirName);
            }
        
            /* copy files */
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (MustFileBeCopied(file.Name,
                                     optionalRestrictOrExcludeFileExtension,
                                     optionalRestrictInsteadOfExclude))
                {
                    string destPath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(destPath, true);
                }
            }

            if (optionalRecursive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    if (MustDirBeCopied(subdir.Name,
                                        optionalRestrictOrExcludeFileExtension,
                                        optionalRestrictInsteadOfExclude))
                    {
                        string destPath = Path.Combine(destDirName, subdir.Name);
                        CopyDirectory(subdir.FullName,
                                        destPath,
                                        optionalRecursive,
                                        optionalRestrictOrExcludeFileExtension,
                                        optionalRestrictInsteadOfExclude);
                    }
                }
            }
        }

        /* Removes given directory recursively, i.e. with all subdirectories. */
        public static bool RemoveDirectory(string dirPath)
        {
            /* Removes directory recursively. */
            Directory.Delete(dirPath, true);
            return true;
        }

    }
}
