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
        private static List<string> ReadVisualStudioSolution(string slnfilename)
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
                }
            }

            return projects;
        }

        static private void SetSolutionDir(Core.Paths paths, string fileName, string direc)
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
                logger.Info("Appended for reading {}", fileName);
                if (!extensions.Contains(ext))
                {
                    extensions.Add(ext);
                }

                fileNames.Add(new KeyValuePair<string, string>(fileName, direc));
                Solution.SetSolutionDir(paths, fileName, direc);
            }
            else if (ext == ".sln")
            {
                logger.Info("Reading projects from {}", fileName);
                direc = System.IO.Path.GetDirectoryName(fileName);
                foreach (var name in VisualStudio.Solution.ReadVisualStudioSolution(fileName))
                {
                    Solution.Files(logger, paths, name, direc, fileNames, extensions, projectExts);
                }

                Solution.SetSolutionDir(paths, fileName, direc);
            }
        }

        static public List<KeyValuePair<string, string>> ExpandFileList(Core.ILogger logger, Core.Paths paths, string[] lines, string[] projectExts)
        {
            logger.Info("Creating file list");
            var fileNames = new List<KeyValuePair<string, string>>();
            var extensions = new List<string>();
            var projectExtensions = new List<string>(projectExts);
            foreach (var fileName in lines)
            {
                Solution.Files(logger, paths, fileName, string.Empty, fileNames, extensions, projectExtensions);
            }

            return fileNames;
        }
    }
}
