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
using eFMI.Misc;

namespace eFMI.ManifestsAndContainers.ManifestFileListing
{
    public class EfmuFileListingEntry
    {
        /* only for CreateFileListingEntryFromData */
        public const bool CheckExistanceOfNonChecksummedFiles = true;

        public const string DefaultId = "NO_ID";

        public const string DefaultPath = EfmuPathNames.CurrentDirectory;

        public const bool DefaultNeedsChecksum = true;

        public string Id { get; private set; }
        public string UniqueName { get; private set; }
        public string Path { get; private set; }
        public bool NeedsChecksum { get; private set; }
        /* May be null, because optional.
         * Must be set if NeedsChecksum=true.
         */
        public string Checksum { get; private set; }

        public EfmuFileEntryRole Role { get; private set; }
        /* May be null, because optional */
        public string OptionalKind { get; private set; }


        /* To be called from ManifestCreator, computes checksum if necessary. */
        public static bool CreateFileListingEntryFromData(string rootDir,
                                                            string uniqueName,
                                                            string path,
                                                            bool needsChecksum,
                                                            EfmuFileEntryRole role,
                                                            string kind, /* optional, may be null */
                                                            ref EfmuFileListingEntry fileEntry)
        {
            bool success = true;

            string checksum = null;

            if (needsChecksum)
            {
                string filePath = System.IO.Path.Combine(rootDir, path, uniqueName);
                /* TODO: If FMUFolder should be checksummed, we have to allow directories here! */
                if (!EfmuFilesystem.DoesFileExist(filePath))
                {
                    EfmuConsoleWriter.WriteErrorLine($"Cannot compute checksum for file listing entry {uniqueName}, because the file does not exist");
                    string tempFilePath = EfmuPathNames.ConvertRelativePathToUrlNotationWithPrefixAndRemoveIntermediateDot(filePath);
                    EfmuConsoleWriter.WriteErrorLineNoPrefix($" file path: {tempFilePath}");
                    success = false;
                }

                if (success)
                {
                    checksum = EfmuChecksum.ComputeChecksumOfFile(filePath);
                }
            }
            else if (CheckExistanceOfNonChecksummedFiles && !EfmuPathNames.CanExistanceOfFileBeIgnored(path))
            {
                string filePath = System.IO.Path.Combine(rootDir, path, uniqueName);
                bool isFileInsteadOfDirectory = false;
                if (!EfmuFilesystem.DoesFileOrDirectoryExist(filePath, ref isFileInsteadOfDirectory))
                {
                    EfmuConsoleWriter.WriteErrorLine($"The file listing entry {uniqueName} cannot be created, because the file/directory does not exist");
                    string tempFilePath = EfmuPathNames.ConvertRelativePathToUrlNotationWithPrefixAndRemoveIntermediateDot(filePath);
                    EfmuConsoleWriter.WriteErrorLineNoPrefix($" file path: {tempFilePath}");
                    success = false;
                }
            }

            if (success)
            {
                fileEntry = new EfmuFileListingEntry(uniqueName,
                    path,
                    needsChecksum,
                    checksum,
                    role,
                    kind);
            }

            return success;
        }

