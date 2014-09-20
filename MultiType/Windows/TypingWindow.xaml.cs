using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MultiType.SocketsAPI;
using MultiType.ViewModels;

namespace MultiType.Windows
{
    /// <summary>
    /// Interaction logic for TypingWindow.xaml
    /// </summary>
    public partial class TypingWindow : Window
    {
        private TypingVm _viewModel;

		private int _contentLength;
		private readonly bool _isSinglePlayer;
		private readonly bool _isServer;

        private TypingWindow(string lessonString, int racerSpeed, IAsyncTcpClient socket, bool isServer,
            PlayerStatsVm remote)
        {
            InitializeComponent();
            var local = new PlayerStatsVm();
            LocalStatsGrid.DataContext = local;
            RemoteStatsGrid.DataContext = remote;
            _viewModel = new TypingVm(lessonString, UserInput, local, remote, socket, isServer, racerSpeed);
            DataContext = _viewModel;
            UserInput.Focus();
            _isServer = isServer;
        }

		public TypingWindow(string lessonString, int racerSpeed) 
            : this(lessonString, racerSpeed, null, false, null)
        {
            //adjust the placement of the local stats grid to account for a single player
            LocalStatsGrid.Margin = new Thickness(LocalStatsGrid.Margin.Left, LocalStatsGrid.Margin.Top + 20,
                LocalStatsGrid.Margin.Right, LocalStatsGrid.Margin.Bottom);
            RemoteStatsGrid.Visibility = Visibility.Hidden;
            _isSinglePlayer = true;
		}

        public TypingWindow(string lessonString, IAsyncTcpClient socket, bool isServer = false)
            : this(lessonString, 0, socket, isServer, new PlayerStatsVm())
        {
			_isSinglePlayer = false;
			ChangeLesson.Visibility = Visibility.Collapsed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			OpenStartGameDialog();
		}

        private void Window_KeyDown(object sender, KeyEventArgs e)
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
			PeerContentScroll.ScrollToVerticalOffset(offset);
			LessonContentScroll.ScrollToVerticalOffset(offset);
		}

		private void LessonSelect_Click(object sender, RoutedEventArgs e)
		{
			if (_isSinglePlayer == false) return;
			var window = new LessonSelect(null, _isSinglePlayer);
			window.Show();
			Close();
		}

		private void Menu_Click(object sender, RoutedEventArgs e)
		{
			var window = new Menu();
			window.Show();
			Close();
		}

		private void PopupOpen_Unchecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			if (checkbox.IsChecked == false)
				UserInput.Focus();
		}
		
		private void GameComplete_Checked(object sender, RoutedEventArgs e)
		{
			if (_isSinglePlayer || _isServer)
			{
				var completeWindow = new LessonComplete();
				if (completeWindow.ShowDialog() == false)
				{
					completeWindow.Close();
                    Application.Current.Shutdown();
				}
				else switch (completeWindow.Result)
				{
				    case Miscellaneous.DialogResult.Repeat:
				        completeWindow.Close();
				        _viewModel.RepeatLesson();
				        OpenStartGameDialog();
				        return;
				    case Miscellaneous.DialogResult.New:
				    {
				        completeWindow.Close();
				        var window = new SimpleLessonSelect();
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
				const string message = "Click OK to start the game.";
				const string caption = "Prompt";
                if (MessageBox.Show(message, caption, MessageBoxButton.OK) == MessageBoxResult.OK)
				{
					_viewModel.StartGame();
				}
				else
				{
					var window = new Menu();
					window.Show();
					Close();
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
