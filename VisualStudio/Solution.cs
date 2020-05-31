//------------------------------------------------------------------------------
// <copyright file="Project.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeToVisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProjectIO.Core;

    public class Solution
    {
        public static string[] Templates { get; } = new string[] {
            "vcxproj", "vcxproj.compile", "vcxproj.reference", "vcxproj.console",
            "vcxproj.filters", "vcxproj.filters.compile", "vcxproj.filters.filter",
            "sln", "sln.project", "sln.global" };

        private Dictionary<string, string> _templates = new Dictionary<string, string>();

        private Dictionary<string, string> _guids = new Dictionary<string, string>();

        private readonly Dictionary<string, Core.Project> _binaries;

        private readonly Dictionary<string, string> _filters;

        public Solution(Dictionary<string, Core.Project> binaries, Dictionary<string, string> filters)
        {
            _binaries = binaries;
            _filters = filters;
        }

        private static string Definitions(Core.Project binary)
        {
            var definitions = binary.CompileDefinitions.Split().ToList();
            if (binary.IsExe)
                definitions.Add("_CONSOLE");

            if (definitions.Count == 0)
                return string.Empty;

            return string.Join(";", definitions) + ";";
        }

        private static string IncludeDirectories(Core.Project binary)
        {
            if (binary.IncludeDirectories.Count == 0)
                return string.Empty;

            return string.Join(";", binary.IncludeDirectories) + ";";
        }

        private string References(Core.Project binary)
        {
            var references = string.Empty;
            foreach (var library in binary.Libraries)
            {
                if (!_binaries.ContainsKey(library))
                    continue;

                var reference = _templates["vcxproj.reference"];
                reference = reference.Replace("{{name}}", library);
                reference = reference.Replace("{{guid}}", _guids[library]);
            }

            return references;
        }

        private string Console(Core.Project binary)
        {
            if (!binary.IsExe)
                return string.Empty;

            return _templates["vcxproj.console"];
        }

        private string Libraries(Project binary)
        {
            if (binary.Libraries.Count == 0)
                return string.Empty;

            return string.Join(".lib;", binary.Libraries) + ".lib;";
        }

        private void VCXProj(string name, Core.Project binary, string directory)
        {
            var compiles = string.Empty;
            var includes = string.Empty;
            foreach (var filePath in binary.FilePaths)
            {
                var compile = _templates["vcxproj.compile"];
                compile = compile.Replace("{{path}}", filePath);
                if (Core.Project.IsHeader(filePath))
                    includes += compile.Replace("{{compile}}", "ClInclude");
                else
                    compiles += compile.Replace("{{compile}}", "ClCompile");
            }

            var file = new System.IO.StreamWriter(System.IO.Path.Combine(directory, name + ".vcxproj"));
            var vcxproj = _templates["vcxproj"];
            vcxproj = vcxproj.Replace("{{compiles}}", compiles.TrimEnd());
            vcxproj = vcxproj.Replace("{{includes}}", includes.TrimEnd());
            vcxproj = vcxproj.Replace("{{references}}", References(binary));
            vcxproj = vcxproj.Replace("{{includedirecs}}", IncludeDirectories(binary));
            vcxproj = vcxproj.Replace("{{definitions}}", Definitions(binary));
            vcxproj = vcxproj.Replace("{{console}}", Console(binary));
            vcxproj = vcxproj.Replace("{{guid}}", _guids[name]);
            vcxproj = vcxproj.Replace("{{name}}", name);
            vcxproj = vcxproj.Replace("{{binary}}", binary.IsExe ? "Application" : "DynamicLibrary");
            vcxproj = vcxproj.Replace("{{libraries}}", Libraries(binary));
            file.Write(vcxproj);
            file.Close();
        }

        private static void AddFilter(string filter, HashSet<string> maps)
        {
            for (int i = 0; i < filter.Length; ++i)
            {
                if (filter[i] == '\\')
                {
                    maps.Add(filter.Substring(0, i));
                }
            }

            maps.Add(filter);
        }

        private void Filters(string name, Core.Project binary, string directory)
        {
            var maps = new HashSet<string>();
            var compiles = string.Empty;
            var includes = string.Empty;
            foreach (var filePath in binary.FilePaths)
            {
                if (!_filters.ContainsKey(filePath))
                    continue;

                AddFilter(_filters[filePath], maps);
                var compile = _templates["vcxproj.filters.compile"];
                compile = compile.Replace("{{path}}", filePath).Replace("{{filter}}", _filters[filePath]);
                if (Core.Project.IsHeader(filePath))
                    includes += compile.Replace("{{compile}}", "ClInclude");
                else
                    compiles += compile.Replace("{{compile}}", "ClCompile");
            }

            if (maps.Count == 0)
                return;

            var filters = string.Empty;
            foreach (var item in maps)
            {
                var filter = _templates["vcxproj.filters.filter"];
                filters += filter.Replace("{{filter}}", item).Replace("{{guid}}", System.Guid.NewGuid().ToString());
            }

            var file = new System.IO.StreamWriter(System.IO.Path.Combine(directory, name + ".vcxproj.filters"));
            var vcxproj_filters = _templates["vcxproj.filters"];
            vcxproj_filters = vcxproj_filters.Replace("{{filters}}", filters.TrimEnd());
            vcxproj_filters = vcxproj_filters.Replace("{{compiles}}", compiles.TrimEnd());
            vcxproj_filters = vcxproj_filters.Replace("{{includes}}", includes.TrimEnd());
            file.Write(vcxproj_filters);
            file.Close();
        }

        private void LoadTemplates(string directory)
        {
            foreach (var fileName in Templates)
            {
                var filePath = System.IO.Path.Combine(directory, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    _templates[fileName] = System.IO.File.ReadAllText(filePath);
                }
                else
                {
                    _templates[fileName] = string.Empty;
                }
            }
        }

        private void SolutionFile(string name, string directory)
        {
            var sln_projects = string.Empty;
            var sln_globals = string.Empty;
            foreach (var binary in _binaries)
            {
                var sln_project = _templates["sln.project"];
                sln_project = sln_project.Replace("{{name}}", binary.Key).Replace("{{guid}}", _guids[binary.Key]);
                sln_projects += sln_project;

                var sln_global = _templates["sln.global"];
                sln_global = sln_global.Replace("{{guid}}", _guids[binary.Key]);
                sln_globals += sln_global;
            }

            var file = new System.IO.StreamWriter(System.IO.Path.Combine(directory, name + ".sln"));
            var sln = _templates["sln"];
            sln = sln.Replace("{{projects}}", sln_projects.TrimEnd()).Replace("{{globals}}", sln_globals.TrimEnd());
            file.Write(sln);
            file.Close();
        }

        public void Write(string solutionName, string outputDirectory, string templateDirectory)
        {
            LoadTemplates(templateDirectory);
            foreach (var project in _binaries)
            {
                _guids[project.Key] = System.Guid.NewGuid().ToString();
            }

            SolutionFile(solutionName, outputDirectory);
            foreach (var project in _binaries)
            {
                var directory = System.IO.Path.Combine(outputDirectory, project.Key);
                System.IO.Directory.CreateDirectory(directory);
                VCXProj(project.Key, project.Value, directory);
                Filters(project.Key, project.Value, directory);
            }
        }
    }
}
