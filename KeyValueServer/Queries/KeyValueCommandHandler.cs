using KeyValueServer.Queries;
using KeyValueServer.Queries.Result;
using KeyValueServer.Repositories.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Repositories
{
    /// <summary>
    /// KeyValueCommandHandler is service over IKeyValueRepository,
    /// handles all of the buisness logic operations of KeyValue store.
    /// </summary>
    public class KeyValueCommandHandler : ICommandHandler
    {
        /// <summary>
        /// KeyValue repository where Key and Value string types. 
        /// Using DI container. Repository pattern.
        /// </summary>
        IKeyValueRepository _store;
        public KeyValueCommandHandler(IKeyValueRepository repository)
        {
            _store = repository;
        }
        /// <summary>
        /// Redirects commands to corresponding method.
        /// </summary>
        /// <param name="command">Command that will be executed.</param> 
        /// <returns>Result of command execution or message with error status</returns>
        public CommandResult Handle(Command command)
        {
            switch (command.Type)
            {
                case Command.CommandType.Create:
                    return Create(command.Key, command.Value);
                case Command.CommandType.Read:
                    return Read(command.Key);
                case Command.CommandType.ReadAll:
                    return ReadAll();
                case Command.CommandType.Update:
                    return Update(command.Key, command.Value);
                case Command.CommandType.Delete:
                    return Delete(command.Key);
                default:
                    return new FailureResult();
            }
        }
        /// <summary>
        /// Fetch and return all keys in result
        /// </summary>
        private CommandResult ReadAll()
        {
            return new SuccessResult<ICollection<string>>
            {
                Result = _store.GetAllKeys()
            };
        }
        /// <summary>
        /// Fetch and return specific key in result
        /// </summary>
        private CommandResult Read(string key)
        {
            try
            {
                if (key is null) throw new ArgumentNullException("key");
                return new SuccessResult<string>
                {
                    Result = _store.GetValue(key)
                };
            }
            catch (KeyNotFoundInRepositoryException)
            {
                return new FailureResult
                {
                    ErrorMessage = "key not found"
                };
            }
            catch (ArgumentNullException e)
            {
                return new FailureResult
                {
                    ErrorMessage = e.ParamName +" is required"
                }; 
            }

        }
        /// <summary>
        /// Store KeyValue and return success result
        /// </summary>
        private CommandResult Create(string key, string value)
        {
            try
            {
                if (key is null || key.Trim().Length == 0) throw new ArgumentNullException("key");
                if (value is null) throw new ArgumentNullException("value");
                _store.SetValue(key, value);
                return new SuccessResult();
            }
            catch (ArgumentNullException e)
            {
                return new FailureResult
                {
                    ErrorMessage = e.ParamName + " is required"
                };
            }

        }
        /// <summary>
        /// Update KeyValue only if specific key already exist.
        /// Return error message if not.
        /// </summary>
        private CommandResult Update(string key, string value)
        {
            try
            {
                if (key is null || key.Trim().Length == 0) throw new ArgumentNullException("key");
                if (value is null) throw new ArgumentNullException("value");                
                _store.UpdateValue(key, value);
            }
            catch (KeyNotFoundInRepositoryException)
            {
                return new FailureResult
                {
                    ErrorMessage = "key not found"
                };
            }
            catch (ArgumentNullException e)
            {
                return new FailureResult
                {
                    ErrorMessage = e.ParamName + " is required"
                };
            }
            return new SuccessResult();
        }
        /// <summary>
        /// Delete existed KeyValue. If key is not found return error message.
        /// </summary>
        private CommandResult Delete(string key)
        { 
            try
            {
                if (key is null) throw new ArgumentNullException("key");
                _store.DeleteValue(key);
            }
            catch (KeyNotFoundInRepositoryException)
            {
                return new FailureResult
                {
                    ErrorMessage = "key not found"
                };
            }
            catch (ArgumentNullException e)
            {
                return new FailureResult
                {
                    ErrorMessage = e.ParamName + " is required"
                };
            }
            return new SuccessResult();

        }
    }
}
