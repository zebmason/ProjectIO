//------------------------------------------------------------------------------
// <copyright file="Proj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    public abstract class Proj
    {
        public virtual string FilePath
        {
            get
            {
                return _path.FilePath;
            }
        }

        protected readonly ProjectPath _path;

        protected XMLUtils _xml;

        public Proj(ProjectPath path)
        {
            _path = path;
            _xml = new XMLUtils(path.FilePath);
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
                    link = _path.Path(link);
                    list.Add(link);
                }
            }

            return list;
        }

        public abstract List<string> Externals();
    }
}