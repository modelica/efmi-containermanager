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
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace eFMI.Misc
{
    public class EfmuChecksum
    {
        public static string ComputeChecksumOfFile(string filePath,
                                                   string optionalHashAlgorithm = "SHA1")
        {
            /* Hash must be created once for each hash computation! */
            using (var hashAlg = HashAlgorithm.Create(optionalHashAlgorithm))
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = hashAlg.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /* NOTE: The checksum of a subtree is consistent over several calls of this method by definition.
         * If you observe different checksums for subtrees which are equal at the first glance
         * but the checksum validation does not fail, please remember that BeyondCompare
         * may ignore the first lines of the manifest XML files with the GUIDs ...
         * The GUIDs 
         */

        public static string ComputeChecksumOfDirectory(string dirPath,
                                                        string optionalHashAlgorithm = "SHA1")
        {
            EfmuConsoleWriter.WriteDebugLine($"> ComputeChecksumOfDirectory: {dirPath}");
            string[] entries = Directory.GetFileSystemEntries(dirPath, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();
            int dirPathLength = dirPath.Length;

            using (var hashAlg = HashAlgorithm.Create(optionalHashAlgorithm))
            {
                foreach (string entry in entries)
                {
                    EfmuConsoleWriter.WriteDebugLine($" {entry}");

                    string relEntryPath = entry.Remove(0, dirPathLength + 1);

                    /* Attention: The entry values are output paths!
                     * If a path string is intended to be used as input of the hash algorithm,
                     * it must be converted to either a relative path within the container
                     * or the directory/file name!
                     */
                    if (File.GetAttributes(entry).HasFlag(FileAttributes.Directory))
                    {
                        //ConsoleWriter.WriteDebugLine(" => directory: " + Path.GetFileName(entry));
                        EfmuConsoleWriter.WriteDebugLine($" => directory: {relEntryPath}");

                        /* hash directory path */
                        byte[] pathBytes = Encoding.UTF8.GetBytes(relEntryPath);
                        hashAlg.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);
                    }
                    else
                    {
                        //ConsoleWriter.WriteDebugLine(" => file: " + Path.GetFileName(entry));
                        EfmuConsoleWriter.WriteDebugLine($" => file: {relEntryPath}");

                        /* hash file path */
                        byte[] pathBytes = Encoding.UTF8.GetBytes(relEntryPath);
                        hashAlg.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                        /* hash file content */
                        byte[] contentBytes = File.ReadAllBytes(entry);
                        hashAlg.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                    }
                }

                /* Handles empty filePaths case */
                hashAlg.TransformFinalBlock(new byte[0], 0, 0);

                return BitConverter.ToString(hashAlg.Hash).Replace("-", "").ToLower();
            }
        }
    }
}
