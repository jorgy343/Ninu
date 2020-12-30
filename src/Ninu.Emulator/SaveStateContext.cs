using Newtonsoft.Json;
using Ninu.Emulator.CentralProcessor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ninu.Emulator
{
    public class SaveStateContext
    {
        private readonly Dictionary<string, object?> _values = new();

        public SaveStateContext()
        {

        }

        public SaveStateContext(byte[] data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            _values = JsonConvert.DeserializeObject<Dictionary<string, object?>>(Encoding.UTF8.GetString(data), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            })!;
        }

        public void AddToState(string key, object? value) => _values[key] = value;

        public bool TryGetFromState(string key, Type type, out object? value)
        {
            if (!_values.ContainsKey(key))
            {
                value = null;
                return false;
            }

            if (type == typeof(byte))
            {
                var a = (long)_values[key]!;
                value = (byte)a;
            }
            else if (type == typeof(ushort))
            {
                var a = (long)_values[key]!;
                value = (ushort)a;
            }
            else if (type == typeof(int))
            {
                var a = (long)_values[key]!;
                value = (int)a;
            }
            else if (type == typeof(CpuFlags))
            {
                var a = (long)_values[key]!;
                value = (CpuFlags)a;
            }
            else
            {
                value = _values[key];
            }

            return true;
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