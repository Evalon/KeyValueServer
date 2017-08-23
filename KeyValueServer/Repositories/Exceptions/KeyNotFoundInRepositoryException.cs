using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyValueServer.Repositories.Exceptions
{
    public class KeyNotFoundInRepositoryException : Exception
    {
        
        public KeyNotFoundInRepositoryException()
        {
        }

        public KeyNotFoundInRepositoryException(string message = "key not found") : base(message)
        {
        }
    }
}
