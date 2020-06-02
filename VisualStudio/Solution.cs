//------------------------------------------------------------------------------
// <copyright file="Solution.cs" company="Zebedee Mason">
//     Copyright (c) 2016-2017 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    public class Solution
    {
        private static List<string> ReadVisualStudioSolution(Core.ILogger logger, string slnfilename)
        {
            List<string> projects = new List<string>();
            var direc = System.IO.Path.GetDirectoryName(slnfilename);
            foreach (var line in System.IO.File.ReadAllLines(slnfilename))
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
                    projects.Add(System.IO.Path.Combine(direc, filename));
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

        private static void ExtractProjects(Core.ILogger logger, Core.Paths paths, List<string> filePaths, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters)
        {
            var skipped = new List<string>();
            foreach (var filePath in filePaths)
            {
                var ext = System.IO.Path.GetExtension(filePath);

                if (ext == ".vcxproj")
                {
                    logger.Info("Appended for reading \"{0}\"", filePath);
                    logger.Info("Reading Visual C++");
                    VCProj.Extract(logger, paths, filePath, projects, filters);
                    continue;
                }

                if (ext == ".csproj")
                {
                    CSProj.Extract(logger, paths, filePath,  projects);
                    continue;
                }

                if (ext == ".shproj")
                {
                    SHProj.Extract(logger, paths, filePath, projects);
                    continue;
                }

                if (ext == ".vbproj")
                {
                    VBProj.Extract(logger, paths, filePath, projects);
                    continue;
                }

                skipped.Add(filePath);
            }

            filePaths.Clear();
            filePaths.AddRange(skipped);
        }

        private static void Extract(Core.ILogger logger, Core.Paths paths, string solutionPath, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters)
        {
            logger.Info("Reading projects from \"{0}\"", solutionPath);
            var direc = System.IO.Path.GetDirectoryName(solutionPath);
            SetSolutionDir(paths, solutionPath, direc);
            var filePaths = ReadVisualStudioSolution(logger, solutionPath);
            ExtractProjects(logger, paths, filePaths, projects, filters);
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, List<string> filePaths, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters)
        {
            var skipped = new List<string>();
            foreach (var filePath in filePaths)
            {
                var ext = System.IO.Path.GetExtension(filePath);
                if (ext == ".sln")
                {
                    Extract(logger, paths, filePath, projects, filters);
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

            ExtractProjects(logger, paths, filePaths, projects, filters);
            paths.Remove("$(ProjectDir)");
        }
    }
}
