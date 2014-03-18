﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypingGame.Models;

namespace TypingGame.ViewModels
{
	class LessonViewModel: INotifyPropertyChanged
	{
		#region Private Fields

		private string[] _lessonNames;
		private string _lessonString;
		private int _selectedIndex;
		private string _ipAddress;
		private string _portNum;
		private bool _showPopup;
		private bool _connectionEstablished;
		private LessonModel _model;
		internal Resources.AsyncTcpClient asyncClient;
		private string _errorText;
		private string _lessonNameEdit;
		private string _editErrorText;
		private string[] _racerSpeeds;
		private int _racerIndex;

		#endregion

		#region DataBound Properties

		public int RacerSpeed
		{
			get { return Int32.Parse(_racerSpeeds[_racerIndex].Split(' ')[0]); }
		}
		public string[] RacerSpeeds
		{
			get { return _racerSpeeds; }
			set
			{
				_racerSpeeds = value;
				NotifyPropertyChanged("RacerSpeeds");
			}
		}

		public int RacerIndex
		{
			get { return _racerIndex; }
			set
			{
				_racerIndex = value;
				NotifyPropertyChanged("RacerIndex");
			}
		}

		public bool AllowEdit
		{
			get { return _selectedIndex != 0; }
		}

		public string LessonNameEdit
		{
			get { return _lessonNameEdit; }
			set{
				_lessonNameEdit = value;
				NotifyPropertyChanged("LessonNameEdit");
			}
		}

		public string EditErrorText
		{
			get { return _editErrorText; }
			set{
				_editErrorText = value;
				NotifyPropertyChanged("EditErrorText");
			}
		}

		public string ErrorText
		{
			get { return _errorText; }
			set
			{
				_errorText = value;
				NotifyPropertyChanged("ErrorText");
			}
		}

		public bool ConnectionEstablished
		{
			get { return _connectionEstablished; }
			set
			{
				_connectionEstablished = value;
				if (value)
					ShowPopup = false;
				NotifyPropertyChanged("ConnectionEstablished");
			}
		}

		public bool ShowPopup
		{
			get{return _showPopup;}
			set{
				_showPopup=value;
				NotifyPropertyChanged("ShowPopup");
			}
		}
		public string IPAddress
		{
			get { return _ipAddress; }
			set 
			{
				_ipAddress = "IP Address: " + value;
				NotifyPropertyChanged("IPAddress");
			}
		}

		public string PortNum
		{
			get { return _portNum; }
			set
			{
				_portNum = "Port Number: " + value;
				NotifyPropertyChanged("PortNum");
			}
		}

		public string[] LessonNames
		{
			get { return _lessonNames; }
			set
			{
				_lessonNames = value;
				NotifyPropertyChanged("LessonNames");
			}
		}
		public string LessonString
		{
			get { return _lessonString; }
			set
			{
				_lessonString = value;
				NotifyPropertyChanged("LessonString");
			}
		}
		public string SelectedLessonIndex
		{
			get { return _selectedIndex.ToString(); }
			set 
			{ 
				_selectedIndex = Int32.Parse(value);
				NotifyPropertyChanged("SelectedLessonIndex");
				NotifyPropertyChanged("AllowEdit");
				if(_model==null) return;
				if (_selectedIndex == 0) // the default option has been reselected, clear the lesson string
					LessonString = "";
				else
				{
					var lessonName = _lessonNames[_selectedIndex];
					LessonString = _model.GetLessonText(lessonName);
				}
			}
		}

		#endregion

		internal LessonViewModel()
		{
			_model = new LessonModel(this);
			LessonString = "";
			IPAddress = "";
			PortNum = "";
			RacerSpeeds = new string[] { "10 WPM", "20 WPM", "30 WPM", "40 WPM", "50 WPM", "60 WPM", "70 WPM", "80 WPM", 
				"90 WPM", "100 WPM", "110 WPM", "120 WPM", "130 WPM", "140 WPM", "150 WPM" };
			RacerIndex = 5;
		}

		internal void OpenConnectionPendingPopup()
		{
			if (SelectedLessonIndex == "0" || LessonString == null)
				throw new Exception("User must select a lesson before attempting to host the game.");
			ShowPopup = true;			
			_model.OpenListenSocket();
		}

		internal bool CreateNewLesson(string lessonName, string lessonText)
		{
			try
			{
				_model.CreateNewLesson(lessonName, lessonText);
				var index = Array.IndexOf(LessonNames, lessonName, 0);
				if(index!=-1)
					SelectedLessonIndex = index.ToString();
				return true;
			}
			catch (Resources.BadLessonEntryException e)
			{
				ErrorText = e.Message;
				return false;
			}
		}

		internal bool EditLesson(string newLessonName, string newLessonText)
		{
			try{
				_model.EditLesson(LessonNames[_selectedIndex], newLessonName, newLessonText);
				var index = Array.IndexOf(LessonNames, newLessonName, 0);
				if (index != -1)
					SelectedLessonIndex = index.ToString();
				return true;
			}
			catch (Resources.BadLessonEntryException e)
			{
				EditErrorText = e.Message;
				return false;
			}
		}

		internal void DeleteCurrentLesson()
		{
			if (_selectedIndex == 0)
				return;
			_model.DeleteCurrentLesson(_lessonNames[_selectedIndex]);
			SelectedLessonIndex = "0";
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