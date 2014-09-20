using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MultiType.Services
{
    public interface ILessonManagementService
    {
        /// <summary>
        /// Load a list of all lesson names from the Lessons directory located in the same directory as the executable file.
        /// Populate the lesson select combo box with the list of names through a databound property.
        /// If the directory does not exist or some other error occurs, create a default list.
        /// </summary>
        List<string> GetLessonNames();

        /// <summary>
        /// Read the contents of the .txt file with the specified lesson name. Exceptions are handled internally.
        /// All white space characters in the file will be replaced with single spaces.
        /// </summary>
        /// <param name="lessonName">Name of the lesson to read text for.</param>
        /// <returns></returns>
        string GetLessonText(string lessonName);

        /// <summary>
        /// Create a new lesson with the provided name and content.
        /// Throws BadLessonEntryException if either parameter is invalid.
        /// </summary>
        /// <param name="lessonName">Name of lesson to create.</param>
        /// <param name="lessonText">Content of lesson to create.</param>
        void CreateNewLesson(string lessonName, string lessonText);

        /// <summary>
        /// Very similar function to CreateLesson() method. Simply allows for modification of a lesson instead of creation of a new lesson.
        /// Process is similar: the old file is deleted and a new one is created with the new lesson text.
        /// </summary>
        /// <param name="oldName">Previous name of the lesson</param>
        /// <param name="newName">Editted of the lesson</param>
        /// <param name="newLessonText">Editted content of the lesson.</param>
        void EditLesson(string oldName, string newName, string newLessonText);

        /// <summary>
        /// Deletes the specified lesson
        /// </summary>
        /// <param name="lessonName">Name of the lesson to delete.</param>
        void DeleteLesson(string lessonName);
    }

    public class LessonManagementService : ILessonManagementService
    {
		private string _folderPath; // path to the folder containing the executable

		/// <summary>
		/// Load a list of all lesson names from the Lessons directory located in the same directory as the executable file.
		/// Populate the lesson select combo box with the list of names through a databound property.
		/// If the directory does not exist or some other error occurs, create a default list.
		/// </summary>
		public List<string> GetLessonNames()
		{
			// http://stackoverflow.com/questions/3259583/how-to-get-files-in-a-relative-path-in-c-sharp
			_folderPath = "";
			try //attempt to load a list of all lesson names.
			{
				// Get the absolute path of the lessons directory and load the names of all text files from it.
				var currentProcess = Process.GetCurrentProcess();
				var fileName = currentProcess.MainModule.FileName;
				_folderPath = Path.GetDirectoryName(fileName) + @"\Lessons\";
				const string filter = "*.txt"; // we want only .txt files
				var files = Directory.GetFiles(_folderPath, filter).ToList();
				var lessonNames = new List<string> {"Select One..."}; // holds all lessons plus a default option
			    files.ForEach(c => lessonNames.Add(Path.GetFileNameWithoutExtension(c)));
				return lessonNames;
			}
			catch (Exception e)
			{ // set property to default value.
				return new List<string>{"Select One..."};
			}
		}

		/// <summary>
		/// Read the contents of the .txt file with the specified lesson name. Exceptions are handled internally.
		/// All white space characters in the file will be replaced with single spaces.
		/// </summary>
		/// <param name="lessonName">Name of the lesson to read text for.</param>
		/// <returns></returns>
		public string GetLessonText(string lessonName)
		{
			var fileName = lessonName + ".txt";
			var lessonText = "";
			try
			{
				using (var reader = new StreamReader(_folderPath + fileName))
				{
					lessonText = reader.ReadToEnd();
					lessonText = Regex.Replace(lessonText, @"\s+", " ");
				}
			}
			catch (Exception e)
			{ // return error text instead of lesson text
				return "An error has occured. " + lessonName + " could not be opened. Please try a different file";
			}
			return lessonText+"\r\n"; // append an extra new line to the lesson so that the overlayed textblocks and RTB display properly at the end of the lesson
		}

		/// <summary>
		/// Create a new lesson with the provided name and content.
		/// Throws BadLessonEntryException if either parameter is invalid.
		/// </summary>
		/// <param name="lessonName">Name of lesson to create.</param>
		/// <param name="lessonText">Content of lesson to create.</param>
		public void CreateNewLesson(string lessonName, string lessonText)
		{
			lessonText = Regex.Replace(lessonText, @"\s+", " "); // replace all whitespace characters with single spaces. Prevents double spaces, tabs, linebreaks, etc. from appearing in the lesson.
			var errorString = ""; // initiallize error string.
			if (lessonName.Trim().Equals("") || lessonText.Trim().Equals("")) // if either parameter is empty or whitespace, modify error string
				errorString += "Please enter text for both the name and content of the lesson.\r\n";
			else if (LessonNameInUse(lessonName)) // if the specified lesson name is already being used, modify error string
				errorString += "Enter a lesson name that is not already in use.\r\n";
			else if (!IsValidFileName(lessonName)) // if the lesson name contains any illegal characters, modify error string
				errorString += "Please enter a file name that does not contain illegal characters: ";
			if (errorString != "") // throw BadLessonEntryException if the error string has been modified
				throw new Exceptions.BadLessonEntryException(errorString);
			if (!Directory.Exists(_folderPath)) // create the lessons directory in the same directory as the executible if it does not already exist
				Directory.CreateDirectory(_folderPath);
			var fullPath = _folderPath + lessonName + ".txt";
			if (File.Exists(fullPath)) // remove file if it already exists, should not take place due to earlier checks
				File.Delete(fullPath);
			using (var file = File.Create(_folderPath + lessonName + ".txt"))
			{ // create the new .txt file and write the lesson content.
				var text = Encoding.ASCII.GetBytes(lessonText);
				file.Write(text, 0, text.Length);
			}
		}

		/// <summary>
		/// Very similar function to CreateLesson() method. Simply allows for modification of a lesson instead of creation of a new lesson.
		/// Process is similar: the old file is deleted and a new one is created with the new lesson text.
		/// </summary>
		/// <param name="oldName">Previous name of the lesson</param>
		/// <param name="newName">Editted of the lesson</param>
		/// <param name="newLessonText">Editted content of the lesson.</param>
		public void EditLesson(string oldName, string newName, string newLessonText)
		{
			newLessonText = Regex.Replace(newLessonText, @"\s+", " ");
			var errorString = "";
			if (newName.Trim().Equals("") || newLessonText.Trim().Equals(""))
				errorString += "Please enter text for both the name and content of the lesson.\r\n";
			else if (LessonNameInUse(newName) && newName != oldName)
				errorString += "Enter a lesson name that is not already in use.\r\n";
			else if (!IsValidFileName(newName))
				errorString += "Please enter a file name that does not contain illegal characters: ";
			if (errorString != "")
				throw new Exceptions.BadLessonEntryException(errorString);

			var fullPath = _folderPath + oldName + ".txt";
			if (!Directory.Exists(_folderPath))
				Directory.CreateDirectory(_folderPath);
			if (File.Exists(fullPath))
				File.Delete(fullPath);
			using (var fileStream = File.Create(_folderPath + newName + ".txt"))
			{
				var text = Encoding.ASCII.GetBytes(newLessonText);
				fileStream.Write(text, 0, text.Length);
			}
		}

		/// <summary>
		/// Deletes the specified lesson
		/// </summary>
		/// <param name="lessonName">Name of the lesson to delete.</param>
		public void DeleteLesson(string lessonName)
		{
			var fullPath = _folderPath + lessonName + ".txt";
			if (!Directory.Exists(_folderPath))
				Directory.CreateDirectory(_folderPath);
			if (File.Exists(fullPath))
				File.Delete(fullPath);
		}	

		private static bool IsValidFileName(string fileName)
		{
			return fileName.IndexOfAny(Path.GetInvalidFileNameChars(), 0, fileName.Length) != -1;
		}

		private bool LessonNameInUse(string lessonName)
		{
			return GetLessonNames().Contains(lessonName);
		}
	}
}
