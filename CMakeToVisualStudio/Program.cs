//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeToVisualStudio
{
    using System.Collections.Generic;

    public class Program
    {
        public static void MainFunc(string[] args, CMakeParser.IWriter writer)
        {
            writer.WriteLine("CMakeParser command line application for creating a Visual Studio solution");
            writer.WriteLine("Copyright (c) 2020 Zebedee Mason");

            bool printUsage = false;
            if (args.Length > 2)
            {
                if (!System.IO.Directory.Exists(args[0]))
                {
                    writer.WriteLine("First argument is not an output directory");
                    printUsage = true;
                }
                else if (!System.IO.Directory.Exists(args[1]))
                {
                    writer.WriteLine("Second argument is not a template directory");
                    printUsage = true;
                }
                else if (System.IO.Path.GetFileName(args[2]) != "CMakeLists.txt" && System.IO.File.Exists(args[2]))
                {
                    writer.WriteLine("Third argument is not an existing CMakeLists.txt");
                    printUsage = true;
                }
                else
                {
                    foreach (var fileName in VisualStudio.Writer.Templates)
                    {
                        if (!System.IO.File.Exists(System.IO.Path.Combine(args[1], fileName)))
                        {
                            writer.WriteLine(string.Format("Second argument is not a directory containing {0}", fileName));
                            printUsage = true;
                        }
                    }
                }
            }
            else
                printUsage = true;

            if (args.Length > 3 && System.IO.Path.GetFileName(args[3]) != "CMakeCache.txt" && System.IO.File.Exists(args[3]))
            {
                writer.WriteLine("Fourth argument is not an existing CMakeCache.txt");
                printUsage = true;
            }

            if (printUsage)
            {
                writer.WriteLine("Create a Visual Studio solution");
                writer.WriteLine("Usage:");
                writer.WriteLine("  CMakeParser.VisualStudio.exe <output directory> <template directory> <CMakeLists.txt> [CMakeCache.txt]");
                return;
            }

            var sourceDirec = System.IO.Path.GetDirectoryName(args[2]);
            var binaryDirec = string.Empty;
            if (args.Length > 3)
                binaryDirec = System.IO.Path.GetDirectoryName(args[3]);

            var state = new CMakeParser.State(sourceDirec, binaryDirec);
            state.ReadCache(args[3]);

            var binaries = new Dictionary<string, Core.Project>();
            var filters = new Dictionary<string, string>();
            var builder = CMakeParser.Builder.Instance(state, binaries, filters, writer);
            builder.Read();

            var solutionName = "solution";
            if (state.Variables.ContainsKey("${CMAKE_PROJECT_NAME}"))
                solutionName = state.Variables["${CMAKE_PROJECT_NAME}"];

            var solution = new VisualStudio.Writer(binaries, filters);
            solution.Write(solutionName, args[0], args[1]);
        }

        static void Main(string[] args)
        {
            MainFunc(args, new CMakeParser.Writer());
        }
    }
}
