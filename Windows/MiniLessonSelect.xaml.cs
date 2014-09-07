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
using MultiType.ViewModels;

namespace MultiType
{
	/// <summary>
	/// Interaction logic for MiniLessonSelect.xaml
	/// Allows a single player or the host of a multiplayer game to choose a new lesson after completing their current lesson.
	/// </summary>
	public partial class MiniLessonSelect : Window
	{
		public string LessonString { get; set; }
		private LessonViewModel _viewModel;

		public MiniLessonSelect()
		{
			InitializeComponent();
			_viewModel = new LessonViewModel();
			this.DataContext = _viewModel;
		}

		private void Choose_Click(object sender, RoutedEventArgs e)
		{
			// todo must ensure that the user has actually selected a lesson
			if (_viewModel == null) return;
			if (_viewModel.SelectedLessonIndex.Equals("0")) return;
			LessonString = _viewModel.LessonString;
			DialogResult = true;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
