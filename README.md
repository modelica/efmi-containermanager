# Repository overview

This repository provides the source code and releases of the _eFMI Container Manager_, a tool for creating, checking, reading and modifying eFMUs and their individual containers, like Behavioral Model, Algorithm Code and Production Code containers (cf. the [eFMI standard](https://efmi-standard.org) for details).

The eFMI Container Manager can be used to create new eFMUs, add additional containers to existing eFMUs or check containers for their consistency. The latter includes checks that container content is properly listed, checksums are correct, manifests satisfy their respective container-type XML Schema, trace links between containers are not broken etc.

The eFMI Container Manager can work on zipped and unzipped eFMUs. It can also be used to transform an eFMU to a valid FMU if the eFMU provides production code for an [FMI](https://fmi-standard.org/) platform (e.g., Windows/Linux x64/32 desktop machine).

## User interface

The eFMI Container Manager is a command line tool. It requires an installed [Microsoft .NET 5.0](https://dotnet.microsoft.com/) runtime environment.

A typical user session is for example

```
Add example
```

In above example, first ...TODO.

For usage details, please just call the eFMI Container Manager with command line argument `help`, i.e.,

```
Add example
```



## Contributing, security and repository policies

Please consult the [contributing guidelines](CONTRIBUTING.md) for details on how to report issues and contribute to the repository.

For security issues, please consult the [security guidelines](SECURITY.md).

General MAP eFMI repository setup and configuration policies are summarized in the [MAP eFMI repository policies](https://github.com/modelica/efmi-organization/wiki/Repositories#public-repository-policies) (only relevant for repository administrators and therefor private webpage).