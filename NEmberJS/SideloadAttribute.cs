using System;

namespace NEmberJS
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class SideloadAttribute : Attribute
    {
    }
}