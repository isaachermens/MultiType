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
		public LessonSelect(Window owner = null, bool isSinglePlayer = false)
		{
		    Owner = owner;
			InitializeComponent();
			DataContext = new LessonVm{IsSinglePlayer = isSinglePlayer};
			_viewModel = (LessonVm)DataContext;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
		    DialogResult = true;
			Close();
		}

        // todo burn this with FIRE
		private void ConnectionEstablished_Checked(object sender, RoutedEventArgs e)
		{
			if (_viewModel == null && _viewModel.asyncClient!=null) return;
			// connection has been established, open the primary window, passing in the peer socket
			this.Visibility = Visibility.Hidden;
			var window = new MainWindow(_viewModel.asyncClient, _viewModel.LessonString, true);
			window.Show();
			this.Close();
		}
	}
}
