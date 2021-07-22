using System;

namespace simple.eventbus.Decorators.Requester
{
    public class ReplierException : Exception
    {
        public ReplierException(string message) : base(message)
        {
        }
    }
}