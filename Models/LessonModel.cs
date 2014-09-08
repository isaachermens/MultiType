using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MultiType.ViewModels;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;

namespace MultiType.Models
{
	class LessonModel
	{
		private LessonViewModel _viewModel;
		private string _folderPath; // path to the folder containing the executable
		internal LessonModel(LessonViewModel viewModel)
		{
			_viewModel = viewModel;		
			GetLessonNames();
		}

		/// <summary>
		/// Load a list of all lesson names from the Lessons directory located in the same directory as the executable file.
		/// Populate the lesson select combo box with the list of names through a databound property.
		/// If the directory does not exist or some other error occurs, create a default list.
		/// </summary>
		private void GetLessonNames()
		{
			// http://stackoverflow.com/questions/3259583/how-to-get-files-in-a-relative-path-in-c-sharp
			_folderPath = "";
			try //attempt to load a list of all lesson names.
			{
				// Get the absolute path of the lessons directory and load the names of all text files from it.
				var currentProcess = Process.GetCurrentProcess();
				var fileName = currentProcess.MainModule.FileName;
				_folderPath = Path.GetDirectoryName(fileName) + @"\Lessons\";
				var filter = "*.txt"; // we want .txt files
				string[] files = Directory.GetFiles(_folderPath, filter);
				var lessonNames = new string[files.Length + 1]; // holds all lessons plus a default option
				lessonNames[0] = "Select One..."; // default option
				for (var i = 0; i < files.Length; i++)
				{// strip the path and extension from each lesson name.
					lessonNames[i + 1] = Path.GetFileNameWithoutExtension(files[i]);
				}
				_viewModel.LessonNames = lessonNames;
			}
			catch (Exception e)
			{ // set property to default value.
				_viewModel.LessonNames = new string[]{"Select One..."};
			}
		}

