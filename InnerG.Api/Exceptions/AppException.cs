using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InnerG.Api.Exceptions
{
    public abstract class AppException : Exception
    {
        public int StatusCode { get; }
        protected AppException(string message, int statusCode)
                : base(message)
        {
            StatusCode = statusCode;
        }
    }
}