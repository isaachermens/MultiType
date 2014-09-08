using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
