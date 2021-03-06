﻿//------------------------------------------------------------------------------
// <copyright file="Paths.cs" company="Zebedee Mason">
//     Copyright (c) 2018-2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Core
{
    using System.Collections.Generic;

    public class Paths
    {
        public Dictionary<string, string> Mapping { get; } = new Dictionary<string, string>();

        private static string WrapAlias(string alias)
        {
            return string.Format("$({0})\\", alias);
        }

        public static string UnwrapKey(string key)
        {
            return key.Substring(2, key.Length - 4);
        }

        public bool ContainsAlias(string alias)
        {
            return Mapping.ContainsKey(WrapAlias(alias));
        }

        public string Value(string alias)
        {
            return Mapping[WrapAlias(alias)];
        }

        public string InsertAliases(string filePath)
        {
            foreach (var path in Mapping)
            {
                filePath = filePath.Replace(path.Value, path.Key);
            }

            return filePath;
        }

        private string RemovePass(string filePath)
        {
            foreach (var path in Mapping)
            {
                if (path.Value.Length == 0)
                {
                    continue;
                }

                filePath = filePath.Replace(path.Key, path.Value);
            }

            return filePath;
        }

        private void Extract(string key)
        {
            if (Mapping.ContainsKey(key))
            {
                return;
            }

            var raw = UnwrapKey(key);
            var val = System.Environment.GetEnvironmentVariable(raw);
            Add(raw, val == null ? string.Empty : val);
        }

        private void ExtractAliases(string path)
        {
            var i = 0;
            while (true)
            {
                i = path.IndexOf("$(", i);
                if (i == -1)
                {
                    break;
                }

                var j = path.IndexOf(")", i);
                if (j == -1)
                {
                    break;
                }

                var key = path.Substring(i, j - i + 1) + System.IO.Path.DirectorySeparatorChar;
                Extract(key);

                i = j;
            }
        }

        public string RemoveAliases(string filePath)
        {
            var path = RemovePass(filePath);
            var i = path.IndexOf("$(");
            if (i == -1)
            {
                return path;
            }

            ExtractAliases(path);
            path = RemovePass(path);

            return path;
        }

        public void Add(string alias, string path, string localDirec = "")
        {
            var length = path.Length;
            if (length == 0)
            {
                return;
            }

            if (path[0] == '.')
            {
                path = System.IO.Path.Combine(localDirec, path);
                path = System.IO.Path.GetFullPath(path);
            }

            if (path[length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                path += System.IO.Path.DirectorySeparatorChar;
            }

            Mapping[WrapAlias(alias)] = path;
        }

        public void Remove(string alias)
        {
            var wrapped = WrapAlias(alias);
            if (Mapping.ContainsKey(wrapped))
            {
                Mapping.Remove(wrapped);
            }
        }

        public string Path(string link)
        {
            if (link.Length > 2 && link.Substring(0, 2) == "..")
            {
                link = System.IO.Path.Combine(Value("ProjectDir"), link);
                link = System.IO.Path.GetFullPath(link);
            }

            return link;
        }

        public string Combine(string direc, string filePath)
        {
            filePath = RemoveAliases(filePath);

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
            filePath = filePath.Replace(WrapAlias("ProjectDir"), Value("ProjectDir") + "\\");
            if (ContainsAlias("SolutionDir"))
            {
                filePath = filePath.Replace(WrapAlias("SolutionDir"), Value("SolutionDir") + "\\");
            }

            return Combine(Value("ProjectDir"), filePath);
        }
    }
}
