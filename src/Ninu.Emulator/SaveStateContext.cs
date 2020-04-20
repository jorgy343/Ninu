using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Ninu.Emulator
{
    public class SaveStateContext
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public void AddToState(string key, object value) => _values[key] = value;
        public T GetFromState<T>(string key)
        {
            if (typeof(T) == typeof(byte))
            {
                var a = (long)_values[key];
                return (T)(object)(byte)a;
            }
            else if (typeof(T) == typeof(ushort))
            {
                var a = (long)_values[key];
                return (T)(object)(ushort)a;
            }
            else if (typeof(T) == typeof(int))
            {
                var a = (long)_values[key];
                return (T)(object)(int)a;
            }
            else if (typeof(T) == typeof(CpuFlags))
            {
                var a = (long)_values[key];
                return (T)(object)(CpuFlags)a;
            }

            return (T)_values[key];
        }

        public SaveStateContext()
        {

        }

        public SaveStateContext(byte[] data)
        {
            _values = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(data), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            });

            //_values = JsonSerializer.Deserialize<Dictionary<string, object>>(Encoding.UTF8.GetString(data));
        }

        public string ToDataString()
        {
            return JsonConvert.SerializeObject(_values, Formatting.None, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            });

            //return JsonSerializer.Serialize(_values, new JsonSerializerOptions
            //{
            //    WriteIndented = false,
            //});
        }

        public byte[] ToData()
        {
            return Encoding.UTF8.GetBytes(ToDataString());
        }
    }
}