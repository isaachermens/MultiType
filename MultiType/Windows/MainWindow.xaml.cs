using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MultiType.ViewModels;

namespace MultiType.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PrimaryVm _viewModel;
		private int _contentLength;
		private bool _isSinglePlayer;
		private bool _isServer;

		internal MainWindow(string lessonString, int racerSpeed)
		{
			InitializeComponent();
			PeerStatsGrid.Visibility = Visibility.Hidden;
			//adjust the placement of the local stats grid to account for a single player
			LocalStatsGrid.Margin = new Thickness(LocalStatsGrid.Margin.Left, LocalStatsGrid.Margin.Top+20,
				LocalStatsGrid.Margin.Right, LocalStatsGrid.Margin.Bottom);
			_viewModel = new PrimaryVm(lessonString, UserInput, racerSpeed:racerSpeed);
			_isSinglePlayer = true;
			this.DataContext = _viewModel;
			//_contentLength = 0;
			UserInput.Focus();
		}

        internal MainWindow(SocketsAPI.AsyncTcpClient socket, string lessonString, bool isServer = false)
        {
			InitializeComponent();
			this.DataContext = new PrimaryVm(lessonString, UserInput, socket, isServer );
			_viewModel = (PrimaryVm)DataContext;
			_isSinglePlayer = false;
			ChangeLesson.Visibility = Visibility.Collapsed;
			_isServer=isServer;			
        }

		internal void Window_Loaded(object sender, RoutedEventArgs e)
		{
			OpenStartGameDialog();
		}

        private void Window_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Pause)
                _viewModel.TogglePause();
        }

		private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
        {
			var content = new TextRange(UserInput.Document.ContentStart, UserInput.Document.ContentEnd).Text;
			if (_contentLength == content.Length) return;
			_contentLength = content.Length;
			if (content.Length < 2) return;
			content = content.Substring(0, content.Length - 2);
            if (_viewModel == null)
                return;
			_viewModel.CharacterTyped(content);
        }
		
		private void RTBScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			var scroller = (ScrollViewer)sender;
			scroller.ScrollToBottom();
			var offset = scroller.VerticalOffset;
			var blah = OtherUser;
			PeerContentScroll.ScrollToVerticalOffset(offset);
			LessonContentScroll.ScrollToVerticalOffset(offset);
		}

		private void LessonSelect_Click(object sender, RoutedEventArgs e)
		{
			if (_isSinglePlayer == false) return;
			var window = new LessonSelect(null, _isSinglePlayer);
			window.Show();
			this.Close();
		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			var window = new Menu();
			window.Show();
			this.Close();
		}

		private void PopupOpen_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			if (checkbox.IsChecked == false)
				UserInput.Focus();
		}
		
		private void GameComplete_Checked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			if (_isSinglePlayer || _isServer)
			{
				var completeWindow = new LessonComplete();
				if (completeWindow.ShowDialog() == false)
				{
					completeWindow.Close();
                    App.Current.Shutdown();
				}
				else if (completeWindow.Result == Miscellaneous.DialogResult.Repeat)
				{
					completeWindow.Close();
					_viewModel.RepeatLesson();
					OpenStartGameDialog();
					return;
				}
                else if (completeWindow.Result == Miscellaneous.DialogResult.New)
				{
					completeWindow.Close();
					var window = new MiniLessonSelect();
					if (window.ShowDialog() == true)
					{
						var lessonString = window.LessonString;
						_viewModel.NewLesson(lessonString);
						OpenStartGameDialog();
					}
					else
					{
						// should we notify the peer in this case?
						var menu = new Menu();
						menu.Show();
					}
					window.Close();
					//this.Close();
					return;
				}
			}
			else
			{
				_viewModel.ShowPopup = true;
				_viewModel.StaticPopupText = "Waiting for the host to select a lesson and start a new game...";
                return;
			}
		}

		internal void OpenStartGameDialog()
		{
			if (_isSinglePlayer)
				return;
			else if (!_isServer)
			{
				_viewModel.ShowPopup = true;
				_viewModel.StaticPopupText = "Waiting for host to start...";
			}
			else // isServer
			{
				string message = "Click OK to start the game.";
				string caption = "Prompt";
				MessageBoxButton buttons = MessageBoxButton.OK;
				if (MessageBox.Show(message, caption, buttons) == MessageBoxResult.OK)
				{
					_viewModel.StartGame();
				}
				else
				{
					var window = new Menu();
					window.Show();
					this.Close();
				}
			}
		}

		private void ClearRTB_Checked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			UserInput.Document.Blocks.Clear();
			var paragraph = new Paragraph();
			paragraph.Foreground = Brushes.Blue;
			paragraph.LineHeight = 48;
			UserInput.Document.Blocks.Add(paragraph);
			checkbox.IsChecked = false;
		}
    }
}
