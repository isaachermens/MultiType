using System;
using System.Diagnostics;
using System.Timers;
using MultiType.SocketsAPI;
using MultiType.ViewModels;
using System.Windows.Documents;
using System.Windows.Media;

namespace MultiType.Models
{
	internal class PrimaryModel
	{
		#region Private Fields

		internal string _typedContent; // stores the currently typed material
		internal string _lessonString; // stores the lesson content
		internal int _WPM; // tracks WPM
		internal int _errors; // stores the current number of errors
		internal int _charactersTyped; // stores the total number of characters typed; never decremented, only incremented
		internal string _completionPercent; // stores the display string for % completion
		internal int _totalErrors; // used to track the total number of errors comitted; never decremented, only incremented
		internal string _accuracy; // stores the display string for accuracy percentage
		internal string _timeElapsed; // stores the time elapsed

		internal int _lessonLength; // stores the length of the lesson, adjusted to remove the trailing CRLF
		private int _previousContentLength; // stores the length of the typed content at the end of the previous call to the UpdateStatistics() method
		private string _previousTypedContent; // stores the state of the typed content at the end of hte previous call to the UpdateStatistics() method
        internal Timer _timer; // timer used to track WPM and send packets to the other user (in multiplayer)
		private Timer _racerTimer; // timer used to trigger racer events in single player
		private int _racerSpeed; // stores the racer speed provided from the lesson select window so that it can be used to reinitiallize the game
		private int _currentRacerIndex; // stores the current location of the racer in single player games
		internal Stopwatch _stopwatch; // stopwatch used to track time elapsed and calculate WPM
		private PrimaryViewModel _viewModel; // reference to the view model
		private AsyncTcpClient _socket; // stores the asynchronous TcpClient used to send and receive data to peer in multiplayer
		private bool _isMulti; // stores whether the game is multiplayer or not
		private bool _isServer; // for multiplayer games, stores whether this is the host or client
		private bool _initiatedPause; // tracks whether this instance initiated a pause in the game.
		internal string _adjustedLessonString; // the lesson content with trailing whitespace removed.
        private bool _lastCharError; // tracks whether or not the last character inputted was an error
		private bool _lastCharEdit; // tracks whether or not the last action performed removed characters from the lesson string

		#endregion 

		internal PrimaryModel(PrimaryViewModel viewModel, bool isServer, int racerSpeed, AsyncTcpClient socket = null)
		{
			_viewModel = viewModel;
			_racerSpeed = racerSpeed;
			if (socket == null)
				_isMulti = false;
			else
			{
				_isMulti = true;
				_socket = socket;
				_socket._model = this;
				_socket._viewModel = _viewModel;
				_isServer = isServer;
			}
		}
		internal void InitiallizeModel(bool isReinitialize=false)
		{	
			//Initiate the stopwatch used to track time elapsed.
            _stopwatch = new Stopwatch();
			// Initiallize the timer used to track WPM and transmit UserStatistics packets
			_timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            _timer.Interval = 500;
			// initiate private fields referred to by databound properties and used for calculations
            _typedContent = "";
			_previousContentLength = 0;
			_previousTypedContent = "";
			_lastCharError = false;
			_lastCharEdit = false;
			_totalErrors = 0;
			_previousTypedContent = "";
            if (!_isMulti)
			{ // set up the racer for single player games
				_currentRacerIndex = 0;
				_racerTimer = new Timer();
				var cpm = _racerSpeed * 5;
				var millisecondsPerChar = 60000.0 / cpm;
				_timer.Elapsed+=new ElapsedEventHandler(RacerTrigger);
				_timer.Interval = millisecondsPerChar;
			}
			else if (_isServer)
			{ // if this is the server, transmit the lesson text to the client
                SendLessonText(_lessonString);
				// start asynchronous read operations on the tcp socket
				_socket.BeginReading();
			}
			else if(!isReinitialize)
			{ // If this is the client and this is a fresh game (no lessons have been completed yet
				// begin reading from the socket and wait to receive the lesson from the host
				_socket.BeginReading();
				_lessonLength = 0;
				while (_lessonLength == 0)
				{
					var read = _socket.readData;
					if(read != null && read.IsLessonText)
						_viewModel.LessonString = ((LessonText)read).Lesson;
				}
			}
		}

