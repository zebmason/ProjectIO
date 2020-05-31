//------------------------------------------------------------------------------
// <copyright file="CSProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    public class CSProj : NetProj
    {
        public CSProj(ProjectPath path)
            : base(path)
        {
        }

        public override List<string> Dependencies()
        {
            var list = base.Dependencies();
            var l1 = new List<System.Xml.XmlElement>();
            xml.SelectNodes(xml.root, "Import", l1);
            foreach (var i1 in l1)
            {
                if (!i1.HasAttribute("Label"))
                    continue;

                if (i1.GetAttribute("Label") != "Shared")
                    continue;

                var link = i1.GetAttribute("Project");
                link = path.Path(link);
                list.Add(link);
            }

            return list;
        }
    }
}
