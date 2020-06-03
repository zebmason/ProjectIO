"C:\Program Files (x86)\NuGet\nuget.exe" pack CMakeToVisualStudio.csproj -IncludeReferencedProjects -properties Configuration=Release
echo Install-Package ...\CMakeToVisualStudio\ProjectIO.CMakeToVisualStudio.2.0.0.nupkg
pause -1