//------------------------------------------------------------------------------
// <copyright file="Solution.cs" company="Zebedee Mason">
//     Copyright (c) 2016-2021 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    public class Solution
    {
        public static List<string> ReadVisualStudioSolution(Core.ILogger logger, string solutionPath)
        {
            List<string> projects = new List<string>();
            var direc = System.IO.Path.GetDirectoryName(solutionPath);
            foreach (var line in System.IO.File.ReadAllLines(solutionPath))
            {
                if (line.Length <= 10)
                {
                    continue;
                }

                if (line.Substring(0, 10) != "Project(\"{")
                {
                    continue;
                }

                var bits = line.Split(',');
                var filename = bits[1].Trim().Substring(1);
                filename = filename.Substring(0, filename.Length - 1);
                var ext = System.IO.Path.GetExtension(filename);
                if (ext == ".csproj" || ext == ".shproj" || ext == ".vbproj" || ext == ".vcxproj")
                {
                    var projectPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(direc, filename));
                    if (!System.IO.File.Exists(projectPath))
                    {
                        logger.Warn(string.Format("{0} contains non-existent project {1}", solutionPath, projectPath));
                        continue;
                    }

                    projects.Add(projectPath);
                    logger.Info("Appended for reading \"{0}\"", filename);
                }
            }

            return projects;
        }

        static public void SetSolutionDir(Core.Paths paths, string fileName, string direc)
        {
            if (paths.ContainsAlias("SolutionDir"))
                return;

            var path = direc.Length == 0 ? System.IO.Path.GetDirectoryName(fileName) : direc;
            var info = new System.IO.DirectoryInfo(path);
            paths.Add("SolutionDir", info.FullName);
        }

        static private void Files(Core.ILogger logger, Core.Paths paths, string fileName, string direc, List<KeyValuePair<string, string>> fileNames, List<string> extensions, List<string> projectExts)
        {
            var ext = System.IO.Path.GetExtension(fileName);
            if (projectExts.Contains(ext))
            {
                logger.Info("Appended for reading \"{0}\"", fileName);
                if (!extensions.Contains(ext))
                {
                    extensions.Add(ext);
                }

                fileNames.Add(new KeyValuePair<string, string>(fileName, direc));
                SetSolutionDir(paths, fileName, direc);
            }
            else if (ext == ".sln")
            {
                logger.Info("Reading projects from \"{0}\"", fileName);
                direc = System.IO.Path.GetDirectoryName(fileName);
                foreach (var name in ReadVisualStudioSolution(logger, fileName))
                {
                    Files(logger, paths, name, direc, fileNames, extensions, projectExts);
                }

                SetSolutionDir(paths, fileName, direc);
            }
        }

        public static Dictionary<string, Proj> ExtractProjects(Core.ILogger logger, Core.Paths paths, List<string> filePaths, string configPlatform = "Debug|AnyCPU")
        {
            var projects = new Dictionary<string, Proj>();
            var skipped = new List<string>();
            foreach (var filePath in filePaths)
            {
                var ext = System.IO.Path.GetExtension(filePath);

                if (ext == ".vcxproj")
                {
                    logger.Info("Appended for reading \"{0}\"", filePath);
                    logger.Info("Reading Visual C++");
                    var proj = new VCProj(logger, filePath, paths, configPlatform);
                    projects[filePath] = proj;
                    continue;
                }

                if (ext == ".csproj")
                {
                    var proj = new CSProj(logger, filePath, paths, configPlatform);
                    projects[filePath] = proj;
                    continue;
                }

                if (ext == ".shproj")
                {
                    var proj = new SHProj(logger, filePath, paths, configPlatform);
                    projects[filePath] = proj;
                    continue;
                }

                if (ext == ".vbproj")
                {
                    var proj = new VBProj(logger, filePath, paths, configPlatform);
                    projects[filePath] = proj;
                    continue;
                }

                skipped.Add(filePath);
            }

            filePaths.Clear();
            filePaths.AddRange(skipped);

            return projects;
        }

        private static void ExtractProjects(Core.ILogger logger, Core.Paths paths, List<string> filePaths, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters, string configPlatform)
        {
            var dependencies = new Dictionary<Core.Project, List<string>>();
            var mapping = new Dictionary<string, string>();
            foreach (var proj in ExtractProjects(logger, paths, filePaths, configPlatform))
            {
                if (proj.Value is VCProj vcProj)
                {
                    projects[proj.Value.Name] = vcProj.Extract(logger, paths, proj.Key, filters, dependencies, mapping);
                }
                else if (proj.Value is NetProj netProj)
                {
                    projects[proj.Value.Name] = netProj.Extract(logger, paths, proj.Key, dependencies, mapping);
                }
            }

            foreach (var dep in dependencies)
            {
                var proj = dep.Key;
                foreach (var filePath in dep.Value)
                {
                    if (mapping.ContainsKey(filePath))
                    {
                        proj.Dependencies.Add(mapping[filePath]);
                    }
                }
            }
        }

        private static void Extract(Core.ILogger logger, Core.Paths paths, string solutionPath, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters, string configPlatform)
        {
            logger.Info("Reading projects from \"{0}\"", solutionPath);
            var direc = System.IO.Path.GetDirectoryName(solutionPath);
            SetSolutionDir(paths, solutionPath, direc);
            var filePaths = ReadVisualStudioSolution(logger, solutionPath);
            ExtractProjects(logger, paths, filePaths, projects, filters, configPlatform);
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, List<string> filePaths, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters, string configPlatform = "Debug|AnyCPU")
        {
            var skipped = new List<string>();
            foreach (var filePath in filePaths)
            {
                var ext = System.IO.Path.GetExtension(filePath);
                if (ext == ".sln")
                {
                    Extract(logger, paths, filePath, projects, filters, configPlatform);
                    continue;
                }

                skipped.Add(filePath);
            }

            filePaths.Clear();
            filePaths.AddRange(skipped);


            if (filePaths.Count > 0)
            {
                SetSolutionDir(paths, filePaths[0], string.Empty);
            }

            ExtractProjects(logger, paths, filePaths, projects, filters, configPlatform);
            paths.Remove("$(ProjectDir)");
        }
    }
}
