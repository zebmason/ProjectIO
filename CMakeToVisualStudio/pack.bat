"C:\Program Files (x86)\NuGet\nuget.exe" pack CMakeToVisualStudio.csproj -IncludeReferencedProjects -properties Configuration=Release
echo Use 7-Zip to rename lib\net472 to tools\any
echo Use 7-Zip to edit the nuspec to remove all mentions of files and dependencies
echo Install-Package ...\CMakeToVisualStudio\ProjectIO.CMakeToVisualStudio.2.0.0.nupkg
pause -1