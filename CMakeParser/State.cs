//------------------------------------------------------------------------------
// <copyright file="State.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class State
    {
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public State(string sourceDirec, string binaryDirec)
        {
            Variables["${PROJECT_SOURCE_DIR}"] = sourceDirec;
            Variables["${PROJECT_BINARY_DIR}"] = binaryDirec;
            Variables["${CMAKE_CURRENT_SOURCE_DIR}"] = "${PROJECT_SOURCE_DIR}";
            Variables["${CMAKE_CURRENT_BINARY_DIR}"] = "${PROJECT_BINARY_DIR}";
        }

        public State(State state)
        {
            Variables = state.Variables.ToDictionary(entry => entry.Key, entry => entry.Value);
            Properties = state.Properties.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        public State SubDirectory(string sub)
        {
            var state = new State(this);
            state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"] = Variables["${CMAKE_CURRENT_SOURCE_DIR}"] + "/" + sub;
            state.Variables["${CMAKE_CURRENT_BINARY_DIR}"] = Variables["${CMAKE_CURRENT_BINARY_DIR}"] + "/" + sub;
            return state;
        }

        private string Replace(string initial, int index)
        {
            index = initial.IndexOf("${", index);
            if (index == -1)
                return initial;

            var i2 = initial.IndexOf("}", index);
            var key = initial.Substring(index, i2 - index + 1);
            if (!Variables.ContainsKey(key))
                return Replace(initial, index + 1);

            return Replace(initial.Replace(key, Variables[key]), index);
        }

        public string Replace(string initial)
        {
            initial = initial.Replace("$env{", "$ENV{");
            var index = initial.IndexOf("$ENV{");
            while (index != -1)
            {
                var i2 = initial.IndexOf("}", index);
                var env = initial.Substring(index + 5, i2 - index - 5);
                var val = System.Environment.GetEnvironmentVariable(env);
                initial = initial.Replace("$ENV{" + env + "}", val);
                index = initial.IndexOf("$ENV{");
            }

            return Replace(initial, 0);
        }

        private string FullPath(string path)
        {
            path = path.Replace("\"", string.Empty);

            if (System.IO.Path.GetPathRoot(path).Length == 0)
            {
                path = System.IO.Path.Combine(Replace("${CMAKE_CURRENT_SOURCE_DIR}", 0), path);
            }

            return System.IO.Path.GetFullPath(path);
        }

        public List<string> FileOrDirectoryList(string list)
        {
            var items = new List<string>();
            foreach (var item in SplitList(list))
            {
                items.Add(FullPath(item));
            }

            return items;
        }

        public List<string> SplitList(string list)
        {
            var items = new List<string>();
            list = Replace(" " + list + " ");
            foreach (var item in list.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                items.Add(item);
            }

            return items;
        }
    }
}
