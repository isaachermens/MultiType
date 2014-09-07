using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiType.Resources
{
	class BadLessonEntryException: Exception
	{
		public string Message { get; set; }
		internal BadLessonEntryException(string message)
		{
			Message = message;
		}
	}

	/// <summary>
	/// Used to return results from dialog windows in the user interace
	/// </summary>
	internal enum DialogResult { Menu, Repeat, New };

}
