//------------------------------------------------------------------------------
// <copyright file="NetProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal abstract class NetProj : Proj
    {
        public NetProj(Core.ILogger logger, string path, Core.Paths paths, string configPlatform)
            : base(logger, path, paths, configPlatform)
        {
        }

        public abstract Core.Project Extract(Core.ILogger logger, Core.Paths paths, string filePath,
            Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping);

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

                return Path.GetFileNameWithoutExtension(FilePath);
            }
        }

        private static string Include(System.Xml.XmlElement element, bool includeSystem)
        {
            var link = element.GetAttribute("Include");
            if (includeSystem)
                return link;

            if (link == "System" || (link.Length > 7 && link.Substring(0, 7) == "System."))
                return string.Empty;

            return link;
        }

        public override Dictionary<string, string> ExternalsWithVersions(bool includeSystem = false)
        {
            var dict = new Dictionary<string, string>();
            var l1 = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(_xml._root, "ItemGroup", l1);
            var config = new Dictionary<string, string>();
            if (l1.Any())
            {
                var direc = Path.GetDirectoryName(_filePath);
                if (direc != null)
                {
                    var configPath = Path.Combine(direc, "packages.config");
                    if (File.Exists(configPath))
                    {
                        var xml = new XMLUtils(configPath);
                        var packages = new List<System.Xml.XmlElement>();
                        xml.SelectNodes(xml._root, "package", packages, true);
                        foreach (var package in packages)
                        {
                            config[package.GetAttribute("id").ToLower()] = package.GetAttribute("version");
                        }
                    }
                }
            }

            foreach (var i1 in l1)
            {
                var l2 = new List<System.Xml.XmlElement>();
                _xml.SelectNodes(i1, "Reference", l2);
                foreach (var i2 in l2)
                {
                    var link = Include(i2, includeSystem);
                    if (link.Length == 0)
                        continue;

                    var value = "*";
                    var i = link.IndexOf(",", StringComparison.Ordinal);
                    if (i != -1)
                    {
                        var line = link;
                        link = link.Substring(0, i);
                        var lower = link.ToLower();
                        if (config.ContainsKey(lower))
                        {
                            value = config[lower];
                        }
                        else
                        {
                            var bits = line.Split(new char[] { ' ', '=', '"', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (bits.Length > 2 && bits[1] == "Version")
                            {
                                value = bits[2];
                            }
                        }
                    }

                    dict[link] = value;
                }

                l2 = new List<System.Xml.XmlElement>();
                _xml.SelectNodes(i1, "PackageReference", l2);
                foreach (var i2 in l2)
                {
                    var link = Include(i2, includeSystem);
                    if (link.Length == 0)
                        continue;

                    dict[link] = i2.GetAttribute("Version");
                }
            }

            return dict;
        }

        public override List<string> Externals()
        {
            return ExternalsWithVersions().Keys.ToList();
        }

        public virtual void Compiles(List<string> files, Core.ILogger logger, Core.Paths filePath)
        {
            _xml.DotNetCompiles(this, files, logger, filePath);
        }
    }
}