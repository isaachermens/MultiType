using System.Windows;

namespace MultiType.Windows
{

	/// <summary>
	/// Interaction logic for LessonComplete.xaml
	/// </summary>
	public partial class LessonComplete : Window
	{
        internal Miscellaneous.DialogResult Result { get; set; }

		public LessonComplete()
		{
			InitializeComponent();
		}

	    private void SelectNew_Click(object sender, RoutedEventArgs e)
	    {
            Result = Miscellaneous.DialogResult.New;
            DialogResult = true;
	    }

	    private void Repeat_Click(object sender, RoutedEventArgs e)
	    {
            Result = Miscellaneous.DialogResult.Repeat;
            DialogResult = true;
	    }

	    private void Quit_Click(object sender, RoutedEventArgs e)
	    {
            DialogResult = false;
	    }
	}
}

