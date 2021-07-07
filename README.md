# Repository overview

This repository provides the source code of the _eFMI Container Manager_, a tool for creating, checking, reading and modifying eFMUs and their individual containers, like Behavioral Model, Algorithm Code and Production Code containers (cf. the [eFMI standard](https://efmi-standard.org) for details).

The eFMI Container Manager can be used to create new eFMUs, add additional containers to existing eFMUs or check containers for their consistency. The latter includes checks that container content is properly listed, checksums are correct, manifests satisfy their respective container-type XML Schema, trace links between containers are not broken etc.

The eFMI Container Manager works on zipped eFMUs. Working with unzipped eFMUs means to perform zip/unzip operations before and after using the eFMI Container Manager. It can also be used to transform an eFMU to a valid FMU if the eFMU provides production code for an [FMI](https://fmi-standard.org/) platform (e.g., Windows/Linux x64/32 desktop machine).

## Dependencies

The eFMI Container Manager is a C# application and uses [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/) as compiler, [Microsoft .NET 4.8 Framework](https://dotnet.microsoft.com/) SDK as well as [Command Line Parser 2.8.0](https://github.com/commandlineparser/commandline) nuget package, which is available via https://www.nuget.org.

## User interface

The eFMI Container Manager is a command line tool. It requires an installed [Microsoft .NET 4.8 Framework](https://dotnet.microsoft.com/) runtime environment.

For simplification, one invocation of the eFMI Container Manager corresponds to one basic operation. Hence, to create an eFMU container with N different model representations, N+1 calls of the eFMI Container Manager are required: The first call creates the container. The remaining N calls add the model representations to the container.

Basic operations which demand an existing container, i.e. add/replace, read the container eFMU file before performing the operation.

This includes:
- unzip the container to a temporary directory
- read the manifest __content.xml
  - load and validate it
  - extract the data including the description of the model representations
- validate each model representation
  - validate the checksum
  - load and validate the manifest file

Basic operations which imply writing a container, i.e. create/add, write the container eFMU file after performing the operation.
This includes:
- creating a new __content.xml and validate it
- zipping the entire container from the temporary directory back to the container eFMU file

Checksums of model representations are only created/updated when adding/replacing them.

An overview of the command line interface can be obtained by calling the eFMI Container Manager with "--help" parameter.

The command line options for BASIC OPERATIONS use lowercase letters. The "--tidyroot" operation is the only exception of that rule.

Each basic operations has several NEEDED/OPTIONAL PARAMETERS. The command line options for these parameters use UPPERCASE letters.

The following example demonstrates how to create an eFMU container with a single model representation of kind "ProductionCode".
In order to facility the readability, the long versions of the command line options have been used:

```
eFMUContainerManager.CLI.exe --create --name "eFMU Container" --inputdir D:\eFMI\SCHEMAS_ROOT --output container.fmu
```

This command creates an eFMU container without any model representations. The schema files for the "eFMU/schemas" directory are copied from the given directory. An additional option "--force" would demand to overwrite an existing container.fmu file.

```
eFMUContainerManager.CLI.exe --add --name MyProdCodeRep --manifest manifest.xml --eFMU container.fmu --inputdir "ModelDir"
```

This command adds a model representation to the container created in step before.
The add operation gets three parameters:
- the input directory where to read the model representation from
- the name of the model representation: MyProdCodeRep
- the name of the manifest file: manifest.xml (must be located in the root of the input directory)
The source directory must contain the given manifest file. It is validated against the linked schema.
The kind of the manifest file determines the kind of the model representation.

The container must not contain a model representation of the given name.
In order to overwrite an existing model representation, the "replace" operation has to be invoked.

If a model representation has an optional FMU, it can be unpacked to the root of the container in order to simulate an FMU container.

```
eFMUContainerManager.CLI.exe --unpackfmu --name MyProdCodeRep --eFMU container.fmu
```

Currently, this operation is only supported for model representations with kind "ProductionCode".
Unpacking an FMU is performed in two steps:
- removing all non-eFMU entities from the container root (i.e. all except the "eFMU" directory)
- unpacking the FMU => FMU becomes "active"

The first step can also be performed explicitly (--tidyroot).
It is performed implicitly if a model representation is removed from the container and its FMU is currently active.



## Contributing, security and repository policies

Please consult the [contributing guidelines](CONTRIBUTING.md) for details on how to report issues and contribute to the repository.

For security issues, please consult the [security guidelines](SECURITY.md).

General MAP eFMI repository setup and configuration policies are summarized in the [MAP eFMI repository policies](https://github.com/modelica/efmi-organization/wiki/Repositories#public-repository-policies) (only relevant for repository administrators and therefor private webpage).