        /* To be called from tools reading xyzCode manifests, verify checksums. */
        public static bool CreateFileListingEntryFromXml(string rootDir,
                                                            string uniqueName,
                                                            string path,
                                                            bool needsChecksum,
                                                            string checksum, /* optional depending on needsChecksum */
                                                            EfmuFileEntryRole role,
                                                            string kind, /* optional, may be null */
                                                            bool validateChecksums,
                                                            ref EfmuFileListingEntry fileEntry)
        {
            bool success = true;

            if (needsChecksum)
            {
                if (null == checksum)
                {
                    EfmuConsoleWriter.WriteErrorLine($"Cannot create file listing entry for {uniqueName} because checksum is not given but required");
                    success = false;
                }
                else
                {
                    string filePath = System.IO.Path.Combine(rootDir, path, uniqueName);
                    /* TODO: If FMUFolder should be checksummed, we have to allow directories here! */
                    if (!EfmuFilesystem.DoesFileExist(filePath))
                    {
                        EfmuConsoleWriter.WriteErrorLine($"Cannot verify checksum for file listing entry {uniqueName}, because the file does not exist");
                        string tempFilePath = EfmuPathNames.ConvertRelativePathToUrlNotationWithPrefixAndRemoveIntermediateDot(filePath);
                        EfmuConsoleWriter.WriteErrorLineNoPrefix($" file path: {tempFilePath}");
                        success = false;
                    }

                    if (success)
                    {
                        string computedChecksum = EfmuChecksum.ComputeChecksumOfFile(filePath);
                        if (!checksum.Equals(computedChecksum))
                        {
                            string tempFilePath =
                                EfmuPathNames.ConvertRelativePathToUrlNotationWithPrefixAndRemoveIntermediateDot(
                                    filePath);
                            if (validateChecksums)
                            {
                                EfmuConsoleWriter.WriteErrorLine(
                                    $"The checksum of the file listing entry {uniqueName} is invalid");
                                EfmuConsoleWriter.WriteErrorLineNoPrefix($" file path: {tempFilePath}");
                                EfmuConsoleWriter.WriteErrorLineNoPrefix($" checksum from manifest: {checksum}");
                                EfmuConsoleWriter.WriteErrorLineNoPrefix($" checksum of file: {computedChecksum}");
                                success = false;
                            }
                            else
                            {
                                EfmuConsoleWriter.WriteWarningLine(
                                    $"Ignored invalid checksum of the file listing entry {uniqueName}");
                                EfmuConsoleWriter.WriteWarningLineNoPrefix($" file path: {tempFilePath}");
                                EfmuConsoleWriter.WriteWarningLineNoPrefix($" checksum from manifest: {checksum}");
                                EfmuConsoleWriter.WriteWarningLineNoPrefix($" checksum of file: {computedChecksum}");
                            }
                        }
                    }
                }
            }
            else
            {
                if (null != checksum)
                {
                    EfmuConsoleWriter.WriteErrorLine($"Cannot create file listing entry for {uniqueName} because checksum is given but not required");
                    success = false;
                }
                else if (!EfmuPathNames.CanExistanceOfFileBeIgnored(path))
                {
                    string filePath = System.IO.Path.Combine(rootDir, path, uniqueName);
                    bool isFileInsteadOfDirectory = false;
                    if (!EfmuFilesystem.DoesFileOrDirectoryExist(filePath, ref isFileInsteadOfDirectory))
                    {
                        EfmuConsoleWriter.WriteErrorLine($"The file listing entry {uniqueName} cannot be created, because the file does not exist");
                        string tempFilePath = EfmuPathNames.ConvertRelativePathToUrlNotationWithPrefixAndRemoveIntermediateDot(filePath);
                        EfmuConsoleWriter.WriteErrorLineNoPrefix($" file path: {tempFilePath}");
                        success = false;
                    }
                }
            }

            if (success)
            {
                fileEntry = new EfmuFileListingEntry(uniqueName,
                    path,
                    needsChecksum,
                    checksum,
                    role,
                    kind);
            }

            return success;
        }

        public static bool IsFmuFileOrFolder(EfmuFileEntryRole role)
        {
            return EfmuFileEntryRole.FMU == role
                   || EfmuFileEntryRole.FMUFolder == role;
        }


        /* The creator has to ensure that checksum and needsChecksum fit together! */
        private EfmuFileListingEntry(string uniqueName,
                                        string path,
                                        bool needsChecksum,
                                        string checksum, /* optional depending on needsChecksum */
                                        EfmuFileEntryRole role,
                                        string kind) /* optional, may be null */
        {
            this.Id = DefaultId;
            this.UniqueName = uniqueName;
            this.Path = path;
            this.NeedsChecksum = needsChecksum;
            this.Checksum = checksum;
            this.Role = role;
            this.OptionalKind = kind;
        }

        public void SetId(string id)
        {
            this.Id = id;
        }

        public string CombineNameAndPath()
        {
            //return Path + "/" + UniqueName;
            return System.IO.Path.Combine(Path, UniqueName);
        }

        public bool CheckAccordingToSchema()
        {
            bool success = true;

            /* (1) Path */

            /* TODO: path must start with "/" or "./"
             * => Path must not be empty
             */
            if (String.IsNullOrEmpty(Path))
            {
                EfmuConsoleWriter.WriteErrorLine("Path of FileListingEntry is empty:");
                Dump(EfmuConsoleWriter.WriteErrorLineNoPrefix);
                success = false;
            }
            else if (Path.StartsWith(EfmuPathNames.RelativeUrlPrefix))
            {
                /* ok */
            }
            else if (Path.StartsWith(EfmuPathNames.RootUrl))
            {
                EfmuConsoleWriter.WriteErrorLine($"Path of FileListingEntry starts with {EfmuPathNames.RootUrl} which is not supported yet");
                Dump(EfmuConsoleWriter.WriteErrorLineNoPrefix);
                success = false;
            }
            else
            {
                EfmuConsoleWriter.WriteWarningLine($"Path of FileListingEntry neither starts with {EfmuPathNames.RelativeUrlPrefix} or {EfmuPathNames.RootUrl}");
                EfmuConsoleWriter.WriteErrorLineNoPrefix("This will be forbidden in the near future");
                Dump(EfmuConsoleWriter.WriteErrorLineNoPrefix);
            }

            /* (2) UniqueName */

            /* TODO: directory must end with "/" ? */

            return success;
        }

        public void Dump(EfmuConsoleWriter.DelegateWriteLine handler)
        {
            //handler(Environment.NewLine + "> FileListingEntry");

            handler($"Id: {Id}");
            handler($"Name: {UniqueName}");
            handler($"Path: {Path}");
            handler($"NeedsChecksum: {NeedsChecksum}");
            handler($"Checksum: {Checksum}");
            handler($"Role: {Role}");
        }
    }
}
