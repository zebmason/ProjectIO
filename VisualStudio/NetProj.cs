//------------------------------------------------------------------------------
// <copyright file="NetProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    internal abstract class NetProj : Proj
    {
        public NetProj(string path, Core.Paths paths, string configPlatform)
            : base(path, paths, configPlatform)
        {
        }

        public override string Name
        {
            get
            {
                var l1 = new List<System.Xml.XmlElement>();
                _xml.SelectNodes(_xml._root, "PropertyGroup", l1);
                foreach (var i1 in l1)
                {
                    var l2 = new List<System.Xml.XmlElement>();
                    _xml.SelectNodes(i1, "AssemblyName", l2);
                    foreach (var i2 in l2)
                    {
                        return i2.InnerText;
                    }
                }

                return string.Empty;
            }
        }

        public override List<string> Externals()
        {
            var list = new List<string>();
            var l1 = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(_xml._root, "ItemGroup", l1);
            foreach (var i1 in l1)
            {
                var l2 = new List<System.Xml.XmlElement>();
                _xml.SelectNodes(i1, "Reference", l2);
                foreach (var i2 in l2)
                {
                    var link = i2.GetAttribute("Include");
                    if (link == "System" || (link.Length > 7 && link.Substring(0, 7) == "System."))
                        continue;

                    var i = link.IndexOf(",");
                    if (i != -1)
                        link = link.Substring(0, i);

                    list.Add(link);
                }
            }

            return list;
        }

        public virtual void Compiles(List<string> files, Core.ILogger logger, Core.Paths filePath)
        {
            _xml.DotNetCompiles(this, files, logger, filePath);
        }
    }
}