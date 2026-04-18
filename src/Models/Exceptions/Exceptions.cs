using System;

namespace Consensus.Models.Exceptions;

public class NotYetInitializationException : Exception
{
    public NotYetInitializationException() {}
    public NotYetInitializationException(string message) : base(message) {}
    public NotYetInitializationException(string message, Exception inner) : base(message, inner) {}

}