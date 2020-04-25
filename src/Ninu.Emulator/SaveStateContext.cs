using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ninu.Emulator
{
    public class SaveStateContext
    {
        private readonly Dictionary<string, object?> _values = new Dictionary<string, object?>();

        public void AddToState(string key, object? value) => _values[key] = value;

        public object? GetFromState(string key, Type type)
        {
            if (type == typeof(byte))
            {
                var a = (long)_values[key]!;
                return (byte)a;
            }
            else if (type == typeof(ushort))
            {
                var a = (long)_values[key]!;
                return (ushort)a;
            }
            else if (type == typeof(int))
            {
                var a = (long)_values[key]!;
                return (int)a;
            }
            else if (type == typeof(CpuFlags))
            {
                var a = (long)_values[key]!;
                return (CpuFlags)a;
            }

            return _values[key];
        }

        public SaveStateContext()
        {

        }

        public SaveStateContext(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            _values = JsonConvert.DeserializeObject<Dictionary<string, object?>>(Encoding.UTF8.GetString(data), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            })!;
        }

        public string ToDataString()
        {
            return JsonConvert.SerializeObject(_values, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            });
        }

        public byte[] ToData()
        {
            return Encoding.UTF8.GetBytes(ToDataString());
        }
    }
}