using System;

namespace Ninu.Emulator
{
    /// <summary>
    /// Specifies that this field or property should be saved or loaded for save states. By default the name of the key
    /// will be the name of the field or property this attribute is applied to. This can be overriden using the
    /// <see cref="SaveAttribute.SaveAttribute(string)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SaveAttribute : Attribute
    {
        /// <summary>
        /// If this property is not null, it indicates the name to use for the key. If it is null, the name of the
        /// field or property will be used instead.
        /// </summary>
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