using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppSDK
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AppAttribute : Attribute
    {
        public string Name { get; }
        public Version Version { get; }
        public string Description { get; set; }
        public string Author { get; set; }

        public AppAttribute(string name, string version)
        {
            Name = name;
            Version = new Version(version);
        }
    }
}
