using System;

namespace DuAnTotNghiep.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
