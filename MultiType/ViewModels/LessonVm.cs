using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using MultiType.Commands;
using MultiType.Models;
using MultiType.Services;
using MultiType.SocketsAPI;
using MultiType.Windows;
using PropertyChanged;

namespace MultiType.ViewModels
{
    [ImplementPropertyChanged]
	class LessonVm: BaseVm
	{
		#region Private Fields

		private LessonManagementService _managementService;
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
		public bool AllowEdit { get { return SelectedLessonIndex > 0; }
		}
        [DependsOn("SelectedLessonIndex")]
        public bool AllowChoose { get { return SelectedLessonIndex > 0; } }

	    public string EditLessonName { get; set; }

	    public string EditErrorMessage { get; set; }

        public string EditLessonContent { get; set; }
        
		public List<string> LessonNames { get; set; }

        [DependsOn("SelectedLessonIndex")]
        public string LessonName { get; set; }
        public string LessonContent { get; set; }
        public string CreateErrorMessage { get; set; }

        public string NewLessonName { get; set; }

        public string NewLessonContent { get; set; }

        public bool IsEditing { get; set; }

        [DependsOn("IsEditing,IsCreating")]
        public bool IsShowingNormal { get { return !(IsEditing || IsCreating); }}

        public bool IsCreating { get; set; }

	    public int SelectedLessonIndex
	    {
	        get { return _selectedLessonIndex; }
		    set
		    {
		        _selectedLessonIndex = value;
		        if(_managementService==null) return;
                if (_selectedLessonIndex == 0) // the default option has been reselected, clear the lesson string
		            LessonContent = "";
		        else
		        {
                    var lessonName = LessonNames[_selectedLessonIndex];
		            LessonContent = _managementService.GetLessonText(lessonName);
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
            EditLessonContent = LessonContent;
            EditLessonName = LessonNames[SelectedLessonIndex];
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
			_managementService = new LessonManagementService();
			LessonContent = "";
		    RacerSpeeds = new[] {10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150};
			RacerIndex = 5;
		    LessonNames = _managementService.GetLessonNames();
		}
        private void ChooseLesson(Window host)
        {
            if (IsSinglePlayer)
            {
                ShowWindowAsDialog(host, new TypingWindow(LessonContent, RacerSpeed));
            }
            else
            {
                ShowWindowAsDialog(host, new HostWindow(host, LessonContent));
            }
        }

        private void ShowNormal()
        {
            IsEditing = false;
            IsCreating = false;
            EditLessonContent = "";
            EditLessonName = "";
            NewLessonName = "";
            NewLessonContent = "";
        }

		internal void CreateNewLesson()
		{
			try
			{
				_managementService.CreateNewLesson(NewLessonName, NewLessonContent);
			    LessonNames = _managementService.GetLessonNames();
                var index = LessonNames.IndexOf(LessonName, 0);
			    if (index >= 0)
			    {
			        SelectedLessonIndex = index;
			    }
			    ShowNormal();
			}
			catch (Exceptions.BadLessonEntryException e)
			{
				CreateErrorMessage = e.Message;
			}
		}

	    internal void EditLesson()
		{
			try{
				_managementService.EditLesson(LessonNames[SelectedLessonIndex], EditLessonName, EditLessonContent);
			    LessonNames = _managementService.GetLessonNames();
				var index = LessonNames.IndexOf(EditLessonName, 0);
				if (index > -1)
					SelectedLessonIndex = index;
                ShowNormal();
			}
			catch (Exceptions.BadLessonEntryException e)
			{
				EditErrorMessage = e.Message;
			}
		}

		internal void DeleteCurrentLesson()
		{
		    if (MessageBox.Show("Are you sure you wish to delete this lesson?", "Confirm Deletion", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _managementService.DeleteCurrentLesson(LessonNames[SelectedLessonIndex]);
                LessonNames = _managementService.GetLessonNames();
                SelectedLessonIndex = 0;
		    }
		}
	}
}
