using System;

namespace MultiType.Exceptions
{
    class BadLessonEntryException : Exception
    {
        new public string Message { get; set; }

        internal BadLessonEntryException(string message)
        {
            Message = message;
        }
    }
}
