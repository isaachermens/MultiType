using System;
using System.Windows;
using MultiType.ViewModels;

namespace MultiType.Windows
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
	}
}
