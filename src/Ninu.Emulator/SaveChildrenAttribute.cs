using System;

namespace Ninu.Emulator
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SaveChildrenAttribute : Attribute
    {
        public string? Name{ get; }

        public SaveChildrenAttribute()
        {

        }

        public SaveChildrenAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}