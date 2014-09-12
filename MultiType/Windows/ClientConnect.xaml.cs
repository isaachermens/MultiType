using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MultiType.ViewModels;

namespace MultiType.Windows
{
	/// <summary>
	/// Interaction logic for ClientConnect.xaml
	/// </summary>
	public partial class ClientConnect : Window
	{
		ConnectVm _vm;
		public ClientConnect(Window owner)
		{
			// set up the window and its data context
		    Owner = owner;
			InitializeComponent();
			_vm = new ConnectVm();
			DataContext = _vm;
		}

        // todo KILL IT WITH FIRE
		/// <summary>
		/// A connection to the host has been established.
		/// Open the main window, passing in an empty lesson string (we don't know what the lesson is yet) and the socket connected to the host.
		/// </summary>
		private void ConnectionEstablished_Checked(object sender, RoutedEventArgs e)
		{
			if (_vm == null && _vm.asyncSocket!=null) return;
			// connection has been established, open the primary window, passing in the peer socket
			// we don't know what the lesson is yet, so pass an empty string to the view model
			const string lessonString = "";
			var window = new TypingWindow(_vm.asyncSocket, lessonString);
			window.Show();
			Close();
		}

		/// <summary>
		/// Block improper input to the IP address text box
		/// </summary>
		private void IP_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			const string pattern = @"^\d$";
			if (!e.Text.Equals(".") && !Regex.IsMatch(e.Text, pattern))
				e.Handled = true;
			if (e.Text.Length >= 15)
				e.Handled = true;
		}

		/// <summary>
		/// Block improper input to the Port # input text box
		/// </summary>
		private void Port_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!Regex.IsMatch(e.Text, @"^\d$"))
				e.Handled = true;
			if (e.Text.Length >= 5)
				e.Handled = true;
		}
	}
}
