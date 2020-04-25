using System;

namespace Ninu.Emulator
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SaveAttribute : Attribute
    {
        public string? Name { get; }

        public SaveAttribute()
        {

        }

        public SaveAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}