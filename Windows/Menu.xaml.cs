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

namespace MultiType
{
	/// <summary>
	/// Interaction logic for Menu.xaml
	/// </summary>
	public partial class Menu : Window
	{
		public Menu()
		{
			InitializeComponent();
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		/// <summary>
		/// Open the lesson select window to allow the user to host a game
		/// </summary>
		private void Host_Click(object sender, RoutedEventArgs e)
		{
			var lessonSelect = new LessonSelect();
			lessonSelect.Show();
			this.Close();
		}

		/// <summary>
		/// Open the client connect window to allow the user to connect to a host
		/// </summary>
		private void Join_Click(object sender, RoutedEventArgs e)
		{
			var window = new ClientConnect();
			this.Close();
			window.Show();
		}

		/// <summary>
		/// Open the lesson select window to allow the user to begin a single player game
		/// </summary>
		private void SinglePlayer_Click(object sender, RoutedEventArgs e)
		{
			var window = new LessonSelect(isSinglePlayer:true);
			window.Show();
			this.Close();
		}
	}
}
