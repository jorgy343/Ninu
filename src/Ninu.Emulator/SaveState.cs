using System;
using System.Reflection;

namespace Ninu.Emulator
{
    public static class SaveState
    {
        public static byte[] Save(Console console)
        {
            if (console is null) throw new ArgumentNullException(nameof(console));

            var context = new SaveStateContext();

            SaveObject(context, console, "Console");

            return context.ToData();
        }

        private static void SaveObject(SaveStateContext context, object obj, string keyPath)
        {
            var type = obj.GetType();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var saveChildrenAttribute = field.GetCustomAttribute<SaveChildrenAttribute>();
                var saveAttribute = field.GetCustomAttribute<SaveAttribute>();

                // If both the SaveChildrenAttribute and SaveAttribute exist on a field, ignore the SaveAttribute.
                if (saveChildrenAttribute is not null)
                {
                    var value = field.GetValue(obj);

                    if (value is not null)
                    {
                        var name = saveChildrenAttribute.Name ?? field.Name;

                        SaveObject(context, value, $"{keyPath}.{name}");
                    }
                }
                else if (saveAttribute is not null)
                {
                    var name = saveAttribute.Name ?? field.Name;

                    context.AddToState($"{keyPath}.{name}", field.GetValue(obj));
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var saveChildrenAttribute = property.GetCustomAttribute<SaveChildrenAttribute>();
                var saveAttribute = property.GetCustomAttribute<SaveAttribute>();

                // If both the SaveChildrenAttribute and SaveAttribute exist on a property, ignore the SaveAttribute.
                if (saveChildrenAttribute is not null)
                {
                    var value = property.GetValue(obj);

                    if (value is not null)
                    {
                        var name = saveChildrenAttribute.Name ?? property.Name;

                        SaveObject(context, value, $"{keyPath}.{name}");
                    }
                }
                else if (saveAttribute is not null)
                {
                    var name = saveAttribute.Name ?? property.Name;

                    context.AddToState($"{keyPath}.{name}", property.GetValue(obj));
                }
            }
        }

        public static void Load(Console console, byte[] data)
        {
            if (console is null) throw new ArgumentNullException(nameof(console));
            if (data is null) throw new ArgumentNullException(nameof(data));

            var context = new SaveStateContext(data);

            LoadObject(context, console, "Console");
        }

        private static void LoadObject(SaveStateContext context, object obj, string keyPath)
        {
            var type = obj.GetType();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var saveChildrenAttribute = field.GetCustomAttribute<SaveChildrenAttribute>();
                var saveAttribute = field.GetCustomAttribute<SaveAttribute>();

                if (saveChildrenAttribute is not null)
                {
                    var value = field.GetValue(obj);

                    if (value is not null)
                    {
                        var name = saveChildrenAttribute.Name ?? field.Name;

                        LoadObject(context, value, $"{keyPath}.{name}");
                    }
                }
                else if (saveAttribute is not null)
                {
                    var name = saveAttribute.Name ?? field.Name;

                    if (context.TryGetFromState($"{keyPath}.{name}", field.FieldType, out var value))
                    {
                        field.SetValue(obj, value);
                    }
                }
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var saveChildrenAttribute = property.GetCustomAttribute<SaveChildrenAttribute>();
                var saveAttribute = property.GetCustomAttribute<SaveAttribute>();

                if (saveChildrenAttribute is not null)
                {
                    var value = property.GetValue(obj);

                    if (value is not null)
                    {
                        var name = saveChildrenAttribute.Name ?? property.Name;

                        LoadObject(context, value, $"{keyPath}.{name}");
                    }
                }
                else if (saveAttribute is not null)
                {
                    var name = saveAttribute.Name ?? property.Name;

                    if (context.TryGetFromState($"{keyPath}.{name}", property.PropertyType, out var value))
                    {
                        property.SetValue(obj, value);
                    }
                }
            }
        }
    }
}