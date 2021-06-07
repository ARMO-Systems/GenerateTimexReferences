@echo On
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

"%nuget%" restore Source\Main.sln -NoCache -NonInteractive
"C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe" Source\Main.sln /p:Configuration="%config%" /p:Platform="Any CPU" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false


cd Packs

set firstPartNuget="%nuget%" pack 
set lastPartNuget= -NonInteractive %version%

cmd /c %firstPartNuget%GenerateTimexReferences.nuspec%lastPartNuget%