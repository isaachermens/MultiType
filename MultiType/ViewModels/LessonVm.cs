using System;
using System.ComponentModel;
using System.Windows;
using MultiType.Models;
using MultiType.SocketsAPI;

namespace MultiType.ViewModels
{
	class LessonVm: BaseVm
	{
		#region Private Fields

		private LessonModel _model;
		internal AsyncTcpClient asyncClient;

		#endregion

		#region DataBound Properties

		public int RacerSpeed
		{ // todo not sure
			get { return RacerSpeeds[RacerIndex]; }
		}
		public int[] RacerSpeeds { get; set; }

		public int RacerIndex { get; set; }

		public bool AllowEdit
		{
			get { return SelectedLessonIndex != 0; } // todo hmm?
		}

	    public string LessonNameEdit { get; set; }

	    public string EditErrorText { get; set; }

		public string ErrorText { get; set; }

		public bool ConnectionEstablished { get; set; }

		public bool ShowPopup { get; set; }
		
        public string IPAddress { get; set; }

		public string PortNum { get; set; }

		public string[] LessonNames { get; set; }
        public string LessonName { get; set; }

		public string LessonString { get; set; }

        // TODO definitely fix
	    public int SelectedLessonIndex { get; set;  //get { return _selectedIndex.ToString(); }
		    //set 
		    //{ 
		    //    _selectedIndex = Int32.Parse(value);
		    //    NotifyPropertyChanged("SelectedLessonIndex");
		    //    NotifyPropertyChanged("AllowEdit");
		    //    if(_model==null) return;
		    //    if (_selectedIndex == 0) // the default option has been reselected, clear the lesson string
		    //        LessonString = "";
		    //    else
		    //    {
		    //        var lessonName = _lessonNames[_selectedIndex];
		    //        LessonString = _model.GetLessonText(lessonName);
		    //    }
		    //}
		}

		#endregion

		internal LessonVm()
		{
			_model = new LessonModel(this); // todo remove
			LessonString = "";
		    IPAddress = "";
			PortNum = "";
		    RacerSpeeds = new[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150};
			RacerIndex = 5;
		}

		internal void OpenConnectionPendingPopup()
		{
			if (SelectedLessonIndex == 0 || LessonString == null)
				throw new Exception("User must select a lesson before attempting to host the game.");
			ShowPopup = true;			
			_model.OpenListenSocket();
		}

		internal bool CreateNewLesson(string lessonText)
		{
			try
			{
				_model.CreateNewLesson(LessonName, lessonText);
                var index = Array.IndexOf(LessonNames, LessonName, 0);
				if(index!=-1)
					SelectedLessonIndex = index;
				return true;
			}
			catch (Exceptions.BadLessonEntryException e)
			{
				ErrorText = e.Message;
				return false;
			}
		}

	    internal bool EditLesson(string newLessonName, string newLessonText)
		{
			try{
				// todo fix_model.EditLesson(LessonNames[_selectedIndex], newLessonName, newLessonText);
				var index = Array.IndexOf(LessonNames, newLessonName, 0);
				if (index != -1)
					SelectedLessonIndex = index;
				return true;
			}
			catch (Exceptions.BadLessonEntryException e)
			{
				EditErrorText = e.Message;
				return false;
			}
		}

		internal void DeleteCurrentLesson()
		{
            //if (_selectedIndex == 0)
            //    return;
            //_model.DeleteCurrentLesson(_lessonNames[_selectedIndex]);
            //SelectedLessonIndex = "0";
            // todo fix
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
