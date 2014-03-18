using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TypingGame.ViewModels;
using System.Net.Sockets;

namespace TypingGame
{
	/// <summary>
	/// Interaction logic for LessonSelect.xaml
	/// </summary>
	public partial class LessonSelect : Window
	{
		private LessonViewModel _viewModel;
		private bool _isSinglePlayer;
		public LessonSelect(bool isSinglePlayer = false)
		{
			_isSinglePlayer = isSinglePlayer;
			InitializeComponent();
			this.DataContext = new LessonViewModel();
			_viewModel = (LessonViewModel)this.DataContext;
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
			var menu = new Menu();
			menu.Show();
			this.Close();
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
			var lessonName = LessonName.Text;
			if (_viewModel.CreateNewLesson(lessonName, lessonText))
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

		private void CreateNew_Click(object sender, RoutedEventArgs e)
		{
			LessonEntryGrid.Visibility = Visibility.Visible;
			LessonContentGrid.Visibility = Visibility.Collapsed;
		}

		private void HideEntryGrid()
		{
			LessonEntryGrid.Visibility = Visibility.Collapsed;
			LessonContentGrid.Visibility = Visibility.Visible;	
		}

		private void HideEditGrid()
		{
			LessonContentGrid.Visibility = Visibility.Visible;
			LessonEditGrid.Visibility = Visibility.Collapsed;
		}

		private void EditBtn_Click(object sender, RoutedEventArgs e)
		{
			LessonEdit.Text = _viewModel.LessonString;
			LessonNameEdit.Text = _viewModel.LessonNames[Int32.Parse(_viewModel.SelectedLessonIndex)];
			LessonContentGrid.Visibility = Visibility.Collapsed;
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
