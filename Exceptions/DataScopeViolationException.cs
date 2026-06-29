using System;

namespace DuAnTotNghiep.Exceptions
{
    public class DataScopeViolationException : Exception
    {
        public DataScopeViolationException(string message) : base(message)
        {
        }
    }
}
