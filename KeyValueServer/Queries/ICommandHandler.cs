using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Queries
{
    public interface ICommandHandler
    {
        CommandResult Handle(Command command);
    }
}
