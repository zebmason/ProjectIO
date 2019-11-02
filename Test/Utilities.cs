//------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Test
{
    using FluentAssertions;

    using System.Collections.Generic;

    class Utilities
    {
        class Writer : Program.IWriter
        {
            private readonly System.IO.StringWriter _writer = new System.IO.StringWriter();

            public void WriteLine(string line)
            {
                _writer.WriteLine(line);
            }

            public string Buffer
            {
                get { return _writer.ToString(); }
            }
        }

        private readonly string _dataDirec;

        private readonly List<string> _args = new List<string>();

        private string _lines;

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

        private void CompareLines(string lines, string expected)
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

        internal void Eval(string result)
        {
            var writer = new Writer();
            Program.MainFunc(_args.ToArray(), writer);

            Delete(result);
            _lines = writer.Buffer.Replace(Combine("Source"), "${SourceDirec}").Replace(Combine("Binary"), "${BinaryDirec}").Replace("/", "\\");
            var file = new System.IO.StreamWriter(Combine(result));
            file.Write(_lines);
            file.Close();
        }

        public Utilities ConstructRead(string sourceDirec, string binaryDirec)
        {
            _args.Clear();
            _args.Add(Combine(sourceDirec));
            if (binaryDirec.Length > 0)
                _args.Add(Combine(binaryDirec));

            return this;
        }

        public static void ReadTest(string direc)
        {
            var utils = new Utilities();
            var result = System.IO.Path.Combine("Results", direc) + ".log";
            utils.ConstructRead(System.IO.Path.Combine("Source", direc), System.IO.Path.Combine("Binary", direc)).Eval(result);

            var processed = System.IO.Path.Combine("Processed", direc) + ".log";
            utils.CompareLogs(processed);
        }
    }
}
