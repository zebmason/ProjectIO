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
                return path.FilePath;
            }
        }

        protected readonly ProjectPath path;

        protected XMLUtils xml;

        public Proj(ProjectPath path)
        {
            this.path = path;
            xml = new XMLUtils(path.FilePath);
        }

        public abstract string Name { get; }

        public virtual List<string> Dependencies()
        {
            var list = new List<string>();
            var l1 = new List<System.Xml.XmlElement>();
            xml.SelectNodes(xml.root, "ItemGroup", l1);
            foreach (var i1 in l1)
            {
                var l2 = new List<System.Xml.XmlElement>();
                xml.SelectNodes(i1, "ProjectReference", l2);
                foreach (var i2 in l2)
                {
                    var link = i2.GetAttribute("Include");
                    link = path.Path(link);
                    list.Add(link);
                }
            }

            return list;
        }

        public abstract List<string> Externals();
    }
}