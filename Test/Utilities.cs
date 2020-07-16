//------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Zebedee Mason">
//     Copyright (c) 2019-2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Test
{
    using FluentAssertions;

    using System.Collections.Generic;

    class Writer : Core.ILogger
    {
        private readonly System.IO.StringWriter _writer = new System.IO.StringWriter();

        public void Info(string line)
        {
            _writer.WriteLine(line);
        }

        public void Info(string message, string argument)
        {
            _writer.WriteLine(string.Format(message, argument));
        }

        public void Warn(string line)
        {
            _writer.WriteLine(line);
        }

        public void Warn(string message, string argument)
        {
            _writer.WriteLine(string.Format(message, argument));
        }

        public string Buffer
        {
            get { return _writer.ToString(); }
        }
    }

    class Utilities
    {
        private readonly string _dataDirec;

        protected readonly List<string> _args = new List<string>();

        protected string _lines;

        internal Utilities(string testDirec = @"../../Data")
        {
            _dataDirec = System.Reflection.Assembly.GetAssembly(typeof(Utilities)).Location;
            _dataDirec = System.IO.Path.GetDirectoryName(_dataDirec);
            _dataDirec = System.IO.Path.Combine(_dataDirec, testDirec);
            _dataDirec = System.IO.Path.GetFullPath(_dataDirec);
        }

        public string Combine(string fileName)
        {
            return System.IO.Path.Combine(_dataDirec, fileName);
        }

        internal void CompareLines(string lines, string expected)
        {
            lines = lines.Replace("\r", string.Empty);
            expected = expected.Replace("\r", string.Empty);
            if (lines != expected)
            {
                var linesSplit = lines.Split('\n');
                var expectedSplit = expected.Split('\n');
                var num = linesSplit.Length;
                if (num > expectedSplit.Length)
                {
                    num = expectedSplit.Length;
                }

                for (var i = 0; i < num; ++i)
                {
                    linesSplit[i].Should().Be(expectedSplit[i]);
                }

                if (linesSplit.Length > expectedSplit.Length)
                {
                    linesSplit[num].Should().Be(string.Empty);
                }
                else
                {
                    string.Empty.Should().Be(expectedSplit[num]);
                }
            }
        }

        internal void CompareLogs(string processed)
        {
            processed = Combine(processed);

            if (System.IO.File.Exists(processed))
            {
                var expected = System.IO.File.ReadAllText(processed);
                CompareLines(_lines, expected);
            }
            else
            {
                _lines.Should().Be("Not missing!");
            }
        }

        internal void Delete(string fileName)
        {
            fileName = Combine(fileName);
            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }
        }

        public static string GetDataDirec()
        {
            var utils = new Utilities();
            return utils._dataDirec;
        }
    }

    class ListerUtilities : Utilities
    {
        internal void Eval(string result)
        {
            var writer = new Writer();
            CMakeLister.Program.MainFunc(_args.ToArray(), writer);

            Delete(result);
            _lines = writer.Buffer.Replace(Combine("Source"), "${SourceDirec}").Replace(Combine("Binary"), "${BinaryDirec}").Replace("/", "\\");
            var file = new System.IO.StreamWriter(Combine(result));
            file.Write(_lines);
            file.Close();
        }

        private ListerUtilities ConstructRead(string direc)
        {
            string sourceDirec = System.IO.Path.Combine("Source", direc);
            string binaryDirec = System.IO.Path.Combine("Binary", direc);
            _args.Clear();
            _args.Add(Combine(sourceDirec));
            if (binaryDirec.Length > 0)
                _args.Add(Combine(binaryDirec));

            return this;
        }

        public static void ReadTest(string direc)
        {
            var utils = new ListerUtilities();
            var result = System.IO.Path.Combine("Results", direc) + ".log";
            utils.ConstructRead(direc).Eval(result);

            var processed = System.IO.Path.Combine("Processed", direc) + ".log";
            utils.CompareLogs(processed);
        }
    }

    class VSUtilities : Utilities
    {
        internal void Eval(string result)
        {
            var writer = new Writer();
            CMakeToVisualStudio.Program.MainFunc(_args.ToArray(), writer);

            Delete(result);
            _lines = writer.Buffer.Replace(Combine("Source"), "${SourceDirec}").Replace(Combine("Binary"), "${BinaryDirec}").Replace("/", "\\");
            var file = new System.IO.StreamWriter(Combine(result));
            file.Write(_lines);
            file.Close();
        }

        private VSUtilities ConstructRead(string direc)
        {
            string cmakeLists = System.IO.Path.Combine(System.IO.Path.Combine("Source", direc), "CMakeLists.txt");
            string cmakeCache = System.IO.Path.Combine(System.IO.Path.Combine("Binary", direc), "CMakeCache.txt");
            _args.Clear();
            _args.Add(Combine("Results"));
            _args.Add(Combine("Templates"));
            _args.Add(Combine(cmakeLists));
            _args.Add(Combine(cmakeCache));

            return this;
        }

        internal string StripGuids(string lines)
        {
            int i = 0;
            while ((i = lines.IndexOf('{', i)) != -1)
            {
                int j = i + 37;
                if (j >= lines.Length)
                    break;

                if (lines[j] == '}')
                {
                    lines = lines.Substring(0, i + 1) + lines.Substring(j);
                }

                ++i;
            }
            return lines;
        }

        internal void CompareFiles(string result, string processed, string replace = "", string with = "")
        {
            result = Combine(result);
            if (!System.IO.File.Exists(result))
            {
                result.Should().Be("Not missing!");
            }

            processed = Combine(processed);
            if (!System.IO.File.Exists(processed))
            {
                processed.Should().Be("Not missing!");
            }

            var lines = System.IO.File.ReadAllText(result);
            if (replace.Length > 0)
                lines = lines.Replace(replace, with);

            var expected = System.IO.File.ReadAllText(processed);
            CompareLines(StripGuids(lines), StripGuids(expected));
        }

        public static void ReadTest(string direc, IEnumerable<string> projects)
        {
            var utils = new VSUtilities();
            var result = System.IO.Path.Combine("Results", direc) + ".vs.log";
            utils.ConstructRead(direc).Eval(result);

            var processed = System.IO.Path.Combine("Processed", direc) + ".vs.log";
            utils.CompareLogs(processed);

            result = System.IO.Path.Combine("Results", direc) + ".sln";
            processed = System.IO.Path.Combine("Processed", direc) + ".sln";
            utils.CompareFiles(result, processed);

            foreach(var project in projects)
            {
                var replace = utils.Combine("");

                result = System.IO.Path.Combine(System.IO.Path.Combine("Results", project), project) + ".vcxproj";
                processed = System.IO.Path.Combine(System.IO.Path.Combine("Processed", project), project) + ".vcxproj";
                utils.CompareFiles(result, processed, replace, "..\\..");

                result += ".filters";
                processed += ".filters";
                utils.CompareFiles(result, processed, replace, "..\\..");
            }
        }
    }
}
