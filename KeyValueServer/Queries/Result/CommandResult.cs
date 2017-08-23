using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Queries
{
    public class CommandResult
    { 
        public enum CommandStatus
        {
            Success,
            Failure
        };
        public virtual CommandStatus Status { get; set; }
    }
}
