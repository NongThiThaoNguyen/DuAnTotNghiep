using System;

namespace DuAnTotNghiep.Models.Exceptions
{
    public class DataScopeViolationException : Exception
    {
        public DataScopeViolationException(string message) : base(message)
        {
        }
    }
}
