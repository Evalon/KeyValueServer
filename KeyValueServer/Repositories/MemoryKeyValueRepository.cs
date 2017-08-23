using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Repositories
{
    public class MemoryKeyValueRepository : IKeyValueRepository
    {
        private readonly ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>();
        public ICollection<string> GetAllKeys() => 
            _store.Keys;

        public string GetValue(string key)
        {
            if(_store.TryGetValue(key, out string value))
                return value;
            throw new Exceptions.KeyNotFoundInRepositoryException("Key Not Found");
        }

        public void SetValue(string key, string value)
        {
            _store.AddOrUpdate(key, value, (k, v) => value);
        }

        public void UpdateValue(string key, string value)
        {
            if (!_store.ContainsKey(key))
                throw new Exceptions.KeyNotFoundInRepositoryException("Key Not Found");
            SetValue(key, value);
        }

        public void DeleteValue(string key)
        {
            if (!_store.TryRemove(key, out string value))
                throw new Exceptions.KeyNotFoundInRepositoryException("Key Not Found");
        }

    }
}
