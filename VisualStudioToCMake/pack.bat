"C:\Program Files (x86)\NuGet\nuget.exe" pack VisualStudioToCMake.csproj -IncludeReferencedProjects -properties Configuration=Release
echo Install-Package ...\VisualStudioToCMake\ProjectIO.VisualStudioToCMake.2.0.0.nupkg
pause -1