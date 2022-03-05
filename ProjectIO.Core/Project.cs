//------------------------------------------------------------------------------
// <copyright file="Project.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class Project
    {
        public bool IsExe { get; set; } = false;

        public List<string> FilePaths { get; } = new List<string>();

        public List<string> Dependencies { get; } = new List<string>();

        public static List<string> Order(ILogger logger, Dictionary<string, Project> projects)
        {
            var order = new List<string>();

            var available = projects.Keys.ToList();
            while (available.Count != 0)
            {
                int completed = order.Count;
                foreach (var name in available)
                {
                    bool found = false;
                    foreach (var dep in projects[name].Dependencies)
                    {
                        if (projects.ContainsKey(dep) && available.Contains(dep))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        order.Add(name);
                    }
                }

                if (completed == order.Count)
                {
                    logger.Warn("Circular dependency found in project order");
                    return projects.Keys.ToList();
                }

                for (int i = completed; i < order.Count; ++i)
                {
                    available.Remove(order[i]);
                }
            }

            return order;
        }
    }
}