		/// <summary>
		/// Event handler for the racer timer
		/// </summary>
		private void RacerTrigger(object source, ElapsedEventArgs e)
		{
			if (_currentRacerIndex >= _lessonLength)
			{ // stop the racer if we have completed the lesson
				_racerTimer.Stop();
				return;
			}
			// append  the next character in the lesson to the racer textblock and advance the racer's position
			_viewModel.PeerTypedContent += _viewModel.LessonString[_currentRacerIndex];
			_currentRacerIndex++;
		}

		/// <summary>
		/// Event handler for the primary timer.
		/// </summary>
		private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
			// calculate elapsed time using milliseconds from the stopwatch
			var milliseconds = _stopwatch.ElapsedMilliseconds;
            var seconds = milliseconds / 1000;
            var minutes = seconds / 60;
            seconds %= 60;
            var secondsString = seconds.ToString();
            if (secondsString.Length == 1)
                secondsString = "0" + secondsString;
            _viewModel.TimeElapsed =  minutes + ":" + secondsString;

            //Calculate WPM using typed content and milliseconds from the stopwatch
            var wpmMinutes = (double)_stopwatch.ElapsedMilliseconds / 60000;
			if (_typedContent == null) return;
            var words = (double)_typedContent.Length / 5; //A word is defined as 5 characters
            if (words != 0.0 && wpmMinutes >= .01)
                _viewModel.WPM = Math.Floor(words / wpmMinutes).ToString();
            else
                _viewModel.WPM = "0";
			if(_isMulti) SendStatsPacket(); // in multiplayer games, send a stats packet to your peer.
        }

