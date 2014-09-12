using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using MultiType.Commands;
using MultiType.Models;
using MultiType.SocketsAPI;
using MultiType.Windows;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
	class LessonVm: BaseVm
	{
		#region Private Fields

		private LessonModel _model;
		internal AsyncTcpClient asyncClient;
        private int _selectedLessonIndex;

		#endregion

		#region DataBound Properties

        public bool IsSinglePlayer { get; set; }

		public int RacerSpeed
		{
			get { return RacerSpeeds[RacerIndex]; }
		}
		public int[] RacerSpeeds { get; set; }

		public int RacerIndex { get; set; }

        [DependsOn("SelectedLessonIndex")]
		public bool AllowEdit
		{
			get { return SelectedLessonIndex > 0; }
		}

	    public string LessonNameEdit { get; set; }

	    public string EditErrorText { get; set; }

        public string LessonTextEdit { get; set; }
        
		public bool ConnectionEstablished { get; set; }

		public bool ShowPopup { get; set; }
		
        public string IpAddress { get; set; }

		public string PortNum { get; set; }

		public List<string> LessonNames { get; set; }

        [DependsOn("SelectedLessonIndex")]
        public string LessonName { get; set; }
        public string CreateErrorText { get; set; }

		public string LessonString { get; set; }

        public string NewLessonName { get; set; }

        public string NewLessonText { get; set; }

        [DependsOn("SelectedLessonIndex")]
        public bool AllowChoose { get { return SelectedLessonIndex > 0; } }

        public bool IsEditing { get; set; }

        [DependsOn("IsEditing,IsCreating")]
        public bool IsShowingNormal { get { return !(IsEditing || IsCreating); }}

        public bool IsCreating { get; set; }

        // TODO definitely fix
	    public int SelectedLessonIndex
	    {
	        get { return _selectedLessonIndex; }
		    set
		    {
		        _selectedLessonIndex = value;
		        if(_model==null) return;
                if (_selectedLessonIndex == 0) // the default option has been reselected, clear the lesson string
		            LessonString = "";
		        else
		        {
                    var lessonName = LessonNames[_selectedLessonIndex];
		            LessonString = _model.GetLessonText(lessonName);
		        }
		    }
		}

		#endregion

        #region Commands

        public RelayCommand<Window> Choose { get { return new RelayCommand<Window>(ChooseLesson); } }
        
        public LambdaCommand BeginEdit { get { return new LambdaCommand(() =>
        {
            IsEditing = true;
            IsCreating = false;
            LessonTextEdit = LessonString;
            LessonNameEdit = LessonNames[SelectedLessonIndex];
        });} }

        public LambdaCommand SaveEdit { get { return new LambdaCommand(EditLesson);} }

        public LambdaCommand CompleteEdit { get { return new LambdaCommand(ShowNormal); } }
        public LambdaCommand BeginCreate { get
            {
                return new LambdaCommand(() =>
                {
                    IsEditing = false;
                    IsCreating = true;
                });
            }
        }
        public LambdaCommand SaveNew { get { return new LambdaCommand(CreateNewLesson); } }

        public LambdaCommand CompleteCreate { get { return new LambdaCommand(ShowNormal); } }

        public LambdaCommand DeleteCurrent { get { return new LambdaCommand(DeleteCurrentLesson);} }
        #endregion // Commands

        internal LessonVm()
		{
			_model = new LessonModel(this); // todo remove
			LessonString = "";
		    IpAddress = "";
			PortNum = "";
		    RacerSpeeds = new[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150};
			RacerIndex = 5;
		    LessonNames = _model.GetLessonNames();
		}
        private void ChooseLesson(Window host)
        {
            if (IsSinglePlayer)
            {
                ShowWindowAsDialog(host, new TypingWindow(LessonString, RacerSpeed));
            }
            else
            {
                try
                { //open a listen port and show the pending connection popup
                    OpenConnectionPendingPopup();
                    // when this method call returns, a connection has been received from another user
                }
                catch (Exception exc)
                {
                    // Todo make this not awful...probably with a dialog box
                    LessonString = exc.Message;
                }
            }
        }

        private void ShowNormal()
        {
            IsEditing = false;
            IsCreating = false;
            LessonTextEdit = "";
            LessonNameEdit = "";
            NewLessonName = "";
            NewLessonText = "";
        }

		internal void OpenConnectionPendingPopup()
		{
			if (SelectedLessonIndex == 0 || LessonString == null)
				throw new Exception("User must select a lesson before attempting to host the game.");
			ShowPopup = true;			
			_model.OpenListenSocket();
		}

		internal void CreateNewLesson()
		{
			try
			{
				_model.CreateNewLesson(NewLessonName, NewLessonText);
			    LessonNames = _model.GetLessonNames();
                var index = LessonNames.IndexOf(LessonName, 0);
			    if (index >= 0)
			    {
			        SelectedLessonIndex = index;
			    }
			    ShowNormal();
			}
			catch (Exceptions.BadLessonEntryException e)
			{
				CreateErrorText = e.Message;
			}
		}

	    internal void EditLesson()
		{
			try{
				_model.EditLesson(LessonNames[SelectedLessonIndex], LessonNameEdit, LessonTextEdit);
			    LessonNames = _model.GetLessonNames();
				var index = LessonNames.IndexOf(LessonNameEdit, 0);
				if (index > -1)
					SelectedLessonIndex = index;
                ShowNormal();
			}
			catch (Exceptions.BadLessonEntryException e)
			{
				EditErrorText = e.Message;
			}
		}

		internal void DeleteCurrentLesson()
		{
		    if (MessageBox.Show("Are you sure you wish to delete this lesson?", "Confirm Deletion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _model.DeleteCurrentLesson(LessonNames[SelectedLessonIndex]);
                LessonNames = _model.GetLessonNames();
                SelectedLessonIndex = 0;
		    }
		}
	}
}
