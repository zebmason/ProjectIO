//------------------------------------------------------------------------------
// <copyright file="SHProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    public class SHProj : NetProj
    {
        public override string FilePath
        {
            get
            {
                var l1 = new List<System.Xml.XmlElement>();
                _xml.SelectNodes(_xml._root, "Import", l1);
                foreach (var i1 in l1)
                {
                    if (!i1.HasAttribute("Label"))
                        continue;

                    if (i1.GetAttribute("Label") != "Shared")
                        continue;

                    var link = i1.GetAttribute("Project");
                    link = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_path.FilePath), link);

                    return link;
                }

                return string.Empty;
            }
        }

        public SHProj(ProjectPath path)
            : base(path)
        {
        }

        public override string Name
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(_path.FilePath);
            }
        }

        public override List<string> Dependencies()
        {
            return new List<string>();
        }

        public override List<string> Externals()
        {
            var list = new List<string>();
            return list;
        }

        public override void Compiles(Dictionary<string, string> files, Core.ILogger logger, Core.Paths filePath)
        {
            var xml2 = new XMLUtils(FilePath);
            xml2.DotNetCompiles(this, files, logger, filePath);
        }
    }
}
