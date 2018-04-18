using System;
using System.Collections.Generic;
using System.Text;

namespace Docker.AppSDK
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ParameterAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; set; }
        public bool Mandatory { get; set; }
        public ParameterAttribute(string name) => Name = name;

    }
}
