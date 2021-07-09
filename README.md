# Repository overview

This repository provides the source code of the _eFMI Container Manager_, a tool for creating, checking, reading and modifying eFMUs and their individual containers, like Behavioral Model, Algorithm Code and Production Code containers (cf. the [eFMI standard](https://efmi-standard.org) for details).

The eFMI Container Manager can be used to create new eFMUs, add additional containers to existing eFMUs or check containers for their consistency. The latter includes checks that container content is properly listed, checksums are correct, manifests satisfy their respective container-type XML Schema, trace links between containers are not broken etc.

The eFMI Container Manager works on zipped eFMUs; of course, users can just unzip and repack such. It can also be used to transform an eFMU to a valid FMU if the eFMU provides production code for an [FMI](https://fmi-standard.org/) platform (e.g., Windows/Linux x64/32 desktop machine).

## Dependencies

The _eFMI Container Manager_ is a [.NET](https://dotnet.microsoft.com/) application; to use it, an installed [Microsoft .NET 4.8 Framework](https://dotnet.microsoft.com/) runtime environment is required. It also uses the [Command Line Parser 2.8.0](https://github.com/commandlineparser/commandline) package, which is available via the [NuGet](https://www.nuget.org) packet manager.

It is developed in [C#](https://en.wikipedia.org/wiki/C_Sharp_(programming_language)) using [Microsoft Visual Studio 2019](https://visualstudio.microsoft.com/) as development environment.

## User interface

### General usage characteristics: eFMUs, containers and integrity checks 

In the eFMI specification varying content-types for eFMUs are formally defined, so called model representations, like the Algorithm Code, Production Code or Behavioral Model model representations. A single eFMU can contain several instances of the same and varying model representations as content. For brevity, we simply call each such instance just a container (of the eFMU) in the following.

For most _eFMI Container Manager_ operations an existing eFMU to operate on is required. The eFMU operated on must be zipped. The _eFMI Container Manager_ unzips the eFMU into a temporary directory, validates its content, performs the desired operation, validates the result and packs it back. In that process, the checksums given in manifests are only changed when the content of the referenced file (ignoring file attributes/meta-information) is changed by the requested operation.

In case of any eFMI violations -- i.e., the given zip file is not a correct eFMU or the result of the operation would violate its integrity -- the operation is aborted and the original zip is left unchanged. Thus, eFMUs are only changed if the operation requested can be successfully executed yielding a valid eFMU. The checks performed include checks that the eFMU manifest (`eFMU/__content.xml` in the root of the eFMU) and all container manifests (`manifest.xml` at the root of the respective container) satisfy the eFMI XML Schema, that content listed in the eFMU manifest exists, that references within and in between manifests can be traced (i.e., the referenced thing exists) and that checksums are correct. If any check fails, respective error messages are given and the requested operation is aborted.

### Command line interface

The _eFMI Container Manager_ is a command line tool. Besides the eFMU, most _eFMI Container Manager_ operations work on a specific container of the eFMU, for example to add a new container, delete an existing container or update an existing container and its content. Each such operation corresponds to a single _eFMI Container Manager_ command line invocation stating the requested operation and eFMU and container to work on. The eFMU is given via the `--eFMU` parameter; the container is given via the `--name` parameter; the operation on it is given via respective operation options (e.g., `--add`, `--replace`, `--delete` or `--extract`). If the operation requires external content (like what to add), such is given in terms of a source directory providing the content and referred to via the `--inputdir` parameter; the manifest of that source has to be denoted via the `--manifest` parameter (in other words, the source directory is treated as a source _container_, emphasizing that it has to be a valid eFMI model representation). If the operation extracts eFMU content, the destination directory where to store the extracted content must be specified via the `--output` parameter.

Most command line options and parameters also provide an abbreviated, single letter alternative for their long name; e.g., `-a` for `--add`  or `-I` for `--inputdir`. Actual operations use lower-case letters whereas the further needed parameters of such -- and which are shared between different operations -- use uppercase letters.

An overview of all command line options can be obtained by calling the _eFMI Container Manager_ with `--help` as argument.

### Example 1: Creating a new eFMU and adding a container

The following example demonstrates how to create an eFMU with a single container. In order to facilitate readability, the long versions of the command line options are used and the single line commands are broken over successive lines using `\` as line continuation.

To create a new eFMU with file name `my-eFMU.fmu` the command line call is:

```
eFMUContainerManager.CLI.exe \
	--create \
	--name "Example eFMU created using the eFMI Container Manager" \
	--inputdir directory/with/eFMI/schemas \
	--output my-eFMU.fmu
```

The new eFMU `my-eFMU.fmu` is empty, without any containers. Its XML Schema files are copied from the `directory/with/eFMI/schemas` directory. If the `--force` option would have been set additionally, any already existing `my-eFMU.fmu` file would have been overwritten.

The following adds a new container named `MyContainer` taken from the source directory `path/to/source-container` with manifest file `manifest.xml` at its root to `my-eFMU.fmu`:

```
eFMUContainerManager.CLI.exe \
	--eFMU my-eFMU.fmu
	--add \
	--inputdir path/to/source-container \
	--manifest manifest.xml \
	--name MyContainer
```

Note, that the manifest of the source container defines which kind of container it is (e.g., Algorithm Code, Production Code or Behavioral Model container). The source's manifest is validated against the XML Schemas of `my-eFMU.fmu`; only if the validation passes, the new container is added. `my-eFMU.fmu` also must not already contain a container named `MyContainer`. If it would, such could be replaced (i.e., completely overwritten) using the `--replace` operation instead of `--add`.

### Example 2: Transforming an eFMU into a FMU

If any Production Code container of an eFMU provides an optional FMU, that FMU can be unpacked to the root of the eFMU transforming it into a valid FMU. The resulting FMU is still a valid eFMU as well, as long as its `eFMU` directory is not deleted.

The command to transform an eFMU into a FMU is:

```
eFMUContainerManager.CLI.exe \
	--eFMU my-eFMU.fmu \
	--unpackfmu \
	--name PCC
```

`PCC` must be a Production Code container providing an FMU. All content at the root of `my-eFMU.fmu` besides its `eFMU` directory is deleted (`--tidyroot` operation) and the content of the FMU provided by `PCC` is unpacked into the root of `my-eFMU.fmu`. The manifest of `PCC` is also updated to denote that its FMU is active. Any further `--delete` operation targetting `PCC` while its FMU is still active implicitly also performs `--tidyroot`.

## Contributing, security and repository policies

Please consult the [contributing guidelines](CONTRIBUTING.md) for details on how to report issues and contribute to the repository.

For security issues, please consult the [security guidelines](SECURITY.md).

General MAP eFMI repository setup and configuration policies are summarized in the [MAP eFMI repository policies](https://github.com/modelica/efmi-organization/wiki/Repositories#public-repository-policies) (only relevant for repository administrators and therefor private webpage).
