//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudioToCMake
{
    using System.Collections.Generic;

    public class Program
    {
        public static void MainFunc(string[] args, Core.ILogger logger)
        {
            logger.Info("Visual Studio solution command line application for creating CMake files");
            logger.Info("Copyright (c) 2020 Zebedee Mason");

            bool printUsage = false;
            if (args.Length == 2 || args.Length == 3)
            {
                if (!System.IO.File.Exists(args[0]))
                {
                    logger.Info("First argument is not an existing file");
                    printUsage = true;
                }
                else if (System.IO.Path.GetExtension(args[0]) != ".sln")
                {
                    logger.Info("First argument is not an sln file");
                    printUsage = true;
                }

                if (args.Length == 3 && !System.IO.Directory.Exists(args[2]))
                {
                    logger.Info("Third argument is not an existing directory");
                    printUsage = true;
                }
            }
            else
                printUsage = true;

            if (printUsage)
            {
                logger.Info("Create CMakeLists.txt files");
                logger.Info("Usage:");
                logger.Info("  ProjectIO.VisualStudioToCMake.exe <Visual Studio solution> <configuration|platform> [root directory if different from sln file path]");
                return;
            }

            var filePaths = new List<string> { args[0] };
            var projects = new Dictionary<string, Core.Project>();
            var filters = new Dictionary<string, string>();
            var paths = new Core.Paths();
            VisualStudio.Solution.Extract(logger, paths, filePaths, projects, filters, args[1]);

            var rootDirec = System.IO.Path.GetDirectoryName(args[0]);
            if (args.Length == 3)
            {
                rootDirec = args[2];
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(args[0]);

            CMakeProject.Assemble(rootDirec, name, projects);
        }

        static void Main(string[] args)
        {
            MainFunc(args, new Core.PlainConsoleLogger());
        }
    }
}