		/// <summary>
		/// Triggered each time a character is entered or removed.
		/// </summary>
		/// <param name="content">the current content of the text entry field</param>
		internal void UpdateStatistics(string content)
		{
			_typedContent = content; //store content into an object field
			//If a new character has been typed (as opposed to character(s) deleted).
			if (_typedContent.Length > _previousContentLength)
			{
				_viewModel.CharactersTyped = (_charactersTyped + 1).ToString(); // increment the number of characters typed
				// prevent crashes from overrunning the lesson content
				if (_typedContent.Length > _lessonLength) return;
				var correct = _lessonString[_typedContent.Length - 1]; // retrieve the correct value from the lesson string
				var typed = _typedContent[_typedContent.Length - 1]; // retrive the value that was typed from the typed material
				if (typed!=correct)
				{ // if an error has been entered, increment both error counters and check if highlighting needs to occur
					_viewModel.Errors = (_errors + 1).ToString();
					_totalErrors++;
					if (_lastCharError == false)
					{ // if this is not a repeated error, 
						var document = _viewModel._userInput.Document;
						// get a text pointer to a range that includes the last character typed. The incoming text has a CRLF at the end from the RTB
						var startPointer = document.ContentEnd.GetPositionAtOffset(-3);
						if (startPointer == null || document.ContentEnd == null) return;
						var textRange = new TextRange(startPointer, document.ContentEnd);
						// change the text color of the error to white and highlight it in red
						textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
						textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Crimson);
						_lastCharError = true; // set the flag indicating that the most recently typed character was an error
					}
				}
				else if (_lastCharError || _lastCharEdit) // if the previous input was an error or edit, restore text colors to their normal values
				{
					var document = _viewModel._userInput.Document;
					var startPointer = document.ContentEnd.GetPositionAtOffset(-3);
					if (startPointer == null || document.ContentEnd == null) return;
					var textRange = new TextRange(startPointer, document.ContentEnd);
					textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
					textRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);
					_lastCharError = false; // deactivate the flags
					_lastCharEdit = false;
				}
			}
			else // character(s) have been deleted.
			{
				// start 1 position after the end of the current typed content, loop through until the end of the previous typed content
				// comparing the previous content to the lesson string, decrement errors for each error removed
				for (var x = _typedContent.Length; x < _previousContentLength; x++)
				{
					if (_previousTypedContent[x] != _lessonString[x])
						_errors--;
				}
				// some form of bug is occasionally causing a negative error count.
				// I can't find the bug or reproduce it at will, so this is a stopgap measure.
				if (_errors < 0)
					_errors = 0;
				_viewModel.Errors = _errors.ToString();
				_lastCharEdit = true; // set flag to indicate that text has been deleted
			}
			_previousContentLength = _typedContent.Length;  //update the value for the next round
			_previousTypedContent = _typedContent; // transfer the current typed content into a state variable for possible use in the next call to this method
			var thread1 = new System.Threading.Thread(()=>CalculateAccuracy()); // kick off a new thread to calculate accuracy
            thread1.Start();
			var thread2 = new System.Threading.Thread(()=>CalculateCompletionPercentage()); // kick off a new thread to calculate completion percentage
            thread2.Start();
		}

		internal void CalculateCompletionPercentage()
		{
			// Determine whether or not the user has finished, and calculate the completion percentage,
            _viewModel.CompletionPercentage = _typedContent.Length + "/" + _lessonLength + " = " + Math.Floor(
                ((double)_typedContent.Length / _lessonLength) * 100) + "% Complete";
            if (_typedContent.Length >= _lessonLength)
            { // user is finished
				GameIsComplete();
            }
		}

		// can be called by a local completion or by reception of a packet from the other player indicating completion
		internal void GameIsComplete(bool isLocalCall=true)
		{
			_timer.Stop();
			_stopwatch.Stop();
			if (_isMulti)
			{
				SendStatsPacket();
				if(isLocalCall)
					SendCompletedCommand();
			}
			_viewModel.GameIsComplete();
		}
		internal void CalculateAccuracy()
		{
			//Calculate the user's accuracy. Determind by dividing the total number of characters they have typed correctly by the total number of characters typed
            var difference = _charactersTyped - _totalErrors;
            if(_typedContent.Length>0)
                _viewModel.Accuracy = difference + "/" + _charactersTyped + " = " + (Math.Floor(((double)(difference) / _charactersTyped) 
                    * 100)).ToString() + "%";
            else
                _viewModel.Accuracy="0/0 = 0%";
		}

		/// <summary>
		/// If the game is not paused, pauses. Otherwise, unpauses.
		/// </summary>
		/// <returns>A boolean value indicating whether or not the game is paused at the end of the method.</returns>
		internal bool TogglePause(bool isLocalCall)
		{
			if (_isMulti) // divert the call to a special function in multiplayer games
				return TogglePauseMulti(isLocalCall);
			var currentState = _stopwatch.IsRunning;
			bool gameIsPaused;
			if (currentState)
			{ //if the game is running, stop all timers/stopwatches
				_stopwatch.Stop();
				_timer.Stop();
				_racerTimer.Stop();
				gameIsPaused = true;
			}
			else
			{
				_timer.Start();
				_stopwatch.Start();
				_racerTimer.Start();
				gameIsPaused = false;
			}
			_viewModel.RTBReadOnly = gameIsPaused;
			return gameIsPaused;	
		}

		/// <summary>
		/// Handle pause/unpause events in multiplayer games
		/// </summary>
		/// <param name="isLocalCall">Did the call originate from a local pause attempt of a received packet?</param>
		/// <returns></returns>
		internal bool TogglePauseMulti(bool isLocalCall)
		{
			if (_viewModel.gameHasStarted == false) return true;
			if (isLocalCall) SendPauseCommand(); // only send a pause command if the pause was initiated here
			
			var currentState = _stopwatch.IsRunning;
			bool gameIsPaused;
			if (currentState) // game is running, stop it and open the popup through databindings
			{
				_initiatedPause = isLocalCall;
				_stopwatch.Stop();
				_timer.Stop();
				gameIsPaused = true;
				// show the popup and populate the message
				_viewModel.ShowPopup = true;
				_viewModel.StaticPopupText = "Game paused. The player who initiated the pause may resume by pressing the pause button again.";
			}
			else //game is not running 
			{
				// player did not initiate the pause, return true (game still paused). Game can only be unpaused by the person who paused it
				if ((!_initiatedPause && isLocalCall) || (_initiatedPause && !isLocalCall))
					return true;
				// set the static text on the popup
				_viewModel.StaticPopupText = "Game has been unpaused. Will resume in:";
				// initiate a stopwatch and run it for 5 seconds. While it is running, update the countdown displayed in the popup				
				var stopwatch = new Stopwatch();
				stopwatch.Start();
				while (stopwatch.ElapsedMilliseconds < 5000)
				{
					var seconds = Math.Round(5 - stopwatch.ElapsedMilliseconds / 1000.0, 2).ToString();
					if (seconds.Length == 1)
						seconds += ".0";
					_viewModel.PopupCountdown = seconds + " seconds";
					System.Threading.Thread.Sleep(100); // sleep the thread for a short time simply to reduce unnecessary overhead from this loop
				}
				// hide the popup and clear its text once the delay is complete
				_viewModel.ShowPopup = false; 
				_viewModel.StaticPopupText = "";
				_viewModel.PopupCountdown = "";
				// resume the game
				_timer.Start();
				_stopwatch.Start();
				gameIsPaused = false;
				_initiatedPause = false; //reset the value of initiated pause since the game has been resumed
			}
			return gameIsPaused;	
		}

		/// <summary>
		/// Begin the game after a delay of 5 seconds
		/// </summary>
		/// <param name="isLocalCall">Is this being called from the view model or upon receipt of a start command from the host?</param>
		internal void StartGame(bool isLocalCall)
		{
			if (isLocalCall) // only want to send a start command packet if this is called locally, not upon received of a packet
				SendStartCommand();
			_viewModel.ShowPopup = false;
			_viewModel.ShowPopup = true;
			_viewModel.StaticPopupText = "The game has been started. Activating in:";
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			while(stopwatch.ElapsedMilliseconds < 5000)
			{
				var seconds = Math.Round(5 - stopwatch.ElapsedMilliseconds/1000.0, 3);
				_viewModel.PopupCountdown = seconds.ToString("F2") + " seconds";
				System.Threading.Thread.Sleep(93);
			}
			stopwatch.Stop();
			_viewModel.ShowPopup = false;
			_viewModel.StaticPopupText = "";
			_viewModel.PopupCountdown = "";
			_stopwatch.Start();
			_timer.Start();
			_viewModel.gameHasStarted = true;
			_viewModel.RTBReadOnly = false;
		}

		#region Packet Send Methods

		/// <summary>
		/// Create and send a Stats update packet to the peer.
		/// </summary>
		internal void SendStatsPacket()
		{
			var StatsSnapshot = new UserStatistics
			{
				IsCommand = false,
				IsLessonText = false,
				IsUserStatictics = true,
				CompletionPercentage = _completionPercent,
				TypedContent = _typedContent,
				CharactersTyped = _charactersTyped,
				Accuracy = _accuracy,
				Errors = _errors,
				WPM = _WPM
			};
			var packet = Serializer.SerializeToByteArray(StatsSnapshot);
			_socket.Write(packet);
		}

		/// <summary>
		/// Create and send a command to start the game to your client.
		/// </summary>
		private void SendStartCommand()
		{
			var startCommand = new Command
			{
				IsCommand = true,
				IsLessonText = false,
				IsUserStatictics = false,
				IsPauseCommand = false,
				//PauseGame = gameIsPaused,
				StartCommand=true
			};
			_socket.Write(startCommand);
		}

		/// <summary>
		/// Create and send a command to pause the game to your peer.
		/// </summary>
		private void SendPauseCommand()
		{
			var pauseCommand = new Command
			{
				IsCommand = true,
				IsLessonText = false,
				IsUserStatictics = false,
				IsPauseCommand = true,
				//PauseGame = gameIsPaused,
				IsGameComplete = false,
				GameHasStarted = _viewModel.gameHasStarted,
			};
			_socket.Write(pauseCommand);
		}

		/// <summary>
		/// Create and send a command indicating that this user has completed the lesson.
		/// </summary>
		private void SendCompletedCommand()
		{
			var packet = new Command
			{
				IsCommand = true,
				IsLessonText = false,
				IsUserStatictics = false,
				IsGameComplete = true
			};
			_socket.Write(packet);
		}

		/// <summary>
		/// Create and send a command from server to client ordering the client to reinitiallize the game
		/// and begin a new match with the same lesson as this match.
		/// </summary>
		internal void SendRepeatLessonCommand()
		{
			var packet = new Command
			{
				IsCommand = true,
				IsResetCommand = true,
				ResetIsRepeatedLesson = true
			};
			_socket.Write(packet);
		}

		/// <summary>
		/// Create and send a command from server to client ordering the client to reinitiallize the game
		/// and begin a new match with the contained new lesson.
		/// </summary>
		internal void SendNewLessonCommand(string lessonString)
		{
			var packet = new Command
			{
				IsCommand = true,
				IsResetCommand = true,
				ResetIsNewLesson = true,
                LessonText = lessonString
			};
			_socket.Write(packet);
		}

        internal void SendLessonText(string lessonString)
        {
            var lessonText = new LessonText
                 {
                     IsCommand = false,
                     IsLessonText = true,
                     IsUserStatictics = false,
                     Lesson = _lessonString
                 };
            _socket.Write(lessonText);
        }

		#endregion

	}
}
