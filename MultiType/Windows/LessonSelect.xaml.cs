using System;
using System.Windows;
using MultiType.ViewModels;

namespace MultiType
{
	/// <summary>
	/// Interaction logic for LessonSelect.xaml
	/// </summary>
	public partial class LessonSelect : Window
	{
		private readonly LessonVm _viewModel;
		private readonly bool _isSinglePlayer;
		public LessonSelect(Window owner = null, bool isSinglePlayer = false)
		{
		    Owner = owner;
			_isSinglePlayer = isSinglePlayer;
			InitializeComponent();
			this.DataContext = new LessonVm(this);
			_viewModel = (LessonVm)this.DataContext;
			if(_isSinglePlayer==false)
			{
				RacerSpeed.Visibility = Visibility.Collapsed;
				RacerSpeeds.Visibility = Visibility.Collapsed;
			}
		}

		private void Choose_Click(object sender, RoutedEventArgs e)
		{
			// todo must ensure that the user has actually selected a lesson
			if (_viewModel == null) return;
			if (!_isSinglePlayer)
			{
				try
				{ //open a listen port and show the pending connection popup
					_viewModel.OpenConnectionPendingPopup();
					// when this method call returns, a connection has been received from another user
				}
				catch (Exception exc)
				{
					_viewModel.LessonString = exc.Message;
				}	
			}
			else
			{
				if (_viewModel.SelectedLessonIndex.Equals("0"))
				{
					_viewModel.LessonString = "An error has occured. You must select a lesson before beginning the game.";
					return;
				}
					
				var window = new MainWindow(_viewModel.LessonString, _viewModel.RacerSpeed);
				window.Show();
				this.Close();
			}
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
		    DialogResult = true;
			Close();
		}

		private void ConnectionEstablished_Checked(object sender, RoutedEventArgs e)
		{
			if (_viewModel == null && _viewModel.asyncClient!=null) return;
			// connection has been established, open the primary window, passing in the peer socket
			this.Visibility = Visibility.Hidden;
			var window = new MainWindow(_viewModel.asyncClient, _viewModel.LessonString, true);
			window.Show();
			this.Close();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			var lessonText = LessonEntry.Text;
			if (_viewModel.CreateNewLesson(lessonText))
			{
				HideEntryGrid();
				LessonEntry.Text = "";
				LessonName.Text = "";		
			}
		}

		private void CancelAdd_Click(object sender, RoutedEventArgs e)
		{
			HideEntryGrid();
		}
        // Todo convery visibilty sets to use data binding and a value converter
		private void CreateNew_Click(object sender, RoutedEventArgs e)
		{
			LessonEntryGrid.Visibility = Visibility.Visible;
			LessonContent.Visibility = Visibility.Collapsed;
		}

		private void HideEntryGrid()
		{
			LessonEntryGrid.Visibility = Visibility.Collapsed;
            LessonContent.Visibility = Visibility.Visible;	
		}

		private void HideEditGrid()
		{
            LessonContent.Visibility = Visibility.Visible;
			LessonEditGrid.Visibility = Visibility.Collapsed;
		}

		private void EditBtn_Click(object sender, RoutedEventArgs e)
		{
			LessonEdit.Text = _viewModel.LessonString;
			LessonNameEdit.Text = _viewModel.LessonNames[_viewModel.SelectedLessonIndex];
            LessonContent.Visibility = Visibility.Collapsed;
			LessonEditGrid.Visibility = Visibility.Visible;
		}

		private void CancelEdit_Click(object sender, RoutedEventArgs e)
		{
			HideEditGrid();
		}

		private void SaveEdit_Click(object sender, RoutedEventArgs e)
		{
			var newLessonName = LessonNameEdit.Text;
			var newLessonText = LessonEdit.Text;
			if (_viewModel.EditLesson(newLessonName, newLessonText))
			{
				HideEditGrid();
			}
		}

		private void DeleteBtn_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.DeleteCurrentLesson();
		}		
	}
}
