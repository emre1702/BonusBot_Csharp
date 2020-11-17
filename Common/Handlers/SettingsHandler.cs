using System;
using System.Configuration;
using BonusBot.Common.Interfaces;

namespace BonusBot.Common.Handlers
{
    public sealed class SettingsHandler : IHandler
    {
        public T Get<T>(string key, out bool loadSuccessful)
        {
            string value = ConfigurationManager.AppSettings[key];
            object result = default(T);
            loadSuccessful = false;
            if (value == null)
            {
                return (T)Convert.ChangeType(result, typeof(T));
            }

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    if (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                        loadSuccessful = true;
                    } 
                    else if (value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        result = false;
                        loadSuccessful = true;
                    }
                    break;
                case TypeCode.Char:
                    result = value.Substring(0, 1);
                    loadSuccessful = true;
                    break;
                case TypeCode.Int32:
                    if (int.TryParse(value, out int i))
                    {
                        result = i;
                        loadSuccessful = true;
                    }
                    break;
                case TypeCode.String:
                    result = value;
                    loadSuccessful = true;
                    break;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public T Get<T>(string key)
        {
            return Get<T>(key, out bool _);
        }

        public T GetOrKey<T>(string key)
        {
            T value = Get<T>(key, out bool couldLoad);
            if (!couldLoad)
                return (T)Convert.ChangeType(key, typeof(T));
            return value;
        }
    }
}
