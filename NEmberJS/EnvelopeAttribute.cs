using System;

namespace NEmberJS
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EnvelopeAttribute : Attribute
    {
        public EnvelopeAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
