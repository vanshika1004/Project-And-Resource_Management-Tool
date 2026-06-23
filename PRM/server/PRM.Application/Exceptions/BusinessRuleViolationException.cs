using System;

namespace PRM.Application.Exceptions;

public class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}
