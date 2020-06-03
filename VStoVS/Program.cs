//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VStoVS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        public static void MainFunc(string[] args, Core.ILogger logger)
        {
            logger.Info("VStoVS command line application for creating a Visual Studio solution from a Visual Studio solution!");
            logger.Info("Copyright (c) 2020 Zebedee Mason");

            bool printUsage = false;
            if (args.Length > 3)
            {
                if (!System.IO.Directory.Exists(args[0]))
                {
                    logger.Info("First argument is not an output directory");
                    printUsage = true;
                }
                else if (!System.IO.Directory.Exists(args[1]))
                {
                    logger.Info("Second argument is not a template directory");
                    printUsage = true;
                }
                else if (System.IO.Path.GetExtension(args[2]) != ".sln" && System.IO.File.Exists(args[2]))
                {
                    logger.Info("Third argument is not an existing sln file");
                    printUsage = true;
                }
                else
                {
                    foreach (var fileName in VisualStudio.Writer.Templates)
                    {
                        if (!System.IO.File.Exists(System.IO.Path.Combine(args[1], fileName)))
                        {
                            logger.Info(string.Format("Second argument is not a directory containing {0}", fileName));
                            printUsage = true;
                        }
                    }
                }
            }
            else
                printUsage = true;

            if (printUsage)
            {
                logger.Info("Create a Visual Studio solution");
                logger.Info("Usage:");
                logger.Info("  ProjectIO.VStoVS.exe <output directory> <template directory> <Visual Studio solution> <configuration|platform>");
                return;
            }

            var filePaths = new List<string> { args[2] };
            var projects = new Dictionary<string, Core.Project>();
            var filters = new Dictionary<string, string>();
            var paths = new Core.Paths();
            VisualStudio.Solution.Extract(logger, paths, filePaths, projects, filters);

            var solutionName = System.IO.Path.GetFileNameWithoutExtension(args[2]);
            var solution = new VisualStudio.Writer(projects, filters);
            solution.Write(solutionName, args[0], args[1]);
        }

        static void Main(string[] args)
        {
            MainFunc(args, new Core.PlainConsoleLogger());
        }
    }
}