		/// <summary>
		/// Read the contents of the .txt file with the specified lesson name. Exceptions are handled internally.
		/// </summary>
		/// <param name="lessonName">Name of the lesson to read text for.</param>
		/// <returns></returns>
		internal string GetLessonText(string lessonName)
		{
			var fileName = lessonName + ".txt";
			var lessonText = "";
			try
			{
				using (StreamReader sr = new StreamReader(_folderPath + fileName))
				{
					lessonText = sr.ReadToEnd();
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
		internal void CreateNewLesson(string lessonName, string lessonText)
		{
			lessonText = Regex.Replace(lessonText, @"\s+", " "); // replace all whitespace characters with single spaces. Prevents double spaces, tabs, linebreaks, etc. from appearing in the lesson.
			var errorString = ""; // initiallize error string.
			if (lessonName.Trim().Equals("") || lessonText.Trim().Equals("")) // if either parameter is empty or whitespace, modify error string
				errorString += "Please enter text for both the name and content of the lesson.\r\n";
			else if (LessonNameInUse(lessonName)) // if the specified lesson name is already being used, modify error string
				errorString += "Enter a lesson name that is not already in use.\r\n";
			else if (IsInvalidFileName(lessonName)) // if the lesson name contains any illegal characters, modify error string
				errorString += "Please enter a file name that does not contain illegal characters: ";
			if (errorString != "") // throw BadLessonEntryException if the error string has been modified
				throw new Exceptions.BadLessonEntryException(errorString);
			if (!Directory.Exists(_folderPath)) // create the lessons directory in the same directory as the executible if it does not already exist
				Directory.CreateDirectory(_folderPath);
			var fullPath = _folderPath + lessonName + ".txt";
			if (File.Exists(fullPath)) // remove file if it already exists, should not take place due to earlier checks
				File.Delete(fullPath);
			using (FileStream fs = File.Create(_folderPath + lessonName + ".txt"))
			{ // create the new .txt file and write the lesson content.
				var text = Encoding.ASCII.GetBytes(lessonText);
				fs.Write(text, 0, text.Length);
			}
			GetLessonNames(); // refresh the drop down list of lesson names
		}

		/// <summary>
		/// Very similar function to CreateLesson() method. Simply allows for modification of a lesson instead of creation of a new lesson.
		/// Process is similar: the old file is deleted and a new one is created with the new lesson text.
		/// </summary>
		/// <param name="oldName">Previous name of the lesson</param>
		/// <param name="newName">Editted of the lesson</param>
		/// <param name="newLessonText">Editted content of the lesson.</param>
		internal void EditLesson(string oldName, string newName, string newLessonText)
		{
			newLessonText = Regex.Replace(newLessonText, @"\s+", " ");
			var errorString = "";
			if (newName.Trim().Equals("") || newLessonText.Trim().Equals(""))
				errorString += "Please enter text for both the name and content of the lesson.\r\n";
			else if (LessonNameInUse(newName) && newName != oldName)
				errorString += "Enter a lesson name that is not already in use.\r\n";
			else if (IsInvalidFileName(newName))
				errorString += "Please enter a file name that does not contain illegal characters: ";
			if (errorString != "")
				throw new Exceptions.BadLessonEntryException(errorString);

			var fullPath = _folderPath + oldName + ".txt";
			if (!Directory.Exists(_folderPath))
				Directory.CreateDirectory(_folderPath);
			if (File.Exists(fullPath))
				File.Delete(fullPath);
			using (FileStream fs = File.Create(_folderPath + newName + ".txt"))
			{
				var text = Encoding.ASCII.GetBytes(newLessonText);
				fs.Write(text, 0, text.Length);
			}
			GetLessonNames();// refresh the drop down list of lesson names
		}

		/// <summary>
		/// Deletes the specified lesson and refreshes the list of lesson names.
		/// </summary>
		/// <param name="lessonName">Name of the lesson to delete.</param>
		internal void DeleteCurrentLesson(string lessonName)
		{
			var fullPath = _folderPath + lessonName + ".txt";
			if (!Directory.Exists(_folderPath))
				Directory.CreateDirectory(_folderPath);
			if (File.Exists(fullPath))
				File.Delete(fullPath);
			GetLessonNames();
		}	

		private bool IsInvalidFileName(string fileName)
		{
			return fileName.IndexOfAny(Path.GetInvalidFileNameChars(), 0, fileName.Length) != -1;
		}

		private bool LessonNameInUse(string lessonName)
		{
			return _viewModel.LessonNames.Contains(lessonName);
		}

		public static ManualResetEvent tcpClientConnected =	new ManualResetEvent(false);

		internal void OpenListenSocket()
		{
			// Create a TCPListener socket to wait for a peer/client to connect
			var listener = new TcpListener(IPAddress.Any, 0);
			listener.Start();
			// extract the port number of the listener
			var endPoint = (IPEndPoint)listener.LocalEndpoint;
			_viewModel.PortNum = endPoint.Port.ToString();
			// get the local computer IPv4 address. This method of retrieving the IP address will have difficulties if a computer
			// has multiple IPv4 addresses
			var hostDns = Dns.GetHostEntry(Dns.GetHostName());
			var ip = hostDns.AddressList.FirstOrDefault(c => c.AddressFamily.ToString().Equals("InterNetwork"));
			if(ip==null) return; //todo throw an exception here?
			_viewModel.IPAddress = ip.ToString(); // set databound IPaddress property
			//Reset the ManualReset event and begin async op to accept connection request
			tcpClientConnected.Reset();
			listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClientConnection), listener);
		}

		public void AcceptClientConnection(IAsyncResult ar)
		{
			var listener = (TcpListener)ar.AsyncState; // cast the result to a TcpListener
			var client = listener.EndAcceptTcpClient(ar); // stop listening for clients
			_viewModel.asyncClient = new SocketsAPI.AsyncTcpClient(client); // create an asynchronous tcp socket using the tcp client socket.
			listener.Stop(); // kill the listener
			tcpClientConnected.Set(); // set the data bound
			_viewModel.ConnectionEstablished = true;
		}
	}
}
