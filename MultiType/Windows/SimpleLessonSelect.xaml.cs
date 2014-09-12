using System.Windows;
using MultiType.ViewModels;

namespace MultiType.Windows
{
	/// <summary>
	/// Interaction logic for SimpleLessonSelect.xaml
	/// Allows a single player or the host of a multiplayer game to choose a new lesson after completing their current lesson.
	/// </summary>
	public partial class SimpleLessonSelect : Window
	{
		public string LessonString { get; set; }

		public SimpleLessonSelect()
		{
			InitializeComponent();
			DataContext = new LessonVm();
		}

	    private void Choose_OnClick(object sender, RoutedEventArgs e)
	    {
	        LessonString = LessonDisplay.Text;
	        DialogResult = true;
	    }
	}
}
