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

using System.Collections.Generic;
using eFMI.Misc;

namespace eFMI.ManifestsAndContainers.ManifestFileListing
{
    public class EfmuManifestFileListing
    {
        private Dictionary<string, EfmuFileListingEntry> UniqueNamesToEntries;

        private EfmuFileListingEntry FmuFileEntry;


        public EfmuManifestFileListing()
        {
            this.UniqueNamesToEntries = new Dictionary<string, EfmuFileListingEntry>();
        }

        public bool AddFileEntry(EfmuFileListingEntry fileEntry)
        {
            bool success = true;

            if (UniqueNamesToEntries.ContainsKey(fileEntry.UniqueName))
            {
                /* safe */
                EfmuConsoleWriter.WriteErrorLine($"Inconsistent state: File with unique name {fileEntry.UniqueName} cannot be added multiple times");
                success = false;
            }
            else if ((null != FmuFileEntry) && (EfmuFileListingEntry.IsFmuFileOrFolder(fileEntry.Role)))
            {
                EfmuConsoleWriter.WriteErrorLine($"Inconsistent state: File/Directory with unique name {fileEntry.UniqueName} is FMU but file listing already contains FMU entry {FmuFileEntry.UniqueName}");
                success = false;
            }
            else
            {
                UniqueNamesToEntries[fileEntry.UniqueName] = fileEntry;
                if (EfmuFileListingEntry.IsFmuFileOrFolder(fileEntry.Role))
                {
                    FmuFileEntry = fileEntry;
                }
            }

            return success;
        }

        public bool GetFileEntryForUniqueName(string uniqueName,
                                              ref EfmuFileListingEntry fileEntry)
        {
            return UniqueNamesToEntries.TryGetValue(uniqueName, out fileEntry);
        }

        public bool HasFileEntryWithUniqueName(string uniqueName)
        {
            return UniqueNamesToEntries.ContainsKey(uniqueName);
        }

        public bool TryGetFmuEntry(out EfmuFileListingEntry fmuFileEntry)
        {
            bool success = true;

            if (null != FmuFileEntry)
            {
                fmuFileEntry = FmuFileEntry;
            }
            else
            {
                fmuFileEntry = null;
                success = false;
            }

            return success;
        }

        public bool GetFileListingEntries(ref IEnumerable<EfmuFileListingEntry> fileListingEntries)
        {
            bool success = true;

            fileListingEntries = UniqueNamesToEntries.Values;

            return success;
        }
    }
}
