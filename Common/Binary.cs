//------------------------------------------------------------------------------
// <copyright file="Binary.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Common
{
    using System.Collections.Generic;
    using System.Linq;

    public class Binary
    {
        private readonly IWriter _writer;

        public bool IsExe { get; }

        public List<string> FilePaths { get; } = new List<string>();

        public List<string> Libraries { get; } = new List<string>();

        public string CompileDefinitions { get; set; } = string.Empty;

        public List<string> IncludeDirectories { get; } = new List<string>();

        public Binary(IWriter writer, Dictionary<string, string> properties, bool isExe)
        {
            _writer = writer;
            CompileDefinitions = properties["COMPILE_DEFINITIONS"];
            IsExe = isExe;
        }

        public static bool IsHeader(string fileName)
        {
            var exts = new string[] { ".h", ".hh", ".hpp", ".hxx", ".h++" };
            return exts.Contains(System.IO.Path.GetExtension(fileName));
        }
    }
}
