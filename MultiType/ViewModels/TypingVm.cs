using System;
using System.ComponentModel;
using System.Windows.Controls;
using MultiType.Models;

namespace MultiType.ViewModels
{
    public class TypingVm : INotifyPropertyChanged
	{
		#region Peer Fields

		private string _peerTypedContent;
		private int _peerWPM;
		private int _peerErrors;
		private int _peerCharactersTyped;
		private string _peerCompletionPercent;
		private string _peerAccuracy;

		#endregion

		#region Private Field

		private TypingModel _model;
		internal RichTextBox _userInput;
		private string _popupText;
		private bool _showPopup;
		private string _popupCountdown;
		internal bool gameHasStarted;
		private bool _isMulti;
		private bool _rtbReadOnly;
		private bool _gameComplete;
		private bool _isServer;
		private bool _clearRtb;

		#endregion

		#region Local Properties

		public bool ClearRTB 
		{ 
			get{return _clearRtb;}
			set
			{
				_clearRtb = value;
				NotifyPropertyChanged("ClearRTB");
			}
		}

		public bool GameComplete
		{
			get { return _gameComplete; }
			set
			{
				_gameComplete = value;
				NotifyPropertyChanged("GameComplete");
			}
		}

		public bool RTBReadOnly
		{
			get { return _rtbReadOnly; }
			set
			{
				_rtbReadOnly = value;
				NotifyPropertyChanged("RTBReadOnly");
			}
		}

		public string StaticPopupText
		{
			get { return _popupText; }
			set
			{
				_popupText = value;
				NotifyPropertyChanged("StaticPopupText");
			}
		}

		public bool ShowPopup
		{
			get { return _showPopup; }
			set
			{
				_showPopup = value;
				NotifyPropertyChanged("ShowPopup");
			}
		}

		public string PopupCountdown
		{
			get { return _popupCountdown; }
			set
			{
				_popupCountdown = value;
				NotifyPropertyChanged("PopupCountdown");
			}
		}

        public string CompletionPercentage
        {
			get { return _model._completionPercent; }
            set
            {
				_model._completionPercent = value;
                NotifyPropertyChanged("CompletionPercentage");
            }
        }
        
		public string CharactersTyped
        {
			get { return "Letters Typed: " + _model._charactersTyped; }
            set
            {
				_model._charactersTyped = Int32.Parse(value);
                NotifyPropertyChanged("CharactersTyped");
            }
        }
        public string Accuracy
        {
            get { return "Accuracy: " + _model._accuracy; }
            set
            {
                _model._accuracy = value;
                NotifyPropertyChanged("Accuracy");
            }
        }
        public string Errors
        {
			get { return _model._errors + " Errors"; }
            set
            {
				_model._errors = Int32.Parse(value);
                NotifyPropertyChanged("Errors");
            }
        }
        public string LessonString
        {
			get { return _model._lessonString; }
            set
            {
				_model._lessonString = value;
                _model._adjustedLessonString = value.TrimEnd();
                _model._lessonLength = _model._adjustedLessonString.Length;
                NotifyPropertyChanged("LessonContent");
            }
        }
        public string TimeElapsed
        {
            get 
            {
				return _model._timeElapsed;
            }
			set
			{
				_model._timeElapsed = value;
				NotifyPropertyChanged("TimeElapsed");
			}
        }
        public string WPM
        {
			get { return _model._WPM + " WPM"; }
            set
            {
				_model._WPM = Int32.Parse(value);
                NotifyPropertyChanged("WPM");
            }
        }

        #endregion 

		#region Peer Properties

		public string PeerCompletionPercentage
		{
			get { return  _peerCompletionPercent; }
			set
			{
				 _peerCompletionPercent = value;
				 NotifyPropertyChanged("PeerCompletionPercentage");
			}
		}
		public string PeerTypedContent
		{
			get { return  _peerTypedContent; }
			set
			{
				 _peerTypedContent = value;
				 NotifyPropertyChanged("PeerTypedContent");
			}
		}
		public string PeerCharactersTyped
		{
			get { return "Letters Typed: " +  _peerCharactersTyped; }
			set
			{
				 _peerCharactersTyped = Int32.Parse(value);
				 NotifyPropertyChanged("PeerCharactersTyped");
			}
		}
		public string PeerAccuracy
		{
			get { return "Accuracy: " +  _peerAccuracy; }
			set
			{
				 _peerAccuracy = value;
				 NotifyPropertyChanged("PeerAccuracy");
			}
		}
		public string PeerErrors
		{
			get { return  _peerErrors + " Errors"; }
			set
			{
				 _peerErrors = Int32.Parse(value);
				 NotifyPropertyChanged("PeerErrors");
			}
		}
		
