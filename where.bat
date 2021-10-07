@rem Copyright (c) 2021, dSPACE GmbH, Modelica Association and contributors
@rem 
@rem Licensed under the 3-Clause BSD license (the "License");
@rem you may not use this software except in compliance with
@rem the "License".
@rem 
@rem This software is not fully developed or tested.
@rem 
@rem THE SOFTWARE IS PROVIDED "as is", WITHOUT ANY WARRANTY
@rem of any kind, either express or implied, and the use is 
@rem completely at your own risk.
@rem 
@rem The software can be redistributed and/or modified under
@rem the terms of the "License".
@rem 
@rem See the "License" for the specific language governing
@rem permissions and limitations under the "License".

@rem Find given executable. Echos all absolut paths where it can be found.
@rem Echos nothing and returns with an error code if not found.

@echo off

set "found=1"
for %%e in (%PATHEXT%) do (
	for %%i in (%1%%e) do (
		if not "%%~$PATH:i"=="" (
			echo=%%~$PATH:i
			set "found=0"
		)
	)
)
exit /b %found%
