SET TARGET=RSPO.exe
SET VAR=Debug
SET RUNNERVER=2.3.1
SET RUNTIME=net452

cd ..
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" 
cd RSPO

"packages\xunit.runner.console\tools\%RUNTIME%\xunit.console.exe" bin\%VAR%\%TARGET% -nologo -nocolor
