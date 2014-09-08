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
	/// Interaction logic for LessonComplete.xaml
	/// </summary>
	public partial class LessonComplete : Window
	{
		internal SocketsAPI.DialogResult Result { get; set; }

		public LessonComplete()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Determine which of the three buttons has been clicked and return the appropriate dialog result to the main window.
		/// </summary>
		private void Btn_Click(object sender, RoutedEventArgs e)
		{
			var btn = (Button)sender;
			if (btn.Name == "Repeat")
			{
				Result = MultiType.SocketsAPI.DialogResult.Repeat;
				DialogResult = true;
			}
			else if (btn.Name == "SelectNew")
			{
				Result = MultiType.SocketsAPI.DialogResult.New;
				DialogResult = true;
			}
			else
				DialogResult = false;
		}
	}
}