		public string PeerWPM
		{
			get { return  _peerWPM + " WPM"; }
			set
			{
				 _peerWPM = Int32.Parse(value);
				 NotifyPropertyChanged("PeerWPM");
			}
		}

		#endregion

        public TypingVm(string lessonString, RichTextBox userInput, SocketsAPI.AsyncTcpClient socket=null, bool isServer=false, int racerSpeed=0)
        {
			if (socket == null)
			{
				_model = new TypingModel(this, isServer, racerSpeed);
			}
			else
			{
				_model = new TypingModel(this, isServer, racerSpeed, socket);
			}
			_isServer = isServer;
			_isMulti = socket != null;
			_userInput = userInput;
			InitiallizeViewModel(lessonString);
		}

		/// <summary>
		/// Begin a new game with the specified content
		/// </summary>
		/// <param name="lessonString">The new lesson content</param>
		/// <param name="isLocalCall">Is this being called by the view/view model?</param>
        internal void NewLesson(string lessonString, bool isLocalCall = true)
		{
			if (_isMulti)
			{
                if (isLocalCall)
                {
                    _model.SendNewLessonCommand(lessonString);
                }
			}
			InitiallizeViewModel(lessonString);
		}

		/// <summary>
		/// Restart the game with the same lesson content
		/// </summary>
		/// <param name="isLocalCall">Is this being called by the view/view model?</param>
		internal void RepeatLesson(bool isLocalCall=true)
		{
			if (_isMulti)
			{
				if(isLocalCall)
					_model.SendRepeatLessonCommand();
			}
			InitiallizeViewModel(LessonString, isReinitialization:true);
		}

		private void InitiallizeViewModel(string lessonString, bool isReinitialization=false)
		{
			// Initiallize data bound properties.
			GameComplete = false;
			gameHasStarted = false;
			LessonString = lessonString;
			CharactersTyped = "0";
			Accuracy = "0/0 = 0%";
			TimeElapsed = "";
			CompletionPercentage = "0/" + LessonString.Length + " = 0% Complete";
			Errors = "0";
			WPM = "0";
			RTBReadOnly = _isMulti;
			// initiallize/reinitiallize the model
			_model.InitiallizeModel(isReinitialization);
			ClearRTB = true;
			// initiallize peer stats display properties
			InitiallizePeerFields();
			if(_isMulti)
				_model.SendStatsPacket();
		}

		private void InitiallizePeerFields()
		{
			PeerAccuracy = "";
			PeerCharactersTyped = "0";
			PeerCompletionPercentage = "";
			PeerErrors = "0";
			PeerTypedContent = "";
			PeerWPM = "0";
		}

        internal void CharacterTyped(string content)
        {
			if (LessonString.Length == 0) return;
			//If the program is currently frozen (game has just started or has been paused by the user), start the timer
			if (_isMulti==false && !_model._stopwatch.IsRunning && content.Length>0)
			{
				gameHasStarted = true;
				_model.TogglePause(isLocalCall: true);
			}
			_model.UpdateStatistics(content);
        }

		internal void GameIsComplete()
		{
			RTBReadOnly = true;
			GameComplete = true;
		}

		internal void StartGame()
		{
			if (gameHasStarted == true) return;
			var thread = new System.Threading.Thread( () => _model.StartGame(true));
			thread.Start();
		}

        internal void TogglePause()
        {
			// kick off a new thread to pause the game. A new thread must be used in order for the popup text to update properly
			var thread = new System.Threading.Thread( ()=>_model.TogglePause(isLocalCall:true));
			thread.Start();
        }

        #region NPC Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        #endregion NPC Implementation    
	}
}
