﻿//------------------------------------------------------------------------------
// <copyright file="Proj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    internal abstract class Proj
    {
        protected string _filePath;

        protected Core.Paths _paths;

        public virtual string FilePath
        {
            get
            {
                return _filePath;
            }
        }

        protected XMLUtils _xml;

        public Proj(string path, Core.Paths paths)
        {
            _filePath = path;
            _paths = paths;
            _xml = new XMLUtils(path);
        }

        public abstract string Name { get; }

        public virtual List<string> Dependencies()
        {
            var list = new List<string>();
            var l1 = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(_xml._root, "ItemGroup", l1);
            foreach (var i1 in l1)
            {
                var l2 = new List<System.Xml.XmlElement>();
                _xml.SelectNodes(i1, "ProjectReference", l2);
                foreach (var i2 in l2)
                {
                    var link = i2.GetAttribute("Include");
                    link = _paths.RemoveAliases(link);
                    list.Add(link);
                }
            }

            return list;
        }

        public abstract List<string> Externals();

        public void SetProjectPath()
        {
            _paths.Add("$(ProjectDir)", System.IO.Path.GetDirectoryName(_filePath));
        }

        public static void UnsetProjectPath(Core.Paths paths)
        {
            paths.Remove("$(ProjectDir)");
        }
    }
}