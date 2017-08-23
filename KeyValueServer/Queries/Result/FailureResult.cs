using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Queries.Result
{
    public class FailureResult : CommandResult
    {
        public override CommandStatus Status { get => CommandStatus.Failure; }
        public string ErrorMessage;
    }

}
