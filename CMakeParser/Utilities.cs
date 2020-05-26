//------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public static class Utilities
    {
        public static KeyValuePair<string, string> Split(string line)
        {
            var index = line.IndexOf(" ");
            if (index == -1)
            {
                return new KeyValuePair<string, string>(line, string.Empty);
            }

            return new KeyValuePair<string, string>(line.Substring(0, index), line.Substring(index).Trim());
        }

        public static Dictionary<string, string> Split(string line, string[] keywords)
        {
            line = string.Format(" {0} ", line);
            var indices = new Dictionary<string, int>();
            foreach (var keyword in keywords)
            {
                var item = string.Format(" {0} ", keyword);
                var index = line.IndexOf(item);
                if (index == -1)
                {
                    item = string.Format(" {0} ", keyword.ToLower());
                    index = line.IndexOf(item);
                    if (index == -1)
                    {
                        continue;
                    }
                }

                indices[keyword] = index;
            }

            var dict = new Dictionary<string, string>();
            foreach (var index in indices.Values.OrderByDescending(num => num))
            {
                var keyword = indices.FirstOrDefault(x => x.Value == index).Key;
                dict[keyword] = line.Substring(index + keyword.Length + 2).Trim();
                line = line.Substring(0, index);
            }

            dict[""] = line.Trim();
            return dict;
        }
    }
}
