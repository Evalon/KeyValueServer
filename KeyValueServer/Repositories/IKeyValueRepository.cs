using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Repositories
{
    /// <summary>
    /// Common interface for KeyValue repositories.
    /// </summary>
    public interface IKeyValueRepository
    {
        ICollection<string> GetAllKeys();
        string GetValue(string key);
        void SetValue(string key, string value);
        void UpdateValue(string key, string value);
        void DeleteValue(string key);
    }
}
