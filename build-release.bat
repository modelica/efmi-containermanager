@echo off
setlocal

rem Clear any user set ERRORLEVEL variable:
set "ERRORLEVEL="

rem Enable advanced batch file commands:
verify argument_to_enforce_error 2>nul
setlocal EnableExtensions
if ERRORLEVEL 1 (
	echo=SCRIPT ABORTED: Command extensions not supported.
	exit /b 1
)
verify argument_to_enforce_error 2>nul
setlocal EnableDelayedExpansion
if ERRORLEVEL 1 (
	endlocal rem Undo "setlocal EnableExtensions"
	echo=SCRIPT ABORTED: Delayed expansion not supported.
	exit /b 1
)

rem Configure "exit" to terminate subroutines and entire script:
if not "%SelfWrapped%"=="%~0" (
	set "SelfWrapped=%~0"
	%ComSpec% /s /c ""%~0" %*"
	endlocal rem Undo "setlocal EnableExtensions"
	endlocal rem Undo "setlocal EnableDelayedExpansion"
	goto :eof
)

set "SCRIPTPATH=%~dp0"
set "SCRIPTPATH=%SCRIPTPATH:~0,-1%"

rem Initialize Microsoft build environment:
set "arch=x86"
call where msbuild >nul 2>nul
if ERRORLEVEL 1 (
	for %%y in ( "2019" "2017" ) do (
		for %%v in ( "Enterprise" "Professional" "Community" "BuildTools" ) do (
			if exist "C:\Program Files (x86)\Microsoft Visual Studio\%%y\%%v\VC\Auxiliary\Build\vcvarsall.bat" (
				call "C:\Program Files (x86)\Microsoft Visual Studio\%%y\%%v\VC\Auxiliary\Build\vcvarsall.bat" ^
					%arch%
				if ERRORLEVEL 1 ( rem Not working: 'vcvarsall.bat' never returns error code.
					set "EMESSAGE=Microsoft build environment initialization failed (Microsoft Visual Studio %%y)"
					call :ERROR
				)
				goto :VISUAL_STUDIO_INITIALIZED
			)
		)
	)
	for %%v in ( "14.0" "12.0" "11.0" "10.0" ) do (
		if exist "C:\Program Files (x86)\Microsoft Visual Studio %%v\VC\vcvarsall.bat" (
			call "C:\Program Files (x86)\Microsoft Visual Studio %%v\VC\vcvarsall.bat" ^
				%arch%
			if ERRORLEVEL 1 (
				set "EMESSAGE=Microsoft build environment initialization failed (Microsoft Visual Studio v%%v)"
				call :ERROR
			)
			goto :VISUAL_STUDIO_INITIALIZED
		)
	)
	set "EMESSAGE=No Microsoft Visual Studio installation found"
	call :ERROR
)
:VISUAL_STUDIO_INITIALIZED

rem Cleanup old builds:
rmdir /S /Q "%SCRIPTPATH%/eFMUContainerManager.CLI/bin"
rmdir /S /Q "%SCRIPTPATH%/eFMUContainerManager.CLI/obj"
rmdir /S /Q "%SCRIPTPATH%/eFMUContainerManager.Core/bin"
rmdir /S /Q "%SCRIPTPATH%/eFMUContainerManager.Core/obj"
rmdir /S /Q "%SCRIPTPATH%/eFMUContainerManager.Core.Interfaces/bin"
rmdir /S /Q "%SCRIPTPATH%/eFMUContainerManager.Core.Interfaces/obj"
rmdir /S /Q "%SCRIPTPATH%/eFMUManifestsAndContainers/bin"
rmdir /S /Q "%SCRIPTPATH%/eFMUManifestsAndContainers/obj"
rmdir /S /Q "%SCRIPTPATH%/eFMUMisc/bin"
rmdir /S /Q "%SCRIPTPATH%/eFMUMisc/obj"

rem Build new release:
msbuild ^
	"%SCRIPTPATH%/eFMUContainerManager.sln" ^
	"/property:Configuration=Release" ^
	"/property:Platform=Any CPU"
if ERRORLEVEL 1 (
	set "EMESSAGE=Build failed with errors"
	call :ERROR
)
exit %ERRORLEVEL%

rem ********************************************************************************************************** Support functions:

rem Print error message and exit:
:ERROR
echo=
echo=ERROR: %EMESSAGE%.
echo=
exit 1
