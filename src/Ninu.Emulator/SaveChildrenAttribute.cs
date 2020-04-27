using System;

namespace Ninu.Emulator
{
    /// <summary>
    /// Indicates that the save state logic will seek other <see cref="SaveChildrenAttribute"/> and
    /// <see cref="SaveAttribute"/> within the instance of the field and property this attribute is attached to. The
    /// name of the field this attribute is applied to is used in the key. The name can be overriden with the
    /// <see cref="SaveChildrenAttribute.SaveChildrenAttribute(string)"/> constructor.
    /// </summary>
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