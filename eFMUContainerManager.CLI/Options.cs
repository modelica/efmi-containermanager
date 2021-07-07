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
using CommandLine;

namespace eFMI.ContainerManager
{
	class Options : ICloneable
	{
	    [Option('c', "create", Required = false, Default = false, HelpText = "Create new container including manifest file and schemas directory but without any model representations. Without -O: container.fmu. # Needed: -I <PathToSchemaFiles> -N / Optional: -O, -F")]
	    public bool CreateContainer { get; set; }

	    [Option('a', "add", Required = false, Default = false, HelpText = "Add model representation to container. # Needed: -E, -N, -I <PathToFilesToBeAdded>, -M")]
	    public bool AddToContainer { get; set; }

	    [Option('r', "replace", Required = false, Default = false, HelpText = "Replace model representation with given name in container. # Needed: -E, -N, -I <PathToFilesToBeAdded>, -M")]
	    public bool ReplaceInContainer { get; set; }

	    [Option('d', "delete", Required = false, Default = false, HelpText = "Delete model representation with given name from container.# Needed: -E, -N")]
	    public bool DeleteFromContainer { get; set; }

	    [Option('e', "extract", Required = false, Default = false, HelpText = "Extract model representation with given name to a directory. The container is not changed by this operation. # Needed: -E, -N, -O / Optional: -F")]
	    public bool ExtractFromContainer { get; set; }

        [Option("extractschemas", Required = false, Default = false, HelpText = "Extract schemas to a directory. The container is not changed by this operation. # Needed: -E, -O / Optional: -F")]
        public bool ExtractSchemasFromContainer { get; set; }

		[Option('u', "unpackfmu", Required = false, Default = false, HelpText = "Unpack FMU of given production code model representation to root directory of the container. Implies 'tidyroot'. # Needed: -E, -N")]
	    public bool UnpackFmu { get; set; }

	    [Option("tidyroot", Required = false, Default = false, HelpText = "Tidies the root directory of the container, i.e. removes all files/directories except the eFMU directory. # Needed: -E")]
	    public bool TidyRoot { get; set; }

	    [Option('l', "list", Required = false, Default = false, HelpText = "List the content of the container including all model representations. # Needed: -E")]
	    public bool ListContainerContent { get; set; }

	    [Option('E', "efmu", Required = false, HelpText = "Path of .fmu file (EFMU container with eFMU directory!) or directory for container manipulation/query.")]
	    public string ContainerFilePath { get; set; }

	    [Option('I', "inputdir", Required = false, HelpText = "Directory with input data needed by all operations adding/updating model representations / schema files.")]
	    public string InputDir { get; set; }

	    [Option('N', "name", Required = false, HelpText = "Name of container in __content.xml (-c only) or model representation (other options demanding name).")]
	    public string Name { get; set; }

	    [Option('M', "manifest", Required = false, HelpText = "Name of manifest XML file within input directory. (The manifest file of a model representation must be located in the root directory)")]
	    public string ManifestFileName { get; set; }

	    [Option('O', "output", Required = false, HelpText = "Path of .fmu file (-c only) or directory (-e only).")]
	    public string OutputPath { get; set; }

	    [Option('F', "force", Required = false, Default = false, HelpText = "ForceOverwriting overwriting of output directory/file.")]
	    public bool ForceOverwriting { get; set; }

	    [Option("noxmlvalidation", Required = false, Default = false, Hidden = true, HelpText = "Disables validation of read/written manifest files (For debugging purposes only!!!).")]
	    public bool NoXmlValidation { get; set; }

	    [Option("ignorechecksums", Required = false, Default = false, Hidden = true, HelpText = "Ignores invalid checksums. Only a warning is produced. (For debugging purposes only!!!).")]
	    public bool IgnoreChecksums { get; set; }


        [Option('v', "verbose", Required = false, Default = false, Hidden = true, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }

		public object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
