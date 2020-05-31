//------------------------------------------------------------------------------
// <copyright file="ProjectPath.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    public class ProjectPath
    {
        private readonly string solutionDirectory;

        private readonly string projectDirectory;

        public string FilePath { get; }

        public ProjectPath(string filePath, string solutionDirectory)
        {
            projectDirectory = System.IO.Path.GetDirectoryName(filePath);
            FilePath = filePath;
            this.solutionDirectory = solutionDirectory;
        }

        public string Path(string link)
        {
            if (link.Length > 2 && link.Substring(0, 2) == "..")
            {
                link = System.IO.Path.Combine(projectDirectory, link);
                link = System.IO.Path.GetFullPath(link);
            }

            return link;
        }

        public static string Combine(string direc, string filePath)
        {
            if (filePath.Contains("$"))
            {
                var env = System.Environment.GetEnvironmentVariables();
                foreach (var e in env.Keys)
                {
                    filePath = filePath.Replace("$(" + e as string + ")", env[e] as string);
                }
            }

            if (!filePath.Contains(":"))
            {
                filePath = System.IO.Path.Combine(direc, filePath);
            }

            filePath = filePath.Replace("/", "\\");
            filePath = System.IO.Path.GetFullPath(filePath);
            return filePath;
        }

        public string Combine(string filePath)
        {
            filePath = filePath.Replace("$(ProjectDir)", projectDirectory + "\\");
            if (solutionDirectory.Length > 0)
            {
                filePath = filePath.Replace("$(SolutionDir)", solutionDirectory + "\\");
            }

            return Combine(projectDirectory, filePath);
        }
    }
}
