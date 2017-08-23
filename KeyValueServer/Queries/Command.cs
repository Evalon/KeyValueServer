using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Queries
{

    public class Command
    {
        public enum CommandType
        {
            Create,
            Read,
            ReadAll,
            Update,
            Delete,
        }
        public CommandType Type;
        public string Key;
        public string Value;
    }
}
