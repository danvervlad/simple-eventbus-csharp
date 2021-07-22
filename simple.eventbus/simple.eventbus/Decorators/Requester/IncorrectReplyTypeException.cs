using System;

namespace simple.eventbus.Decorators.Requester
{
    public class IncorrectReplyTypeException : Exception
    {
        public IncorrectReplyTypeException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}