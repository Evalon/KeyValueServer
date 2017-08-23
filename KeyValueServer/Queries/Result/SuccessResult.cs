using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Queries.Result
{
    public class SuccessResult<ResultType> : CommandResult
    {
        public override CommandStatus Status => CommandStatus.Success;
        public ResultType Result;
    }
    public class SuccessResult : CommandResult
    {
        public override CommandStatus Status => CommandStatus.Success;
    }
}
