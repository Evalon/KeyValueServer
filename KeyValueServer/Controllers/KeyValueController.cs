using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KeyValueServer.Repositories;
using KeyValueServer.Queries;
using System.Web;

namespace KeyValueServer.Controllers
{
    /// <summary>
    /// Controller for KeyValue REST API
    /// Responsible for receive and send Commands in REST way
    /// </summary>
    [Route("api/[controller]")]
    public class KeyValueController : Controller
    {
        /// <summary>
        /// Command Handler
        /// Responsible for handle commands (~Service layer)
        /// </summary>
        private ICommandHandler _commandHandler;
        public KeyValueController(ICommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }
        private IActionResult HandleCommand(Command command)
        {
            var result = _commandHandler.Handle(command);
            if(result.Status == CommandResult.CommandStatus.Success)
                return Json(result);
            return BadRequest(result);
        }
        /// <summary>
        /// GET /
        /// Return all keys
        /// </summary>
        [HttpGet]
        public IActionResult GetAll()
        {

            var command = new Command() { Type = Command.CommandType.ReadAll };
            return HandleCommand(command);
        }
        /// <summary>
        /// GET /{key}
        /// Return value by key
        /// </summary>
        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            var command = new Command()
            {
                Type = Command.CommandType.Read,
                Key = HttpUtility.UrlDecode(key)
            };
            return HandleCommand(command);
        }
        /// <summary>
        /// POST /
        /// Create new KeyValue. Fully replace KeyValue if exists.
        /// </summary>
        [HttpPost]
        public IActionResult Create([FromBody]Command command)
        {
            if(command is null) command = new Command();
            return HandleCommand(command);
        }
        /// <summary>
        /// PUT /{key}
        /// Create new KeyValue. Fully replace KeyValue if exists.
        /// </summary>
        [HttpPut("{key}")]
        public IActionResult Update(string key, [FromBody]Command reqCommand)
        {
            if (reqCommand is null) reqCommand = new Command();
            var command = new Command()
            {
                Type = Command.CommandType.Update,
                Key = HttpUtility.UrlDecode(key),
                Value = reqCommand.Value
            };
            return HandleCommand(command);
        }
        /// <summary>
        /// DELETE /{key}
        /// Delete KeyValue by key.
        /// </summary>
        [HttpDelete("{key}")]
        public IActionResult Delete(string key)
        {
            var command = new Command()
            {
                Type = Command.CommandType.Delete,
                Key = HttpUtility.UrlDecode(key)
            };
            return HandleCommand(command);
        }

        [HttpPost("/api/keyvalue-actions")]
        public IActionResult Actions([FromBody]IEnumerable<Command> commands)
        {
            if (commands is null) commands = new List<Command> { new Command() };
            return Json(commands.Select(_commandHandler.Handle));
        }
    }
}
