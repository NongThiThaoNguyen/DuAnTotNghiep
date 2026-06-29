using System;

namespace DuAnTotNghiep.Models.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
