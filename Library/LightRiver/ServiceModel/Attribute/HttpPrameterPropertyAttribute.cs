using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightRiver.ServiceModel
{
    [AttributeUsage(AttributeTargets.Property, Inherited=true)]
    public class HttpPrameterPropertyAttribute : Attribute
    {
        public string Name { get; set; }

        public int ByteCount { get; set; }

        public HttpPrameterPropertyAttribute(string name)
            : this(name, 0)
        {
        }

        public HttpPrameterPropertyAttribute(int byteCount)
            : this(null, byteCount)
        {
        }

        public HttpPrameterPropertyAttribute(string name, int byteCount)
        {
            Name = name;
            ByteCount = byteCount;
        }
    }
}
