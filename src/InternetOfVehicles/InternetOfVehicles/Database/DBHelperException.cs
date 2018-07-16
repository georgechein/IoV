using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DBLibrary.Database
{
    [Serializable]
    public class DBHelperException : Exception
    {
        public DBHelperException() : base() { }

        public DBHelperException(string message) : base(message) { }

        public DBHelperException(string format, params object[] args) : base(string.Format(format, args)) { }

        public DBHelperException(string message, Exception innerException) : base(message, innerException) { }

        public DBHelperException(string format, Exception innerException, params object[] args) : base(string.Format(format, args), innerException) { }
    }
}