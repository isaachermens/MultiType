using System;
using System.ComponentModel;
using System.Windows.Controls;
using MultiType.Models;
using MultiType.SocketsAPI;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
    public class TypingVm : INotifyPropertyChanged
	{
        public PlayerStatsVm LocalStats { get; set; }
        public PlayerStatsVm RemoteStats { get; set; }
		#region Private Field

		private TypingModel _model;
		internal RichTextBox _userInput;
		private string _popupText;
		private bool _showPopup;
		private string _popupCountdown;
		private bool _isGameRunning;
		private bool _isMulti;
		private bool _rtbReadOnly;
		private bool _gameComplete;
		private bool _isServer;
		private bool _clearRtb;

		#endregion

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

        public string PeerTypedContent { get; set; }

        public TypingVm(string lessonString, RichTextBox userInput, PlayerStatsVm local, PlayerStatsVm remote, IAsyncTcpClient socket, bool isServer, int racerSpeed)
        {
            // store references to the stat VMs
            LocalStats = local;
            RemoteStats = remote;
            if (socket != null && remote != null)
            {
                socket.ContentReceived += (obj, args) => PeerTypedContent = args.Content;
                socket.StatsReceived += (obj, args) => remote.UpdateStats(args.StatsUpdate);
            }
			_model = new TypingModel(this, isServer, racerSpeed, socket);
			_isServer = isServer;
			_isMulti = socket != null;
			_userInput = userInput;
			InitiallizeViewModel(lessonString);
            RegisterCommandCallbacks(socket);
        }

        private void RegisterCommandCallbacks(IGameController controller)
        {
            controller.NewLesson = NewLesson;
            controller.RepeatLesson = RepeatLesson;
            controller.SetPauseState = (gameRunning) => _isGameRunning = gameRunning;
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
			_isGameRunning = false;
            LessonString = lessonString;
            TimeElapsed = "";
            PeerTypedContent = "";
			RTBReadOnly = _isMulti;
			// initiallize/reinitiallize the model
			_model.InitiallizeModel(isReinitialization);
			ClearRTB = true;
			// initiallize peer stats display properties
			if(_isMulti)
				_model.SendStatsPacket();
		}

        internal void CharacterTyped(string content)
        {
			if (LessonString.Length == 0) return;
			//If the program is currently frozen (game has just started or has been paused by the user), start the timer
			if (_isMulti==false && !_model._stopwatch.IsRunning && content.Length>0)
			{
				_isGameRunning = true;
				_model.TogglePause(true);
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
			if (_isGameRunning) return;
			var thread = new System.Threading.Thread( () => _model.StartGame(true, (gameRunning)=>_isGameRunning=gameRunning));
			thread.Start();
		}

        internal void TogglePause()
        {
			// kick off a new thread to pause the game. A new thread must be used in order for the popup text to update properly
			var thread = new System.Threading.Thread( ()=>_model.TogglePause(true));
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
